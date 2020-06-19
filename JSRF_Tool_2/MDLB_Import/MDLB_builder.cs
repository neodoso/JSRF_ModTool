using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSRF_Tool_2.Vector;
using System.IO;

namespace JSRF_Tool_2
{
    class MDLB_builder
    {
        // build JSRF MDLB model
        public byte[] build_MDLB(List<MDLB_Import.ModelPart_Import_Settings> mdl_list, List<MDLB_builder.material> materials) //, vertex_type vert_type
        {
            #region import each SMD

            // create array for each SMD
            SMD[] SMD_parts = new SMD[mdl_list.Count];
            int mdl_parts_count = mdl_list.Count;

            // import each model parts's SMD file and store it in list SMD_parts[]
            for (int i = 0; i < mdl_parts_count; i++)
            {
                SMD_parts[i] = new SMD(mdl_list[i].filepath);
            }

            #endregion

            #region check SMD for incompatiblity or errors


            if (SMD_parts[SMD_parts.Length - 1].nodes.Count < mdl_list.Count - 1) //vertex_type >= 40 
            {
                System.Windows.MessageBox.Show("MDLB_builder.cs Error: the imported SMD bone count must match the original model's bone count (" + mdl_parts_count + ") bones, " +
                                                "\nimported SMD has [" + SMD_parts[SMD_parts.Length - 1].nodes.Count + "] bones.");
                return new byte[0];
            }


            // check that the max number of deformers per vertex is 2
            for (int i = 0; i < SMD_parts[mdl_list.Count - 1].vertices_list.Count - 1; i++)
            {
                SMD.vertex v = SMD_parts[mdl_list.Count - 1].vertices_list[i];
                if (v.bone_weights.Count > 2)
                {
                    System.Windows.MessageBox.Show("Error: the imported SMD file " + Path.GetFileName(SMD_parts[mdl_list.Count - 1].FilePath) + " has one or multiple vertices with more than two deformers per vertex, " +
                                                    "\nplease ensure that there is only a maximum of 2 deformers per vertex." + "\nVertex position is:" + v.pos.X + " " + v.pos.Y + " " + v.pos.Z);
                    return new byte[0];
                }
            }

            #endregion

            #region convert nodes to JSRF format and import lower SMD model parts

            List<node_jsrf> nodes_jsrf = new List<node_jsrf>();
            SMD smd_lastPart = SMD_parts[mdl_list.Count - 1];

            // rigged models (multiple model parts, has bones/rigging)
            if (mdl_list.Count > 1)
            {
                #region convert nodes to JSRF format 

                // re-organize nodes to JSRF format list of: (child_node_id, shared_parent_node_id)
                #region get list of nodes that share the same parent node

                List<shared_parent> shared_parents = new List<shared_parent>();
                int[] shared_parent_cnt = new int[smd_lastPart.nodes.Count];

                // count how many times a node is used as a parent
                for (int i = 1; i < smd_lastPart.nodes.Count; i++)
                {
                    if (smd_lastPart.nodes[i].parent_ID >= 0)
                    {
                        shared_parent_cnt[smd_lastPart.nodes[i].parent_ID] += 1;
                    }
                }

                // count how many times a node is used as a parent
                for (int i = 1; i < shared_parent_cnt.Length - 1; i++)
                {
                    if (shared_parent_cnt[i] > 1)
                    {
                        shared_parents.Add(new shared_parent(i));
                    }
                }

                // add shared children for each parent node in shared_parents
                for (int x = 0; x < shared_parents.Count; x++)
                {
                    for (int i = 1; i < smd_lastPart.nodes.Count; i++)
                    {
                        if (shared_parents[x].parent == smd_lastPart.nodes[i].parent_ID)
                        {
                            shared_parents[x].children.Add(smd_lastPart.nodes[i].ID);
                        }
                    }
                }

                #endregion



                #region copy smd nodes to list for JSRF nodes format
                // generate nodes list for JSRF format
                for (int i = 0; i < smd_lastPart.nodes.Count; i++)
                {

                    if (smd_lastPart.nodes[i].parent_ID == -1)
                    {
                        nodes_jsrf.Add(new node_jsrf(smd_lastPart.nodes[i].ID, smd_lastPart.nodes[i].parent_ID)); //smd_data.nodes[i].parent_ID
                    }
                    else
                    {
                        nodes_jsrf.Add(new node_jsrf(smd_lastPart.nodes[i].ID, 0));
                    }


                    // if parent id is negative make last part the parent
                    if (nodes_jsrf[i].shared_parent_id == -1)
                    {
                        nodes_jsrf[i].shared_parent_id = smd_lastPart.nodes.Count;
                    }

                    // get/set children node
                    if (i + 1 < smd_lastPart.nodes.Count)
                    {
                        // if parent id is negative make last part the parent
                        if (smd_lastPart.nodes[i + 1].parent_ID == i)
                        {
                            nodes_jsrf[i].child_id = i + 1;
                        }

                        if (nodes_jsrf[i].child_id == i)
                        {
                            nodes_jsrf[i].child_id = 0;
                        }
                    }
                }

                // set last node's child = 0 (aka none)
                nodes_jsrf[nodes_jsrf.Count - 1].child_id = 0;

                #endregion

                #region setup shared parent nodes ids
                // for each list of child nodes sharing the same parent
                for (int p = 0; p < shared_parents.Count; p++)
                {
                    // for each child sharing the same parent node
                    for (int c = 0; c < shared_parents[p].children.Count; c++)
                    {

                        // if last child is sharing the same parent, set parent = 0
                        if (c == shared_parents[p].children.Count - 1)
                        {
                            nodes_jsrf[shared_parents[p].children[c]].shared_parent_id = 0;
                            continue;
                        }

                        if (c >= 0)
                        {
                            nodes_jsrf[shared_parents[p].children[c]].shared_parent_id = shared_parents[p].children[c + 1];
                            continue;
                        }
                    }
                }

                #endregion

                #endregion

                #region recalculate bone positions

                for (int i = 0; i < smd_lastPart.skeleton_nodes.Count; i++)
                {
                    // recalculate bone position (substract bone.pos by parent_bone.pos)
                    if (i > 0)
                    {
                        Vector3 pos = smd_lastPart.skeleton_nodes[i].pos;
                        Vector3 vpp = new Vector3();

                        if (i >= 0)
                        {
                            if (smd_lastPart.nodes[i].parent_ID != -1)
                            {
                                //vpp = smd_data.skeleton_nodes[nodes_jsrf[i].shared_parent_id].pos;
                                vpp = smd_lastPart.skeleton_nodes[smd_lastPart.nodes[i].parent_ID].pos;
                            }
                        }

                        // substract parent position 
                        smd_lastPart.skeleton_nodes[i].pos = new Vector3(pos.X + vpp.X, pos.Y + vpp.Y, pos.Z + vpp.Z);
                    }
                }

                #endregion

                #region import lower model parts SMDs


                string smd_part_path;
                List<string> smd_not_found = new List<string>();

                // import model parts if SMD file exists
                for (int i = 0; i < mdl_parts_count - 1; i++)
                {
                    smd_part_path = Path.GetDirectoryName(smd_lastPart.FilePath) + "\\" + Path.GetFileNameWithoutExtension(smd_lastPart.FilePath) + "_p_" + i + ".smd";
                    if (File.Exists(smd_part_path))
                    {
                        SMD_parts[i] = new SMD(smd_part_path);
                    }
                    else
                    {
                        smd_not_found.Add(i.ToString());

                        // System.Windows.MessageBox.Show("MDLB_builder Error: could not find SMD model part " + i + "\nexpecting file: " + smd_part_path);
                    }
                }
                /*
                // if SMD model part not found give error message
                if(smd_not_found.Count > 0)
                {
                    string mdl_nums = String.Empty;
                    for (int i = 0; i < smd_not_found.Count; i++)
                    {
                        mdl_nums = mdl_nums + "  [p_" + smd_not_found[i] + "]";
                    }


                    System.Windows.MessageBox.Show("MDLB_builder Error: could not find SMD models parts: " + mdl_nums);
                    return new byte[0];
                }
                */
                #endregion

            }
            else
            { // static model (only one model part = 1 mesh, no rigging)
                SMD_parts[0] = smd_lastPart;
            }

            #endregion


            #region triangle groups list

            // list to store generated triangle_group definition byte arrays
            List<byte[]> bytes_triangle_groups_list = new List<byte[]>();
            triangle_group tg; 

                // for each model part
                for (int i = 0; i < mdl_parts_count; i++)
                {
                    // only build triangle_groups array if the SMD has more than one material
                    if (SMD_parts[i].mat_groups_list.Count > 1 || Main.model.Model_Parts_header_List[i].triangle_groups_count > 0)
                    {

                        // for each material group from the imported SMD file
                        for (int m = 0; m < SMD_parts[i].mat_groups_list.Count; m++)
                        {

                        if(!SMD_parts[i].mat_groups_list[m].material_name.Contains("mat_"))
                        {
                            System.Windows.Forms.MessageBox.Show("Error importing model, invalid material name, expected 'mat_'");
                            return null;
                        }

                        int mesh_type = 1;
                        // if its a visual mesh  set mesh_type to 0
                        if( i == mdl_parts_count-1)
                        {
                            mesh_type = 0;
                        }

                            // TODO : determine if mat_index in triangle_group(_mat_index) needs to be shifted of +1 or not
                            // instance triangle group with SMD data and  (mesh_type, material_index) default values
                            tg = new triangle_group((SMD_parts[i].mat_groups_list[m].triangle_count / 3) +1, SMD_parts[i].mat_groups_list[m].triangle_start_index, mesh_type, Convert.ToInt32( SMD_parts[i].mat_groups_list[m].material_name.Replace("mat_", "")) );

                        
                            // if triangle group exists in original model, get original values for (mesh_type, material_index)
                            if (Main.model.Model_Parts_header_List[i].triangle_groups_List.Count > 0 && m < Main.model.Model_Parts_header_List[i].triangle_groups_List.Count)
                            {
                                DataFormats.JSRF.MDLB.triangle_group ori_tg = Main.model.Model_Parts_header_List[i].triangle_groups_List[m];
                                tg = new triangle_group((SMD_parts[i].mat_groups_list[m].triangle_count / 3) +1, SMD_parts[i].mat_groups_list[m].triangle_start_index, mesh_type, ori_tg.material_index);
                            }
                            

                        // serialize and store in list
                        bytes_triangle_groups_list.Add(tg.serialize());
                        }
                    }
                }
          
            // convert triangles_group_list of byte arrays into a single array
            byte[] triangle_groups_list_bytes = new byte[0];
            triangle_groups_list_bytes = bytes_triangle_groups_list.SelectMany(byteArr => byteArr).ToArray();

            #endregion

            #region materials list

            // list to store serialized materials to list of byte arrays
            List<byte[]> bytes_materials_list = new List<byte[]>();
            byte[] materials_list_bytes = new byte[0];

            // if original model has materials after the triangles_groups
            //if (Main.model.materials_List.Count > 0)
            for (int i = 0; i < materials.Count; i++) //Main.model.header.materials_count
            {
                bytes_materials_list.Add(materials[i].serialize());
            }

            // if original model gave materials
            if (Main.model.header.materials_count > 0)
            {
                // add serialized material to list of byte arrays
                bytes_materials_list.Add(new byte[calc_remainder_padding(bytes_materials_list.Count * 20)]);
                // padding
                //bytes_materials_list.Add(new byte[16]);
                materials_list_bytes = bytes_materials_list.SelectMany(byteArr => byteArr).ToArray();
            }


            #endregion


            #region calculate "verts_tris_buffers_start_offset"
            int model_parts_headers_total_size = mdl_parts_count * 128;

            int verts_tris_buffers_start_offset = 0;
            // calculate start offset for vertex/triangles buffers
            if (Main.model.header.materials_count > 0) //(Main.model.header.materials_count > 0)
            {
                 verts_tris_buffers_start_offset = (mdl_parts_count * 128) + triangle_groups_list_bytes.Length + materials_list_bytes.Length;
            }
            else
            {
                 verts_tris_buffers_start_offset = (mdl_parts_count * 128) + triangle_groups_list_bytes.Length;
            }
            #endregion

            #region Build Model_Part_header(s), Vertex_triangles_buffers_header and vertex/triangle buffers

            int vertex_blocks_offset = verts_tris_buffers_start_offset;

            List<byte[]> mdl_part_headers_Serialized_List, vtx_tri_buffers_Serialized_List, vtx_buffer_list, tris_buffer_list;
            vtx_tri_buffers_Serialized_List = new List<byte[]>();
            mdl_part_headers_Serialized_List = new List<byte[]>();
            byte[] vtx_buffer, tris_buffer;
            Vertex_triangles_buffers_header vertex_tris_header = new Vertex_triangles_buffers_header();
            //int vert_def_size;

            // for each model part
            for (int i = 0; i < mdl_parts_count; i++)
            {
                vertex_tris_header = new Vertex_triangles_buffers_header();
                vtx_buffer_list = new List<byte[]>(); tris_buffer_list = new List<byte[]>();
                List<byte[]> materials_buffer_list = new List<byte[]>();
                byte[] vtx_materials_list_bytes = new byte[0];
                int vtx_materials_count = 0;

                #region build vertex tris buffer materials list

                // if from original model, materials = 0, it means materials are stored after the vertex_tris_header, (meaning there is no materials table after the triangle groups)
                if (Main.model.header.materials_count == 0)
                {
                    // lower part
                    if (i < mdl_parts_count - 1 && Main.model.header.materials_count == 0) //materials.Count > 0
                    {
                        //changed nor sure if this works
                        vtx_materials_count++;
                        materials_buffer_list.Add(materials[0].serialize());
                        materials_buffer_list.Add(BitConverter.GetBytes(SMD_parts[i].triangles_list.Count));
                        byte[] tmp_buff = materials_buffer_list.SelectMany(byteArr => byteArr).ToArray();
                        byte[] tmp_padding = new byte[calc_remainder_padding(tmp_buff.Length)];
                        materials_buffer_list.Add(tmp_padding);
                        vtx_materials_list_bytes = materials_buffer_list.SelectMany(byteArr => byteArr).ToArray();
                    }

                    // last model part
                    if (i == mdl_parts_count - 1)
                    {
                        for (int m = 0; m < materials.Count; m++) //smd_lastPart.mat_groups_list.Count
                        {
                            vtx_materials_count++;
                            materials_buffer_list.Add(materials[m].serialize()); //Main.model.Model_Parts_header_List[i].vtx_buffer_materials[m].serialize()
                        }

                        byte[] tmp_materials_list_bytes = materials_buffer_list.SelectMany(byteArr => byteArr).ToArray();
                        byte[] padding = new byte[calc_remainder_padding(tmp_materials_list_bytes.Length)];
                        materials_buffer_list.Add(padding);

                        vtx_materials_list_bytes = materials_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                    }


                }




                #endregion

                #region setup vertex triangle buffer header (16 bytes)

                // if last part 
                vertex_tris_header.vertex_def_size = mdl_list[i].vertex_def_size;
                /*
                // if lower model part is a bone set the unkown flag as 530
                if (i < mdl_parts_count - 1)
                {
                    vertex_tris_header.unk_flag = 530;
                }
                */
                #region set unknown flag in 'vertex_tris_header' that varies depending on the vertex_def_size

                // TODO add case for were flag value is different if its a bone model part ("if bone")
                switch (vertex_tris_header.vertex_def_size)
                {
                    case 12:
                        vertex_tris_header.unk_flag = 2;
                        break;

                    case 20:
                        vertex_tris_header.unk_flag = 258;
                        break;

                    case 24:
                        vertex_tris_header.unk_flag = 18; // 386 // 322
                        break;

                    case 32:
                        vertex_tris_header.unk_flag = 274;
                        break;

                    case 40:
                        if (i < mdl_parts_count - 1)
                        {
                            vertex_tris_header.unk_flag = 530;
                        } else {
                            vertex_tris_header.unk_flag = 4376; ///4376; // 530 if bone
                        }

                        break;

                    case 44:
                        vertex_tris_header.unk_flag = 4440; // 530 if bone
                        break;

                    case 48:
                        vertex_tris_header.unk_flag = 4632; // 722 if bone
                        break;

                    case 56:
                        vertex_tris_header.unk_flag = 4824;
                        break;

                }

                #endregion


                #endregion


                #region build vertex buffer

                int bid_1;

                int vert_num = 0;
                // for each vertex
                foreach (var v in SMD_parts[i].vertices_list)
                {
                    bid_1 = 0;

                    // if rigged model, import lower model parts as bones
                    if (i < mdl_parts_count - 1)
                    {
                        // if its a shadow or bone model set vertex weight to current bone
                        vtx_buffer_list.Add((new vertex_def(v.pos, v.uv, v.norm, 1, 0, 0)).Serialize(mdl_list[i].vertex_def_size));
                    }
                    else // last model part
                    {
                        // if second bone id is defined
                        if (v.bone_weights.Count > 1)
                        {
                            bid_1 = v.bone_weights[1].bone_ID;
                        }

                        if (v.bone_weights.Count == 0)
                        {
                            System.Windows.MessageBox.Show("MDLB_builder.cs : Error, one or multiple vertices do not have a deformer assigned." + "\nError at vert number: " + vert_num);
                            return new byte[0];
                        }

                        if (v.bone_weights[0].weight == 1)
                        {
                            bid_1 = v.bone_weights[0].bone_ID; 
                        }

                        vtx_buffer_list.Add((new vertex_def(v.pos, v.uv, v.norm, v.bone_weights[0].weight, (sbyte)v.bone_weights[0].bone_ID, (sbyte)bid_1)).Serialize(mdl_list[i].vertex_def_size));
                    }

                    vert_num++;
                }

                // serialize vertex buffer
                vtx_buffer = vtx_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                // calculate add remainder padding for vertex buffer
                vtx_buffer_list.Add(new byte[calc_remainder_padding(vtx_buffer.Length)]);      
                vtx_buffer = vtx_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                #endregion


                vertex_tris_header.triangles_count = SMD_parts[i].triangles_list.Count; // + 1

                if (Main.model.header.materials_count > 0)
                {
                    vertex_tris_header.vertex_buffer_offset = vertex_blocks_offset + 32; //vtx_materials_list_bytes.Length;
                    vertex_tris_header.triangles_buffer_offset = vertex_tris_header.vertex_buffer_offset + vtx_buffer.Length;
                }
                else
                {
                    vertex_tris_header.vertex_buffer_offset = vertex_blocks_offset + vtx_materials_list_bytes.Length + 32;
                    vertex_tris_header.triangles_buffer_offset = vertex_tris_header.vertex_buffer_offset + vtx_buffer.Length;
                }

                vertex_tris_header.vertex_count = (vertex_tris_header.triangles_buffer_offset - vertex_tris_header.vertex_buffer_offset) / vertex_tris_header.vertex_def_size;

                #region build triangles buffer
                // for each triangle
                foreach (var t in SMD_parts[i].triangles_list)
                {
                    tris_buffer_list.Add(BitConverter.GetBytes(t));
                }

                // calculate add remainder padding
                //tris_buffer_list.Add(new byte[calc_remainder_padding(tris_buffer_list.Count * 2)]);
                // serialize triangles buffer
                tris_buffer = tris_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                int padding_count = calc_remainder_padding(tris_buffer.Length);

                // debug padding to match the original model 
                //if (i == 0 || i == 2 || i == 5 || i == 8 || i == 10 || i == 13 || i == 14 || i == 17 || i == 18 ) { padding_count = padding_count + 16; }

                tris_buffer_list.Add(new byte[padding_count]);
                tris_buffer = tris_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                #endregion

                #region merge buffers and add to list

                List<byte[]> vertex_tris_buffers = new List<byte[]>();

                int head_mat_offset = 0;

                if (Main.model.header.materials_count != 0)
                {
                    vertex_tris_buffers.Add(vertex_tris_header.serialize());

                    //head_mat_offset = vtx_materials_list_bytes.Length;
                    //vertex_tris_buffers.Add(vtx_materials_list_bytes);
                }
                else
                {
                    // head_mat_offset = vtx_materials_list_bytes.Length;
                    vertex_tris_buffers.Add(vertex_tris_header.serialize());
                    vertex_tris_buffers.Add(vtx_materials_list_bytes);
                }


                vertex_tris_buffers.Add(vtx_buffer);
                vertex_tris_buffers.Add(tris_buffer);
                // add to list of vtx_tris buffers
                vtx_tri_buffers_Serialized_List.Add(vertex_tris_buffers.SelectMany(byteArr => byteArr).ToArray());

                #endregion



                #region setup model part header

                Model_Part_header mdl_part_header = new Model_Part_header();
                mdl_part_header = new Model_Part_header();

                mdl_part_header.vertex_block_offset = vertex_blocks_offset; //head_mat_offset
                if (Main.model.header.materials_count > 0)
                {
                    mdl_part_header.materials_count = materials.Count;
                }
                else if (vtx_materials_count > 0)
                {
                    mdl_part_header.materials_count = vtx_materials_count;
                } else {
                    mdl_part_header.materials_count = materials.Count;
                }


                /*
                if (Main.model.header.materials_count == 0)
                {
                    mdl_part_header.vertex_block_offset = vertex_blocks_offset; //head_mat_offset
                    mdl_part_header.materials_count = vtx_materials_count;
                }
                else
                {
                    mdl_part_header.vertex_block_offset = vertex_blocks_offset;
                    mdl_part_header.materials_count = vtx_materials_count;
                }
                */

                //(mdl_parts_count * 128) + (i * 32) + vertex_blocks_offset + 16; // (if its stripped or has mat def after mdl part header)
                // TODO calculate proper corresponding triangle group offset (one model part may have multiple triangle groups, shifting other's parts offset...)

                #region triangle groups and materials count definitions

                // NOTE: these change depending on if the materials are defined after the triangle groups
                // OR if the materials are defined after the vertex_triangles_buffer_header
                // we base some of these parameters on the original model

                if (Main.model.header.materials_count == 0) //materials.Count == 0
                {


                    /*
                    if (Main.model.header.materials_count != 0)
                    {
                        mdl_part_header.materials_count = Main.model.header.materials_count;
                    } else {

                        mdl_part_header.materials_count = SMD_parts[i].mat_groups_list.Count; //vtx_materials_count;
                    }
                    */




                    // if last model part
                    if (i == mdl_parts_count - 1)
                    {
                        mdl_part_header.triangle_groups_list_offset = (mdl_parts_count * 128);
                        mdl_part_header.materials_list_offset = mdl_part_header.vertex_block_offset + 32;
                        mdl_part_header.triangle_groups_count = SMD_parts[i].mat_groups_list.Count;
                    }

                    if (Main.model.Model_Parts_header_List[i].triangle_groups_list_offset == 0 && Main.model.Model_Parts_header_List[i].triangle_groups_count == 0)
                    {
                        mdl_part_header.triangle_groups_list_offset = (mdl_parts_count * 128)+ i*32;
                        mdl_part_header.triangle_groups_count = 1;
                    }

                    // if SMD has no material/triangle groups, set group data to zero
                    if (SMD_parts[i].mat_groups_list.Count <= 1)
                    {
                        mdl_part_header.triangle_groups_list_offset = 0;
                        mdl_part_header.triangle_groups_count = 0;
                    }


                    mdl_part_header.materials_list_offset = mdl_part_header.vertex_block_offset + 32;

                    mdl_part_header.materials_count = vtx_materials_count;
                    if(vtx_materials_count == 0)
                    {
                        mdl_part_header.materials_count = materials.Count;
                    }
                }
                else
                {

                    mdl_part_header.triangle_groups_list_offset = (mdl_parts_count * 128) + (i * 32);
                    //mdl_part_header.triangle_groups_count = Main.model.Model_Parts_header_List[i].triangle_groups_count;
                    mdl_part_header.triangle_groups_count = SMD_parts[i].mat_groups_list.Count;


                    if (i < mdl_parts_count-1)
                   {
                        mdl_part_header.materials_count = 1;
                   } else
                   {
                        mdl_part_header.materials_count = materials.Count;
                   }
                      

                    mdl_part_header.materials_list_offset = (mdl_parts_count * 128) + triangle_groups_list_bytes.Length;
                }

                #endregion

                // set model part number
                mdl_part_header.model_part_number = i;

                mdl_part_header.draw_distance_x = mdl_list[i].drawDist_x;
                mdl_part_header.draw_distance_y = mdl_list[i].drawDist_y;
                mdl_part_header.draw_distance_z = mdl_list[i].drawDist_z;
                mdl_part_header.draw_distance_w = mdl_list[i].drawDist_w;



                // lower part models
                if (i < mdl_parts_count - 1)
                {
                    mdl_part_header.bone_pos = new Vector3(smd_lastPart.skeleton_nodes[i].pos.X, smd_lastPart.skeleton_nodes[i].pos.Y, smd_lastPart.skeleton_nodes[i].pos.Z);

                    // TODO import bone hierarchy from each SMD model part? (currently using bone structure from last model part)
                    mdl_part_header.bone_child_id = nodes_jsrf[i].child_id * 128; // * 128 (size of a Model_Part_header block)

                    // fix, if = 21 set to 20
                    if (nodes_jsrf[i].shared_parent_id == 21) { nodes_jsrf[i].shared_parent_id = 20; }
                    mdl_part_header.bone_parent_id = nodes_jsrf[i].shared_parent_id * 128;// * 128 (size of a Model_Part_header block)
                    mdl_part_header.model_type = 1;


                }
                else
                { // last model part
                    mdl_part_header.bone_pos = new Vector3(0, 0, 0);
                    mdl_part_header.bone_child_id = 0; // * 128 (size of a Model_Part_header block)
                    mdl_part_header.bone_parent_id = 0;// * 128 (size of a Model_Part_header block)
                }



                // if model only has one part (static mesh)
                if (mdl_parts_count == 1)
                {
                    mdl_part_header.model_type = 0;
                }

                mdl_part_header.unk_float1 = new Vector3(1f, 1f, 1f);
                mdl_part_header.unk_float2 = new Vector3(0f, 1f, 0f);

                //serialized and add Model_Part_header to serialized list
                mdl_part_headers_Serialized_List.Add(mdl_part_header.serialize());

                #endregion

                // offset position
                vertex_blocks_offset += vtx_tri_buffers_Serialized_List[vtx_tri_buffers_Serialized_List.Count - 1].Length;
            }

            #endregion



            #region assemble byte arrays into file
            // Assemble file
            // file byte array
            List<byte[]> file_bytes = new List<byte[]>();

            //include main start header (16 bytes)
            //file_bytes.Add(new header(mdl_parts_count, materials.Count).serialize());
            file_bytes.Add(new header(mdl_parts_count, materials.Count).serialize()); //Main.model.header.materials_count


            file_bytes.Add(mdl_part_headers_Serialized_List.SelectMany(byteArr => byteArr).ToArray());
            file_bytes.Add(triangle_groups_list_bytes);

            if (Main.model.header.materials_count != 0) //materials.Count
            {
                file_bytes.Add(materials_list_bytes);
            }

            file_bytes.Add(vtx_tri_buffers_Serialized_List.SelectMany(byteArr => byteArr).ToArray());

            #endregion

            // return file (as a byte array)
            return file_bytes.SelectMany(byteArr => byteArr).ToArray();
        }



