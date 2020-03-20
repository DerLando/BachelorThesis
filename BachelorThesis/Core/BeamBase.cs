using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace BachelorThesis.Core
{
    public class BeamBase
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

        public BeamBase(Curve axis, double width, double height, Vector3d up)
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

        public Brep GetGeometry()
        {
            return _geometry;
        }
    }
}
