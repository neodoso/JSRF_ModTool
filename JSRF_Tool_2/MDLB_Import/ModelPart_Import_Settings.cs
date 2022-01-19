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
        public float draw_distance { get; set; }


        public ModelPart_Import_Settings(string _filepath, int _model_type, int _vertex_def_size, float _draw_distance)
        {
            this.filepath = _filepath;
            this.model_type = _model_type;
            this.vertex_def_size = _vertex_def_size;
            this.draw_distance = _draw_distance;
        }
    }
}