        // calculates remainder padding bytes count
        // example, if triangles list = 72 bytes, we need to add 8 padding bytes so it aligns to a 16 bytes structure
        // 72/16 = 4.5 // 1- 0.5 = 0.5 //  0.5 * 8
        private int calc_remainder_padding(int byte_count)
        {
            int padding_count = (int)((1 - ((byte_count / 16f) % 1)) * 16);
            // return 0, because 16 bytes aligns with the structure, its useless padding
            if (padding_count == 16)
            {
                return 0;
            } else
            {
                return padding_count;
            }
           // return (int)((1 - ((byte_count / 16f) % 1)) * 16);
        }

        #region bone/node converstion classes

        private class shared_parent
        {
            public int parent { get; set; }
            public List<int> children { get; set; }

            public shared_parent(int _parent) //, int _child
            {
                children = new List<int>();
                this.parent = _parent;
                //this.children.Add(_child);
            }
        }

        public class node_jsrf
        {
            public int child_id { get; set; }
            public int shared_parent_id { get; set; }

            public node_jsrf(int _child_id, int _shared_parent_id)
            {
                this.child_id = _child_id;
                this.shared_parent_id = _shared_parent_id;
            }
        }

        #endregion
        #region data transfer classes

        /*
        public class model_part_triangle_groups
        {
            //public int model_part_number { get; set; }
            public List<JSRF_Tool_2.DataFormats.JSRF.MDLB.triangle_group> tri_groups { get; set; }

            public model_part_triangle_groups()
            {
                tri_groups = new List<JSRF_Tool_2.DataFormats.JSRF.MDLB.triangle_group>();
            }
        }
        */

