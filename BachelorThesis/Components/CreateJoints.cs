using System;
using System.Collections.Generic;
using BachelorThesis.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BachelorThesis.Components
{
    public class CreateJoints : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateJoints class.
        /// </summary>
        public CreateJoints()
          : base("CreateJoints", "CreateJs",
              "Creates Joints from a network of beams",
              BachelorThesisInfo.MAINCATEGORY, BachelorThesisInfo.SUBCATEGORY_CREATE)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Beams", "B", "Beams to create joints from", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Joints", "J", "Created joints", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BeamBase> beams = new List<BeamBase>();

            if(!DA.GetDataList(0, beams)) return;

            DA.SetDataList(0, JointFactory.CreateJoints(beams));
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
            get { return new Guid("42025e7d-af4b-4c25-b7f7-a7e7a34d276e"); }
        }
    }
}