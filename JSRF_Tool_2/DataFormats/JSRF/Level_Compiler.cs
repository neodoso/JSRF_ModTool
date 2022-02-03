using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using JSRF_ModTool.DataFormats.JSRF;

namespace JSRF_ModTool.DataFormats.JSRF
{
    class Level_Compiler
    {
        static string import_dir = @"C:\Users\Mike\Desktop\JSRF\Stg_Compiles\Stg_SkatePark\";
        static string stg_num = "stg00_";


        public void compile()
        {

            #region StageXX_XX.dat files (models, textures)

            #region visual models

            // import visual models
            Level_Model_Compiler Level_MDL_Compiler = new Level_Model_Compiler();
            string[] vis_models_dirs = Directory.GetDirectories(import_dir + "Visual\\");

            int level_models_count = 0;

            List<byte[]> Level_Models = new List<byte[]>();
            // for each visual model folder
            for (int i = 0; i < vis_models_dirs.Length; i++)
            {
                level_models_count++;
                // for each .obj file
                string[] obj_files = System.IO.Directory.GetFiles(vis_models_dirs[i], "*.obj");
                for (int o = 0; o < obj_files.Length; o++)
                {
                    // add compiled model level to Level_Models byte array list
                    Level_Models.Add(Level_MDL_Compiler.build(obj_files[o]));

                    // if level model returned is an empty array, abort process, return
                    if (Level_Models[Level_Models.Count - 1].Length == 0)
                    {
                        return;
                    }
                }
            }

            #endregion

            #region textures

            for (int i = 0; i < Level_MDL_Compiler.textures.Count; i++)
            {
                string tex_filepath = Level_MDL_Compiler.textures[i].texture_filepath;

                #region convert texture

                //string args = "-i=" + texture_id + ".png -o=" + "import.dds " + " -format=" + "dxt1" + " -genmipmaps=" + "9"; // (mipmap_count-1).ToString()
                string args = "-i=" + tex_filepath + " -o=" + Level_MDL_Compiler.textures[i].texture_id + ".dds " + " -format=" + "dxt1" + " -genmipmaps=" + "9"; // (mipmap_count-1).ToString()

                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = System.Windows.Forms.Application.StartupPath + "\\resources\\tmp\\",
                        FileName = System.Windows.Forms.Application.StartupPath + "\\resources\\tools\\VampConvert.exe",
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

                Level_MDL_Compiler.textures[i].texture_filepath = System.Windows.Forms.Application.StartupPath + "\\resources\\tmp\\" + Level_MDL_Compiler.textures[i].texture_id + ".dds";
            }

            #endregion

            #region generate StgXX_XX .dat file(s)

            File_Containers.INDEXED indx = new File_Containers.INDEXED();

            // level models
            for (int i = 0; i < Level_Models.Count; i++)
            {
                File_Containers.item item = new File_Containers.item(File_Containers.item_data_type.Level_Model, Level_Models[i]);
                indx.items.Add(item);
            }

            // textures
            for (int i = 0; i < Level_MDL_Compiler.textures.Count; i++)
            {
                // loads DDS file to array
                byte[] dds = File.ReadAllBytes(Level_MDL_Compiler.textures[i].texture_filepath);
                // get texture resolution
                int res_x = BitConverter.ToInt32(dds, 12); int res_y = BitConverter.ToInt32(dds, 16);

                if(res_x != res_y)
                {
                    System.Windows.Forms.MessageBox.Show("Error: textures must have the same scale on X and Y. " +
                        "\nTexture " + Path.GetFileNameWithoutExtension(Level_MDL_Compiler.textures[i].texture_filepath) + "does not have equal reslution on X Y.\n\nCompilation cancelled.");

                    return;
                }

                // build JSRF texture header
                Texture_header tex_head = new Texture_header();
                tex_head.compression_format = 5;
                tex_head.has_alpha = 5;
                tex_head.mipmap_count = 9;
                tex_head.resolution = res_x;
                tex_head.Texture_ID = Level_MDL_Compiler.textures[i].texture_id;
                byte[] jtex_head = tex_head.serialize();

                byte[] dds_import = new byte[dds.Length -96];
                // normally we'd skip 128 bytes to skip the DDS header
                // but to avoid doing two heavy array operations
                // we only remove 96, leaving 32 bytes for the jsrf texture header
                Array.Copy(dds, 96, dds_import, 0, dds_import.Length);

                Array.Copy(jtex_head, 0, dds_import, 0, jtex_head.Length);

                File_Containers.item item = new File_Containers.item(File_Containers.item_data_type.Texture, dds_import);
                indx.items.Add(item);
            }

            // write file
            File_Containers fc = new File_Containers();
            byte[] file = fc.build_Indexed_file(indx);
            File.WriteAllBytes(@"C:\Users\Mike\Desktop\JSRF\game_files\files\ModOR\Stage\" + stg_num + "00.dat", file);

            #endregion

            #endregion


            #region generate StgXX_.bin file
            
            Level_bin_Compiler lvl_bin_Compiler = new Level_bin_Compiler();
            lvl_bin_Compiler.header.Level_Models_count =  28; //shouldn't it be: Level_Models.Count; ? //// level_models_count  // if set to low numbers it will crash on load (maybe this number is also defined and dependent on another file)
            lvl_bin_Compiler.header.set_stg_dat_count(1);
            lvl_bin_Compiler.build(import_dir);
            
            #endregion



        }

        // for every folder in
        // for every .obj in



        // generate StageXX_XX.dat files 
        // depending on how many level modelsm MDLB and textures

        // generate StageXX_.bin
        // get total number of level models and mdlb from  StageXX_XX.dat 

        // (block00) import collision mesh

        // (block01) import grind paths

        // generate block02 spawns

    }
}