        //public static List<vtx_buffer_materials> vtx_buff_materials;

        /// <summary>
        /// List of materials after vertex_triangles_buffer_header
        /// </summary>
        public class vtx_buffer_materials
        {
            public List<JSRF_Tool_2.DataFormats.JSRF.MDLB.material> materials { get; set; }

            public vtx_buffer_materials(List<JSRF_Tool_2.DataFormats.JSRF.MDLB.material> _mats)
            {
                materials = new List<JSRF_Tool_2.DataFormats.JSRF.MDLB.material>();
                this.materials = materials;
            }
        }
        #endregion

        #region MDLB classes

        /// <summary>
        ///  MDLB header
        /// </summary>
        private class header
        {
            public string header_name = "MDLB";
            public Int32 model_parts_count { get; set; }
            public Int32 Materials_count { get; set; }
            public Int32 unk = 0;

            public header(Int32 _mdl_parts_count, Int32 _materials_count)
            {
                this.model_parts_count = _mdl_parts_count;
                this.Materials_count = _materials_count;
            }

            public byte[] serialize()
            {
                List<byte[]> b = new List<byte[]>();
                b.Add(Encoding.ASCII.GetBytes(header_name));
                b.Add(BitConverter.GetBytes(model_parts_count));
                b.Add(BitConverter.GetBytes(Materials_count));
                b.Add(BitConverter.GetBytes(unk));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }


