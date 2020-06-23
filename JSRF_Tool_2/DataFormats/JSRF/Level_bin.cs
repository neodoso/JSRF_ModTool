using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSRF_ModTool.Vector;
using System.Runtime.InteropServices;
using System.IO;

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

        public block_00_header block_00_header;
        public block_02 block_02_header;


        public Level_bin(byte[] data)
        {
            header = (Level_bin_Header)(Parsing.binary_to_struct(data, 0, typeof(Level_bin_Header)));

            // copy block_00 from "data" array into a new array
            byte[] data_block_00 = new byte[header.block_00_size];
            Array.Copy(data, header.block_00_start_offset, data_block_00, 0, header.block_00_size);

            block_00_header = (block_00_header)(Parsing.binary_to_struct(data_block_00, 0, typeof(block_00_header)));


            #region block_00  (collision models)

            block_00_header.block_A_headers_list = new List<coll_header_A>();

            byte[] tmp_arr = new byte[32];
            // for each header of type coll_header_A
            // get header data as coll_header_A and add it to list
            for (int i = 1; i < block_00_header.coll_headers_A_chunk_count + 1; i++)
            {
                tmp_arr = new byte[32];
                Array.Copy(data_block_00, 32 * i, tmp_arr, 0, 32);
                block_00_header.block_A_headers_list.Add((coll_header_A)(Parsing.binary_to_struct(tmp_arr, 0, typeof(coll_header_A))));
            }


            // to do stg00 has more headers list after this, the 3 coll_header_A in stg00 point to the start of that secondary headers list
            for (int i = 0; i < block_00_header.block_A_headers_list.Count; i++)
            {
                // for each coll_model_header header for this block_A_header item
                for (int j = 0; j < block_00_header.block_A_headers_list[i].models_list_count; j++)
                {
                    tmp_arr = new byte[112];
                    Array.Copy(data_block_00, block_00_header.block_A_headers_list[i].models_list_start_offset + 112 * j, tmp_arr, 0, 112);
                    block_00_header.block_A_headers_list[i].coll_model_list.Add((coll_model_header)(Parsing.binary_to_struct(tmp_arr, 0, typeof(coll_model_header))));
                }
            }


            #region export data to text files

            // for each collision model: read vertex buffer and export collision data to text files

            for (int i = 0; i < block_00_header.block_A_headers_list.Count; i++)
            {
                // for each coll_model_header header for this block_A_header item
                for (int j = 0; j < block_00_header.block_A_headers_list[i].models_list_count; j++)
                {
                    coll_model_header mdl = block_00_header.block_A_headers_list[i].coll_model_list[j];

                    List<coll_vertex> vertices = new List<coll_vertex>();
                    List<coll_triangle> triangles = new List<coll_triangle>();

                    int tris_buff_size = mdl.triangle_count * 8;

                    int vert_end = mdl.vertices_start_offset + (mdl.vertices_count * 16);

                    for (int v = 0; mdl.vertices_start_offset + v < vert_end; v += 16)
                    {
                        //if (mdl.vertices_start_offset + v + 16 >= vert_end) { break; }
                        vertices.Add((coll_vertex)Parsing.binary_to_struct(data_block_00, mdl.vertices_start_offset + v, typeof(coll_vertex)));
                    }

                    int tris_end = mdl.triangles_start_offset + (mdl.triangle_count * 8);

                    for (int t = 0; mdl.triangles_start_offset + t < tris_end; t += 8)
                    {
                        // if (mdl.triangles_buffer_offset + t + 8 > tris_end) { break; }
                        triangles.Add((coll_triangle)Parsing.binary_to_struct(data_block_00, mdl.triangles_start_offset + t, typeof(coll_triangle)));
                    }

                    
                    string filename = @"C:\Users\Mike\Desktop\JSRF\research\stage_bin\stg00_coll\" + i + "_" + j + ".txt";

                    if (File.Exists(filename)) { File.Delete(filename); }

                    using (FileStream fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var v in vertices)
                        {
                            sw.WriteLine(v.vert.X + " " + v.vert.Y + " " + v.vert.Z);
                        }
                    }


                    string sss = @"C:\Users\Mike\Desktop\JSRF\research\stage_bin\stg00_coll\" + i + "_" + j + "_tris.txt";
                    
                    //string sss = @"filename";

                    if (File.Exists(sss)) { File.Delete(sss); }

                    //int div_factor = 16;

                    using (FileStream fs = new FileStream(sss, FileMode.CreateNew, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var t in triangles)
                        {
                            //  sw.WriteLine(normalize_string_spacing(t.unk_0 / 8 + " " + t.unk_1 / 8 + " " + t.unk_2 / 16 + " " + t.unk_3 / 16, 6) + " | " + normalize_string_spacing(t.unk_4 + " " + t.unk_5 + " " + t.unk_6 + " " + t.unk_7, 6));
                            sw.WriteLine(normalize_string_spacing(t.unk_0 + " " + t.unk_1 + " " + t.unk_2 + " " + t.unk_3, 6) + " | " + normalize_string_spacing(t.unk_4 + " " + t.unk_5 + " " + t.unk_6 + " " + t.unk_7, 6));
                            //sw.WriteLine(normalize_string_spacing(t.unk_0 + " " + t.unk_2 , 6) + " | " + normalize_string_spacing(t.unk_4 + " " + t.unk_5 + " " + t.unk_6 + " " + t.unk_7, 6));
                        }
                    }



                }
            }

            #endregion

            #endregion


            #region block_02

            // copy block_02 from "data" array into a new array
            byte[] data_block_02 = new byte[header.block_02_size];
            Array.Copy(data, header.block_02_start_offset, data_block_02, 0, header.block_02_size);

            block_02_header = new block_02(data_block_02);



            // block_02_header = (unk_block_02)(Parsing.binary_to_struct(data_block_02, 0, typeof(unk_block_02)));

            /*
             // copy block_02 from "data" array into a new array
             byte[] data_block_02_block_02 = new byte[block_02_header.block_02_count * 80];
             Array.Copy(data_block_02, block_02_header.block_02_starto, data_block_02_block_02, 0, block_02_header.block_02_count * 80);

             block_02_header.block_02_test = (unk_block_02.block_02)(Parsing.binary_to_struct(data_block_02_block_02, 0, typeof(unk_block_02.block_02)));
            */
            #endregion

        }


        // reformats string to a certain length, for instance normalize_string_spacing("5", 3);  will return "5  "  (added 2 spaces, since we indicated 3 spaces)
        private string normalize_string_spacing(string str, int space)
        {
            string[] strings = str.Split(' ');
            string formatted = "";

            foreach (var item in strings)
            {
                string spaces = "";


                for (int i = 0; i < space - item.Length; i++)
                {
                    spaces = spaces + " ";
                }

                formatted = formatted + item + spaces;
            }


            return formatted;
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

        public Int32 count { get; set; } // 
        public Int32 unk { get; set; } // 

        // level number
        public Int32 unk_32 { get; set; } // 
        public Int32 unk_36 { get; set; } // 
        public Int32 unk_40 { get; set; } // 
        public Int32 unk_44 { get; set; } // 
        public Int32 unk_48 { get; set; } // 
        public Int32 unk_52 { get; set; } // 
        public Int32 unk_56 { get; set; } // 
        public Int32 unk_60 { get; set; } // 
        public Int32 unk_64 { get; set; } // 
        public Int32 unk_68 { get; set; } // 
        public Int32 unk_72 { get; set; } // 
        public Int32 unk_76 { get; set; } // 
        public Int32 unk_80 { get; set; } // 
        public Int32 unk_84 { get; set; } // 
        public Int32 unk_88 { get; set; } // 
        public Int32 unk_92 { get; set; } // 
        public Int32 unk_96 { get; set; } // 
        public Int32 unk_100 { get; set; } // 
        public Int32 unk_104 { get; set; } // 
        public Int32 unk_108 { get; set; } // 
        public Int32 unk_112 { get; set; } // 
        public Int32 unk_116 { get; set; } // 
        public Int32 unk_120 { get; set; } // 
        public Int32 unk_124 { get; set; } // 
        public Int32 unk_128 { get; set; } // 
        public Int32 unk_132 { get; set; } // 
        public Int32 unk_136 { get; set; } // 
        public Int32 unk_140 { get; set; } // 
        public Int32 unk_144 { get; set; } // 
        public Int32 unk_148 { get; set; } // 
        public Int32 unk_152 { get; set; } // 

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
    public class block_00_header
    {
        // from there probably another header & block type
        public Int32 coll_headers_A_offset { get; set; } // // number of coll_model_headers
        public Int32 coll_headers_A_chunk_count { get; set; } // number of blocks of a certain type (after this header)
        public Int32 unk_08 { get; set; }  // always =  0
        public Int32 unk_12 { get; set; } // always =  0

        public Int16 unk_16 { get; set; } // always 32
        public Int16 unk_18 { get; set; } // count?

        public Int16 unk_20 { get; set; } // count?
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

        public List<coll_header_A> block_A_headers_list { get; set; }


    }




    /// <summary>
    ///  gives offset to list of "coll_model"s (32 bytes)
    /// </summary>
    public class coll_header_A
    {
        public Vector3 v1 { get; set; }
        public Vector3 v2 { get; set; }
        public Int32 models_list_start_offset { get; set; } // offset to list of coll_model list
        public Int32 models_list_count { get; set; } // number of items in block

        public List<coll_model_header> coll_model_list { get; set; }

        public coll_header_A()
        {
            coll_model_list = new List<coll_model_header>();
        }
    }

    // there i sa second type of collision model header?


    /// <summary>
    /// collision model (part) (112 bytes)
    /// </summary>
    /// <remarks>
    /// points to the vertices data block and triangles? data blocks
    /// vectors probably define bounding box for model part
    /// </remarks>
    public class coll_model_header
    {
        // (rotation matrix / quaternion)??
        public Vector3 v1 { get; set; }
        public Vector3 v2 { get; set; }

        public Vector3 v3 { get; set; }
        public Vector3 v4 { get; set; }

        public Vector3 v5 { get; set; }
        public Vector3 v6 { get; set; }

        public Vector3 v7 { get; set; }


        public float f { get; set; } // offset 84

        // vertex definition = 16 bytes
        public Int32 vertices_start_offset { get; set; }
        public Int32 vertices_count { get; set; } // multiply by 16 (bytes) (1 vertex = 3 floats + 1 int32)

        // triangle definition = 8 bytes
        public Int32 triangles_start_offset { get; set; }
        public Int32 triangle_count { get; set; } // multiply by 8 (bytes)


        public Int32 unk_104 { get; set; }
        public Int32 unk_108 { get; set; }
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
    public struct coll_triangle
    {

        public byte unk_0 { get; set; } // 
        public byte unk_1 { get; set; } // 
        public byte unk_2 { get; set; } // 
        public byte unk_3 { get; set; } // 

        public byte unk_4 { get; set; } // 
        public byte unk_5 { get; set; } // 
        public byte unk_6 { get; set; } // 
        public byte unk_7 { get; set; } // 

        /*
        public Int16 unk_0 { get; set; } // 

        public Int16 unk_2 { get; set; } // 


        public byte unk_4 { get; set; } // 
        public byte unk_5 { get; set; } // 
        public byte unk_6 { get; set; } // 
        public byte unk_7 { get; set; } // 
        */
        /*
        public byte unk_0 { get; set; } // 
        public byte unk_1 { get; set; } // 
        public byte unk_2 { get; set; } // 
        public byte unk_3 { get; set; } // 

        public byte unk_4 { get; set; } // 
        public byte unk_5 { get; set; } // 
        public byte unk_6 { get; set; } // 
        public byte unk_7 { get; set; } // 
        */
    }


    #endregion

    #region main_block_01

    /// <summary>
    /// most likely grind paths curves
    /// </summary>
    /// <remarks>
    /// see (block01 stg00_.bin at offset 59808)
    /// starts with a list of offsets that point to a list of Vector3 ( my guess is: curve point position) + Vector3 (orientation?)
    /// example of the vertex stg00_block01_ offset 11256
    /// this block probably starts with a List pointing to blocks of data (grind paths? curves?)
    /// some may be linked as a lot of the offsets defined in the header point to the same position quite often
    /// </remarks>
    public class block_01
    {
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
        public List<block_00> block_00_list { get; set; }

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


            block_00_list = new List<block_00>();

            for (int i = 0; i < block_00_count; i++)
            {
                byte[] block = new byte[340];
                Array.Copy(data, block_00_starto + (i * 340), block, 0, 340);
                block_00_list.Add((block_00)(Parsing.binary_to_struct(block, 0, typeof(block_00))));
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

            for (int i = 0; i < block_05_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_05_starto + (i * 80), block, 0, 80);
                block_05_props_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
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

            for (int i = 0; i < block_09_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_09_starto + (i * 80), block, 0, 80);
                block_09_list_prop.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
            }

            block_10_list = new List<object_spawn>();

            for (int i = 0; i < block_10_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, block_10_starto + (i * 80), block, 0, 80);
                block_10_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
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

        public struct block_00 // 340 bytes?
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
