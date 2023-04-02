using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Media;
using System.Threading;

using JSRF_ModTool.DataFormats;
using JSRF_ModTool.DataFormats.JSRF; 
using JSRF_ModTool.Vector;
using JSRF_ModTool.Functions;


using HelixToolkit.Wpf;
using Microsoft.Win32;


namespace JSRF_ModTool
{
    public partial class Main : Form
    {
        #region Declarations

        public static string startup_dir; //= Application.StartupPath;
        private string tmp_dir; //= Application.StartupPath + "\\resources\\tmp\\";
        string settings_dir; //= Application.StartupPath + "/resources/";

        FileExplorer fe = new FileExplorer();

        /// Dynamic parser: pr.binary_to_struct() method automatically parses binary files and loads the data to properties in an instance of a class/struct
        Parsing pr = new Parsing();

        private List<DataFormats.JSRF.Material> materials_dat_list = new List<DataFormats.JSRF.Material>();
        private List<DataFormats.JSRF.Material> materials_bin_list = new List<DataFormats.JSRF.Material>();
        private List<string> textures_id_list = new List<string>();

        public File_Containers jsrf_file = null;


        public string container_type = "null";


        public static DataFormats.JSRF.MDLB current_model;
        public static byte[] current_item_data; // latest loaded item/block

        // store copy/paste item block data here
        private byte[] block_copy_clipboard;
        private List<File_Containers.item> node_copy_clipboard;

        // used to know if the model has already been loaded, so the model part count on the model viewer is properly updated
        bool mdlb_first_load = false;

        private static Process proc_ImgEditor;

        private List<string> settings = new List<string>();
        private List<string> settings_dirs = new List<string>();

        private string current_filepath = "";

        private TreeNode current_node;
        private TreeNode last_selected_node;

        #endregion

        public Main()
        {
            InitializeComponent();

           startup_dir = GetShortPath(Application.StartupPath);
           tmp_dir = startup_dir + "\\resources\\tmp\\";
           settings_dir = startup_dir + "\\resources\\";

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region setup
            Settings_load();

            // set file explorer directory
            fe.set_dir(txtb_jsrf_mod_dir.Text);
            fe.CreateTree(this.trv_explorer);

            // clear temporary directory
            Functions.IO.DeleteDirectoryContent(tmp_dir);

            if (cb_show_adv_mdlb_nfo.Checked)
            {
                panel_adv_mdl_nfo.Visible = true;
                panel_lvl_mdl_info.Visible = true;
            }

            #endregion

            if (!Directory.Exists(tmp_dir)) { Directory.CreateDirectory(tmp_dir); }

#if DEBUG
            btn_fix_drawdist.Visible = true;
            panel_lvl_mdl_info.Visible = true;
            label5.Visible = true;

            //Load_file(txtb_jsrf_mod_dir.Text + @"\People\People01.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1].Nodes[0];

            //Load_file(txtb_jsrf_mod_dir.Text + @"\Player\Gum.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1].Nodes[0]; // 0 8 0 for texture

            //Load_file(txtb_jsrf_mod_dir.Text + @"\Event\Event\e291.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[9].Nodes[0];


            //Load_file(txtb_jsrf_mod_dir.Text + @"\Stage\stg00_00.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[4];

            Load_file(txtb_jsrf_mod_dir.Text + @"\Sounds\SE\pv_beat.dat");
            trv_file.SelectedNode = trv_file.Nodes[0].Nodes[0];


            // load stg00_.bin
            //DataFormats.JSRF.Stage_Bin.Parser stgBin_00 = new DataFormats.JSRF.Stage_Bin.Parser(txtb_jsrf_mod_dir.Text + "\\Stage\\stg00_.bin");

            #region loading methods



            //JSRF_ModTool.DataFormats.JSRF.Stage_Bin.Parser stageBinParser = new JSRF_ModTool.DataFormats.JSRF.Stage_Bin.Parser(@"C:\Users\Mike\Desktop\JSRF\game_files\Media\Stage\Stg23_.bin"); //13

            /*
            // extract all Stage collision meshes (from JSRF original dir)
            foreach (string file in Directory.EnumerateFiles(txtb_jsrf_original_dir.Text + "\\Stage\\", "*.bin", SearchOption.AllDirectories))
            {   
                if (file.Contains("_t.bin")) { continue; } // skip skybox bin file
                if (file.Contains("\\Custom\\")) { continue; } // skip Custom levels

                JSRF_ModTool.DataFormats.JSRF.Stage_Bin.Parser stageBinParser = new JSRF_ModTool.DataFormats.JSRF.Stage_Bin.Parser(file);
            }
            */

            //Load_file(txtb_jsrf_mod_dir.Text + @"\Player\Bis.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1].Nodes[0];

            /*
            Stage_Compiler Stage_compiler = new Stage_Compiler();
            // arguments: export_dir, media_dir, stage_num
            Stage_compiler.Compile(@"C:\Users\Mike\Desktop\JSRF\Stg_Compiles\Stg_demo\", @"C:\Users\Mike\Desktop\JSRF\game_files\ModOR\", "stg00");
            */




            /*
            Stage_Compiler Stage_compiler = new Stage_Compiler();
            Stage_compiler.Compile(txtb_vis_mdls_dir.Text + "\\", txtb_coll_mdls_dir.Text + "\\", txtb_grindPaths_path.Text, txtb_jsrf_mod_dir.Text + "\\", txtb_stage_num.Text);
            System.Media.SystemSounds.Asterisk.Play();
            Application.Exit();
            */


            // scans every StageXX_YY.dat to get the number of models/textures/triangles and get the average
            //Stage_Data_Metrics stg_stats = new Stage_Data_Metrics(@"C:\Users\Mike\Desktop\JSRF\game_files\ModOR\Stage\");


            //Mission_dat msn = new Mission_dat(@"C:\Users\Mike\Desktop\JSRF\game_files\ModOR\Mission\mssn0101.dat");


            /*

            // Export Stage models

            Load_file(txtb_jsrf_mod_dir.Text + "\\" + @"Stage\stg52_00.dat");
            for (int i = 0; i < trv_file.Nodes[0].Nodes.Count; i++)
            {
                // get item from 'jsrf_file'
                File_Containers.item item = jsrf_file.get_item(0, i);

                if (item.type == File_Containers.item_data_type.Stage_Model)
                {
                    Stage_Model mdl = new Stage_Model(item.data);

                    //if(mdl.vtx_tri_buff_head.is_stripped == 0)
                    // {
                    mdl.export_model(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\stg52\Stage_Model_" + i + ".obj");
                    // }
                }
            }
            */


            // load stg00_.bin
            // DataFormats.JSRF.Stage_Bin.Parser stgBin_00 = new DataFormats.JSRF.Stage_Bin.Parser(txtb_jsrf_mod_dir.Text + "\\Stage\\stg00_.bin");

            #endregion

            #region other file loading methods

            /*
            // loop through indexed file node items
            for (int i = 0; i < jsrf_file.INDX_root.items.Count; i++)
            {
                File_Containers.item item = jsrf_file.INDX_root.items[i];

                // if it's a stage model
                if(item.type != File_Containers.item_data_type.Stage_Model)
                {
                    continue;
                }

                Stage_Model mdl = new Stage_Model(item.data);
            }
            */


            //DataFormats._3D_Model_Formats.OBJ obj = new DataFormats._3D_Model_Formats.OBJ(@"C:\Users\Mike\Documents\XSI_Projects\JSRF\Models_export\test.obj");


            //mission_dat msndat = new mission_dat(@"C:\Users\Mike\Desktop\JSRF\game_files\files\ModOR\Mission\mssn0965.dat");


            //Load_file(game_dir + @"Event\Event\e291.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[9].Nodes[0];


            //Load_file(game_dir + @"Stage\stg00_00.dat", false);
            /*
            Stage_Model_Compiler lvlmdl_comp = new Stage_Model_Compiler();
            byte[] item_data = lvlmdl_comp.build(@"C:\Users\Mike\Documents\XSI_Projects\JSRF\Models_export\test.obj");
            if(item_data.Length > 0)
            {
                jsrf_file.INDX_root.items[20].data = item_data;
                jsrf_file.rebuild_file(jsrf_file.filepath);
            }
            // Application.Exit();
            */

            /*
            //Load_file(game_dir + @"Stage\stg33_01.dat"); // stg22_01 - 0
            Load_file(game_dir + @"Stage\stg00_00.dat", false);
            File_Containers.item item = jsrf_file.get_item(0, 20); 
            Stage_Model mdl = new Stage_Model(item.data);
            mdl.export_model(@"C:\Users\Mike\Documents\XSI_Projects\JSRF\Models_export\model.obj");
            Application.Exit();
            */


            //mass_search_texture_by_ID("855694150");


            /*
            Load_file(game_dir + @"Stage\stg00_00.dat");
            for (int i = 0; i < trv_file.Nodes[0].Nodes.Count; i++)
            {
                // get item from 'jsrf_file'
                File_Containers.item item = jsrf_file.get_item(0, i);

                if(item.type == File_Containers.item_data_type.Stage_Model)
                {
                    Stage_Model mdl = new Stage_Model(item.data);

                    //if(mdl.vtx_tri_buff_head.is_stripped == 0)
                    // {
                        mdl.export_model(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\export\Stage_Model_" + i + ".obj");
                    // }

                }
            }
            Application.Exit();
            */

            #endregion

#endif
        }


        #region Load file

        /// <summary>
        /// loads JSRF file into corresponding file structure type and returns the load file as a class object
        /// </summary>
        public object Load_file(string filepath, bool load_bin_materials = false)
        {
            Reset_vars();

            jsrf_file = new File_Containers(filepath);

            if (filepath.Contains(".dat") && load_bin_materials)
            {
                string filepath_bin = filepath.Replace(".dat", ".bin");
                // get corresponding .bin file and load materials list from it
                if (File.Exists(filepath_bin))
                {
                    // load bin materials into list
                    materials_bin_list = Get_materials_to_list(new File_Containers(filepath_bin));
                }
            }

            Populate_file_treeview(jsrf_file);

            current_filepath = filepath;
            lab_filename.Text = Path.GetFileName(filepath);

            return jsrf_file;
        }



        // load stage model
        private void load_stage_model(byte[] data)
        {
            Stage_Model mdl = new Stage_Model(data);

            panel_lvl_mdl_info.Visible = true;
            panel_adv_mdl_nfo.Visible = false;
            ///mdl.export_model(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\export\Stage_Model_0.obj");
            // setup labels giving model info
            lab_lvlmdl_tex_ids.Text = mdl.texture_ids.Count.ToString();
            lab_lvlmdl_mat_count.Text = mdl.header.x124_mat_count.ToString();
            lab_lvlmdl_triGroup_count.Text = mdl.header.x132_mat_groups_count.ToString();
            lab_lvlmdl_vtx_def_size.Text = mdl.vtx_tri_buff_head.vertex_def_size.ToString();
            lab_lvlmdl_vtx_flag.Text = mdl.vtx_tri_buff_head.vertex_struct.ToString();
            lab_lvlmdl_tri_count.Text = ( mdl.vtx_tri_buff_head.triangle_buffer_size / 3).ToString();
            lab_lvlmdl_tri_stripped.Text = mdl.vtx_tri_buff_head.is_stripped.ToString();
            lab_lvlmdl_drawdist.Text = mdl.header.model_radius.ToString();

            // setup stage model for model viewer
            MeshData mesh_data = new MeshData();

            for (int i = 0; i < mdl.triangles_list.Count; i++)
            {
                mesh_data.triangles.Add(mdl.triangles_list[i].a - 1);
                mesh_data.triangles.Add(mdl.triangles_list[i].b - 1);
                mesh_data.triangles.Add(mdl.triangles_list[i].c - 1);
            }

            #region calculate mesh center, bounds, average distance

            bounds bounds = new bounds();

            for (int i = 0; i < mdl.vertices_list.Count; i++)
            {
                Vector3 vert = mdl.vertices_list[i];

                mesh_data.vertices.Add(new Point3D(vert.X*-1, vert.Z, vert.Y));
                bounds.add_point(new Vector3(vert.X * -1, vert.Z, vert.Y));
            }
           
            mesh_data.mesh_center = new Point3D(bounds.center.X, bounds.center.Y, bounds.center.Z);
            mesh_data.avg_distance = new Point3D(Math.Abs((Math.Abs(bounds.Xmin) + bounds.Xmax)), Math.Abs((Math.Abs(bounds.Ymin) + bounds.Ymax)), Math.Abs((Math.Abs(bounds.Zmin) + bounds.Zmax)));
            mesh_data.mesh_bounds = new Point3D(bounds.Xmin + bounds.Xmax, bounds.Ymin + bounds.Xmax, bounds.Zmin + bounds.Zmax);

            #endregion

            #region load full mesh with one material (no clusters nor splititng the mesh)          

            List<GeometryModel3D>  meshes = new List<GeometryModel3D>();

            // model color
            System.Windows.Media.Color clr = System.Windows.Media.Color.FromRgb(128, 128, 128);

            meshes.Add(Get_WPF_model(mesh_data, clr));
            Load_model_to_HelixModelViewer(meshes, mesh_data.mesh_center, mesh_data.mesh_bounds, mesh_data.avg_distance);

            #endregion
        }

        #endregion

        #region Form Events

        // folder explorer event
        private void Trv_explorer_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (File.Exists(e.Node.FullPath.ToString()))
            {
                Load_file(e.Node.FullPath.ToString(), true);
            }
        }