        /// <summary>
        /// Model part 
        /// </summary>
        /// <remarks>
        /// NDLB seem to have up to 21 different model paths.
        /// A Model_Part_header block (128 bytes) is used to define a model part.
        /// </remarks>
        private class Model_Part_header // 128 bytes
        {
            public Int32 vertex_block_offset { get; set; } // pointer // // this value + 16 = start offset of VertexBlock_header
            public Int32 materials_list_offset { get; set; } // pointer // this value + 16 = start offset of list of material_groups
            public Int32 model_part_number { get; set; }
            public Int32 model_type { get; set; } // 0 = visual mesh // 1 = shadow mesh // 2 = bone

            // 16
            public float draw_distance_x { get; set; }
            public float draw_distance_y { get; set; }
            public float draw_distance_z { get; set; }
            public float draw_distance_w { get; set; }
            public Vector3 bone_pos { get; set; } // 0x28
            public float unk_44 { get; set; }

            public Int32 unk_48 { get; set; }
            public Int32 unk_52 { get; set; }
            public Int32 unk_56 { get; set; }
            public Int32 unk_60 { get; set; }

            public Vector3 unk_float1 { get; set; }
            public Vector3 unk_float2 { get; set; }

            public UInt32 unk1a { get; set; } // 0x88
            public UInt32 unk1b { get; set; }

