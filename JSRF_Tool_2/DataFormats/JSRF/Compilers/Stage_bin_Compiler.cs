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
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace JSRF_ModTool.DataFormats.JSRF
{
    /// <summary>
    ///  JSRF Stage_00.bin file format class
    /// </summary>
    /// <remarks>
    ///  Examples of this file type:
    ///  Media\Stage\stg00_.bin
    ///  Media\Stage\stg10_.bin
    ///  Media\Stage\stg11_.bin
    ///  ...
    /// This filetype contains 3 main blocks of data.
    /// It contains data such as, (block_00): Stage physics collision, (block_01): grind curve paths, (block_02): prop/decals/level_models_instance (StgObj models) placement/spawn lists etc
    /// </remarks>
    public class Stage_bin_Compiler
    {
        public Stage_bin_Header header = new Stage_bin_Header();

        //public block_00 block_0; // Stage physics collision 3d model data
        //public block_01 block_1; // grind paths
        //public block_02 block_2; // object spawns (spawns/positions MDLB containeed in StgXX_XX.dat and/or StgObj)

        private int vis_models_count;

        public Stage_bin_Compiler(int _vis_models_count)
        {
            vis_models_count = _vis_models_count;
        }


        // build stage bin file
        public void build(string source_dir, string media_dir, string stage_num)
        {
            byte[] block_00_data = new block_00().build(source_dir);

            header.block_00_start_offset = 160;
            header.block_00_size = block_00_data.Length;

            // File.WriteAllBytes(Path.Combine(@"C:\Users\Mike\Desktop\JSRF\research\Stg_Collision\block_00_compiled_test.dat"), block_00_data);

            // import and build grind paths (block_01)
            byte[] block_01_data = new block_01().build(Path.Combine(source_dir + "grind_paths.txt")); //@"C:\Users\Mike\Desktop\JSRF\Stg_Compiles\Stg_SkatePark\GrindPaths\grind_paths.txt"

            // File.WriteAllBytes(@"C:\Users\Mike\Desktop\JSRF\research\Stg_Collision\block_01_compiled_test.dat", block_01_data);


            // block sizes do not account for the 8 byte flags after the block
            header.block_01_start_offset = header.block_00_start_offset + header.block_00_size + 8; // +8 bytes are: NaN + Uknown number(this number increases of +1 per Stage)
            header.block_01_size = block_01_data.Length;



            #region load block_2 from binary resource file


            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream fs;
            fs = asm.GetManifestResourceStream("JSRF_ModTool.DataFormats.JSRF.Compilers.Data.block_02_base.bin");


            if (fs == null) { return; }

            // convert stream to byte array
            byte[] result;
            using (var streamReader = new MemoryStream())
            {
                fs.CopyTo(streamReader);
                result = streamReader.ToArray();
            }

            fs.Dispose();


            // import block_02 copied & extracted from Stg00_.bin
            byte[] block_02_data = result;//File.ReadAllBytes(@"C:\Users\Mike\Desktop\JSRF\research\Stg_bin_block_02\stg52_bin__block_02.bin");
            header.block_02_start_offset = header.block_01_start_offset + header.block_01_size + 8; // +8 bytes are: NaN + Uknown number(the number increase of +1 per Stage)
            header.block_02_size = block_02_data.Length; // -4 substract NaN flag


            #endregion

            #region create block_2 from scratch
            /*
            block_02 block_02 = new block_02();
            // import block_02 copied & extracted from Stg00_.bin
            byte[] block_02_data = block_02.build(vis_models_count);
            header.block_02_start_offset = header.block_01_start_offset + header.block_01_size + 8; // +8 bytes are: NaN + Uknown number(the number increase of +1 per Stage)
            header.block_02_size = block_02_data.Length; // -4 substract NaN flag
            */

            /*
            // write block to file for debugging
            try
            {
                File.WriteAllBytes(@"C:\Users\Mike\Desktop\JSRF\research\Stg_Collision\block_02_compiled_test.dat", block_02_data);
            }
            catch(Exception ex)
            {
                //throw ex;
            }
            */

            #endregion

            #region serialize and write to file

            List<byte[]> buffer = new List<byte[]>();

            // serialize and add header to byte[] list
            buffer.Add(header.Serialize());
            // unknown NaN (1.#QNAN) flag indicating end/start? of block
            buffer.Add(BitConverter.GetBytes((Int32)2147347457));


            // add block_00 to byte[] list
            buffer.Add(block_00_data);
            // unknown NaN (1.#QNAN) flag indicating end/start? of block
            buffer.Add(BitConverter.GetBytes((Int32)2147347457));
            // unknown value, seems to increase as we get to higher number of StgXX_.bin, Stg00_.bin has 66 for this value
            buffer.Add(BitConverter.GetBytes((Int32)66));


            // add block_01 to byte[] list
            buffer.Add(block_01_data);
            // unknown NaN (1.#QNAN) flag indicating end/start? of block
            buffer.Add(BitConverter.GetBytes((Int32)2147347457));
            // unknown value, seems to increase as we get to higher number of StgXX_.bin, Stg00_.bin has 66 for this value
            buffer.Add(BitConverter.GetBytes((Int32)66));


            //File.WriteAllBytes(@"C:\Users\Mike\Desktop\JSRF\research\Stg_bin_block_02\block_02_compiled_test.dat", block_02_data);

            // add block_02 to byte[] list
            buffer.Add(block_02_data);
            // unknown NaN (1.#QNAN) flag indicating end/start? of block
            // disabled for now, since block_02_data is sampled from Stg00_.bin and already contains NaN end flag
            //buffer.Add(BitConverter.GetBytes((Int32)2147347457));

            File.WriteAllBytes(media_dir + @"Stage\" + stage_num + ".bin", buffer.SelectMany(byteArr => byteArr).ToArray());
            // File.WriteAllBytes(@"C:\Users\Mike\Desktop\JSRF\game_files\ModOR\Stage\stg00_.bin", buffer.SelectMany(byteArr => byteArr).ToArray());

            #endregion
        }


        #region header

        //  JSRF Stage_xx.bin file Header
        public class Stage_bin_Header
        {
            // block_00 = grind path/curves data? 
            public Int32 block_00_start_offset { get; set; } // 
            public Int32 block_00_size { get; set; } // 59640

            public Int32 block_01_start_offset { get; set; } // 
            public Int32 block_01_size { get; set; } // 

            public Int32 block_02_start_offset { get; set; } // 
            public Int32 block_02_size { get; set; } // 

            public Int32 Stage_Models_count { get; set; } = 28; // includes Stage models and MDLB in the stageXX_XX.dat files, make it 28 minimum (28 is the garage's number of stage models, if it's set any lower than 28 the game crashes on load) 
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

            //public Int32 unk_156 { get; set; } = 2147347457; // NaN (indicates end of block or header??)

            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();

                b.Add(BitConverter.GetBytes(block_00_start_offset));
                b.Add(BitConverter.GetBytes(block_00_size));

                b.Add(BitConverter.GetBytes(block_01_start_offset));
                b.Add(BitConverter.GetBytes(block_01_size));

                b.Add(BitConverter.GetBytes(block_02_start_offset));
                b.Add(BitConverter.GetBytes(block_02_size));

                b.Add(BitConverter.GetBytes(Stage_Models_count));

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

                //b.Add(BitConverter.GetBytes(unk_156));

                return b.SelectMany(byteArr => byteArr).ToArray();
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

        #endregion

        // Stage physics collision 3d model data
        #region main_block_00

        /// <summary>
        /// block 00 - collision models
        /// </summary>
        public class block_00
        {
            public int coll_headers_A_offset { get; set; }
            public int coll_headers_A_chunk_count { get; set; }

            public int unk_08 { get; set; }
            public int unk_12 { get; set; }

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

            public byte[] build(string import_dir)
            {
                block_00 block_0 = new block_00();

                #region block_00 : import collision models into classes instances

                Directory.CreateDirectory(import_dir);
                string[] coll_models_dirs = Directory.GetDirectories(import_dir + "Collision\\");

                int models_count = 0;

                // for each collision model folder/group
                for (int i = 0; i < coll_models_dirs.Length; i++)
                {
                    // create a collision model group for each folder
                    block_0.coll_headers_A_chunk_count++;
                    block_00.collision_models_list coll_model_list = new block_00.collision_models_list();
                    block_0.block_A_headers_list.Add(coll_model_list);

                    // for each .obj file
                    string[] obj_files = System.IO.Directory.GetFiles(coll_models_dirs[i], "*.obj");

                    block_00.surface_properties_list SurfProps = new block_00.surface_properties_list();
                    // global bounding box (a bbox of all meshes combined)
                    bounds bboxG = new bounds();

                    models_count = obj_files.Length;
                    for (int o = 0; o < obj_files.Length; o++)
                    {
                        OBJ obj = new OBJ(obj_files[o]);

                        if (!obj.imported_succeeded)
                        {
                            // no need for a messagebox here, the OBJ class already gives a messagebox with the info on where/what failed
                            return new byte[0];
                        }

                        // create collision model object
                        block_00.collision_model coll_mdl = new block_00.collision_model();

                        // sum up bounding boxes to get the bouding box of all meshes
                        bboxG.add_AB(obj.mesh.bbox_A, obj.mesh.bbox_B);
                        // set mesh bouding box
                        coll_mdl.bbox_A = obj.mesh.bbox_A;
                        coll_mdl.bbox_B = obj.mesh.bbox_B;
                      
                        // import vertices buffer
                        for (int v = 0; v < obj.mesh.vertex_buffer.Count; v++)
                        {
                            Vector3 vert = obj.mesh.vertex_buffer[v];
                            coll_mdl.vertex_list.Add(new block_00.coll_vertex(new Vector3(vert.X, vert.Y, vert.Z), 0)); //vert.Z *-1

                        }
                        // sert vextex count
                        coll_mdl.vertices_count = coll_mdl.vertex_list.Count;

                        int mat_count = 0;
                        // loop through mesh[] faces(triangles)
                        // import triangles buffer
                        for (int f = 0; f < obj.mesh.face_indices.Count; f += 3)
                        {
                            // cache the triangle's vertex indices as vtx_id*
                            int vtx_id1 = obj.mesh.face_indices[f] - 1;
                            int vtx_id2 = obj.mesh.face_indices[f + 1] - 1;
                            int vtx_id3 = obj.mesh.face_indices[f + 2] - 1;

                            block_00.coll_triangle tri = new block_00.coll_triangle();
                            // set triangle's vertex indices
                            tri.index1 = (UInt32)vtx_id1;
                            tri.index2 = (UInt32)vtx_id2;
                            tri.index3 = (UInt32)vtx_id3;

                            // get mesh's materal name // surface property material
                            //string mat_name = obj.mesh.materials[f];
                            string mat_name = obj.mesh.materials[f]; // obj.mesh.material_groups[0].mat_name


                            mat_count++;

                            // set tri.surface_properties with the surface property number based on the mesh's material name
                            // SurfProps.get_ID_byName() returns the number/ID of the surface_property, given the material name (i.e: "surfprop_wall" = 4  or "surfprop_floor" = 1 etc) 
                            // the types of surface properties are defined in: block_00.surface_properties_list
                            tri.surface_properties += (ushort)SurfProps.get_ID_byName(mat_name); //.Replace("surfprop_", "")


                            // cache triangle's points positions
                            Vector3 p1 = coll_mdl.vertex_list[vtx_id1].vert;
                            Vector3 p2 = coll_mdl.vertex_list[vtx_id2].vert;
                            Vector3 p3 = coll_mdl.vertex_list[vtx_id3].vert;

                            // calculate bounding box for the triangle
                            // the collision triangle has its own bounding box
                            // with point A and B, where the bbx point A(min) B(max) is defined
                            // by indices from 0 to 2 which indicate the vertex position component:
                            // position.X or position.Y or position.Z
                            // see this the following image for a simpler explanation: https://www.dropbox.com/s/a8rydbv9gtbz2cd/jsrf_collision_tri_extra2.png
                            bbox_vars b = new bbox_vars();

                            // we pass b, index and point(0,1,2) to evaluate and get the point's position x y z  min/max values
                            // so we update b. and keep the smallest/largest values of X, Y, Z for points p0 p1 p2
                            // we also determine which: X or Y or Z component is picked to define which X Y Z and vertex position (p1 p2 p3) 
                            // is used to define the triangle's bounding box
                            // where the indices (0 1 2) are  p1 = 0   p2 = 1   and p3 = 2
                            b = get_bbox_MinMax(b, 0, p1);
                            b = get_bbox_MinMax(b, 1, p2);
                            b = get_bbox_MinMax(b, 2, p3);

                            // set triangle's bounding box min/max vertex component(X,Y,Z) indices
                            tri.index_of_vertex_with_min_x = b.id_minX;
                            tri.index_of_vertex_with_min_y = b.id_minY;
                            tri.index_of_vertex_with_min_z = b.id_minZ;
                            tri.index_of_vertex_with_max_x = b.id_maxX;
                            tri.index_of_vertex_with_max_y = b.id_maxY;
                            tri.index_of_vertex_with_max_z = b.id_maxZ;
                            // add collision triangle "tri" to list of triangles
                            coll_mdl.triangles_list.Add(tri);
                        }

                        // set triangle count in coll_mdl
                        coll_mdl.triangle_count = coll_mdl.triangles_list.Count;

                        /*
                        // check if bounding box vertex component indices (tri.index_of_vertex_with_***_*) are correct or not
                        if (block_00.checkIF_coll_tri_aabb_HasErrors(coll_mdl))
                        {
                            string is_incorrect = "true";
                        }
                        */

                        // add collision model to list to: block_0.block_A_headers_lis[x]
                        block_0.block_A_headers_list[i].coll_model_list.Add(coll_mdl);
                        block_0.block_A_headers_list[i].models_list_count = block_0.block_A_headers_list[i].coll_model_list.Count;
                    }


                    // manually define collision mesh's bounding box
                    // coll_model_list.v1 = new Vector3(-472.8279f, -137.3619f, -422.7164f);
                    // coll_model_list.v2 = new Vector3(505.2245f, 200.9728f, 399.7578f);

                    // set global bounding box (bbox of all the meshes bbox combined)
                    coll_model_list.bbox_A = bboxG.bounding_box.A;
                    coll_model_list.bbox_B = bboxG.bounding_box.B;
                }

                #endregion

                #region serialize collision models

                List<byte[]> block_00_byte_arrays = new List<byte[]>();

                block_0.coll_headers_A_offset = 32;
                block_00_byte_arrays.Add(block_0.Serialize());

                int prev_models_list_count = 0;
                //int add = 0;

                int coll_list_headers_size = 0;

                for (int i = 0; i < block_0.block_A_headers_list.Count; i++)
                {
                    block_0.block_A_headers_list[i].models_list_count = block_0.block_A_headers_list[i].coll_model_list.Count;
                    // calculate models list start offset // 112 = size of (block_A_headers_list block) + 32 = size of (block_A_headers_list)
                    block_0.block_A_headers_list[i].models_list_start_offset = (prev_models_list_count * 112) + (block_0.block_A_headers_list.Count * 32) + 32;

                    coll_list_headers_size += 32;

                    // serialize block_A_header and add to file
                    block_00_byte_arrays.Add(block_0.block_A_headers_list[i].Serialize());
                    prev_models_list_count++;  //= block_0.block_A_headers_list[i].models_list_count;
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
                            triangles_buffer_byte_arrays.Add(block_0.block_A_headers_list[i].coll_model_list[c].triangles_list[t].raw_data.Serialize());
                        }
                    }
                }

                #endregion

                coll_headers_size += coll_list_headers_size + 32; // + 32 bytes is to account for block_00's main header

                int vertices_offset = coll_headers_size;
                // add headers size/position + vertex buffer size
                int triangles_offset = coll_headers_size + vertex_buff_size;

                // for each collision block_A_headers
                for (int i = 0; i < block_0.block_A_headers_list.Count; i++)
                {
                    // for each collision model in block_A_header
                    for (int c = 0; c < block_0.block_A_headers_list[i].coll_model_list.Count; c++)
                    {
                        // set vertex/triangle buffers offsets
                        block_0.block_A_headers_list[i].coll_model_list[c].vertices_start_offset = vertices_offset;
                        block_0.block_A_headers_list[i].coll_model_list[c].triangles_start_offset = triangles_offset;

                        // serialize collision model
                        block_00_byte_arrays.Add(block_0.block_A_headers_list[i].coll_model_list[c].Serialize());

                        // update vert/tri offsets
                        vertices_offset += block_0.block_A_headers_list[i].coll_model_list[c].vertex_list.Count * 16;
                        triangles_offset += block_0.block_A_headers_list[i].coll_model_list[c].triangles_list.Count * 8;
                    }
                }


                // serialize vertex and triangle lists
                block_00_byte_arrays.Add(vertex_buffer_byte_arrays.SelectMany(byteArr => byteArr).ToArray());
                block_00_byte_arrays.Add(triangles_buffer_byte_arrays.SelectMany(byteArr => byteArr).ToArray());

                #endregion

                // merge byte array lists into a single by array and return it
                return block_00_byte_arrays.SelectMany(byteArr => byteArr).ToArray();
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


            public class collision_models_list // (32 bytes)
            {
                public Vector3 bbox_A { get; set; }
                public Vector3 bbox_B { get; set; }

                public int models_list_start_offset { get; set; }
                public int models_list_count { get; set; }

                public List<collision_model> coll_model_list { get; set; }

                public collision_models_list()
                {
                    coll_model_list = new List<collision_model>();
                }

                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    models_list_count = coll_model_list.Count;

                    b.Add(BitConverter.GetBytes(bbox_A.X)); b.Add(BitConverter.GetBytes(bbox_A.Y)); b.Add(BitConverter.GetBytes(bbox_A.Z)); // v1
                    b.Add(BitConverter.GetBytes(bbox_B.X)); b.Add(BitConverter.GetBytes(bbox_B.Y)); b.Add(BitConverter.GetBytes(bbox_B.Z)); // v2
                    b.Add(BitConverter.GetBytes(models_list_start_offset));
                    b.Add(BitConverter.GetBytes(models_list_count));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }
            }

            public class collision_model // (112 bytes)
            {
                public Vector3 bbox_A { get; set; }
                public Vector3 bbox_B { get; set; }

                // v3 v4 v5 are a transform matrix
                public Vector3 v3 { get; set; }
                public float w3 { get; set; }

                public Vector3 v4 { get; set; }
                public float w4 { get; set; }

                public Vector3 v5 { get; set; }
                public float w5 { get; set; }

                public Vector3 v7_position { get; set; }

                public float f { get; set; }

                public int vertices_start_offset { get; set; }
                public int vertices_count { get; set; }

                public int triangles_start_offset { get; set; }
                public int triangle_count { get; set; }

                public int unk_104 { get; set; }
                public int unk_108 { get; set; }

                public List<coll_vertex> vertex_list { get; set; }
                public List<coll_triangle> triangles_list { get; set; }

                public collision_model()
                {
                    bbox_A = new Vector3();
                    bbox_B = new Vector3();

                    f = 1;

                    // set neutral transform matrix
                    v3 = new Vector3(1, 0, 0);
                    v4 = new Vector3(0, 1, 0);
                    v5 = new Vector3(0, 0, 1);

                    v7_position = new Vector3();
                    vertex_list = new List<coll_vertex>();
                    triangles_list = new List<coll_triangle>();
                }

                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(BitConverter.GetBytes(bbox_A.X)); b.Add(BitConverter.GetBytes(bbox_A.Y)); b.Add(BitConverter.GetBytes(bbox_A.Z));
                    b.Add(BitConverter.GetBytes(bbox_B.X)); b.Add(BitConverter.GetBytes(bbox_B.Y)); b.Add(BitConverter.GetBytes(bbox_B.Z));

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

            public class coll_vertex
            {
                public Vector3 vert { get; set; }
                public int unk { get; set; }

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
                public coll_triangle(raw in_raw)
                {
                    raw_data = in_raw;
                }

                public coll_triangle()
                {
                    raw_data = new raw();
                }

                public class raw
                {
                    public System.UInt32 indices_raw { get; set; }

                    // Bounds indices and surface flags
                    public System.UInt32 other_data { get; set; }

                    public raw()
                    {
                        indices_raw = 0;
                        other_data = 0;
                    }

                    public byte[] Serialize()
                    {
                        List<byte[]> b = new List<byte[]>();

                        b.Add(BitConverter.GetBytes(indices_raw));
                        b.Add(BitConverter.GetBytes(other_data));

                        return b.SelectMany(byteArr => byteArr).ToArray();
                    }
                }

                public raw raw_data;

                // Member access 
                public System.UInt32 index1
                {
                    get { return raw_data.indices_raw & 0x3ffU; }
                    set { SafelyCheckIndex(value); raw_data.indices_raw &= ~0x3ffU; raw_data.indices_raw |= value; }
                }
                public System.UInt32 index2
                {
                    get { return (raw_data.indices_raw >> 10) & 0x3ffU; }
                    set { SafelyCheckIndex(value); raw_data.indices_raw &= ~(0x3ffU << 10); raw_data.indices_raw |= (value << 10); }
                }
                public System.UInt32 index3
                {
                    get { return (raw_data.indices_raw >> 20) & 0x3ffU; }
                    set { SafelyCheckIndex(value); raw_data.indices_raw &= ~(0x3ffU << 20); raw_data.indices_raw |= (value << 20); }
                }
                public System.UInt32 indices_raw_leftover
                {
                    get { return (raw_data.indices_raw >> 30) & 0x3U; }
                    set { SafelyCheckInteger(value, 2); raw_data.indices_raw &= ~(0x3U << 30); raw_data.indices_raw |= (value << 30); }
                }

                public System.UInt16 surface_properties
                {
                    get { return (System.UInt16)(raw_data.other_data & 0xffffU); }
                    set { raw_data.other_data &= ~0xffffU; raw_data.other_data |= value; }
                }

                public System.Byte index_of_vertex_with_min_x
                {
                    get { return SafelyGetBoundsIndex((raw_data.other_data >> 16) & 3U); }
                    set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 16); raw_data.other_data |= ((value & 0x3U) << 16); }
                }

                public System.Byte index_of_vertex_with_max_x
                {
                    get { return SafelyGetBoundsIndex((raw_data.other_data >> 18) & 3U); }
                    set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 18); raw_data.other_data |= ((value & 0x3U) << 18); }
                }
                public System.Byte index_of_vertex_with_min_y
                {
                    get { return SafelyGetBoundsIndex((raw_data.other_data >> 20) & 3U); }
                    set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 20); raw_data.other_data |= ((value & 0x3U) << 20); }
                }
                public System.Byte index_of_vertex_with_max_y
                {
                    get { return SafelyGetBoundsIndex((raw_data.other_data >> 22) & 3U); }
                    set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 22); raw_data.other_data |= ((value & 0x3U) << 22); }
                }
                public System.Byte index_of_vertex_with_min_z
                {
                    get { return SafelyGetBoundsIndex((raw_data.other_data >> 24) & 3U); }
                    set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 24); raw_data.other_data |= ((value & 0x3U) << 24); }
                }
                public System.Byte index_of_vertex_with_max_z
                {
                    get { return SafelyGetBoundsIndex((raw_data.other_data >> 26) & 3U); }
                    set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 26); raw_data.other_data |= ((value & 0x3U) << 26); }
                }
                public System.Byte other_data_leftover
                {
                    get { return SafelyGetBoundsIndex((raw_data.other_data >> 28) & 3U); }
                    set { SafelyCheckInteger(value, 4); raw_data.other_data &= ~(0xfU << 28); raw_data.other_data |= ((value & 0xfU) << 28); }
                }

                // Helpers

                public System.UInt32 GetVertexIndex(System.Byte index)
                {
                    SafelyCheckBoundsIndex(index);
                    int shift = 10 * index;
                    return (raw_data.indices_raw >> shift) & 0x3ffU;
                }

                public static System.Byte SafelyGetBoundsIndex(System.UInt32 value)
                {
                    SafelyCheckBoundsIndex((System.Byte)value);
                    return (System.Byte)value;
                }
                public static void SafelyCheckInteger(System.UInt32 value, System.UInt32 bit_size)
                {
                    var limit = (1U << (int)bit_size) - 1;
                    if (value > limit)
                    {
                        throw new System.IndexOutOfRangeException(string.Format("{0} is > {1}", value, limit));
                    }
                }

                public static void SafelyCheckBoundsIndex(System.Byte value)
                {
                    if (value > 2)
                    {
                        throw new System.IndexOutOfRangeException("Index can only be 0, 1 or 2. Triangle has only three indices");
                    }
                }

                public static void SafelyCheckIndex(System.UInt32 value)
                {
                    if (value > 0x3ffU)
                    {
                        throw new System.IndexOutOfRangeException(string.Format("Error: could not compile the collision mesh, because a model group exceeds the maximum triangles limit (1024),\n" +
                                                                                "please separate the collision meshes into multiple groups with a maximum of 1024 triangles per group.", value)); //Index can be max 1023, but was {0}
                    }
                }

                public string ToTableRow()
                {
                    return string.Format("{0,6} {1,6} {2,6} {3,8} {4,8} {5,8} {6,8} {7,8} {8,8} {9,9}", index1, index2, index3, index_of_vertex_with_min_x,
                        index_of_vertex_with_max_x, index_of_vertex_with_min_y, index_of_vertex_with_max_y, index_of_vertex_with_min_z, index_of_vertex_with_max_z, surface_properties);
                }
            }


            private class AABB
            {
                public Vector3 min { get; set; } = new Vector3();
                public Vector3 max { get; set; } = new Vector3();

                public static AABB calculate_triangle_aabb(Vector3 v1, Vector3 v2, Vector3 v3)
                {
                    AABB ret = new AABB();
                    ret.min.X = System.Math.Min(v1.X, System.Math.Min(v2.X, v3.X));
                    ret.min.Y = System.Math.Min(v1.Y, System.Math.Min(v2.Y, v3.Y));
                    ret.min.Z = System.Math.Min(v1.Z, System.Math.Min(v2.Z, v3.Z));

                    ret.max.X = System.Math.Max(v1.X, System.Math.Max(v2.X, v3.X));
                    ret.max.Y = System.Math.Max(v1.Y, System.Math.Max(v2.Y, v3.Y));
                    ret.max.Z = System.Math.Max(v1.Z, System.Math.Max(v2.Z, v3.Z));
                    return ret;
                }

                public override bool Equals(System.Object obj)
                {
                    if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                    {
                        return false;
                    }
                    AABB other = (AABB)obj;
                    return min.X == other.min.X && min.Y == other.min.Y && min.Z == other.min.Z && max.X == other.max.X && max.Y == other.max.Y && max.Z == other.max.Z;
                }
            }

            // check if collision's bounding box vertex component indices are correct
            private static bool checkIF_coll_tri_aabb_HasErrors(collision_model coll_model)
            {
                for (int i = 0; i < coll_model.triangles_list.Count; i++)
                {
                    coll_triangle coll_triangle = coll_model.triangles_list[i];

                    AABB uncompressed_aabb = new AABB();
                    uncompressed_aabb.min.X = coll_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_min_x)].vert.X;
                    uncompressed_aabb.min.Y = coll_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_min_y)].vert.Y;
                    uncompressed_aabb.min.Z = coll_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_min_z)].vert.Z;
                    uncompressed_aabb.max.X = coll_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_max_x)].vert.X;
                    uncompressed_aabb.max.Y = coll_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_max_y)].vert.Y;
                    uncompressed_aabb.max.Z = coll_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_max_z)].vert.Z;

                    if (!uncompressed_aabb.Equals(AABB.calculate_triangle_aabb(coll_model.vertex_list[(int)coll_triangle.index1].vert, coll_model.vertex_list[(int)coll_triangle.index2].vert, coll_model.vertex_list[(int)coll_triangle.index3].vert)))
                    {
                        //  throw new System.Exception();
                        System.Windows.MessageBox.Show(string.Format("Collision model with " + coll_model.triangle_count + " triangles has incorrect bbox data.\n\n" + i));
                        // has errors - return true
                        //   return true;
                    }
                }

                return false;
            }


            // bouding box and properties needed to calculate the bounding box
            // of a collision triangle
            private class bbox_vars
            {
                public Vector3 min { get; set; }
                public Vector3 max { get; set; }

                public byte id_minX { get; set; }
                public byte id_minY { get; set; }
                public byte id_minZ { get; set; }

                public byte id_maxX { get; set; }
                public byte id_maxY { get; set; }
                public byte id_maxZ { get; set; }

                public bbox_vars(bbox_vars _b)
                {
                    min = _b.min;
                    max = _b.max;

                    id_minX = _b.id_minX;
                    id_minY = _b.id_minY;
                    id_minZ = _b.id_minZ;

                    id_maxX = _b.id_maxX;
                    id_maxY = _b.id_maxY;
                    id_maxZ = _b.id_maxZ;
                }

                public bbox_vars()
                {
                    // set the values to infinite, by default they were set to 0 and in that case the initial comparison wouldn't work as expected
                    min = new Vector3(Single.PositiveInfinity, Single.PositiveInfinity, Single.PositiveInfinity);
                    max = new Vector3(Single.NegativeInfinity, Single.NegativeInfinity, Single.NegativeInfinity);
                }
            }

            // returns min/max X Y Z values and index of vertex component that defines the triangle's bounding box
            // by component we mean X, Y or Z, where the index "i" is a value from 0 to 2
            // for for instance the first point of the triangle will defined as "0"
            // let's say in this case "b.max.X < p.X" is true. then "id_maxX" aka the
            // highest value for X (out of all the mesh points) will be used to define X in the Vector3 point "B" of the bounding box
            // b.id_min* will define the XYZ components used to define the Vector3 point "A" of the bounding box (for the triangle)
            // see this image, that explains it better: https://www.dropbox.com/s/a8rydbv9gtbz2cd/jsrf_collision_tri_extra2.png
            private bbox_vars get_bbox_MinMax(bbox_vars b, int i, Vector3 p)
            {
                if (b.max.X < p.X)
                {
                    b.max.X = p.X;
                    b.id_maxX = (byte)i;
                }

                if (b.max.Y < p.Y)
                {
                    b.max.Y = p.Y;
                    b.id_maxY = (byte)i;
                }

                if (b.max.Z < p.Z)
                {
                    b.max.Z = p.Z;
                    b.id_maxZ = (byte)i;
                }

                if (b.min.X > p.X)
                {
                    b.min.X = p.X;
                    b.id_minX = (byte)i;
                }

                if (b.min.Y > p.Y)
                {
                    b.min.Y = p.Y;
                    b.id_minY = (byte)i;
                }

                if (b.min.Z > p.Z)
                {
                    b.min.Z = p.Z;
                    b.id_minZ = (byte)i;
                }

                return b;
            }


            public class surface_properties_list
            {
                public List<surfprop_item> items { get; set; }

                public surface_properties_list()
                {
                    this.items = new List<surfprop_item>();
                    items.Add(new surfprop_item(0,    "surfprop_pass_through",  "90 45 109"));
                    items.Add(new surfprop_item(1,    "surfprop_floor",         "65 138 214"));
                    items.Add(new surfprop_item(4,    "surfprop_wall",          "214 138 65"));
                    items.Add(new surfprop_item(16,   "surfprop_stairs",        "218 225 40"));
                    items.Add(new surfprop_item(32,   "surfprop_billboard",     "44 225 40"));
                    items.Add(new surfprop_item(128,  "surfprop_halfpipe",      "23 23 132"));
                    items.Add(new surfprop_item(256,  "surfprop_ceiling",       "0 254 232"));
                    items.Add(new surfprop_item(512,  "surfprop_untouchable",   "225 40 40"));
                    items.Add(new surfprop_item(8192, "surfprop_ramp",          "205 40 225"));
                }

                public struct surfprop_item
                {
                    public int num { get; set; }
                    public string name { get; set; }
                    public string color { get; set; }

                    public surfprop_item(int _num, string _name, string _color)
                    {
                        this.num = _num;
                        this.name = _name;
                        this.color = _color;
                    }
                }

                public int get_ID_byName(string name)
                {
                    name = name.ToLower();

                    if (name.Contains("surfprop_passthrough")) { return 0; }
                    if (name.Contains("surfprop_floor"))       { return 1; }
                    if (name.Contains("surfprop_wall"))        { return 4; }
                    if (name.Contains("surfprop_stairs"))      { return 16; }
                    if (name.Contains("surfprop_billboard"))   { return 32; }
                    if (name.Contains("surfprop_halfpipe"))    { return 128; }
                    if (name.Contains("surfprop_ceiling"))     { return 256; }
                    if (name.Contains("surfprop_untouchable")) { return 512; }
                    if (name.Contains("surfprop_ramp"))        { return 8192; }
                    
                    return 0;
                }
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
            public Int32 unk_id { get; set; } = 4;
            // 8192 bytes list of [8 bytes blocks] of item count + start offset (1024 items slots)
            public List<item_header> items { get; set; }

            public List<Int32> gp_headers_offsets_list { get; set; } = new List<int>();

            public byte[] build(string curves_filepath)
            {
                // import grind_paths from text file
               List<grind_path_data> imported_grind_paths_list = import_grind_paths_data(curves_filepath);

                items = new List<item_header>();

                // grind paths are defined by groups containing multiple grind paths
                List<List<grind_path_data>> gp_groups = new List<List<grind_path_data>>();
                List<grind_path_data> gp_group = new List<grind_path_data>();
                int current_group = 0;


                #region group grind paths as specified by gp.parent

                // group grind_paths according to grind_path_data.parent
                for (int i = 0; i < imported_grind_paths_list.Count; i++)
                {
                    grind_path_data gp = imported_grind_paths_list[i];
                    
                    // if moving into a different group
                    if (gp.parent != current_group)
                    {
                        current_group = gp.parent;

                        // single item in group
                        if (gp_group.Count == 0)
                        {
                            gp_groups.Add(new List<grind_path_data> { gp });
                            continue;
                        }
                        // add grouped grind paths to gp_groups list 
                        gp_groups.Add(gp_group);
                        gp_group = new List<grind_path_data>();
                        gp_group.Add(gp);

                        // if it's the last item in the list, add it (so we don't miss it as the loop ends)
                        if (i >= imported_grind_paths_list.Count - 1)
                        {
                            gp_groups.Add(gp_group);
                        }
                        continue;
                    }

                    // add grind path to gp_group list
                    gp_group.Add(gp);

                    // if it's the last item in the list, add it (so we don't miss it as the loop ends)
                    if (i >= imported_grind_paths_list.Count - 1)
                    {
                        gp_groups.Add(gp_group);
                    }
                }

                #endregion

                // clear imported data, as we've transferred it to gp_groups<<grind_path_data>>
                imported_grind_paths_list.Clear();


                // Generate list of items
                #region create item_header list and calculate the item_header_pos offset

                const int HashtableElementCount = 1024;

                List<UInt32>[] incident_list = new List<UInt32>[HashtableElementCount];
                int incident_list_occupancy = 0;
                for (int i = 0; i < HashtableElementCount; i++)
                {
                    incident_list[i] = new List<UInt32>();
                }

                const int OffsetAfterHashTable = HashtableElementCount * 8 + 4;
                int offset = OffsetAfterHashTable; // 1024* 8 + 4 bytes for the pointer to the start of the hash-table. 

                #endregion

                // Generate gp_headers list
                #region generate grind_path_headers

                List<grind_path_header> gp_headers_list = new List<grind_path_header>();
                int gp_headers_count = 0;
                int gp_headers_total_size = 0;

                // loop through groups
                for (int i = 0; i < gp_groups.Count; i++)
                {
                    // loop through list of grind_path_data
                    for (int g = 0; g < gp_groups[i].Count; g++)
                    {
                        // get grind path data
                        grind_path_data gp_data = gp_groups[i][g];

                        // instance grind_path_header
                        grind_path_header gp_header = new grind_path_header();
                        gp_header.grind_points_count = gp_data.positions.Count;
                        gp_header.flag_A = gp_data.flag_A;
                        gp_header.flag_B = gp_data.flag_B;

                        bounds gp_bbox = new bounds();

                        // calculate gp_header's bounding box
                        // loop through grind path/curve's point positions
                        for (int p = 0; p < gp_data.positions.Count; p++)
                        {
                            // pass point position to bounds{} object instance to add/sub the min/max of the bounding box
                            // in other words, we calculate the bounding box by going through every point position of the curve
                            // and retain the closest(A) and farthest(B) positions of the curve's points
                            gp_bbox.add_point(gp_data.positions[p]);
                        }

                        // set bounding box (bbBox_A is lower, bbBox_B is higher)
                        gp_header.bbBox_A = gp_bbox.bounding_box.A;
                        gp_header.bbBox_B = gp_bbox.bounding_box.B;


                        int min_x = (int)Math.Floor(gp_header.bbBox_A.X) >> 7;
                        int min_z = (int)Math.Floor(gp_header.bbBox_A.Z) >> 7;
                        int max_x = (int)Math.Ceiling(gp_header.bbBox_B.X) >> 7;
                        int max_z = (int)Math.Ceiling(gp_header.bbBox_B.Z) >> 7;
                        for (int x = min_x; x <= max_x; x++)
                        {
                            for (int z = min_z; z <= max_z; z++)
                            {
                                int cell_32z = 32 * (z & 0x1F);
                                int hashed_cell_index = cell_32z + (x & 0x1F);
                                incident_list[hashed_cell_index].Add((UInt32)gp_headers_count);
                                incident_list_occupancy++;
                            }
                        }

                        // add gp_header to gp_headers_list
                        gp_headers_list.Add(gp_header);
                        gp_headers_count++;
                    }
                }

                // 6 - get gp_headers_list total size
                gp_headers_total_size = gp_headers_count * 36;
                offset += gp_headers_total_size;
                offset += incident_list_occupancy * 4; // 4-bytes per item.

                #endregion

                #region setup gp_point position + normal array

                List<gp_point> pos_norm_list = new List<gp_point>();

                // 7 - Generate list of gp points: position + normal
                // based on the points count from gp_groups[i][g].positions
                int gp_index = 0;
                // loop through groups
                for (int i = 0; i < gp_groups.Count; i++)
                {
                    // loop through list of grind_path_data
                    for (int g = 0; g < gp_groups[i].Count; g++)
                    {
                        // for each point pos/norm
                        for (int p = 0; p < gp_groups[i][g].positions.Count; p++)
                        {
                            // add pos/norm to list
                            pos_norm_list.Add(new gp_point(gp_groups[i][g].positions[p], gp_groups[i][g].normals[p]));
                        }

                        if (gp_index >= gp_headers_list.Count)
                        {
                            break;
                        }
                        // 8 - update gp_header with offset to pos_norm list
                        gp_headers_list[gp_index].grind_points_list_start_offset = offset;

                        gp_index++;

                        offset += 24 * gp_groups[i][g].positions.Count;
                    }
                }

                #endregion

                #region serialize

                // list of byte array where we add blocks of data to seralize
                List<byte[]> buffer = new List<byte[]>();

                // block_01 starts with an Int32 that = 4, which is the offset to the hashtable following immediately after. 
                buffer.Add(BitConverter.GetBytes((Int32)4));

                // serialize items
                int cumulative_offset = 0;
                for (int i = 0; i < HashtableElementCount; i++)
                {
                    buffer.Add(new item_header(OffsetAfterHashTable + cumulative_offset, incident_list[i].Count).Serialize());
                    cumulative_offset += incident_list[i].Count * 4;
                }
                int offsetOfHeaderList = OffsetAfterHashTable + cumulative_offset;

                // serialize gp_headers_offsets_list
                for (int i = 0; i < HashtableElementCount; i++)
                {
                    foreach (UInt32 id in incident_list[i])
                    {
                        // IMPORTANT: Do not simplify. Ensure GetBytes(UInt32) variant is used here
                        UInt32 header_offset = (UInt32)id * 36 + (UInt32)offsetOfHeaderList;
                        buffer.Add(BitConverter.GetBytes(header_offset));
                    }
                }

                // serialize gp_headers_list
                for (int i = 0; i < gp_headers_list.Count; i++)
                {
                    buffer.Add(gp_headers_list[i].Serialize());
                }

                // serialize pos_norm_list
                for (int i = 0; i < pos_norm_list.Count; i++)
                {
                    buffer.Add(pos_norm_list[i].serialize());
                }

                #endregion

                // DEBUG :: output generated grind paths
                //File.WriteAllBytes(@"C:\Users\Mike\Desktop\JSRF\research\Stg_Grind_Paths\stg00_block_01_compile_test.bin", buffer.SelectMany(a => a).ToArray());


                return buffer.SelectMany(byteArr => byteArr).ToArray();
            }


            // curve point object containing position and normal
            public class gp_point
            {
                public Vector3 position { get; set; } // point position
                public Vector3 normal { get; set; } // point orientation

                public gp_point(Vector3 _pos, Vector3 _norm)
                {
                    this.position = _pos;
                    this.normal = _norm;
                }


                // serialize grind_paths_list's position/normal
                public byte[] serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(BitConverter.GetBytes(position.X)); b.Add(BitConverter.GetBytes(position.Y)); b.Add(BitConverter.GetBytes(position.Z));
                    b.Add(BitConverter.GetBytes(normal.X)); b.Add(BitConverter.GetBytes(normal.Y)); b.Add(BitConverter.GetBytes(normal.Z));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }
            }

            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();

                b.Add(BitConverter.GetBytes(unk_id));

                // serialize list of start_offset + items_count (8196 bytes)
                for (int i = 0; i < items.Count; i++)
                {
                    b.Add(BitConverter.GetBytes(items[i].start_offset));
                    b.Add(BitConverter.GetBytes(items[i].items_count));
                }

                return b.SelectMany(byteArr => byteArr).ToArray();
            }

            /// <summary>
            /// item_header is a list of [ [item count] and [start offset] ]
            /// each item_header
            /// </summary>
            public class item_header // ( 8 bytes)
            {
                public Int32 start_offset { get; set; }
                public Int32 items_count { get; set; }


                public item_header(Int32 _start_offset, Int32 _items_count) //  (8 bytes)
                {
                    this.start_offset = _start_offset;
                    this.items_count = _items_count;
                }

                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(BitConverter.GetBytes(start_offset));
                    b.Add(BitConverter.GetBytes(items_count));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }


                public class grind_path_header // (36 bytes)
                {
                    public Int32 grind_points_list_start_offset { get; set; }
                    public Int32 grind_points_count { get; set; }
                    public Int16 flag_A { get; set; }
                    public Int16 flag_B { get; set; }
                    public Vector3 bbBox_A { get; set; } // bounding box point A
                    public Vector3 bbBox_B { get; set; } // bounding box point B


                    public byte[] Serialize()
                    {
                        List<byte[]> b = new List<byte[]>();

                        b.Add(BitConverter.GetBytes(grind_points_list_start_offset));
                        b.Add(BitConverter.GetBytes(grind_points_count));
                        b.Add(BitConverter.GetBytes(flag_A));
                        b.Add(BitConverter.GetBytes(flag_B));
                        b.Add(BitConverter.GetBytes(bbBox_A.X)); b.Add(BitConverter.GetBytes(bbBox_A.Y)); b.Add(BitConverter.GetBytes(bbBox_A.Z)); // v1
                        b.Add(BitConverter.GetBytes(bbBox_B.X)); b.Add(BitConverter.GetBytes(bbBox_B.Y)); b.Add(BitConverter.GetBytes(bbBox_B.Z));

                        return b.SelectMany(byteArr => byteArr).ToArray();
                    }

                    public class grind_path_point
                    {
                        public Vector3 position { get; set; } // point position
                        public Vector3 normal { get; set; } // point orientation

                        public grind_path_point(Vector3 _pos, Vector3 _norm)
                        {
                            this.position = _pos;
                            this.normal = _norm;
                        }

                        public byte[] Serialize()
                        {
                            List<byte[]> b = new List<byte[]>();

                            b.Add(BitConverter.GetBytes(position.X)); b.Add(BitConverter.GetBytes(position.Y)); b.Add(BitConverter.GetBytes(position.Z)); // v1
                            b.Add(BitConverter.GetBytes(normal.X)); b.Add(BitConverter.GetBytes(normal.Y)); b.Add(BitConverter.GetBytes(normal.Z));

                            return b.SelectMany(byteArr => byteArr).ToArray();
                        }
                    }
                }
            }


            // we import the grind path/curve points/normals and parenting/grouping info
            // into this class
            public class grind_path_data
            {
                public int parent { get; set; }
                public int child { get; set; }
                public short flag_A { get; set; }
                public short flag_B { get; set; }

                public List<Vector3> positions { get; set; }
                public List<Vector3> normals { get; set; }

                public grind_path_data()
                {
                    positions = new List<Vector3>();
                    normals = new List<Vector3>();
                }
            }


            public class grind_path_header // (36 bytes)
            {
                public Int32 grind_points_list_start_offset { get; set; }
                public Int32 grind_points_count { get; set; }
                public Int16 flag_A { get; set; }
                public Int16 flag_B { get; set; }
                public Vector3 bbBox_A { get; set; } // bounding box point A
                public Vector3 bbBox_B { get; set; } // bounding box point B

                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(BitConverter.GetBytes(grind_points_list_start_offset));
                    b.Add(BitConverter.GetBytes(grind_points_count));
                    b.Add(BitConverter.GetBytes(flag_A));
                    b.Add(BitConverter.GetBytes(flag_B));

                    b.Add(BitConverter.GetBytes(bbBox_A.X)); b.Add(BitConverter.GetBytes(bbBox_A.Y)); b.Add(BitConverter.GetBytes(bbBox_A.Z));
                    b.Add(BitConverter.GetBytes(bbBox_B.X)); b.Add(BitConverter.GetBytes(bbBox_B.Y)); b.Add(BitConverter.GetBytes(bbBox_B.Z));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }
            }

            // imports text file containing grind paths/curves
            // into grind_paths_list
            private List<grind_path_data> import_grind_paths_data(string filepath)
            {
                List<grind_path_data> gp_list = new List<grind_path_data>();

                // if file doesn't exist, return empty gp_list
                if (!File.Exists(filepath))
                {
                    System.Windows.MessageBox.Show("Grind paths importer: " + Path.GetFileName(filepath) + " file does not exist.");
                    return gp_list;
                }

                #region import if it's a .obj

                if (filepath.ToLower().Contains(".obj"))
                {
                    List<String> linesn = new List<string>();
                    string linen;

                    System.IO.StreamReader filen = new System.IO.StreamReader(filepath);

                    while ((linen = filen.ReadLine()) != null)
                    {
                        // add line to list, while removing double spaces and trim start/end
                        linesn.Add(Regex.Replace(linen, @"\s+", " ").Trim());
                    }


                    // for every line of the .obj
                    for (int i = 0; i < linesn.Count; i++)
                    {
                        string l = linesn[i].ToLower();
                        if (string.IsNullOrEmpty(l)) { continue; }

                        // object
                        if (l.StartsWith("o "))
                        {
                            // create grind_path_data instance and fill in the imported data
                            grind_path_data gp_data = new grind_path_data();

                            string name = l.Split(' ')[1];
                            // grind path flags
                            if (name.Contains(':'))
                            {
                                if(name.Split(':').Length == 3)
                                {
                                    string[] args = name.Split(':');

                                    gp_data.flag_A = short.Parse(args[1]);
                                    gp_data.flag_B = short.Parse(args[2]);
                                }
                            }
                            

                            int i_shift = 0;
                            // loop through lines to get all the curve points
                            for (int j = i + 1; j < linesn.Count; j++)
                            {
                                string v = linesn[j].ToLower();
                                if (!v.StartsWith("v ")) { break; }
                                if (string.IsNullOrEmpty(v)) { continue; }

                                // get curve point position and normal
                                string[] data = v.Split(' ');
                                gp_data.positions.Add(new Vector3(data[1], data[2], data[3]));
                                gp_data.normals.Add(new Vector3(0f, 1f, 0f)); //new Vector3(data[4], data[5], data[6]

                                i_shift++;
                            }
                            i += i_shift;
                            gp_list.Add(gp_data);
                        }
                    }
                }

                #endregion


                #region .txt grind paths import

                if (!filepath.ToLower().Contains(".txt"))
                {
                    return gp_list;
                }

                List<String> lines = new List<string>();
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(filepath);

                try
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        // add line to list, while removing double spaces and trim start/end
                        lines.Add(Regex.Replace(line, @"\s+", " ").Trim());
                    }

                    file.Close();
                    file.Dispose();
                } 
                catch
                {
                    file.Close();
                    file.Dispose();
                    System.Windows.Forms.MessageBox.Show("Error: could not read Grind Paths file.");
                }



                // for every line of the .obj
                for (int i = 0; i < lines.Count; i++)
                {
                    string l = lines[i].ToLower();
                    if (string.IsNullOrEmpty(l)) { continue; }

                    if (l.StartsWith("["))
                    {
                        string[] args = l.Split('[')[1].Split(']')[0].Split(':');

                        // create grind_path_data instance and fill in the imported data
                        grind_path_data gp_data = new grind_path_data();
                        gp_data.parent = int.Parse(args[0]);
                        gp_data.child = int.Parse(args[1]);
                        gp_data.flag_A = short.Parse(args[2].Split(' ')[0]);
                        gp_data.flag_B = short.Parse(args[2].Split(' ')[1]);

                        int i_shift = 0;
                        // loop through lines to get all the curve points
                        for (int j = i + 1; j < lines.Count; j++)
                        {
                            string v = lines[j].ToLower();
                            if (string.IsNullOrEmpty(v)) { continue; }
                            // exit loop if we reach the "end" line
                            if (v == "end")
                            {
                                break;
                            }

                            // get curve point position and normal-
                            string[] data = v.Split(' ');
                            gp_data.positions.Add(new Vector3(data[0], data[1], data[2]));
                            gp_data.normals.Add(new Vector3(data[3], data[4], data[5]));

                            i_shift++;
                        }
                        i += i_shift;
                        gp_list.Add(gp_data);
                    }
                }

                return gp_list;
            }
            #endregion

           
        }

        #endregion

        // spawns (decals, instanced level models, stage MDLBs) and PVS(potential_visibility_set)  optimization blocks
        #region main_block_02

        /// <summary>
        /// List of various decals spawns lists also Stage model instance, and possibly 
        /// </summary>
        /// <remarks>
        /// Model IDs correspond to models in files "StgObj" or MDLB contained within Stage_XX.dat files.
        ///
        /// at offset 4908, there seems to be more data, maybe Stage model parts culling?
        /// </remarks>
        public class block_02
        {
            // TODO properly build PVS (potential_visibility_set)
            // for now we create a single gigantic 30k bound PVS 
            // so right now a new stage will not be optimize and all visual models
            // will be rendering constantly
            public byte[] build(int vis_models_count)
            {
                // list of byte array where we add blocks of data to seralize
                List<byte[]> buffer = new List<byte[]>();

                int PVS_count = 1;

                #region main header

                header head = new header();

                head.potential_visibility_sets_count = PVS_count;
                head.potential_visibility_sets_starto = 108 + (vis_models_count * 80);

                // no spawns, so we set all the start offsets to the end of the main header (108)
                head.block_01_starto = 108;
                head.block_02_starto = 108 + (vis_models_count * 80);
                head.block_03_starto = 108 + (vis_models_count * 80);
                head.block_04_starto = 108 + (vis_models_count * 80);
                head.block_05_starto = 108 + (vis_models_count * 80);
                head.block_06_starto = 108 + (vis_models_count * 80);
                head.block_07_starto = 108 + (vis_models_count * 80);
                head.block_08_starto = 108 + (vis_models_count * 80);
                head.block_09_starto = 108 + (vis_models_count * 80);
                head.block_10_starto = 108 + (vis_models_count * 80);
                head.block_11_starto = 108 + (vis_models_count * 80);
                head.block_12_starto = 108 + (vis_models_count * 80);

                head.block_01_count = vis_models_count;

                // based on the Garage stg00_.bin block_02, head size should be 108 bytes
                // serialize and add to buffer
                buffer.Add(head.Serialize());


                #endregion

                for (int i = 0; i < vis_models_count; i++)
                {
                    object_spawn obj_spawn = new object_spawn();
                    obj_spawn.num_c = 1;
                    obj_spawn.padding_3 = 1;
                    obj_spawn.resource_ID = i;

                    buffer.Add(obj_spawn.Serialize());
                }
                

                #region build Potential visibility sets

                // visiblity range of PVS (range at which Stage models will be displayed while the player is standing inside this PVS)
                float bound_range = 30000;

                for (int i = 0; i < PVS_count; i++)
                {
                    // calculating total block_02 size
                    // 108 + 340 + 144 + 4
                    // main_header.size + potential_visibility_set.size + pvs_bounds.size + pvs_links_list.size

                    // 340 bytes per pvs block
                    potential_visibility_set pvs = new potential_visibility_set();
                    pvs.pvs_bounds_offset = 108 + 340 + ((PVS_count-1) * 340);


                    pvs.pvs_links_offset = 108 + 340 + 144 + ((PVS_count - 1) * 340);

                    if (i == 0)
                    {
                        pvs.pvs_bounds_count = 1;
                        pvs.pvs_links_count = PVS_count;
                    }
                    else // set link to first PVS
                    {
                        pvs.pvs_bounds_count = 0;
                        pvs.pvs_links_count = 0;
                    }


                    int block_02_size = 108 + 340 + 144 + ((PVS_count - 1) * 340); //+ 4 

                    // set block_02 file size in PVS header
                    pvs.unk_16_block02_size = block_02_size;
                    pvs.unk_18_block02_size = block_02_size;
                    pvs.unk_20_block02_size = block_02_size;
                    pvs.unk_22_block02_size = block_02_size;


                    // TODO : calculare or ask the user to define the PVS bounding boxes ranges
                    // for now we create one big PVS so everything is rendering constantly
                    pvs.v00 = new Vector3(-bound_range, -bound_range, -bound_range);
                    pvs.v01 = new Vector3(bound_range, -bound_range, -bound_range);
                    pvs.v02 = new Vector3(bound_range, -bound_range, bound_range);
                    pvs.v03 = new Vector3(-bound_range, -bound_range, bound_range);

                    pvs.v04 = new Vector3(-bound_range, -bound_range, -bound_range);
                    pvs.v05 = new Vector3(-bound_range, bound_range, -bound_range);

                    pvs.v06 = new Vector3(0, -1, 0);
                    pvs.v07 = new Vector3(0, 0, -1);
                    pvs.v08 = new Vector3(1, 0, 0);
                    pvs.v09 = new Vector3(0, 0, 1);
                    pvs.v10 = new Vector3(-1, 0, 0);
                    pvs.v11 = new Vector3(0, 1, 0);

                    pvs.v12 = new Vector3(-0.4163f, -0.7199f, -0.6334f);
                    pvs.v13 = new Vector3(-0.4163f, -0.7199f, -0.6334f);

                    // default values (same values accross multiple files)
                    pvs.unk_2 = 2130706432;
                    pvs.unk_4 = 2130706432;
                    pvs.unk_6 = 16777002;
                    pvs.unk_7 = 2700141;

                    // serialize and add to buffer
                    buffer.Add(pvs.Serialize());
                }


                #region create PVS_bounds

                // 144 bytes per PVS_bounds
                PVS_bounds pvs_bbox = new PVS_bounds();
                // TODO get bounding box scales from user defined data (import PVS bboxes created in the 3D DCC)
                pvs_bbox.v00 = new Vector3(-bound_range, -bound_range, -bound_range);
                pvs_bbox.v01 = new Vector3(bound_range, -bound_range, -bound_range);
                pvs_bbox.v02 = new Vector3(bound_range, -bound_range, bound_range);
                pvs_bbox.v03 = new Vector3(-bound_range, -bound_range, bound_range);

                pvs_bbox.v04 = new Vector3(-bound_range, -bound_range, -bound_range);
                pvs_bbox.v05 = new Vector3(-bound_range, bound_range, -bound_range);

                pvs_bbox.vs00 = new Vector3(0, -1, 0);
                pvs_bbox.vs01 = new Vector3(0, 0, -1);
                pvs_bbox.vs02 = new Vector3(1, 0, 0);
                pvs_bbox.vs03 = new Vector3(0, 0, 1);
                pvs_bbox.vs04 = new Vector3(-1, 0, 0);
                pvs_bbox.vs05 = new Vector3(0, 1, 0);

                #endregion


                #endregion


                #region serialize and return bytes block


                buffer.Add(pvs_bbox.Serialize());

                // PVS links list, we make it just one since we only have one PVS

                for (int i = 0; i < PVS_count; i++)
                {
                    buffer.Add(BitConverter.GetBytes((Int32)i));
                }

                return buffer.SelectMany(byteArr => byteArr).ToArray();

                #endregion


            }

            public class header
            {
                // TODO : the number of spawn blocks seems to vary and there's more than 12 defined here, see stg10_.bin stg12_.bin

                public Int32 blocks_count { get; set; } = 12; // unsure of what this is, default is 12

                public Int32 potential_visibility_sets_starto { get; set; } = 108;//108 = end of spawns list (block_xx_starto bloc_xx_count // start offset of blocks
                public Int32 potential_visibility_sets_count { get; set; } = 1; // number of blocks

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


                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(BitConverter.GetBytes(blocks_count));

                    b.Add(BitConverter.GetBytes(potential_visibility_sets_starto));
                    b.Add(BitConverter.GetBytes(potential_visibility_sets_count));

                    b.Add(BitConverter.GetBytes(block_01_starto));
                    b.Add(BitConverter.GetBytes(block_01_count));

                    b.Add(BitConverter.GetBytes(block_02_starto));
                    b.Add(BitConverter.GetBytes(block_02_count));

                    b.Add(BitConverter.GetBytes(block_03_starto));
                    b.Add(BitConverter.GetBytes(block_03_count));

                    b.Add(BitConverter.GetBytes(block_04_starto));
                    b.Add(BitConverter.GetBytes(block_04_count));

                    b.Add(BitConverter.GetBytes(block_05_starto));
                    b.Add(BitConverter.GetBytes(block_05_count));

                    b.Add(BitConverter.GetBytes(block_06_starto));
                    b.Add(BitConverter.GetBytes(block_06_count));

                    b.Add(BitConverter.GetBytes(block_07_starto));
                    b.Add(BitConverter.GetBytes(block_07_count));

                    b.Add(BitConverter.GetBytes(block_08_starto));
                    b.Add(BitConverter.GetBytes(block_08_count));

                    b.Add(BitConverter.GetBytes(block_09_starto));
                    b.Add(BitConverter.GetBytes(block_09_count));

                    b.Add(BitConverter.GetBytes(block_10_starto));
                    b.Add(BitConverter.GetBytes(block_10_count));

                    b.Add(BitConverter.GetBytes(block_11_starto));
                    b.Add(BitConverter.GetBytes(block_11_count));

                    b.Add(BitConverter.GetBytes(block_12_starto));
                    b.Add(BitConverter.GetBytes(block_12_count));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }
            }


            // PVS: bounding boxes + list of IDs to other PVS, to define which level's visual meshes parts are rendering while standing inside a PVS
            // potential_visibility_sets, a sort of bounding box with 4 points plane + 2 height value
            // manages which stage model and props are actively being rendered or not drawing
            // potential_visibility_set has a list (pvs_links) which is a list of IDs defining other potential_visibility_set(s)
            // if a PVS is in the pvs_links list, then that PVS region will be rendering while we're inside, this, current PVS
            public class potential_visibility_set // 340 bytes
            {
                // bounding box, 4 Vector3 form a 4 sided plane, v04_height gives the box height
                public Vector3 v00 { get; set; } // plane point
                public Vector3 v01 { get; set; } // plane point
                public Vector3 v02 { get; set; } // plane point
                public Vector3 v03 { get; set; } // plane point
                public Vector3 v04 { get; set; } // plane point
                public Vector3 v05 { get; set; } // (point aligned with v04) but v05.Y gives the height of the bbox

                // not sure if floats if so, rotations or matrix, else might be int16 or flags
                public Vector3 v06 { get; set; }
                public Vector3 v07 { get; set; }
                public Vector3 v08 { get; set; }
                public Vector3 v09 { get; set; }
                public Vector3 v10 { get; set; }
                public Vector3 v11 { get; set; }


                public Int32 pvs_bounds_offset { get; set; } // points to start offset for blocks of type "pvs_bounds", which seem to contain the same bounding box / matrix data contained in this (potential_visibility_set.v00 v01 etc) 
                public Int32 pvs_bounds_count { get; set; }  /// number of pvs blocks
                public Int32 pvs_links_offset { get; set; } //  points to end block that is a list of integers (probably pvs block numbers, or stage model number)
                public Int32 pvs_links_count { get; set; }  // number of "links" aka list of PVS numbers/ids of pvs that will be drawn while we're inside this pvs/bounding box (stage models, MDLB, decals etc that are inside a pvs/bounding box)


                public Vector3 v12 { get; set; }
                public Int32 unk_2 { get; set; }
                public Int32 unk_3 { get; set; }
                public Int32 unk_4 { get; set; }
                public Int32 unk_5 { get; set; }
                public Vector3 v13 { get; set; }
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

                public Int32 unk_16_block02_size { get; set; } // block 02 total size?
                public Int32 unk_17 { get; set; }

                public Int32 unk_18_block02_size { get; set; } // block 02 total size?
                public Int32 unk_19 { get; set; }

                public Int32 unk_20_block02_size { get; set; } // block 02 total size?
                public Int32 unk_21 { get; set; }

                public Int32 unk_22_block02_size { get; set; } // block 02 total size?
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


                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(v00.Serialize());
                    b.Add(v01.Serialize());
                    b.Add(v02.Serialize());
                    b.Add(v03.Serialize());
                    b.Add(v04.Serialize());
                    b.Add(v05.Serialize());

                    b.Add(v06.Serialize());
                    b.Add(v07.Serialize());
                    b.Add(v08.Serialize());
                    b.Add(v09.Serialize());
                    b.Add(v10.Serialize());
                    b.Add(v11.Serialize());


                    b.Add(BitConverter.GetBytes(pvs_bounds_offset));
                    b.Add(BitConverter.GetBytes(pvs_bounds_count));  
                    b.Add(BitConverter.GetBytes(pvs_links_offset));
                    b.Add(BitConverter.GetBytes(pvs_links_count));


                    b.Add(v12.Serialize());
                    b.Add(BitConverter.GetBytes(unk_2));
                    b.Add(BitConverter.GetBytes(unk_3));
                    b.Add(BitConverter.GetBytes(unk_4));
                    b.Add(BitConverter.GetBytes(unk_5));
                    b.Add(v13.Serialize());
                    b.Add(BitConverter.GetBytes(unk_6));
                    b.Add(BitConverter.GetBytes(unk_7));

                    b.Add(BitConverter.GetBytes(unk_8_padding));
                    b.Add(BitConverter.GetBytes(unk_9_padding));
                    b.Add(BitConverter.GetBytes(unk_10_padding));
                    b.Add(BitConverter.GetBytes(unk_11_padding));

                    b.Add(BitConverter.GetBytes(unk_12_padding));
                    b.Add(BitConverter.GetBytes(unk_13_padding));
                    b.Add(BitConverter.GetBytes(unk_14_padding));
                    b.Add(BitConverter.GetBytes(unk_15_padding));

                    b.Add(BitConverter.GetBytes(unk_16_block02_size));
                    b.Add(BitConverter.GetBytes(unk_17));

                    b.Add(BitConverter.GetBytes(unk_18_block02_size));
                    b.Add(BitConverter.GetBytes(unk_19));


                    b.Add(BitConverter.GetBytes(unk_20_block02_size));
                    b.Add(BitConverter.GetBytes(unk_21));

                    b.Add(BitConverter.GetBytes(unk_22_block02_size));
                    b.Add(BitConverter.GetBytes(unk_23));

                    b.Add(BitConverter.GetBytes(unk_24_padding));
                    b.Add(BitConverter.GetBytes(unk_25_padding));
                    b.Add(BitConverter.GetBytes(unk_26_padding));
                    b.Add(BitConverter.GetBytes(unk_27_padding));
                    b.Add(BitConverter.GetBytes(unk_28_padding));
                    b.Add(BitConverter.GetBytes(unk_29_padding));
                    b.Add(BitConverter.GetBytes(unk_31_padding));
                    b.Add(BitConverter.GetBytes(unk_32_padding));
                    b.Add(BitConverter.GetBytes(unk_33_padding));
                    b.Add(BitConverter.GetBytes(unk_34_padding));
                    b.Add(BitConverter.GetBytes(unk_35_padding));
                    b.Add(BitConverter.GetBytes(unk_36_padding));
                    b.Add(BitConverter.GetBytes(unk_37_padding));
                    b.Add(BitConverter.GetBytes(unk_38_padding));
                    b.Add(BitConverter.GetBytes(unk_39_padding));
                    b.Add(BitConverter.GetBytes(unk_40_padding));
                    b.Add(BitConverter.GetBytes(unk_41_padding));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }

            }


            // same list of Vector3 that define a 4 point plane + height point
            // these values doesn't seem to effect the "potential_visibility_set"
            public class PVS_bounds // 144 bytes
            {
                public Vector3 v00 { get; set; }
                public Vector3 v01 { get; set; }

                public Vector3 v02 { get; set; }
                public Vector3 v03 { get; set; }

                public Vector3 v04 { get; set; }
                public Vector3 v05 { get; set; }

                public Vector3 vs00 { get; set; }
                public Vector3 vs01 { get; set; }

                public Vector3 vs02 { get; set; }
                public Vector3 vs03 { get; set; }

                public Vector3 vs04 { get; set; }
                public Vector3 vs05 { get; set; }

                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(v00.Serialize());
                    b.Add(v01.Serialize());

                    b.Add(v02.Serialize());
                    b.Add(v03.Serialize());

                    b.Add(v04.Serialize());
                    b.Add(v05.Serialize());

                    b.Add(vs00.Serialize());
                    b.Add(vs01.Serialize());

                    b.Add(vs02.Serialize());
                    b.Add(vs03.Serialize());

                    b.Add(vs04.Serialize());
                    b.Add(vs05.Serialize());

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }
            }

            // potential_visibility_sets, a sort of bounding box with 4 points plane + 2 height value
            // manages which stage model and props are actively being rendered or not drawing
            // potential_visibility_set has a list (pvs_links) which is a list of IDs defining other potential_visibility_set(s)
            // if a PVS is in the pvs_links list, then that PVS region will be rendering while we're inside, this, current PVS
            public List<potential_visibility_set> potential_visibility_sets_list { get; set; }

            public List<object_spawn> block_01_MDLB_list { get; set; }
            public List<object_spawn> block_02_decals_list { get; set; }
            public List<object_spawn> block_03_decals_list { get; set; }
            public List<object_spawn> block_04_decals_list { get; set; }
            public List<object_spawn> block_05_props_list { get; set; }
            public List<object_spawn> block_06_decals_list { get; set; }
            public List<object_spawn> block_07_props_list { get; set; }
            public List<object_spawn> block_08_MDLB_list { get; set; } // more props, small fences under ufo in Garage stage
            public List<object_spawn> block_09_prop_list { get; set; } // basket ball props  in Garage stage
            public List<object_spawn> block_10_list { get; set; }
            public List<object_spawn> block_11_decals_list { get; set; }
            public List<object_spawn> block_12_decals_list { get; set; } // seems like larger decals



            /// <summary>
            /// generic class of object (model) spawn
            /// </summary>
            /// <remarks>
            /// Rotation matrix is defined by multiple vector3
            /// </remarks>
            public class object_spawn // 80 bytes
            {
                public Vector3 v0 { get; set; }    
                public float padding_0 { get; set; }

                public Vector3 v1 { get; set; }
                public float padding_1 { get; set; }

                public Vector3 v2 { get; set; }
                public float padding_2 { get; set; }

                public Vector3 v3 { get; set; }
                public float padding_3 { get; set; }

                public Int32 resource_ID { get; set; }
                public Int32 num_b { get; set; }
                public Int32 num_c { get; set; }
                public Int32 num_d { get; set; }

                public object_spawn()
                {
                    v0 = new Vector3(1, 0, 0);
                    v1 = new Vector3(0, 1, 0);
                    v2 = new Vector3(0, 0, 1);
                    v3 = new Vector3(0, 0, 0);
                }

                public byte[] Serialize()
                {
                    List<byte[]> b = new List<byte[]>();

                    b.Add(BitConverter.GetBytes(v0.X)); b.Add(BitConverter.GetBytes(v0.Y)); b.Add(BitConverter.GetBytes(v0.Z));
                    b.Add(BitConverter.GetBytes(padding_0));

                    b.Add(BitConverter.GetBytes(v1.X)); b.Add(BitConverter.GetBytes(v1.Y)); b.Add(BitConverter.GetBytes(v1.Z));
                    b.Add(BitConverter.GetBytes(padding_1));


                    b.Add(BitConverter.GetBytes(v2.X)); b.Add(BitConverter.GetBytes(v2.Y)); b.Add(BitConverter.GetBytes(v2.Z));
                    b.Add(BitConverter.GetBytes(padding_2));


                    b.Add(BitConverter.GetBytes(v3.X)); b.Add(BitConverter.GetBytes(v3.Y)); b.Add(BitConverter.GetBytes(v3.Z));
                    b.Add(BitConverter.GetBytes(padding_3));

                    b.Add(BitConverter.GetBytes(resource_ID));
                    b.Add(BitConverter.GetBytes(num_b));
                    b.Add(BitConverter.GetBytes(num_c));
                    b.Add(BitConverter.GetBytes(num_d));

                    return b.SelectMany(byteArr => byteArr).ToArray();
                }
            }
        }

        #endregion
    }

}
