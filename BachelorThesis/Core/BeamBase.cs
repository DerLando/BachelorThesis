using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace BachelorThesis.Core
{
    public class Beam : ICloneable
    {
        public Curve Axis { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Vector3d Up { get; set; }
        public double EvaluationDistance { get; set; } = 0.1;

        private Vector3d _startTangent;
        private Vector3d _endTangent;
        private double _startParam;
        private double _endParam;
        private bool _startModified;
        private bool _endModified;
        private Brep _volumeGeometry;
        private Brep _geometry;

        public Brep Geometry => _geometry;

        public Beam() { }

        public Beam(Curve axis, double width, double height, Vector3d up)
        {
            Axis = axis;
            Width = width;
            Height = height;
            Up = up;

            InitializeGeometry();
        }

        private void InitializeGeometry()
        {
            _startTangent = Axis.TangentAtStart;
            _endTangent = -Axis.TangentAtEnd;
            _startParam = Axis.Domain.Min;
            _endParam = Axis.Domain.Max;

            int divisionCount = Convert.ToInt32(Math.Ceiling(Axis.GetLength() / EvaluationDistance));
            var planeParams = Axis.DivideByCount(divisionCount, true);

            var crossSections = new Curve[planeParams.Length];
            var planes = new Plane[planeParams.Length];
            var angles = new double[planeParams.Length];
            var widthInterval = new Interval(-Width / 2.0, Width / 2.0);
            var heightInterval = new Interval(-Height / 2.0, Height / 2.0);

            for (int i = 0; i < crossSections.Length; i++)
            {
                var param = planeParams[i];
                var tangent = Axis.TangentAt(param);
                bool success = Axis.PerpendicularFrameAt(param, out var plane);

                var xDirection = Vector3d.CrossProduct(tangent, Up);
                var angle = Vector3d.VectorAngle(plane.XAxis, xDirection, plane);
                if (Math.Abs(angle) > 100000) angle = 0.0;
                plane.Rotate(angle, plane.ZAxis);

                // DEBUG!
                planes[i] = plane;
                angles[i] = angle;

                // only works for linear beams
                //var plane = new Plane(Axis.PointAt(param), Vector3d.CrossProduct(tangent, Up), Up);

                crossSections[i] = new Rectangle3d(plane, widthInterval, heightInterval).ToPolyline().ToPolylineCurve();
            }

            var lofted = Brep.CreateFromLoft(crossSections, Point3d.Unset, Point3d.Unset, LoftType.Tight, false);
            if (lofted.Length == 1) _volumeGeometry = lofted[0];

            _geometry = _volumeGeometry.DuplicateBrep();
        }

        public void SetEndTangent(CurveEnd end, Vector3d tangent)
        {
            switch (end)
            {
                case CurveEnd.None:
                    break;
                case CurveEnd.Start:
                    _startTangent = tangent;
                    break;
                case CurveEnd.End:
                    _endTangent = tangent;
                    break;
                case CurveEnd.Both:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(end), end, null);
            }
        }

        public void SetEndCondition(CurveEnd end, Vector3d tangent, double param)
        {
            switch (end)
            {
                case CurveEnd.None:
                    break;
                case CurveEnd.Start:
                    _startTangent = tangent;
                    _startParam = param;
                    _startModified = true;
                    break;
                case CurveEnd.End:
                    _endTangent = tangent;
                    _endParam = param;
                    _endModified = true;
                    break;
                case CurveEnd.Both:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(end), end, null);
            }
        }

        public void ShortenEnd(CurveEnd end, double length, bool absolute)
        {
            Plane plane = Plane.Unset;
            Point3d origin = Point3d.Unset;
            Vector3d tangent = Vector3d.Unset;

            switch (end)
            {
                case CurveEnd.None:
                    break;
                case CurveEnd.Start:
                    tangent = _startTangent;
                    origin = absolute ? Axis.PointAtLength(length) : Axis.PointAt(length);
                    break;
                case CurveEnd.End:
                    tangent = _endTangent;
                    origin = absolute ? Axis.PointAtLength(Axis.GetLength() - length) : Axis.PointAt(length);
                    break;
                case CurveEnd.Both:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(end), end, null);
            }

            plane = new Plane(origin, Vector3d.CrossProduct(tangent, Up), Up);
            plane = new Plane(origin, -tangent);
            if (plane == Plane.Unset) return;

            var trimmed = _geometry.Trim(plane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if (trimmed.Length != 1)
            {
                //RhinoDoc.ActiveDoc.Objects.AddBrep(_geometry);
                //RhinoDoc.ActiveDoc.Objects.AddBrep(Brep.CreateTrimmedPlane(plane,
                //    new Rectangle3d(plane, 1, 1).ToNurbsCurve()));
                //_geometry = _volumeGeometry.DuplicateBrep();
            }
            else _geometry = trimmed[0];
        }

        private bool isInNeedOfShortening()
        {
            return _startModified | _endModified;
            //var noStartCondition = (_startTangent == Axis.TangentAtStart) && (_startParam == Axis.Domain.Min);
            //var noEndCondition = (_endTangent == -Axis.TangentAtEnd) && (_endParam == Axis.Domain.Max);
            //return !noStartCondition | !noEndCondition;
        }

        public void ShortenEnds()
        {
            if (!isInNeedOfShortening()) return;
            var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            var plane = new Plane(Axis.PointAt(_startParam), -_startTangent);
            var trimmed = _volumeGeometry.Trim(plane, tol * 2.0);
            if (trimmed.Length != 1)
            {
                // Try harder
                RhinoApp.WriteLine($"ShortenEnds WARNING: Planesplit at start failed with tolerance {tol}");
                var interval = new Interval(-0.5, 0.5);
                var trimmedPlane = Brep.CreateTrimmedPlane(plane,
                    new Rectangle3d(plane, interval, interval).ToNurbsCurve());
                var split = _volumeGeometry.Split(trimmedPlane, tol * 2.0);
                if (split.Length != 2)
                {
                    Intersection.BrepPlane(_volumeGeometry, plane, tol, out var intCrvs, out var intPts);
                    List<Guid> ids = new List<Guid>();
                    foreach (var intCrv in intCrvs)
                    {
                        ids.Add(RhinoDoc.ActiveDoc.Objects.AddCurve(intCrv));
                    }

                    foreach (var brep in split)
                    {
                        ids.Add(RhinoDoc.ActiveDoc.Objects.AddBrep(brep));
                    }
                    ids.Add(RhinoDoc.ActiveDoc.Objects.AddBrep(trimmedPlane));

                    RhinoDoc.ActiveDoc.Groups.Add("start", ids);
                }
                else
                {
                    var areas = (from part in split select part.GetArea()).ToArray();
                    Array.Sort(areas, split);
                    _geometry = split[1];
                }
            }

            else _geometry = trimmed[0];

            plane = new Plane(Axis.PointAt(_endParam), -_endTangent);
            trimmed = _geometry.Trim(plane, tol * 2.0);
            if (trimmed.Length != 1)
            {
                // Try harder
                RhinoApp.WriteLine($"ShortenEnds WARNING: Planesplit at end failed with tolerance {tol}");
                var interval = new Interval(-0.5, 0.5);
                var trimmedPlane = Brep.CreateTrimmedPlane(plane,
                    new Rectangle3d(plane, interval, interval).ToNurbsCurve());
                var split = _geometry.Split(trimmedPlane, tol * 2.0);
                if (split.Length != 2)
                {
                    Intersection.BrepPlane(_geometry, plane, tol, out var intCrvs, out var intPts);
                    List<Guid> ids = new List<Guid>();
                    foreach (var intCrv in intCrvs)
                    {
                        ids.Add(RhinoDoc.ActiveDoc.Objects.AddCurve(intCrv));
                    }

                    foreach (var brep in split)
                    {
                        ids.Add(RhinoDoc.ActiveDoc.Objects.AddBrep(brep));
                    }
                    ids.Add(RhinoDoc.ActiveDoc.Objects.AddBrep(trimmedPlane));

                    RhinoDoc.ActiveDoc.Groups.Add("end", ids);
                }
                else
                {
                    var areas = (from part in split select part.GetArea()).ToArray();
                    Array.Sort(areas, split);
                    _geometry = split[1];
                }
            }

            else _geometry = trimmed[0];
        }

        public Brep GetVolumeGeometry()
        {
            return _volumeGeometry.DuplicateBrep();
        }

        public Vector3d[] GetTangents()
        {
            return new[] {_startTangent, _endTangent};
        }

        public object Clone()
        {
            return new Beam
            {
                Axis = Axis.DuplicateCurve(),
                _endTangent = _endTangent,
                _geometry = _geometry.DuplicateBrep(),
                _startTangent = _startTangent,
                _volumeGeometry = _volumeGeometry.DuplicateBrep(),
                EvaluationDistance = EvaluationDistance,
                Height = Height,
                Up = Up,
                Width = Width
            };
        }

        public Beam Duplicate()
        {
            return (Beam) Clone();
        }
    }
}
