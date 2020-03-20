using System;
using System.Collections.Generic;
using BachelorThesis.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BachelorThesis.Components
{
    public class Beam : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Beam class.
        /// </summary>
        public Beam()
          : base("Beam", "Beam",
              "Creates a beam",
              BachelorThesisInfo.MAINCATEGORY, BachelorThesisInfo.SUBCATEGORY_CREATE)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Axis", "A", "Axis of beam", GH_ParamAccess.item);
            pManager.AddVectorParameter("Up", "U", "Up direction of beam", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width of beam", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height of beam", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Beam", "B", "The created beam", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve axis = null;
            Vector3d up = Vector3d.Unset;
            double width = 0;
            double height = 0;

            if (!DA.GetData(0, ref axis)) return;
            if (!DA.GetData(1, ref up)) return;
            if (!DA.GetData(2, ref width)) return;
            if (!DA.GetData(3, ref height)) return;

            DA.SetData(0, new BeamBase(axis, width, height, up));
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
            get { return new Guid("2b1c4688-54fb-492f-8035-f44de035aa69"); }
        }
    }
}