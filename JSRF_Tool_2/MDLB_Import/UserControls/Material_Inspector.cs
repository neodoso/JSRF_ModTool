using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JSRF_ModTool.MDLB_Import;

namespace JSRF_ModTool.MDLB_Import
{
    public partial class Material_Inspector : UserControl
    {
        public int id;

        public Material_Inspector()
        {
            InitializeComponent();
        }

        public void set_values(Materials.color c, int shader_id, int unk_id, float hb)
        {
            // set color
            color_0.set_colors(c.R, c.G, c.B, c.A);

            #region set material id

            // set material id
            string[] names = Enum.GetNames(typeof(Materials.materials));
            int[] values = (int[])Enum.GetValues(typeof(Materials.materials));
            bool mat_found = false;


            // find corresponding material id and select item in combobox
            for (int i = 0; i < names.Length; i++)
            {
                //int test = values[i];
                if (values[i] == shader_id)
                {
                    mat_found = true;
                    cb_material_id.SelectedItem = names[i];
                    break;
                }
            }

            if(!mat_found)
            {
                /// search to select the material
                cb_material_id.Items.Add(shader_id.ToString());
                int index = 0;
                for (int i = 0; i < cb_material_id.Items.Count-1; i++)
                {
                   
                    if (cb_material_id.Items[i].ToString() == shader_id.ToString())
                    {
                        cb_material_id.SelectedIndex = i;
                        break;
                    }
                }
                //cb_material_id.SelectedIndex = cb_material_id.Items.Count-1;
            }
            #endregion

            txtb_unk_id2.Text = unk_id.ToString();
            txtb_hb.Text = hb.ToString();
            lab_id.Text = "[mat_" + id.ToString() + "]";
        }

        public int get_shader_type()
        {

            string[] names = Enum.GetNames(typeof(Materials.materials));
            int[] values = (int[])Enum.GetValues(typeof(Materials.materials));

            int shader_id = 0;
            bool is_number = int.TryParse(cb_material_id.Text, out shader_id);

            // if the shader id is defined as a string
            if (!is_number)
            {
                // search shader name and corresponding return shader id from names/values arrays
                for (int n = 0; n < names.Length; n++)
                {
                    if (names[n] == cb_material_id.Text)
                    {
                        return values[n];
                    }
                }

            }

            return Convert.ToInt32(cb_material_id.Text);

        }

        public int get_unk_id()
        {
            return Convert.ToInt32(txtb_unk_id2.Text);
        }

        public Materials.color get_color()
        {
            return color_0.get_colors();
        }

        public float get_hb()
        {
            return Convert.ToSingle(txtb_hb.Text);
        }

        private void txtb_hb_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void txtb_hb_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (Convert.ToSingle(tb.Text) > Single.MaxValue)
            {
                tb.Text = Single.MaxValue.ToString();
            }

            if (Convert.ToSingle(tb.Text) < Single.MinValue)
            {
                tb.Text = Single.MinValue.ToString();
            }
        }

        private void MaterialProperties_Load(object sender, EventArgs e)
        {
  
            foreach (var item in Enum.GetNames(typeof(Materials.materials)))
            {
                cb_material_id.Items.Add(item);
            }

        }

        private void btn_remove_Click(object sender, EventArgs e)
        {

            Form ctrl = this.Parent.FindForm();
            ((MDLB_import_options_Form)ctrl).remove_mat_inspector(this.id);


            this.Dispose();
        }

        public void set_mat_id()
        {
            lab_id.Text = "[mat_" + id.ToString() + "]";
        }
    }
}
