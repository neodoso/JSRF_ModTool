using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSRF_ModTool.Vector;
using System.Runtime.InteropServices;
using System.IO;
using JSRF_ModTool.Functions;

namespace JSRF_ModTool.DataFormats.JSRF
{

    /// <summary>
    ///  JSRF level_00.bin file format class
    /// </summary>
    /// <remarks>
    ///  Examples of this file type:
    ///  Media\Stage\stg00_.bin
    ///  Media\Stage\stg10_.bin
    ///  Media\Stage\stg11_.bin
    ///  ...
    /// This filetype contains 3 main blocks of data.
    /// It contains data such as, level physics collision, grind curve paths, prop (StgObj models) placement/spawn lists, character spawn points etc
    /// </remarks>
    class Level_bin
    {
        public Level_bin_Header header = new Level_bin_Header();

        public block_00 block_00;
        public block_01 block_01;
        public block_02 block_02;

        /// <summary>
        /// converts triangles vertex indices to JSRF's format
        /// </summary>
        /// <param name="tris"></param>
        /// <returns></returns>
        public List<short> convert_coll_triangles_indices(List<short> tris)
        {
            short a = tris[0];
            short b = tris[1];
            short c = tris[2];
            /*
            if (a == 256 && b == 282 && c == 3)
            {
                string testxx = "";
            }
            */
            short addB = 0;

            if (a == 512)
            {
                addB += 1;
            }


            if (a > 256)
            {
                addB += (short)(((a + 256 - 1) / 256) - 1);
                a -= (short)(addB * 256);
            }

            if (a == 256)
            {
                addB += 1;
                a -= 256;
            }

            int newA = a % 1024;
            int newB = (a / 1024) + ((b * 4) % 256);
            int newC = (c * 16) + (b / 64);

            newB += (short)addB;

            return new List<short> { (short)newA, (short)newB, (short)newC };
        }

        Boolean export_data = true;
        Boolean export_vtx_tri = true;