            public Int32 bone_child_id { get; set; } // pointer // this value + 16 = offset to parent? // divide it by 128 to get the model_part_number
            public Int32 bone_parent_id { get; set; } // pointer // this value + 16 = offset to child? // divide it by 128 to get the model_part_number
            public Int32 unk1f { get; set; }
            public Int32 materials_count { get; set; }

            public Int32 triangle_groups_list_offset { get; set; } // pointer // this value + 16 = offset of triangle groups definitions
            public Int32 triangle_groups_count { get; set; } // number of material groups for this model part
            public Int32 unk4 { get; set; } // likely unused
            public Int32 unk5 { get; set; } // likely unused

            public Model_Part_header()
            {
                bone_pos = new Vector3();
                unk_float1 = new Vector3();
                unk_float2 = new Vector3();
            }


            public byte[] serialize()
            {
                List<byte[]> b = new List<byte[]>();
                b.Add(BitConverter.GetBytes(vertex_block_offset));
                b.Add(BitConverter.GetBytes(materials_list_offset));
                b.Add(BitConverter.GetBytes(model_part_number));
                b.Add(BitConverter.GetBytes(model_type));

                b.Add(BitConverter.GetBytes(draw_distance_x)); b.Add(BitConverter.GetBytes(draw_distance_y)); b.Add(BitConverter.GetBytes(draw_distance_z));
                b.Add(BitConverter.GetBytes(draw_distance_w));
                b.Add(BitConverter.GetBytes(bone_pos.X)); b.Add(BitConverter.GetBytes(bone_pos.Y)); b.Add(BitConverter.GetBytes(bone_pos.Z));
                b.Add(BitConverter.GetBytes(unk_44));

                b.Add(BitConverter.GetBytes(unk_48));
                b.Add(BitConverter.GetBytes(unk_52));
                b.Add(BitConverter.GetBytes(unk_56));
                b.Add(BitConverter.GetBytes(unk_60));

                b.Add(BitConverter.GetBytes(unk_float1.X)); b.Add(BitConverter.GetBytes(unk_float1.Y)); b.Add(BitConverter.GetBytes(unk_float1.Z));
                b.Add(BitConverter.GetBytes(unk_float2.X)); b.Add(BitConverter.GetBytes(unk_float2.Y)); b.Add(BitConverter.GetBytes(unk_float2.Z));
                b.Add(BitConverter.GetBytes(unk1a));
                b.Add(BitConverter.GetBytes(unk1a));

                b.Add(BitConverter.GetBytes(bone_child_id));
                b.Add(BitConverter.GetBytes(bone_parent_id));
                b.Add(BitConverter.GetBytes(unk1f));
                b.Add(BitConverter.GetBytes(materials_count));

                b.Add(BitConverter.GetBytes(triangle_groups_list_offset));
                b.Add(BitConverter.GetBytes(triangle_groups_count));
                b.Add(BitConverter.GetBytes(unk4));
                b.Add(BitConverter.GetBytes(unk5));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }

