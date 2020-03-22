using System;
using System.Collections.Generic;
using BachelorThesis.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BachelorThesis.Components
{
    public class ExplodeBeam : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExplodeBeam class.
        /// </summary>
        public ExplodeBeam()
          : base("ExplodeBeam", "Nickname",
              "Description",
              BachelorThesisInfo.MAINCATEGORY, BachelorThesisInfo.SUBCATEGORY_EXPLODE)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Beam", "B", "Beam to explode", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Volume geometry", GH_ParamAccess.item);
            pManager.AddCurveParameter("Axis", "A", "Axis of beam", GH_ParamAccess.item);
            pManager.AddVectorParameter("StartTangent", "S", "Tangent at start", GH_ParamAccess.item);
            pManager.AddVectorParameter("EndTangent", "E", "Tangent at end", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Core.Beam beam = null;

            if (!DA.GetData(0, ref beam)) return;

            DA.SetData(0, beam.Geometry.DuplicateBrep());
            DA.SetData(1, beam.Axis.DuplicateCurve());
            DA.SetData(2, beam.GetTangents()[0]);
            DA.SetData(3, beam.GetTangents()[1]);
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
            get { return new Guid("b8d382c5-71e6-49a0-8a61-e7d805c07b36"); }
        }
    }
}