        public Level_bin(string lvl_bin_filepath)
        {
            byte[] data = Parsing.FileToByteArray(lvl_bin_filepath, 0);
            header = (Level_bin_Header)(Parsing.binary_to_struct(data, 0, typeof(Level_bin_Header)));

            // copy block_00 from "data" array into a new array
            byte[] data_block_00 = new byte[header.block_00_size];
            Array.Copy(data, header.block_00_start_offset, data_block_00, 0, header.block_00_size);
            // load block_00 binary data into class instance
            block_00 = (block_00)(Parsing.binary_to_struct(data_block_00, 0, typeof(block_00)));



            #region block_00  (collision models)


            #region load headers
            block_00.block_A_headers_list = new List<block_00.collision_models_list>();

            byte[] tmp_arr = new byte[32];
            // for each header of type coll_header_A
            // get header data as coll_header_A and add it to list "block_00.block_A_headers_list"
            for (int i = 1; i < block_00.coll_headers_A_chunk_count + 1; i++)
            {
                Array.Copy(data_block_00, 32 * i, tmp_arr, 0, 32); // Array.Copy(data_block_00, 32 + 32 * i, tmp_arr, 0, 32);
                block_00.block_A_headers_list.Add((block_00.collision_models_list)(Parsing.binary_to_struct(tmp_arr, 0, typeof(block_00.collision_models_list))));
            }

            tmp_arr = new byte[112];
            // to do stg00 has more headers list after this, the 3 coll_header_A in stg00 point to the start of that secondary headers list
            for (int i = 0; i < block_00.block_A_headers_list.Count; i++)
            {
                
                // for each coll_model_header header for this block_A_header item
                for (int j = 0; j < block_00.block_A_headers_list[i].models_list_count; j++)
                {
                    //tmp_arr = new byte[112]; // moved outside of loops see above ^^^
                    Array.Copy(data_block_00, block_00.block_A_headers_list[i].models_list_start_offset + 112 * j, tmp_arr, 0, 112);
                    block_00.block_A_headers_list[i].coll_model_list.Add((block_00.collision_model)(Parsing.binary_to_struct(tmp_arr, 0, typeof(block_00.collision_model))));
                }
            }

            #endregion


            List<string> tri_errors = new List<string>();

            #region process each collision model
            // for each collision model: read vertex buffer and export collision data to text files
            // for (int i = 1; i < 2; i++)
            for (int i = 0; i < block_00.block_A_headers_list.Count; i++) 
            {
                // for each coll_model_header header for this block_A_header item
                //for (int j = 12; j < 13; j++)
                for (int j = 0; j < block_00.block_A_headers_list[i].models_list_count; j++)
                {
                    block_00.collision_model mdl = block_00.block_A_headers_list[i].coll_model_list[j];

                    List<block_00.coll_vertex> vertices = new List<block_00.coll_vertex>();
                    List<block_00.coll_triangle> triangles = new List<block_00.coll_triangle>();
         
                    int vert_end = mdl.vertices_start_offset + (mdl.vertices_count * 16);

                    // for each vertex definition (16 bytes per vert definition)
                    for (int v = 0; mdl.vertices_start_offset + v < vert_end; v += 16)
                    {
                        mdl.vertex_list.Add((block_00.coll_vertex)Parsing.binary_to_struct(data_block_00, mdl.vertices_start_offset + v, typeof(block_00.coll_vertex)));
                    }

                   // int tris_buff_size = mdl.triangle_count * 8;
                    int tris_end = mdl.triangles_start_offset + (mdl.triangle_count * 8);

                   

                        // for each triangle definition (8 bytes per definition)
                    for (int t = 0; mdl.triangles_start_offset + t < tris_end; t += 8)
                    {
                        block_00.coll_triangle tri = new block_00.coll_triangle();
                        tri.a = data_block_00[mdl.triangles_start_offset + t];
                        tri.b = data_block_00[mdl.triangles_start_offset + t + 1];
                        tri.c = BitConverter.ToInt16(data_block_00, mdl.triangles_start_offset + t + 2); //data_block_00[mdl.triangles_start_offset + t + 2];
                        tri.surface_property = BitConverter.ToInt16(data_block_00, mdl.triangles_start_offset + t + 4);
                        //tri.unk = BitConverter.ToInt16(data_block_00, mdl.triangles_start_offset + t + 6);
                        tri.surface_data_0 = data_block_00[mdl.triangles_start_offset + t + 6];
                        tri.surface_data_1 = data_block_00[mdl.triangles_start_offset + t + 7];

                        short Ta = tri.a;
                        short Tb = tri.b;
                        short Tc = tri.c;

                        tri.a_raw = tri.a;
                        tri.b_raw = tri.b;
                        tri.c_raw = tri.c;

                        if(i == 15 && j == 0)
                        {
                            if (Ta == 253 && Tb == 248 && Tc == 4099)
                            {
                                string test = "the princess is in this castle!";
                            }

                            if (Ta == 253 && Tb == 0 && Tc == 4084)
                            {
                                string test = "the princess is in this castle!";
                            }

                        }



                        // calculate remainder of the vertex's index division, that will be used to add to the index number for vertex_ID_0 and vertex_ID_1
                        tri.vtx1_remainder = (decimal)((decimal)(tri.b / 4.0) - Math.Truncate((decimal)(tri.b / 4.0)));
                        tri.vtx2_remainder = (decimal)((decimal)(tri.c / 16.0) - Math.Truncate((decimal)(tri.c / 16.0)));
                       

                        // divide vertex IDs to get truncated number
                        tri.b = (byte)(tri.b / 4);
                        tri.c = (byte)(tri.c / 16);

                        if (tri.vtx1_remainder > 0)
                        {
                            // add remainder (from vertex_ID_1) index and add it to  tri.vertex_ID_0
                            tri.a += (short)(tri.vtx1_remainder * 1024);

                            /*
                            if ( tri.vtx1_remainder == (decimal)0.25) //tri.vtx2_remainder == 0 &&
                            {
                                tri.a -= 256;
                            }
                            */
                        }

                        if (tri.vtx2_remainder > 0)
                        {
                            // add remainder (from vertex_ID_2) index and add it to  tri.vertex_ID_1
                            tri.b += (short)(tri.vtx2_remainder * 1024);
                        }

                        if (tri.c_raw >= 4096) // && tri.b_raw > 254
                        {
                           tri.c = (short)(tri.b + 1);
                        }
                        
                        if(tri.b >= 254)
                        {
                            tri.c += 254;
                        }
                        

                        mdl.triangles_list.Add(tri);

                        #region debug test triangles
                        /*
                         // test converting regular triangles indices to JSRF and compare them to game's original tris indices to see if the conversion matches properly
                        // convert triangles to game's format
                        List<short> tt = convert_coll_triangles_indices(new List<short> { tri.a, tri.b, tri.c });

                        // if game's original encoded triangle indices are different from the re-encoded triangle's indices
                        if (tt[0] != Ta || tt[1] != Tb || tt[2] != Tc)
                        {
                             // source triangle // game's encoded triangle // re-encoded triangle
                             string test =  + tri.a + "  " + tri.b + "  " + tri.c + " || " + Ta + " " + Tb + " " + Tc  + " || " + tt[0] + "  " + tt[1] + "  " + tt[2];
                            // log triangles differences
                            Main.tri_errors.Add(test);
                            tri_errors.Add(test);
                        }
                        */
                        #endregion
                    }


                    // reassign model data to class instance block_00.block_A_headers_list
                    block_00.block_A_headers_list[i].coll_model_list[j] = mdl;


                    #region early debug export data used to visualize collision mesh triangle data and vertices

                    
                    if (export_vtx_tri)
                    {
                        // string coll_export_dir = @"C:\Users\Mike\Desktop\JSRF\research\stage_bin\exp\" + Path.GetFileNameWithoutExtension(lvl_bin_filepath) + "coll\\";
                        string coll_tris_export_dir = @"C:\Users\Mike\Desktop\JSRF\stg_collision\" + Path.GetFileNameWithoutExtension(lvl_bin_filepath).Replace("_", "") + "\\triangle_data\\";
                        if (!Directory.Exists(coll_tris_export_dir)) { Directory.CreateDirectory(coll_tris_export_dir); }

                        #region write vertex data to binary .vtx file
                        /*
                        byte[] vertex_buffer = new byte[mdl.vertices_count * 16];
                        Array.Copy(data_block_00, mdl.vertices_start_offset, vertex_buffer, 0, vertex_buffer.Length);
                        try
                        {
                            File.WriteAllBytes(coll_export_dir + i + "_" + j + ".vtx", vertex_buffer);
                        } catch
                        {
                            System.Windows.Forms.MessageBox.Show("Error, could not write file: \n" + coll_export_dir + i + "_" + j + ".vtx" + "\n\nMake sure another application is not using the file.");
                        }
                    
                        */
                        #endregion

                        #region write triangle data to binary .tri file

                       // byte[] triangles_buffer = new byte[mdl.triangle_count * 8];
                       // Array.Copy(data_block_00, mdl.triangles_start_offset, triangles_buffer, 0, triangles_buffer.Length);
                        /*
                        try
                        {
                            File.WriteAllBytes(coll_export_dir + i + "_" + j + ".tri", triangles_buffer);
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show("Error, could not write file: \n" + coll_export_dir + i + "_" + j + ".tri" + "\n\nMake sure another application is not using the file.");
                        }
                        */
                        #endregion

                        #region export triangle data as test


                        List<block_00.coll_triangle> tris = new List<block_00.coll_triangle>();

                        string faces_list = "";

                        List<string> lines = new List<string>();



                        for (int t = 0; t < mdl.triangles_list.Count; t ++)
                        {
                            block_00.coll_triangle tri = mdl.triangles_list[t];

                            //lines.Add(tri.a.ToString("D3") + "  " + tri.b.ToString("D3") + "  " + tri.c.ToString("D3") + " | " + tri.surface_property.ToString("D3") + " | " + tri.surface_data_0.ToString("D3") + "  " + tri.surface_data_1.ToString("D3"));
                            //lines.Add(fn(tri.a.ToString("D3")) + "  " + fn(tri.b.ToString("D3")) + "  " + fn(tri.c.ToString("D3")) + " |" + fn(tri.surface_property.ToString("D3")) + " | " + fn(tri.surface_data_0.ToString("D3")) + "  " + fn(tri.surface_data_1.ToString("D3")));
                            //lines.Add(fn(tri.a.ToString("D3")) + "  " + fn(tri.b.ToString("D3")) + "  " + fn(tri.c.ToString("D3")) + " |" + fn(tri.surface_property.ToString("D3")) + " | " + fn(tri.unk.ToString("D3")) );
                            lines.Add(fn(tri.a_raw.ToString("D3")) + "  " + fn(tri.b_raw.ToString("D3")) + "  " + fn(tri.c_raw.ToString("D3")) +
                                    " |" + fn(tri.a.ToString("D3")) + "  " + fn(tri.b.ToString("D3")) + "  " + fn(tri.c.ToString("D3")) +
                                    " |" + tri.vtx1_remainder.ToString() + "  " + tri.vtx2_remainder.ToString() +
                                    " #" +  fn(tri.surface_property.ToString("D3")) + " | " + fn(tri.surface_data_0.ToString("D3")) + "  " + fn(tri.surface_data_1.ToString("D3")));
                            //tris.Add(tri);

                            if (tri.surface_data_0 == 65)
                            {
                                faces_list = faces_list  + "," + t.ToString();
                            }

                        }

                        if(faces_list != "")
                            faces_list = faces_list.Remove(0, 1);

                        lines.Add("");
                        //lines.Add(faces_list);

                        System.IO.File.WriteAllLines(coll_tris_export_dir + "tris_" + i + "_" + j + ".txt", lines);

                        #endregion
                    }

                    #endregion
                }
            }

            string fn(string s)
            {


                if (s.StartsWith("00"))
                {
                    return s.Replace("00", "  ");
                }

                if (s.StartsWith("0"))
                {
                    StringBuilder sb = new StringBuilder(s);
                    sb[0] = ' ';
                    s = sb.ToString();
                    return s;
                }

                return s;
            }


            #endregion

            #endregion


            #region block_01 (grind paths)

            // copy block_01 from "data" array into a new array

            byte[] data_block_01 = new byte[header.block_01_size];
            Array.Copy(data, header.block_01_start_offset, data_block_01, 0, header.block_01_size);
            // load block_01 binary data into class instance
            block_01 = new block_01(data_block_01);


            #endregion

            #region block_02 (object / models spawns)

            // copy block_02 from "data" array into a new array
            byte[] data_block_02 = new byte[header.block_02_size];
            Array.Copy(data, header.block_02_start_offset, data_block_02, 0, header.block_02_size);

            block_02 = new block_02(data_block_02);



            // block_02_header = (unk_block_02)(Parsing.binary_to_struct(data_block_02, 0, typeof(unk_block_02)));

            /*
             // copy block_02 from "data" array into a new array
             byte[] data_block_02_block_02 = new byte[block_02_header.block_02_count * 80];
             Array.Copy(data_block_02, block_02_header.block_02_starto, data_block_02_block_02, 0, block_02_header.block_02_count * 80);

             block_02_header.block_02_test = (unk_block_02.block_02)(Parsing.binary_to_struct(data_block_02_block_02, 0, typeof(unk_block_02.block_02)));
            */
            #endregion





#if DEBUG

            if(export_data)
            {
                string export_dir = @"C:\Users\Mike\Desktop\JSRF\stg_collision\" + Path.GetFileNameWithoutExtension(lvl_bin_filepath).Split('_')[0] + "\\";

                block_00.export_all_collision_meshes(export_dir + "collision_models\\");

                // exports single collision model
                //export_coll_mesh(export_dir, 14, 3);

                block_01.export_grind_path_data(export_dir + "grind_paths.txt");
                block_01.export_grind_path_data_blender(export_dir + "grind_paths_blender.obj");
            }

#endif


        }