        /// <summary>
        /// (32 bytes) material group, defines triangles indices and material/color (aka HB block) to apply 
        /// </summary>
        public class triangle_group
        {
            public UInt32 triangle_group_size { get; set; } // start index = (triangle_start_index /3) + (triangle_group_size -1)
            public UInt32 triangle_start_offset { get; set; } // divide this by 3
            public Int32 flag_8 { get; set; }
            public Int32 mesh_type { get; set; } // =texture id????

            public Int32 material_index { get; set; } // gives material number index (0, 1, 2...) from materials list
            public Int32 mesh_type_1 { get; set; } // if = 1 is a bone?  // if = 0 is a visual mesh?
            public Int32 flag_24 { get; set; } //
            public Int32 flag_28 { get; set; } // if 1 = bone? 

            public triangle_group(UInt32 _tri_group_size, UInt32 _tri_start_offset, Int32 _mesh_type, Int32 _mat_index)
            {
                this.triangle_group_size = _tri_group_size;
                this.triangle_start_offset = _tri_start_offset;

                this.mesh_type = _mesh_type;
                this.material_index = _mat_index;
                //this.mesh_type_1 = _mesh_type;

                this.flag_24 = -1;
                this.flag_28 = -1;
            }

            public byte[] serialize()
            {
                List<byte[]> b = new List<byte[]>();
                b.Add(BitConverter.GetBytes(triangle_group_size));
                b.Add(BitConverter.GetBytes(triangle_start_offset));
                b.Add(BitConverter.GetBytes(flag_8));
                b.Add(BitConverter.GetBytes(mesh_type));

                b.Add(BitConverter.GetBytes(material_index));
                b.Add(BitConverter.GetBytes(mesh_type_1));
                b.Add(BitConverter.GetBytes(flag_24));
                b.Add(BitConverter.GetBytes(flag_28));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }


