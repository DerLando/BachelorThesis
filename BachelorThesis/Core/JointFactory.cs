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
        public static IEnumerable<Joint> CreateJoints(IEnumerable<Beam> beams, out Beam[] beamArray)
        {
            beamArray = (from beam in beams select beam.Duplicate()).ToArray();
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

            return from valuePair in vertexBeamTable select new Joint(valuePair.Value, new JointVoxel(valuePair.Key, tol, 0));
        }

        public static IEnumerable<Joint> CreateJoints(IEnumerable<Beam> beams, double jointRadius, out Beam[] beamArray)
        {
            beamArray = (from beam in beams select beam.Duplicate()).ToArray();
            var indices = Enumerable.Range(0, beamArray.Length);
            var beamIndexPairs = indices.Permutations().ToArray();
            var tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            var jointTree = new TragwerkTree();
            var beamTable = new Dictionary<int, List<Beam>>();
            var voxelTable = new Dictionary<int, JointVoxel>();

            ///TODO: write this method utilizing voxels
            /// The idea is to have an insert method on the voxel tree, which returns
            /// either the index integer of the voxel to be inserted
            /// or the index of another voxel containing this voxel
            /// From that we can build up our beamTable with its voxel indices
            /// storing lists of beams inside of them
            ///

            for (int i = 0; i < beamIndexPairs.Length; i++)
            {
                var curPair = beamIndexPairs[i];
                var beamA = beamArray[curPair.Item1];
                var beamB = beamArray[curPair.Item2];

                var xEvents = Intersection.CurveCurve(beamA.Axis, beamB.Axis, tol * 2.0, 0.0);
                if(xEvents.Count == 0) continue;

                foreach (var curveIntersection in xEvents)
                {
                    if (!curveIntersection.IsPoint) continue;
                    var intPoint = RoundPoint(curveIntersection.PointA, tol);
                    var voxel = new JointVoxel(intPoint, jointRadius, i);

                    var index = jointTree.Insert(voxel);

                    if (index == i)
                    {
                        beamTable[index] = new List<Beam> {beamA, beamB};
                        voxelTable[index] = voxel;
                    }
                    else
                    {
                        if(!beamTable[index].Contains(beamA)) beamTable[index].Add(beamA);
                        if(!beamTable[index].Contains(beamB)) beamTable[index].Add(beamB);
                    }
                }
            }

            return from valuePair in beamTable select new Joint(valuePair.Value, voxelTable[valuePair.Key]);
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
