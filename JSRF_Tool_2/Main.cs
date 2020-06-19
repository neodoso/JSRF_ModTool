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

using JSRF_Tool_2.DataFormats;
using JSRF_Tool_2.DataFormats.JSRF;
using JSRF_Tool_2.Vector;

using System.Threading;

using HelixToolkit.Wpf;

namespace JSRF_Tool_2
{
    public partial class Main : Form
    {
        #region Declarations

        // converts long paths to short paths, for "dos" console commands
        // [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        // public static extern int GetShortPathName(
        // [MarshalAs(UnmanagedType.LPTStr)]string path,
        // [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);

        public static string startup_dir = Application.StartupPath;
        private string tmp_dir = Application.StartupPath + "\\resources\\tmp\\";

        FileExplorer fe = new FileExplorer();

        /// Dynamic parser
        /// pr.binary_to_struct() method automatically parses binary files and loads them into an instance of a class/struct
        Parsing pr = new Parsing();

        private List<DataFormats.JSRF.Material> materials_dat_list = new List<DataFormats.JSRF.Material>();
        private List<DataFormats.JSRF.Material> materials_bin_list = new List<DataFormats.JSRF.Material>();
        private List<string> textures_id_list = new List<string>();

        public JSRF_Containers jsrf_file = null;


        public string container_type = "null";

        byte[] fdata = null; // our bin or dat file loaded into a byte array

        public static DataFormats.JSRF.MDLB model;
        public static byte[] data_block; // latest loaded item/block

        // store copy/paste item block data here
        private byte[] block_copy_clipboard;
        private List<JSRF_Containers.item> node_copy_clipboard;

        // used to know if the model has already been loaded, so the model part count on the model viewer is properly updated
        bool mdlb_first_load = false;

        private static Process proc_ImgEditor;

        private List<string> settings = new List<string>();
        string settings_dir = Application.StartupPath + "/resources/";

        private string current_filepath = "";

        private TreeNode current_node;
        private TreeNode last_selected_node;

        #endregion

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Settings_load();

            // set file explorer directory
            fe.set_dir(txtb_jsrf_mod_dir.Text);
            fe.CreateTree(this.trv_explorer);

            // clear temporary directory
            Functions.IO.DeleteDirectoryContent(tmp_dir);

            if(cb_show_adv_mdlb_nfo.Checked)
            {
                panel_adv_mdl_nfo.Visible = true;
            }

#if DEBUG

                label5.Visible = true;

            string game_dir = @"C:\Users\Mike\Desktop\JSRF\game_files\files\ModOR\";

            // automatically load file
            //Load_file(game_dir + @"Stage\stg00_00.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[0];

            //Load_file(game_dir + @"Player\Bis.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1].Nodes[0];

            //Load_file(game_dir + @"StgObj\StgObj00.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[2].Nodes[0];

            //Load_file(game_dir + @"People\People01.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1].Nodes[0];

            //Load_file(game_dir + @"\Progress\Progress.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1];

            //Load_file(game_dir + @"Player\Yoyo.dat");
            // trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1].Nodes[0];

            //Load_file(game_dir + @"Disp\SprNorm.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[0].Nodes[0];

            // Load_file(game_dir + @"Stage\stg00_00.dat");
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[0];

            // Load_file(folder + @"People\People01.dat");
            //  trv_file.SelectedNode =trv_file.Nodes[0].Nodes[1].Nodes[44];


            //Load_level_bin(Parsing.FileToByteArray(game_dir + "Stage\\stg00_.bin", 0) );


#endif
        }

        /// <summary>
        /// reset global variables and UI controls
        /// </summary>
        private void Reset_vars()
        {
            materials_dat_list.Clear();
            materials_bin_list.Clear();

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

            HelixModelViewer ModelViewer = new HelixModelViewer();
            HelixViewport3D view = ModelViewer.view1;
            ModelVisual3D model = new ModelVisual3D();
            view.Children.Add(model);
            elementHost_model_editor.Child = ModelViewer;


            lab_filename.Text = "no file loaded";

            #endregion
        }

        #region Load file

        /// <summary>
        /// loads JSRF file into corresponding file structure type and returns the load file as a class object
        /// </summary>
        private object Load_file(string filepath)
        {
            Reset_vars();
            Clear_file_view();

            jsrf_file = new JSRF_Containers(filepath);


            if (filepath.Contains(".dat"))
            {
                string filepath_bin = filepath.Replace(".dat", ".bin");
                if (File.Exists(filepath_bin))
                {
                    // load bin materials into list
                    materials_bin_list = Get_materials_to_list(new JSRF_Containers(filepath_bin));
                }
            }

            Populate_file_treeview(jsrf_file);

            current_filepath = filepath;

            lab_filename.Text = Path.GetFileName(filepath);

            return jsrf_file;
        }


        /// <summary>
        /// load level binary files (i.e: Media\Stage\stg00_.bin or  Stg10_.bin  or Stg11_.bin etc)
        /// </summary>
        private void Load_level_bin(byte[] data)
        {
            DataFormats.JSRF.Level_bin level_bin = new DataFormats.JSRF.Level_bin(data);
        }

        #endregion

        #region Form Events

        // folder explorer event
        private void Trv_explorer_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (File.Exists(e.Node.FullPath.ToString()))
            {
                Load_file(e.Node.FullPath.ToString());
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
                string path = folderBrowserDialog1.SelectedPath;
                txtb_jsrf_original_dir.Text = path;
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
            Load_file(current_filepath);

            // select the node that was select before resetting the file to its original state
            try
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

            } catch
            {

            }

            System.Media.SystemSounds.Beep.Play();
        }

        // nulls out model data (developed this to remove the game HUD to take screenshots)
        private void Btn_null_model_Click(object sender, EventArgs e)
        {
            // check selected item is valid
            if (!selected_item_is_MDLB())
            {
                return;
            }

            byte[] data = Collapse_model_vertices(data_block);


            jsrf_file.set_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, data);

            // rewrite array to disk file
            jsrf_file.rebuild_file(jsrf_file.filepath);

            // reselect node to reload MDLB
            TreeNode tn = trv_file.SelectedNode;
            trv_file.SelectedNode = null;
            trv_file.SelectedNode = tn;

        }

        // import model
        private void Btn_import_mdl_Click(object sender, EventArgs e)
        {
            // check selected item is valid
            if (!selected_item_is_MDLB())
            {
                return;
            }

            #region select SMD file dialog

            string filepath = String.Empty;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "dat files (*.smd)|*.smd";
            dialog.RestoreDirectory = true;
            dialog.Title = "Select an SMD file";
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


            byte[] MDLB_import = new byte[0];

            #region import Form with "using"

            // setup import form // "using {}" is employed here because the form and its variables (such as the heavy data SMD-model)
            // will be disposed/cleared of once we exit the 'using' {} space
            using (var f = new MDLB_Import.MDLB_import_options_Form())
            {
                //MDLB_Import.MDLB_import_options_Form import_Form = new MDLB_Import.MDLB_import_options_Form();
                f.main_SMD_filepath = filepath;

                DialogResult dr = new DialogResult();
                dr = f.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    // model = f.SMD_model;
                    MDLB_import = f.MDLB_import;
                    f.Close();
                }
                else if (dr == DialogResult.Cancel)
                {
                    // f.SMD_model = null;
                    return;
                }
            }

            #endregion


            if ((MDLB_import == null) || (MDLB_import.Length == 0))
            {
                MessageBox.Show("Error could not import model");
                return;
            }

            jsrf_file.set_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, MDLB_import);
            Rebuild_file(true, true, true);

