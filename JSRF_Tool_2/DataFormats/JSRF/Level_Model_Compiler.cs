using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSRF_ModTool.Vector;
using JSRF_ModTool.DataFormats._3D_Model_Formats;
using System.IO;

namespace JSRF_ModTool.DataFormats.JSRF
{
    class Level_Model_Compiler
    {
        public List<texture_info> textures { get; set; }

        public Level_Model_Compiler()
        {
            textures = new List<texture_info>();
        }

        private bool debug_draw_distance = true;

        public class texture_info
        {
            public string texture_filepath;
            public Int32 texture_id;

            public texture_info(string _filepath, Int32 _tex_id)
            {
                this.texture_filepath = _filepath;
                this.texture_id = _tex_id;
            }
        }

        public byte[] build(string obj_filepath)
        {
            // list of arrays of the data to compile for the file
            List<byte[]> file_buffers_list = new List<byte[]>();

            // load .obj file into OBJ instance
            OBJ obj = new OBJ(obj_filepath);
           // if obj has not meshes, return
            if (obj.meshes.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: OBJ mesh could not be loaded.");
                return new byte[0];
            }

            // check if there is more than one group with the same material
            // if so reject import
            List<String> mat_names = new List<string>();
            for (int i = 0; i < obj.meshes[0].material_groups.Count; i++)
            {
                if(mat_names.Contains(obj.meshes[0].material_groups[i].mat_name))
                {
                    System.Windows.Forms.MessageBox.Show("Error: OBJ mesh " + Path.GetFileName(obj.FilePath) + " has different face groups using the same material.\n\nPlease merge the different face groups using the same material into a single one.");
                    return new byte[0];
                }

                mat_names.Add(obj.meshes[0].material_groups[i].mat_name);
            }

            // flip UV map on Y axis
            for (int i = 0; i < obj.meshes[0].uv_buffer.Count; i++)
            {
                obj.meshes[0].uv_buffer[i].Y = ((obj.meshes[0].uv_buffer[i].Y) * -1f) + 1; //
            }


            #region process material groups and materials from the .mtl

            List<Int32> texture_IDs_list = new List<int>();
            
            OBJ.obj_mesh mesh = obj.meshes[0];
            

            // get texture paths and generate ids for them
            for (int i = 0; i < mesh.material_groups.Count; i++)
            {
                // for each material form the mtl file
                for (int e = 0; e < obj.mtl_materials_list.Count; e++)
                {
                    // if material group mat matches material name in mtl
                    if(mesh.material_groups[i].mat_name.ToLower() == obj.mtl_materials_list[e].material_name.ToLower())
                    {
                        // sleep for a bit, otherwise MathI.RandomDigits() keeps ouputting the same number in a short timeframe
                        System.Threading.Thread.Sleep(50);
                        // generate unique ID which will be what is used by the game file format to index textures
                        //int random_num = Functions.MathI.RandomDigits(9);
                        //int textutre_id = Int32.Parse(Path.GetFileNameWithoutExtension(obj.mtl_materials_list[e].texture_path));

                        Int32 tex_id = -1;

                        // check if texture path already exists in textures list
                        foreach (var tx in textures)
                        {
                            if(tx.texture_filepath == obj.mtl_materials_list[e].texture_path)
                            {
                                tex_id = tx.texture_id;
                                break;
                            }
                        }

                        bool valid_tex_path = true;
                        // if texture wasn't found generate id and add to textures list
                        if(tex_id == -1)
                        {
                            tex_id = Functions.MathI.RandomDigits(9);

                            if (File.Exists(obj.mtl_materials_list[e].texture_path))
                            {
                                valid_tex_path = true;
                                textures.Add(new texture_info(obj.mtl_materials_list[e].texture_path, tex_id));
                            } else {
                                valid_tex_path = false;
                               // System.Windows.Forms.MessageBox.Show("Material \"" + obj.mtl_materials_list[e].material_name + "\" texture path is invalid.\nThis material and texture will not be added to the compile.");
                                obj.mtl_materials_list.RemoveAt(e);
                                
                            }  
                        }

                        if(valid_tex_path)
                        {
                            // generate texture ID and store it
                            obj.mtl_materials_list[e].ID = tex_id;
                            mesh.material_groups[i].ID = tex_id;

                            // add ID to texture id list
                            texture_IDs_list.Add(tex_id);

                            break;
                        }
                    }
                }
            }

            List<string> mat_names_list = new List<string>();

            #region merge triangle groups using the same material

            #endregion

            // add count of texture ids to buffer
            file_buffers_list.Add(BitConverter.GetBytes((Int32)texture_IDs_list.Count));
            // add list of texture ids
            file_buffers_list.Add(texture_IDs_list.SelectMany(BitConverter.GetBytes).ToArray());


            #endregion

            #region build main header

            Header head = new Header();
            head.x124_mat_count = 1;
            head.x128_unk = 180;
            head.x132_tri_groups_count = mesh.material_groups.Count;

            #region calculate mesh center and bounding box

            // min max to calculate the mesh point's bounding box
            float Xmin = 0, Xmax = 0, Ymin = 0, Ymax = 0, Zmin = 0, Zmax = 0;
            Vector3 ctr = new Vector3();

            List<Vector3> points_list = new List<Vector3>();

            // loop through the mat's triangles (from mg.start_index; to  mg.start_index + mg.size)
            // to get all vertices positions to calculate min/max bouding box and center of the material group mesh
            for (int o = 0 ; o < mesh.vertex_buffer.Count; o++)
            {
                Vector3 vert = mesh.vertex_buffer[o];

                // ignore duplicate verts
                if (points_list.Contains(vert))
                {
                    continue;
                }

                points_list.Add(vert);

                // if vertex value decreases or increases update min/max value
                Xmin = vert.X < Xmin ? Xmin = vert.X : Xmin;
                Ymin = vert.Y < Ymin ? Ymin = vert.Y : Ymin;
                Zmin = vert.Z < Zmin ? Zmin = vert.Z : Zmin;
                Xmax = vert.X > Xmax ? Xmax = vert.X : Xmax;
                Ymax = vert.Y > Ymax ? Ymax = vert.Y : Ymax;
                Zmax = vert.Z > Zmax ? Zmax = vert.Z : Zmax;

                // if min/max is equal zero
                Xmin = Xmin == 0 ? Xmin = vert.X : Xmin;
                Ymin = Ymin == 0 ? Ymin = vert.Y : Ymin;
                Zmin = Zmin == 0 ? Zmin = vert.Z : Zmin;
                Xmax = Xmax == 0 ? Xmax = vert.X : Xmax;
                Ymax = Ymax == 0 ? Ymax = vert.Y : Ymax;
                Zmax = Zmax == 0 ? Zmax = vert.Z : Zmax;

                ctr = new Vector3(ctr.X + vert.X, ctr.Y + vert.Y, ctr.Z + vert.Z);
            }


            // divide ctr floats by number of vertices
            ctr = new Vector3(ctr.X / points_list.Count, ctr.Y / points_list.Count, ctr.Z / points_list.Count);
            points_list.Clear();

            head.model_center = ctr;
            head.model_radius = (Math.Abs((Math.Abs(Xmax) - Math.Abs(Xmin))) + Math.Abs((Math.Abs(Ymax) - Math.Abs(Ymin))) + Math.Abs((Math.Abs(Zmax) - Math.Abs(Zmin)))) * (85f / 100f);

            #if DEBUG

            if (debug_draw_distance)
            {
                head.model_radius = 4000f;
            }

            #endif
            #endregion

            head.x080_unk_Vector3 = new Vector3(1, 1, 1);
            head.x004_unk = 1;
            head.mat_group_start_offset = 128 + (head.x124_mat_count * 20);
            head.x020_unk = 128;

            head.x136_unk_ID = 1243916; // TODO unknown id
            head.x140_unk_ID = 2013322001; // TODO unknown id

            file_buffers_list.Add(head.Serialize());

            #endregion

            #region materials list
             // for new we only do one, some models can have two more?
            material mat = new material();
            mat.color = new MDLB.color(255, 255, 255, 255);
            mat.shader_id = 0;
            mat.unk_id2 = 16777215;
            mat.HB = 50;

            file_buffers_list.Add(mat.Serialize());

            #endregion

            // start position aftet the texture ids array
            int startoff_no_texIDs = (Functions.Parsing.calc_length_bytes_list(file_buffers_list) - (texture_IDs_list.Count * 4 + 4));

            #region vertex triangles buffers header

            vertex_triangles_buffers_header vtxt_head = new vertex_triangles_buffers_header();
            // 16 shift + length of header data (without texture_ids list) + this 32 bytes (vtxt_head) length + material groups length + material group bounding box
            vtxt_head.vertex_buffer_startoffset = 16 + startoff_no_texIDs + (mesh.material_groups.Count * 32) + (mesh.material_groups.Count * 16) ; // bytes count from main header to end of bbox list
            vtxt_head.vertex_count = mesh.vertex_buffer.Count;
            vtxt_head.vertex_struct = 514; // 514 = 28 flag //  vtx def 28 = 512 flag //   vtx def 32 = 274 flag   // //  vtx def 24 = 322 flag //  vtx def 16 = 66 flag
            vtxt_head.vertex_def_size = 28;
            vtxt_head.triangle_buffer_startoffset = vtxt_head.vertex_buffer_startoffset + (vtxt_head.vertex_count * vtxt_head.vertex_def_size); // todo
            vtxt_head.triangle_buffer_size = mesh.face_indices.Count; //  if= 36 then real byte size is *21 = 72  // offset from texture counts it(start of this model)  +128

            // set vertex flag that varies depending on the vertex_def_size
            switch (vtxt_head.vertex_def_size)
            {
                case 32:
                    vtxt_head.vertex_struct = 274;
                    break;

                case 28:
                    vtxt_head.vertex_struct = 514;
                    break;

                case 24:
                    vtxt_head.vertex_struct = 322;
                    break;

                case 16:
                    vtxt_head.vertex_struct = 66;
                    break;
            }

            file_buffers_list.Add(vtxt_head.Serialize());

            #endregion

            #region material_groups

            int starto_mat_groups = Functions.Parsing.calc_length_bytes_list(file_buffers_list);
            //List<byte[]> mat_groups_list = new List<byte[]>();
            for (int i = 0; i < mesh.material_groups.Count; i++)
            {
                //material_group mat_group = new material_group();
                OBJ.obj_mesh.material_group mg = mesh.material_groups[i];
                material_group mat_group  = new material_group();
                mat_group.triangle_count = mg.size;
                mat_group.triangle_start_index = mg.start_index * 3;
                mat_group.unk_08 = 0; 
                mat_group.material_ID = i;

                mat_group.unk_16 = 0;
                mat_group.unk_20 = 0;
                mat_group.unk_24 = 0;
                mat_group.unk_28 = starto_mat_groups + (mesh.material_groups.Count * 32) + (i * 16) -32 -16; // Functions.Parsing.calc_length_bytes_list(file_buffers_list) + (mesh.material_groups.Count * 32) -32; // count bytes from start minus 16 // todo calculate // increases by +16 for each triangle group // first tri group start value seems the number of bytes from the start position to end of triangles group, minus -16

                file_buffers_list.Add(mat_group.Serialize());
            }

            #endregion

            #region setup material groups bounding boxes
            List<string> lines_bboxes = new List<string>();
            for (int i = 0; i < mesh.material_groups.Count; i++)
            {
                // get mesh's material group
                OBJ.obj_mesh.material_group mg = mesh.material_groups[i];

                // min max to calculate the mesh point's bounding box
                 Xmin = Xmax = Ymin = Ymax = Zmin =  Zmax = 0;
                 ctr = new Vector3();

                 points_list = new List<Vector3>();

                // loop through the mat's triangles (from mg.start_index; to  mg.start_index + mg.size)
                // to get all vertices positions to calculate min/max bouding box and center of the material group mesh
                for (int o = mg.start_index; o < (mg.start_index  + mg.size); o++)
                {
                    if (mesh.face_indices[o] >= mesh.vertex_buffer.Count) { break; }
                    Vector3 vert = mesh.vertex_buffer[mesh.face_indices[o]];

                    bool vert_already_indexed = false;
                    // for each point in list check if vertex already exists
                    for (int p = 0; p < points_list.Count; p++)
                    {
                            Vector3 ve = points_list[p];
                            if (ve.X == vert.X && ve.Y == vert.Y && ve.Z == vert.Z)
                            {
                                vert_already_indexed = true;
                                break;
                            }
                    }

                    // if vertex isn't exist indexed
                    if (!vert_already_indexed)
                    {
                        points_list.Add(vert);
                        vert_already_indexed = false;

                        // if vertex value decreases or increases update min/max value
                        Xmin = vert.X < Xmin ? Xmin = vert.X : Xmin;
                        Ymin = vert.Y < Ymin ? Ymin = vert.Y : Ymin;
                        Zmin = vert.Z < Zmin ? Zmin = vert.Z : Zmin;
                        Xmax = vert.X > Xmax ? Xmax = vert.X : Xmax;
                        Ymax = vert.Y > Ymax ? Ymax = vert.Y : Ymax;
                        Zmax = vert.Z > Zmax ? Zmax = vert.Z : Zmax;

                        // if min/max is equal zero
                        Xmin = Xmin == 0 ? Xmin = vert.X : Xmin;
                        Ymin = Ymin == 0 ? Ymin = vert.Y : Ymin;
                        Zmin = Zmin == 0 ? Zmin = vert.Z : Zmin;
                        Xmax = Xmax == 0 ? Xmax = vert.X : Xmax;
                        Ymax = Ymax == 0 ? Ymax = vert.Y : Ymax;
                        Zmax = Zmax == 0 ? Zmax = vert.Z : Zmax;

                        ctr = new Vector3(ctr.X + vert.X, ctr.Y + vert.Y, ctr.Z + vert.Z);
                    }
                }

                
                List<string> lines = new List<string>();

                for (int l = 0; l < points_list.Count; l++)
                {
                    lines.Add(points_list[l].X + " " + points_list[l].Y + " " + points_list[l].Z);
                }

                File.WriteAllLines(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\export\" + "matgroup_" + i + ".txt", lines);


                ctr = new Vector3(ctr.X / points_list.Count, ctr.Y / points_list.Count, ctr.Z / points_list.Count);
                // calculate distance between min and max points
                //bounds_legth = (float)Math.Sqrt(Math.Pow(Xmax - Math.Abs(Xmin), 2) + Math.Pow(Ymax - Math.Abs(Ymin), 2) + Math.Pow(Zmax - Math.Abs(Zmin), 2));
                //bounds_legth /= 10;

                // float radius = (float)Math.Sqrt(Math.Pow((Xmax - Xmin) + ctr.X, 2) + Math.Pow((Ymax - Ymin) + ctr.Y, 2) + Math.Pow((Zmax - Zmin) + ctr.Z, 2));

                /*
                float bx = Xmax - Xmin;
                float by = Ymax - Ymin;
                float bz = Zmax - Zmin;

                float brounded = (bx + by + bz) / 3;

                // bounds_legth /= 6;
                // reduce bouds_length of 15%
                //bounds_legth *= (65f / 100f); // removed 35%
                */

                // divide ctr floats by number of vertices

                float radius = Math.Abs((Math.Abs(Xmax) - Math.Abs(Xmin))) + Math.Abs((Math.Abs(Ymax) - Math.Abs(Ymin))) + Math.Abs((Math.Abs(Zmax) - Math.Abs(Zmin)));


                material_group_BBox mgBbox = new material_group_BBox();
                mgBbox.position = ctr;
                mgBbox.radius = radius;


#if DEBUG
                if (mgBbox.radius == 0)
                {
                    mgBbox.radius = 500;
                }

                if (debug_draw_distance)
                {
                    mgBbox.radius *= 10;
                }
#endif

                lines_bboxes.Add(mgBbox.position.X + " " + mgBbox.position.Y + " " + mgBbox.position.Z + " " + mgBbox.radius);

                points_list.Clear();

                file_buffers_list.Add(mgBbox.Serialize());
            }

            File.WriteAllLines(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\export\bboxes.txt", lines_bboxes);

            #endregion

            #region build vertex buffer


            if (vtxt_head.vertex_def_size == 28)
            {
                vertex_def v;
                for (int i = 0; i < mesh.vertex_buffer.Count; i++)
                {
                    // f mesh has normals include normals b
                    // if (mesh.normals_buffer.Count > 0)
                    // {
                    //v = new vertex_def(mesh.vertex_buffer[i], mesh.uv_buffer[mesh.uv_indices[i]], mesh.normals_buffer[mesh.normals_indices[i]]);
                    v = new vertex_def(mesh.vertex_buffer[i], mesh.uv_buffer[i], new Vector3(0, 0, 0)); //
                    file_buffers_list.Add(v.Serialize(28));
                    // }
                }
            }

            #endregion

            #region build triangle indices buffer


            for (int i = 0; i < mesh.face_indices.Count; i++)
            {
                file_buffers_list.Add((BitConverter.GetBytes((Int16)(mesh.face_indices[i] -1))));
            }

            #endregion

            return file_buffers_list.SelectMany(a => a).ToArray();
        }



        #region classes


        /// <summary>
        ///  second header 144 bytes
        /// </summary>
        public class Header
        {
            public Int32 x000_unk { get; set; }
            public Int32 x004_unk { get; set; } // always = 1
            public Int32 x008_unk { get; set; }
            public Int32 x012_unk { get; set; }

            public Int32 mat_group_start_offset { get; set; } // position relative to this Int32, cound from this position x016 + mat_group_start_offset
            public Int32 x020_unk { get; set; } // always 128
            public Int32 x024_unk { get; set; }
            public Int32 x00028_unk { get; set; }

            public Vector3 model_center { get; set; }
            public float model_radius { get; set; }

            public Int32 x048_unk { get; set; }
            public Int32 x052_unk { get; set; }
            public Int32 x056_unk { get; set; }
            public Int32 x060_unk { get; set; }

            public Int32 x064_unk { get; set; }
            public Int32 x068_unk { get; set; }
            public Int32 x072_unk { get; set; }
            public Int32 x076_unk { get; set; }

            public Vector3 x080_unk_Vector3 { get; set; }
            public float x092_unk { get; set; }

            public Int32 x096_unk { get; set; }
            public Int32 x100_unk { get; set; }
            public Int32 x104_unk { get; set; }
            public Int32 x108_unk { get; set; }

            public Int32 x112_unk { get; set; }
            public Int32 x116_unk { get; set; }
            public Int32 x120_unk { get; set; }

            public Int32 x124_mat_count { get; set; }
            public Int32 x128_unk { get; set; }
            public Int32 x132_tri_groups_count { get; set; }

            public Int32 x136_unk_ID { get; set; }
            public Int32 x140_unk_ID { get; set; }

            public Header()
            {
                x080_unk_Vector3 = new Vector3();
            }

            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();

                b.Add(BitConverter.GetBytes(x000_unk));
                b.Add(BitConverter.GetBytes(x004_unk));
                b.Add(BitConverter.GetBytes(x008_unk));
                b.Add(BitConverter.GetBytes(x012_unk));

                b.Add(BitConverter.GetBytes(mat_group_start_offset));
                b.Add(BitConverter.GetBytes(x020_unk));
                b.Add(BitConverter.GetBytes(x024_unk));
                b.Add(BitConverter.GetBytes(x00028_unk));

                b.Add(BitConverter.GetBytes(model_center.X));
                b.Add(BitConverter.GetBytes(model_center.Y));
                b.Add(BitConverter.GetBytes(model_center.Z));
                b.Add(BitConverter.GetBytes(model_radius));

                b.Add(BitConverter.GetBytes(x048_unk));
                b.Add(BitConverter.GetBytes(x052_unk));
                b.Add(BitConverter.GetBytes(x056_unk));
                b.Add(BitConverter.GetBytes(x060_unk));

                b.Add(BitConverter.GetBytes(x064_unk));
                b.Add(BitConverter.GetBytes(x068_unk));
                b.Add(BitConverter.GetBytes(x072_unk));
                b.Add(BitConverter.GetBytes(x076_unk));

                b.Add(BitConverter.GetBytes(x080_unk_Vector3.X));
                b.Add(BitConverter.GetBytes(x080_unk_Vector3.Y));
                b.Add(BitConverter.GetBytes(x080_unk_Vector3.Z));
                b.Add(BitConverter.GetBytes(x092_unk));

                b.Add(BitConverter.GetBytes(x096_unk));
                b.Add(BitConverter.GetBytes(x100_unk));
                b.Add(BitConverter.GetBytes(x104_unk));
                b.Add(BitConverter.GetBytes(x108_unk));

                b.Add(BitConverter.GetBytes(x112_unk));
                b.Add(BitConverter.GetBytes(x116_unk));
                b.Add(BitConverter.GetBytes(x120_unk));

                b.Add(BitConverter.GetBytes(x124_mat_count));
                b.Add(BitConverter.GetBytes(x128_unk));
                b.Add(BitConverter.GetBytes(x132_tri_groups_count));

                b.Add(BitConverter.GetBytes(x136_unk_ID));
                b.Add(BitConverter.GetBytes(x140_unk_ID));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }

        }

        /// <summary>
        ///  vertex buffer header
        /// </summary>
        public class vertex_triangles_buffers_header
        {
            public Int32 vertex_buffer_startoffset { get; set; } // gives position to last material_group_BBox
            public Int32 vertex_count { get; set; }
            public Int32 vertex_struct { get; set; }
            public Int32 vertex_def_size { get; set; }

            public Int32 triangle_buffer_startoffset { get; set; } // =  ( x004_vertex_count * x012_vertex_def_size ) + x000_unk
            public Int32 triangle_buffer_size { get; set; } // multiply by two
            public Int32 is_stripped { get; set; }
            public Int32 x0_28_padding { get; set; }


            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();

                b.Add(BitConverter.GetBytes(vertex_buffer_startoffset));
                b.Add(BitConverter.GetBytes(vertex_count));
                b.Add(BitConverter.GetBytes(vertex_struct));
                b.Add(BitConverter.GetBytes(vertex_def_size));
                b.Add(BitConverter.GetBytes(triangle_buffer_startoffset));
                b.Add(BitConverter.GetBytes(triangle_buffer_size));
                b.Add(BitConverter.GetBytes(is_stripped));
                b.Add(BitConverter.GetBytes(x0_28_padding));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }

        }

