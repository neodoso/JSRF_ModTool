using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JSRF_Tool_2.MDLB_Import;
using System.IO;
using System.Text.RegularExpressions;

namespace JSRF_Tool_2.MDLB_Import
{
    public partial class MDLB_import_options_Form : Form
    {
        public byte[] MDLB_import;
        public string main_SMD_filepath;
        public int srcMDLB_vert_size; // vertex def size of last part
        //public int srcMDLB_model_parts_cnt;
        //public int srcMDLB_materials_count;

        List<Material_Inspector> mat_inspector_list = new List<Material_Inspector>();
        List<Model_Inspector> mdl_inspector_list = new List<Model_Inspector>();

        public MDLB_import_options_Form()
        {
            InitializeComponent();
        }

        // on load
        private void MDLB_import_options_Form_Load(object sender, EventArgs e)
        {
            panel_materials.AutoScroll = true;
            panel_materials.HorizontalScroll.Visible = false;
            panel_materials.VerticalScroll.Visible = true;

            panel_models.AutoScroll = true;
            panel_models.HorizontalScroll.Visible = false;
            panel_models.VerticalScroll.Visible = true;


            srcMDLB_vert_size = Main.model.VertexBlock_header_List[Main.model.Model_Parts_header_List.Count-1].vertex_def_size;

            txtb_drawDist_x.Text = Main.model.Model_Parts_header_List[Main.model.Model_Parts_header_List.Count - 1].draw_distance_x.ToString();
            txtb_drawDist_y.Text = Main.model.Model_Parts_header_List[Main.model.Model_Parts_header_List.Count - 1].draw_distance_y.ToString();
            txtb_drawDist_z.Text = Main.model.Model_Parts_header_List[Main.model.Model_Parts_header_List.Count - 1].draw_distance_z.ToString();
            txtb_drawDist_w.Text = Main.model.Model_Parts_header_List[Main.model.Model_Parts_header_List.Count - 1].draw_distance_w.ToString();
            //srcMDLB_materials_count = Main.model.materials_List.Count;

            cb_vertex_def_size.SelectedItem = srcMDLB_vert_size.ToString();
            process_smd_input();
            load_materials();
        }

        private void process_smd_input()
        {
            lab_main_model_name.Text = Path.GetFileName(main_SMD_filepath);
            Size size = TextRenderer.MeasureText(lab_main_model_name.Text, lab_main_model_name.Font);
            lab_main_model_name.Width = size.Width;
            lab_main_model_name.Height = size.Height;


            #region get other related SMD files in folder

            string[] filepaths_arr = Directory.GetFiles(Path.GetDirectoryName(main_SMD_filepath), "*.smd");
            List<String> filepaths = filepaths_arr.ToList<String>();

            bool remove = false;
            for (int i = 0; i < filepaths.Count; i++)
            {
                string f = filepaths[i];
                remove = false;
                // remove main model from list
                if (f == main_SMD_filepath)
                {
                    remove = true;
                }
                // if filename doesn't match main file name remove item
                if(!Path.GetFileNameWithoutExtension(f).Contains(Path.GetFileNameWithoutExtension(main_SMD_filepath)))
                {
                    remove = true;
                }

                if(remove)
                {
                    filepaths.RemoveAt(i);
                    i--;
                }
            }

            List<String> filepaths_sorted = new List<string>(new string[filepaths.Count]);

            // TODO sort by number, ascending
            for (int i = 0; i < filepaths.Count; i++)
            {
                string filename = Path.GetFileName(filepaths[i]);
                string[] args = filename.Split('_');

                int number = -1;
                try
                {
                    number = Convert.ToInt32((args[args.Length - 1]).ToString().Split('.')[0]);
                }
                catch
                {

                }


                if (number >= 0)
                {
                  filepaths_sorted[number] = filepaths[i];
                }
            }

            filepaths = filepaths_sorted;

            #endregion

            #region for each imported SMD model part

            for (int i = 0; i < filepaths.Count; i++)
            {
                if(filepaths[i] == null)
                {
                    continue;
                }
                int model_type = -1;
                //Main.model.Model_Parts_header_List[i].vertex_block_offset
                // get the original vertex block size of model the part
                //int vert_size = BitConverter.ToInt32(Main.data_block, Main.model.Model_Parts_header_List[i].vertex_block_offset + 16); 

                int vert_size = Main.model.VertexBlock_header_List[i].vertex_def_size;

                string filename = Path.GetFileNameWithoutExtension(filepaths[i]);

                /*
                if (filename.Contains("_p_"))
                {
                    model_type = 0;
                }

                if (filename.Contains("_s_"))
                {
                    vert_size -= 8;
                    model_type = 1;
                }

                if (filename.Contains("_b_"))
                {
                    vert_size -= 8;
                    model_type = 2;
                }
                */

                // add model property editor
                add_mdl_properties_control(i, filepaths[i], model_type, vert_size);

                // find control 
                for (int x = 0; x <= panel_models.Controls.Count - 1; x++)
                {
                    Control ct = panel_models.Controls[x];
                    // set properties
                    if (ct is Model_Inspector && x == i)
                    {
                        mdl_inspector_list.Add((Model_Inspector)ct);
                        break;
                    }
                }
            }

            #endregion
        }