        private class trig
        {
            public short a { get; set; }
            public short b { get; set; }
            public short c { get; set; }

            public trig(short _a, short _b, short _c)
            {
                this.a = _a;
                this.b = _b;
                this.c = _c;
            }
        }


    }



    /// <summary>
    ///  JSRF level_xx.bin file Header
    /// </summary>
    public class Level_bin_Header
    {
        // block_00 = grind path/curves data? seems to have offsets and intergers as well as floats xyz + xyz (start and end point?)
        public Int32 block_00_start_offset { get; set; } // 
        public Int32 block_00_size { get; set; } // 59640

        public Int32 block_01_start_offset { get; set; } // 
        public Int32 block_01_size { get; set; } // 

        public Int32 block_02_start_offset { get; set; } // 
        public Int32 block_02_size { get; set; } // 

        public Int32 models_count { get; set; } // 
        public Int32 unk { get; set; } // zero

        // level number
        public Int32 unk_32 { get; set; } // zero
        public Int32 unk_36 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_40 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_44 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_48 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_52 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_56 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_60 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_64 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_68 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_72 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_76 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_80 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_84 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_88 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_92 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_96 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_100 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_104 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_108 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_112 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_116 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_120 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_124 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_128 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_132 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_136 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_140 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_144 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_148 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_152 { get; set; } // number of StgXX_XX parts 

        public Int32 unk_156 { get; set; } // NaN (indicates end of block or header??)
        // END HEADER??
    }






    #region main_block_00

    /// <summary>
    /// Level block_00 header (64 bytes)
    /// </summary>
    /// 
    /// <remarks>
    /// BLOCK_00
    /// level bounding boxes called "DrawBlocks" (see stg10_block_00 it has the string "us06DrawBlock")
    /// seems to delimit the area in which collision models are contained
    ///
    /// collision data (see offset 3712 (block_00) from stg00_block_00, list of (Vector3 + 4 bytes))
    /// offset 120 read as int32 gives   3712
    ///
    /// stg00_block_00   offset:35280  collision data triangle list?? (may point to offsets of vertecies blocks from block 3712)
    ///
    /// see offset 3592 (block_00) maybe collision triangles definitions?
    /// or be pushed back if the level is not yet unlocked or area of the map not accessible)
    /// two Vector3 (see offset 64 of stg00_block00) define opposite diagonals of a plane or box, bounding boxes?
    ///
    /// there seems to be other byte or int data, which maybe point to other triggers? (linked tirggers maybe)
    /// </remarks>
    public class block_00
    {
        // from there probably another header & block type
        public Int32 coll_headers_A_offset { get; set; } // // number of coll_model_headers
        public Int32 coll_headers_A_chunk_count { get; set; } // number of blocks of a certain type (after this header)
        public Int32 unk_08 { get; set; }  // always =  0
        public Int32 unk_12 { get; set; } // always =  0

        /*

        public Int16 unk_16 { get; set; } // always 32
        public Int16 unk_18 { get; set; } // count?

        public Int16 unk_20_count { get; set; } // count?
        public Int16 unk_22 { get; set; } // 

        public Int16 unk_24 { get; set; } // always 32
        public Int16 unk_26 { get; set; } // count?

        public Int32 unk_28_count { get; set; } // count?

        public Int16 unk_32 { get; set; } // 
        public Int16 unk_34 { get; set; } // 

        public Int16 unk_36 { get; set; } // 
        public Int16 unk_38 { get; set; } // 

        public Int16 unk_40 { get; set; } // 
        public Int16 unk_42 { get; set; } // 

        public Int16 unk_44 { get; set; } // 
        public Int16 unk_46 { get; set; } // 

        public Int16 unk_48 { get; set; } // 
        public Int16 unk_50 { get; set; } // 

        public Int16 unk_52 { get; set; } // 
        public Int16 unk_54 { get; set; } // 

        public Int32 unk_56_coll_models_headers { get; set; } // points to the start of the collision model headers (112 bytes each)
        public Int32 unk_60_coll_models_count { get; set; }
        */

        public List<collision_models_list> block_A_headers_list { get; set; }


        /// <summary>
        ///  gives offset to list of "collision_model"s (32 bytes)
        /// </summary>
        public class collision_models_list
        {
            public Vector3 v1 { get; set; }
            public Vector3 v2 { get; set; }
            public Int32 models_list_start_offset { get; set; } // offset to list of coll_model list
            public Int32 models_list_count { get; set; } // number of items in block