        /// <summary>
        /// (32 bytes) triangle group, defines triangles indices and material to apply 
        /// </summary>
        public class material_group
        {
            public Int32 triangle_count { get; set; } //
            public Int32 triangle_start_index { get; set; } // divide by 9 (multiply by 3 for compiling)
            public Int32 unk_08 { get; set; } // increases by +16 for each triangle group // first tri group start value seems th be the number of bytes from the start position to end of triangles group, minus -16
            public Int32 material_ID { get; set; } // tri count

            public Int32 unk_16 { get; set; } // 
            public Int32 unk_20 { get; set; } // 
            public Int32 unk_24 { get; set; } //
            public Int32 unk_28 { get; set; } //

            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();

                b.Add(BitConverter.GetBytes(triangle_count));
                b.Add(BitConverter.GetBytes(triangle_start_index));
                b.Add(BitConverter.GetBytes(unk_08));
                b.Add(BitConverter.GetBytes(material_ID));

                b.Add(BitConverter.GetBytes(unk_16));
                b.Add(BitConverter.GetBytes(unk_20));
                b.Add(BitConverter.GetBytes(unk_24));
                b.Add(BitConverter.GetBytes(unk_28));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class material_group_BBox
        {
            public Vector3 position { get; set; }
            public float radius { get; set; }

            public material_group_BBox()
            {
                position = new Vector3();
            }

            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();

                b.Add(BitConverter.GetBytes(position.X));
                b.Add(BitConverter.GetBytes(position.Y));
                b.Add(BitConverter.GetBytes(position.Z));
                b.Add(BitConverter.GetBytes(radius));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }

