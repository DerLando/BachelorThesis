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
    public class Joint
    {
        public List<Beam> Beams { get; private set; } = new List<Beam>();
        public JointVoxel Voxel { get; private set; }
        public Point3d Center { get; private set; }
        public Vector3d[] EndTangents { get; set; }
        public Point3d[] IntPts { get; set; }

        public Joint(List<Beam> beams, JointVoxel voxel)
        {
            Beams = beams;
            Voxel = voxel;
            Center = Voxel.Center;
        }

        public void AddBeam(Beam beam)
        {
            Beams.Add(beam);
        }

        private CurveEnd FindEnd(Beam beam)
        {
            var isStart = Voxel.Contains(beam.Axis.PointAtStart);
            var isEnd = Voxel.Contains(beam.Axis.PointAtEnd);
            if (isStart && !isEnd) return CurveEnd.Start;
            if (!isStart && isEnd) return CurveEnd.End;
            if (isStart && isEnd)
                throw new Exception(
                    $"FindEnd ERROR: Could not define end for {beam.Axis} for end points {beam.Axis.PointAtStart}, {beam.Axis.PointAtEnd} and voxel {Voxel}!");
            return CurveEnd.None;
        }

        /// <summary>
        /// Aligns all beams connected by this joint, so they don't overlap each other
        /// </summary>
        public void AlignBeamGeometry()
        {
            var ends = (from beam in Beams select FindEnd(beam)).ToArray();
            var mainBeamIndex = ends.ToList().FindIndex(e => e == CurveEnd.None);

            if (mainBeamIndex == -1)
            {
                // test for closed loop
                var testLoop = Curve.JoinCurves(from beam in Beams select beam.Axis,
                    RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 2);
                if (testLoop.Length == 1 && testLoop[0].IsClosed)
                {
                    // TODO: Actually handle the closed loop case
                    return;
                }
                var doc = RhinoDoc.ActiveDoc;
                var ids = new List<Guid>();
                ids.Add(doc.Objects.AddSphere(Voxel.Sphere));
                ids.Add(doc.Objects.AddPoint(Voxel.Center));
                foreach (var beam in Beams)
                {
                    ids.Add(doc.Objects.AddBrep(beam.Geometry));
                    ids.Add(doc.Objects.AddCurve(beam.Axis));
                }

                doc.Groups.Add("No main beam", ids);
                throw new Exception("AlignBeamGeometry ERROR: No Main beam found!");
            }

            var mainBeam = Beams[mainBeamIndex];
            var mainBeamGeo = mainBeam.GetVolumeGeometry();

            var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var node = new Sphere(Center, mainBeam.Width / 2.0).ToNurbsSurface();

            var endTangents = new List<Vector3d>();
            var intPts = new List<Point3d>();

            for (int i = 0; i < Beams.Count; i++)
            {
                if (i == mainBeamIndex) continue;

                var curBeam = Beams[i];
                var curEnd = ends[i];

                // Should never have multiple mainBeam!
                if (curEnd == CurveEnd.None)
                {
                    if (ends.All(e => e == CurveEnd.None)) continue;
                    var doc = RhinoDoc.ActiveDoc;
                    var ids = new List<Guid>();
                    foreach (var beam in Beams)
                    {
                        ids.Add(doc.Objects.AddCurve(beam.Axis));
                    }
                    ids.Add(doc.Objects.AddBrep(mainBeamGeo));
                    ids.Add(doc.Objects.AddPoint(Voxel.Center));
                    doc.Groups.Add("Voxel", ids);
                    throw new Exception($"AlignBeamGeometry ERROR: {curEnd} for beam {i}!");
                }

                var axis = curBeam.Axis.DuplicateCurve();

                // Axis should always intersect the mainBeamGeometry!
                if (!Intersection.CurveBrep(axis, mainBeamGeo, tol, 0.01, out var crvIntParams))
                {
                    throw new Exception("AlignBeamGeometry ERROR: Axis did not intersect mainBeamGeometry!");
                }

                if (crvIntParams.Length > 1)
                {
                    crvIntParams[0] = crvIntParams.First(p => axis.ClosestEndFromParam(p) == curEnd);
                }

                var intPt = axis.PointAt(crvIntParams[0]);
                mainBeamGeo.ClosestPoint(intPt, out _, out _, out _, out _, tol, out var normal);

                curBeam.SetEndCondition(curEnd, normal, crvIntParams[0]);

                axis.Dispose();

                endTangents.Add(normal);
                intPts.Add(intPt);
            }

            mainBeamGeo.Dispose();

            EndTangents = endTangents.ToArray();
            IntPts = intPts.ToArray();

        }

    }
}