        private void Trv_explorer_BeforeExpand_1(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes[0].Text == "")
            {
                TreeNode node = fe.EnumerateDirectory(e.Node);
            }

        }

        private void Trv_explorer_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes[0].Text == "")
            {
                TreeNode node = fe.EnumerateDirectory(e.Node);
            }

        }

        // select JSRF folder
        private void Btn_favfolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                txtb_jsrf_mod_dir.Text = path;
            }

            trv_explorer.Nodes.Clear();

            fe.set_dir(txtb_jsrf_mod_dir.Text);

            fe.CreateTree(this.trv_explorer);

            // clear temporary directory
            Functions.IO.DeleteDirectoryContent(tmp_dir);

            if (!Directory.Exists(tmp_dir))
                Directory.CreateDirectory(tmp_dir);
        }

        // select original jsrf files directory
        private void Btn_jsrf_original_dir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtb_jsrf_original_dir.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        // select cxbx folder
        private void btn_sel_cxbx_dir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtb_cxbx_dir.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        // expand treeview child items
        private void Btn_expand_tree_Click(object sender, EventArgs e)
        {
            if (btn_expand_tree.Text == "+")
            {
                trv_file.ExpandAll();
                btn_expand_tree.Text = "-";
            }
            else
            {
                trv_file.CollapseAll();

                if (trv_file.Nodes.Count > 0)
                {
                    trv_file.Nodes[0].Expand();
                }

                btn_expand_tree.Text = "+";
            }
        }



        // send file to xbox through FTP
        private void Btn_ftp_to_xbox_Click(object sender, EventArgs e)
        {
            // return if no JSRF file is currently loaded in the JSRF tool
            if (current_filepath == "") { MessageBox.Show("No file loaded, select a file."); return; }
            // return if one or mutiple FTP settings are undefined
            if ((txtb_xbox_ip.Text == "") || (txtb_xbox_login.Text == "") || (txtb_xbox_pass.Text == "")) { MessageBox.Show("Please make sure the Xbox FTP IP, login and password are defined in the settings tab."); return; }


            // create ftp client with xbox login/password defined in Settings tab
            ftp ftpClient = new ftp(@"ftp://" + txtb_xbox_ip.Text + "/", txtb_xbox_login.Text, txtb_xbox_pass.Text);

            string subFolfder = "";
            int split_dir_at = 0;
            string[] folders = current_filepath.Split(Path.DirectorySeparatorChar);

            // find the "Media" folder in path and substract the parent folders from it
            for (int i = 0; i < folders.Length; i++) { if (folders[i] == "Media") { split_dir_at = i; } }
            for (int i = split_dir_at + 1; i < folders.Length; i++) { subFolfder = subFolfder + "\\" + folders[i]; }

            // bool uploaded = ftpClient.upload((txtb_xbox_jsrf_dir.Text.TrimEnd(Path.DirectorySeparatorChar) + subFolfder).Replace("\\", "/"), @current_filepath); 

            bool uploaded = ftpClient.upload((txtb_xbox_jsrf_dir.Text.TrimEnd(Path.DirectorySeparatorChar) + subFolfder).Replace("\\", "/"), @current_filepath);

            // only try to delete cache if uploaded succeded (in case if it didn't, we avoid FTP error spamming the user :) )
            if (uploaded)
                // delete file from cache (so the game will reload the new modded file we just uploaded)
            ftpClient.delete("X:/Media" + subFolfder.Replace("\\", "/"));
            ftpClient.delete("Y:/Media" + subFolfder.Replace("\\", "/"));
            ftpClient.delete("Z:/Media" + subFolfder.Replace("\\", "/"));


            System.Media.SystemSounds.Beep.Play();
        }

        // revert to riginal file (unmodded)
        private void Btn_restore_original_file_Click(object sender, EventArgs e)
        {
            if (current_filepath == "") { MessageBox.Show("No file loaded, select a file."); return; }
            if (txtb_jsrf_original_dir.Text == "") { MessageBox.Show("Error: \"JSRF folder: original files\" in Settings is not defined, please select the folder."); return; }
            if (!Directory.Exists(txtb_jsrf_original_dir.Text)) { MessageBox.Show("Error: \"JSRF folder: original files\" in Settings does not exist."); return; }


            string subolfder = "";
            int split_dir_at = 0;

            string[] folders = current_filepath.Split(Path.DirectorySeparatorChar);

            // find the "Media" folder in path and substract the parent folders from it
            for (int i = 0; i < folders.Length; i++) { if (folders[i] == "Media" || folders[i] == "ModOR") { split_dir_at = i; } }
            for (int i = split_dir_at + 1; i < folders.Length; i++) { subolfder = subolfder + "\\" + folders[i]; }
            string original_filepath = txtb_jsrf_original_dir.Text.TrimEnd(Path.DirectorySeparatorChar) + subolfder;

            if (!File.Exists(original_filepath)) { MessageBox.Show("Error: original file (" + original_filepath + ") does not exist."); return; }

            // replace the modded file by a copy of the original
            File.Copy(original_filepath, current_filepath, true);


            TreeNode selected_node = trv_file.SelectedNode;

            // reload file
            Load_file(current_filepath, true);

            // select the node that was select before resetting the file to its original state
            if(selected_node != null)
            {
                if (selected_node.Level == 0)
                {
                    trv_file.SelectedNode = trv_file.Nodes[selected_node.Index];
                }

                if (selected_node.Level == 1)
                {
                    trv_file.SelectedNode = trv_file.Nodes[selected_node.Parent.Index].Nodes[selected_node.Index];
                }

                if (selected_node.Level == 2)
                {
                    trv_file.SelectedNode = trv_file.Nodes[selected_node.Parent.Parent.Index].Nodes[selected_node.Parent.Index].Nodes[selected_node.Index];
                }
            }

            clear_cxbx_cache();

            System.Media.SystemSounds.Beep.Play();
        }

        // nulls out model data (developed this to remove the game HUD to take screenshots)
        private void Btn_null_model_Click(object sender, EventArgs e)
        {
            // check selected item is valid
            if (!selected_MDLB_is_valid())
            {
                return;
            }

            byte[] data = Collapse_model_vertices(current_item_data);


            jsrf_file.set_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, data);

            // rewrite array to disk file
            jsrf_file.rebuild_file(jsrf_file.filepath);

            clear_cxbx_cache();

            // reselect node to reload MDLB
            TreeNode tn = trv_file.SelectedNode;
            trv_file.SelectedNode = null;
            trv_file.SelectedNode = tn;

        }

        
        // import model
        private void Btn_import_mdl_Click(object sender, EventArgs e)
        {

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            // check selected item is valid
            if (!selected_MDLB_is_valid())
            {
                return;
            }

            #region select SMD file dialog

            string filepath = String.Empty;
            //System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "dat files (*.smd)|*.smd";
            //dialog_impSMD.RestoreDirectory = true;
            dialog.Title = "Select an SMD file";

            // restore previous saved directory
            string import_dir = Setting_load("smd_import_dir");
            if (Directory.Exists(import_dir)) { dialog.InitialDirectory = import_dir; }
           

            //dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filepath = dialog.FileName;
            }
            else
            {
                return;
            }

            #endregion


            Setting_save(Path.GetDirectoryName(filepath) + "\\", "smd_import_dir");

            byte[] imported_MDLB_data = new byte[0];

            #region import Form with "using"

            // setup import form // "using {}" is employed here because the form and its variables (such as the heavy data SMD-model)
            // will be disposed/cleared of once we exit the 'using' {} space
            using (var MDLB_Import_Form = new MDLB_Import.MDLB_import_options_Form())
            {
                //MDLB_Import.MDLB_import_options_Form import_Form = new MDLB_Import.MDLB_import_options_Form();
                MDLB_Import_Form.main_SMD_filepath = filepath;
                

                DialogResult dr = new DialogResult();
                dr = MDLB_Import_Form.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    imported_MDLB_data = MDLB_Import_Form.MDLB_import;
                    MDLB_Import_Form.Close();
                }
                else if (dr == DialogResult.Cancel)
                {
                    // f.SMD_model = null;
                    return;
                }
            }

            #endregion


            if ((imported_MDLB_data == null) || (imported_MDLB_data.Length == 0))
            {
                MessageBox.Show("Error could not import model");
                return;
            }

            jsrf_file.set_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, imported_MDLB_data);
            Rebuild_file(true, true, true);

        }

        private void cb_show_adv_mdlb_nfo_CheckStateChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                panel_adv_mdl_nfo.Visible = true;
            }
            else
            {
                panel_adv_mdl_nfo.Visible = false;
            }
        }

        // save material data
        private void btn_save_mat_data_Click_1(object sender, EventArgs e)
        {

             Save_block_Material();
        }

        private void btn_fix_drawdist_Click(object sender, EventArgs e)
        {
            // check selected item is valid
            if (!selected_MDLB_is_valid())
            {
                return;
            }

            byte[] data = fix_mdl_draw_distance(current_item_data);

            //byte[] data = nullify_model(data_block, 21);

            jsrf_file.set_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, data);

            // rewrite array to disk file
            jsrf_file.rebuild_file(jsrf_file.filepath);

            clear_cxbx_cache();

            // reselect node to reload MDLB
            TreeNode tn = trv_file.SelectedNode;
            trv_file.SelectedNode = null;
            trv_file.SelectedNode = tn;
        }

        private void txtb_img_editor_path_TextChanged(object sender, EventArgs e)
        {
            proc_ImgEditor = null; // reset image editor process
            Settings_save(sender, e);
        }

        // select image editor file path (i.e. photoshop.exe)
        private void Btn_img_editor_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string path = openFileDialog1.FileName;
                txtb_img_editor_path.Text = path;
            }
        }

       


        private void btn_sel_vis_mdl_dir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                txtb_stage_source_dir.Text = path;
            }
        }


        // compile stage button event
        private void button4_Click(object sender, EventArgs e)
        {
            Stage_Compiler Stage_compiler = new Stage_Compiler();
            Stage_compiler.Compile(Path.GetFullPath(txtb_stage_source_dir.Text + "\\"), Path.GetFullPath(txtb_jsrf_mod_dir.Text + "\\"), txtb_stage_num.Text);

            clear_cxbx_cache();

            System.Media.SystemSounds.Asterisk.Play();
        }


        #endregion

        #region settings

        // yeah back when I started learning C# I had no idea VS had built-in Settings to store settings
        // so I made this, feel free to change it if you feel Properties>Settings works better for you
        // this automatically loads all settings by calling settings_load()  (dynamically loads setting values into form controls)

        private void Settings_save(object sender, EventArgs e)
        {
            try
            {
                // Properties ob =  (Object)sender;
                string obj_name = "";
                string arg = "";

           

                if (sender.GetType() == typeof(CheckBox))
                {
                    CheckBox obj = (CheckBox)sender;
                    obj_name = obj.Name.ToString();
                    arg = obj.Checked.ToString();
                }

                if (sender.GetType() == typeof(TextBox))
                {
                    TextBox obj = (TextBox)sender;
                    obj_name = obj.Name.ToString();
                    arg = obj.Text.ToString();
                }

                if (sender.GetType() == typeof(ComboBox))
                {
                    ComboBox obj = (ComboBox)sender;
                    obj_name = obj.Name.ToString();
                    arg = obj.SelectedIndex.ToString();

                }


                if (sender.GetType() == typeof(Panel))
                {
                    Panel obj = (Panel)sender;
                    obj_name = obj.Name.ToString();
                    arg = obj.BackColor.R + ":" + obj.BackColor.G + ":" + obj.BackColor.B;
                }

                TextWriter tw = new StreamWriter(settings_dir + "settings.ini");

                Boolean set_found = false;
                for (int i = 0; i < settings.Count; i++)
                {
                    if (settings[i].Split('<')[1] == obj_name)
                    {
                        settings[i] = arg + "<" + obj_name;
                        set_found = true;
                    }
                    tw.WriteLine(settings[i]);
                }

                if ((settings.Count == 0) || (!set_found) && (arg != "") && (obj_name != ""))
                {
                    settings.Add(arg + "<" + obj_name);
                    tw.WriteLine(arg + "<" + obj_name);
                }


                tw.Close();

            }

            catch (System.Exception excep)
            {
                MessageBox.Show("Error saving settings " + excep.Message);
            }

        }

        private void Settings_load()
        {
            settings.Clear();

            try
            {
                if (File.Exists(settings_dir + "settings.ini"))
                {

                    string f = settings_dir + "settings.ini";
                    List<string> lines = new List<string>();

                    using (StreamReader r = new StreamReader(f))
                    {

                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }


                    foreach (string s in lines)
                    {
                        string[] arg = s.Split('<');
                        settings.Add(s);

                        Object prop =null;

                        try
                        {
                            prop = this.Controls.Find(arg[1], true)[0];
                        } catch
                        {

                        }

                        // if setting is not a control
                        if (prop == null) { continue; }
       
                        if (prop.GetType() == typeof(CheckBox))
                        {
                            CheckBox obj = (CheckBox)prop;
                            if (arg[0].ToLower() == "true") { obj.Checked = true; }
                            if (arg[0].ToLower() == "false") { obj.Checked = false; }
                        }

                        if (prop.GetType() == typeof(TextBox))
                        {
                            TextBox obj = (TextBox)prop;
                            obj.Text = arg[0];
                        }

                        if (prop.GetType() == typeof(ComboBox))
                        {
                            ComboBox obj = (ComboBox)prop;
                            obj.SelectedIndex = Convert.ToInt32(arg[0]);
                        } 
                    }
                }
            }

            catch (System.Exception excep)
            {
                // MessageBox.Show("Error saving settings " + excep.Message);
            }
        }


        private void Setting_save(string arg, string obj_name)
        {
            if (!File.Exists(settings_dir + "settings_dirs.ini")) { File.Create(settings_dir + "settings_dirs.ini"); }

            List<string> lines = new List<string>();
            try
            {
               lines = File.ReadAllLines(settings_dir + "settings_dirs.ini").ToList();
            }
            catch (System.Exception excep)
            {
                MessageBox.Show("Error saving settings " + excep.Message);
            }

            try
            {

                bool found = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    string[] args = lines[i].Split('<');
                    if (args[1] == obj_name)
                    {
                        lines[i] = arg + "<" + obj_name;
                        found = true;
                    }
                }

                if(!found)
                {
                    lines.Add(arg + "<" + obj_name);
                }

                File.WriteAllLines(settings_dir + "settings_dirs.ini", lines);
            }
            catch (System.Exception excep)
            {
                MessageBox.Show("Error saving settings " + excep.Message);
            }


        }

        private string Setting_load(string obj_name)
        {
            try
            {
                if (!File.Exists(settings_dir + "settings_dirs.ini")) { return ""; }

                string f = settings_dir + "settings_dirs.ini";
                //List<string> lines = new List<string>();

                using (StreamReader r = new StreamReader(f))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        string[] arg = line.Split('<');
                        if (arg[1] == obj_name)
                        {
                            return arg[0];
                        }
                    }
                }
            }

            catch (System.Exception excep)
            {
                MessageBox.Show("Error saving settings " + excep.Message);
                return "";
            }

            return "";
        }

        #endregion


        #region TreeView


        // load MULT / NORM class object into the treeview
        private void Populate_file_treeview(File_Containers file)
        {
            #region MULT
            if (file.type == File_Containers.container_types.MULT)
            {
                // cast object as MULT class
                //JSRF_Container.MULT_list MULT = (JSRF_Container.MULT_list)file;


                // Main node
                TreeNode root = new TreeNode(file.type.ToString() + " [" + (file.MULT_root.items.Count) + "]");
                trv_file.Nodes.Add(root);


                // Children
                for (int m = 0; m < file.MULT_root.items.Count; m++)
                {

                    TreeNode nNORM = new TreeNode("NORM" + " [" + (file.MULT_root.items[m].items.Count) + "]");
                    // add NORM node to treview
                    root.Nodes.Add(nNORM);

                    List<TreeNode> childNodes = new List<TreeNode>();

                    bool all_child_empty = true;
                    // loop through NORMs child items
                    for (int n = 0; n < file.MULT_root.items[m].items.Count; n++)
                    {
                        File_Containers.item item = file.MULT_root.items[m].items[n];

                        if (item.type == File_Containers.item_data_type.MDLB) { root.Nodes[m].BackColor = System.Drawing.Color.PaleVioletRed; }
                        if (item.type == File_Containers.item_data_type.Texture)
                        {
                            // set parent node color
                            root.Nodes[m].BackColor = System.Drawing.Color.SandyBrown;
                            // get texture id and add it to the list (so when loading materials we can check if an ID corresponds to a texture id)
                            textures_id_list.Add((BitConverter.ToInt32(item.data, 0)).ToString());
                        }

                        // if its a material we store it in the materials list
                        if (item.type == File_Containers.item_data_type.Material)
                        {
                            root.Nodes[m].BackColor = System.Drawing.Color.Linen;
                            // file_has_materials = true; 
                            materials_dat_list.Add(new DataFormats.JSRF.Material(item.data));
                        }

                        TreeNode nChild = new TreeNode(String.Format("{0,4} {1,-14}", n.ToString(), "   " + item.type.ToString()));

                        if (item.data.Length == 0)
                        {
                            nChild = new TreeNode(String.Format("empty"));
                        }
                        else
                        {
                            all_child_empty = false;
                        }

                        // add item
                        childNodes.Add(nChild);
                    }

                    // if at least one of the child nodes is not empty add items
                    if (!all_child_empty)
                    {
                        nNORM.Nodes.AddRange(childNodes.ToArray());
                    }
                }

                root.Expand();
            }

            #endregion

            #region NORM
            if (file.type == File_Containers.container_types.NORM)
            {
                // Main node
                TreeNode root = new TreeNode(file.type.ToString() + " [" + (file.NORM_root.items.Count) + "]");
                trv_file.Nodes.Add(root);

                List<TreeNode> childNodes = new List<TreeNode>();

                for (int n = 0; n < file.NORM_root.items.Count; n++)
                {
                    File_Containers.item item = file.NORM_root.items[n];

                    if (item.type == File_Containers.item_data_type.MDLB) { root.BackColor = System.Drawing.Color.PaleVioletRed; }
                    if (item.type == File_Containers.item_data_type.Texture)
                    {
                        // set parent node color
                        root.BackColor = System.Drawing.Color.SandyBrown;
                        // get texture id and add it to the list (so when loading materials we can check if an ID corresponds to a texture id)
                        textures_id_list.Add((BitConverter.ToInt32(item.data, 0)).ToString());
                    }

                    // if its a material we store it in the materials list
                    if (item.type == File_Containers.item_data_type.Material)
                    {
                        root.BackColor = System.Drawing.Color.Linen;
                        // file_has_materials = true; 
                        materials_dat_list.Add(new DataFormats.JSRF.Material(item.data));
                    }

                    TreeNode nChild = new TreeNode(String.Format("{0,4} {1,-14}", n.ToString(), "   " + item.type.ToString()));

                    if (item.data.Length == 0)
                    {
                        nChild = new TreeNode(String.Format("empty"));
                    }

                    // add item
                    childNodes.Add(nChild);
                }

                root.Nodes.AddRange(childNodes.ToArray());
                root.Expand();
            }

            #endregion

            #region Indexed

            if (file.type == File_Containers.container_types.indexed)
            {
                TreeNode root = new TreeNode(file.type.ToString() + " [" + (file.INDX_root.items.Count) + "]");
                trv_file.Nodes.Add(root);

                List<TreeNode> childNodes = new List<TreeNode>();

                // for each item in file.INDX_root
                for (int m = 0; m < file.INDX_root.items.Count; m++)
                {
                    File_Containers.item item = file.INDX_root.items[m];
                    
                    TreeNode nChild = new TreeNode(String.Format("{0,4}",   item.type.ToString()));
                    nChild.Text = String.Format("{0,4} {1,-14}", m.ToString(), "   " + item.type.ToString());

                    if (item.data.Length == 0)
                    {
                        nChild = new TreeNode(String.Format("empty"));
                    }
                   
                    childNodes.Add(nChild);
                }

                root.Nodes.AddRange(childNodes.ToArray());
                root.Expand();
            }
            
            #endregion
        }

        // when node is selected
        private void Trv_file_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Load_TreeView_item(e.Node);
            current_node = e.Node; //e.Node
            last_selected_node = e.Node; //e.Node
        }

        // load treeview item/data
        private void Load_TreeView_item(TreeNode node)
        {
            panel_mat_editor.Enabled = false; // clear material editor
            rtxtb_materials.Clear();
            pictureBox_texture_editor.Enabled = false; // clear texture editor
            elementHost_model_editor.Enabled = false; // clear model editor


            if (node == null) { MessageBox.Show("Error: node is null."); return; }
            if (node.Nodes == null) { MessageBox.Show("Error: node.Nodes is null."); return; }
            if (node.Nodes.Count > 0) { return; } // if node is an empty container, ignore
            if (node.Text.Contains("NORM") || node.Text.Contains("MULT")) { return; } // return if node is a container

            // get item from 'jsrf_file'
            File_Containers.item item = jsrf_file.get_item(node.Parent.Index, node.Index);

            // if item is empty
            if (item.data.Length == 0)
            {
                if (item.type != File_Containers.item_data_type.empty && item.type != File_Containers.item_data_type.unknown)
                {
                    MessageBox.Show("Item data is empty.");
                    return;
                }
            }

            // set label defining item's size in bytes
            lab_itemSel_length.Text = "(" + (item.data.Length.ToString("# ##0") + " bytes").PadRight(10) + ")";

            // cope item.data to global variable
            current_item_data = item.data;

            switch (item.type)
            {
                // Texture
                case File_Containers.item_data_type.Texture:
                    pictureBox_texture_editor.Image = null;
                    Load_block_Texture(current_item_data, false, false);
                    pictureBox_texture_editor.Enabled = true;
                    tabControl1.SelectedIndex = 1;
                    break;

                // MDLB Model
                case File_Containers.item_data_type.MDLB:
                    mdlb_first_load = true;
                    Load_block_MDLB(current_item_data, 21);
                    elementHost_model_editor.Enabled = true;
                    tabControl1.SelectedIndex = 0;
                    break;

                // Stage MDLB
                case File_Containers.item_data_type.Stage_MDLB:
                    // MDLB in Stage (Stg00_00.dat) have a header starting with extra data
                    // int32 texture ids count -- and the list of int32s for each id
                    Int32 texture_ids_count = BitConverter.ToInt32(current_item_data, 0);
                    // remove texture_ids_count and list of texture ids from array
                    byte[] headerless_item_data = new byte[current_item_data.Length - ( 4 + texture_ids_count * 4)];
                    Array.Copy(current_item_data, 4 + texture_ids_count * 4, headerless_item_data, 0, headerless_item_data.Length); // - (4 + texture_ids_count * 4)

                    mdlb_first_load = true;
                    Load_block_MDLB(headerless_item_data, 21);
                    elementHost_model_editor.Enabled = true;
                    tabControl1.SelectedIndex = 0;
                    break;

                // Stage Model
                case File_Containers.item_data_type.Stage_Model:
                    load_stage_model(current_item_data);
                    elementHost_model_editor.Enabled = true;
                    tabControl1.SelectedIndex = 0;
                    break;

                // Material
                case File_Containers.item_data_type.Material:
                    Load_block_Material_info(item.data);
                    panel_mat_editor.Enabled = true;
                    tabControl1.SelectedIndex = 2;
                    break;

                // Sound
                case File_Containers.item_data_type.Sound:

                    export_sound_file(current_item_data);

                    if (cb_auto_play_sound.Checked)
                    {
                        play_sound();
                    }
                    tabControl1.SelectedIndex = 3;
                    break;
            }
        }


        // returns true if selected item is an MDLB
        private bool selected_MDLB_is_valid()
        {
            // check selected node
            if (trv_file.SelectedNode == null || trv_file.SelectedNode.Index == -1 || trv_file.SelectedNode.Nodes.Count != 0)
            {
                MessageBox.Show("Select an item in the list.");
                return false;
            }

            // check if selected item is an MDLB
            File_Containers.item selected_item = jsrf_file.get_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index);
            if (selected_item.type != File_Containers.item_data_type.MDLB)
            {
                MessageBox.Show("Selected item is not a MDLB.");
                return false;
            }

            if (current_model == null) { MessageBox.Show("Model is empty or no model selected."); return false; }
            if (current_model.VertexBlock_header_List == null) { MessageBox.Show("Model header is null."); return false; }
            if (current_model.VertexBlock_header_List.Count == 0) { MessageBox.Show("Model contains 0 model parts."); return false; }

            return true;
        }


        #endregion


        #region model parsing


        /// <summary>
        /// load JSRF model (MDLB) and store vertex, uv, normals, triangles etc data into arrays to display or export the model data
        /// </summary>
        private void Load_block_MDLB(byte[] data, int mdl_partnum)
        {
            current_model = new DataFormats.JSRF.MDLB(data);

            if (current_model.Model_Parts_header_List.Count == 0)
            {
                System.Windows.MessageBox.Show("Error: MDLB has 0 model parts.");
                return;
            }

            panel_lvl_mdl_info.Visible = false;
            panel_adv_mdl_nfo.Visible = true;

            // gets material from the previous parent node or node that contains materials and has the same count of child items as the textures nodes
            // if no materials in DAT  load materials list from bin
            Int32 texture_id = Get_MDLB_texture_id_mat_list();
            List<GeometryModel3D> meshes = new List<GeometryModel3D>();
         

            #region selected model part

            // model part we want to export (models have up to 21 parts)
            // usually part 21 (20, since 0 to 20) is the actual mesh
            // lower number parts often are just cubes at the position of the bone

            // if its the first time its loaded from the treeview we load the last part
            // otherwise we let the user specify what part to load from the model viewer
            if (mdlb_first_load)
            {
                // if part number higher than what the MDLB has, we load the last part
                if (mdl_partnum > current_model.VertexBlock_header_List.Count - 1)
                {
                    mdl_partnum = current_model.VertexBlock_header_List.Count - 1;
                    txtb_mdl_partnum.Text = (current_model.VertexBlock_header_List.Count - 1).ToString();
                }

                if (mdl_partnum < 0)
                {
                    mdl_partnum = 0;
                }
            }

            #endregion


            #region Load Header data

            // material clusters
            int mat_cluster_count = current_model.Model_Parts_header_List[mdl_partnum].triangle_groups_count;

            // HEADER DATA
            int vert_size = current_model.VertexBlock_header_List[mdl_partnum].vertex_def_size;
            int start_offset = current_model.VertexBlock_header_List[mdl_partnum].vertex_buffer_offset + 16;
            int vert_count = current_model.VertexBlock_header_List[mdl_partnum].vertex_count;

            lab_vtx_flag.Text = (current_model.VertexBlock_header_List[mdl_partnum].unk_flag).ToString();

            int end_offset = (vert_size * vert_count) + start_offset;

            int tris_start = current_model.VertexBlock_header_List[mdl_partnum].triangles_buffer_offset + 16;
            int tris_count = current_model.VertexBlock_header_List[mdl_partnum].triangles_count;
            end_offset = tris_start + (tris_count * 2);

            #endregion

            #region set model viewer info labels

            // materials count
            lab_mdl_mat_typex.Text = "mat count: " + current_model.header.materials_count.ToString();
            // materials defined with 'Vertex_triangles_buffers_header'
            lab_mdl_mat_vtx.Text = "mat vtx count: ";

            if (mdl_partnum == current_model.header.model_parts_count)
            {
                lab_mdl_mat_vtx.Text = "mat vtx count: " + current_model.Model_Parts_header_List[mdl_partnum - 1].materials_count;
            }
            lab_mdlb_parts_count.Text = current_model.VertexBlock_header_List.Count.ToString();

            lab_vertBlockSize.Text = vert_size.ToString();
            lab_vert_count.Text = vert_count.ToString();
            lab_tris_count.Text = (tris_count / 3).ToString();
            lab_mdl_DrawDist.Text = current_model.Model_Parts_header_List[current_model.Model_Parts_header_List.Count - 1].draw_distance.ToString();

            lab_triStrip.Text = "no";
            if (current_model.VertexBlock_header_List[mdl_partnum].stripped_triangles == 1)
            {
                lab_triStrip.Text = "yes";
            }

            #endregion

            // read and store raw mesh data
            MeshData mesh_data = new MeshData();

            #region GET VERTEX DATA

            bounds bounds = new bounds();

            #region determine vertex data order

            // determine uv/normals offset depending on the vertblock size
            // there might be vertex skinning weight/bone data on some blocks like 44,48,56

            int bone_ids_offset = -1, bone_weights_offset = -1, norm_offset = -1, uv_offset = -1;
            lab_vertBlockSize.Text = vert_size.ToString();

            switch (vert_size)
            {
                case 20:
                    uv_offset = 12;
                    break;
                case 24:
                    uv_offset = 12;
                    break;
                case 28:
                    uv_offset = 12;
                    break;

                case 32:
                    norm_offset = 12;
                    uv_offset = 24;
                    break;

                case 36:
                    norm_offset = 12;
                    uv_offset = 24;
                    break;

                case 40:
                    bone_weights_offset = 12; // ok
                    bone_ids_offset = 16; // ok
                    norm_offset = 20;
                    uv_offset = 32; //ok
                    break;

                case 44:
                    bone_weights_offset = 12;
                    bone_ids_offset = 16;
                    norm_offset = 20;
                    uv_offset = 32;
                    break;

                case 48:

                    bone_weights_offset = 12; // ok
                    bone_ids_offset = 16;  // ok
                    norm_offset = 20;
                    uv_offset = 32;

                    // bone weights offset 40

                    break;

                case 56:
                    bone_weights_offset = 12;  // ok
                    bone_ids_offset = 16;  // ok
                    norm_offset = 20;
                    uv_offset = 40;
                    break;
            }
            #endregion

            // loops through vert blocks
            for (int i = start_offset; i < end_offset - vert_size; i = i + vert_size)
            {
                // Vertex
                // mesh_data.vertices.Add(new Point3D(BitConverter.ToSingle(data, i),  BitConverter.ToSingle(data, i + 8) *-1f, BitConverter.ToSingle(data, i + 4)));
                if (i + 8 > data.Length)
                {
                    MessageBox.Show("index out of range (vertex block loop)");
                    return;
                }

                double x = BitConverter.ToSingle(data, i);
                double y = BitConverter.ToSingle(data, i +4);
                double z = BitConverter.ToSingle(data, i +8);

                // flip order 
                Point3D vert = new Point3D(x*-1f, z,y); //new Point3D(-x, z, y);  //// correct face position new Point3D(y, z, x);
                // add vert
                mesh_data.vertices.Add(vert);


                // add point to bounds to calculate the mesh'es bounding box
                bounds.add_point(new Vector3((float)x, (float)z, (float)y));

                // Normals
                if (norm_offset != -1)
                {
                    mesh_data.normals.Add(new Vector3D(BitConverter.ToSingle(data, i + norm_offset), BitConverter.ToSingle(data, i + norm_offset + 8), BitConverter.ToSingle(data, i + norm_offset + 4)));
                }

                // UVs
                if (uv_offset != -1)
                {
                    double u = BitConverter.ToSingle(data, i + uv_offset);
                    double v = BitConverter.ToSingle(data, i + uv_offset + 4);

                    mesh_data.UVs.Add(new System.Windows.Point(u, v));
                }
            }

            mesh_data.mesh_center = new Point3D(bounds.center.X, bounds.center.Y, bounds.center.Z );
            mesh_data.avg_distance = new Point3D(Math.Abs((Math.Abs(bounds.Xmin) + bounds.Xmax)), Math.Abs((Math.Abs(bounds.Ymin) + bounds.Ymax)), Math.Abs((Math.Abs(bounds.Zmin) + bounds.Zmax)));
            mesh_data.mesh_bounds = new Point3D(bounds.Xmin + bounds.Xmax, bounds.Ymin + bounds.Xmax, bounds.Zmin + bounds.Zmax);

            #endregion

            // reads triangle indices data and fills mesh_data Int32Collection
            #region GET TRIANGLE DATA

            List<Int32> triangles_strips = new List<Int32>();

            for (int i = tris_start; i < end_offset; i += 2)
            {
                mesh_data.triangles.Add(BitConverter.ToInt16(data, i));
                triangles_strips.Add(BitConverter.ToInt16(data, i));
            }

            // if stripped convert back to triangle list
            if (current_model.VertexBlock_header_List[mdl_partnum].stripped_triangles == 1)
            {
                Int32Collection triangles_list = new Int32Collection();

                int num_tris = mesh_data.triangles.Count; //triangles_strips.Count; // - 2;
                int num_list_indices = num_tris * 3;

                // re-order stripped triangles
                for (int c = 0; c < triangles_strips.Count - 2; c++)
                {

                    if (c % 2 == 0)
                    {
                        triangles_list.Add(triangles_strips[c]);
                        triangles_list.Add(triangles_strips[c + 1]);
                        triangles_list.Add(triangles_strips[c + 2]);
                    }
                    else
                    {
                        triangles_list.Add(triangles_strips[c + 2]);
                        triangles_list.Add(triangles_strips[c + 1]);
                        triangles_list.Add(triangles_strips[c]);
                    }
                }

                mesh_data.triangles = triangles_list;
            }

            #endregion




            #region load full mesh with one material (no clusters nor splititng the mesh)          
            
            Extract_texture_by_id(texture_id);
            mesh_data.texture_id = texture_id;
            meshes = new List<GeometryModel3D>();

            // model color
            System.Windows.Media.Color clr = System.Windows.Media.Color.FromRgb(128, 128, 128);
            // get first triangle group's material
            if (current_model.Model_Parts_header_List[mdl_partnum].triangle_groups_List.Count > 0)
            {
                MDLB.triangle_group tg = current_model.Model_Parts_header_List[mdl_partnum].triangle_groups_List[0];
                if(current_model.materials_List.Count > 0)
                {      
                    if(tg.material_index  < current_model.materials_List.Count )
                    {
                        try
                        {
                            MDLB.color color = current_model.materials_List[tg.material_index].color;
                            clr.R = color.R; clr.G = color.G; clr.B = color.B;
                        } catch (Exception error)
                        {
                            MessageBox.Show("Error:'MDLB.color color' material groups has an invalid index.");
                        }

                    }

                }
            }

            meshes.Add(Get_WPF_model(mesh_data, clr));
            Load_model_to_HelixModelViewer(meshes, mesh_data.mesh_center, mesh_data.mesh_bounds, mesh_data.avg_distance);
            
            #endregion

        }

        /// <summary>
        ///  loads mesh data into a WPF GeometryModel3D and return it
        /// </summary>
        private GeometryModel3D Get_WPF_model(MeshData mesh_data, System.Windows.Media.Color color)
        {

            // WPF model containers
            GeometryModel3D GeometryModel = new GeometryModel3D();
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            #region mesh data

            myMeshGeometry3D.Positions = mesh_data.vertices;
            myMeshGeometry3D.TextureCoordinates = mesh_data.UVs;
            myMeshGeometry3D.Normals = mesh_data.normals;
            myMeshGeometry3D.TriangleIndices = mesh_data.triangles;

            // Apply the mesh to the geometry model.
            GeometryModel.Geometry = myMeshGeometry3D;

            #endregion

            #region material

            // solid color
            /*
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Colors.LightGray;
            DiffuseMaterial myMaterial = new DiffuseMaterial(brush);
            GeometryModel.Material = myMaterial;
             */
            // assign texture if it was extracted and file exists
            if (File.Exists(tmp_dir + mesh_data.texture_id + ".png"))
            {
                ImageBrush image_brush = new ImageBrush();
                byte[] buffer = System.IO.File.ReadAllBytes((tmp_dir + mesh_data.texture_id + ".png"));
                MemoryStream ms = new MemoryStream(buffer);

                System.Windows.Media.Imaging.BitmapImage imageSource = new System.Windows.Media.Imaging.BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = ms;
                imageSource.EndInit();
                imageSource.Freeze();

                #region flip image
                /*
                    TransformedBitmap tb = new TransformedBitmap();
                    // Properties must be set between BeginInit and EndInit calls.
                    tb.BeginInit();
                    tb.Source = imageSource;
                    ScaleTransform transform = new ScaleTransform(1, -1);
                    // Set image rotation.
                    // RotateTransform transform = new RotateTransform(90);
                    tb.Transform = transform;
                    tb.EndInit();

                    image_brush.ImageSource = tb;
                 */
                #endregion

                image_brush.ImageSource = imageSource;

                // Define material and apply to the mesh geo
                DiffuseMaterial myMaterial = new DiffuseMaterial(image_brush);
                GeometryModel.Material = myMaterial;
            }
            else // grey material
            {
                System.Windows.Media.Media3D.Material myMaterial = new DiffuseMaterial(new SolidColorBrush(color));
                // myMaterial.Color = System.Windows.Media.Color.FromRgb(90, 90, 90);
                GeometryModel.Material = myMaterial;
            }

            #endregion


            return GeometryModel;
        }

        #endregion

        #region model operations

        /// <summary>
        /// rewrites model's vertices positions to collapse them all into a single point
        /// and returns its modified array
        /// </summary>
        /// <param name="data">block of data</param>
        /// <param name="mdl_partnum">model part number</param>
        private byte[] Collapse_model_vertices(byte[] data)
        {
            current_model = new DataFormats.JSRF.MDLB(data);

            // for each model part
            for (int i = 0; i < current_model.VertexBlock_header_List.Count; i++)
            {
                #region get header data needed for model part

                int vert_size = current_model.VertexBlock_header_List[i].vertex_def_size;
                int start_offset = current_model.VertexBlock_header_List[i].vertex_buffer_offset + 16;
                int vert_count = current_model.VertexBlock_header_List[i].vertex_count;
                int end_offset = (vert_size * vert_count) + start_offset;

                #endregion

                #region SET VERTEX DATA

                // loops through vert blocks
                for (int v = start_offset; v < end_offset - vert_size; v = v + vert_size)
                {
                    // set vertex 3 floats (0 -100 0) x y z
                    ///Buffer.BlockCopy(new byte[] { 0x00, 0x00, 0x00, 0x00,     0x88, 0x13, 0x00, 0x00,      0x00, 0x00, 0x00, 0x00, }, 0, data, i, 12); //Buffer.BlockCopy(new byte[12], 0, data, i, 12);
                    // set empty vertex data based on vertex block size, so vertex will be sert to xzy = 0 and vertex weight to bone 0, uv to zero etc
                    Buffer.BlockCopy(new byte[vert_size], 0, data, v, vert_size);
                }
                #endregion
            }
            return data;
        }


        /// <summary>
        /// makes model invisible by setting the draw distance to zero
        /// </summary>
        /// <param name="data">block of data</param>
        /// <param name="mdl_partnum">model part number</param>
        private byte[] Nullify_model(byte[] data, int mdl_partnum)
        {

            current_model = new DataFormats.JSRF.MDLB(data);
            int part = mdl_partnum;

            // if its the first time its loaded from the treeview we load the last part
            // otherwise we let the user specify what part to load from the model viewer
            if (mdlb_first_load)
            {
                // if part number higher than what the MDLB has, we load the last part
                if (mdl_partnum > current_model.VertexBlock_header_List.Count - 1)
                {
                    part = current_model.VertexBlock_header_List.Count - 1;
                    //txtb_mdl_partnum.Text = (model.VertexBlock_header_List.Count - 1).ToString();
                }

                if (mdl_partnum < 0)
                {
                    part = 0;
                }
            }


            // (16 = MDLB header) (28 position of drawdistance value in model part header) (+128 bytes for each model part header)
            int draw_distance_property_offset = 16 + 28 + (128 * (mdl_partnum - 1));
            // set value to zero
            // not sure if all these values are for draw distance 
            Buffer.BlockCopy(new byte[4], 0, data, draw_distance_property_offset - 12, 4);
            Buffer.BlockCopy(new byte[4], 0, data, draw_distance_property_offset - 8, 4);
            Buffer.BlockCopy(new byte[4], 0, data, draw_distance_property_offset - 4, 4);
            Buffer.BlockCopy(new byte[4], 0, data, draw_distance_property_offset, 4);


            return data;

        }


        /// <summary>
        /// makes model invisible by setting the triangle group triangle count to zero
        /// </summary>
        /// <param name="data">block of data</param>
        /// <param name="mdl_partnum">model part number</param>
        private byte[] make_model_invisible(byte[] data, int mdl_partnum)
        {

            current_model = new DataFormats.JSRF.MDLB(data);
            int part_num = mdl_partnum;

            // if its the first time its loaded from the treeview we load the last part
            // otherwise we let the user specify what part to load from the model viewer
            if (mdlb_first_load)
            {
                // if part number higher than what the MDLB has, we load the last part
                if (mdl_partnum > current_model.VertexBlock_header_List.Count - 1)
                {
                    part_num = current_model.VertexBlock_header_List.Count - 1;
                    //txtb_mdl_partnum.Text = (model.VertexBlock_header_List.Count - 1).ToString();
                }

                if (mdl_partnum < 0)
                {
                    part_num = 0;
                }
            }


            int tris_group_count = current_model.Model_Parts_header_List[part_num].triangle_groups_count;

            for (int i = 0; i < tris_group_count; i++)
            {
                // make triangle group count = 0
                int tris_group_offset = current_model.Model_Parts_header_List[part_num].triangle_groups_list_offset + i * 32;

                Buffer.BlockCopy(new byte[4], 0, data, tris_group_offset + 16, 4);

            }

            return data;
        }

        /// <summary>
        /// set model's draw distance
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] fix_mdl_draw_distance(byte[] data)
        {
            current_model = new DataFormats.JSRF.MDLB(data);

            int drawdistY_offset = 32 + ((current_model.VertexBlock_header_List.Count - 1) * 128) + 4;

            // set y draw distance to 8.5
            //Buffer.BlockCopy(BitConverter.GetBytes(15f), 0, data, drawdistY_offset, 4);

            Buffer.BlockCopy(BitConverter.GetBytes(20f), 0, data, drawdistY_offset + 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(20f), 0, data, drawdistY_offset, 4);

            return data;
        }

        #endregion

        #region model viewer


        /// <summary>
        /// loads like of meshes (+materials) into the Helix model viewer
        /// </summary>
        private void Load_model_to_HelixModelViewer(List<GeometryModel3D> meshes, Point3D mesh_center, Point3D mesh_bounds, Point3D avg_distance)
        {
            // clean element host and create new model viewer
            // HelixModelViewer mv = (HelixModelViewer)elementHost_model.Child;
            HelixModelViewer ModelViewer = new HelixModelViewer();
            ModelVisual3D models = new ModelVisual3D();
            HelixViewport3D view = ModelViewer.view1;


            #region lights

            Model3DGroup light_group = new Model3DGroup();
            ModelVisual3D lights_visual = new ModelVisual3D();

            DirectionalLight light = new DirectionalLight();
            light.Color = Colors.White;
            light.Direction = new Vector3D(0.61, -0.5, 0.61);
            lights_visual.Content = light;

            /*
            DirectionalLight light1 = new DirectionalLight();
            light1.Color = System.Windows.Media.Color.FromArgb(255, 255, 100, 100);
            light1.Direction = new Vector3D(-0.61, 0.5, 0.61);
            lights_visual.Content = light1;
            */

            // new_mv.view1.Viewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });
            view.Viewport.Children.Add(lights_visual);

            #endregion

            ModelVisual3D model = new ModelVisual3D();


            for (int i = 0; i < meshes.Count; i++)
            {
                model.Content = meshes[i];
            }

            ScaleTransform3D myScaleTransform3D = new ScaleTransform3D();

            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // 
            // // //   NOTICE! model gets flipped the following params                                            // // // // 
            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // 

            myScaleTransform3D.ScaleX = 1; // -1
            myScaleTransform3D.ScaleY = -1;
            myScaleTransform3D.ScaleZ = 1;
            model.Transform = myScaleTransform3D;

            ProjectionCamera cam = view.Camera;

            cam.Position = new Point3D(mesh_center.X + avg_distance.X/2, mesh_center.Y + avg_distance.Y/2, mesh_center.Z + avg_distance.Z/2);
            cam.LookAt(new Point3D(mesh_center.X, mesh_center.Y, mesh_center.Z), 0);

            // add model to view
            view.Children.Add(model);
            // load scene into model viewer
            elementHost_model_editor.Child = ModelViewer;
        }

        // switch model part +=
        private void Btn_mdl_partPlus_Click(object sender, EventArgs e)
        {
            // check selected item is valid
            if (!selected_MDLB_is_valid())
            {
                return;
            }

            int num = Convert.ToInt16(txtb_mdl_partnum.Text);

            num += 1;

            txtb_mdl_partnum.Text = num.ToString();

            Load_block_MDLB(current_item_data, num);
        }
        // switch model part -=
        private void Btn_mdl_partMin_Click(object sender, EventArgs e)
        {
            // check selected item is valid
            if (!selected_MDLB_is_valid())
            {
                return;
            }

            int num = Convert.ToInt16(txtb_mdl_partnum.Text);

            if (num > 0)
            {
                num -= 1;

                txtb_mdl_partnum.Text = num.ToString();

                Load_block_MDLB(current_item_data, num - 1);
            }
        }

        #endregion


        #region load Texture

        /// <summary>
        /// extracts, converts texture and loads it into bitmap viewer
        /// </summary>
        /// <param name="data">texture block of data</param>
        /// <param name="id">texture ID</param>
        /// <param name="silent">load bitmap into picturebox</param>
        private string Load_block_Texture(byte[] data, bool silent, bool by_id) //string id,
        {
            DDS_CompressionFormat compressionFormat = DDS_CompressionFormat.unknown1;
            string dxt_compression_type = "unknown";

            // #region read texture header

            Int32 id = BitConverter.ToInt32(data, 0);
            Int32 unk_8 = BitConverter.ToInt32(data, 8);
            Int32 unk_16 = BitConverter.ToInt32(data, 16);
            Int32 res_x = BitConverter.ToInt32(data, 20);

            byte dxt_format = data[24]; // 5 = dxt1 | 6 = dxt3 |
            byte unk_25 = data[25]; // has alpha? is a cube map? not sure, probably if has alpha
            byte swizzled = data[26]; // if = 1 texture is swizzled (swizzle for xbox textures) // http:// fgiesen.wordpress.com/2011/01/17/texture-tiling-and-swizzling/ // http:// gtaforums.com/topic/213907-unswizzle-tool/#entry3172924 // http:// forum.xentax.com/viewtopic.php?t=2640
            // http:// en.wikipedia.org/wiki/Z-order_curve // morton order
            byte unk_27 = data[27];
            Int16 mipmap_count = Convert.ToInt16(current_item_data[28]);

            Int32 end_padding = BitConverter.ToInt32(data, 28); // mip map count if > 0 add 8 bytes of padding at the end of file

            switch (dxt_format)
            {
                case 1:
                    dxt_compression_type = "1";
                    compressionFormat = DDS_CompressionFormat.DXT1;
                    break;

                case 5:
                    dxt_compression_type = "dxt1";
                    compressionFormat = DDS_CompressionFormat.DXT1;
                    break;

                case 6:
                    dxt_compression_type = "dxt3";
                    compressionFormat = DDS_CompressionFormat.DXT3;
                    break;

                case 7:
                    dxt_compression_type = "dxt5"; // ?
                    compressionFormat = DDS_CompressionFormat.DXT5;
                    break;
            }

            // set texture info fox texbox
            if(!silent)
            {

                #region write texture info in textbox

                rtxtb_textureinfo.Clear();
                rtxtb_textureinfo.AppendText("\n" + "Texture ID: " + id.ToString());
                //rtxtb_textureinfo.AppendText("\n" + unk_8.ToString() + " unkown (offset 8)");
                //rtxtb_textureinfo.AppendText("\n" + unk_16.ToString() + " unkown (offset 16)");
                rtxtb_textureinfo.AppendText("\n" + "Resolution: " + res_x.ToString() + "x" + res_x.ToString());
                rtxtb_textureinfo.AppendText("\n" + "Compression: " + dxt_compression_type.ToUpper()); //+ dxt_format.ToString() + 
                rtxtb_textureinfo.AppendText("\n" + "Mipmap count: " + mipmap_count.ToString() + " ");

                string is_swizzled = "no";
                if (swizzled == 1)
                {
                    is_swizzled = "yes";
                }
                rtxtb_textureinfo.AppendText("\n" + "Swizzled: " + swizzled);
                if (unk_25 == 1)
                {
                    rtxtb_textureinfo.AppendText("\n" + "Is transparent " + unk_25.ToString()); //unkown (offset 25)
                }

                if (unk_25 == 5)
                {
                    rtxtb_textureinfo.AppendText("\n" + "No transparency " + unk_25.ToString()); //unkown (offset 25)
                }


                //rtxtb_textureinfo.AppendText("\n" + unk_27.ToString());
                //rtxtb_textureinfo.AppendText("\n" + end_padding.ToString() + " mipmap count");


                #endregion
            }

            if(dxt_compression_type != "dxt1" && dxt_compression_type != "dxt3")
            {
                rtxtb_textureinfo.AppendText("\n" + "Compression: " + dxt_format.ToString());
                return id.ToString();
            }

            // /TODO remove data header
            byte[] data_noheader = new byte[data.Length - 32];
            System.Buffer.BlockCopy(data, 32, data_noheader, 0, data_noheader.Length);// /

            byte[] dds_header = Generate_dds_header(res_x, dxt_compression_type);
            byte[] texture_file = new byte[dds_header.Length + data_noheader.Length + 32];

            if (swizzled == 1)
            {
                byte[] data_unswizz = DataFormats.Xbox.TextureSwizzle.QuadtreeUnswizzle(data_noheader, res_x);
                byte[] dds_header_1 = GenerateDdsHeader(compressionFormat, res_x, mipmap_count);

                System.Buffer.BlockCopy(dds_header_1, 0, texture_file, 0, dds_header_1.Length);
                System.Buffer.BlockCopy(data_unswizz, 0, texture_file, dds_header_1.Length, data_unswizz.Length);

                /*
                byte[] data_unswizz = new byte[data.Length - 32];
                data_unswizz = DataFormats.Xbox.TextureSwizzle.(data_noheader, 0, res_x, res_x, (int)numupDown_tex_depth.Value, (int)numupDown_tex_bitCount.Value, true);

                if (data_unswizz == null) { System.Media.SystemSounds.Asterisk.Play(); return ""; }

                System.Buffer.BlockCopy(dds_header, 0, texture_file, 0, dds_header.Length);
                System.Buffer.BlockCopy(data_unswizz, 0, texture_file, dds_header.Length, data_unswizz.Length);
                */
                /*
                //byte[] data_decompressed = new byte[data.Length - 32];
                //DataFormats.Xbox.TextureSwizzle.DecompressDxt3(data_decompressed, data_noheader, res_x, res_x);

                byte[] data_unswizz = new byte[data.Length - 32];
                data_unswizz = DataFormats.Xbox.TextureSwizzle.Swizzle(data_noheader, res_x, res_x, (int)numupDown_tex_depth.Value, (int)numupDown_tex_bitCount.Value, true);

                if (data_unswizz == null) { System.Media.SystemSounds.Asterisk.Play(); return ""; }

                System.Buffer.BlockCopy(dds_header, 0, texture_file, 0, dds_header.Length);
                System.Buffer.BlockCopy(data_unswizz, 0, texture_file, dds_header.Length, data_unswizz.Length);
                */

            } else {
                System.Buffer.BlockCopy(dds_header, 0, texture_file, 0, dds_header.Length);
                System.Buffer.BlockCopy(data_noheader, 0, texture_file, dds_header.Length, data_noheader.Length);
            }
          


            string filename = "tmp";

            if (by_id) { filename = id.ToString(); }

            Parsing.ByteArrayToFile(tmp_dir + "\\" + filename + ".dds", texture_file);

            #region convert dds to png


            string args = "-i=" + filename + ".dds -o=" + filename + ".png -genmipmaps=1";

            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Application.StartupPath + "\\resources\\tmp\\",
                    FileName = Application.StartupPath + "\\resources\\tools\\VampConvert.exe",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();
            proc.Dispose();

            if (!File.Exists(tmp_dir + filename + ".png"))
            {
                MessageBox.Show("Could not load texture file: \n" + tmp_dir + filename + ".png");
                return "";
            }

            #endregion

            // if not in silent mode: load bitmap into picturebox and texture info in textbox
            if (!silent)
            {
                if (res_x > 512)
                {
                    pictureBox_texture_editor.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    pictureBox_texture_editor.SizeMode = PictureBoxSizeMode.CenterImage;
                }

                Stream BitmapStream = System.IO.File.Open(tmp_dir + filename + ".png", System.IO.FileMode.Open);
                Image imgPhoto = Image.FromStream(BitmapStream, true);

                BitmapStream.Dispose();
                BitmapStream.Close();

                Image bmp = new Bitmap(imgPhoto);
                pictureBox_texture_editor.Image = bmp;
            }

            return id.ToString();
        }

        public enum DDS_CompressionFormat
        {
            DXT1 = 5,
            DXT3 = 6,
            DXT5 = 7,//?
            unknown1 = 1
        }

        private byte[] GenerateDdsHeader(DDS_CompressionFormat _CompressionFormat, int _Resolution, int _MipmapCount)
        {
            var ms = new MemoryStream();
            ms.Write(new byte[] { 0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x0A, 0x00, 0x00 }, 0, 0xC);
            int linearSize = _Resolution * _Resolution;
            if (_CompressionFormat == DDS_CompressionFormat.DXT1)
                linearSize /= 2;
            foreach (var value in new int[] { _Resolution, _Resolution, linearSize })
                ms.Write(BitConverter.GetBytes(value), 0, 0x4);

            ms.Write(new byte[0x4], 0, 0x4);
            ms.Write(BitConverter.GetBytes((int)Math.Log(_Resolution, 2) + 1), 0, 0x4);

            ms.Write(new byte[0x2C], 0, 0x2C);

            ms.Write(BitConverter.GetBytes(32), 0, 0x4);
            ms.Write(BitConverter.GetBytes(4), 0, 0x4);
            ms.WriteByte(0x44);
            ms.WriteByte(0x58);

            ms.WriteByte(0x54);
            if (_CompressionFormat == DDS_CompressionFormat.DXT1) ms.WriteByte(0x31);
            else if (_CompressionFormat == DDS_CompressionFormat.DXT3) ms.WriteByte(0x33);
            else if (_CompressionFormat == DDS_CompressionFormat.DXT5) ms.WriteByte(0x35);
            else ms.WriteByte(0x31); // unknown fallback

            ms.Write(new byte[0x14], 0, 0x14);

            if (_CompressionFormat == DDS_CompressionFormat.DXT1 || (_CompressionFormat == DDS_CompressionFormat.DXT3 && _MipmapCount > 0))
                ms.Write(BitConverter.GetBytes(4198408), 0, 0x4);
            else if (_CompressionFormat == DDS_CompressionFormat.DXT3 && _MipmapCount == 0)
                ms.Write(BitConverter.GetBytes(4096), 0, 0x4);
            ms.Write(new byte[0x10], 0, 0x10);
            if (ms.Length != 0x80) throw new InvalidDataException($"Generated header was {ms.Length.ToString("X")} bytes instead of 0x80.");
            return ms.ToArray();
        }

        /// <summary>
        /// generate DDS header data that we need to add to reconstruct the DDS texture file
        //create DDS header data that we need to add to reconstruct the DDS texture file
        private byte[] Generate_dds_header(int size, string dxt_compression_type)
        {

            string str = "";
            List<string> str_list = new List<string>();

            str_list.Add("44 44 53 20 7C 00 00 00 07 10 0A 00 00");

            //image resolution xy

            if (dxt_compression_type == "dxt3" | dxt_compression_type == "dxt3_")
            {
                switch (size)
                {
                    case 2048:
                        str_list.Add("00 08 00 00 00 08 00 00 00 00 40 00");
                        break;
                    case 1024:
                        str_list.Add("00 04 00 00 00 04 00 00 00 00 10 00");
                        break;
                    case 512:
                        str_list.Add("00 02 00 00 00 02 00 00 00 00 04 00");
                        break;
                    case 256:
                        str_list.Add("00 01 00 00 00 01 00 00 00 00 01 00");
                        break;
                    case 128:
                        str_list.Add("80 00 00 00 80 00 00 00 00 40 00 00");
                        break;
                    case 64:
                        str_list.Add("40 00 00 00 40 00 00 00 00 10 00 00");
                        break;
                    case 32:
                        str_list.Add("20 00 00 00 20 00 00 00 00 04 00 00");
                        break;
                    case 16:
                        str_list.Add("10 00 00 00 10 00 00 00 00 01 00 00");
                        break;
                    case 8:
                        str_list.Add("08 00 00 00 08 00 00 00 40 00 00 00");
                        break;
                }

                //image resolution xy

            }
            else if (dxt_compression_type == "dxt1")
            {
                switch (size)
                {
                    case 2048:
                        str_list.Add("00 08 00 00 00 08 00 00 00 00 20 00");
                        break;
                    case 1024:
                        str_list.Add("00 04 00 00 00 04 00 00 00 00 08 00");
                        break;
                    case 512:
                        str_list.Add("00 02 00 00 00 02 00 00 00 00 02 00");
                        break;
                    case 256:
                        str_list.Add("00 01 00 00 00 01 00 00 00 80 00 00");
                        break;
                    case 128:
                        str_list.Add("80 00 00 00 80 00 00 00 00 20 00 00");
                        break;
                    case 64:
                        str_list.Add("40 00 00 00 40 00 00 00 00 08 00 00");
                        break;
                    case 32:
                        str_list.Add("20 00 00 00 20 00 00 00 00 02 00 00");
                        break;
                    case 16:
                        str_list.Add("10 00 00 00 10 00 00 00 80 00 00 00");
                        break;
                    case 8:
                        str_list.Add("08 00 00 00 08 00 00 00 20 00 00 00");
                        break;
                }

            }



            str_list.Add(" 00 00 00 00 ");

            //image size
            switch (size)
            {
                case 2048:
                    str_list.Add("0C 00 00 00");
                    break;
                case 1024:
                    str_list.Add("0B 00 00 00");
                    break;
                case 512:
                    str_list.Add("0A 00 00 00");
                    break;
                case 256:
                    str_list.Add("09 00 00 00");
                    break;
                case 128:
                    str_list.Add("08 00 00 00");
                    break;
                case 64:
                    str_list.Add("07 00 00 00");
                    break;
                case 32:
                    str_list.Add("06 00 00 00");
                    break;
                case 16:
                    str_list.Add("05 00 00 00");
                    break;
                case 8:
                    str_list.Add("04 00 00 00");
                    break;
            }

            str_list.Add(" 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 00 00 00 04 00 00 00 44 58");

            //DXT# type
            if (dxt_compression_type == "dxt1")
            {
                str_list.Add(" 54 31");
            }
            else if (dxt_compression_type == "dxt3" | dxt_compression_type == "dxt3_")
            {
                str_list.Add(" 54 33");
            }
            else if (dxt_compression_type == "dtx5")
            {
                str_list.Add(" 54 35");
            }

            str_list.Add(" 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            //defines if the texture has mip maps
            if (dxt_compression_type == "dxt1" | dxt_compression_type == "dxt3")
            {
                //has mipmaps
                str_list.Add(" 08 10 40 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                // no mip maps
            }
            else if (dxt_compression_type == "dxt3_")
            {
                str_list.Add(" 00 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            }




            try
            {
                str = str_list[0] + str_list[1] + str_list[2] + str_list[3] + str_list[4] + str_list[5] + str_list[6] + str_list[7];
            }
            catch
            {
            }

            return Parsing.String_HexToBytes(str);


        }

        #endregion


        #region materials
        /// <summary>
        /// reads material block and displays the info in the Form1 rtxtb_materialinfo
        /// </summary>
        private void Load_block_Material_info(byte[] data)
        {
            int pos = 0;
            while(pos < data.Length)
            {
                rtxtb_materials.AppendText(BitConverter.ToInt32(data, pos).ToString() + Environment.NewLine );

                pos += 4;
            }

            tabControl1.SelectedIndex = 2;
        }



        /// <summary>
        /// saves currently selected material block data (and changes applied through Texture editor tab)
        /// </summary>
        private void Save_block_Material()
        {
            var lines = this.rtxtb_materials.Text.Split('\n').ToList();

            //if (lines.Count > 4) { MessageBox.Show("Error: there should only be 4 lines to define the materials."); return; }

            List<String> mat_data = new List<string> { "0", "0", "0", "0" };


            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] == "") { continue; }
                mat_data[i] = lines[i];
            }
            
            if (current_item_data == null) { MessageBox.Show("Material is empty or no material selected."); return; }
            if (current_item_data.Length == 0) { MessageBox.Show("Material data is empty."); return; }

            current_item_data = new byte[16];
            //Buffer.BlockCopy(data, 0, fdata, 0, data.Length);

            // get material info from textboxes and set into array data_block
            Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(mat_data[0])), 0, current_item_data, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(mat_data[1])), 0, current_item_data, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(mat_data[2])), 0, current_item_data, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(mat_data[3])), 0, current_item_data, 12, 4);
  


            // rewrite block into file
            jsrf_file.set_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, current_item_data);
            Rebuild_file(true, true, true);
        }


        /// <summary>
        /// gets the node from the NORM above the current selected node and checks that its a material
        /// </summary>
        /// <remarks>
        /// this is a quick workaround/placeholder method, this might not be working properly
        /// </remarks>
        private Int32 Get_MDLB_texture_id_mat_list()
        {
            Int32 texture_id = 0;
            List<Int32> texture_ids = new List<Int32>();

            if ((materials_dat_list.Count == 0) && (materials_bin_list.Count == 0)) { return 0; }

            //if (last_selected_node == null) { return new List<int>(); }
            int mdl_container_items_count = trv_file.SelectedNode.Parent.Nodes.Count; //trv_file.SelectedNode.Index;
            int node_index = trv_file.SelectedNode.Index;

            for (int m = 0; m < materials_dat_list.Count; m++)
            {
                if(m == trv_file.SelectedNode.Index)
                {
                    texture_id = materials_dat_list[m].texture_id[0];
                }
            }

            for (int m = 0; m < materials_bin_list.Count; m++)
            {
                if (m == trv_file.SelectedNode.Index)
                {
                    texture_id = materials_bin_list[m].texture_id[0];
                }
            }


            /*
            for (int m = 0; m < materials_bin_list.Count; m++)
            {

                for (int mid = 0; mid < materials_bin_list[m].texture_id.Count; mid++)
                {
                    int mat_id = materials_bin_list[m].texture_id[mid];

                    // TODO support different containers and headerless indexed files
                    if(trv_file.SelectedNode.Parent.Parent == null)
                    {
                        texture_ids.Add(0);
                        return texture_ids;
                    }

                    // find textures container with same number of items as MDLB container
                    for (int n = 0; n< trv_file.SelectedNode.Parent.Parent.Nodes.Count; n++)
                    {
                        TreeNode tn = trv_file.SelectedNode.Parent.Parent.Nodes[n];

                        // if number of child nodes match and is textures
                        if (tn.Nodes.Count == trv_file.SelectedNode.Parent.Nodes.Count)
                        {
                            if(JSRF_Containers.item_data_type.Texture == ((JSRF_Containers.item)jsrf_file.get_item(n, 0)).type)
                            {
                                for (int nid = 0; nid < trv_file.SelectedNode.Parent.Parent.Nodes[n].Nodes.Count; nid++)
                                {
                                    JSRF_Containers.item item = jsrf_file.get_item(n, nid);
                                    if(item.data.Length > 0)
                                    {
                                        int tid = BitConverter.ToInt32(item.data, 0);

                                        if (tid == mat_id)
                                        {
                                            texture_ids.Add(tid);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            */
            /*
            if (materials_dat_list.Count == 0)
            {
                if (node_index >= materials_bin_list.Count)
                {
                    node_index = materials_bin_list.Count - 1;
                }
                if (node_index < materials_bin_list.Count)
                    mat = materials_bin_list[node_index].texture_id;

            } else {
                if (node_index >= materials_dat_list.Count)
                {
                    node_index = materials_dat_list.Count - 1;
                }
                mat = materials_dat_list[node_index].texture_id;
            }
            */
            return texture_id;
        }

        /// <summary>
        /// searches all textures in file and gets the one with matching id
        /// </summary>
        /// <remarks>
        /// this is a quick workaround/placeholder method, this might not be working properly
        /// </remarks>
        private void Extract_texture_by_id(Int32 texture_id)
        {
            List<File_Containers.item_match> matches = jsrf_file.find_items_ofType(File_Containers.item_data_type.Texture);

            foreach (var m in matches)
            {
                // if texture ID matches
                if (BitConverter.ToInt32(m.item.data, 0) == texture_id)
                {
                    // load texture
                    Load_block_Texture(m.item.data, true, true);
                }
            }
        }


        /// <summary>
        /// returns the textue id from texture block
        /// </summary>
        private int Get_texture_id(byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }

        /// <summary>
        /// searches all materials blocks in the dat and adds it to the  materials_dat_list
        /// </summary>
        /// <remarks>
        /// TODO: add support for files that start with a NORM container.
        /// </remarks>
        private List<DataFormats.JSRF.Material> Get_materials_to_list(File_Containers file)
        {
            if (!file.has_items()) { return new List<DataFormats.JSRF.Material>(); }

            List<DataFormats.JSRF.Material> materials_list = new List<DataFormats.JSRF.Material>();

            // find material items
            List<File_Containers.item_match> matches = file.find_items_ofType(File_Containers.item_data_type.Material);

            foreach (var item in matches)
            {
                materials_list.Add(new DataFormats.JSRF.Material(item.item.data));
            }
            return materials_list;
        }

        #endregion



        #region export/import data


        // export model last part
        private void Btn_extract_mdl_lastp_Click(object sender, EventArgs e)
        {
            // check selected item is valid if popup messagbox saying why and return
            if (!selected_MDLB_is_valid())
            {
                return;
            }

            #region setup file save dialog

            // select export folder and main file name?
            System.Windows.Forms.SaveFileDialog saveFileDiag = new System.Windows.Forms.SaveFileDialog();
            //saveFileDialog1.InitialDirectory = @"C:\";      
            saveFileDiag.Title = "Export model as SMD files";
            //saveFileDialog1.CheckFileExists = true;
            saveFileDiag.CheckPathExists = true;
            saveFileDiag.DefaultExt = "smd";
            saveFileDiag.Filter = "SMD files (*.smd)|*.smd";
            saveFileDiag.FilterIndex = 2;
            //saveFileDiag.RestoreDirectory = true;

            // restore previous saved directory
            string import_dir = Setting_load("smd_export_dir");
            if (Directory.Exists(import_dir)) { saveFileDiag.InitialDirectory = import_dir; }

            #endregion

            // if save file dialog result is OK
            if (saveFileDiag.ShowDialog() == DialogResult.OK)
            {
                // get filepath
                string filepath = saveFileDiag.FileName;
                string save_dir = Path.GetDirectoryName(filepath)  +"\\";
                string filename = Path.GetFileNameWithoutExtension(filepath);

                Setting_save(save_dir, "smd_export_dir");

                #region make sure filepath and filename are valid

                // if save directory doesn't exist inform user and ask to regive input, return
                if (!Directory.Exists(save_dir))
                {
                    MessageBox.Show("Save directory doesn't exist, please select an existing folder to save the files.");
                    return;
                }

                // make sure filename is valid
                if(filename == "")
                {
                    MessageBox.Show("Filename is empty, please save to an existing folder and with a file name.");
                    return;
                }

                #endregion

                // for each model part we export an SMD file
                for (int i = 0; i < current_model.VertexBlock_header_List.Count; i++)
                {
                    Export_model_part(save_dir, filename, i);
                }
            }
        }


        // export model part
        private void Export_model_part(string save_dir, string filename, int part_num)
        {
            if (current_model == null)
            {
                MessageBox.Show("export_model_part(" + part_num + ") Error, could not export model part (model is null)");
                return;
            }

            #region get & parse model data

            byte[] data = current_item_data;

            // TODO split mesh by material cluster
            // right now we only read the first cluster and get the material number (number for the texture_id_list)
            int mat_cluster_count = current_model.Model_Parts_header_List[part_num].triangle_groups_count;

            #region Load Header data

            int vert_size = current_model.VertexBlock_header_List[part_num].vertex_def_size;
            int start_offset = current_model.VertexBlock_header_List[part_num].vertex_buffer_offset + 16;
            int vert_count = current_model.VertexBlock_header_List[part_num].vertex_count;
            int end_offset = (vert_size * vert_count) + start_offset;

            int tris_start = current_model.VertexBlock_header_List[part_num].triangles_buffer_offset + 16;
            int tris_count = current_model.VertexBlock_header_List[part_num].triangles_count;
            int tris_end = tris_start + (tris_count * 2);

            #endregion

            lab_triStrip.Text = current_model.VertexBlock_header_List[part_num].stripped_triangles.ToString();

            // read and store mesh data
            MeshData mesh_data = new MeshData();

            #region GET VERTEX DATA

            #region determine vertex data order

            // determine uv/normals offset depending on the vertblock size
            // there might be vertex skinning weight/bone data on some blocks like 44,48,56

            int bone_ids_offset = -1, bone_weights_offset = -1, norm_offset = -1, uv_offset = -1;
            lab_vertBlockSize.Text = vert_size.ToString();

            switch (vert_size)
            {

                case 20:
                    uv_offset = 12;
                    break;
                case 24:
                    uv_offset = 12;
                    // uv_offset = +1;
                    break;
                case 28:
                    uv_offset = 12;
                    break;

                case 32:
                    norm_offset = 12; // ok
                    uv_offset = 24;  // ok
                    break;

                case 36:
                    norm_offset = 12;
                    uv_offset = 20;
                    break;

                case 40:
                    bone_weights_offset = 12; // ok
                    bone_ids_offset = 16; // ok
                    norm_offset = 20; // ok
                    uv_offset = 32; //ok
                    break;

                case 44:
                    bone_weights_offset = 28;
                    bone_ids_offset = 32;
                    uv_offset = 32;
                    norm_offset = -1;
                    break;

                case 48:

                    bone_weights_offset = 12; // ok
                    bone_ids_offset = 16;  // ok
                    norm_offset = 20;
                    uv_offset = 32;

                    // bone weights offset 40

                    break;

                case 56:
                    bone_weights_offset = 12;  // ok
                    bone_ids_offset = 16;  // ok
                    norm_offset = 20;
                    uv_offset = 40;
                    break;
            }
            #endregion

            // loops through vert blocks
            for (int i = start_offset; i < end_offset; i = i + vert_size)
            {
                // Vertex // flip x *-1
                Point3D vert = new Point3D(BitConverter.ToSingle(data, i), BitConverter.ToSingle(data, i + 4), BitConverter.ToSingle(data, i + 8)); // (Point3D)Parsing.brReadPoint3D(block, 3, i);
                mesh_data.vertices.Add(vert);


                // Normals
                if (norm_offset != -1)
                {
                    Vector3D normal = new Vector3D(BitConverter.ToSingle(data, i + norm_offset), BitConverter.ToSingle(data, i + norm_offset + 4), BitConverter.ToSingle(data, i + norm_offset + 8));  // Parsing.brReadVector3D(block, i + norm_offset);
                    mesh_data.normals.Add(normal);
                }

                // UVs
                if (uv_offset != -1)
                {
                    Single v = BitConverter.ToSingle(data, i + uv_offset + 4); //BitConverter.ToSingle(data, i + uv_offset + 4);
                    /*
                    // I can't even remember what this is for.....
                    if (vert_size <= 32)
                    {
                        v = v * -1;
                    }
                    */

                    // flip vertically
                    //Single v = (-1 * (BitConverter.ToSingle(data, i + uv_offset + 4))) + 1; //BitConverter.ToSingle(data, i + uv_offset + 4);
                    //Single v_flipped = (-1 * v) + 1;
                    //(-1 * (BitConverter.ToSingle(data, i + uv_offset + 4))) + 1)
                    System.Windows.Point uv = new System.Windows.Point(BitConverter.ToSingle(data, i + uv_offset), v * -1f);  // Parsing.brReadVector3D(block, i + norm_offset);
                    mesh_data.UVs.Add(uv);
                }
                // vertex weights
                if (vert_size > 32)
                {
                    // bone ids
                    mesh_data.bone_weights.Add(new MeshData.bone_weight(data[i + bone_ids_offset], data[i + bone_ids_offset + 1], BitConverter.ToSingle(data, i + bone_weights_offset), 1 - BitConverter.ToSingle(data, i + bone_weights_offset)));
                }
                else
                {
                    // static mesh assign vert weights to bone 0
                    mesh_data.bone_weights.Add(new MeshData.bone_weight(0, 0, 1, 0));
                }

            }

            #endregion

            // reads triangle indices data and fills mesh_data Int32Collection
            #region GET TRIANGLE DATA

            List<Int16> triangles_strips = new List<Int16>();

            for (int i = tris_start; i < tris_end; i += 2)
            {
                mesh_data.triangles.Add(BitConverter.ToInt16(data, i));
                triangles_strips.Add(BitConverter.ToInt16(data, i));
            }

            // if stripped convert back to triangle list
            if (current_model.VertexBlock_header_List[part_num].stripped_triangles == 1)
            {
                Int32Collection triangles_list = new Int32Collection();

                int num_tris = triangles_strips.Count; // - 2;
                int num_list_indices = num_tris * 3;

                // re-order stripped triangles
                for (int c = 0; c < triangles_strips.Count - 2; c++)
                {

                    if (c % 2 == 0)
                    {
                        triangles_list.Add(triangles_strips[c]);
                        triangles_list.Add(triangles_strips[c + 1]);
                        triangles_list.Add(triangles_strips[c + 2]);
                    }
                    else
                    {
                        triangles_list.Add(triangles_strips[c + 2]);
                        triangles_list.Add(triangles_strips[c + 1]);
                        triangles_list.Add(triangles_strips[c]);
                    }
                }

                mesh_data.triangles = triangles_list;
            }

            #endregion


            #endregion

            #region write SMD

            List<String> nodes = new List<String>();
            List<String> skeleton = new List<String>();
            List<String> triangles = new List<String>();

            // determine which type of model this is (shadow, bone, part)
            string mdl_type_prefix = "p_";


            // for each model part
            for (int i = 0; i < current_model.Model_Parts_header_List.Count -1; i++)
            {
                MDLB.Model_Part_header mp = current_model.Model_Parts_header_List[i];
                // if first node
                if (i == 0)
                {
                    nodes.Add(" " + i + " " + mdl_type_prefix + i + " " + -1);
                }
                else
                {
                    nodes.Add(" " + i + " " + mdl_type_prefix + i + " " + mp.real_parent_id);
                }


                Vector3 pos = mp.bone_pos;
                // recalculate bone position (substract bone.pos by parent_bone.pos)
                if (i > 0)
                {
                    if (mp.real_parent_id > -1)
                    {
                        // get parent bone position
                        Vector3 vpp = current_model.Model_Parts_header_List[mp.real_parent_id].bone_pos;
                        // substract parent position // invert X pos  *-1f
                        pos = new Vector3((pos.X - vpp.X), pos.Y - vpp.Y, pos.Z - vpp.Z);
                    }
                }

                skeleton.Add(" " + i + " " + pos.X + " " + pos.Y + " " + pos.Z + " " + 0 + " " + 0 + " " + 0);
            }

            // if model only has one part (static mesh)
            if (current_model.Model_Parts_header_List.Count == 1)
            {
                Vector3 pos = current_model.Model_Parts_header_List[0].bone_pos;
                nodes.Add(" " + 0 + " " + mdl_type_prefix + 0 + " " + -1);
                skeleton.Add(" " + 0 + " " + pos.X + " " + pos.Y + " " + pos.Z + " " + 0 + " " + 0 + " " + 0);
            }

            string filepath = save_dir + filename + "_" + mdl_type_prefix + part_num + ".smd";

            if (part_num == current_model.Model_Parts_header_List.Count - 1)
            {
                filepath = save_dir + filename + ".smd";
            }

            // write file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath))
            {
                // make sure we export floats with the decimal represented by a dot and not a comma
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

                file.WriteLine("version 1");

                #region nodes

                file.WriteLine("nodes");
                for (int i = 0; i < nodes.Count; i++)
                {
                    file.WriteLine(nodes[i]);
                }

                file.WriteLine("end");

                #endregion

                #region skeleton

                file.WriteLine("skeleton");

                file.WriteLine("time 0");

                for (int i = 0; i < skeleton.Count; i++)
                {
                    file.WriteLine(skeleton[i]);
                }

                file.WriteLine("end");
                #endregion

                #region triangles

                file.WriteLine("triangles");

                string bone_weights;
                string mat_name = "mat_0";


                for (int i = 0; i < mesh_data.triangles.Count; i += 3)
                {



                    #region determine material group

                    // for each triangle group
                    for (int g = 0; g < current_model.Model_Parts_header_List[part_num].triangle_groups_List.Count; g++)
                    {
                        MDLB.triangle_group tg = current_model.Model_Parts_header_List[part_num].triangle_groups_List[g];

                        // if triangle index <= triangle group size
                        if (i / 3 <= (tg.triangle_start_index / 3 ) + (tg.triangle_group_size - 1))
                        {
                            // set material name to triangle's group material index
                            mat_name = "mat_" + tg.material_index; //g.ToString();
                            break;
                        } else {
                            // keep the same material name if we're still inside the triangle group
                           
                            continue;
                        }
                    }

                    #endregion



                    file.WriteLine(mat_name);

                    Point3D v = mesh_data.vertices[mesh_data.triangles[i]];
                    System.Windows.Point uv = new System.Windows.Point();
                    if (mesh_data.UVs.Count > 0)
                    {
                        uv = mesh_data.UVs[mesh_data.triangles[i]];
                    }

                    Vector3D n = new Vector3D();
                    MeshData.bone_weight bw;


                    // write triangle's vertices (3)
                    for (int t = 0; t < 3; t++)
                    {
                        if (i + t >= mesh_data.triangles.Count) { break; }
                        v = mesh_data.vertices[mesh_data.triangles[i + t]];

                        if (mesh_data.UVs.Count > 0)
                        {
                            uv = mesh_data.UVs[mesh_data.triangles[i + t]];
                        }

                        // if model has bone weights
                        if (mesh_data.bone_weights.Count > 0)
                        {
                            bw = mesh_data.bone_weights[mesh_data.triangles[i + t]];
                        }
                        else
                        { // if not, assign vert weights to bone 0
                            bw = new MeshData.bone_weight(0, 0, 1, 0);
                        }

                        n = new Vector3D();
                        // if model has normals
                        if (mesh_data.normals.Count > 0) { n = mesh_data.normals[mesh_data.triangles[i + t]]; }

                        // if bone weight is < 1 then second weight = bw.bone_0_weight -1 (this was already calculated when storing the weights)
                        if (bw.bone_0_weight != 1)
                        {
                            bone_weights = "2 " + bw.bone_id_0 + " " + bw.bone_0_weight + " " + bw.bone_id_1 + " " + bw.bone_1_weight;
                        }
                        else
                        {
                            bone_weights = "1 " + bw.bone_id_0 + " " + bw.bone_0_weight;
                        }

                        file.WriteLine("0 " + Float_toString(v.X) + " " + Float_toString(v.Y) + " " + Float_toString(v.Z) + " " + Float_toString(n.X) + " " + Float_toString(n.Y) + " " + Float_toString(n.Z) + " " + Float_toString(uv.X) + " " + Float_toString(uv.Y) + " " + bone_weights); //2 deformers bone_id_0 weight_0 bone_id_1 weight_1+ "1 0 1"
                    }

                }

                file.WriteLine("end");

                #endregion
            }

            #endregion
        }



        // export block of data to binary file (.dat file)
        private void Btn_extract_block_Click(object sender, EventArgs e)
        {
            // if node is a container, ignore
            if (current_node == null || current_node.Level <= 0)
            {
                MessageBox.Show("Select an item to export first.");
                return;
            }

            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog1.Title = "Save item's data block";
            saveFileDialog1.RestoreDirectory = true;

            string filename = (Path.GetFileName(jsrf_file.filepath)).Replace(".dat", "_dat").Replace(".bin", "_bin");
            string nodeName = Regex.Replace(current_node.Text, @"\s+", "_");

            saveFileDialog1.FileName = filename + "_" + nodeName.TrimStart('_');
            saveFileDialog1.DefaultExt = "dat";
            saveFileDialog1.Filter = "Binary file (*.dat)|*.dat";
            saveFileDialog1.FilterIndex = 2;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    MessageBox.Show("Invalid file name.");
                    return;
                }

                // write file
                File.WriteAllBytes(saveFileDialog1.FileName, current_item_data);
            }
        }

        // import block of data from binary file (.dat file)
        private void Btn_import_block_Click(object sender, EventArgs e)
        {
            if (trv_file.SelectedNode == null || trv_file.SelectedNode.Index == -1 || trv_file.SelectedNode.Nodes.Count != 0)
            {
                MessageBox.Show("Select an item in the list.");
                return;
            }

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "dat files (*.dat)|*.dat";
            dialog.RestoreDirectory = true;
            dialog.Title = "Select a dat file";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                byte[] array = File.ReadAllBytes(dialog.FileName);

                jsrf_file.set_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, array);
                Rebuild_file(true, true, true);
            }
        }



        private double ToRad(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        // convert float to string, change coma to dot
        private string Float_toString(double f)
        {
            // truncate to 6 decimal places after comma and convert to string
            return (Math.Truncate(f * 1000000f) / 1000000f).ToString(); //"n6", CultureInfo.CreateSpecificCulture("en-US"))
        }

        #endregion


        #region texture editing


        // export texture to custom filepath
        private void btn_save_texture_file_Click(object sender, EventArgs e)
        {
            #region check if texture item is selected and valid

            File_Containers.item_data_type selected_node_type = File_Containers.get_item_data_type(current_item_data);

            if (selected_node_type != File_Containers.item_data_type.Texture)
            {
                MessageBox.Show("Select a texture item first.");
                return;
            }

            string texture_id = "";

            if (selected_node_type == File_Containers.item_data_type.Texture)
            {
                //texture_id = Load_block_Texture(current_item_data, true, true);
                texture_id = BitConverter.ToInt32(current_item_data, 0).ToString();

                if (texture_id == "")
                {
                    MessageBox.Show("Could not read texture.");
                    return;
                }

                /*
                if (!File.Exists(tmp_dir + texture_id + ".png"))
                {
                    MessageBox.Show("Could not extract texture: ID " + texture_id);
                    return;
                }
                */
            }

            #endregion

            string texture_path = tmp_dir + "tmp" + ".png"; //GetShortPath()
            #region setup Save File Dialog

            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog1.Title = "Save texture";
            //saveFileDialog1.RestoreDirectory = true;

            string filename = Path.GetFileNameWithoutExtension(jsrf_file.filepath);
            string nodeName = Regex.Replace(current_node.Text, @"\s+", "_");

            saveFileDialog1.FileName = filename + "_" + texture_id;
            saveFileDialog1.DefaultExt = "png";
            saveFileDialog1.Filter = "PNG file (*.png)|*.png";
            saveFileDialog1.FilterIndex = 2;

            // restore previous saved directory
            string export_dir = Setting_load("tex_export_dir");
            if (Directory.Exists(export_dir)) { saveFileDialog1.InitialDirectory = export_dir; }

            #endregion

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    MessageBox.Show("Invalid file name.");
                    return;
                }

                try
                {
                    Setting_save(Path.GetDirectoryName(saveFileDialog1.FileName) + "\\", "tex_export_dir");

                    File.Copy(texture_path, saveFileDialog1.FileName, true);

                }
                catch (Exception err)
                {
                    MessageBox.Show("Error: could not copy texture file.\n\n" + err.Message);
                    return;
                }
            }
        }

        // import texture from file
        private void btn_import_texture_file_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "PNG file (*.PNG)|*.PNG";
            //dialog.RestoreDirectory = true;
            dialog.Title = "Select a PNG file";
            dialog.Multiselect = false;

            // restore previous saved directory
            string export_dir = Setting_load("tex_import_dir");
            if (Directory.Exists(export_dir)) { dialog.InitialDirectory = export_dir; }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(dialog.FileName)) { MessageBox.Show("Error: file does not exist."); return; }

                Setting_save(Path.GetDirectoryName(dialog.FileName) + "\\", "tex_import_dir");

                save_texture(dialog.FileName);
            }
        }


        private void Btn_edit_texture_Click(object sender, EventArgs e)
        {

            if (!File.Exists(txtb_img_editor_path.Text))
            {
                MessageBox.Show("You need setup the image editor in the settings tab to edit a texture.");
                return;
            }

            #region get texture id from last loaded node data

            File_Containers.item_data_type selected_node_type = File_Containers.get_item_data_type(current_item_data);

            if (selected_node_type != File_Containers.item_data_type.Texture)
            {
                MessageBox.Show("Select a texture item.");
                return;
            }

            string texture_id = "";

            if (selected_node_type == File_Containers.item_data_type.Texture)
            {
                texture_id = Load_block_Texture(current_item_data, true, true);

                if (texture_id == "")
                {
                    MessageBox.Show("Could not read texture.");
                    return;
                }

                if (!File.Exists(tmp_dir + texture_id + ".png"))
                {
                    MessageBox.Show("Could not extract texture: ID " + texture_id);
                    return;
                }
            }

            #endregion

            string texture_path = GetShortPath(tmp_dir + texture_id + ".png");

           
            /*
            // check if image editor is active and texture is is defined, else write bmp file
            if ((proc_ImgEditor != null) && (texture_id != ""))
            {
                try
                {
                    StreamWriter myStreamWriter = proc_ImgEditor.StandardInput;
                    myStreamWriter.WriteLine(texture_path);
                }
                catch
                {
                }
            }
            */
           // MessageBox.Show(texture_path);

            //  wait(1000);
            //if(proc_ImgEditor == null)
            Launch_image_editor(texture_path);
        }

        private void wait(int milliseconds)
        {
            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            if (milliseconds == 0 || milliseconds < 0) return;
            timer1.Interval = milliseconds;
            timer1.Enabled = true;
            timer1.Start();
            timer1.Tick += (s, e) =>
            {
                timer1.Enabled = false;
                timer1.Stop();
            };
            while (timer1.Enabled)
            {
                Application.DoEvents();
            }
        }

        // save texture changes: convert tmp.png to tmp.dds and import to selected texture node
        private void Btn_save_texture_edits_Click(object sender, EventArgs e)
        {
            #region get texture id from last loaded node data block

            File_Containers.item_data_type selected_node_type = File_Containers.get_item_data_type(current_item_data);

            if (selected_node_type != File_Containers.item_data_type.Texture)
            {
                MessageBox.Show("Select a texture item first.");
                return;
            }

            save_texture();
        }


        /// <summary>
        /// converts png image file to dds and saves it in current selected texture item in the jsrf file
        /// if "png_filepath" is set, png is loaded that custom path, converted to dds and saved  in current selected texture item in the jsrf file
        /// if "png_filepath" is not set, the png in tmp_dir (named with the ID of currently selected texture item) is saved in current selected texture item in the jsrf file
        /// </summary>
        /// <param name="png_filepath"> png is loaded from this custom path</param>
        private void save_texture(string png_filepath = null)
        {
            //string texture_id = "";

            // if texture is loaded from custom filepath
            if (!string.IsNullOrEmpty(png_filepath))
            {
                string png_filename = Path.GetFileNameWithoutExtension(png_filepath);

                if (!File.Exists(png_filepath))
                {
                    MessageBox.Show("Could not find image file " + png_filename + ".png " + "in:\n\n" + png_filepath);
                    return;
                }

                try
                {
                    File.Copy(png_filepath, tmp_dir + "tmp.png", true);
                } 
                catch (Exception ex)
                {
                    MessageBox.Show("Error: could not cope source file to tmp\\tmp.png\n\n" + ex.Message);
                    return;
                }
            }


            #endregion

            string[] compression_types = { "dxt1", "dxt3" };


            #region load texture header from selected block

            string dxt_compression_type = "dxt unknown";


            //Int32 unk_8 = BitConverter.ToInt32(data_block, 8);
            //Int32 unk_16 = BitConverter.ToInt32(data_block, 16);
            //Int32 resolution = BitConverter.ToInt32(data_block, 20);

            byte dxt_format = current_item_data[24]; // 5 = dxt1 | 6 = dxt3 |
            //byte unk_25 = data_block[25]; // has alpha? is a cube map? not sure, probably if has alpha
            //byte swizzled = data_block[26]; // if = 0 texture is swizzled (swizzle for xbox textures) // http:// fgiesen.wordpress.com/2011/01/17/texture-tiling-and-swizzling/ // http:// gtaforums.com/topic/213907-unswizzle-tool/#entry3172924 // http:// forum.xentax.com/viewtopic.php?t=2640
            // http:// en.wikipedia.org/wiki/Z-order_curve // morton order
            //byte unk_27 = data_block[27];
            Int16 mipmap_count = Convert.ToInt16(current_item_data[28]);

            Int32 end_padding = BitConverter.ToInt32(current_item_data, 28); // mip map count if > 0 add 8 bytes of padding at the end of file

            switch (dxt_format)
            {
                case 5:
                    dxt_compression_type = "dxt1";
                    break;

                case 6:
                    dxt_compression_type = "dxt3";
                    break;
            }


            if (!compression_types.Contains(dxt_compression_type))
            {
                MessageBox.Show("Texture format not supported.");
                return;
            }



            #endregion

            // if (end_padding > 0) { end_padding = 8; }

            // VampConvert -genmipmap=0 means generate ALL mipmaps
            // so we get it to -genmipmap=1 so it doesn't generate any (only main texture)
            if (mipmap_count == 0) { mipmap_count = 1; }

         

            #region convert tmp.png to import.dds

            string args = "-i=tmp.png -o=" + "import.dds " + " -format=" + dxt_compression_type + " -genmipmaps=" + mipmap_count; // (mipmap_count-1).ToString()


            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Application.StartupPath + "\\resources\\tmp\\",
                    FileName = Application.StartupPath + "\\resources\\tools\\VampConvert.exe",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();
            proc.Dispose();

            #endregion

            // import dds file to byte array (skip DDS header - 128 bytes)
            byte[] imported_texture = Parsing.FileToByteArray(tmp_dir + "import.dds", 128);

            #region get DDS width and height

            int height = 0;
            int width = 0;

            try
            {
                using (BinaryReader b = new BinaryReader(File.Open(tmp_dir + "import.dds", FileMode.Open)))
                {
                    b.BaseStream.Position = 12;
                    height = b.ReadInt32();
                    b.BaseStream.Position = 16;
                    width = b.ReadInt32();
                }
            }
            catch
            {
                MessageBox.Show("Error: could not read " + tmp_dir + "import.dds");
                return;
            }

            #endregion


            // END PADDING IS NOT DEFINED BY "end_padding" it may be random? depending on how the DDS was originally imported there's 0000 padding at the end in some cases, others not
            // so I just ended up making the imported texture the same size as the original one
            // the imported DDS generated by VampConvert.exe is always slightly smaller than the original (a few bytes) so the remaining bytes at the end will be zeros

            #region create texture array and add header


            int tex_size = imported_texture.Length; //data_block.Length;
            //int tex_rewrite_start_offset = data_block_start_offset;

            if (container_type == "Indexed")
            {
                tex_size += 4;
                //tex_rewrite_start_offset += 8;
            }

            byte[] new_texture = new Byte[tex_size + 32];

            // get original texture header
            byte[] texture_header = new Byte[32];
            Array.Copy(current_item_data, 0, texture_header, 0, 32);

            // rewrite width value in JSRF texture header
            byte[] wb = BitConverter.GetBytes(width);
            texture_header[20] = wb[0];
            texture_header[21] = wb[1];
            texture_header[22] = wb[2];
            texture_header[23] = wb[3];


            // copy jsrf texture header to new_texture
            Array.Copy(texture_header, 0, new_texture, 0, 32);
            // copy imported_texture data to new_texture (after 32 bytes header)
            Array.Copy(imported_texture, 0, new_texture, 32, imported_texture.Length);

            #endregion

            // copy new texture data to jsrf file data array (fdata)
            jsrf_file.set_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, new_texture);

            // rewrite current file
            jsrf_file.rebuild_file(jsrf_file.filepath);

            // reload node/block
            Load_TreeView_item(last_selected_node);

            clear_cxbx_cache();

            System.Media.SystemSounds.Beep.Play();
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

        private static string GetShortPath(string longPath)
        {
            StringBuilder shortPath = new StringBuilder(255);
            GetShortPathName(longPath, shortPath, 255);
            return shortPath.ToString();
        }

        private void Launch_image_editor(string args)
        {
            #region setup process

            proc_ImgEditor = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = tmp_dir,
                    FileName = txtb_img_editor_path.Text,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            #endregion
        
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
            proc_ImgEditor.Start();
            
        }

        #endregion


        #region file structure editor functions


        /// <summary>
        /// rebuilds JSRF file and saves it
        /// </summary>
        /// <param name="reload_file_treeview">reloads file so treeview i s also reloaded</param>
        private void Rebuild_file(bool reload_file_treeview, bool play_sound, bool expand_selection)
        {
            int x, y;
            if (trv_file.SelectedNode == null)
            {
                x =0; y = 0;
            } else {
                 x = trv_file.SelectedNode.Parent.Index;
                 y = trv_file.SelectedNode.Index;
            }


            TreeNode tn = trv_file.SelectedNode;

            jsrf_file.rebuild_file(jsrf_file.filepath);

            if (reload_file_treeview)
                Load_file(jsrf_file.filepath, true);

            // reslect modified node and expand
            if (expand_selection)
            {
                try
                {
                    // if MULT container
                    if (jsrf_file.type == File_Containers.container_types.MULT)
                    {

                        if (y >= trv_file.Nodes[0].Nodes[x].Nodes.Count)
                        {
                            y = trv_file.Nodes[0].Nodes[x].Nodes.Count - 1;
                        }

                        trv_file.SelectedNode = trv_file.Nodes[0].Nodes[x].Nodes[y];
                    }

                    // if NORM or indexed root type of container
                    if (jsrf_file.type == File_Containers.container_types.NORM || jsrf_file.type == File_Containers.container_types.indexed)
                    {
                        if (x >= trv_file.Nodes[0].Nodes[x].Nodes.Count)
                        {
                            x = trv_file.Nodes[0].Nodes[x].Nodes.Count - 1;
                        }

                        trv_file.SelectedNode = trv_file.Nodes[0].Nodes[y];
                    }

                    // expand selected node
                    trv_file.SelectedNode.Expand();

                }
                catch
                {
                }
            }

            // clear cache
            clear_cxbx_cache();

            if (play_sound)
                System.Media.SystemSounds.Beep.Play();
        }



        /// <summary>
        /// copy current selected node item data to variable block_copy_clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_copy_block_Click(object sender, EventArgs e)
        {
            if (trv_file.SelectedNode == null || trv_file.SelectedNode.Index == -1)
            {
                MessageBox.Show("Select an item in the list.");
                return;
            }

            // if its a container
            if (trv_file.SelectedNode.Nodes.Count > 0)
            {
                node_copy_clipboard = jsrf_file.get_node_items(trv_file.SelectedNode.Index);
            }

            // if its an item
            if (trv_file.SelectedNode.Nodes.Count == 0)
            {
                // allocate byte array
                block_copy_clipboard = new byte[jsrf_file.get_item_data_length(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index)];
                // copy item data to var clipboard
                block_copy_clipboard = jsrf_file.get_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index);
            }
        }

        /// <summary>
        /// pastes (overwrites) selected block by data copied in variable block_copy_clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_paste_block_Click(object sender, EventArgs e)
        {
            if (trv_file.SelectedNode == null || trv_file.SelectedNode.Index == -1)
            {
                MessageBox.Show("Select an item in the list.");
                return;
            }

            // if its a container
            if (trv_file.SelectedNode.Nodes.Count > 0)
            {
                if (node_copy_clipboard == null)
                {
                    MessageBox.Show("Clipboard is empty, select and copy a container first.");
                    return;
                }

                jsrf_file.set_node_items(trv_file.SelectedNode.Index, node_copy_clipboard);
            }


            // if its an item
            if (trv_file.SelectedNode.Nodes.Count == 0)
            {
                if (block_copy_clipboard == null)
                {
                    MessageBox.Show("Clipboard is empty, copy a block first.");
                    return;
                }
                if (block_copy_clipboard.Length == 0)
                {
                    MessageBox.Show("Clipboard is empty, copy a block first.");
                    return;
                }

                jsrf_file.set_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, block_copy_clipboard);
            }

            Rebuild_file(true, true, true);
        }

        /// <summary>
        /// insert block of data in file structure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_insert_block_Click(object sender, EventArgs e)
        {
            if (block_copy_clipboard == null)
            {
                MessageBox.Show("Clipboard is empty, copy a block first.");
                return;
            }

            if (block_copy_clipboard.Length == 0)
            {
                MessageBox.Show("Clipboard is empty, copy a block first.");
                return;
            }

            if (trv_file.SelectedNode == null || trv_file.SelectedNode.Index == -1)
            {
                MessageBox.Show("Select an item in the list.");
                return;
            }

            jsrf_file.insert_item_after(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, File_Containers.item_data_type.unknown, block_copy_clipboard);

            Rebuild_file(true, true, true);
        }


        /// <summary>
        /// empty block data in file structure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_empty_block_Click(object sender, EventArgs e)
        {
            if (trv_file.SelectedNode == null || trv_file.SelectedNode.Index == -1)
            {
                MessageBox.Show("Select an item in the list.");
                return;
            }

            // if its an item, empty its data
            if (trv_file.SelectedNode.Nodes.Count == 0)
            {
                jsrf_file.empty_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index);
            }

            // if its a container, remove all children
            if (trv_file.SelectedNode.Nodes.Count > 0)
            {
                jsrf_file.remove_child_items(trv_file.SelectedNode.Index);
            }

            Rebuild_file(true, true, true);
        }

        /// <summary>
        /// remove block of data in file structure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_remove_block_Click(object sender, EventArgs e)
        {
            if (trv_file.SelectedNode == null || trv_file.SelectedNode.Index == -1)
            {
                MessageBox.Show("Select an item in the list.");
                return;
            }

            jsrf_file.remove_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index);

            Rebuild_file(true, true, true);
        }



        #endregion

        #region misc functions


        // reset global variables and UI controls
        private void Reset_vars()
        {
            materials_dat_list.Clear();
            materials_bin_list.Clear();

            trv_file.Nodes.Clear();
            container_type = null;

            #region UI

            lab_itemSel_length.Text = "";

            // texture viewer
            pictureBox_texture_editor.Image = null;
            rtxtb_textureinfo.Text = "";

            // model viewer4
            lab_vert_count.Text = "0";
            lab_tris_count.Text = "0";
            txtb_mdl_partnum.Text = "0";
            lab_vertBlockSize.Text = "0";
            lab_triStrip.Text = "no";
            lab_mdl_DrawDist.Text = "";

            HelixModelViewer ModelViewer = new HelixModelViewer();
            HelixViewport3D view = ModelViewer.view1;
            ModelVisual3D model = new ModelVisual3D();
            view.Children.Add(model);
            elementHost_model_editor.Child = ModelViewer;

            lab_filename.Text = "no file loaded";

            #endregion
        }

        // searches textures by ID on all .dat files (except indexed ones)
        public void mass_search_texture_by_ID(string texture_id)
        {
            List<string> matches = new List<string>();

            foreach (string file in Directory.EnumerateFiles(txtb_jsrf_mod_dir.Text, "*.dat", SearchOption.AllDirectories))
            {
                if (!file.Contains(".dat")) { continue; }
                // ignore mission files (because they give out block nodes reading errors)
                if (file.Contains("mssn")) { continue; }
                Load_file(file, false);
                if (jsrf_file.type == File_Containers.container_types.MULT)
                {
                    for (int i = 0; i < jsrf_file.MULT_root.items.Count; i++)
                    {
                        File_Containers.NORM norm = jsrf_file.MULT_root.items[i];
                        for (int e = 0; e < norm.items.Count; e++)
                        {
                            if (norm.items[e].type == File_Containers.item_data_type.Texture)
                            {
                                int id = BitConverter.ToInt32(norm.items[e].data, 0);

                                if (id.ToString() == texture_id)
                                {
                                    matches.Add(Path.GetFileName(file) + "   MULT:NORM[" + i + "] Item[" + e + "]");
                                }
                            }
                        }
                    }
                }


                if (jsrf_file.type == File_Containers.container_types.NORM)
                {
                    for (int e = 0; e < jsrf_file.NORM_root.items.Count; e++)
                    {
                        if (jsrf_file.NORM_root.items[e].type == File_Containers.item_data_type.Texture)
                        {
                            int id = BitConverter.ToInt32(jsrf_file.NORM_root.items[e].data, 0);

                            if (id.ToString() == texture_id)
                            {
                                matches.Add(Path.GetFileName(file) + "   MULT: Item[" + e + "]");
                            }
                        }
                    }
                }

                if (jsrf_file.type == File_Containers.container_types.indexed)
                {

                }
            }
        }

        // deletes cache files in Cxbx cache folders so mod file changes are reloaded when restarting the game
        private void clear_cxbx_cache()
        {
            string cxbx_dir = txtb_cxbx_dir.Text;

  
            if (Directory.Exists(cxbx_dir + "EmuDisk\\"))
            {
                // clear Partition 2 to 7
                for (int i = 2; i < 8; i++)
                {
                    IO.DeleteDirectoryContent(cxbx_dir + "EmuDisk\\Partition" + i + "\\");
                }
            }

            string cxbx_roaming_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Cxbx-Reloaded\\";

            if (Directory.Exists(cxbx_roaming_dir))
            {
                for (int i = 2; i < 8; i++)
                {
                    IO.DeleteDirectoryContent(cxbx_roaming_dir + "EmuDisk\\Partition" + i + "\\");
                }
            }
        }


        #endregion


        #region Sound Editor


        private bool selected_item_is_sound()
        {
            // if node is a container, ignore
            if (current_node == null || current_node.Level <= 0)
            {
                MessageBox.Show("Select an item to export first.");
                return false;
            }

            // get item from 'jsrf_file'
            File_Containers.item item = jsrf_file.get_item(current_node.Parent.Index, current_node.Index);


            if (item.type != File_Containers.item_data_type.Sound)
            {
                MessageBox.Show("Error: selected item is not a Sound.\nPlease select a sound item.");
                return false;
            }

            return true;
        }

        #region button events

        // play sound
        private void btn_play_sound_Click(object sender, EventArgs e)
        {
            if (!selected_item_is_sound()) { return; }

            play_sound();
        }

        // export one sound file
        private void btn_export_sound_Click(object sender, EventArgs e)
        {

            if (!selected_item_is_sound()) { return; }

            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();


            saveFileDialog1.Title = "Save sound .wav file";
            //saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = lab_filename.Text.Replace(".dat", "") + "_" + current_node.Index.ToString();
            saveFileDialog1.DefaultExt = "wav";
            saveFileDialog1.Filter = "Wav file (*.wav)|*.wav";
            saveFileDialog1.FilterIndex = 2;

            // restore previous saved directory
            string export_dir = Setting_load("sound_export_dir");
            if (Directory.Exists(export_dir)) { saveFileDialog1.InitialDirectory = export_dir; }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    MessageBox.Show("Invalid file name.");
                    return;
                }

                export_sound_file(current_item_data);

                if (File.Exists(tmp_dir + "tmp.wav"))
                {
                    try
                    {

                        Setting_save(Path.GetDirectoryName(saveFileDialog1.FileName) + "\\", "sound_export_dir");

                        File.Copy(tmp_dir + "tmp.wav", saveFileDialog1.FileName, true);

                        
                    }
                    catch (Exception error)
                    {
                        throw new InvalidOperationException("(btn_export_sound)\n\nCould not delete file:" + tmp_dir + "tmp.wav" + "\n\n" + error.Message);
                    }
                }
            }
            System.Media.SystemSounds.Beep.Play();

        }

        // export all sound files
        private void btn_export_all_sounds_Click(object sender, EventArgs e)
        {
            if (trv_file.Nodes.Count != 1)
            {
                MessageBox.Show("Error: current file doesn't seem to be a sounds file.");
                return;
            }

            #region setup file save dialog

            // select export folder and main file name?
            System.Windows.Forms.SaveFileDialog saveFileDiag = new System.Windows.Forms.SaveFileDialog();
            //saveFileDialog1.InitialDirectory = @"C:\";      
            saveFileDiag.Title = "Save WAV sound files";
            //saveFileDialog1.CheckFileExists = true;
            saveFileDiag.CheckPathExists = true;
            saveFileDiag.DefaultExt = "wav";
            saveFileDiag.Filter = "WAV files (*.wav)|*.wav";
            saveFileDiag.FilterIndex = 2;
            //saveFileDiag.RestoreDirectory = true;
            saveFileDiag.FileName = "Save Here";

            // restore previous saved directory
            string export_dir = Setting_load("sound_export_dir");
            if (Directory.Exists(export_dir)) { saveFileDiag.InitialDirectory = export_dir; }

            #endregion

            // if save file dialog result is OK
            if (saveFileDiag.ShowDialog() == DialogResult.OK)
            {
                // get filepath
                string filepath = saveFileDiag.FileName;
                string save_dir = Path.GetDirectoryName(filepath) + "\\";

                #region make sure filepath and filename are valid

                // if save directory doesn't exist inform user and ask to regive input, return
                if (!Directory.Exists(save_dir))
                {
                    MessageBox.Show("Save directory doesn't exist, please select an existing folder to save the files.");
                    return;
                }


                #endregion

                Setting_save(save_dir, "sound_export_dir");


                // for each sound item in file
                for (int i = 0; i < trv_file.Nodes[0].Nodes.Count; i++)
                {
                    File_Containers.item item = jsrf_file.get_item(0, i);

                    if (item.type == File_Containers.item_data_type.Sound)
                    {
                        bool soundFileExportedSuccessfully = export_sound_file(item.data);
                        if(!soundFileExportedSuccessfully) {  return; }

                        try
                        {
                            File.Copy(tmp_dir + "tmp.wav", save_dir + lab_filename.Text.Replace(".dat", "") + "_" + i + ".wav", true);
                        } 
                        catch (Exception error)
                        {
                            throw new InvalidOperationException("(Export all sound files)\n\n Could not copy file:\n\n"+ tmp_dir + "tmp.wav" + " .\n\n" + error.Message);
                        }
                    
                    }
                }
            }

            System.Media.SystemSounds.Beep.Play();
        }

        // import sound
        private void btn_import_sound_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "WAV files (*.wav)|*.wav";
            // openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            // restore previous saved directory
            string import_dir = Setting_load("sound_import_dir");
            if (Directory.Exists(import_dir)) { openFileDialog.InitialDirectory = import_dir; }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] fileNames = openFileDialog.FileNames;

                Setting_save(Path.GetDirectoryName(openFileDialog.FileNames[0]) + "\\", "sound_import_dir");

                for (int i = 0; i < fileNames.Length; i++)
                {
                    #region parse filename and check filename validity

                    string filepath = fileNames[i];
                    string name = Path.GetFileNameWithoutExtension(filepath);
                    string[] separators = name.Split('_');
                    string numstr = name.Split('_')[separators.Length - 1];
                    int num;
                    bool isNumber = int.TryParse(numstr, out num);


                    if (!isNumber)
                    {
                        MessageBox.Show("Error: filename '" + name + "' is invalid, it should end with a number.");
                        return;
                    }

                    if (num > trv_file.Nodes[0].Nodes.Count - 1)
                    {
                        MessageBox.Show("Error: filename '" + name + "' is invalid, the ending number exceeds the number of sound items in the file.");
                        return;
                    }

                    if (separators.Length == 0)
                    {
                        MessageBox.Show("Error: filename '" + name + "' is invalid, it should be: " + lab_filename.Text.Replace(".dat", "") + "_");
                        return;
                    }



                    if (!name.StartsWith(lab_filename.Text.Replace(".dat", "") + "_"))
                    {
                        MessageBox.Show("Error: filename '" + name + "' is invalid, it should be: " + lab_filename.Text.Replace(".dat", "") + "_" + num);
                        return;
                    }

                    #endregion

                    byte[] array = File.ReadAllBytes(filepath);
                    if(BitConverter.ToInt16(array, 34) != 16 )
                    {
                        MessageBox.Show("Error: wav file " + name + ".wav is not 16 Bit,\nplease save the file as 16 bit PCM wav.");
                        return;
                    }

                    import_sound_file(filepath);

                    Thread.Sleep(Int32.Parse(txtb_sound_import_delay.Text));  

                    jsrf_file.set_item_data(0, num, File.ReadAllBytes(tmp_dir + "tmp_import.wav"));
                }

                Rebuild_file(true, true, true);
            }

            System.Media.SystemSounds.Beep.Play();
        }


        #endregion


        private void play_sound()
        {
            if (!File.Exists(tmp_dir + "tmp.wav")) { return; }

            SoundPlayer sp = new SoundPlayer(tmp_dir + "tmp.wav");
            sp.Play();
            sp.Dispose();
        }


        private bool export_sound_file(byte[] sound_data)
        {
            try
            {
                File.Delete(tmp_dir + "tmp_import.wav");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("(export_sound_file)\n\nCould not delete file:" + tmp_dir + "tmp_import.wav" + "\n\n" + e.Message);
            }


            // if audio is not encoded export raw wav
            if(BitConverter.ToInt32(sound_data, 16) == 16)
            {
                try
                {
                    File.WriteAllBytes(tmp_dir + "tmp.wav", sound_data);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("(export_sound_file)\n\nCould not write file:" + tmp_dir + "tmp.wav" + "\n\n" + e.Message);
                }

                return true;
            }


            if (File.Exists(tmp_dir + "tmp.wav"))
            {
                try
                {
                    File.Delete(tmp_dir + "tmp.wav");
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("(export_sound_file)\n\nCould not delete file:" + tmp_dir + "tmp.wav" + "\n\n" + e.Message);
                }
            }


            try
            {
                // export raw sound file to tmp directory
                File.WriteAllBytes(tmp_dir + "snd_raw.wav", sound_data);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("(export_sound_file)\n\nCould not write file:" + tmp_dir + "snd_raw.wav" + "\n\n" + e.Message);
            }

            try
            {
                Thread.Sleep(500);
                string VerbArg = "";
                if (System.Environment.OSVersion.Version.Major >= 6)
                {
                    VerbArg = "runas";
                }

                // convert sound file
                string args = "snd_raw.wav" + " " + "tmp.wav";
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = tmp_dir,
                        FileName = Application.StartupPath + "\\resources\\tools\\xbadpdec.exe", // xbadpdec XboxADPCM.exe doesn't convert some of the sound files so we use xbadpdec
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Verb = VerbArg
                    }
                };

                proc.Start();
                proc.WaitForExit();
                proc.Dispose();

                return true;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("(export_sound_file)\n\nCould not run audio conversion tool.\n\n" + e.Message);
            }
        }



        private void import_sound_file(string filepath)
        {
            if (File.Exists(tmp_dir + "tmp_import.wav")) { File.Delete(tmp_dir + "tmp_import.wav"); }

            // convert sound file
            string args = GetShortPath(filepath) + " " + "tmp_import.wav";

            string VerbArg = "";
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                VerbArg = "runas";
            }

            try
            {
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = tmp_dir,
                        FileName = Application.StartupPath + "\\resources\\tools\\XboxADPCM.exe", // xbadpdec.exe
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Verb = VerbArg
                    }
                };

                proc.Start();
                proc.WaitForExit();
                proc.Dispose();
            }
            catch (Exception error)
            {
                throw new InvalidOperationException("(import_sound_file())\n\n Could not import wav file." + " .\n\n" + error.Message);
            }
        }

        // textbox accept numbers only
        private void txtb_sound_import_delay_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        #endregion

        #region about tab

        private void linkLabel_jsrf_inside_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://jsrf-inside.blogspot.com");
        }

        private void linkLabel_github_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/neodoso/JSRF_ModTool");
        }


        #endregion


    }
}
