using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSRF_ModTool.MDLB_Import
{
    public class ModelPart_Import_Settings
    {
        public string filepath { get; set; }
        public int model_type { get; set; }
        public int vertex_def_size { get; set; }
        public Single drawDist_x { get; set; }
        public Single drawDist_y { get; set; }
        public Single drawDist_z { get; set; }
        public Single drawDist_w { get; set; }


        public ModelPart_Import_Settings(string _filepath, int _model_type, int _vertex_def_size, Single _drawDist_x, Single _drawDist_y, Single _drawDist_z, Single _drawDist_w)
        {
            this.filepath = _filepath;
            this.model_type = _model_type;
            this.vertex_def_size = _vertex_def_size;

            this.drawDist_x = _drawDist_x;
            this.drawDist_y = _drawDist_y;
            this.drawDist_z = _drawDist_z;
            this.drawDist_w = _drawDist_w;
        }
    }
}
