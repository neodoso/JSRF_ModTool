using System;
using System.Collections.Generic;
using JSRF_ModTool.Vector;
using JSRF_ModTool.Functions;

namespace JSRF_ModTool.DataFormats.JSRF
{
    public class Stage_Model
    {
        //MDLBL_header header;
        //header_second header_sec;
        public Header header;
        public List<Int32> texture_ids;
        public List<material> materials;
        public List<material_group> materials_groups;
        public vertex_triangles_buffers_header vtx_tri_buff_head;

        List<material_group_boundary> mat_group_boundary_list;

        public List<Vector3> vertices_list;
        public List<Vector3> normals_list;
        public List<Vector2> uv_list;
        public List<triangle> triangles_list;

        /// <summary>
        /// Load Stage model
        /// This is a work in progress, the Stage model headers can vary in size
        /// TODO: figure out why there sometimes is +12 bytes of data before triangle_group list
        /// TODO: find if there is a flag in the headers for when there is +12 bytes (couldnt find any flag so far)
        /// TODO: it seems the vertex buffer starts with series of vector3 and the real mesh points start a bit after
        /// </summary>
        /// <param name="data">Input data</param>
        public Stage_Model(byte[] _data)
        {
            Int32 texture_ids_count = BitConverter.ToInt32(_data, 0);
            Int32 tex_ids_length = texture_ids_count * 4;

            texture_ids = new List<int>();
            // read texture ids and add to list
            for (int i = 4; i < texture_ids_count*4 +4; i+=4)
            {
                texture_ids.Add(BitConverter.ToInt32(_data, i));
            }


            // remove texture_ids_count and list of texture ids from array
            byte[] data = new byte[_data.Length - tex_ids_length -4 ];
            Array.Copy(_data, tex_ids_length +4, data, 0, data.Length);


            Int32 offset = 0;
            materials = new List<material>();
            materials_groups = new List<material_group>();

            /////////////////// HEADER  //////////////////////////////////////////////////////////////////////////////////////////////
            // get second header part
            header = (Header)(Parsing.binary_to_struct(data, offset, typeof(Header)));
            offset += 144; //144

            /////////////////// MATERIALS LIST  //////////////////////////////////////////////////////////////////////////////////////////////
            // get list of materials
            for (int i = offset; i < offset + header.x124_mat_count * 20; i += 20)
            {
                materials.Add((material)(Parsing.binary_to_struct(data, i, typeof(material))));
            }

            // calculate offset after materials (1 material definition = 20 bytes)
            offset += header.x124_mat_count * 20;

            // if not materials, there's still 20 bytes of padding to skip
            // see stg00_00 : Stage model number 23
            if(header.x124_mat_count == 0 )
            {
                offset += 20;
            }

            /////////////////// VERTEX TRIANGLES DATA HEADERS  //////////////////////////////////////////////////////////////////////////////////////////////
            // vertex triangles buffer header
            vtx_tri_buff_head = (vertex_triangles_buffers_header)(Parsing.binary_to_struct(data, offset, typeof(vertex_triangles_buffers_header)));

            // set offset after " vertex triangles buffer header" (32 bytes)
            offset += 32;

            /////////////////// TRIANGLE GROUPS  //////////////////////////////////////////////////////////////////////////////////////////////
            // get each header.x132_tri_groups_count get triangle group
            // 1 triangle group = 32 bytes
            for (int i = 0; i < header.x132_mat_groups_count * 32; i += 32)
            {
                materials_groups.Add((material_group)(Parsing.binary_to_struct(data, offset + i, typeof(material_group))));

                // recalculate triangle counts
                materials_groups[materials_groups.Count - 1].triangle_start_index /= 3;
            }

            // set offset after triangles groups
            offset += header.x132_mat_groups_count * 32;


            /////////////////// MATERIAL GROUP BOUNDING BOXES  //////////////////////////////////////////////////////////////////////////////////////////////
            mat_group_boundary_list = new List<material_group_boundary>();
            int mat_group_BBoxes_end = offset + (header.x132_mat_groups_count * 16);
            // get each material group bouding box
            for (int i = offset; i < mat_group_BBoxes_end; i += 16)
            {
                mat_group_boundary_list.Add((material_group_boundary)(Parsing.binary_to_struct(data, offset + i, typeof(material_group_boundary))));
            }

            offset = mat_group_BBoxes_end;

            /////////////////// VERTICES NORMALS UVS  //////////////////////////////////////////////////////////////////////////////////////////////
            // calculate and store vertex buffer end position
            vertices_list = new List<Vector3>();
            uv_list = new List<Vector2>();
            normals_list = new List<Vector3>();
            int vertex_buffer_end = offset + vtx_tri_buff_head.vertex_buffer_size - vtx_tri_buff_head.last_MatGroupBbox_offset;

            // for each vertex-block (of size vtx_tri_buff_head.x012_vertex_def_size) 
            // add vertex into vertices_list
            for (int i = offset; i < vertex_buffer_end; i += vtx_tri_buff_head.vertex_def_size)
            {
                if(i + 8 < data.Length)
                vertices_list.Add(new Vector3(BitConverter.ToSingle(data, i), BitConverter.ToSingle(data, i + 4), BitConverter.ToSingle(data, i + 8)));

                if (vtx_tri_buff_head.vertex_def_size == 36)
                {
                    //normals_list.Add(new Vector3(BitConverter.ToSingle(data, i + 16), BitConverter.ToSingle(data, i + 20), BitConverter.ToSingle(data, i + 24)));
                    uv_list.Add(new Vector2(BitConverter.ToSingle(data, i + 20), BitConverter.ToSingle(data, i + 24)));
                }

                if (vtx_tri_buff_head.vertex_def_size == 32)
                {
                    normals_list.Add(new Vector3(BitConverter.ToSingle(data, i + 12), BitConverter.ToSingle(data, i + 16), BitConverter.ToSingle(data, i + 20)));
                    uv_list.Add(new Vector2(BitConverter.ToSingle(data, i + 24), BitConverter.ToSingle(data, i + 28)));
                }
                else if (vtx_tri_buff_head.vertex_def_size == 28)
                {
                    if(i + 16 < data.Length)
                    uv_list.Add(new Vector2(BitConverter.ToSingle(data, i + 12),  (-1 * BitConverter.ToSingle(data, i + 16)) + 1 ));
                }
                else if (vtx_tri_buff_head.vertex_def_size == 24)
                {
                    if(i + 20 < data.Length)
                    uv_list.Add(new Vector2(BitConverter.ToSingle(data, i + 16), BitConverter.ToSingle(data, i + 20)));
                }
            }

            /////////////////// READ AND STORE TRIANGLES VERT INDICES  //////////////////////////////////////////////////////////////////////////////////////////////
            List<short> tri_indx = new List<short>();
            triangles_list = new List<triangle>();
            int triangles_buffer_end = vertex_buffer_end + vtx_tri_buff_head.triangle_buffer_size * 2;
 
            for (int i = vertex_buffer_end; i < triangles_buffer_end; i += 2)
            {
                if(i < data.Length)
                tri_indx.Add((short)(BitConverter.ToInt16(data, i) +1));
            }

            /*
            List<string> tri_indices = new List<string>();
            for (int i = 0; i < tri_indx.Count-2; i+=3)
            {
                tri_indices.Add((tri_indx[i]).ToString() + " " + (tri_indx[i +1]).ToString() + " " + (tri_indx[i+2]).ToString());
            }

            // debug data export
            System.IO.File.Delete(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\export\stripped_tris.txt");
            System.IO.File.AppendAllLines(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\export\stripped_tris.txt", tri_indices);
            */

            List<String> tris_list = new List<string>();

            List<int> tri_lstart = new List<int>();

            /////////////////// PROCESS TRIANGLES  //////////////////////////////////////////////////////////////////////////////////////////////
            // if mesh is stripped
            if (vtx_tri_buff_head.is_stripped == 0)
            {
                // for each triangle vertex index
                for (int i = 0; i < tri_indx.Count-1; i += 3)
                {
                    triangles_list.Add(new triangle(tri_indx[i], tri_indx[i + 1], tri_indx[i + 2]));
                }
            } 
            else // process stripped triangles into triangles list
            {
                #region stripped triangles processing

                short a, b, c;
                bool is_invalid = false; bool prev_is_invalid = false;
                int prev_valid_count = 0; int curr_valid_count = 0;
                int prev_invalid_count = 0; bool plus_one = false;
                int invalid_count = 0;

                //int one_shift = 0;

                // for each triangle vertex index
                // convert strip to list
                for (int i = 0; i < tri_indx.Count - 2; i++)
                {
                    a = tri_indx[i]; b = tri_indx[i + 1]; c = tri_indx[i + 2];

                    // INVALID TRIANGLE
                    if (a == b || a == c || b == c)
                    {
                        // if previous is valid
                        if (!prev_is_invalid)
                        {
                            // prev_valid_count = curr_valid_count;
                            // if the count of previous valid triangles = 1
                            // add one to the next curr_valid_count
                            if (prev_invalid_count == 1)
                            {
                                // one_shift = 1;
                            }

                            // if curr_valid_count has enough triangles(6) 
                            if (curr_valid_count == 6 && prev_invalid_count == 1 && (curr_valid_count - prev_valid_count) > 2)
                            {
                                // ensure that list of triangles is bigger than 5 (substracting the extra +1 from previous lone triangle)
                                if(plus_one && curr_valid_count-1 > 5)
                                {
                                    int index = triangles_list.Count - (Math.Abs(curr_valid_count - prev_valid_count));
                                    triangles_list.RemoveRange(index, 2);
                                }
                            }

                                // if triangle list hast more than 4 triangles
                                if (curr_valid_count > 6 && (curr_valid_count - prev_valid_count) > 2) // (curr_valid_count > (prev_valid_count + 1))
                                {
                                plus_one = false;
                                int index = triangles_list.Count - (Math.Abs(curr_valid_count - prev_valid_count));

                                triangles_list.RemoveRange(index, 2);
                                }
                        }

                        is_invalid = true; prev_is_invalid = true;
                        invalid_count++;
                    }
                    else  // valid triangle
                    {
                        // if previous triangle was invalid
                        if (prev_is_invalid)
                        { 
                            prev_is_invalid = false;
                            prev_invalid_count = invalid_count;
                            prev_valid_count = curr_valid_count;
                            curr_valid_count = 0;

                            if(prev_invalid_count == 1 && prev_valid_count == 1)
                            {
                                plus_one = true;
                                curr_valid_count++;
                            }

                            //if (invalid_count == 1) { one_shift = 1 }
                            invalid_count = 0;
                        }

                        is_invalid = false; curr_valid_count++;
                    }
                        


                    #region set triangles

                    if (i % 2 == 0)
                    {
                       // tris_list.Add(a + " " + b + " " + c + ((is_invalid) ? " #" : "") + " :" + curr_valid_count + ":" + prev_valid_count + "    | " + i); ;
                        if (!is_invalid)
                        {
                            triangles_list.Add(new triangle(a, b, c));
                        }
                    }
                    else
                    {
                       // tris_list.Add(c + " " + b + " " + a + ((is_invalid) ? " #" : "") + " :" + curr_valid_count + ":" + prev_valid_count + "    | " + i); ;
                        if (!is_invalid)
                        {
                            triangles_list.Add(new triangle(c, b, a));
                        }
                    }

                    #endregion

                   

                }

                #endregion
            }

            // debug data export
            //System.IO.File.Delete(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\export\destripped_tris.txt");
            //System.IO.File.AppendAllLines(@"C:\Users\Mike\Desktop\JSRF\research\mdls_stg\export\destripped_tris.txt", tris_list);
        }

