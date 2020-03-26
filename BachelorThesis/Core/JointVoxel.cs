using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace BachelorThesis.Core
{
    public struct JointVoxel
    {
        public Interval X { get; set; }
        public Interval Y { get; set; }
        public Interval Z { get; set; }
        public Point3d Center { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public Sphere Sphere { get; set; }
        public double Radius { get; set; }
        public int Index { get; set; }

        private static Interval _intervalHelper(double mid, double radius)
        {
            return new Interval(mid - radius, mid + radius);
        }

        public JointVoxel(Point3d center, double radius, int index)
        {
            Center = center;
            X = _intervalHelper(Center.X, radius);
            Y = _intervalHelper(Center.Y, radius);
            Z = _intervalHelper(Center.Z, radius);
            BoundingBox = new BoundingBox(X.Min, Y.Min, Z.Min, X.Max, Y.Max, Z.Max);
            Radius = radius;
            Sphere = new Sphere(Center, Radius);
            Index = index;
        }

        public bool Contains(Point3d testPoint)
        {
            //return X.IncludesParameter(testPoint.X) && Y.IncludesParameter(testPoint.Y) &&
            //       Z.IncludesParameter(testPoint.Z);
            //return BoundingBox.Contains(testPoint);
            return Center.DistanceTo(testPoint) < Radius;
        }
    }
}
