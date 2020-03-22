using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

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
            _endTangent = Axis.TangentAtEnd;

            int divisionCount = Convert.ToInt32(Math.Ceiling(Axis.GetLength() / EvaluationDistance));
            var planeParams = Axis.DivideByCount(divisionCount, true);

            var crossSections = new Curve[planeParams.Length];
            var widthInterval = new Interval(-Width / 2.0, Width / 2.0);
            var heightInterval = new Interval(-Height / 2.0, Height / 2.0);

            for (int i = 0; i < crossSections.Length; i++)
            {
                var param = planeParams[i];
                var tangent = Axis.TangentAt(param);
                bool success = Axis.PerpendicularFrameAt(param, out var plane);

                var xDirection = Vector3d.CrossProduct(tangent, Up);
                var angle = Vector3d.VectorAngle(plane.XAxis, xDirection, plane);
                plane.Rotate(angle, plane.ZAxis);

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
            if (plane == Plane.Unset) return;

            var trimmed = _geometry.Trim(plane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if (trimmed.Length != 1) _geometry = _volumeGeometry.DuplicateBrep();
            else _geometry = trimmed[0];
        }

        public Brep GetGeometry()
        {
            return _geometry;
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