        private void add_mdl_properties_control(int model_num, string filepath, int model_type, int vertex_def_size)
        {
            Model_Inspector mp = new Model_Inspector();
            mp.Location = new Point(0, model_num * 30);
            mp.filepath = filepath;

            if (model_num % 2 == 0)
            {
                mp.BackColor = System.Drawing.Color.LightGray;
            }

            foreach (Control c in mp.Controls)
            {
                // set smd filename
                if (c.Name == "lab_smd_name")
                {
                    c.Text = Path.GetFileName(filepath);
                }

                // set model part type
                if (c.Name == "cb_mdl_part_type")
                {
                    ComboBox cb = (ComboBox)c;
                    cb.SelectedIndex = model_type;
                }

                // set model part type
                if (c.Name == "cb_vertex_def_size")
                {
                    ComboBox cb = (ComboBox)c;
                    cb.SelectedText = vertex_def_size.ToString();
                }
            }

            // add control
            panel_models.Controls.Add(mp);
        }

        public void remove_mat_inspector(int num)
        {
            for (int i = 0; i < mat_inspector_list.Count; i++)
            {
                if(i >= num +1)
                {
                    mat_inspector_list[i].Location = new Point(0, (i * 90) -90);
                    mat_inspector_list[i].id -= 1;
                    mat_inspector_list[i].set_mat_id();
                }
            }

            mat_inspector_list.RemoveAt(num);
        }