            public List<collision_model> coll_model_list { get; set; }

            public collision_models_list()
            {
                coll_model_list = new List<collision_model>();
            }
        }

        // there i sa second type of collision model header?


        /// <summary>
        /// collision model (part) (112 bytes)
        /// </summary>
        /// <remarks>
        /// </remarks>
        public class collision_model
        {
            /*
            // (rotation matrix / quaternion)??
            public Vector3 v1 { get; set; }
            public Vector3 v2 { get; set; }

            public Vector3 v3 { get; set; } // scale X?----
            public Vector3 v4 { get; set; }

            public Vector3 v5 { get; set; } // rot? z = y
            public Vector3 v6 { get; set; } // scale Y? -  y = z 

            public Vector3 v7_position { get; set; }
            */

            public Vector3 v1_bbox_min { get; set; }
            public Vector3 v2_bbox_max { get; set; }

            public Vector3 v3 { get; set; }
            public float w3 { get; set; }
            public Vector3 v4 { get; set; }
            public float w4 { get; set; }

            public Vector3 v5 { get; set; }
            public float w5 { get; set; }
            public Vector3 v7_position { get; set; }

            //public Vector3 v7_position { get; set; }


            public float f { get; set; } // offset 84

            // vertex definition = 16 bytes
            public Int32 vertices_start_offset { get; set; }
            public Int32 vertices_count { get; set; } // multiply by 16 (bytes) (1 vertex = 3 floats + 1 int32)

            // triangle definition = 8 bytes
            public Int32 triangles_start_offset { get; set; }
            public Int32 triangle_count { get; set; } // multiply by 8 (bytes)


            public Int32 unk_104 { get; set; }
            public Int32 unk_108 { get; set; }

            public List<coll_vertex> vertex_list { get; set; }
            public List<coll_triangle> triangles_list { get; set; }

            public collision_model()
            {
                vertex_list = new List<coll_vertex>();
                triangles_list = new List<coll_triangle>();
            }
        }



        /// <summary>
        /// Collision Vertex defition  (16 bytes)
        /// </summary>
        public struct coll_vertex
        {
            public Vector3 vert { get; set; }
            public Int32 unk { get; set; } // usually zero
        }

        /// <summary>
        /// Collision Triangle? defition  (8 bytes)
        /// </summary>
        /// <remarks>
        /// After several attempts at importing collision data as a 3D mesh
        /// its unclear if these really are or not triangle (list) defitions
        /// usually 3D meshes are definex by vertecies, then a triangle list which gives the order 
        /// in which said vertecies are connected to compose edges and triangles
        /// Perhaps these are "stripped" triangles, or its not common a triangle list? custom collision model format?
        /// </remarks>
        public class coll_triangle
        {
            // triangle's vertex indices (the game encodes these in a particular way, "0 8 16" raw data indices are decoded back into regular "0 1 2" indices by the JSRF tool)
            public Int16 a { get; set; } // 
            public Int16 b { get; set; } // divide by 4  for real vertex index
            public Int16 c { get; set; } // divide by 16 for real vertex index

            public Int16 surface_property { get; set; } // defines if surface is a wall, floor, stairs, ramp etc
            //public Int16 unk { get; set; }
            public byte surface_data_0 { get; set; } // aka "extra2" 2 bytes at the end of a collision triangle data. if we figure out what this is and how to calculate these two for each triangle
            public byte surface_data_1 { get; set; } // then we can have custom levels :O


            public Int16 a_raw { get; set; } // these 3 "_raw" values does not actually exist in the game's file structure
            public Int16 b_raw { get; set; } // 
            public Int16 c_raw { get; set; } // this is are just to keep the vertices indices un-decoded for research/debugging

            public decimal vtx1_remainder { get; set; } // stored for debugging (doesn't exist in the in-game class/format)
            public decimal vtx2_remainder { get; set; } // stored for debugging (doesn't exist in the in-game class/format)
        }