        /// <summary>
        /// material (20 bytes)  (HB block)
        /// </summary>
        public class material // 20 bytes
        {
            public color color { get; set; } // its BGRA instead of RGBA (invert color order)
            public Int32 shader_id { get; set; } // if this = FFFFFF color_0 is used // otherwise external material or texture?
            public Int32 unk_id2 { get; set; }
            public Int32 pad12 { get; set; }
            public float HB { get; set; }

            public material(color _color, Int32 _shader_id, Int32 _unk_id2, float _HB)
            {
                this.color = _color;
                this.shader_id = _shader_id;
                this.unk_id2 = _unk_id2;
                this.HB = _HB;
            }


            public byte[] serialize()
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
        /// BGRA color
        /// </summary>
        public class color
        {
            public byte B { get; set; }
            public byte G { get; set; }
            public byte R { get; set; }
            public byte A { get; set; }

            public color(byte _B, byte _G, byte _R, byte _A)
            {
                this.B = _B;
                this.G = _G;
                this.R = _R;
                this.A = _A;
            }

            public byte[] Serialize()
            {
                byte[] buff = new byte[4];
                buff[0] = this.B;
                buff[1] = this.G;
                buff[2] = this.R;
                buff[3] = this.A;
                return buff;
            }
        }


        /// <summary>
        /// vertex and triangle buffers header
        /// </summary>
        /// <summary>
        /// Defines starting offsets and sizes of vertex/triangle buffer for a model part.
        /// </summary>
        private class Vertex_triangles_buffers_header  // 32 bytes
        {
            public Int32 vertex_buffer_offset { get; set; } // add +16 to get real position
            public Int32 vertex_count { get; set; } // 
            public Int32 unk_flag { get; set; } // unknown flag that changes depending on the vertex_def_size
            public Int32 vertex_def_size { get; set; } // size of a vertex definition

