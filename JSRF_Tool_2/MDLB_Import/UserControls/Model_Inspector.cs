using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSRF_ModTool.MDLB_Import
{
    public partial class Model_Inspector : UserControl
    {
        public string filepath;
        public Model_Inspector()
        {
            InitializeComponent();
        }

        public int get_mdl_type()
        {
            return cb_mdl_part_type.SelectedIndex;
        }

        public int get_vert_def_size()
        {
            return Convert.ToInt32(cb_vertex_def_size.Text);
        }


        public void set_vert_def_size(int s)
        {
            if(cb_mdl_part_type.SelectedIndex > 0)
            {
                s -= 8;
            }

            cb_vertex_def_size.SelectedItem = s.ToString();
        }
    }
}