        public void export_all_collision_meshes(string dir)
        {
            Directory.CreateDirectory(dir);
            List<String> smd_lines = new List<string>();
            List<String> transform_lines = new List<string>();

            // for each collision model: read vertex buffer and export collision data to text files
            for (int i = 0; i < this.block_A_headers_list.Count; i++) //block_00.block_A_headers_list.Count
            {
                // for each coll_model_header header for this block_A_header item
                //for (int j = 12; j < 13; j++)
                for (int j = 0; j < this.block_A_headers_list[i].coll_model_list.Count; j++)
                {
                    block_00.collision_model coll_head = this.block_A_headers_list[i].coll_model_list[j];

                    #region SMD export (deprecated)
                    /*
                    smd_lines = new List<string>();

                    smd_lines.Add("version 1");

                    smd_lines.Add("nodes");
                    smd_lines.Add("0 root -1");
                    smd_lines.Add("end");

                    smd_lines.Add("skeleton");
                    smd_lines.Add("time 0");
                    smd_lines.Add(" 0 0 0 0 0 0 0");
                    smd_lines.Add("end");

                    smd_lines.Add("triangles");

                    for (int t = 0; t < coll_head.triangles_list.Count; t++)
                    {
                        smd_lines.Add("JSRF_coll");
                        block_00.coll_triangle tri = coll_head.triangles_list[t];

                        smd_lines.Add("0 " + coll_head.vertex_list[tri.vertex_ID_0].vert.X + " " + coll_head.vertex_list[tri.vertex_ID_0].vert.Y + " " + coll_head.vertex_list[tri.vertex_ID_0].vert.Z + " 0 0 0 0 0 1 0 1");
                        smd_lines.Add("0 " + coll_head.vertex_list[tri.vertex_ID_1].vert.X + " " + coll_head.vertex_list[tri.vertex_ID_1].vert.Y + " " + coll_head.vertex_list[tri.vertex_ID_1].vert.Z + " 0 0 0 0 0 1 0 1");
                        smd_lines.Add("0 " + coll_head.vertex_list[tri.vertex_ID_2].vert.X + " " + coll_head.vertex_list[tri.vertex_ID_2].vert.Y + " " + coll_head.vertex_list[tri.vertex_ID_2].vert.Z + " 0 0 0 0 0 1 0 1");
                    }

                    smd_lines.Add("end");
                    System.IO.File.WriteAllLines(dir + "coll_" + i + "_" + j + ".smd", smd_lines);



                    */

                    #endregion


                    transform_lines = new List<string>();
                    transform_lines.Add(coll_head.v7_position.X + " " + coll_head.v7_position.Y + " " + coll_head.v7_position.Z);


                    transform_lines.Add(coll_head.v3.X + " " + coll_head.v3.Y + " " + coll_head.v3.Z);
                    transform_lines.Add(coll_head.v4.X + " " + coll_head.v4.Y + " " + coll_head.v4.Z);
                    transform_lines.Add(coll_head.v5.X + " " + coll_head.v5.Y + " " + coll_head.v5.Z);

                    List<string> obj_lines = new List<string>();

 
                    //obj_lines.Add("mtllib " + System.IO.Path.GetFileNameWithoutExtension(filepath) + ".mtl");
                    obj_lines.Add("o " + "coll_" + i + "_" + j);
                    obj_lines.Add("");

                    #region TODO add materials support (for collision surface properties)

                    /*
                    // for each triangle group
                    for (int g = 0; g < this.materials_groups.Count; g++)
                    {
                        material_group grp = this.materials_groups[g];
                        tri_end += grp.triangle_count;

                        mat_group_indices.Add(Tuple.Create(tri_end, texture_ids[grp.material_ID]));

                        tri_group_offset += grp.triangle_count;

                        // write mtl material
                        mtl_lines.Add("newmtl mat_" + texture_ids[grp.material_ID]);
                        mtl_lines.Add("map_Kd C:/Users/Mike/Desktop/JSRF/research/mdls_stg/export/textures/" + texture_ids[grp.material_ID] + ".bmp");
                        mtl_lines.Add("Ks 0 0 0");
                        mtl_lines.Add("");
                    }
                    

                    if (materials_groups.Count == 0)
                    {
                        mat_group_indices.Add(Tuple.Create(triangles_list.Count, 0));
                    }
                    */


                    /*
                            if (t >= mat_group_indices[0].Item1)
                            {
                                if (mat_group_indices.Count > 1)
                                    mat_group_indices.RemoveAt(0);
                            }
                            //triangles.Add("usemtl mat_" + mat_group_indices[0].Item2);
                    */

                    #endregion


                    // for each triangle in this group
                    for (int v = 0; v < coll_head.vertex_list.Count; v++)
                    {
                        block_00.coll_vertex vert = coll_head.vertex_list[v];

                        obj_lines.Add("v " + vert.vert.X + " " + vert.vert.Y + " " + vert.vert.Z);
                    }

                    obj_lines.Add("");

                    // for each face
                    for (int t = 0; t < coll_head.triangles_list.Count; t++)
                    {
                        block_00.coll_triangle tri = coll_head.triangles_list[t];

                        obj_lines.Add("f " + (tri.a +1) + " " + (tri.b + 1) + " " + (tri.c + 1));
                    }

                    // export level model
                    System.IO.File.Delete(dir + "coll_" + i + "_" + j + ".obj");
                    System.IO.File.AppendAllLines(dir + "coll_" + i + "_" + j + ".obj", obj_lines);

 

                    ///transform_lines.Add("scl:" + coll_head. + " " + coll_head.v7_position.Y + " " + coll_head.v7_position.Z);
                    System.IO.File.WriteAllLines(dir + "coll_" + i + "_" + j + ".xyz", transform_lines);
                }
            }
        }

        public void export_single_coll_mesh(string dir, int a, int b)
        {
            List<String> smd_lines = new List<string>();
            List<String> transform_lines = new List<string>();

            // for each collision model: read vertex buffer and export collision data to text files
            for (int i = a; i < a + 1; i++) //block_00.block_A_headers_list.Count
            {
                // for each coll_model_header header for this block_A_header item
                for (int j = b; j < b + 1; j++)
                {
                    block_00.collision_model coll_head = this.block_A_headers_list[i].coll_model_list[j];

                    #region SMD export, deprecated
                    /*
                    smd_lines = new List<string>();

                    smd_lines.Add("version 1");

                    smd_lines.Add("nodes");
                    smd_lines.Add("0 root -1");
                    smd_lines.Add("end");

                    smd_lines.Add("skeleton");
                    smd_lines.Add("time 0");
                    smd_lines.Add(" 0 0 0 0 0 0 0");
                    smd_lines.Add("end");

                    smd_lines.Add("triangles");

                    for (int t = 0; t < coll_head.triangles_list.Count; t++)
                    {
                        smd_lines.Add("JSRF_coll");
                        block_00.coll_triangle tri = coll_head.triangles_list[t];

                        smd_lines.Add("0 " + coll_head.vertex_list[tri.vertex_ID_0].vert.X + " " + coll_head.vertex_list[tri.vertex_ID_0].vert.Y + " " + coll_head.vertex_list[tri.vertex_ID_0].vert.Z + " 0 0 0 0 0 1 0 1");
                        smd_lines.Add("0 " + coll_head.vertex_list[tri.vertex_ID_1].vert.X + " " + coll_head.vertex_list[tri.vertex_ID_1].vert.Y + " " + coll_head.vertex_list[tri.vertex_ID_1].vert.Z + " 0 0 0 0 0 1 0 1");
                        smd_lines.Add("0 " + coll_head.vertex_list[tri.vertex_ID_2].vert.X + " " + coll_head.vertex_list[tri.vertex_ID_2].vert.Y + " " + coll_head.vertex_list[tri.vertex_ID_2].vert.Z + " 0 0 0 0 0 1 0 1");
                    }

                    smd_lines.Add("end");
                    System.IO.File.WriteAllLines(dir + "coll_" + i + "_" + j + ".smd", smd_lines);


                    transform_lines = new List<string>();
                    transform_lines.Add(coll_head.v7_position.X + " " + coll_head.v7_position.Y + " " + coll_head.v7_position.Z);

                    transform_lines.Add(coll_head.v3.X + " " + coll_head.v3.Y + " " + coll_head.v3.Z);
                    transform_lines.Add(coll_head.v4.X + " " + coll_head.v4.Y + " " + coll_head.v4.Z);
                    transform_lines.Add(coll_head.v5.X + " " + coll_head.v5.Y + " " + coll_head.v5.Z);
                    //transform_lines.Add(1 + " " + 1 + " " + 1);
                    */

                    #endregion

                    transform_lines = new List<string>();
                    transform_lines.Add(coll_head.v7_position.X + " " + coll_head.v7_position.Y + " " + coll_head.v7_position.Z);


                    transform_lines.Add(coll_head.v3.X + " " + coll_head.v3.Y + " " + coll_head.v3.Z);
                    transform_lines.Add(coll_head.v4.X + " " + coll_head.v4.Y + " " + coll_head.v4.Z);
                    transform_lines.Add(coll_head.v5.X + " " + coll_head.v5.Y + " " + coll_head.v5.Z);

                    List<string> obj_lines = new List<string>();


                    //obj_lines.Add("mtllib " + System.IO.Path.GetFileNameWithoutExtension(filepath) + ".mtl");
                    obj_lines.Add("o " + "coll_" + i + "_" + j);
                    obj_lines.Add("");

                    #region TODO add materials support (for collision surface properties)

                    /*
                    // for each triangle group
                    for (int g = 0; g < this.materials_groups.Count; g++)
                    {
                        material_group grp = this.materials_groups[g];
                        tri_end += grp.triangle_count;

                        mat_group_indices.Add(Tuple.Create(tri_end, texture_ids[grp.material_ID]));

                        tri_group_offset += grp.triangle_count;

                        // write mtl material
                        mtl_lines.Add("newmtl mat_" + texture_ids[grp.material_ID]);
                        mtl_lines.Add("map_Kd C:/Users/Mike/Desktop/JSRF/research/mdls_stg/export/textures/" + texture_ids[grp.material_ID] + ".bmp");
                        mtl_lines.Add("Ks 0 0 0");
                        mtl_lines.Add("");
                    }
                    

                    if (materials_groups.Count == 0)
                    {
                        mat_group_indices.Add(Tuple.Create(triangles_list.Count, 0));
                    }
                    */


                    /*
                            if (t >= mat_group_indices[0].Item1)
                            {
                                if (mat_group_indices.Count > 1)
                                    mat_group_indices.RemoveAt(0);
                            }
                            //triangles.Add("usemtl mat_" + mat_group_indices[0].Item2);
                    */

                    #endregion


                    // for each triangle in this group
                    for (int v = 0; v < coll_head.vertex_list.Count; v++)
                    {
                        block_00.coll_vertex vert = coll_head.vertex_list[v];

                        obj_lines.Add("v " + vert.vert.X + " " + vert.vert.Y + " " + vert.vert.Z);
                    }

                    obj_lines.Add("");

                    // for each face
                    for (int t = 0; t < coll_head.triangles_list.Count; t++)
                    {
                        block_00.coll_triangle tri = coll_head.triangles_list[t];

                        obj_lines.Add("f " + (tri.a + 1) + " " + (tri.b + 1) + " " + (tri.c + 1));
                    }

                    // export level model
                    System.IO.File.Delete(dir + "coll_" + i + "_" + j + ".obj");
                    System.IO.File.AppendAllLines(dir + "coll_" + i + "_" + j + ".obj", obj_lines);

                    ///transform_lines.Add("scl:" + coll_head. + " " + coll_head.v7_position.Y + " " + coll_head.v7_position.Z);
                    System.IO.File.WriteAllLines(dir + "coll_" + i + "_" + j + ".xyz", transform_lines);
                }
            }
        }


    }