            public Int32 triangles_buffer_offset { get; set; } // add +16 to get real position
            public Int32 triangles_count { get; set; } //  divide by 3 for triangle count
            public Int32 stripped_triangles { get; set; } // if triangles are stripped this = 1
            public Int32 unk7 { get; set; } // ??

            public byte[] serialize()
            {
                List<byte[]> b = new List<byte[]>();
                b.Add(BitConverter.GetBytes(vertex_buffer_offset));
                b.Add(BitConverter.GetBytes(vertex_count));
                b.Add(BitConverter.GetBytes(unk_flag));
                b.Add(BitConverter.GetBytes(vertex_def_size));

                b.Add(BitConverter.GetBytes(triangles_buffer_offset));
                b.Add(BitConverter.GetBytes(triangles_count));
                b.Add(BitConverter.GetBytes(stripped_triangles));
                b.Add(BitConverter.GetBytes(unk7));

                return b.SelectMany(byteArr => byteArr).ToArray();
            }
        }

        /// <summary>
        /// vertex definition structure, varies in length depending on how many properties are defined
        /// the Serialize class will write a vertex defnition according to the input _size
        /// </summary>
        private class vertex_def
        {
            public Vector3 pos { get; set; }
            public Vector2 uv { get; set; }
            public Vector3 norm { get; set; }
            public float bone_weight { get; set; }
            public sbyte bone_id_1 { get; set; }
            public sbyte bone_id_2 { get; set; }

            public vertex_def(Vector3 _pos, Vector2 _uv, Vector3 _norm, float _bw, sbyte _bid_1, sbyte _bid_2)
            {
                this.pos = _pos;
                this.uv = _uv;
                this.norm = _norm;
                this.bone_weight = _bw;
                this.bone_id_1 = _bid_1;
                this.bone_id_2 = _bid_2;
            }

            public byte[] Serialize(int _size)
            {
                List<byte[]> b = new List<byte[]>();
                // vertex position
                b.Add(BitConverter.GetBytes(pos.X)); b.Add(BitConverter.GetBytes(pos.Y)); b.Add(BitConverter.GetBytes(pos.Z));

                switch (_size)
                {

                    case 20:
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y)); // offset 12
                        break;
                    case 24:
                        b.Add(BitConverter.GetBytes(norm.X)); b.Add(BitConverter.GetBytes(norm.Y)); b.Add(BitConverter.GetBytes(norm.Z));
                        //b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y));  // offset 12
                        //b.Add(BitConverter.GetBytes(0f));
                        break;
                    case 28:
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y));  // offset 12
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

                    case 40:
                        b.Add(BitConverter.GetBytes(bone_weight)); //bone_weights_offset = 12; // ok
                        b.Add(new byte[] { (byte)bone_id_1, (byte)bone_id_2, 0x00, 0x00 }); //b.Add(new byte[1]); //bone_ids_offset = 16; // ok
                        b.Add(BitConverter.GetBytes(norm.X)); b.Add(BitConverter.GetBytes(norm.Y)); b.Add(BitConverter.GetBytes(norm.Z)); //norm_offset = 20; // ok
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y)); //uv_offset = 32; //ok
                        break;

                    case 44:
                        b.Add(BitConverter.GetBytes(bone_weight)); //bone_weights_offset = 28;
                        b.Add(new byte[] { (byte)bone_id_1, (byte)bone_id_2, 0x00, 0x00 }); //b.Add(new byte[1]); // bone_ids_offset = 32;
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y)); // 0x28
                        b.Add(new byte[16]);
                        //norm_offset = -1;
                        break;

                    case 48:

                        b.Add(BitConverter.GetBytes(bone_weight)); //bone_weights_offset = 12; // ok
                        b.Add(new byte[] { (byte)bone_id_1, (byte)bone_id_2, 0x00, 0x00 }); //b.Add(new byte[1]); //bone_ids_offset = 16; // ok
                        b.Add(BitConverter.GetBytes(norm.X)); b.Add(BitConverter.GetBytes(norm.Y)); b.Add(BitConverter.GetBytes(norm.Z)); //norm_offset = 20; // ok
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y)); //uv_offset = 32; //ok
                        b.Add(new byte[8]);

                        // bone weights offset 40

                        break;

                    case 56:
                        b.Add(BitConverter.GetBytes(bone_weight)); // 0x12 +4 bytes
                        b.Add(new byte[] { (byte)bone_id_1, (byte)bone_id_2, 0x00, 0x00 }); // 0x16 +4 bytes
                        b.Add(BitConverter.GetBytes(norm.X)); b.Add(BitConverter.GetBytes(norm.Y)); b.Add(BitConverter.GetBytes(norm.Z)); // 0x20 + 12 bytes
                        b.Add(new byte[8]); //0x32
                        b.Add(BitConverter.GetBytes(uv.X)); b.Add(BitConverter.GetBytes(uv.Y)); // 0x40  + 8 bytes
                        b.Add(new byte[8]); //0x48
                        break;
                }

                return b.SelectMany(byteArr => byteArr).ToArray();
            }


        }





        #endregion
    }
}
