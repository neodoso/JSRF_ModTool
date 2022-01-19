using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSRF_ModTool.Vector;
using System.Runtime.InteropServices;
using System.IO;
using JSRF_ModTool.Functions;
using JSRF_ModTool.Vector;
using JSRF_ModTool.DataFormats._3D_Model_Formats;

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
    /// It contains block 0 = level physics collision, block 1 = grind curve paths, block 2 = prop (StgObj models) placement/spawn lists, character spawn points etc
    /// </remarks>
    public class Level_bin_Compiler
    {
        public Level_bin_Header header = new Level_bin_Header();

        public block_00 block_0; // level physics collision 3d model data
        public block_01 block_1; // grind paths
        public block_02 block_2; // object spawns (spawns/positions of MDLB props(contained in StgXX_XX.dat and/or StgObj), MDLB (contained in the Stg), decals and more)

        public Level_bin_Compiler()
        {


        }

        public void compile_single_model_vtx_tri_buffers()
        {
            string model_path = @"C:\Users\Mike\Documents\XSI_Projects\JSRF\Models_src\stg00\stg00_00_mdlb_21_reinjection.obj";

            OBJ obj = new OBJ(model_path);

            block_0 = new block_00();

            #region import collision models into classes instances

            // create collision model object
            block_00.collision_model coll_mdl = new block_00.collision_model();

            // import vertices buffer
            for (int v = 0; v < obj.meshes[0].vertex_buffer.Count; v++)
            {
                coll_mdl.vertex_list.Add(new block_00.coll_vertex(obj.meshes[0].vertex_buffer[v], 0));
            }
            // set vextex count
            coll_mdl.vertices_count = coll_mdl.vertex_list.Count;

            // import triangles buffer
            // and convert triangles indices with: convert_coll_triangles_indices()
            for (int f = 0; f < obj.meshes[0].face_indices.Count - 2; f += 3)
            {
                List<short> t = block_0.convert_coll_triangles_indices(new List<short> { (short)(obj.meshes[0].face_indices[f] - 1), (short)(obj.meshes[0].face_indices[f + 1] - 1), (short)(obj.meshes[0].face_indices[f + 2] - 1) });


                coll_mdl.triangles_list.Add(new block_00.coll_triangle(t[0], t[1], t[2], 4, 0, 0));
            }
            // triangle count
            coll_mdl.triangle_count = coll_mdl.triangles_list.Count;

            #endregion

 
            List<byte[]> vertex_buffer_byte_arrays = new List<byte[]>();
            List<byte[]> triangles_buffer_byte_arrays = new List<byte[]>();

            // serialize vertices into byte array
            for (int v = 0; v < coll_mdl.vertex_list.Count; v++)
            {
                vertex_buffer_byte_arrays.Add(coll_mdl.vertex_list[v].Serialize());
            }

            // serialize triangles into byte array
            for (int t = 0; t < coll_mdl.triangles_list.Count; t++)
            {
                triangles_buffer_byte_arrays.Add(coll_mdl.triangles_list[t].Serialize());
            }


            // merge byte array lists into a single byte array
            byte[] vtx_arr = vertex_buffer_byte_arrays.SelectMany(byteArr => byteArr).ToArray();
            byte[] tri_arr = triangles_buffer_byte_arrays.SelectMany(byteArr => byteArr).ToArray();

            // export vertex and triangles buffers for debugging
            File.WriteAllBytes(@"C:\Users\Mike\Documents\XSI_Projects\JSRF\Models_src\stg00\vtx.bin", vtx_arr);
            File.WriteAllBytes(@"C:\Users\Mike\Documents\XSI_Projects\JSRF\Models_src\stg00\tri.bin", tri_arr);
        }

        // build stage fuke
        public void build(string import_dir)
        {
            #region import collision models into classes instances

            string[] coll_models_dirs = Directory.GetDirectories(import_dir + "Collision\\");

            int models_count = 0;

            block_0 = new block_00();
            //block_0.block_A_headers_list.Add(new block_00.collision_models_list());
            //block_0.block_A_headers_list[0].models_list_start_offset = 128; // debug test as setup in garage Stage

            // for each collision model folder
            for (int i = 0; i < coll_models_dirs.Length; i++)
            {
                // create a collision model group for each folder

                block_0.coll_headers_A_chunk_count++;
                block_0.block_A_headers_list.Add(new block_00.collision_models_list());

                // for each .obj file
                string[] obj_files = System.IO.Directory.GetFiles(coll_models_dirs[i], "*.obj");

                models_count = obj_files.Length;
                for (int o = 0; o < obj_files.Length; o++)
                {
                    OBJ obj = new OBJ(obj_files[o]);

                    // create collision model object
                    block_00.collision_model coll_mdl = new block_00.collision_model();

                    // import vertices buffer
                    for (int v = 0; v < obj.meshes[0].vertex_buffer.Count; v++)
                    {
                        coll_mdl.vertex_list.Add(new block_00.coll_vertex(obj.meshes[0].vertex_buffer[v], 0));
                    }
                    // sert vextex count
                    coll_mdl.vertices_count = coll_mdl.vertex_list.Count;

                    // import triangles buffer
                    // and convert triangles indices with: convert_coll_triangles_indices()
                    for (int f = 0; f < obj.meshes[0].face_indices.Count-2; f+=3)
                    {
                        List<short> t = block_0.convert_coll_triangles_indices(new List<short> { (short)(obj.meshes[0].face_indices[f] -1), (short)(obj.meshes[0].face_indices[f + 1] - 1), (short)(obj.meshes[0].face_indices[f +2] - 1) });
                        

                        coll_mdl.triangles_list.Add(new block_00.coll_triangle(t[0], t[1], t[2], 4, 0, 0));
                        // ###################################################################### todo set extra2
                    }
                    // triangle count
                    coll_mdl.triangle_count = coll_mdl.triangles_list.Count;

                    // add collision model to list to: block_0.block_A_headers_lis[x]
                    block_0.block_A_headers_list[block_0.block_A_headers_list.Count - 1].coll_model_list.Add(coll_mdl);
                    block_0.block_A_headers_list[block_0.block_A_headers_list.Count - 1].models_list_count = block_0.block_A_headers_list[block_0.block_A_headers_list.Count - 1].coll_model_list.Count;
                }
            }

            //block_0.block_A_headers_list.Add(new block_00.collision_models_list());



            #endregion

            #region serialize collision models

            List<byte[]> block_00_byte_arrays = new List<byte[]>();

            block_0.coll_headers_A_offset = 32;
            block_00_byte_arrays.Add(block_0.Serialize());

            // keeps position after each header
            int current_offset = 32;

            for (int i = 0; i < block_0.block_A_headers_list.Count; i++)
            {
                current_offset += 32;
                block_0.block_A_headers_list[i].models_list_count = block_0.block_A_headers_list[i].coll_model_list.Count;
                block_0.block_A_headers_list[i].models_list_start_offset = block_0.block_A_headers_list.Count * 32 + 32; // * 32 + 32;

                /*
                 // this was to set third block as in the Garage setup
                if(i == block_0.block_A_headers_list.Count-1)
                {
                    // set models_list_start_offset as the vertex buffer start
                    block_0.block_A_headers_list[2].models_list_start_offset = (block_0.block_A_headers_list[1].models_list_count * 112) + 4 * 32;
                }
                */

                // serialize block_A_header and add to file
                block_00_byte_arrays.Add(block_0.block_A_headers_list[i].Serialize());

                // add coll model headers size to current_mdl_offset for next block
                current_offset = block_0.block_A_headers_list.Count * 112;
            }


 

            List<byte[]> vertex_buffer_byte_arrays = new List<byte[]>();
            List<byte[]> triangles_buffer_byte_arrays = new List<byte[]>();


            #region create vertices and triangles buffers

            int vertex_buff_size = 0;
            int coll_headers_size = 0;
            // calculate length of vertex buffer (so that we can properly reference triangle offsets)
            for (int i = 0; i < block_0.block_A_headers_list.Count; i++)
            {
                for (int c = 0; c < block_0.block_A_headers_list[i].coll_model_list.Count; c++)
                {
                    vertex_buff_size += block_0.block_A_headers_list[i].coll_model_list[c].vertex_list.Count * 16;
                    coll_headers_size += 112;

                    // serialize vertices into byte array
                    for (int v = 0; v < block_0.block_A_headers_list[i].coll_model_list[c].vertex_list.Count; v++)
                    {
                        vertex_buffer_byte_arrays.Add(block_0.block_A_headers_list[i].coll_model_list[c].vertex_list[v].Serialize());
                    }

                    // serialize triangles into byte array
                    for (int t = 0; t < block_0.block_A_headers_list[i].coll_model_list[c].triangles_list.Count; t++)
                    {
                        triangles_buffer_byte_arrays.Add(block_0.block_A_headers_list[i].coll_model_list[c].triangles_list[t].Serialize());
                    }
                }
            }

            #endregion

            int vertices_offset = coll_headers_size + 64; 
            // add headers size/position + vertex buffer size
            int triangles_offset = coll_headers_size + 64 + vertex_buff_size;

            // for each collision block_A_headers
            for (int i = 0; i < block_0.block_A_headers_list.Count; i++)
            {
                // for each collision model in block_A_header
                for (int c = 0; c < block_0.block_A_headers_list[i].coll_model_list.Count; c++)
                {
                    block_0.block_A_headers_list[i].coll_model_list[c].vertices_start_offset = vertices_offset;
                    block_0.block_A_headers_list[i].coll_model_list[c].triangles_start_offset = triangles_offset;


                    // need to calculate vertex and triangle buffer positions
                    block_00_byte_arrays.Add(block_0.block_A_headers_list[i].coll_model_list[c].Serialize());

                    vertices_offset += block_0.block_A_headers_list[i].coll_model_list[c].vertex_list.Count * 16;
                    triangles_offset += block_0.block_A_headers_list[i].coll_model_list[c].triangles_list.Count * 8;
                }            
            }


            // build vertices/triangles arrays
            block_00_byte_arrays.Add(vertex_buffer_byte_arrays.SelectMany(byteArr => byteArr).ToArray());
            block_00_byte_arrays.Add(triangles_buffer_byte_arrays.SelectMany(byteArr => byteArr).ToArray());

            #endregion

            // merge byte array lists into a single byte array
            byte[] file = block_00_byte_arrays.SelectMany(byteArr => byteArr).ToArray();

            #region calcualte main header block sizes and offsets

            header.block_00_start_offset = 160;
            header.block_00_size = (block_00_byte_arrays).SelectMany(byteArr => byteArr).ToArray().Length;

            header.block_01_start_offset = header.block_00_start_offset+ header.block_00_size +8;
            header.block_01_size = 19272;

            header.block_02_start_offset = header.block_01_start_offset + header.block_01_size + 8;
            header.block_02_size = 10908;

            // file.CopyTo(header.Serialize(), 0);
            byte[] header_arr = header.Serialize();
            file = header_arr.Concat(file).ToArray();


            #endregion


            string preset_data = @"C:\Users\Mike\Desktop\JSRF\game_files\files\ModOR\Stage\stg00__block_01_02.bin";

            Byte[] pres_data = File.ReadAllBytes(preset_data);

            Byte[] arr = file.Concat(pres_data).ToArray();

            File.WriteAllBytes(@"C:\Users\Mike\Desktop\JSRF\game_files\files\ModOR\Stage\stg00_.bin", arr);
        }


        /// <summary>
        ///  JSRF level_xx.bin file Header
        /// </summary>
        public class Level_bin_Header
        {
            // block_00 = grind path/curves data? 
            public Int32 block_00_start_offset { get; set; } // 
            public Int32 block_00_size { get; set; } // 59640

            public Int32 block_01_start_offset { get; set; } // 
            public Int32 block_01_size { get; set; } // 

            public Int32 block_02_start_offset { get; set; } // 
            public Int32 block_02_size { get; set; } // 

            public Int32 Level_Models_count { get; set; } // includes level models and MDLB in the stageXX_XX.dat files
            public Int32 unk { get; set; } // 

            // number of StgXX_XX.dat files
            // public Int32[] stg_dat_counts  { get; set; }
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

            public Int32 unk_156 { get; set; } = 2147347457; // NaN (indicates end of block or header??)



            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();

                b.Add(BitConverter.GetBytes(block_00_start_offset));
                b.Add(BitConverter.GetBytes(block_00_size));

                b.Add(BitConverter.GetBytes(block_01_start_offset));
                b.Add(BitConverter.GetBytes(block_01_size));

                b.Add(BitConverter.GetBytes(block_02_start_offset));
                b.Add(BitConverter.GetBytes(block_02_size));

                b.Add(BitConverter.GetBytes(Level_Models_count));

                b.Add(BitConverter.GetBytes(unk));


                b.Add(BitConverter.GetBytes(unk_32));
                b.Add(BitConverter.GetBytes(unk_36));
                b.Add(BitConverter.GetBytes(unk_40));
                b.Add(BitConverter.GetBytes(unk_44));
                b.Add(BitConverter.GetBytes(unk_48));
                b.Add(BitConverter.GetBytes(unk_52));
                b.Add(BitConverter.GetBytes(unk_56));
                b.Add(BitConverter.GetBytes(unk_60));
                b.Add(BitConverter.GetBytes(unk_64));
                b.Add(BitConverter.GetBytes(unk_68));
                b.Add(BitConverter.GetBytes(unk_72));
                b.Add(BitConverter.GetBytes(unk_76));
                b.Add(BitConverter.GetBytes(unk_80));
                b.Add(BitConverter.GetBytes(unk_84));
                b.Add(BitConverter.GetBytes(unk_88));
                b.Add(BitConverter.GetBytes(unk_92));
                b.Add(BitConverter.GetBytes(unk_96));
                b.Add(BitConverter.GetBytes(unk_100));
                b.Add(BitConverter.GetBytes(unk_104));
                b.Add(BitConverter.GetBytes(unk_108));
                b.Add(BitConverter.GetBytes(unk_112));
                b.Add(BitConverter.GetBytes(unk_116));
                b.Add(BitConverter.GetBytes(unk_120));
                b.Add(BitConverter.GetBytes(unk_124));
                b.Add(BitConverter.GetBytes(unk_128));
                b.Add(BitConverter.GetBytes(unk_132));
                b.Add(BitConverter.GetBytes(unk_136));
                b.Add(BitConverter.GetBytes(unk_140));
                b.Add(BitConverter.GetBytes(unk_144));
                b.Add(BitConverter.GetBytes(unk_148));
                b.Add(BitConverter.GetBytes(unk_152));

                b.Add(BitConverter.GetBytes(unk_156));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }


            block_00 block_00 = new block_00();

            public Level_bin_Header()
            {

            }

            public void set_stg_dat_count(Int32 _val)
            {
                // stg_dat_counts = Enumerable.Repeat(_val, 31).ToArray();
                unk_36 = _val;
                unk_40 = _val;
                unk_44 = _val;
                unk_48 = _val;
                unk_52 = _val;
                unk_56 = _val;
                unk_60 = _val;
                unk_64 = _val;
                unk_68 = _val;
                unk_72 = _val;
                unk_76 = _val;
                unk_80 = _val;
                unk_84 = _val;
                unk_88 = _val;
                unk_92 = _val;
                unk_96 = _val;
                unk_100 = _val;
                unk_104 = _val;
                unk_108 = _val;
                unk_112 = _val;
                unk_116 = _val;
                unk_120 = _val;
                unk_124 = _val;
                unk_128 = _val;
                unk_132 = _val;
                unk_136 = _val;
                unk_140 = _val;
                unk_144 = _val;
                unk_148 = _val;
                unk_152 = _val;
            }


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

            public Int16 unk_16 { get; set; }
            public Int16 unk_18 { get; set; }
            public Int16 unk_20 { get; set; }
            public Int16 unk_22 { get; set; }
            public Int16 unk_24 { get; set; }
            public Int16 unk_26 { get; set; }
            public Int32 unk_28 { get; set; }


            public List<collision_models_list> block_A_headers_list { get; set; }

            public block_00()
            {
                block_A_headers_list = new List<collision_models_list>();
            }


            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();

                coll_headers_A_chunk_count = block_A_headers_list.Count;

                b.Add(BitConverter.GetBytes(coll_headers_A_offset));
                b.Add(BitConverter.GetBytes(coll_headers_A_chunk_count));
                b.Add(BitConverter.GetBytes(unk_08));
                b.Add(BitConverter.GetBytes(unk_12));

                b.Add(BitConverter.GetBytes(unk_16));
                b.Add(BitConverter.GetBytes(unk_18));
                b.Add(BitConverter.GetBytes(unk_20));
                b.Add(BitConverter.GetBytes(unk_22));
                b.Add(BitConverter.GetBytes(unk_24));
                b.Add(BitConverter.GetBytes(unk_26));
                b.Add(BitConverter.GetBytes(unk_28));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }



            /// <summary>
            ///  gives offset to list of "coll_model"s (32 bytes)
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
                    v1 = new Vector3();
                    v2 = new Vector3();
                }


                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    models_list_count = coll_model_list.Count;

                    //b.Add(BitConverter.GetBytes(v1.X)); b.Add(BitConverter.GetBytes(v1.Y)); b.Add(BitConverter.GetBytes(v1.Z));
                    //b.Add(BitConverter.GetBytes(v2.X)); b.Add(BitConverter.GetBytes(v2.Y)); b.Add(BitConverter.GetBytes(v2.Z));
                    b.Add(BitConverter.GetBytes(0.1f)); b.Add(BitConverter.GetBytes(0.1f)); b.Add(BitConverter.GetBytes(0.1f));
                    b.Add(BitConverter.GetBytes(0.1f)); b.Add(BitConverter.GetBytes(0.1f)); b.Add(BitConverter.GetBytes(0.1f));
                    b.Add(BitConverter.GetBytes(models_list_start_offset));
                    b.Add(BitConverter.GetBytes(models_list_count));
          
                    return b.SelectMany(byteArr => byteArr).ToArray();
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
                public Vector3 v1_bbox_min { get; set; }
                public Vector3 v2_bbox_max { get; set; }

                public Vector3 v3 { get; set; }
                public float w3 { get; set; }
                public Vector3 v4 { get; set; }
                public float w4 { get; set; }

                public Vector3 v5 { get; set; }
                public float w5 { get; set; }
                public Vector3 v7_position { get; set; }

                public float f { get; set; } = 1; // offset 84 

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
                    v1_bbox_min = new Vector3();
                    v2_bbox_max = new Vector3();
                    v3 = new Vector3();
                    v4 = new Vector3();
                    v5 = new Vector3();
                    v7_position = new Vector3();
                    vertex_list = new List<coll_vertex>();
                    triangles_list = new List<coll_triangle>();
                }


                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(BitConverter.GetBytes(v1_bbox_min.X)); b.Add(BitConverter.GetBytes(v1_bbox_min.Y)); b.Add(BitConverter.GetBytes(v1_bbox_min.Z));
                    b.Add(BitConverter.GetBytes(v2_bbox_max.X)); b.Add(BitConverter.GetBytes(v2_bbox_max.Y)); b.Add(BitConverter.GetBytes(v2_bbox_max.Z));
                    b.Add(BitConverter.GetBytes(v3.X)); b.Add(BitConverter.GetBytes(v3.Y)); b.Add(BitConverter.GetBytes(v3.Z));
                    b.Add(BitConverter.GetBytes(w3));
                    b.Add(BitConverter.GetBytes(v4.X)); b.Add(BitConverter.GetBytes(v4.Y)); b.Add(BitConverter.GetBytes(v4.Z));
                    b.Add(BitConverter.GetBytes(w4));
                    b.Add(BitConverter.GetBytes(v5.X)); b.Add(BitConverter.GetBytes(v5.Y)); b.Add(BitConverter.GetBytes(v5.Z));
                    b.Add(BitConverter.GetBytes(w5));
                    b.Add(BitConverter.GetBytes(v7_position.X)); b.Add(BitConverter.GetBytes(v7_position.Y)); b.Add(BitConverter.GetBytes(v7_position.Z));

                    b.Add(BitConverter.GetBytes(f));

                    b.Add(BitConverter.GetBytes(vertices_start_offset));
                    b.Add(BitConverter.GetBytes(vertices_count));

                    b.Add(BitConverter.GetBytes(triangles_start_offset));
                    b.Add(BitConverter.GetBytes(triangle_count));

                    b.Add(BitConverter.GetBytes(unk_104));
                    b.Add(BitConverter.GetBytes(unk_108));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }


            }



            /// <summary>
            /// Collision Vertex defition  (16 bytes)
            /// </summary>
            public class coll_vertex
            {
                public Vector3 vert { get; set; }
                public Int32 unk { get; set; } // possibly used to defined "ignore pick grind path" for geind path along half pipes

                public coll_vertex(Vector3 _v, Int32 _unk)
                {
                    this.vert = _v;
                    this.unk = _unk;
                }


                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(BitConverter.GetBytes((float)vert.X));
                    b.Add(BitConverter.GetBytes((float)vert.Y));
                    b.Add(BitConverter.GetBytes((float)vert.Z));
                    b.Add(BitConverter.GetBytes(unk));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }
            }


            public class coll_triangle
            {
                public short a { get; set; }
                public short b { get; set; }
                public short c { get; set; }
                                                       

                public short surface_property { get; set; } = 4; // defines if surface is a wall, floor, stairs, ramp etc
                public byte surface_data_0 { get; set; } = 17; // 
                public byte surface_data_1 { get; set; } = 16; // 

                public coll_triangle(short _a, short _b, short _c, short _surface_property, byte _surface_data_0, byte _surface_data_1)
                {
                    this.a = _a;
                    this.b = _b;
                    this.c = _c;

                    this.surface_property = _surface_property;
                    this.surface_data_0 = _surface_data_0;
                    this.surface_data_1 = _surface_data_1;
                }



                public byte[] Serialize()
                {
                    List<byte[]> bb = new List<byte[]>();

                    byte[] bt = new byte[2];
                    bt[0] = (byte)a;
                    bt[1] = (byte)b;
                    bb.Add(bt);

                    // third triangle index is an int16
                    bb.Add(BitConverter.GetBytes(c)); //int16

                    bb.Add(BitConverter.GetBytes(surface_property)); //int16

                    byte[] bs = new byte[2];
                    bs[0] = (byte)surface_data_0;
                    bs[1] = (byte)surface_data_1;
                    bb.Add(bs);

                    return bb.SelectMany(byteArr => byteArr).ToArray();
                }

            }

            

            public List<short> convert_coll_triangles_indices(List<short> tris)
            {
                short a = tris[0];
                short b = tris[1];
                short c = tris[2];

                short addB = 0;

                // convert triangle's indices to game format
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


        }









        #endregion

        // grind paths
        #region main_block_01

        /// <summary>
        /// contains lists of grind paths, contained within parent objects
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

                export_grind_path_data();
                export_grind_path_data_blender();
            }

            /// <summary>
            /// item_header is a list of [ [item count] and [start offset] ]
            /// each  item_header
            /// </summary>
            public class item_header
            {
                /// <summary>
                /// file structure: 
                /// [int32] item count
                /// [int32]  start offset

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
                    public Vector3 bbBox_A { get; set; } // bounding box poin A
                    public Vector3 bbBox_B { get; set; } // bounding box

                    public List<grind_path_point> grind_path_points { get; set; }

                    /*
                    public grind_path_header()
                    {
                        //grind_path_points = new List<grind_path_point>();
                    }
                    */

                    /*
                    public grind_path_header(byte[] data, Int32 start_offset)
                    {
                    
                        this.grind_points_list_start_offset = BitConverter.ToInt32(data, start_offset);
                        this.grind_points_count = BitConverter.ToInt32(data, start_offset);
                        this.unk_8 = BitConverter.ToInt16(data, start_offset);
                        this.unk_10 = BitConverter.ToInt16(data, start_offset);
                        this.bbBox_A = BitConverter.ToInt32(data, start_offset);
                        this.bbBox_B = BitConverter.ToInt32(data, start_offset);
                    
                    }
                    */

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


            private void export_grind_path_data()
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

                System.IO.File.WriteAllLines(@"C:\Users\Mike\Desktop\JSRF\research\stage_bin\grind_paths.txt", lines);

            }

            private void export_grind_path_data_blender()
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
                                }
                            }

                            line_point_index += grind_path_item.grind_path_points.Count;

                            lines.Add("");
                            grind_path_item_count++;
                        }
                    }
                    item_count++;

                }

                System.IO.File.WriteAllLines(@"C:\Users\Mike\Desktop\JSRF\research\stage_bin\grind_paths_blender.obj", lines);

            }
        }

        #endregion

        // spawns
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
            // so its probably a box to cull and show/hide the model if its on the field of view or not
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
}