        private void load_materials()
        {
            /*
            // adds material based on the triangle groups (for this model part)
            // this method is wrong, should take all Main.model.materials_List instead, and write the material id (0 ,1, 2 ,3 etc) in the triangle group
            if (Main.model.Model_Parts_header_List[20].triangle_groups_List.Count > 0)
            {
               List<DataFormats.JSRF.MDLB.triangle_group> tg = Main.model.Model_Parts_header_List[20].triangle_groups_List;

                for (int i = 0; i < tg.Count; i++)
                {
                   add_material_inspector(Main.model.materials_List[tg[i].material_ID]);
                }
            }*/



            // load materials list from original model (last part)
            for (int i = 0; i < Main.model.materials_List.Count; i++)
            {
                add_material_inspector(Main.model.materials_List[i]);
            }

            // if model hast materials after triangle groups return;
            if (Main.model.materials_List.Count > 0) { return; }

            /*


            int vtx_mat_count = 0;
            // TODO
            if(Main.model.header.materials_count == 0)
            {
                List<DataFormats.JSRF.MDLB.material> mats = Main.model.Model_Parts_header_List[Main.model.Model_Parts_header_List.Count - 1].vtx_buffer_materials;

                foreach (var m in mats)
                {
                    vtx_mat_count++;
                    add_material_inspector(m);
                }

            }

            */

            //int vtx_mat_count = 0;
            // TODO
            if (Main.model.header.materials_count == 0)
            {
                List<DataFormats.JSRF.MDLB.material> mats = Main.model.Model_Parts_header_List[Main.model.Model_Parts_header_List.Count - 1].vtx_buffer_materials;

                foreach (var m in mats)
                {
                   // vtx_mat_count++;
                    add_material_inspector(m);
                }

            }

            // if (vtx_mat_count > 0) { return; }

            // TODO  if Main.model.materials_List.Count == 0
            // load materials after vtx_tri_headers

            #region load materials from SMD

            // else load materials list from SMD (last part)
            // if original model doesn't have a materials list after the triangles groups
            // materials are stored after vertex_triangles_buffers_header
            // so we get those original materials and try to find matching materials from the SMD mat_id to the original's

            //List<DataFormats.JSRF.MDLB.material> mat_list_ori = Main.model.Model_Parts_header_List[Main.model.Model_Parts_header_List.Count - 1].vtx_buffer_materials;
            return;
            SMD main_smd = new SMD(main_SMD_filepath);
            DataFormats.JSRF.MDLB.material mat = null;

            // for each material/triangles group in SMD (last part) file
            for (int i = 0; i < main_smd.mat_groups_list.Count; i++)
            {
                mat = null;

                // get material number (mat_x) for this material group
                int mat_num = 0;
                if (main_smd.mat_groups_list[i].material_name.Contains("mat_"))
                {
                    string[] args = main_smd.mat_groups_list[i].material_name.Split('_');
                    if(args.Length >1)
                    {
                        mat_num = Convert.ToInt32(args[1]);
                    }
                }

                /*
                // if material number inferior original model materials count
                // set material
                if(mat_num < mat_list_ori.Count)
                {
                    mat = mat_list_ori[mat_num];
                }
                */

                // if material not found 
                // add generic material
                if(mat == null)
                {
                    add_material_inspector();
                    continue;
                }

                // else add corresponding material by id
                add_material_inspector(mat);
            }

            #endregion
        }

        private void add_material_inspector(DataFormats.JSRF.MDLB.material mat = null)
        {
            Material_Inspector mat_inspector = new Material_Inspector();
            mat_inspector.Location = new Point(0, mat_inspector_list.Count * 90);
            Materials.color c = new Materials.color(255, 255, 255, 255);

      
            // add control
            panel_materials.Controls.Add(mat_inspector);

            // set properties values
            for (int x = 0; x <= panel_materials.Controls.Count - 1; x++)
            {
                Control ct = panel_materials.Controls[x];

                if (ct is Material_Inspector && x == mat_inspector_list.Count)
                {
                    ((Material_Inspector)ct).id = mat_inspector_list.Count;

                    // if no input material is defined, load values from last material inspector
                    if (mat == null)
                    {
                        Material_Inspector mi_last = mat_inspector_list[mat_inspector_list.Count - 1];
                        // set material inspector values taken from last material inspector
                       ((Material_Inspector)ct).set_values(mi_last.get_color(), mi_last.get_shader_type(), mi_last.get_unk_id(), mi_last.get_hb());

                    } else {
                        // set material inspector values from input Material (mat)
                        Materials.color color = new Materials.color(mat.color.R, mat.color.G, mat.color.B, mat.color.A);
                        ((Material_Inspector)ct).set_values(color, mat.shader_id, mat.unk_id2, mat.HB);
                    }

                 
                    mat_inspector_list.Add((Material_Inspector)ct);
                }
            }
        }



        #region DialogResult_events

        private void MDLB_import_options_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(this.DialogResult == DialogResult.OK)
            {
                return;
            }