    #endregion

    // grind paths
    #region main_block_01

    /// <summary>
    /// contains lists of grin paths, contained within parent objects
    /// </remarks>
    public class block_01
    {
        // header
        public Int32 unk_id { get; set; }
        // 8192 bytes list of [8 bytes blocks] of item count + start offset (1024 items slots)
        public List<item_header> items { get; set; }


        public block_01(byte[] data)
        {
            unk_id = BitConverter.ToInt32(data, 0);
            items = new List<item_header>();

            for (int i = 4; i < 8196; i+=8)
            {
               items.Add(new item_header(data, BitConverter.ToInt32(data, i), BitConverter.ToInt32(data, i + 4)));
                //items[this.items.Count - 1].items_size = items[this.items.Count - 1].items_count * 4;
            }

        }

        /// <summary>
        /// item_header is a list of [ [item count] and [start offset] ]
        /// each item_header
        /// </summary>
        public class item_header
        {
            /// <summary>
            /// file structure: 
            /// [int32] item count
            /// [int32] start offset

            public Int32 start_offset { get; set; }
            public Int32 items_count { get; set; } // doesn't seen to match the item size/count between this.start_offset and next.item_header.start_offset
            // total items size = items_count * 4

            // offsets of grind_path objects
            //public List<Int32> grind_path_headers_offsets_List { get; set; }
            public List<grind_path_header> grind_path_header_List { get; set; }

            public item_header(byte[] data, Int32 _start_offset, Int32 _items_count)
            {
                this.start_offset = _start_offset;
                this.items_count = _items_count;
                grind_path_header_List = new List<grind_path_header>();

                // for each grind_path_header pointer
                for (int i = 0; i < this.items_count; i++)
                {
                    // load binary data to grind_path_header class intance
                    block_01.item_header.grind_path_header grind_path_head = (block_01.item_header.grind_path_header)(Parsing.binary_to_struct(data, BitConverter.ToInt32(data, this.start_offset + i * 4), typeof(block_01.item_header.grind_path_header)));
                    grind_path_head.grind_path_points = new List<grind_path_header.grind_path_point>();
                    // read each grind path point and add it to grind_path_head.grind_path_points[] list
                    for (int p = 0; p < grind_path_head.grind_points_count; p++)
                    {
                        // read from binary and Vector3 position and Vector3 normal
                        grind_path_header.grind_path_point point = new grind_path_header.grind_path_point( (Vector3)Parsing.binary_to_struct(data, grind_path_head.grind_points_list_start_offset + p * 24, typeof(Vector3)), (Vector3)Parsing.binary_to_struct(data, grind_path_head.grind_points_list_start_offset + p * 24 +12, typeof(Vector3)));

    
                        grind_path_head.grind_path_points.Add(point);
                    }

                    // add  grind_path_header to list
                    grind_path_header_List.Add(grind_path_head);
                }
            }


            public class grind_path_header
            {
                public Int32 grind_points_list_start_offset { get; set; }
                public Int32 grind_points_count { get; set; }
                public Int16 unk_8 { get; set; }
                public Int16 unk_10 { get; set; }
                public Vector3 bbBox_A { get; set; } // bounding box point A
                public Vector3 bbBox_B { get; set; } // bounding box point B

                public List<grind_path_point> grind_path_points { get; set; }

                public class grind_path_point
                {
                    public Vector3 position { get; set; } // point position
                    public Vector3 normal { get; set; } // point orientation

                    public grind_path_point(Vector3 _pos, Vector3 _norm)
                    {
                        this.position = _pos;
                        this.normal = _norm;
                    }
                }
            }
        }


        public void export_grind_path_data(string filepath)
        {
            List<string> lines = new List<string>();

            int item_count = 0;
            int grind_path_item_count = 0;

            List<Int32> grind_points_list_offset = new List<Int32>();

            foreach (var item in items)
            {
                if(item.items_count > 0)
                //lines.Add("Grind Path Group [" + item_count + "]");
                // create model object
                foreach (var grind_path_item in item.grind_path_header_List)
                {
                    // if grind points list has already been exported, skip
                    if(grind_points_list_offset.Contains(grind_path_item.grind_points_list_start_offset))
                    {
                        continue;
                    }

                    grind_points_list_offset.Add(grind_path_item.grind_points_list_start_offset);

                    lines.Add("[" + item_count + ":" + grind_path_item_count + ":" + grind_path_item.unk_8 + " " + grind_path_item.unk_10 + "]"); //Grind Path SubGroup

                    // creatre point
                    foreach (var points in grind_path_item.grind_path_points)
                    {
                        lines.Add(points.position.X + " " + points.position.Y + " " + points.position.Z + " " + points.normal.X + " " + points.normal.Y + " " + points.normal.Z);
                    }
                    grind_path_item_count++;

                    lines.Add("end");
                }

                    item_count++;
             
            }

            System.IO.File.WriteAllLines(filepath, lines);

        }