        public void export_model(string filepath)
        {
            List<string> obj_lines = new List<string>();
            List<string> mtl_lines = new List<string>();
            List<string> vertices = new List<string>();
            List<string> uvs = new List<string>();
            List<string> normals = new List<string>();
            List<string> triangles = new List<string>();

            // set obj (v) vertex and (vt) vertex texture lists
            for (int i = 0; i < vertices_list.Count; i++)
            {
                vertices.Add("v " + vertices_list[i].X + " " + vertices_list[i].Y + " " + vertices_list[i].Z);
                if( i < uv_list.Count)
                {
                    uvs.Add("vt " + uv_list[i].X + " " + uv_list[i].Y);
                }

                if(i < normals_list.Count)
                normals.Add("vn " + normals_list[i].X + " " + normals_list[i].Y + " " + normals_list[i].Z);
            }


            obj_lines.Add("mtllib " + System.IO.Path.GetFileNameWithoutExtension(filepath) + ".mtl");
            obj_lines.Add("o " + System.IO.Path.GetFileNameWithoutExtension(filepath));

            int tri_group_offset = 0; Int32 tri_end = 0;
            var mat_group_indices = new List<Tuple<int, int>>();
            
            // for each triangle group
            for (int g = 0; g < this.materials_groups.Count; g++)
            {     
                material_group grp = this.materials_groups[g];
                tri_end += grp.triangle_count;

                mat_group_indices.Add(Tuple.Create(tri_end, texture_ids[grp.material_num]));

                tri_group_offset += grp.triangle_count;

                // write mtl material
                mtl_lines.Add("newmtl mat_" + texture_ids[grp.material_num]);
                mtl_lines.Add("map_Kd C:/Users/Mike/Desktop/JSRF/research/mdls_stg/stg51/textures/" + texture_ids[grp.material_num] + ".bmp");
                mtl_lines.Add("Ks 0 0 0");
                mtl_lines.Add("");
            }

            if(materials_groups.Count == 0)
            {
                mat_group_indices.Add(Tuple.Create(triangles_list.Count, 0));
            }

            // for each triangle in this group
            for (int t = 0; t < triangles_list.Count; t++)
            {
                if(t < triangles_list.Count)
                {
                    if( t >= mat_group_indices[0].Item1)
                    {
                        if(mat_group_indices.Count > 1)
                        mat_group_indices.RemoveAt(0);
                    }
                    triangles.Add("usemtl mat_" + mat_group_indices[0].Item2);
                    // 32 vertex definition size
                    // we write vertex / uv / normal
                    if (uv_list.Count > 0 && normals.Count > 0) ///vtx_tri_buff_head.vertex_def_size > 28
                    {
                        triangles.Add("f " + triangles_list[t].a + "/" + triangles_list[t].a + "/" + triangles_list[t].a + " " +
                                            triangles_list[t].b + "/" + triangles_list[t].b + "/" + triangles_list[t].b + " " +
                                            triangles_list[t].c + "/" + triangles_list[t].c + "/" + triangles_list[t].c
                                        );

                    }
                    else if (uv_list.Count > 0)
                    { // smaller than vertex def 32
                        // we only write vertex / uv
                        triangles.Add("f " + triangles_list[t].a + "/" + triangles_list[t].a + " " +
                                            triangles_list[t].b + "/" + triangles_list[t].b + " " +
                                            triangles_list[t].c + "/" + triangles_list[t].c
                                        );
                    }
                    else if (uv_list.Count == 0)
                    { // smaller than vertex def 32
                        // we only write vertex / uv
                        triangles.Add("f " + triangles_list[t].a + " " + triangles_list[t].b + " " + triangles_list[t].c);
                    }
                }

            }
           
         

            obj_lines.AddRange(vertices);

            if (uvs.Count > 0) //vtx_tri_buff_head.vertex_def_size >= 20
            {
                obj_lines.AddRange(uvs);
            }

            if (normals.Count > 0) //vtx_tri_buff_head.vertex_def_size > 28
            {
                obj_lines.AddRange(normals);
            }

            obj_lines.AddRange(triangles);



            // export Stage model
            //System.IO.File.Delete(filepath);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filepath));
            System.IO.File.AppendAllLines(filepath, obj_lines);

            //System.IO.File.Delete(filepath.Replace(".obj", ".mtl"));
            System.IO.File.AppendAllLines(filepath.Replace(".obj", ".mtl"), mtl_lines);

        }

