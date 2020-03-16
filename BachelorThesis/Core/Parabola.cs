using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace BachelorThesis.Core
{
    public class Parabola
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public Plane Plane { get; set; }

        public BezierCurve Curve { get; }

        public Parabola(double width, double height, Plane plane)
        {
            Width = width;
            Height = height;
            Plane = plane;

            Curve = new BezierCurve(CalculateControlPoints());
        }

        private Point3d[] CalculateControlPoints()
        {
            var controlPoints = new Point3d[3];
            controlPoints[0] = GetPointAtWidth(-Width / 2.0);
            controlPoints[2] = GetPointAtWidth(Width / 2.0);

            var topControlPoint = GetPointsAtHeight(Height)[0];
            topControlPoint.Transform(Transform.Translation(topControlPoint - Plane.Origin));
            controlPoints[1] = topControlPoint;

            return controlPoints;
        }

        public double[] EvaluateY(double y)
        {
            if (y > Height) return null;

            if (y == Height) return new double[] {0.0};

            var xValues = new double[2];
            xValues[0] = -Math.Sqrt(Width * Width * (Height - y) / 4 * Height);
            xValues[1] = -xValues[0];

            return xValues;
        }

        public double EvaluateX(double x)
        {
            return (-4 * Height) / (Width * Width) + Height;
        }

        public Point3d[] GetPointsAtHeight(double height)
        {
            var xValues = EvaluateY(height);

            if (xValues is null) return null;

            var points = new Point3d[xValues.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = Plane.PointAt(xValues[i], height);
            }

            return points;
        }

        public Point3d GetPointAtWidth(double width)
        {
            return Plane.PointAt(width, EvaluateX(width));
        }

    }
}
