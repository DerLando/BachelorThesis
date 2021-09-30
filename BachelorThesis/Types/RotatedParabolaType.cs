using BachelorThesis.Core;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BachelorThesis.Types
{
    class RotatedParabolaType : GH_Goo<RotatedParabola>
    {

        #region Constructors

        public RotatedParabolaType() : this(new RotatedParabola()) { }

        public RotatedParabolaType(RotatedParabola value) : base(value) { }

        public RotatedParabolaType(RotatedParabolaType source)
        {
            this.Value = source.Value.Duplicate();
        }

        #endregion

        #region GH_Goo implementation

        public override bool IsValid => true;

        public override string TypeName => "Rotated Parabola";

        public override string TypeDescription => "A Parabola that has been rotated in it's plane.S";

        public override IGH_Goo Duplicate()
        {
            return new RotatedParabolaType(this);
        }

        public override string ToString()
        {
            return $"RotatedParabola, width: {Value.Width}, height: {Value.Height}, plane: {Value.Plane}S";
        }

        #endregion

        #region Serialization

        public override bool Write(GH_IWriter writer)
        {
            writer.SetDouble("width", Value.Width);
            writer.SetDouble("height", Value.Height);
            writer.SetPlane("plane", Value.Plane.ToGHPlane());
            writer.SetPoint3D("ctrl_left", Value.Curve.GetControlVertex3d(0).ToGHPoint3d());
            writer.SetPoint3D("ctrl_middle", Value.Curve.GetControlVertex3d(1).ToGHPoint3d());
            writer.SetPoint3D("ctrl_right", Value.Curve.GetControlVertex3d(2).ToGHPoint3d());

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            var width = reader.GetDouble("width");
            var height = reader.GetDouble("height");
            var plane = reader.GetPlane("plane").ToPlane();
            var ctrlLeft = reader.GetPoint3D("ctrl_left").ToPoint3d();
            var ctrlMiddle = reader.GetPoint3D("ctrl_middle").ToPoint3d();
            var ctrlRight = reader.GetPoint3D("ctrl_right").ToPoint3d();

            this.Value.Width = width;
            this.Value.Height = height;
            this.Value.Plane = plane;
            this.Value.Curve = new BezierCurve(new[] { ctrlLeft, ctrlMiddle, ctrlRight });

            return base.Read(reader);
        }

        #endregion

        #region casting

        public override bool CastTo<Q>(ref Q target)
        {
            if(typeof(Q).IsAssignableFrom(typeof(Curve))) {
                object ptr = this.Value.Curve.ToNurbsCurve();
                target = (Q)ptr;
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)))
            {
                object ptr = new GH_Curve(this.Value.Curve.ToNurbsCurve());
                target = (Q)ptr;
                return true;
            }

            return false;
        }

        public override bool CastFrom(object source)
        {
            //Abort immediately on bogus data.
            if (source == null) { return false; }

            if (source is RotatedParabola)
            {
                this.Value = source as RotatedParabola;
                return true;
            }

            Curve crv = null;

            if (source is GH_Curve)
            {
                if (!GH_Convert.ToCurve(source, ref crv, GH_Conversion.Both))
                    return false;
            }
            if (source is Curve)
                crv = source as Curve;

            if (RotatedParabola.TryFromCurve(crv, out var parabola))
            {
                this.Value = parabola;
                return true;
            }

            return false;
        }

        public override object ScriptVariable()
        {
            return this.Value.Duplicate();
        }
        #endregion

    }
}
