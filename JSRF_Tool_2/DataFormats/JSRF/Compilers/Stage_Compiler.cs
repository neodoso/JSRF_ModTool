using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using JSRF_ModTool.DataFormats.JSRF;
using System.Windows.Forms;

namespace JSRF_ModTool.DataFormats.JSRF
{
    public class Stage_Compiler
    {
        /// <summary>
        /// Compiles the main Stage files i.e: stg00_.bin and StgXX_XX.dat files (.bin = collision, grind paths, spawns and .dat = visual models and textures)
        /// </summary>
        /// <param name="_source_dir">Directory where Visual, Collision and grind path data hs been exported to for compilation</param>
        /// <param name="_media_dir">Game directory where the compiler will output the compiled stage files</param>
        /// <param name="_stg_num">Name/Number of level, i.e. 'stg00' is the Garage</param>
        public void Compile(string _source_dir, string _media_dir, string _stg_num)
        {
            _stg_num = _stg_num + "_";

            string visual_dir = Path.GetFullPath(_source_dir + "\\Visual\\");
            string collision_dir = Path.GetFullPath(_source_dir + "\\Collision\\");

            int Stage_models_count = 0;

            #region check import data validity

            // check visual dir exists
            if (!Directory.Exists(visual_dir))
            {
                System.Windows.Forms.MessageBox.Show("Error: could not find 'Visual' directory in: " + _source_dir);
                return;
            }
            // check collision dir exists
            if (!Directory.Exists(collision_dir))
            {
                System.Windows.Forms.MessageBox.Show("Error: could not find 'Collision' directory in: " + _source_dir);
                return;
            }
            


            if(!Directory.Exists(visual_dir))
            {
                Directory.CreateDirectory(visual_dir);
            }
            
            string[] vis_models_dirs = Directory.GetDirectories(visual_dir);

            // CHECK VISUAL MODELS  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // if no visual model folders found
            if (vis_models_dirs.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: could not find model folders in \"Visual\" folder, please export the models in groups such as \\Visual\\ModelGroupFolder\\model_name.obj  \n\nCompile cancelled.");
                return;
            }

            
            // for each visual model folder
            for (int i = 0; i < vis_models_dirs.Length; i++)
            {
                // for each .obj file
                string[] obj_files = System.IO.Directory.GetFiles(vis_models_dirs[i], "*.obj");

                Stage_models_count+= obj_files.Length;
            }

            // if no models found
            if (Stage_models_count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: could not find any model files in \"Visual\" folder, please export the models in groups such as \\Visual\\aModelGroupFolder\\model_name.obj  \n\nCompile cancelled.");
                return;
            }

            // CHECK COLLISION MODELS  //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (!Directory.Exists(collision_dir))
            {
                Directory.CreateDirectory(collision_dir);
            }

            string[] import_coll_dirS = Directory.GetDirectories(collision_dir);

