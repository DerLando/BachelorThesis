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

        /// <summary>
        /// Erzeuge eine neue <see cref="RotatedParabola"/> Instanz.
        /// </summary>
        /// <param name="width">Die Gesamtbreite der Parabel, auf der X-Achse</param>
        /// <param name="height">Die Gesamthöhe der Parabel, auf der Y-Achse</param>
        /// <param name="plane">Die Konstruktionsebene, auf welcher die Parabel erzeugt wird</param>
        /// <param name="drag"></param>
        public RotatedParabola(double width, double height, Plane plane, double drag) : base(width, height, plane)
        {
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

        /// <summary>
        /// Finde alle Punkte auf dieser <see cref="RotatedParabola"/>,
        /// welche eine Höhe von 'globalHeight' haben.
        /// </summary>
        /// <param name="globalHeight">Die Höhe, für welche Punkte gesucht werden</param>
        /// <returns>Die gefundenen Punkte, falls vorhanden</returns>
        public Point3d[] GetGlobalPointsAtHeight(double globalHeight)
        {
            if (this.Curve is null) return base.GetPointsAtHeight(globalHeight);

            // erzeuge eine Hilfs-Schnittebene auf Höhe von 'globalHeight'
            var cuttingPlane = Plane.WorldXY;
            var origin = new Point3d(0.0, 0.0, globalHeight);
            cuttingPlane.Origin = origin;

            // erzeuge eine leere Liste für alle gefundenen Schnittpunkte
            var intPoints = new List<Point3d>();

            // suche nach Schnittmengen zwischen der Schnittebene und der Parabel
            var xEvents = Intersection.CurvePlane(this.Curve.ToNurbsCurve(), cuttingPlane, 0.001);
            if (xEvents is null) return intPoints.ToArray();
            foreach (var xEvent in xEvents)
            {
                // Falls in der Schnittmenge Punkte vorhanden sind, speichere sie
                if (!xEvent.IsPoint) continue;
                intPoints.Add(xEvent.PointA);
            }

            // Gib alle gefundenen Schnittpunkte aus
            return intPoints.ToArray();

        }
    }
}
