using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace BachelorThesis
{
    public class BachelorThesisInfo : GH_AssemblyInfo
    {
        public static string MAINCATEGORY = "Parabola";
        public static string SUBCATEGORY_CREATE = "Create";


        public override string Name
        {
            get
            {
                return "BachelorThesis";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("b29ee3ce-ed5f-4c68-91d2-bd01ac9d5519");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
