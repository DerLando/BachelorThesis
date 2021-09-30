using GH_IO.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BachelorThesis.Types
{
    internal static class ConversionExtensions
    {
        internal static GH_Point3D ToGHPoint3d(this Point3d pt)
        {
            return new GH_Point3D(pt.X, pt.Y, pt.Z);
        }

        internal static GH_Point3D ToGHPoint3d(this Vector3d pt)
        {
            return new GH_Point3D(pt.X, pt.Y, pt.Z);
        }
        internal static GH_Plane ToGHPlane(this Plane plane)
        {
            return new GH_Plane(plane.Origin.ToGHPoint3d(), plane.XAxis.ToGHPoint3d(), plane.YAxis.ToGHPoint3d());
        }

        internal static Point3d ToPoint3d(this GH_Point3D pt)
        {
            return new Point3d(pt.x, pt.y, pt.z);
        }

        internal static Vector3d ToVector3d(this GH_Point3D pt)
        {
            return new Vector3d(pt.x, pt.y, pt.z);
        }

        internal static Plane ToPlane(this GH_Plane plane)
        {
            return new Plane(plane.Origin.ToPoint3d(), plane.XAxis.ToVector3d(), plane.YAxis.ToVector3d());
        }
    }
}
