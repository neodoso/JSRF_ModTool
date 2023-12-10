using System;
using JSRF_ModTool.Functions;
using JSRF_ModTool.Vector;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace JSRF_ModTool.DataFormats.JSRF.Stage_Bin
{
    public class Parser
    {
		string debug_data_export_dir = @"C:\Users\Mike\Desktop\JSRF\Stg_Decompiled\stg10\bin\"; // // @"C:\Users\Mike\Desktop\JSRF\Stg_Bin_Export\" // Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public JSRF.Stage_Bin.header header = new JSRF.Stage_Bin.header();

        public block_00 block_00; // stage physics collision 3d model data
        public block_01 block_01; // grind paths
        public block_02 block_02; // decals, object spawns (spawns/positions:decals, MDLB contained in StgXX_XX.dat and StgObj)

		bool export_data;

		public Parser(string stage_bin_filepath)
        {

			#if DEBUG
             export_data = true;
			#endif

            // load file to byte array
            byte[] data = Parsing.FileToByteArray(stage_bin_filepath, 0);

            // load header
            header = (JSRF.Stage_Bin.header)(Parsing.binary_to_struct(data, 0, typeof(JSRF.Stage_Bin.header)));


			#region load block 00

			byte[] array = Parsing.FileToByteArray(stage_bin_filepath, 0L);
			byte[] array2 = new byte[header.block_00_size];
			Array.Copy(array, header.block_00_start_offset, array2, 0, header.block_00_size);
			block_00 = (block_00)Parsing.binary_to_struct(array2, 0, typeof(block_00));

			block_00.block_A_headers_list = new List<block_00.collision_models_list>();
			byte[] array3 = new byte[32];

			for (int i = 1; i < block_00.coll_headers_A_chunk_count + 1; i++)
			{
				Array.Copy(array2, 32 * i, array3, 0, 32);
				block_00.block_A_headers_list.Add((block_00.collision_models_list)Parsing.binary_to_struct(array3, 0, typeof(block_00.collision_models_list)));
			}
			array3 = new byte[112];
			for (int j = 0; j < block_00.block_A_headers_list.Count; j++)
			{
				for (int k = 0; k < block_00.block_A_headers_list[j].models_list_count; k++)
				{
					Array.Copy(array2, block_00.block_A_headers_list[j].models_list_start_offset + 112 * k, array3, 0, 112);
					block_00.block_A_headers_list[j].coll_model_list.Add((block_00.collision_model)Parsing.binary_to_struct(array3, 0, typeof(block_00.collision_model)));
				}
			}

			for (int l = 0; l < block_00.block_A_headers_list.Count; l++)
			{
				for (int m = 0; m < block_00.block_A_headers_list[l].models_list_count; m++)
				{
					block_00.collision_model collision_model = block_00.block_A_headers_list[l].coll_model_list[m];
					int num = collision_model.vertices_start_offset + collision_model.vertices_count * 16;
					for (int n = 0; collision_model.vertices_start_offset + n < num; n += 16)
					{
						collision_model.vertex_list.Add((block_00.coll_vertex)Parsing.binary_to_struct(array2, collision_model.vertices_start_offset + n, typeof(block_00.coll_vertex)));
					}
					int triangles_end_addr = collision_model.triangles_start_offset + collision_model.triangle_count * 8;
					for (int bytes_offset = 0; collision_model.triangles_start_offset + bytes_offset < triangles_end_addr; bytes_offset += 8)
					{
						collision_model.triangles_list.Add(new block_00.coll_triangle((block_00.coll_triangle.raw)Parsing.binary_to_struct(array2, collision_model.triangles_start_offset + bytes_offset, typeof(block_00.coll_triangle.raw))));
					}
					block_00.block_A_headers_list[l].coll_model_list[m] = collision_model;


                    #region export collision data/models
					/*
                    if (!export_vtx_tri)
					{
						continue;
					}
					string text3 = debug_data_export_dir +"\\" + Path.GetFileNameWithoutExtension(lvl_bin_filepath).Replace("_", "") + "\\triangle_data\\";
					if (!Directory.Exists(text3))
					{
						Directory.CreateDirectory(text3);
					}

					block_00.export_all_collision_meshes(text3);
					List<block_00.coll_triangle> list4 = new List<block_00.coll_triangle>();
					List<string> list5 = new List<string>();
					list5.Add(String.Format("{0,6} {1,6} {2,6} {3,8} {4,8} {5,8} {6,8} {7,8} {8,8} {9,9}  \n\n", "Index1", "Index2", "Index3", "min_x_id", "max_x_id", "min_y_id", "max_y_id", "min_z_id", "max_z_id", "surf_prop"));
					for (int num4 = 0; num4 < collision_model.triangles_list.Count; num4++)
					{
						list5.Add(collision_model.triangles_list[num4].ToString());
					}

					File.WriteAllLines(text3 + "tris_" + l + "_" + m + ".txt", list5);
					*/
					#endregion
				}
			}

			#endregion


			// load block 01
			byte[] data_block_01 = new byte[header.block_01_size];
            Array.Copy(data, header.block_01_start_offset, data_block_01, 0, header.block_01_size);
            // load block_01 binary data into class instance
            block_01 = new block_01(data_block_01);


            // load block_02
            byte[] data_block_02 = new byte[header.block_02_size];
            Array.Copy(data, header.block_02_start_offset, data_block_02, 0, header.block_02_size);
            // load block_02 binary data into class instance
            block_02 = new block_02(data_block_02);


			#if DEBUG

			if (export_data)
			{
				string export_dir = debug_data_export_dir + Path.GetFileNameWithoutExtension(stage_bin_filepath).Split('_')[0] + "\\";

				// export collision meshes
                block_00.export_all_collision_meshes(debug_data_export_dir);

                //block_00.export_all_collision_meshes(export_dir + "\\coll\\");

                // exports single collision model
                //export_coll_mesh(export_dir + "\\coll\\", 14, 3);

				// export grind paths
                block_01.export_grind_path_data(export_dir + "grind_paths.txt");
				//block_01.export_grind_path_data_blender(export_dir + "grind_paths_blender.obj");

				// export PVS
				block_02.export_PVS_data(export_dir);
			}

			#endif
		}

	}
}