#if DEBUG
            //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1];
            //Clear_game_cache();
#endif
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
            // Save_block_Material(Int32.Parse(txtb_mat_nums.Text));
        }

        private void btn_fix_drawdist_Click(object sender, EventArgs e)
        {
            // check selected item is valid
            if (!selected_item_is_MDLB())
            {
                return;
            }

            byte[] data = fix_mdl_draw_distance(data_block);

            //byte[] data = nullify_model(data_block, 21);

            jsrf_file.set_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, data);

            // rewrite array to disk file
            jsrf_file.rebuild_file(jsrf_file.filepath);

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

                        Object prop = this.Controls.Find(arg[1], true)[0];

                        settings.Add(s);

                        if (prop != null)
                        {
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
            }

            catch (System.Exception excep)
            {
                 MessageBox.Show("Error saving settings " + excep.Message);
            }
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


        #endregion


        #region TreeView

        /// <summary>
        /// load MULT / NORM class object into the treeview
        /// </summary>
        private void Populate_file_treeview(JSRF_Containers file)
        {
            #region MULT
            if (file.type == JSRF_Containers.container_types.MULT)
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
                        JSRF_Containers.item item = file.MULT_root.items[m].items[n];

                        if (item.type == JSRF_Containers.item_data_type.MDLB) { root.Nodes[m].BackColor = System.Drawing.Color.PaleVioletRed; }
                        if (item.type == JSRF_Containers.item_data_type.Texture)
                        {
                            // set parent node color
                            root.Nodes[m].BackColor = System.Drawing.Color.SandyBrown;
                            // get texture id and add it to the list (so when loading materials we can check if an ID corresponds to a texture id)
                            textures_id_list.Add((BitConverter.ToInt32(item.data, 0)).ToString());
                        }

                        // if its a material we store it in the materials list
                        if (item.type == JSRF_Containers.item_data_type.Material)
                        {
                            root.Nodes[m].BackColor = System.Drawing.Color.Linen;
                            // file_has_materials = true; 
                            materials_dat_list.Add(new DataFormats.JSRF.Material(item.data));
                        }

                        TreeNode nChild = new TreeNode(String.Format("{0,4} {1,-14}", n.ToString(), "   " + item.type.ToString()));

                        if (item.data.Length == 0)
                        {
                            nChild = new TreeNode(String.Format("empty"));
                            // do not add empty item if option is unchecked
                        }
                        else
                        {
                            all_child_empty = false;
                        }

                        //if(item.data.Length > 0)
                        // add item
                        childNodes.Add(nChild);
                        //nNORM.Nodes.Add(nChild);
                    }

                    // if at least one of the child nodes is not empty add items
                    if (!all_child_empty)
                    {
                        nNORM.Nodes.AddRange(childNodes.ToArray());
                    }
                    else // if all child nodes have no data
                    {
                        ///nMULT.Nodes[m] = new TreeNode("NORM" + " [" + (file.items.items[m].items.Count) + "] Empty");
                    }



                }

                root.Expand();
            }

            #endregion

            #region NORM
            if (file.type == JSRF_Containers.container_types.NORM)
            {
                // cast object as MULT class
                //JSRF_Container.MULT_list MULT = (JSRF_Container.MULT_list)file;


                // Main node
                TreeNode root = new TreeNode(file.type.ToString() + " [" + (file.NORM_root.items.Count) + "]");
                trv_file.Nodes.Add(root);

                List<TreeNode> childNodes = new List<TreeNode>();

                for (int n = 0; n < file.NORM_root.items.Count; n++)
                {
                    JSRF_Containers.item item = file.NORM_root.items[n];

                    if (item.type == JSRF_Containers.item_data_type.MDLB) { root.BackColor = System.Drawing.Color.PaleVioletRed; }
                    if (item.type == JSRF_Containers.item_data_type.Texture)
                    {
                        // set parent node color
                        root.BackColor = System.Drawing.Color.SandyBrown;
                        // get texture id and add it to the list (so when loading materials we can check if an ID corresponds to a texture id)
                        textures_id_list.Add((BitConverter.ToInt32(item.data, 0)).ToString());
                    }

                    // if its a material we store it in the materials list
                    if (item.type == JSRF_Containers.item_data_type.Material)
                    {
                        root.BackColor = System.Drawing.Color.Linen;
                        // file_has_materials = true; 
                        materials_dat_list.Add(new DataFormats.JSRF.Material(item.data));
                    }

                    TreeNode nChild = new TreeNode(String.Format("{0,4} {1,-14}", n.ToString(), "   " + item.type.ToString()));

                    if (item.data.Length == 0)
                    {
                        nChild = new TreeNode(String.Format("empty"));
                        // do not add empty item if option is unchecked
                    }

                    //if(item.data.Length > 0)
                    // add item
                    childNodes.Add(nChild);
                    //nNORM.Nodes.Add(nChild);
                }

                root.Nodes.AddRange(childNodes.ToArray());
                root.Expand();
            }

            #endregion

            #region Indexed

           
            if (file.type == JSRF_Containers.container_types.indexed)
            {
                TreeNode root = new TreeNode(file.type.ToString() + " [" + (file.INDX_root.items.Count) + "]");
                trv_file.Nodes.Add(root);

                List<TreeNode> childNodes = new List<TreeNode>();

                // for each item in file.INDX_root
                for (int m = 0; m < file.INDX_root.items.Count; m++)
                {
                    JSRF_Containers.item item = file.INDX_root.items[m];
                    
                    TreeNode nChild = new TreeNode(String.Format("{0,4}",   item.type.ToString()));
                    nChild.Text = String.Format("{0,4} {1,-14}", m.ToString(), "   " + item.type.ToString());

                    if (item.data.Length == 0)
                    {
                        nChild = new TreeNode(String.Format("empty"));
                        // do not add empty item if option is unchecked
                    }
                   
                    childNodes.Add(nChild);
                }

                root.Nodes.AddRange(childNodes.ToArray());
                root.Expand();
            }
            
            #endregion

            #region old methods
            /*
#region MULT
            if (container_type == "MULT")
            {
                // cast object as MULT class
                JSRF_Container.MULT_list MULT = (JSRF_Container.MULT_list)file_struct;

                TreeNode nMULT = new TreeNode(container_type + " [" + (MULT.Count) + "]");
                trv_file.Nodes.Add(nMULT);
                // MULTs
                for (int m = 0; m < MULT.Count; m++)
                {

                    TreeNode nNORM = new TreeNode("NORM st:" + MULT[m].start.ToString() + " s:" + MULT[m].size.ToString() + " [" + (MULT[m].NORM.child_count) + "]");

                    // add NORM node to treview
                    nMULT.Nodes.Add(nNORM);

                    // loop through NORMs child
                    for (int n = 0; n < MULT[m].NORM.child_count; n++)
                    {

                        int startoffset = JSRF_Container.get_real_offset(MULT, m, n, false, container_type);
                        int endoffset = JSRF_Container.get_real_offset(MULT, m, n, true, container_type);

                        string header = "";

                        // if empty
                        if (endoffset - startoffset == 0)
                        {
                            header = "empty";

                            // if not empty
                        } else {

                            byte[] block = new byte[endoffset - startoffset];
                            Buffer.BlockCopy(fdata, startoffset, block, 0, endoffset - startoffset);

                            // get block header type
                            header = JSRF_Container.get_block_header_type(block);


                            // set parent node (NORM) color to define what type of blocks it contains
                            if (header == "Texture") { nMULT.Nodes[m].BackColor = System.Drawing.Color.SandyBrown; }
                            if (header == "MDLB") { nMULT.Nodes[m].BackColor = System.Drawing.Color.PaleVioletRed; }
                            if (header == "Material") { nMULT.Nodes[m].BackColor = System.Drawing.Color.Linen; }


                            if (header == "Texture")
                            {
                                // get texture id and add it to the list (so when loading materials we can check if an ID corresponds to a texture id)
                                textures_id_list.Add((BitConverter.ToInt32(block, 0)).ToString());
                            }

                            // if its a material we store it in the materials list
                            if (header == "Material")
                            {
                                // file_has_materials = true; 
                                materials_dat_list.Add(new DataFormats.JSRF.Material(block));
                            }

                        }

                        int size = (MULT[m].NORM.childs[n].end - MULT[m].NORM.childs[n].start);
                        //TreeNode nChild = new TreeNode(header + " " + MULT[m].NORM.childs[n].start.ToString() + " s:" + size.ToString());

                        TreeNode nChild = new TreeNode(String.Format("{0,4} {1,-14} {2, 20}", n.ToString(), "   " + header, "[" + size.ToString() + "]"));

                        // if empty don't add it but also remove it from the MULT.NORM.Child array
                        if (size == 0)
                        {
                            MULT[m].NORM.childs.RemoveAt(n);
                            MULT[m].NORM.child_count -= 1;
                            n -= 1;
                        }
                        else
                        {
                            nNORM.Nodes.Add(nChild);
                        }
                        // trv_file.Nodes.Add(nChild);
                    }


                }

                nMULT.Expand();
            }

#endregion

#region NORM
            if (container_type == "NORM")
            {
                // cast object as NORM class
                JSRF_Container.NORM_head NORM = (JSRF_Container.NORM_head)file_struct;

                TreeNode nNORM = new TreeNode(container_type + " [" + (NORM.child_count) + "]");
                trv_file.Nodes.Add(nNORM);

                // MULTs
                for (int n = 0; n < NORM.child_count; n++)
                {
                    int startoffset = JSRF_Container.get_real_offset(NORM, 0, n, false, container_type);
                    int endoffset = JSRF_Container.get_real_offset(NORM, 0, n, true, container_type);

                    string header = "";

                    // if empty
                    if (endoffset - startoffset == 0)
                    {
                        header = "empty";
                    }
                    else // if not empt, get block and check get header type
                    {
                        byte[] block = new byte[endoffset - startoffset];
                        Buffer.BlockCopy(fdata, startoffset, block, 0, endoffset - startoffset);

                        header = JSRF_Container.get_block_header_type(block);

                        // if its a material we store it in the materials list
                        if (header == "Material")
                        {
                            // file_has_materials = true;
                            materials_dat_list.Add(new DataFormats.JSRF.Material(block));
                        }

                    }

                    int size = (NORM.childs[n].end - NORM.childs[n].start);
                    // TreeNode nChild = new TreeNode(header + " " + NORM.childs[n].start.ToString() + " s:" + size.ToString());
                    //TreeNode nChild = new TreeNode(header + " " + n.ToString() + " [" + size.ToString() + "]");
                    TreeNode nChild = new TreeNode(String.Format("{0,4} {1,-14} {2, 20}", n.ToString(), "   " + header, "[" + size.ToString() + "]"));

                    // if empty don't add it but also remove it from the MULT.NORM.Child array
                    if (size == 0)
                    {
                        NORM.childs.RemoveAt(n);
                        NORM.child_count -= 1;
                        n -= 1;
                    }
                    else
                    {
                        nNORM.Nodes.Add(nChild);
                    }
                    // trv_file.Nodes.Add(nChild);
                }

                nNORM.Expand();
            }
#endregion

#region Indexed

            if (container_type == "Indexed")
            {
                // cast object as Indexed class
                JSRF_Container.Indexed_head Indexed = (JSRF_Container.Indexed_head)file_struct;

                TreeNode nIndexed = new TreeNode(container_type + " [" + (Indexed.childs.Count) + "]");
                trv_file.Nodes.Add(nIndexed);

                for (int n = 0; n < Indexed.childs.Count; n++)
                {
                    // Indexed.childs[n].
                    int startoffset = Indexed.childs[n].block_start;
                    int endoffset = Indexed.childs[n].block_end;

                    if ((endoffset < 0) || (startoffset < 0))
                    {
                        MessageBox.Show("Error while loading child block " + n + ", negative offset.");
                        return;
                    }
                    string header = "";

                    // if empty
                    if (endoffset - startoffset == 0)
                    {
                        header = "empty";
                    }
                    else  // if not empt, get block and check get header type
                    {

                        byte[] block = new byte[endoffset - startoffset];
                        Buffer.BlockCopy(fdata, startoffset, block, 0, endoffset - startoffset);

                        header = JSRF_Container.get_block_header_type(block);
                        // if its a material we store it in the materials list
                        if (header == "Material")
                        {
                            // file_has_materials = true;
                            materials_dat_list.Add(new DataFormats.JSRF.Material(block));
                        }
                    }

                    // string header = JSRF_Container.get_block_header_type(fdata, Indexed.childs[n].block_start, Indexed.childs[n].block_end);
                    // string header = "h";
                    int size = (Indexed.childs[n].block_end - Indexed.childs[n].block_start);

                    //TreeNode nChild = new TreeNode(header + " " + Indexed.childs[n].block_start.ToString() + " : " + Indexed.childs[n].block_end.ToString() + " s:" + size.ToString());
                    //TreeNode nChild = new TreeNode(header + " " + n.ToString() + " [" + size.ToString() + "]");
                    // TreeNode nChild = new TreeNode(String.Format("{0,6} {1,-14} {2, 10}",  n.ToString(), "   " +  header, "[" + size.ToString() + "]"));

                    TreeNode nChild = new TreeNode(String.Format("{0,4} {1,-14} {2, 20}", n.ToString(), "   " + header, "[" + size.ToString() + "]"));



                    if (n % 2 == 0)
                    {
                        nChild.BackColor = System.Drawing.Color.Silver;
                    }

                    // if empty don't add it but also remove it from the MULT.NORM.Child array
                    if (size == 0)
                    {
                        Indexed.childs.RemoveAt(n);
                        //  Indexed.childs.Count -= 1;
                        n -= 1;
                    }
                    else
                    {
                        nIndexed.Nodes.Add(nChild);
                    }
                }



                nIndexed.Expand();
            }

#endregion

#region Level bin

            if (container_type == "Level")
            {
                TreeNode nNORM = new TreeNode(container_type);
                trv_file.Nodes.Add(nNORM);
            }

#endregion

            */
            #endregion
        }


        private void Trv_file_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Load_TreeView_item(e.Node);
            current_node = e.Node; //e.Node
            last_selected_node = e.Node; //e.Node
        }

        private void Load_TreeView_item(TreeNode node)
        {
            // disable panels
            panel_mat_editor.Enabled = false;
            pictureBox_texture_editor.Enabled = false;
            elementHost_model_editor.Enabled = false;
            rtxtb_material.Clear();


            if (node == null) { MessageBox.Show("Error: node is null."); return; }
            if (node.Nodes == null) { MessageBox.Show("Error: node.Nodes is null."); return; }

            // if node is a container, ignore
            if (node.Nodes.Count > 0) { return; }

            // get item from jsrf_file
            JSRF_Containers.item item = jsrf_file.get_item(node.Parent.Index, node.Index);

            lab_itemSel_length.Text = "(" + (item.data.Length.ToString("# ##0") + " bytes").PadRight(10) + ")";

            // if empty
            if (item.data.Length == 0)
            {
                if (item.type != JSRF_Containers.item_data_type.empty && item.type != JSRF_Containers.item_data_type.unkown)
                {
                    MessageBox.Show("Item data is empty.");
                    return;
                }
            }

            // item data and store in global variable 
            data_block = item.data;

            switch (item.type)
            {
                // Texture
                case JSRF_Containers.item_data_type.Texture:
                    Load_block_Texture(data_block, false, false); //load_block_Texture(data_block, "tmp", false);
                    pictureBox_texture_editor.Enabled = true;
                    tabControl1.SelectedIndex = 01;
                    break;

                // MDLB Model
                case JSRF_Containers.item_data_type.MDLB:
                    mdlb_first_load = true;
                    Load_block_MDLB(data_block, 21);
                    elementHost_model_editor.Enabled = true;
                    tabControl1.SelectedIndex = 0;
                    break;

                // MDLBL Model
                case JSRF_Containers.item_data_type.MDLBL:

                    load_level_model(data_block);
                    //Load_block_MDLB(data_block, 21);
                    elementHost_model_editor.Enabled = true;
                    tabControl1.SelectedIndex = 0;
                    break;

                // Material
                case JSRF_Containers.item_data_type.Material:
                    Load_block_Material_info(item.data);
                    panel_mat_editor.Enabled = true;
                    break;
            }
        }


        // returns true if selected item is an MDLB
        private bool selected_item_is_MDLB()
        {
            // check selected node
            if (trv_file.SelectedNode == null || trv_file.SelectedNode.Index == -1 || trv_file.SelectedNode.Nodes.Count != 0)
            {
                MessageBox.Show("Select an item in the list.");
                return false;
            }

            // check if selected item is an MDLB
            JSRF_Containers.item selected_item = jsrf_file.get_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index);
            if (selected_item.type != JSRF_Containers.item_data_type.MDLB)
            {
                MessageBox.Show("Selected item is not a MDLB.");
                return false;
            }

            if (model == null) { MessageBox.Show("Model is empty or no model selected."); return false; }
            if (model.VertexBlock_header_List == null) { MessageBox.Show("Model header is null."); return false; }
            if (model.VertexBlock_header_List.Count == 0) { MessageBox.Show("Model contains 0 model parts."); return false; }

            return true;
        }



        /// <summary>
        /// clear variables of current loaded file (file treeviez, MULT instance ...)
        /// </summary>
        private void Clear_file_view()
        {
            fdata = null;
            trv_file.Nodes.Clear();
            container_type = "null";
        }

        #endregion

        #region level model

        private void load_level_model(byte[] data)
        {
            Int32 tex_count = BitConverter.ToInt32(data, 16);
            int tex_size = 4 * tex_count;

            /*
            int tex_count = BitConverter.ToInt32(data, 16);
            int draw_dist_pos = 16 + 48 + tex_count * 4;

            float draw_dist = BitConverter.ToSingle(data, draw_dist_pos);

           double result = Convert.ToDouble(draw_dist);
            long lVal = BitConverter.DoubleToInt64Bits(result);
            string hex = lVal.ToString("X");

            txtb_test.Text  = draw_dist.ToString();
            */
        }
        /*
        unsafe string ToHexString(float f)
        {
            var i = *((int*)&f);
            return "0x" + i.ToString("X8");
        }
        */
        #endregion


        #region model parsing


        /// <summary>
        /// load JSRF model (MDLB) and store vertex, uv, normals, triangles etc data into arrays to display or export the model data
        /// </summary>
        private void Load_block_MDLB(byte[] data, int mdl_partnum)
        {
            model = new DataFormats.JSRF.MDLB(data);
            lab_mdl_mat_typex.Text = "mat count: " + model.header.materials_count.ToString();

            lab_mdl_mat_vtx.Text = "mat vtx count: ";
            if (mdl_partnum == model.header.model_parts_count)
            {
                lab_mdl_mat_vtx.Text = "mat vtx count: " + model.Model_Parts_header_List[mdl_partnum-1].materials_count;
            }

            // gets material from the previous parent node or node that contains materials and has the same count of child items as the textures nodes
            // if no materials in DAT  load materials list from bin
            Int32 texture_id = Get_MDLB_texture_id_mat_list();
            List<GeometryModel3D> meshes = new List<GeometryModel3D>();
         

            // to do materials
            // how to load when no HB?
            #region model part

            // model part we want to export (models have up to 20 parts)
            // usually part 20 (or 19 since from 0 to 19) is the actual mesh
            // lower number parts often are just cubes that seem to be the animation bones
            // int part = model.VertexBlock_header_List.Count - 1;

            // if its the first time its loaded from the treeview we load the last part
            // otherwise we let the user specify what part to load from the model viewer
            if (mdlb_first_load)
            {
                // if part number higher than what the MDLB has, we load the last part
                if (mdl_partnum > model.VertexBlock_header_List.Count - 1)
                {
                    mdl_partnum = model.VertexBlock_header_List.Count - 1;
                    txtb_mdl_partnum.Text = (model.VertexBlock_header_List.Count - 1).ToString();
                }

                if (mdl_partnum < 0)
                {
                    mdl_partnum = 0;
                }
            }

            lab_mdlb_parts_count.Text = model.VertexBlock_header_List.Count.ToString();

            #endregion


            if (model.Model_Parts_header_List.Count == 0)
            {
                System.Windows.MessageBox.Show("Error: MDLB has 0 model parts.");
                return;
            }

            #region Load Header data

            // material clusters
            int mat_cluster_count = model.Model_Parts_header_List[mdl_partnum].triangle_groups_count;

            // HEADER DATA
            int vert_size = model.VertexBlock_header_List[mdl_partnum].vertex_def_size;
            int start_offset = model.VertexBlock_header_List[mdl_partnum].vertex_buffer_offset + 16;
            int vert_count = model.VertexBlock_header_List[mdl_partnum].vertex_count;

            lab_vtx_flag.Text = (model.VertexBlock_header_List[mdl_partnum].unk_flag).ToString();

            int end_offset = (vert_size * vert_count) + start_offset;

            int tris_start = model.VertexBlock_header_List[mdl_partnum].triangles_buffer_offset + 16;
            int tris_count = model.VertexBlock_header_List[mdl_partnum].triangles_count;
            end_offset = tris_start + (tris_count * 2);

            #endregion

            #region set model viewer info labels

            lab_vertBlockSize.Text = vert_size.ToString();
            lab_vert_count.Text = vert_count.ToString();
            lab_tris_count.Text = (tris_count / 3).ToString();

            string is_stripped = "no";
            if (model.VertexBlock_header_List[mdl_partnum].stripped_triangles == 1)
            {
                is_stripped = "yes";
            }

            lab_triStrip.Text = is_stripped;

            #endregion

            // read and store raw mesh data
            MeshData mesh_data = new MeshData();

            #region GET VERTEX DATA

            Point3D mesh_center = new Point3D(0, 0, 0);
            double maxX = 0, maxY = 0, maxZ = 0, minX = 0, minY = 0, minZ = 0;

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
                // mesh_data.vertices.Add(new Point3D(BitConverter.ToSingle(data, i),  BitConverter.ToSingle(data, i + 8) *-1, BitConverter.ToSingle(data, i + 4)));
                if (i + 8 > data.Length)
                {
                    MessageBox.Show("index out of range (vertex block loop)");
                    return;
                }

                double x = BitConverter.ToSingle(data, i);
                double y = BitConverter.ToSingle(data, i +4);
                double z = BitConverter.ToSingle(data, i +8);

                // flip order 
                Point3D vert = new Point3D(x*-1, z,y); //new Point3D(-x, z, y);  //// correct face position new Point3D(y, z, x);
                // add vert
                mesh_data.vertices.Add(vert);

                #region calculate mesh center and bounds

                // calculate mesh center
                mesh_center.X += vert.X;
                mesh_center.Y += vert.Y;
                mesh_center.Z += vert.Z;

                // get max/min vertice distances
                if (vert.X > maxX) { maxX = vert.X; }
                if (vert.Y > maxY) { maxY = vert.Y; }
                if (vert.Z > maxZ) { maxZ = vert.Z; }

                if (vert.X < minX) { minX = vert.X; }
                if (vert.Y < minY) { minY = vert.Y; }
                if (vert.Z < minZ) { minZ = vert.Z; }

                #endregion


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
                   // Single u = (-1 * (BitConverter.ToSingle(data, i + uv_offset + 4))) + 1;

                    mesh_data.UVs.Add(new System.Windows.Point(u, v));
                }
            }


            #region calculate mesh center, bounds, average distance
            // calculate mesh center
            mesh_center.X = mesh_center.X / mesh_data.vertices.Count;
            mesh_center.Y = mesh_center.Y / mesh_data.vertices.Count;
            mesh_center.Z = mesh_center.Z / mesh_data.vertices.Count;

            // mesh_center = new Point3D(BitConverter.ToSingle(data, start_offset), -BitConverter.ToSingle(data, start_offset + 8), BitConverter.ToSingle(data, start_offset + 4));

            mesh_data.mesh_center = mesh_center;
            mesh_data.avg_distance = new Point3D(Math.Abs((Math.Abs(minX) + maxX)), Math.Abs((Math.Abs(minY) + maxY)), Math.Abs((Math.Abs(minZ) + maxZ)));// Math.Abs((Math.Abs(minX) + maxX)) + Math.Abs((Math.Abs(minY) + maxY)) + Math.Abs((Math.Abs(minZ) + maxZ));
            mesh_data.mesh_bounds = new Point3D(minX + maxX, minY + maxY, minZ + maxZ);

            #endregion

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
            if (model.VertexBlock_header_List[mdl_partnum].stripped_triangles == 1)
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
            if (model.Model_Parts_header_List[mdl_partnum].triangle_groups_List.Count > 0)
            {
                MDLB.triangle_group tg = model.Model_Parts_header_List[mdl_partnum].triangle_groups_List[0];
                if(model.materials_List.Count > 0)
                {       
                    MDLB.color color = model.materials_List[tg.material_index].color;
                    clr.R = color.R; clr.G = color.G; clr.B = color.B;
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
            if (File.Exists(tmp_dir + mesh_data.texture_id + ".bmp"))
            {
                ImageBrush image_brush = new ImageBrush();
                byte[] buffer = System.IO.File.ReadAllBytes((tmp_dir + mesh_data.texture_id + ".bmp"));
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
            model = new DataFormats.JSRF.MDLB(data);

            // for each model part
            for (int i = 0; i < model.VertexBlock_header_List.Count; i++)
            {
                #region get header data needed for model part

                int vert_size = model.VertexBlock_header_List[i].vertex_def_size;
                int start_offset = model.VertexBlock_header_List[i].vertex_buffer_offset + 16;
                int vert_count = model.VertexBlock_header_List[i].vertex_count;
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

            model = new DataFormats.JSRF.MDLB(data);
            int part = mdl_partnum;

            // if its the first time its loaded from the treeview we load the last part
            // otherwise we let the user specify what part to load from the model viewer
            if (mdlb_first_load)
            {
                // if part number higher than what the MDLB has, we load the last part
                if (mdl_partnum > model.VertexBlock_header_List.Count - 1)
                {
                    part = model.VertexBlock_header_List.Count - 1;
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

            model = new DataFormats.JSRF.MDLB(data);
            int part_num = mdl_partnum;

            // if its the first time its loaded from the treeview we load the last part
            // otherwise we let the user specify what part to load from the model viewer
            if (mdlb_first_load)
            {
                // if part number higher than what the MDLB has, we load the last part
                if (mdl_partnum > model.VertexBlock_header_List.Count - 1)
                {
                    part_num = model.VertexBlock_header_List.Count - 1;
                    //txtb_mdl_partnum.Text = (model.VertexBlock_header_List.Count - 1).ToString();
                }

                if (mdl_partnum < 0)
                {
                    part_num = 0;
                }
            }


            int tris_group_count = model.Model_Parts_header_List[part_num].triangle_groups_count;

            for (int i = 0; i < tris_group_count; i++)
            {
                // make triangle group count = 0
                int tris_group_offset = model.Model_Parts_header_List[part_num].triangle_groups_list_offset + i * 32;

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
            model = new DataFormats.JSRF.MDLB(data);

            int drawdistY_offset = 32 + ((model.VertexBlock_header_List.Count - 1) * 128) + 4;

            // set y draw distance to 8.5
            Buffer.BlockCopy(BitConverter.GetBytes(-1.1957052f), 0, data, drawdistY_offset, 4);

            Buffer.BlockCopy(BitConverter.GetBytes(13.355667f), 0, data, drawdistY_offset + 8, 4);

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

            DirectionalLight light1 = new DirectionalLight();
            // light1.Color = Color.FromArgb(255, 100, 100, 100);
            light1.Direction = new Vector3D(-0.61, 0.5, 0.61);
            lights_visual.Content = light1;

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

            cam.Position = new Point3D(mesh_center.X + avg_distance.X, mesh_center.Y + avg_distance.Y, mesh_center.Z + avg_distance.Z);
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
            if (!selected_item_is_MDLB())
            {
                return;
            }

            int num = Convert.ToInt16(txtb_mdl_partnum.Text);

            num += 1;

            txtb_mdl_partnum.Text = num.ToString();

            Load_block_MDLB(data_block, num);
        }
        // switch model part -=
        private void Btn_mdl_partMin_Click(object sender, EventArgs e)
        {
            // check selected item is valid
            if (!selected_item_is_MDLB())
            {
                return;
            }

            int num = Convert.ToInt16(txtb_mdl_partnum.Text);

            if (num > 0)
            {
                num -= 1;

                txtb_mdl_partnum.Text = num.ToString();

                Load_block_MDLB(data_block, num - 1);
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
            string dxt_compression_type = "dxt unknown";

            // #region read texture header

            Int32 id = BitConverter.ToInt32(data, 0);
            Int32 unk_8 = BitConverter.ToInt32(data, 8);
            Int32 unk_16 = BitConverter.ToInt32(data, 16);
            Int32 res_x = BitConverter.ToInt32(data, 20);
            Int32 res_y = BitConverter.ToInt32(data, 24);

            byte dxt_format = data[24]; // 5 = dxt1 | 6 = dxt3 |
            byte unk_25 = data[25]; // has alpha? is a cube map? not sure, probably if has alpha
            byte swizzled = data[26]; // if = 0 texture is swizzled (swizzle for xbox textures) // http:// fgiesen.wordpress.com/2011/01/17/texture-tiling-and-swizzling/ // http:// gtaforums.com/topic/213907-unswizzle-tool/#entry3172924 // http:// forum.xentax.com/viewtopic.php?t=2640
            // http:// en.wikipedia.org/wiki/Z-order_curve // morton order
            byte unk_27 = data[27];
            Int16 mipmap_count = Convert.ToInt16(data_block[28]);

            Int32 end_padding = BitConverter.ToInt32(data, 28); // mip map count if > 0 add 8 bytes of padding at the end of file

            switch (dxt_format)
            {
                case 5:
                    dxt_compression_type = "dxt1";
                    break;

                case 6:
                    dxt_compression_type = "dxt3";
                    break;
                // if unknown type exit
                default:
                    return "";
            }

            // /TODO remove data header
            byte[] data_noheader = new byte[data.Length - 32];
            System.Buffer.BlockCopy(data, 32, data_noheader, 0, data_noheader.Length);// /

            byte[] dds_header = Generate_dds_header(res_x, dxt_compression_type);
            byte[] texture_file = new byte[dds_header.Length + data_noheader.Length + 32];
            System.Buffer.BlockCopy(dds_header, 0, texture_file, 0, dds_header.Length);
            System.Buffer.BlockCopy(data_noheader, 0, texture_file, dds_header.Length, data_noheader.Length);

            string filename = "tmp";

            if (by_id) { filename = id.ToString(); }

            Parsing.ByteArrayToFile(tmp_dir + "\\" + filename + ".dds", texture_file);

            #region convert dds to bmp


            string args = "-i=" + filename + ".dds -o=" + filename + ".bmp -genmipmaps=1";

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

            if (!File.Exists(tmp_dir + filename + ".bmp"))
            {
                MessageBox.Show("Could not load texture file: \n" + tmp_dir + filename + ".bmp");
                return "";
            }

            #endregion

            // if not in silent mode: load bitmap into picturebox and texture info in textbox
            if (!silent)
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
                if(swizzled == 1)
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


                if (res_x > 512)
                {
                    pictureBox_texture_editor.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    pictureBox_texture_editor.SizeMode = PictureBoxSizeMode.CenterImage;
                }

                Stream BitmapStream = System.IO.File.Open(tmp_dir + filename + ".bmp", System.IO.FileMode.Open);
                Image imgPhoto = Image.FromStream(BitmapStream, true);

                BitmapStream.Dispose();
                BitmapStream.Close();

                Image bmp = new Bitmap(imgPhoto);
                pictureBox_texture_editor.Image = bmp;
            }

            return id.ToString();
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
                rtxtb_material.AppendText(BitConverter.ToInt32(data, pos).ToString() + Environment.NewLine );

                pos += 4;
            }

            tabControl1.SelectedIndex = 2;
        }



        /// <summary>
        /// saves currently selected material block data (and changes applied through Texture editor tab)
        /// </summary>
        private void Save_block_Material(int mat_count)
        {

            /*
            if (data_block == null) { MessageBox.Show("Material is empty or no material selected."); return; }
            if (data_block.Length == 0) { MessageBox.Show("Material data is empty."); return; }

             data_block = new byte[4 + (Int32.Parse(txtb_mat_nums.Text) * 4)];
            //Buffer.BlockCopy(data, 0, fdata, 0, data.Length);

            // get material info from textboxes and set into array data_block
            Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(txtb_mat_nums.Text)), 0, data_block, 0, 4);

           // if (mat_count >= 1)
           // {
                Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(txtb_texture_id.Text)), 0, data_block, 4, 4);
           // }
           // if (mat_count >= 2)
           // {
                Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(txtb_mat_id_0.Text)), 0, data_block, 8, 4);
           // }

           // if (mat_count >= 3)
           // {
                Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(txtb_mat_id_1.Text)), 0, data_block, 12, 4);
           // }

           // if(mat_count  == 4)
            //{
                //Buffer.BlockCopy(BitConverter.GetBytes(Int32.Parse(txtb_mat_id_2.Text)), 0, data_block, 16, 4);
            //}
            

            
            // rewrite file array
            int end = JSRF_Container.get_real_offset(jsrf_file, current_node.Parent.Index, current_node.Index, true, container_type);
            int start = JSRF_Container.get_real_offset(jsrf_file, current_node.Parent.Index, current_node.Index, false, container_type);
            // rewrite file
            //File.WriteAllBytes(current_filepath, fdata);

            // rewrite block into file
            jsrf_file.set_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, data_block);
            Rebuild_file(true, true, true);

           */
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
            List<JSRF_Containers.item_match> matches = jsrf_file.find_items_ofType(JSRF_Containers.item_data_type.Texture);

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
        private List<DataFormats.JSRF.Material> Get_materials_to_list(JSRF_Containers file)
        {
            if (!file.has_items()) { return new List<DataFormats.JSRF.Material>(); }

            List<DataFormats.JSRF.Material> materials_list = new List<DataFormats.JSRF.Material>();

            // find material items
            List<JSRF_Containers.item_match> matches = file.find_items_ofType(JSRF_Containers.item_data_type.Material);

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
            if (!selected_item_is_MDLB())
            {
                return;
            }

            #region setup file save dialog

            // select export folder and main file name?
            SaveFileDialog saveFileDiag = new SaveFileDialog();
            //saveFileDialog1.InitialDirectory = @"C:\";      
            saveFileDiag.Title = "Export model as SMD files";
            //saveFileDialog1.CheckFileExists = true;
            saveFileDiag.CheckPathExists = true;
            saveFileDiag.DefaultExt = "smd";
            saveFileDiag.Filter = "SMD files (*.smd)|*.smd";
            saveFileDiag.FilterIndex = 2;
            saveFileDiag.RestoreDirectory = true;

            string export_folder = String.Empty;
            string file_name = String.Empty;

            #endregion

            // if save file dialog result is OK
            if (saveFileDiag.ShowDialog() == DialogResult.OK)
            {
                // get filepath
                string filepath = saveFileDiag.FileName;
                string save_dir = Path.GetDirectoryName(filepath)  +"\\";
                string filename = Path.GetFileNameWithoutExtension(filepath);

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
                for (int i = 0; i < model.VertexBlock_header_List.Count; i++)
                {
                    Export_model_part(save_dir, filename, i);
                }
            }
        }


        // export model part
        private void Export_model_part(string save_dir, string filename, int part_num)
        {

#if DEBUG

            if(part_num >= 20)
            {
                string stop = "";
            }

#endif

            if (model == null)
            {
                MessageBox.Show("export_model_part(" + part_num + ") Error, could not export model part (model is null)");
                return;
            }

            #region get & parse model data

            byte[] data = data_block;

            // TODO split mesh by material cluster
            // right now we only read the first cluster and get the material number (number for the texture_id_list)
            int mat_cluster_count = model.Model_Parts_header_List[part_num].triangle_groups_count;

            #region Load Header data

            int vert_size = model.VertexBlock_header_List[part_num].vertex_def_size;
            int start_offset = model.VertexBlock_header_List[part_num].vertex_buffer_offset + 16;
            int vert_count = model.VertexBlock_header_List[part_num].vertex_count;
            int end_offset = (vert_size * vert_count) + start_offset;

            int tris_start = model.VertexBlock_header_List[part_num].triangles_buffer_offset + 16;
            int tris_count = model.VertexBlock_header_List[part_num].triangles_count;
            int tris_end = tris_start + (tris_count * 2);

            #endregion

            lab_triStrip.Text = model.VertexBlock_header_List[part_num].stripped_triangles.ToString();

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
                    System.Windows.Point uv = new System.Windows.Point(BitConverter.ToSingle(data, i + uv_offset), v * -1);  // Parsing.brReadVector3D(block, i + norm_offset);
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
            if (model.VertexBlock_header_List[part_num].stripped_triangles == 1)
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
            for (int i = 0; i < model.Model_Parts_header_List.Count -1; i++)
            {
                MDLB.Model_Part_header mp = model.Model_Parts_header_List[i];
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
                        Vector3 vpp = model.Model_Parts_header_List[mp.real_parent_id].bone_pos;
                        // substract parent position // invert X pos  *-1
                        pos = new Vector3((pos.X - vpp.X), pos.Y - vpp.Y, pos.Z - vpp.Z);
                    }
                }

                skeleton.Add(" " + i + " " + pos.X + " " + pos.Y + " " + pos.Z + " " + 0 + " " + 0 + " " + 0);
            }

            // if model only has one part (static mesh)
            if (model.Model_Parts_header_List.Count == 1)
            {
                Vector3 pos = model.Model_Parts_header_List[0].bone_pos;
                nodes.Add(" " + 0 + " " + mdl_type_prefix + 0 + " " + -1);
                skeleton.Add(" " + 0 + " " + pos.X + " " + pos.Y + " " + pos.Z + " " + 0 + " " + 0 + " " + 0);
            }

            string filepath = save_dir + filename + "_" + mdl_type_prefix + part_num + ".smd";

            if (part_num == model.Model_Parts_header_List.Count - 1)
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

                string mat_name = "mat";


                for (int i = 0; i < mesh_data.triangles.Count; i += 3)
                {
                    #region determine material group


                    for (int g = 0; g < model.Model_Parts_header_List[part_num].triangle_groups_List.Count; g++)
                    {
                        MDLB.triangle_group tg = model.Model_Parts_header_List[part_num].triangle_groups_List[g];

                        // if triangle index is over this group's then go to next
                        if (i / 3 > tg.triangle_start_index + (tg.triangle_group_size - 1))
                        {
                            continue;
                        }
                        else
                        {
                            mat_name = "mat_" + g.ToString();
                            break;

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

            // get item from jsrf_file
            //JSRF_Containers.item item = jsrf_file.get_item(current_node.Parent.Index, current_node.Index);

            System.Windows.Forms.SaveFileDialog file = new System.Windows.Forms.SaveFileDialog();
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();


            saveFileDialog1.Title = "Save item's data block";
            saveFileDialog1.RestoreDirectory = true;

            string filename = (Path.GetFileName(jsrf_file.filepath)).Replace(".dat", "_dat").Replace(".bin", "_bin");
            string nodeName = Regex.Replace(current_node.Text, @"\s+", "_");
            string rootNodeName = "";

            if (jsrf_file.type == JSRF_Containers.container_types.MULT)
            {
                rootNodeName = "MULT_";
            }

            // saveFileDialog1.FileName = filename + " " + rootNodeName + "_" + current_node.Parent.Text + "_" + nodeName.TrimStart('_');
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
                string path = file.FileName;

                File.WriteAllBytes(saveFileDialog1.FileName, data_block);
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

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "dat files (*.dat)|*.dat";
            dialog.RestoreDirectory = true;
            dialog.Title = "Select a dat file";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                byte[] array = File.ReadAllBytes(dialog.FileName);

                jsrf_file.set_item_data(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, array);
                Rebuild_file(true, true, true);

#if DEBUG

                //trv_file.SelectedNode = trv_file.Nodes[0].Nodes[1];
                //Clear_game_cache();

#endif
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

        private void Btn_edit_texture_Click(object sender, EventArgs e)
        {

            if (!File.Exists(txtb_img_editor_path.Text))
            {
                MessageBox.Show("You need setup the image editor in the settings tab to edit a texture.");
                return;
            }

            #region get texture id from last loaded node data

            string selected_node_type = JSRF_Container.get_block_header_type(data_block);

            if (selected_node_type != "Texture")
            {
                MessageBox.Show("Select a texture item.");
                return;
            }

            string texture_id = "";

            if (JSRF_Container.get_block_header_type(data_block) == "Texture")
            {
                texture_id = Load_block_Texture(data_block, true, true);

                if (texture_id == "")
                {
                    MessageBox.Show("Could not read texture.");
                    return;
                }

                if (!File.Exists(tmp_dir + texture_id + ".bmp"))
                {
                    MessageBox.Show("Could not extract texture: ID " + texture_id);
                    return;
                }
            }

            #endregion

            string texture_path = GetShortPath(tmp_dir + texture_id + ".bmp");

           
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

        // save texture changes: convert tmp.bmp to tmp.dds and import to selected texture node
        private void Btn_save_texture_edits_Click(object sender, EventArgs e)
        {

            #region get texture id from last loaded node data block

            string selected_node_type = JSRF_Container.get_block_header_type(data_block);

            if (selected_node_type != "Texture")
            {
                MessageBox.Show("Select a texture item first.");
                return;
            }

            string texture_id = "";

            if (JSRF_Container.get_block_header_type(data_block) == "Texture")
            {

                texture_id = BitConverter.ToInt32(data_block, 0).ToString();
                // texture_id = load_block_Texture(data_block, true, true);

                if (texture_id == "")
                {
                    MessageBox.Show("Could not read texture.");
                    return;
                }

                if (!File.Exists(tmp_dir + texture_id + ".bmp"))
                {
                    MessageBox.Show("Could not extract texture: ID " + texture_id);
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

            byte dxt_format = data_block[24]; // 5 = dxt1 | 6 = dxt3 |
            //byte unk_25 = data_block[25]; // has alpha? is a cube map? not sure, probably if has alpha
            //byte swizzled = data_block[26]; // if = 0 texture is swizzled (swizzle for xbox textures) // http:// fgiesen.wordpress.com/2011/01/17/texture-tiling-and-swizzling/ // http:// gtaforums.com/topic/213907-unswizzle-tool/#entry3172924 // http:// forum.xentax.com/viewtopic.php?t=2640
            // http:// en.wikipedia.org/wiki/Z-order_curve // morton order
            //byte unk_27 = data_block[27];
            Int16 mipmap_count = Convert.ToInt16(data_block[28]);

            Int32 end_padding = BitConverter.ToInt32(data_block, 28); // mip map count if > 0 add 8 bytes of padding at the end of file

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

            #region convert texture_id.bmp to import.dds
            string args = "-i=" + texture_id + ".bmp -o=" + "import.dds " + " -format=" + dxt_compression_type + " -genmipmaps=" + mipmap_count; // (mipmap_count-1).ToString()


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
            } catch {
                MessageBox.Show("Error: could not read " + tmp_dir + "import.dds");
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

            byte[] new_texture = new Byte[tex_size +32];

            // get original texture header
            byte[] texture_header = new Byte[32];
            Array.Copy(data_block, 0, texture_header, 0, 32);

            // rewrite width value in JSRF texture header
            byte[] wb = BitConverter.GetBytes(width);
            texture_header[20] = wb[0];
            texture_header[21] = wb[1];
            texture_header[22] = wb[2];
            texture_header[23] = wb[3];


            //File.WriteAllBytes("C:\\Users\\Mike\\Desktop\\jsrf_texture_header.dat", texture_header);

            // copy jsrf texture header to new_texture
            Array.Copy(texture_header, 0, new_texture, 0, 32);
            // copy imported_texture data to new_texture (after 32 bytes header)
            Array.Copy(imported_texture, 0, new_texture, 32, imported_texture.Length);

            // append header + texture data (leaves 8 bytes padding (0000 0000) at the end)
            //imported_texture.CopyTo(new_texture,  32);

            #endregion

            // copy new texture data to jsrf file data array (fdata)
            jsrf_file.set_item(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, new_texture);

            // rewrite current file
            jsrf_file.rebuild_file(jsrf_file.filepath);

            // reload node/block
            Load_TreeView_item(last_selected_node);

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
            /*
            Process p = new Process();
            p.StartInfo.FileName = GetShortPath(txtb_img_editor_path.Text);
            p.StartInfo.Arguments = args;
            p.StartInfo.WorkingDirectory = GetShortPath(Path.GetDirectoryName(txtb_img_editor_path.Text));
            p.Start();
            */

            
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
        


            //string test = Directory.GetCurrentDirectory();
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
            int x = trv_file.SelectedNode.Parent.Index;
            int y = trv_file.SelectedNode.Index;

            TreeNode tn = trv_file.SelectedNode;

            jsrf_file.rebuild_file(jsrf_file.filepath);

            if (reload_file_treeview)
                Load_file(jsrf_file.filepath);

            // reslect modified node and expand
            if (expand_selection)
            {
                try
                {
                    // if MULT container
                    if (jsrf_file.type == JSRF_Containers.container_types.MULT)
                    {

                        if (y >= trv_file.Nodes[0].Nodes[x].Nodes.Count)
                        {
                            y = trv_file.Nodes[0].Nodes[x].Nodes.Count - 1;
                        }

                        trv_file.SelectedNode = trv_file.Nodes[0].Nodes[x].Nodes[y];
                    }
                    // if NORM or indexed root type of container
                    if (jsrf_file.type == JSRF_Containers.container_types.NORM || jsrf_file.type == JSRF_Containers.container_types.indexed)
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

            //trv_file.SelectedNode = tn;

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

            jsrf_file.insert_item_after(trv_file.SelectedNode.Parent.Index, trv_file.SelectedNode.Index, JSRF_Containers.item_data_type.unkown, block_copy_clipboard);

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

    }
}