            // if no visual model folders found
            if (vis_models_dirs.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: could not find collision model folders in \"Collision\" folder, please export the models in groups such as \\Collision\\aModelGroupFolder\\model_name.obj  \n\nCompile cancelled.");
                return;
            }

            int coll_models_count = 0;
            // for each visual model folder
            for (int i = 0; i < import_coll_dirS.Length; i++)
            {
                // for each .obj file
                string[] obj_files = System.IO.Directory.GetFiles(import_coll_dirS[i], "*.obj");
                coll_models_count += obj_files.Length;
            }

            // if no models found
            if (coll_models_count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: could not find any model files in \"Collision\" folder, please export the models in groups such as \\Collision\\aModelGroupFolder\\model_name.obj  \n\nCompile cancelled.");
                return;
            }

            #endregion

            #region StageXX_XX.dat files (models, textures)

            #region visual models

            // import visual models
            Stage_Model_Compiler Stage_model_Compiler = new Stage_Model_Compiler();

            List<byte[]> Stage_Models = new List<byte[]>();

            // for each visual model folder
            for (int i = 0; i < vis_models_dirs.Length; i++)
            {
                // for each .obj file
                string[] obj_files = System.IO.Directory.GetFiles(vis_models_dirs[i], "*.obj");
                for (int o = 0; o < obj_files.Length; o++)
                {
                    // add compiled model Stage to Stage_Models byte array list
                    Stage_Models.Add(Stage_model_Compiler.build(obj_files[o]));

                    // if Stage model returned is an empty array, abort process, return
                    if (Stage_Models[Stage_Models.Count - 1].Length == 0)
                    {
                        return;
                    }
                }
            }

            #endregion

            #region textures

            // create tmp dir if it doesn't exist
            if(!Directory.Exists(System.Windows.Forms.Application.StartupPath + "\\resources\\tmp\\")) { Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\resources\\tmp\\"); }

            for (int i = 0; i < Stage_model_Compiler.textures.Count; i++)
            {
                string tex_filepath = Stage_model_Compiler.textures[i].texture_filepath;
                string tex_extension = Path.GetFileName(tex_filepath).Split('.')[1].ToLower();

                #region texture file validity checks

                // check that texture file type is bmp or png, if not throw error and cancel compilation
                if (tex_extension != "png")
                {
                    System.Windows.Forms.MessageBox.Show("Invalid texture file format, please make sure all textures are .png.\n\n" + tex_filepath + "\n\nCompile cancelled.");
                    return;
                }

                // check that texture file exists, if not throw error and cancel compilation
                if (!File.Exists(tex_filepath))
                {
                    System.Windows.Forms.MessageBox.Show("Error: cannot find texture file:\n\n" + tex_filepath);
                    return;
                }

                // check that texture is square
                int tex_width, tex_height;
                using (var fileStream = new FileStream(tex_filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var image = System.Drawing.Image.FromStream(fileStream, false, false))
                    {
                        tex_width = image.Height;
                        tex_height = image.Width;
                    }
                }

                if(tex_width != tex_height)
                {
                    System.Windows.Forms.MessageBox.Show("Error: texture must have a square resolution(i.e; 512x512).\nThe following texture resolution need to be changed to a square resolution:\n" + tex_filepath);
                    return;
                }


                // check if texture resolution is a power of two
                int[] tex_resolutions = new int[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };
                bool res_valid = false;
                for (int r = 0; r < tex_resolutions.Length; r++)
                {
                    if (tex_width == tex_resolutions[r])
                    {
                        res_valid = true;
                        break;
                    }
                }
                // if texture resolution is not a power of two
                if(!res_valid)
                {
                    System.Windows.Forms.MessageBox.Show("Error: texture resolution is invalid, must be a power of two(i.e: 128x128 or 256x256 or 512x512).\n" + tex_filepath);
                    return;
                }

                // check if texture resolution exceeds 2048
                if (tex_width > 2048)
                {
                    System.Windows.Forms.MessageBox.Show("Error: maximum texture resolution is 2048 pixels\n" + tex_filepath);
                    return;
                }

                #endregion


                #region convert texture

                // arguments for texture compilation to DDS with 'VampConvert.exe'
                // add "" for output path so the command line tool recognizes the path (even if it has spaces)
                string args = "-i=" + "\"" + tex_filepath + "\"" + " -o=" + Stage_model_Compiler.textures[i].texture_id + ".dds" + " -format=" + "dxt1" + " -genmipmaps=" + "9";


                try
                {
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
                }
                catch  (Exception e)
                {
                    DialogResult dialogResult = MessageBox.Show("Error: could not convert texture to DDS\n\nError Message: " + e.Message + "\n\nInput texture that's failing:" + tex_filepath +"\n\nCancel compilation?", "JSRF Compiler", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        return;
                    }
                }

                #endregion

                Stage_model_Compiler.textures[i].texture_filepath = System.Windows.Forms.Application.StartupPath + "\\resources\\tmp\\" + Stage_model_Compiler.textures[i].texture_id + ".dds";
            }

            #endregion

            #region generate StgXX_XX.dat file(s)

            File_Containers.INDEXED indexed_items_list = new File_Containers.INDEXED();
            File_Containers fc = new File_Containers();
            int file_part_num = 0;
            int Stage_Textures_count = 0;

            // for each stage model
            for (int i = 0; i < Stage_Models.Count; i++)
            {
                File_Containers.item item = new File_Containers.item(File_Containers.item_data_type.Stage_Model, Stage_Models[i]);
                indexed_items_list.items.Add(item);
            }

            // for each texture
            for (int j = 0; j < Stage_model_Compiler.textures.Count; j++)
            {
                // loads DDS file to array
                byte[] dds = File.ReadAllBytes(Stage_model_Compiler.textures[j].texture_filepath);
                // get texture resolution
                int res_x = BitConverter.ToInt32(dds, 12); int res_y = BitConverter.ToInt32(dds, 16);


                // TODO decide if to re-enable this warning
                /*
                if (res_x != res_y)
                {
                    System.Windows.Forms.MessageBox.Show("Error: textures must have the same scale on X and Y." +
                        "\nTexture " + Path.GetFileNameWithoutExtension(Stage_model_Compiler.textures[j].texture_filepath) + " does not have equal resolution on X Y.\n\nCompilation cancelled.");

                    return;
                }
                */

                // build the JSRF texture header
                Texture_header tex_head = new Texture_header();
                tex_head.compression_format = 5;
                tex_head.has_alpha = 5;
                tex_head.mipmap_count = 9;
                tex_head.resolution = res_x;
                tex_head.Texture_ID = Stage_model_Compiler.textures[j].texture_id;
                byte[] jtex_head = tex_head.serialize();

                byte[] dds_import = new byte[dds.Length - 96];

                // normally we'd skip 128 bytes to skip the DDS header
                // but to avoid doing two heavy array operations
                // we only remove 96, leaving 32 bytes for the jsrf texture header (jtex_head)
                Array.Copy(dds, 96, dds_import, 0, dds_import.Length);

                Array.Copy(jtex_head, 0, dds_import, 0, jtex_head.Length);

                File_Containers.item item = new File_Containers.item(File_Containers.item_data_type.Texture, dds_import);
                // add texture item to indx (indexed file container)
                indexed_items_list.items.Add(item);
                Stage_Textures_count++;
            }


            File_Containers.INDEXED list = new File_Containers.INDEXED();

            int idx = 0;
            int x = 0;
            bool completed = false;

            // pack models/textures into YY parts for each StgXX_YY.dat
            // we pack up to 55 items per dat file
            while (x < indexed_items_list.items.Count)
            {
                // loop 55 times (the maximum of items(stage models/textures/MDLB) per StgXX_YY.dat
                for (int i = 0; i < 56; i++)
                {
                    // if we reach the end of the items list, write file
                    if (i + idx >= indexed_items_list.items.Count)
                    {
                        byte[] file = fc.build_Indexed_file(list);
                        File.WriteAllBytes(_media_dir + @"Stage\" + _stg_num + "0" + file_part_num + ".dat", file);
                        completed = true;
                        break;
                    }

                    list.items.Add(indexed_items_list.items[idx + i]);

                    // filled an entire StgXX_YY.dat part file
                    if(i == 55)
                    {
                        byte[] file = fc.build_Indexed_file(list);
                        File.WriteAllBytes(_media_dir + @"Stage\" + _stg_num + "0" + file_part_num + ".dat", file);
                        file_part_num++;
                        list = new File_Containers.INDEXED();
                        idx += 55;
                        i = 0;
                        continue;
                    }
                }
                x += 55;

                if (completed)
                {
                    break;
                }
            }

            #endregion
            
            #endregion

            #region generate StgXX_.bin file
            
            Stage_bin_Compiler lvl_bin_Compiler = new Stage_bin_Compiler(Stage_models_count);

            lvl_bin_Compiler.header.Stage_Models_count = 28;  //Stage_Models_count; //shouldn't it be: Stage_Models.Count; ? //// Stage_models_count  // if set to low numbers it will crash on load (maybe this number is also defined and dependent on another file?)
            if (Stage_models_count > 28) { lvl_bin_Compiler.header.Stage_Models_count = Stage_models_count; }
            lvl_bin_Compiler.header.set_stg_dat_count(file_part_num + 1);
            lvl_bin_Compiler.build(_source_dir, _media_dir, _stg_num);
            
            #endregion

        }
    }
}
