using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSRF_ModTool.Vector;
using JSRF_ModTool.DataFormats._3D_Model_Formats;
using System.IO;
using System.Text.RegularExpressions;

namespace JSRF_ModTool.DataFormats.JSRF
{
    class Stage_Model_Compiler
    {
        public List<texture_info> textures { get; set; }

        public Stage_Model_Compiler()
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

            #region import OBJ and check for errors

            // load .obj file into OBJ instance
            OBJ obj = new OBJ(obj_filepath);

            if(!obj.imported_succeeded)
            {
                // no need for a messagebox here, the OBJ class already gives a messagebox with the info on where/what failed
                return new byte[0];
            }

            // if obj has no vertices, return empty array
            if (obj.mesh.vertex_buffer.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: OBJ mesh could not be loaded.");
                return new byte[0];
            }

            // if model has no uvs
            if (obj.mesh.uv_buffer.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Error: OBJ mesh doesn't have UVs.");
                return new byte[0];
            }

            /*
            // check if there is more than one group with the same material
            // if so reject import
            List<String> mat_names = new List<string>();
            for (int i = 0; i < obj.mesh.material_groups.Count; i++)
            {
                if(mat_names.Contains(obj.mesh.material_groups[i].mat_name))
                {
                    System.Windows.Forms.MessageBox.Show("Error: OBJ mesh " + Path.GetFileName(obj.FilePath) + " has different face groups using the same material.\n\n"+ obj.FilePath + "\n\nPlease merge the different face groups using the same material into a single one." + "\n\nMaterial name: " + obj.mesh.material_groups[i].mat_name);
                    return new byte[0];
                }

                mat_names.Add(obj.mesh.material_groups[i].mat_name);
            }
            */

            #endregion

            //obj.flip_model_for_Stage();

            
            // flip UV map     
            for (int i = 0; i < obj.mesh.uv_buffer.Count; i++)
            {
                //obj.mesh.uv_buffer[i].X = ((obj.mesh.uv_buffer[i].X) * -1f) + 1;
                obj.mesh.uv_buffer[i].Y = ((obj.mesh.uv_buffer[i].Y) * -1f) + 1;
            }




            #region process materials groups and textures  and materials from the .mtl

            List<Int32> texture_IDs_list = new List<int>();
            
            /*
            // if obj's materials count differs from the mtl materials count
            if (obj.meshes[0].material_groups.Count != obj.mtl_materials_list.Count)
            {
                System.Windows.Forms.MessageBox.Show("Error: OBJ materials count does not match with the .mtl materials count.");
                return new byte[0];
            }
            */

            // add count of textures ids
            //texture_IDs_list.Add(BitConverter.GetBytes(obj.mesh_.material_groups.Count)); //BitConverter.GetBytes(obj.mesh_.material_groups.Count)

            // for each obj.mesh_.material_groups, get texture paths and generate ids for them
            for (int i = 0; i < obj.mesh.material_groups.Count; i++)
            {
                // for each material form the mtl file
                for (int e = 0; e < obj.mtl_materials_list.Count; e++)
                {
                    // if material group's texture filepath matches the texture filepath in .mtl material, create ID for the texture
                    if (obj.mesh.material_groups[i].texture_filepath.ToLower() == obj.mtl_materials_list[e].texture_filepath.ToLower())
                    {


                        // sleep for a bit, otherwise MathI.RandomDigits() keeps ouputting the same number in a short timeframe
                        System.Threading.Thread.Sleep(50);

                        Int32 tex_id = -1;

                        // check if texture path already exists in textures list
                        foreach (var tx in textures)
                        {
                            if(tx.texture_filepath == obj.mtl_materials_list[e].texture_filepath)
                            {
                                tex_id = tx.texture_id;
                                break;
                            }
                        }

                        bool valid_tex_path = true;
                        // if texture wasn't found generate ID and add to textures list
                        if(tex_id == -1)
                        {
                            // generate unique ID which will be what is used by the game file format to index textures
                            tex_id = Functions.MathI.RandomDigits(9);

                            if (File.Exists(obj.mtl_materials_list[e].texture_filepath))
                            {
                                valid_tex_path = true;
                                textures.Add(new texture_info(obj.mtl_materials_list[e].texture_filepath, tex_id));
                            } else {
                                valid_tex_path = false;
                                System.Windows.Forms.MessageBox.Show("Warning: the texture file for the material \"" + obj.mtl_materials_list[e].material_name + "\" cannot be found.\n\n"+ 
                                    "The texture path defined in the obj/mtl are invalid: \"" + obj.mtl_materials_list[e].texture_filepath + "\"\n\nThis material and texture will not be added to the compiled Stage.");
                                obj.mtl_materials_list.RemoveAt(e); 
                            }  
                        }

                        // if texture file exists, set texture ID on the mtl_materials_list, material_groups list and texture_IDs_list
                        if (valid_tex_path)
                        {
                            // generate texture ID and store it
                            obj.mtl_materials_list[e].ID = tex_id;
                            obj.mesh.material_groups[i].ID = tex_id;

                            // add ID to texture id list
                            texture_IDs_list.Add(tex_id);

                            break;
                        }
                    }
                }
            }

