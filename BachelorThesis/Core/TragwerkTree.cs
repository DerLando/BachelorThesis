using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace BachelorThesis.Core
{
    public class TragwerkTree
    {
        private RTree _tree;

        public TragwerkTree()
        {
            _tree = new RTree();
        }

        public int Insert(JointVoxel voxel)
        {
            var data = new SearchData();
            _tree.Search(voxel.BoundingBox, SearchCallback, data);

            // voxel is contained in other voxel, return that containing index
            if (data.FoundSomething) return data.ContainedIndex;

            // otherwise insert voxel in tree
            _tree.Insert(voxel.BoundingBox, voxel.Index);
            return voxel.Index;
        }

        private void SearchCallback(object sender, RTreeEventArgs e)
        {
            var data = e.Tag as SearchData;
            if (data is null) return;

            data.ContainedIndex = e.Id;
            e.Cancel = true;
        }
    }

    class SearchData
    {
        public int ContainedIndex = -1;

        public SearchData() { }

        public bool FoundSomething => ContainedIndex != -1;
    }
}
