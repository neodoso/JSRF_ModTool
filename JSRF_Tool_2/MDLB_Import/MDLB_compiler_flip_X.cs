﻿using System;
using System.Collections.Generic;
using System.Linq;
using JSRF_ModTool.Vector;
using System.IO;
using JSRF_ModTool.Functions;

namespace JSRF_ModTool
{
    class MDLB_compiler_flip_X
    {
        // build JSRF MDLB model
        public byte[] build(List<MDLB_Import.ModelPart_Import_Settings> mdl_list, List<MDLB_Import.MDLB_classes.material> materials, int src_mat_count) //, vertex_type vert_type
        {

            bool flip_on_X = false;

            #region import each SMD

            // create array for each SMD
            SMD[] SMD_parts = new SMD[mdl_list.Count];
            int mdl_parts_count = mdl_list.Count;
            SMD smd_lastPart = SMD_parts[mdl_list.Count - 1];

   

            #region import lower model parts SMDs

            List<string> smd_not_found = new List<string>();

            // import model parts if SMD file exists
            for (int i = 0; i < mdl_parts_count; i++)
            {
                if (File.Exists(mdl_list[i].filepath))
                {
                    SMD_parts[i] = new SMD(mdl_list[i].filepath);
                }
                else
                {
                   System.Windows.MessageBox.Show("MDLB_builder Error: could not find SMD model part " + i + "\n\nExpecting file: " + mdl_list[i].filepath);
                    return new byte[0];
                }
            }

            #endregion

       

            #endregion

            #region check SMD for incompatiblity or errors

            // if imported (main/last) SMD does not have the same bone count as the original model
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

            

            #region flip skeleton along X axis ( X * -1 )

            if(flip_on_X)
            {
                // flip skeleton nodes on X axis
                for (int i = 0; i < SMD_parts.Length; i++)
                {
                    for (int j = 0; j < SMD_parts[i].skeleton_nodes.Count; j++)
                    {
                        SMD.skeleton_node nd = SMD_parts[i].skeleton_nodes[j];
                        nd.pos = new Vector3(nd.pos.X * -1, nd.pos.Y, nd.pos.Z);
                        SMD_parts[i].skeleton_nodes[j] = nd;
                    }
                }
            }
            #endregion


            smd_lastPart = SMD_parts[SMD_parts.Length - 1];


            #region convert nodes to JSRF format and import lower SMD model parts

            // rigged models (multiple model parts, has bones/rigging)

            #region convert nodes to JSRF format 

            List<node_jsrf> nodes_jsrf = new List<node_jsrf>();

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

            #region recalculate bone positions relative to parents


            for (int i = 0; i < smd_lastPart.skeleton_nodes.Count; i++)
            {
                // recalculate bone position (substract bone.pos by parent_bone.pos)
                if (i > 0)
                {
                    Vector3 pos = smd_lastPart.skeleton_nodes[i].pos;
                    Vector3 parent_pos = new Vector3();

                    if (i >= 0)
                    {
                        if (smd_lastPart.nodes[i].parent_ID != -1)
                        {
                            //vpp = smd_data.skeleton_nodes[nodes_jsrf[i].shared_parent_id].pos;
                            parent_pos = smd_lastPart.skeleton_nodes[smd_lastPart.nodes[i].parent_ID].pos;
                        }
                    }

                    // substract parent position 
                    smd_lastPart.skeleton_nodes[i].pos = new Vector3(pos.X + parent_pos.X, pos.Y + parent_pos.Y, pos.Z + parent_pos.Z);
                }
            }

            #endregion

            #endregion

            

            #region flip mesh along X axis

            if (flip_on_X)
            {
                // for all model parts
                for (int i = 0; i < SMD_parts.Length; i++)
                {
                    // flip vertex positions and normals on x axis
                    for (int v = 0; v < SMD_parts[i].vertices_list.Count; v++)
                    {
                        SMD.vertex vt = SMD_parts[i].vertices_list[v];
                        vt.pos = new Vector3(vt.pos.X * -1, vt.pos.Y, vt.pos.Z);
                        vt.norm = new Vector3(vt.norm.X * -1, vt.norm.Y, vt.norm.Z);

                        SMD_parts[i].vertices_list[v] = vt;
                    }
                }

                /*
                // for last part model
                // flip vertex positions and normals on x axis
                for (int v = 0; v < smd_lastPart.vertices_list.Count; v++)
                {
                    SMD.vertex vt = smd_lastPart.vertices_list[v];
                    vt.pos = new Vector3(vt.pos.X * -1, vt.pos.Y, vt.pos.Z);
                    vt.norm = new Vector3(vt.norm.X * -1, vt.norm.Y, vt.norm.Z);

                    smd_lastPart.vertices_list[v] = vt;
                }
                */
            }


            #endregion


            smd_lastPart.vertices_list = SMD_parts[SMD_parts.Length - 1].vertices_list;


            if (mdl_list.Count == 1) { SMD_parts[0] = smd_lastPart; }


            #region triangle groups list

            // list to store generated triangle_group definition byte arrays
            List<byte[]> bytes_triangle_groups_list = new List<byte[]>();
            MDLB_Import.MDLB_classes.triangle_group tg;


            /*
            bool prev_mdl_parts_have_material = false;
            // check if lower model part have a material
            if (Main.current_model.Model_Parts_header_List.Count > 1)
            {
                for (int p = 0; p < Main.current_model.Model_Parts_header_List.Count - 2; p++)
                {
                    if (Main.current_model.Model_Parts_header_List[p].triangle_groups_count == 0)
                    {
                        prev_mdl_parts_have_material = true;
                        break;
                    }
                }
            }
            */

            // for each model part
            for (int i = 0; i < mdl_parts_count; i++)
            {

                if (SMD_parts[i].mat_groups_list.Count == 0 && i == SMD_parts.Length -1)
                {
                    System.Windows.Forms.MessageBox.Show("Error importing model, the SMD or model have no material groups.");

                    return null;
                }

                // do not write material group for model_type = 1
                if(Main.current_model.Model_Parts_header_List[i].model_type == 1 && Main.current_model.Model_Parts_header_List[i].materials_count == 1)
                {
                    continue;
                }

                // for each material group from the imported SMD file
                for (int m = 0; m < SMD_parts[i].mat_groups_list.Count; m++)
                {
                    // if original's Model_Parts_header_List[i] has no mat/tri group defined 
                    // skip
                    int mesh_type = 1;
                    int mat_num = 0;

                    if (SMD_parts[i].mat_groups_list[m].material_name.Contains("mat_"))
                    {
                        mat_num = Convert.ToInt32(SMD_parts[i].mat_groups_list[m].material_name.Replace("mat_", ""));
                    }

                    // if its a visual mesh  set mesh_type to 0
                    if (i == mdl_parts_count - 1)
                    {
                        /*
                        // if it's a head model the material index must be increased by one
                        if (prev_mdl_parts_have_material)
                        {
                           // mat_num++;
                        }
                        */
                        mesh_type = 0;
                    }


                    // TODO : determine if mat_index in triangle_group(_mat_index) needs to be shifted of +1 or not
                    // instance triangle group with SMD data and (mesh_type, material_index) default values
                    tg = new MDLB_Import.MDLB_classes.triangle_group((SMD_parts[i].mat_groups_list[m].triangle_count / 3) +1, SMD_parts[i].mat_groups_list[m].triangle_start_index, mesh_type, mat_num);
                        
                    // serialize and store in list
                    bytes_triangle_groups_list.Add(tg.serialize());
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
            if (Main.current_model.header.materials_count > 0)
            {
                // add serialized material to list of byte arrays
                bytes_materials_list.Add(new byte[Parsing.calc_remainder_padding(bytes_materials_list.Count * 20)]);
                // padding
                //bytes_materials_list.Add(new byte[16]);
                materials_list_bytes = bytes_materials_list.SelectMany(byteArr => byteArr).ToArray();
            }


            #endregion

            #region Build Model_Part_header(s), Vertex_triangles_buffers_header and vertex/triangle buffers

            #region calculate "verts_tris_buffers_start_offset"

            int model_parts_headers_total_size = mdl_parts_count * 128;

            int verts_tris_buffers_start_offset = 0;
            // calculate start offset for vertex/triangles buffers
            if (Main.current_model.header.materials_count > 0) //(Main.model.header.materials_count > 0)
            {
                verts_tris_buffers_start_offset = (mdl_parts_count * 128) + triangle_groups_list_bytes.Length + materials_list_bytes.Length;
            }
            else
            {
                verts_tris_buffers_start_offset = (mdl_parts_count * 128) + triangle_groups_list_bytes.Length;
            }
            #endregion

            int vertex_blocks_offset = verts_tris_buffers_start_offset;

            List<byte[]> mdl_part_headers_Serialized_List, vtx_tri_buffers_Serialized_List, vtx_buffer_list, tris_buffer_list;
            vtx_tri_buffers_Serialized_List = new List<byte[]>();
            mdl_part_headers_Serialized_List = new List<byte[]>();
            byte[] vtx_buffer, tris_buffer;

            MDLB_Import.MDLB_classes.Vertex_triangles_buffers_header vertex_tris_header = new MDLB_Import.MDLB_classes.Vertex_triangles_buffers_header();
            //int vert_def_size;

            // for each model part
            for (int i = 0; i < mdl_parts_count; i++)
            {

                vertex_tris_header = new MDLB_Import.MDLB_classes.Vertex_triangles_buffers_header();
                vtx_buffer_list = new List<byte[]>(); tris_buffer_list = new List<byte[]>();
                List<byte[]> materials_buffer_list = new List<byte[]>();
                byte[] vtx_materials_list_bytes = new byte[0];
                int vtx_materials_count = 0;

                #region build vertex tris buffer materials list

                // if from original model, materials = 0, it means materials are stored after the vertex_tris_header, (meaning there is no materials table after the triangle groups)
                if (Main.current_model.header.materials_count == 0)
                {
                    // lower part
                    if (i < mdl_parts_count - 1 && Main.current_model.header.materials_count == 0) //materials.Count > 0
                    {
                        //changed nor sure if this works
                        vtx_materials_count++;
                        materials_buffer_list.Add(materials[0].serialize());
                        materials_buffer_list.Add(BitConverter.GetBytes(SMD_parts[i].triangles_list.Count));
                        byte[] tmp_buff = materials_buffer_list.SelectMany(byteArr => byteArr).ToArray();
                        byte[] tmp_padding = new byte[Parsing.calc_remainder_padding(tmp_buff.Length)];
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
                        byte[] padding = new byte[Parsing.calc_remainder_padding(tmp_materials_list_bytes.Length)];
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
                        }
                        else
                        {
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
                        vtx_buffer_list.Add((new MDLB_Import.MDLB_classes.vertex_def(v.pos, v.uv, v.norm, 1, 0, 0)).Serialize(mdl_list[i].vertex_def_size));
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

                        vtx_buffer_list.Add((new MDLB_Import.MDLB_classes.vertex_def(v.pos, v.uv, v.norm, v.bone_weights[0].weight, (sbyte)v.bone_weights[0].bone_ID, (sbyte)bid_1)).Serialize(mdl_list[i].vertex_def_size));
                    }

                    vert_num++;
                }

                // serialize vertex buffer
                vtx_buffer = vtx_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                // calculate add remainder padding for vertex buffer
                vtx_buffer_list.Add(new byte[Parsing.calc_remainder_padding(vtx_buffer.Length)]);      
                vtx_buffer = vtx_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                #endregion


                vertex_tris_header.triangles_count = SMD_parts[i].triangles_list.Count; // + 1

                // if model part has multiple materials
                if (Main.current_model.header.materials_count > 0)
                {
                    vertex_tris_header.vertex_buffer_offset = vertex_blocks_offset + 32; //vtx_materials_list_bytes.Length;
                    vertex_tris_header.triangles_buffer_offset = vertex_tris_header.vertex_buffer_offset + vtx_buffer.Length;
                }
                else // if model part has only one material
                {
                    vertex_tris_header.vertex_buffer_offset = vertex_blocks_offset + vtx_materials_list_bytes.Length + 32;
                    vertex_tris_header.triangles_buffer_offset = vertex_tris_header.vertex_buffer_offset + vtx_buffer.Length;
                }

                vertex_tris_header.vertex_count = (vertex_tris_header.triangles_buffer_offset - vertex_tris_header.vertex_buffer_offset) / vertex_tris_header.vertex_def_size;

                #region build triangles buffer


                
                if (flip_on_X)
                {
                    // (flip triangles for axis flip: X * -1) create triangle buffer list, while flipping the triangle winding order
                    for (int t = 0; t < SMD_parts[i].triangles_list.Count; t += 3)
                    {
                        tris_buffer_list.Add(BitConverter.GetBytes(SMD_parts[i].triangles_list[t + 2]));
                        tris_buffer_list.Add(BitConverter.GetBytes(SMD_parts[i].triangles_list[t + 1]));
                        tris_buffer_list.Add(BitConverter.GetBytes(SMD_parts[i].triangles_list[t]));
                    }
                } else {
                    // for triangle vertex index
                    foreach (var t in SMD_parts[i].triangles_list)
                    {
                        tris_buffer_list.Add(BitConverter.GetBytes(t));
                    }
                }
                



                // calculate add remainder padding
                //tris_buffer_list.Add(new byte[calc_remainder_padding(tris_buffer_list.Count * 2)]);
                // serialize triangles buffer
                tris_buffer = tris_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                // calculate "tris_buffer" remainder padding and add to the "tris_buffer_list" byte arrays list
                tris_buffer_list.Add(new byte[Parsing.calc_remainder_padding(tris_buffer.Length)]);
                tris_buffer = tris_buffer_list.SelectMany(byteArr => byteArr).ToArray();

                #endregion

                #region merge buffers and add to list

                List<byte[]> vertex_tris_buffers = new List<byte[]>();


                if (Main.current_model.header.materials_count != 0)
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

                MDLB_Import.MDLB_classes.Model_Part_header mdl_part_header = new MDLB_Import.MDLB_classes.Model_Part_header();
                mdl_part_header = new MDLB_Import.MDLB_classes.Model_Part_header();

                #region set unknown parameters according the Model_Part_header_List[i]

                DataFormats.JSRF.MDLB.Model_Part_header mdl_part_ori = Main.current_model.Model_Parts_header_List[i];
                mdl_part_header.unk1a = mdl_part_ori.unk1a;
                mdl_part_header.unk1b = mdl_part_ori.unk1b;
                mdl_part_header.unk1f = mdl_part_ori.unk1f;
                mdl_part_header.unk4 = mdl_part_ori.unk4;
                mdl_part_header.unk5 = mdl_part_ori.unk5;
                mdl_part_header.unk_44 = mdl_part_ori.unk_44;
                mdl_part_header.unk_48 = mdl_part_ori.unk_48;
                mdl_part_header.unk_52 = mdl_part_ori.unk_52;
                mdl_part_header.unk_56 = mdl_part_ori.unk_56;
                mdl_part_header.unk_60 = mdl_part_ori.unk_60;
                mdl_part_header.unk_float = mdl_part_ori.unk_float;
                mdl_part_header.unk_float1 = mdl_part_ori.unk_float1;
                mdl_part_header.unk_float2 = mdl_part_ori.unk_float2;
                mdl_part_header.unk_float_1 = mdl_part_ori.unk_float_1;
                mdl_part_header.unk_float_2 = mdl_part_ori.unk_float_2;

                #endregion


                mdl_part_header.vertex_block_offset = vertex_blocks_offset; //head_mat_offset
                if (Main.current_model.header.materials_count > 0)
                {
                    mdl_part_header.materials_count = materials.Count;
                }
                else if (vtx_materials_count > 0)
                {
                    mdl_part_header.materials_count = vtx_materials_count;
                } else {
                    mdl_part_header.materials_count = materials.Count;
                }


                #region triangle groups and materials count definitions

                // NOTE: these change depending on if the materials are defined after the triangle groups
                // OR if the materials are defined after the vertex_triangles_buffer_header
                // we base some of these parameters on the original model

                if (Main.current_model.header.materials_count == 0) //materials.Count == 0
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

                    if (Main.current_model.Model_Parts_header_List[i].triangle_groups_list_offset == 0 && Main.current_model.Model_Parts_header_List[i].triangle_groups_count == 0)
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

                    /*
                   if (i < mdl_parts_count-1)
                   {
                        mdl_part_header.materials_count = 1;
                   } 
                   else
                   {
                        mdl_part_header.materials_count = materials.Count;
                   }
                   */

                    mdl_part_header.materials_list_offset = (mdl_parts_count * 128) + triangle_groups_list_bytes.Length;
                }

                #endregion

                // set model part number
                mdl_part_header.model_part_number = i;
                // draw distance
                mdl_part_header.draw_distance = mdl_list[i].draw_distance;
           

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

                //mdl_part_header.unk_float1 = new Vector3(1f, 1f, 1f);
                //mdl_part_header.unk_float2 = new Vector3(0f, 1f, 0f);


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


            int mat_count = materials.Count;
            if (src_mat_count == 0)
            {
                mat_count = 0;
            }
        
            file_bytes.Add(new MDLB_Import.MDLB_classes.header(mdl_parts_count, mat_count).serialize()); //new MDLB_Import.MDLB_classes.header(mdl_parts_count, materials.Count).serialize()


            file_bytes.Add(mdl_part_headers_Serialized_List.SelectMany(byteArr => byteArr).ToArray());
            file_bytes.Add(triangle_groups_list_bytes);

            if (Main.current_model.header.materials_count != 0) //materials.Count
            {
                file_bytes.Add(materials_list_bytes);
            }

            file_bytes.Add(vtx_tri_buffers_Serialized_List.SelectMany(byteArr => byteArr).ToArray());

            #endregion

            // return file (as a byte array)
            return file_bytes.SelectMany(byteArr => byteArr).ToArray();
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
            public List<JSRF_ModTool.DataFormats.JSRF.MDLB.triangle_group> tri_groups { get; set; }

            public model_part_triangle_groups()
            {
                tri_groups = new List<JSRF_ModTool.DataFormats.JSRF.MDLB.triangle_group>();
            }
        }
        */

        //public static List<vtx_buffer_materials> vtx_buff_materials;

        /// <summary>
        /// List of materials after vertex_triangles_buffer_header
        /// </summary>
        public class vtx_buffer_materials
        {
            public List<JSRF_ModTool.DataFormats.JSRF.MDLB.material> materials { get; set; }

            public vtx_buffer_materials(List<JSRF_ModTool.DataFormats.JSRF.MDLB.material> _mats)
            {
                materials = new List<JSRF_ModTool.DataFormats.JSRF.MDLB.material>();
                this.materials = materials;
            }
        }
        #endregion


    }
}
