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
        // Breite der Parabel bzw. Abstand der Nullstellen
        public double Width;
        // Höhe der Parabel bzw. Y-Koordinate des Scheitelpunkts
        public double Height;
        // Rhino - Koordinatenebene auf welcher die Parabel definiert wird
        public Plane Plane;
        // 3d - Geometry der Parabel, ausgedrückt als Rhino - Bezierkurve
        public BezierCurve Curve { get; internal set; }

        public Parabola()
        {
            Width = 0.0;
            Height = 0.0;
            Plane = Plane.Unset;
            Curve = null;
        }

        /// Funktion, welche ein neues `Parabel-Objekt` mit den gegebenen Werten
        /// für Breite, Höhe und Koordinatensystem erzeugt
        public Parabola(double width, double height, Plane plane)
        {
            Width = width;
            Height = height;
            Plane = plane;

            Curve = new BezierCurve(CalculateControlPoints());
        }

        /// Errechnet die genaue Position der drei Kontrollpunkte, die benötigt werden
        /// um die Parabel durch eine Bezierkurve abbilden zu können
        protected Point3d[] CalculateControlPoints()
        {
            // Leeres `Array`, welches die 3 Kontrollpunkte aufnehmen kann
            var controlPoints = new Point3d[3];
            // Linke Nullstelle x0
            controlPoints[0] = GetPointAtWidth(-Width / 2.0);
            // Rechte Nullstelle x1
            controlPoints[2] = GetPointAtWidth(Width / 2.0);

            // Scheitelpunkt
            var topControlPoint = GetPointsAtHeight(Height)[0];
            // Verschiebe Scheitelpunkt um seine Höhe nach oben
            topControlPoint.Transform(Transform.Translation(topControlPoint - Plane.Origin));
            controlPoints[1] = topControlPoint;

            // Ausgabe der Kontrollpunkte
            return controlPoints;
        }

        /// Evaluiert einen gegebenen Y-Wert bzw. eine gegebene Höhe.
        public double[] EvaluateY(double y)
        {
            // Falls die gegebene Höhe höher ist als die Parabel, gibt es keine Lösung
            if (y > Height) return null;

            // Bei exakter Parabelhöhe wird die X-Koordinate des Koordinatenursprungs ausgegeben
            if (y == Height) return new double[] {0.0};

            // Leeres `Array` in welches die beiden X-Werte eingefügt werden
            var xValues = new double[2];
            // x1 = -Wurzel(w²(h-y)/4h)
            xValues[0] = -Math.Sqrt(Width * Width * (Height - y) / (4 * Height));
            // x2 = -x, da Parabel Symmetrisch
            xValues[1] = -xValues[0];

            // Ausgabe der gefundenen X-Werte
            return xValues;
        }

        /// Evaluiert einen gegebenen X-Wert bzw. eine gegebene Breite.
        public double EvaluateX(double x)
        {
            // y = (-4h/w²)x² + h
            return (-4 * Height) / (Width * Width) * (x * x) + Height;
        }

        /// Ermittelt die Punkte auf der Parabel zu einer gegebenen Höhe
        public virtual Point3d[] GetPointsAtHeight(double height)
        {
            // Ermittle zur Höhe zugehörige X-Werte
            var xValues = EvaluateY(height);

            // Falls es keine gibt, kein Ergebnis
            if (xValues is null) return null;

            // Leeres `Array` um die gefundenen Punkte aufzunehmen
            var points = new Point3d[xValues.Length];
            // Für jeden gefunden X-Wert...
            for (int i = 0; i < points.Length; i++)
            {
                // ...füge den Kontrollpunkt P(X-Wert, Höhe) in die Liste ein
                points[i] = Plane.PointAt(xValues[i], height);
            }

            // Ausgabe der gefundenen Punkte
            return points;
        }

        /// Ermittelt den Punkt auf der Parabel zu einer gegbenen Breite,
        /// gemessen vom Koordinatenursprung (Breite kann negativ sein)
        public Point3d GetPointAtWidth(double width)
        {
            return Plane.PointAt(width, EvaluateX(width));
        }

        //public Parabola Duplicate()
        //{
        //    return new Parabola(Width, Height, Plane);
        //}
    }
}
