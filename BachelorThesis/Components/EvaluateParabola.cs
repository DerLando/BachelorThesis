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
    public class EvaluateParabola : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EvaluateParabola class.
        /// </summary>
        public EvaluateParabola()
          : base("EvaluateParabola", "EPRBL",
              "Evaluates a parabola for the given parameter",
              BachelorThesisInfo.MAINCATEGORY, BachelorThesisInfo.SUBCATEGORY_CREATE)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Parabola", "P", "The parabola to evaluate", GH_ParamAccess.item);
            pManager.AddGenericParameter("Height", "H", "The height at which to evaluate", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "L", "Line connecting the two evaluated points, if any", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RotatedParabolaType parabola = null;
            double height = 0.0;

            if (!DA.GetData("Parabola", ref parabola)) return;
            if (!DA.GetData("Height", ref height)) return;

            var pts = parabola.Value.GetGlobalPointsAtHeight(height);

            switch (pts.Length)
            {
                case 0:
                    break;

                case 1:
                    DA.SetData("Line", new Line(pts[0], pts[0]));
                    break;

                case 2:
                    DA.SetData("Line", new Line(pts[0], pts[1]));
                    break;

                default: return;
            }

            if (pts.Length == 0) return;

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
            get { return new Guid("C196F2D3-4B37-4D65-B043-5BD1811AAF7E"); }
        }
    }
}