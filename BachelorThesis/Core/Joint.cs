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

            if (Math.Abs(param - beam.Axis.Domain.Min) < 0.01) return CurveEnd.Start;
            if (Math.Abs(param - beam.Axis.Domain.Max) < 0.01) return CurveEnd.End;
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
            var mainBeamGeo = mainBeam.Geometry.DuplicateBrep();

            var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var node = new Sphere(Center, mainBeam.Width / 2.0).ToNurbsSurface();

            var endTangents = new List<Vector3d>();
            var intPts = new List<Point3d>();

            for (int i = 0; i < Beams.Count; i++)
            {
                if (i == mainBeamIndex) continue;

                var curBeam = Beams[i];
                var curEnd = ends[i];
                //var xEvents = Intersection.CurveSurface(curBeam.Axis, node, tol, 0.00);
                //foreach (var curveIntersection in xEvents)
                //{
                //    if (!curveIntersection.IsPoint) continue;
                //    curveIntersection.SurfacePointParameter(out var u, out var v);

                //    node.FrameAt(u, v, out var frame);
                //    curBeam.SetEndTangent(ends[i], frame.Normal);

                //    curBeam.Axis.ClosestPoint(frame.Origin, out var t);
                //    curBeam.ShortenEnd(ends[i], curBeam.Axis.GetLengthFromEnd(t, ends[i]));
                //}

                var axis = curBeam.Axis.DuplicateCurve();
                //if (!Intersection.CurveBrep(axis, mainBeamGeo, tol, out _,
                //    out var intPts, out var crvIntParams)) continue;
                if (!Intersection.CurveBrep(axis, mainBeamGeo, tol, 0.01, out var crvIntParams)) continue;

                var intPt = axis.PointAt(crvIntParams[0]);
                mainBeamGeo.ClosestPoint(intPt, out _, out _, out _, out _, tol, out var normal);
                curBeam.SetEndTangent(curEnd, normal);

                curBeam.ShortenEnd(curEnd, crvIntParams[0], false);

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
