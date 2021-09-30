using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BachelorThesis.Core
{
    class RotatedParabola : Parabola
    {
        public RotatedParabola() : base()
        {
        }

        public RotatedParabola(double width, double height, Plane plane, double drag) : base(width, height, plane)
        {
            // TODO: Drag is doing something wierd
            var points = CalculateControlPoints();
            var displacement = plane.XAxis * drag * width / 2.0;
            points[1].Transform(Transform.Translation(displacement));

            Curve = new BezierCurve(points);
        }

        public RotatedParabola Duplicate()
        {
            return new RotatedParabola
            {
                Height = Height,
                Width = Width,
                Plane = Plane,
                Curve = new BezierCurve(new[] { Curve.GetControlVertex3d(0), Curve.GetControlVertex3d(1), Curve.GetControlVertex3d(2) })
            };
        }

        public static bool TryFromCurve(Curve crv, out RotatedParabola parabola)
        {
            parabola = null;

            if (!crv.IsValid | !crv.IsPlanar(0.01)) return false;

            var nurbs = crv.ToNurbsCurve();

            if (nurbs.Points.Count != 3) return false;

            var helper = new Line(nurbs.Points[0].Location, nurbs.Points[2].Location);
            var ctrlTop = nurbs.Points[1].Location;
            var width = helper.Length;
            var height = helper.DistanceTo(ctrlTop, true) / 2.0;
            var drag = helper.ClosestParameter(ctrlTop) * 2.0 - 1.0;
            var yAxis = nurbs.Points[1].Location - helper.ClosestPoint(ctrlTop, true);
            var plane = new Plane(helper.PointAt(0.5), helper.Direction, yAxis);

            parabola = new RotatedParabola(width, height, plane, drag);
            return true;
        }

        public Point3d[] GetGlobalPointsAtHeight(double globalHeight)
        {
            if (this.Curve is null) return base.GetPointsAtHeight(globalHeight);

            // TODO: Implement new height evaluation, which just generates a plane at the given plane,
            // Intersects it with the intenral curve,
            // Orders the intersection points along the intenral curve, if any
            // And returns them.
            var cuttingPlane = Plane.WorldXY;
            var origin = new Point3d(0.0, 0.0, globalHeight);
            cuttingPlane.Origin = origin;

            // TODO: Maybe there is a smart, mathematical way to evaluate thos points directly?

            var intPoints = new List<Point3d>();

            var xEvents = Intersection.CurvePlane(this.Curve.ToNurbsCurve(), cuttingPlane, 0.001);
            if (xEvents is null) return intPoints.ToArray();
            foreach (var xEvent in xEvents)
            {
                if (!xEvent.IsPoint) continue;

                intPoints.Add(xEvent.PointA);
            }

            return intPoints.ToArray();

            // TODO: How can we pass around rotated parabolas between definitions?


        }
    }
}
