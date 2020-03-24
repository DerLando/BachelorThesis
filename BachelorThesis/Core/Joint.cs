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
        public Point3d Center { get; private set; }
        public Vector3d[] EndTangents { get; set; }
        public Point3d[] IntPts { get; set; }

        public Joint(List<Beam> beams, Point3d center)
        {
            Beams = beams;
            Center = center;
        }

        public void AddBeam(Beam beam)
        {
            Beams.Add(beam);
        }

        private CurveEnd FindEnd(Beam beam)
        {
            beam.Axis.ClosestPoint(Center, out var param);
            var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var epsilon = beam.Axis.Domain.Length * tol;

            var isStart = Math.Abs(param - beam.Axis.Domain.Min) < epsilon;
            var isEnd = Math.Abs(param - beam.Axis.Domain.Max) < epsilon;
            if (isStart && !isEnd) return CurveEnd.Start;
            if (!isStart && isEnd) return CurveEnd.End;
            if (isStart && isEnd)
                throw new Exception(
                    $"FindEnd ERROR: Could not define end for {beam.Axis} at param {param} with epsilon {epsilon}!");
            return CurveEnd.None;
        }

        /// <summary>
        /// Aligns all beams connected by this joint, so they don't overlap each other
        /// </summary>
        public void AlignBeamGeometry()
        {
            var ends = (from beam in Beams select FindEnd(beam)).ToArray();
            var mainBeamIndex = ends.ToList().FindIndex(e => e == CurveEnd.None);
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
                    doc.Objects.AddCurve(curBeam.Axis);
                    doc.Objects.AddBrep(mainBeamGeo);
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
