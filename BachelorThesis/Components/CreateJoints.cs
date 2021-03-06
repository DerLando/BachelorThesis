﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            pManager.AddGenericParameter("Radius", "R", "Radius of joints created", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Joints", "J", "Created joints", GH_ParamAccess.list);
            pManager.AddGenericParameter("Beams", "B", "Beams with modified joint geometry", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Core.Beam> beams = new List<Core.Beam>();
            double radius = 0;

            if(!DA.GetDataList(0, beams)) return;
            if (!DA.GetData(1, ref radius)) return;

            var joints = JointFactory.CreateJoints(beams, radius, out var beamArray).ToArray();
            foreach (var joint in joints)
            {
                joint.AlignBeamGeometry();
            }

            foreach (var beam in beamArray)
            {
                beam.ShortenEnds();
            }

            DA.SetDataList(0, joints);
            DA.SetDataList(1, beamArray);
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