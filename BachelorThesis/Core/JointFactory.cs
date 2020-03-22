using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace BachelorThesis.Core
{
    public static class JointFactory
    {
        public static IEnumerable<Joint> CreateJoints(IEnumerable<Beam> beams)
        {
            var beamArray = (from beam in beams select beam.Duplicate()).ToArray();
            var axes = (from beam in beamArray select beam.Axis).ToArray();
            Dictionary<Point3d, List<Beam>> vertexBeamTable = new Dictionary<Point3d, List<Beam>>();
            var tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            for (int i = 0; i < axes.Length; i++)
            {

                for (int j = 0; j < axes.Length; j++)
                {
                    if (i == j) continue;

                    var xEvents = Intersection.CurveCurve(axes[i], axes[j], tol * 2.0, 0.0);
                    foreach (var curveIntersection in xEvents)
                    {
                        if (!curveIntersection.IsPoint) continue;
                        var intPoint = RoundPoint(curveIntersection.PointA, tol);

                        if (vertexBeamTable.ContainsKey(intPoint))
                        {
                            if (!vertexBeamTable[intPoint].Contains(beamArray[i]))
                                vertexBeamTable[intPoint].Add(beamArray[i]);
                            if (!vertexBeamTable[intPoint].Contains(beamArray[j]))
                                vertexBeamTable[intPoint].Add(beamArray[j]);
                        }
                        else
                        {
                            vertexBeamTable.Add(intPoint, new List<Beam>{ beamArray[i], beamArray[j] });
                        }
                    }
                }
            }

            return from valuePair in vertexBeamTable select new Joint(valuePair.Value, valuePair.Key);
        }

        private static Point3d RoundPoint(Point3d point, double tolerance)
        {
            var tolFactor = 1.0 / tolerance;
            return new Point3d(RoundDouble(point.X, tolerance), RoundDouble(point.Y, tolerance),
                RoundDouble(point.Z, tolerance));
        }

        private static double RoundDouble(double d, double tolerance)
        {
            var tolFactor = 1.0 / tolerance;
            return Math.Round(d * tolFactor, 2) / tolFactor;
        }

        private static byte[] HashPoint(Point3d point)
        {
            var hash = new NameHash(point.ToString());
            return hash.Sha1Hash;
        }
    }
}