        #region classes

        public class triangle
        {
            public Int16 a { get; set; }
            public Int16 b { get; set; }
            public Int16 c { get; set; }

            public triangle(Int16 _a, Int16 _b, Int16 _c)
            {
                this.a = _a;
                this.b = _b;
                this.c = _c;
            }
        }

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

            public Int32 x136_unk { get; set; }
            public Int32 x140_unk { get; set; }
        }

        /// <summary>
        ///  vertex buffer header (28 bytes)
        /// </summary>
        public class vertex_triangles_buffers_header
        {
            public Int32 last_MatGroupBbox_offset { get; set; } // gives position to last material_group_BBox (Material_group_boundary)
            public Int32 vertex_count { get; set; } 
            public Int32 vertex_struct { get; set; }
            public Int32 vertex_def_size { get; set; }
            public Int32 vertex_buffer_size { get; set; } // =  ( x004_vertex_count * x012_vertex_def_size ) + x000_unk
            public Int32 triangle_buffer_size { get; set; } // multiply by two
            public Int32 is_stripped { get; set; }
            public Int32 x0_28_padding { get; set; }
        }

        /// <summary>
        /// (32 bytes) triangle group, defines triangles indices and material to apply 
        /// </summary>
        public class material_group
        {
            public Int32 triangle_count { get; set; } //
            public Int32 triangle_start_index { get; set; } // divide by 9
            public Int32 unk_08 { get; set; } // increases by +16 for each triangle group // first tri group start value seems th be the number of bytes from the start position to end of triangles group, minus -16
            public Int32 material_num { get; set; } // tri count

            public Int32 unk_16 { get; set; } // divide this by 3 
            public Int32 unk_20 { get; set; } // 
            public Int32 unk_24 { get; set; } //
            public Int32 unk_28 { get; set; } // 
        }

        /// <summary>
        /// the material group has a point position for it's center(from the mesh vertices that are part of the material group)
        /// as well as a radius which is used to define the drawing distance for the material group, when the player/camera position
        /// is beyond the material's group radius distance, the material group stops rendering
        /// TODO : re-test the radius and position parameters ingame, afaik the mesh's material group stops rendering if we around beyond the mat group radius
        /// also todo: if that's how this works, inform users of the JSRF ModTool that if they're going to create Stages/stages, it is recommended that the parts of meshes using the same material
        /// should preferabily be grouped close together (distance/space-wise), otherwise the calculated radius will be larger which means the material group
        /// will be more likely to be constantly rendering, and if most of the materials groups have a large radius then nearly everything will be constantly rendering, it will add up and be costly on graphics performance
        /// </summary>
        private class material_group_boundary
        {
            Vector3 position { get; set; }
            float radius { get; set; }
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
        }

        #endregion

    }
}
