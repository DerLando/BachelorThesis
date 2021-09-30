using System;
using System.Collections.Generic;
using System.Linq;
using BachelorThesis.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace BachelorThesis.Components
{
    public class RotatedParabola : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RotatedParabola class.
        /// </summary>
        public RotatedParabola()
          : base("RotatedParabola", "RPRBL",
              "Creates a downwards pointing RotatedParabola",
              BachelorThesisInfo.MAINCATEGORY, BachelorThesisInfo.SUBCATEGORY_CREATE)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Plane to construct RotatedParabola on", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width of the RotatedParabola at y=0", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height of the RotatedParabola at x=0", GH_ParamAccess.item);
            pManager.AddNumberParameter("Drag", "D", "Drag from -1.0 to 1.0", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parabola", "P", "The generated parabola", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curve", "C", "Curve geometry of the RotatedParabola up to y=0", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = Plane.Unset;
            double width = 0.0;
            double height = 0.0;
            double drag = 0.0;
            List<double> heightEvaluation = new List<double>();

            if (!DA.GetData("Plane", ref plane)) return;
            if (!DA.GetData("Width", ref width)) return;
            if (!DA.GetData("Height", ref height)) return;
            if (!DA.GetData("Drag", ref drag)) return;

            var rotatedParabola = new Core.RotatedParabola(width, height, plane, drag);

            DA.SetData("Parabola", new RotatedParabolaType(rotatedParabola));

            DA.SetData("Curve", rotatedParabola.Curve.ToNurbsCurve());
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("BC72FD4C-EDED-4316-8EBB-9FC568535CA9"); }
        }
    }
}