            List<string> mat_names_list = new List<string>();


            // add count of texture ids to buffer
            file_buffers_list.Add(BitConverter.GetBytes((Int32)texture_IDs_list.Count));
            // add list of texture ids
            file_buffers_list.Add(texture_IDs_list.SelectMany(BitConverter.GetBytes).ToArray());


            #endregion

            #region build main header

            Header head = new Header();
            head.x124_mat_count = 1;
            head.x128_unk = 180;
            head.x132_mat_groups_count = obj.mesh.material_groups.Count;

            #region calculate mesh center and bounding box

            //Vector3 model_center = new Vector3();
            bounds bbox = new bounds();

            // loop through the mesh vertex buffer
            // to get all vertices positions to calculate min/max bouding box and center of the material group mesh
            for (int o = 0 ; o < obj.mesh.vertex_buffer.Count; o++)
            {
                Vector3 vert = obj.mesh.vertex_buffer[o];

                bbox.add_point(vert);

                //model_center = new Vector3(model_center.X + vert.X, model_center.Y + vert.Y, model_center.Z + vert.Z);
            }


            // divide ctr floats by number of vertices
           // model_center = new Vector3(model_center.X / obj.mesh_.vertex_buffer.Count, model_center.Y / obj.mesh_.vertex_buffer.Count, model_center.Z / obj.mesh_.vertex_buffer.Count);


            head.model_center = bbox.center;

            // TODO : maybe have the possiblity to set a custom model_radius in case it's rendering turns off in some cases if the player goes out of that radius
            // calculate mesh draw distance radius from it's bounding box min/max and multiply by a factor (of 1.55f right now)
            head.model_radius = (Math.Abs((Math.Abs(bbox.Xmax) - Math.Abs(bbox.Xmin))) + Math.Abs((Math.Abs(bbox.Ymax) - Math.Abs(bbox.Ymin))) + Math.Abs((Math.Abs(bbox.Zmax) - Math.Abs(bbox.Zmin)))) * (1.55f);

#if DEBUG
            // force mesh draw distance radius
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

            head.x136_unk_ID = 1243916; // unknown default id
            head.x140_unk_ID = 2013322001; // unknown default id

            file_buffers_list.Add(head.Serialize());

            #endregion

            #region materials list

             // for now we only do one, some models can have two more?
            material mat = new material();
            mat.color = new MDLB.color(255, 255, 255, 255);
            mat.shader_id = 0;
            mat.unk_id2 = 16777215;
            mat.HB = 50;

            file_buffers_list.Add(mat.Serialize());

            #endregion

            #region vertex triangles buffers header

            vertex_triangles_buffers_header vtxt_head = new vertex_triangles_buffers_header();
           
            vtxt_head.last_MatGroupBbox_offset = Functions.Parsing.calc_length_bytes_list(file_buffers_list) - (texture_IDs_list.Count * 4 + 4) + 32 + (obj.mesh.material_groups.Count * 32) + ((obj.mesh.material_groups.Count -1) * 16);
            vtxt_head.vertex_count = obj.mesh.vertex_buffer.Count;
            vtxt_head.vertex_struct = 514; // 514 = 28 flag //  vtx def 28 = 512 flag //   vtx def 32 = 274 flag   // //  vtx def 24 = 322 flag //  vtx def 16 = 66 flag
            vtxt_head.vertex_def_size = 28;
            vtxt_head.triangle_buffer_startoffset = vtxt_head.last_MatGroupBbox_offset + (vtxt_head.vertex_count * vtxt_head.vertex_def_size);
            vtxt_head.triangle_buffer_size = obj.mesh.face_indices.Count; //  if= 36 then real byte size is * 21 = 72  // offset from texture counts it(start of this model)  +128

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
            for (int i = 0; i < obj.mesh.material_groups.Count; i++)
            {
                //material_group mat_group = new material_group();
                OBJ.obj_mesh.material_group mg = obj.mesh.material_groups[i];
                material_group mat_group  = new material_group();
                mat_group.triangle_count = mg.size;
                mat_group.triangle_start_index = mg.start_index * 3;
                mat_group.unk_08 = 0; 
                mat_group.material_num = i;

                mat_group.unk_16 = 0;
                mat_group.unk_20 = 0;
                mat_group.unk_24 = 0;
                mat_group.unk_28 = starto_mat_groups + (obj.mesh.material_groups.Count * 32) + (i * 16) - 4  - (texture_IDs_list.Count * 4) -16; 

                file_buffers_list.Add(mat_group.Serialize());
            }

