using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace BachelorThesis.Components
{
    public class Parabola : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Parabola class.
        /// </summary>
        public Parabola()
          : base("Parabola", "PRBL",
              "Creates a downwards pointing Parabola",
              BachelorThesisInfo.MAINCATEGORY, BachelorThesisInfo.SUBCATEGORY_CREATE)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Plane to construct parabola on", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width of the parabola at y=0", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height of the parabola at x=0", GH_ParamAccess.item);
            pManager.AddNumberParameter("Evaluate", "E", "Evaluates the Parabola at the given height values",
                GH_ParamAccess.list);

            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve geometry of the parabola up to y=0", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Evaluated height points", GH_ParamAccess.tree);
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
            List<double> heightEvaluation = new List<double>();

            if (!DA.GetData("Plane", ref plane)) return;
            if (!DA.GetData("Width", ref width)) return;
            if (!DA.GetData("Height", ref height)) return;

            DA.GetDataList("Evaluate", heightEvaluation);

            var parabola = new Core.Parabola(width, height, plane);
            DA.SetData("Curve", parabola.Curve.ToNurbsCurve());

            if (heightEvaluation.Count > 0)
            {
                var points = new DataTree<GH_Point>();
                for (int i = 0; i < heightEvaluation.Count; i++)
                {
                    var evaluated = parabola.GetPointsAtHeight(heightEvaluation[i]);
                    if (evaluated is null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            "Height values higher then the parabola can not be evaluated!");
                        points.Add(null, new GH_Path(i));
                        continue;
                    }

                    points.AddRange(from point in evaluated select new GH_Point(point), new GH_Path(i));
                }

                DA.SetDataTree(1, points);
            }
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
            get { return new Guid("84ae1d53-4cf1-4ebc-97b8-289b069c932d"); }
        }
    }
}