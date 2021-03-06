﻿using System;
using System.Collections.Generic;
using BachelorThesis.Core;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BachelorThesis.Components
{
    public class ExplodeJoint : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExplodeJoint class.
        /// </summary>
        public ExplodeJoint()
          : base("ExplodeJoint", "JointBang",
              "Explodes a joint in its parts",
              BachelorThesisInfo.MAINCATEGORY, BachelorThesisInfo.SUBCATEGORY_EXPLODE)
        {
        }
        
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint", "J", "Joint to explode", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Beams", "B", "Beams joined by this joint", GH_ParamAccess.list);
            pManager.AddGenericParameter("Center", "C", "Center of joint force lines", GH_ParamAccess.item);
            pManager.AddVectorParameter("V", "V", "", GH_ParamAccess.list);
            pManager.AddPointParameter("P", "P", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Joint joint = null;

            if (!DA.GetData(0, ref joint)) return;

            DA.SetDataList(0, joint.Beams);
            DA.SetData(1, joint.Center);
            DA.SetDataList(2, joint.EndTangents);
            DA.SetDataList(3, joint.IntPts);
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
            get { return new Guid("76488188-b84d-459c-94d3-c8ef83dd8535"); }
        }
    }
}