            if(e.CloseReason == CloseReason.UserClosing)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void btn_import_Click(object sender, EventArgs e)
        {
            // store each model filepath and other info into list of ModelPart_Import_Settings
            List<ModelPart_Import_Settings> mdlPart_import_settings_List = new List<ModelPart_Import_Settings>();
            // for each model part?
            for (int i = 0; i < mdl_inspector_list.Count; i++)
            {
               Model_Inspector mdl = mdl_inspector_list[i];
               mdlPart_import_settings_List.Add(new ModelPart_Import_Settings(mdl.filepath, mdl.get_mdl_type(), mdl.get_vert_def_size(), 
                   Convert.ToSingle(txtb_drawDist_x.Text),
                   Convert.ToSingle(txtb_drawDist_y.Text),
                   Convert.ToSingle(txtb_drawDist_z.Text),
                   Convert.ToSingle(txtb_drawDist_w.Text)));
            }

            mdlPart_import_settings_List.Add(new ModelPart_Import_Settings(main_SMD_filepath, 0, Convert.ToInt32(cb_vertex_def_size.SelectedItem),
                   Convert.ToSingle(txtb_drawDist_x.Text),
                   Convert.ToSingle(txtb_drawDist_y.Text),
                   Convert.ToSingle(txtb_drawDist_z.Text),
                   Convert.ToSingle(txtb_drawDist_w.Text)));

            List<MDLB_builder.material> materials = new List<MDLB_builder.material>();
            // for each material inspector, create MDLB_builder.material and add to list
            for (int i = 0; i < mat_inspector_list.Count; i++)
            {
                Material_Inspector mi = mat_inspector_list[i];
                Materials.color c = mi.get_color();
                int shader_id = 0;
  
                #region convert material_id Name to id number

                string[] names = Enum.GetNames(typeof(Materials.materials));
                int[] values = (int[])Enum.GetValues(typeof(Materials.materials));

                // find corresponding material name and get shader id
                for (int m = 0; m < names.Length; m++)
                {
                    if (names[m] == mi.cb_material_id.Text)
                    {
                        shader_id = values[m];
                        break;
                    }
                }

                if(mi.cb_material_id.Text.All(char.IsDigit))
                {
                    shader_id = Convert.ToInt32(mi.cb_material_id.Text);
                }

                #endregion

                materials.Add(new MDLB_builder.material(new MDLB_builder.color(c.B, c.G, c.R, c.A), shader_id, Convert.ToInt32(mi.txtb_unk_id2.Text), Convert.ToSingle(mi.txtb_hb.Text)));
            }

            /*
            List<MDLB_builder.model_part_triangle_groups> tris_groups = new List<MDLB_builder.model_part_triangle_groups>();

            for (int i = 0; i < Main.model.Model_Parts_header_List.Count; i++)
            {
                tris_groups.Add(new MDLB_builder.model_part_triangle_groups());

            
                if (Main.model.Model_Parts_header_List[i].triangle_groups_count > 1)
                {
                    tris_groups[i] = new MDLB_builder.model_part_triangle_groups();
                    for (int t = 0; t < Main.model.Model_Parts_header_List[i].triangle_groups_List.Count; t++)
                    {
                        tris_groups[i].tri_groups.Add(Main.model.Model_Parts_header_List[i].triangle_groups_List[t]); //Main.model.Model_Parts_header_List[i].triangle_groups_List[t]
                    }
                }
            }
            */

            MDLB_builder MDLB_build = new MDLB_builder();
            MDLB_import = MDLB_build.build_MDLB(mdlPart_import_settings_List, materials);

            this.DialogResult = DialogResult.OK;
           // this.Close();
        }
        /*
        public class model_part_triangle_groups
        {
            //public int model_part_number { get; set; }
            public List<JSRF_Tool_2.DataFormats.JSRF.MDLB.triangle_group> tri_groups { get; set;}
        }
        */
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        private void btn_add_material_Click(object sender, EventArgs e)
        {
            add_material_inspector();
        }

        private void cb_vertex_def_size_SelectedValueChanged(object sender, EventArgs e)
        {
            foreach (var mdl in mdl_inspector_list)
            {
                mdl.set_vert_def_size(Convert.ToInt32(cb_vertex_def_size.Text));
            }
        }

        private void txtb_numeric_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
            && !char.IsDigit(e.KeyChar)
            && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }
    }
}
