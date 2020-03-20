using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace BachelorThesis.Core
{
    public class Joint
    {
        public List<BeamBase> Beams { get; private set; } = new List<BeamBase>();
        public Point3d Center { get; private set; }

        public Joint(List<BeamBase> beams, Point3d center)
        {
            Beams = beams;
            Center = center;
        }

        public void AddBeam(BeamBase beam)
        {
            Beams.Add(beam);
        }

    }
}