        /// <summary>
        /// material (20 bytes)  (HB block)
        /// </summary>
        public class material // 20 bytes (HB block)
        {
            public DataFormats.JSRF.MDLB.color color { get; set; } // its BGRA instead of RGBA (invert color order)
            public Int32 shader_id { get; set; } // if this = FFFFFF color_0 is used // otherwise external material or texture?
            public Int32 unk_id2 { get; set; }
            public Int32 pad12 { get; set; }
            public float HB { get; set; }

            public byte[] Serialize()
            {
                List<byte[]> b = new List<byte[]>();
                b.Add(color.Serialize());
                b.Add(BitConverter.GetBytes(shader_id));
                b.Add(BitConverter.GetBytes(unk_id2));
                b.Add(BitConverter.GetBytes(pad12));
                b.Add(BitConverter.GetBytes(HB));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }

        /// <summary>
        /// vertex definition structure, varies in length depending on how many properties are defined
        /// the Serialize class will write a vertex defnition according to the input _size
        /// </summary>
        public class vertex_def
        {
            public Vector3 pos { get; set; }
            public Vector2 uv { get; set; }
            public Vector3 norm { get; set; }

            public vertex_def(Vector3 _pos, Vector2 _uv, Vector3 _norm)
            {
                this.pos = _pos;
                this.uv = _uv;
                this.norm = _norm;
            }

            public byte[] Serialize(int _size)
            {
                List<byte[]> b = new List<byte[]>();
                // vertex position
                b.Add(BitConverter.GetBytes(pos.X)); b.Add(BitConverter.GetBytes(pos.Y)); b.Add(BitConverter.GetBytes(pos.Z));

                switch (_size)
                {

                    case 20:
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y));
                        break;
                    case 24:
                        b.Add(new byte[4]); b.Add(BitConverter.GetBytes(uv.X));  b.Add(BitConverter.GetBytes(uv.Y)); 
                        break;
                    case 28:
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y)); 
                        b.Add(new byte[4]);
                        b.Add(new byte[4]);
                        break;

                    case 32:
                        b.Add(BitConverter.GetBytes(norm.X)); b.Add(BitConverter.GetBytes(norm.Y)); b.Add(BitConverter.GetBytes(norm.Z));
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y));
                        break;

                    case 36:
                        b.Add(BitConverter.GetBytes(norm.X)); b.Add(BitConverter.GetBytes(norm.Y)); b.Add(BitConverter.GetBytes(norm.Z));
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y));
                        b.Add(new byte[4]);
                        break;
                }

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }


        #endregion

    }
}