        public void export_grind_path_data_blender(string filepath)
        {
            List<string> lines = new List<string>();

            int item_count = 0;
            int grind_path_item_count = 0;

            List<Int32> grind_points_list_offset = new List<Int32>();
            int line_point_index = 0;
            foreach (var item in items)
            {
             
                if (item.items_count > 0)
                {

                    //lines.Add("Grind Path Group [" + item_count + "]");
                    // create model object
                  //  for (int i = 0; i < length; i++)
                    //foreach (var grind_path_item in item.grind_path_header_List)
                    for (int f = 0; f < item.grind_path_header_List.Count; f++)
                    {
                        item_header.grind_path_header grind_path_item = item.grind_path_header_List[f];

                        // if grind points list has already been exported, skip
                        if (grind_points_list_offset.Contains(grind_path_item.grind_points_list_start_offset))
                        {
                            continue;
                        }

                        grind_points_list_offset.Add(grind_path_item.grind_points_list_start_offset);

                        lines.Add("o gp_" + item_count + "_" + grind_path_item_count); //Grind Path SubGroup 
                        lines.Add("");
                        // creatre point
                        foreach (var points in grind_path_item.grind_path_points)
                        {
                            decimal px = Decimal.Parse(points.position.X.ToString(), System.Globalization.NumberStyles.Any);
                            decimal py = Decimal.Parse(points.position.Y.ToString(), System.Globalization.NumberStyles.Any);
                            decimal pz = Decimal.Parse(points.position.Z.ToString(), System.Globalization.NumberStyles.Any);

                            lines.Add("v " + px + " " + py  + " " + pz);
                        }
                     

                    
                        for (int i = 0; i < grind_path_item.grind_path_points.Count; i+=2)
                        {
                            lines.Add("l " + (line_point_index + i + 1) + " " + (line_point_index + i + 2));
                            if (i < grind_path_item.grind_path_points.Count -2)
                            {
                                lines.Add("l " + (line_point_index + i + 2) + " " + (line_point_index + i + 3));
                            }

                            // if last line
                            if (i == grind_path_item.grind_path_points.Count-1)
                            {
                                lines[lines.Count - 1] = "l " + (line_point_index + i) + " " + (line_point_index + i + 1);
                            
                                /*
                                if (i % 2 == 0)
                                {
                                    lines[lines.Count - 1] = "l " + (line_point_index + i) + " " + (line_point_index + i + 1);
                                } else { 
                                }
                                */
                            }
                            
                        }
                        line_point_index += grind_path_item.grind_path_points.Count;

                        lines.Add("");
                        grind_path_item_count++;
                    }
                }
                item_count++;

            }

            System.IO.File.WriteAllLines(filepath, lines);

        }
    }

    #endregion

    #region main_block_02

    /// <summary>
    /// List of various prop (models) placement/spawns 
    /// </summary>
    /// <remarks>
    /// Model IDs correspond to models in files "StgObj" or MDLB contained within level_XX.dat files.
    ///
    /// at offset 4908, there seems to be more data, maybe level model parts culling?
    /// </remarks>
    public struct block_02
    {

#region header
        
        //stg10 seems to have 35 block types

        public Int32 blocks_count { get; set; } // number of items

        public Int32 block_00_starto { get; set; } // start offset of blocks
        public Int32 block_00_count { get; set; } // number of blocks

        public Int32 block_01_starto { get; set; } // start offset of blocks
        public Int32 block_01_count { get; set; } // number of blocks

        public Int32 block_02_starto { get; set; } // start offset of blocks
        public Int32 block_02_count { get; set; } // number of blocks

        public Int32 block_03_starto { get; set; } // start offset of blocks
        public Int32 block_03_count { get; set; } // number of blocks

        public Int32 block_04_starto { get; set; } // start offset of blocks
        public Int32 block_04_count { get; set; } // number of blocks

        public Int32 block_05_starto { get; set; } // start offset of blocks
        public Int32 block_05_count { get; set; } // number of blocks

        public Int32 block_06_starto { get; set; } // start offset of blocks
        public Int32 block_06_count { get; set; } // number of blocks

        public Int32 block_07_starto { get; set; } // start offset of blocks
        public Int32 block_07_count { get; set; } // number of blocks

        public Int32 block_08_starto { get; set; } // start offset of blocks
        public Int32 block_08_count { get; set; } // number of blocks

        public Int32 block_09_starto { get; set; } // start offset of blocks
        public Int32 block_09_count { get; set; } // number of blocks

        public Int32 block_10_starto { get; set; } // start offset of blocks
        public Int32 block_10_count { get; set; } // number of blocks

        public Int32 block_11_starto { get; set; } // start offset of blocks
        public Int32 block_11_count { get; set; } // number of blocks

        public Int32 block_12_starto { get; set; } // start offset of blocks
        public Int32 block_12_count { get; set; } // number of blocks

#endregion



        // rendering zones? has data which seems to be coords for cubes, defined by 4 Vector3 points + 1 for height and one more for unknown purpose
        // these boxes seem to emcompass part of a level model part
        // so its probably a box to cull and show/hide the model if its on screen or not
        public List<draw_distance_region> draw_distance_regions { get; set; }

        public List<object_spawn> block_01_list { get; set; }
        public List<object_spawn> block_02_list { get; set; }
        public List<object_spawn> block_03_list { get; set; }
        public List<object_spawn> block_04_decals_list { get; set; }
        public List<object_spawn> block_05_props_list { get; set; }
        public List<object_spawn> block_06_list { get; set; }
        public List<object_spawn> block_07_props_list { get; set; }
        public List<object_spawn> block_08_MDLB_list { get; set; } // more props, small fences under ufo in Garage level
        public List<object_spawn> block_09_list_prop { get; set; } // basket ball props  in Garage level
        public List<object_spawn> block_10_list { get; set; }
        public List<object_spawn> block_11_list { get; set; }
        public List<object_spawn> block_12_list { get; set; }

        /// <summary>
        /// load list of data blocks into class instance
        /// </summary>
        public block_02(byte[] data)
        {
            this = (block_02)(Parsing.binary_to_struct(data, 0, typeof(block_02)));


            draw_distance_regions = new List<draw_distance_region>();

            for (int i = 0; i < block_00_count; i++)
            {
                byte[] block = new byte[340];
                Array.Copy(data, block_00_starto + (i * 340), block, 0, 340);
                draw_distance_regions.Add((draw_distance_region)(Parsing.binary_to_struct(block, 0, typeof(draw_distance_region))));
            }



            block_01_list = new List<object_spawn>();

            for (int i = 0; i < block_01_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_01_starto + (i * 80), block, 0, 80);
                block_01_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }

