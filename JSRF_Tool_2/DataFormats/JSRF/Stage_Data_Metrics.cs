using System;
using System.Collections.Generic;
using System.IO;

namespace JSRF_ModTool.DataFormats.JSRF
{
    internal class Stage_Data_Metrics
    {
        public List<stage_info> stages_info { get; set; }

        public int average_models_count_per_stage { get; set; }
        public int average_triangles_count_per_stage { get; set; }
        public int average_textures_count_per_stage { get; set; }

        public int max_models_count_per_stage { get; set; }
        public int max_triangles_count_per_stage { get; set; }
        public int max_textures_count_per_stage { get; set; }

        public string max_models_count_stage_number { get; set; }
        public string max_triangles_count_stage_number { get; set; }
        public string max_textures_count_stage_number { get; set; }


        /// <summary>
        /// Loads \Stage\StgXX_YY.dat files given the first (YY = 00) StgXX_YY.dat and loads all other YY.dat (StgXX_01 StgXX_02 StgXX_03 etc) files
        /// and for each file gets the statistics (triangles count, textures count etc)
        /// as well as calculating the total sum of these stats, per file, and for all the files of the Stage and the total average for each Stage
        /// </summary>
        public Stage_Data_Metrics(string stages_dir)
        {
            stages_info = new List<stage_info>();


            int f_pos = 0;
            stages_info = new List<stage_info>();
            // for every file in Stage directory (and subdirectories)
            foreach (string file in Directory.EnumerateFiles(stages_dir, "*.dat", SearchOption.AllDirectories))
            {
                string filepath = Path.GetDirectoryName(file) + "\\";
                string filename = Path.GetFileNameWithoutExtension(file);

                // skip files that are not StgXX_00 (we only pick the first StgXX_YY.dat file part (_00))
                if (!filename.ToLower().Contains("stg") || !filename.Contains("_00"))
                {
                    continue;
                }

                #region for each YY file part of StgXX_YY

                string stage_num = filename.Split('_')[0];

                stages_info.Add(new stage_info(stage_num));


                // for each stage part [i] ( StgXX_00 to StgXX_08)
                for (int i = 0; i < 9; i++)
                {
                    stages_info[stages_info.Count - 1].total_stage_file_parts += 1;

                    Int32 stage_part_num = Int32.Parse(filename.Split('_')[1]);
                    string stg_part_filepath = filepath + stage_num + "_0" + i + ".dat";

                    // if StageXX_YY.dat file part doesn't exist, break loop so we skip to the next StgXX_00.dat
                    if (!File.Exists(stg_part_filepath))
                    {
                        break;
                    }

                    // load file
                    File_Containers jsrf_file = new File_Containers(stg_part_filepath);

                    // if file structure is not of container type "indexed" skip to next file
                    if (jsrf_file.type != File_Containers.container_types.indexed) { continue; }

                    stages_info[f_pos].stage_parts.Add(new Stage_part_file());
               
                    // for each item contained
                    for (int j = 0; j < jsrf_file.INDX_root.items.Count; j++)
                    {
                        File_Containers.item item = jsrf_file.INDX_root.items[j];

                        // if it's a stage model
                        if (item.type == File_Containers.item_data_type.Stage_Model)
                        {
                            stages_info[f_pos].stage_parts[i].items.Add(new Stage_part_file.Info(new Stage_Model(item.data)));
                            stages_info[f_pos].total_triangles_count += stages_info[f_pos].stage_parts[i].items[stages_info[f_pos].stage_parts[i].items.Count - 1].triangle_count;
                            stages_info[f_pos].total_models_count++;
                        } // Texture
                        else if (item.type == File_Containers.item_data_type.Texture)
                        {
                            stages_info[f_pos].stage_parts[i].items.Add(new Stage_part_file.Info());
                            stages_info[f_pos].stage_parts[i].items[stages_info[f_pos].stage_parts[i].items.Count - 1].texture = true;
                            stages_info[f_pos].total_textures_count++;
                        } // MDLB
                        else if (item.type == File_Containers.item_data_type.Stage_MDLB)
                        {
                            stages_info[f_pos].stage_parts[i].items.Add(new Stage_part_file.Info());
                            stages_info[f_pos].stage_parts[i].items[stages_info[f_pos].stage_parts[i].items.Count - 1].MDLB = true;
                        }
                    }
  
                }
                stages_info[stages_info.Count - 1].total_stage_file_parts--;
                f_pos++;

                #endregion
            }


            int count = 0;
            // for each stage, get total stats and add it to the global stats
            for (int i = 0; i < stages_info.Count; i++)
            {
                if(stages_info[i].total_models_count > max_models_count_per_stage)
                {
                    max_models_count_per_stage = stages_info[i].total_models_count;
                    max_models_count_stage_number = stages_info[i].stage_number;
                }

                if (stages_info[i].total_triangles_count > max_triangles_count_per_stage)
                {
                    max_triangles_count_per_stage = stages_info[i].total_triangles_count;
                    max_triangles_count_stage_number = stages_info[i].stage_number;
                }

                if (stages_info[i].total_textures_count > max_textures_count_per_stage)
                {
                    max_textures_count_per_stage = stages_info[i].total_textures_count;
                    max_textures_count_stage_number = stages_info[i].stage_number;
                }


                average_models_count_per_stage += stages_info[i].total_models_count;
                average_triangles_count_per_stage += stages_info[i].total_triangles_count;
                average_textures_count_per_stage += stages_info[i].total_textures_count;
                

                count++;
            }

            // divide to get average stats
            average_models_count_per_stage /= count;
            average_triangles_count_per_stage /= count;
            average_textures_count_per_stage /= count;
        }



        public class Stage_part_file
        {
            public int models_count { get; set; }
            public int triangles_count { get; set; }
            public int textures_count { get; set; }

            public List<Info> items { get; set; } = new List<Info>();

            public Stage_part_file()
            {
                items = new List<Info>();
            }

            public Stage_part_file(Stage_Model lmdl)
            {
                items = new List<Info>();
                items.Add(new Info(lmdl));
            }

            public class Info
            {
                public int triangle_count { get; set; }
                public int textures_count { get; set; }
                // public int materials_count { get; set; }  // no need, for stage models, materials_count = texture_count
                public bool texture { get; set; }
                public bool MDLB { get; set; }

                public Info()
                {
                }

                public Info(Stage_Model lmdl)
                {
                    this.triangle_count = lmdl.triangles_list.Count;
                    this.textures_count = lmdl.texture_ids.Count;
                    // this.materials_count = lmdl.materials_groups.Count;
                }
            }
        }

        public class stage_info
        {
            public string stage_number { get; set; }

            public int total_models_count { get; set; }
            public int total_triangles_count { get; set; }
            public int total_textures_count { get; set; }
            //public int total_materials_count { get; set; } // no need, for stage models, materials_count = texture_count

            public int total_stage_file_parts { get; set; } // how many _YY.dat there is for a given StgXX_ stage files

            public List<Stage_part_file> stage_parts { get; set; }

            public stage_info(string _stage_number)
            {
                this.stage_parts = new List<Stage_part_file>();
                this.stage_number = _stage_number;
            }
        }

    }
}