            #endregion

            #region setup material groups boundaries(works as a radius)

            bounds bounds = new bounds();

            for (int i = 0; i < obj.mesh.material_groups.Count; i++)
            {
                // get mesh's material group
                OBJ.obj_mesh.material_group mg = obj.mesh.material_groups[i];

                // loop through the mat's triangles (from mg.start_index; to  mg.start_index + mg.size) * 3
                // (times 3 since we need 3 vertices for one triangle from face indices)
                // to get all vertices positions to calculate min/max bouding box and center of the material group mesh
                for (int o = mg.start_index * 3; o < (mg.start_index  + mg.size) * 3; o++)
                {
                    if (o >= obj.mesh.face_indices.Count) { break; }

                    Vector3 vert = obj.mesh.vertex_buffer[obj.mesh.face_indices[o] - 1];
                    bounds.add_point(vert);
                }


                float radius = Math.Abs((Math.Abs(bounds.Xmax) - Math.Abs(bounds.Xmin))) + Math.Abs((Math.Abs(bounds.Ymax) - Math.Abs(bounds.Ymin))) + Math.Abs((Math.Abs(bounds.Zmax) - Math.Abs(bounds.Zmin)));

                material_group_boundary mat_group_Boundary = new material_group_boundary();

                mat_group_Boundary.position = bounds.center;

                mat_group_Boundary.radius = radius;
 
                file_buffers_list.Add(mat_group_Boundary.Serialize());
            }

            #endregion

            #region build vertex buffer

            vertex_def v;
            Vector2 uv = new Vector2();

            if (vtxt_head.vertex_def_size == 28)
            {
                // for each vertex
                for (int i = 0; i < obj.mesh.vertex_buffer.Count; i++)
                {
                    // for each triangle's vertex index
                    for (int f = 0; f < obj.mesh.face_indices.Count; f++)
                    {
                        // if tri vertex index == i
                        if(obj.mesh.face_indices[f] -1 == i)
                        {
                            // get uv that we have to assign to the vertex
                            uv = obj.mesh.uv_buffer[obj.mesh.uv_indices[f] - 1];
                            break;
                        }
                    }

                    // create vertex with point position + uv + normal
                    v = new vertex_def(obj.mesh.vertex_buffer[i], uv, new Vector3(0, 0, 0)); // obj.mesh_.normals_buffer[i]  
                    file_buffers_list.Add(v.Serialize(28));
                }
            }

            #endregion

            #region build triangle buffer

            for (int t = 0; t < obj.mesh.face_indices.Count; t++)
            {
                file_buffers_list.Add(BitConverter.GetBytes((Int16)(obj.mesh.face_indices[t] -1)));
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
            public Int32 x132_mat_groups_count { get; set; }

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
                b.Add(BitConverter.GetBytes(x132_mat_groups_count));

                b.Add(BitConverter.GetBytes(x136_unk_ID));
                b.Add(BitConverter.GetBytes(x140_unk_ID));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }

        }

        /// <summary>
        ///  vertex / triangle buffer header
        /// </summary>
        public class vertex_triangles_buffers_header
        {
            public Int32 last_MatGroupBbox_offset { get; set; } // gives position to last material_group_boundary
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

                b.Add(BitConverter.GetBytes(last_MatGroupBbox_offset));
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
        /// (32 bytes) triangle material group, defines triangles indices and material to apply 
        /// </summary>
        public class material_group
        {
            public Int32 triangle_count { get; set; } //
            public Int32 triangle_start_index { get; set; } // divide by 9 (multiply by 3 for compiling)
            public Int32 unk_08 { get; set; } // increases by +16 for each triangle group // first tri group start value seems th be the number of bytes from the start position to end of triangles group, minus -16
            public Int32 material_num { get; set; } // tri count

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
                b.Add(BitConverter.GetBytes(material_num));

                b.Add(BitConverter.GetBytes(unk_16));
                b.Add(BitConverter.GetBytes(unk_20));
                b.Add(BitConverter.GetBytes(unk_24));
                b.Add(BitConverter.GetBytes(unk_28));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }

        /// <summary>
        /// the material group has a point position for it's center(calculated from the average of mesh vertices(points position) that are part of the material group)
        /// as well as a radius which is used to define the drawing distance for the material group, when the player/camera position
        /// is beyond the material's group radius distance, the material group stops rendering
        /// </summary>
        private class material_group_boundary
        {
            public Vector3 position { get; set; }
            public float radius { get; set; }

            public material_group_boundary()
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

            // serialize vertex definition according to the type/size of vertex def
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