            block_02_list = new List<object_spawn>();

            for (int i = 0; i < block_02_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_02_starto + (i * 80), block, 0, 80);
                block_02_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }

            block_03_list = new List<object_spawn>();

            for (int i = 0; i < block_03_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_03_starto + (i * 80), block, 0, 80);
                block_03_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }

            block_04_decals_list = new List<object_spawn>();

            for (int i = 0; i < block_04_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_04_starto + (i * 80), block, 0, 80);
                block_04_decals_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }

            block_05_props_list = new List<object_spawn>();

            if(block_05_count < 90000)
            {
                for (int i = 0; i < block_05_count; i++)
                {
                    byte[] block = new byte[80];
                    Array.Copy(data, block_05_starto + (i * 80), block, 0, 80);
                    block_05_props_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                }
            }


            block_06_list = new List<object_spawn>();

            for (int i = 0; i < block_06_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_06_starto + (i * 80), block, 0, 80);
                block_06_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }

            block_07_props_list = new List<object_spawn>();

            for (int i = 0; i < block_07_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_07_starto + (i * 80), block, 0, 80);
                block_07_props_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }


            block_08_MDLB_list = new List<object_spawn>();

            for (int i = 0; i < block_08_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_08_starto + (i * 80), block, 0, 80);
                block_08_MDLB_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }

            block_09_list_prop = new List<object_spawn>();

            if(block_09_count < 90000) //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            {
                for (int i = 0; i < block_09_count; i++)
                {
                    byte[] block = new byte[80];
                    if (block.Length > block_09_starto + (i * 80)) //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    {
                        Array.Copy(data, block_09_starto + (i * 80), block, 0, 80);
                        block_09_list_prop.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                    }

                }
            }


            block_10_list = new List<object_spawn>();


            block_10_list = new List<object_spawn>();

            if (block_10_count < 90000) //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            {
                for (int i = 0; i < block_10_count; i++)
                {
                    byte[] block = new byte[80];
                    if(block.Length > block_10_starto + (i * 80)) //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    {
                        Array.Copy(data, block_10_starto + (i * 80), block, 0, 80);
                        block_10_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                    }

                }
            }

            block_11_list = new List<object_spawn>();

            for (int i = 0; i < block_11_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_11_starto + (i * 80), block, 0, 80);
                block_11_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }

            block_12_list = new List<object_spawn>();

            for (int i = 0; i < block_12_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_12_starto + (i * 80), block, 0, 80);
                block_12_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }
            // blocks_count = 0;
        }

        /// <summary>
        /// generic class of object (model) spawn
        /// </summary>
        /// <remarks>
        /// Rotation matrix is defined by multiple vector3
        /// </remarks>
        public struct object_spawn
        {
            public Vector.Vector3 v0 { get; set; }
            public float padding_0 { get; set; }

            public Vector.Vector3 v1 { get; set; }
            public float padding_1 { get; set; }

            public Vector.Vector3 v2 { get; set; }
            public float padding_2 { get; set; }

            public Vector.Vector3 v3 { get; set; }
            public float padding_3 { get; set; }

            public Int32 model_ID { get; set; }
            public Int32 num_b { get; set; }
            public Int32 num_c { get; set; }
            public Int32 num_d { get; set; }
        }

        public struct draw_distance_region // 340 bytes?
        {
            public Vector.Vector3 v0 { get; set; }
            public Vector.Vector3 v1 { get; set; }
            public Vector.Vector3 v2 { get; set; }
            public Vector.Vector3 v3 { get; set; }
            public Vector.Vector3 v4_height { get; set; }
            public Vector.Vector3 v5 { get; set; }

            // not sure if floats if so, rotations or matrix, else might be int16 or flags
            public Vector.Vector3 v6 { get; set; }
            public Vector.Vector3 v7 { get; set; }
            public Vector.Vector3 v8 { get; set; }
            public Vector.Vector3 v9 { get; set; }
            public Vector.Vector3 v10 { get; set; }
            public Vector.Vector3 v11 { get; set; }


            public Int32 unk_0_p_0 { get; set; } // points to a global offset (relative to parent block_02)
            public Int32 unk_0 { get; set; }
            public Int32 unk_0_p_1 { get; set; }
            public Int32 unk_1 { get; set; }  // points to a global offset (relative to parent block_02)


            public Vector.Vector3 v12 { get; set; }
            public Int32 unk_2 { get; set; }
            public Int32 unk_3 { get; set; }
            public Int32 unk_4 { get; set; }
            public Int32 unk_5 { get; set; }
            public Vector.Vector3 v13 { get; set; }
            public Int32 unk_6 { get; set; } // id?
            public Int32 unk_7 { get; set; } // id?

            public Int32 unk_8_padding { get; set; }
            public Int32 unk_9_padding { get; set; }
            public Int32 unk_10_padding { get; set; }
            public Int32 unk_11_padding { get; set; }

            public Int32 unk_12_padding { get; set; }
            public Int32 unk_13_padding { get; set; }
            public Int32 unk_14_padding { get; set; }
            public Int32 unk_15_padding { get; set; }

            public Int32 unk_16_p { get; set; } // points to a global offset (relative to parent block_02)
            public Int32 unk_17 { get; set; }

            public Int32 unk_18_p { get; set; } // points to a global offset (relative to parent block_02)
            public Int32 unk_19 { get; set; }

            public Int32 unk_20_p { get; set; } // points to a global offset (relative to parent block_02)
            public Int32 unk_21 { get; set; }

            public Int32 unk_22_p { get; set; } // points to a global offset (relative to parent block_02)
            public Int32 unk_23 { get; set; }

            public Int32 unk_24_padding { get; set; }
            public Int32 unk_25_padding { get; set; }
            public Int32 unk_26_padding { get; set; }
            public Int32 unk_27_padding { get; set; }
            public Int32 unk_28_padding { get; set; }
            public Int32 unk_29_padding { get; set; }
            public Int32 unk_31_padding { get; set; }
            public Int32 unk_32_padding { get; set; }
            public Int32 unk_33_padding { get; set; }
            public Int32 unk_34_padding { get; set; }
            public Int32 unk_35_padding { get; set; }
            public Int32 unk_36_padding { get; set; }
            public Int32 unk_37_padding { get; set; }
            public Int32 unk_38_padding { get; set; }
            public Int32 unk_39_padding { get; set; }
            public Int32 unk_40_padding { get; set; }
            public Int32 unk_41_padding { get; set; }
        }

        // TODO
        // block of 500 bytes at offset 340 = coordinates list? or bounding boxes?
    }





    #endregion

}
