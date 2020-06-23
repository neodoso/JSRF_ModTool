using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSRF_ModTool.Vector;

namespace JSRF_ModTool.DataFormats.JSRF
{
    /// <summary>
    ///  JSRF MDLB model format definition
    /// </summary>
    /// 
    /// <remarks>
    /// MDLB are composed of mesh parts defined by Model_Part_header
    /// The number of headers varies depending on the number of mesh parts.
    /// There's a series of different header types at the start of the file, for each model part.
    /// 
    /// After the headers theres a block of Vertex data (vertex position, normal, UV and also probably skinning/weights for the bones)
    /// After the vertex block, there's the triangles list block.
    /// 
    /// There are several types of vertex definitions, a vertex definition may contain: position normal and UV
    /// Other vertex type can have, position normal uv and vertex weight etc
    /// So the different vertex types vary in size.
    /// 
    /// The headers of model parts point to parts of the Vertex and Triangle blocks from which we get a mesh part.
    /// 
    /// Model can also have material clusters, if the model has multiple materials 
    /// to define how the materials are assigned to the mesh's triangles
    /// 
    /// random info
    /// some models of the player model from part 0 to 20 are lowpoly meshes used for shadow casting of the player
    /// part 21 actually is the visual mesh
    /// in other cases parts 0 to 20 only have a cube in them as mesh, probably these model parts are the bones of the rig
    ///
    /// in other models all parts are actual meshes
    /// TODO: figure out bones and vertex skinning/weights
    /// 
    /// TODO: refactor Main.load_block_MDLB() as voids in the MDLB class
    /// also create a class for MDLB triangle and vertex defitions, right now this is in load_block_MDLB()
    /// </remarks>

    public class MDLB 
    { 
       public MDLB_header header = new MDLB_header();
        public List<Model_Part_header> Model_Parts_header_List = new List<Model_Part_header>(); // possible mat clusters

        public List<material> materials_List = new List<material>();
        public List<Vertex_triangles_buffers_header> VertexBlock_header_List = new List<Vertex_triangles_buffers_header>();

        // list of materials after the Vertex_triangles_buffers_header
       // public List<List<material>> vtx_tri_buff_materials = new List<List<material>>();


        /// <summary>
        ///  Read and load model byte array intro MDLB headers/data 
        /// </summary>
        public MDLB(byte[] data)
        {
            // read and fill data
            header = (MDLB_header)(Parsing.binary_to_struct(data, 0, typeof(MDLB_header)));

            #region Load Model parts

            // load model parts headers
            for (int i = 0; i < header.model_parts_count ; i++)
            {
                Model_Part_header mdl_part_header = (Model_Part_header)(Parsing.binary_to_struct(data, 16 + (128 * i), typeof(Model_Part_header)));

                // recalculate id multiplied by the size of a Model_Part_header
                mdl_part_header.bone_child_id = mdl_part_header.bone_child_id / 128; //+ 16
                mdl_part_header.bone_parent_id = mdl_part_header.bone_parent_id / 128; //+ 16

                Model_Parts_header_List.Add(mdl_part_header);
                
                // load and store material cluster for this model part
                // for each "ModelPart_header_List[i].Materials_Cluster_count" add a Material_cluster (32 bytes) to ModelPart_header_List[i].Materials_Cluster_List
                for (int c = 0; c < Model_Parts_header_List[i].triangle_groups_count; c++)
                {
                    Model_Parts_header_List[i].triangle_groups_List.Add((triangle_group)(Parsing.binary_to_struct(data, Model_Parts_header_List[i].triangle_groups_list_offset + 16 + (c * 32), typeof(triangle_group))));
                }

                // calculate triangle group start/end triangles
                for (int g = 0; g < Model_Parts_header_List[i].triangle_groups_List.Count; g++)
                {
                    Model_Parts_header_List[i].triangle_groups_List[g].triangle_start_index = Model_Parts_header_List[i].triangle_groups_List[g].triangle_start_index / 3;
                    //Model_Part_header_List[i].triangle_groups_List[g].triangle_group_size -=1;      
                }     
            }
            #endregion

            #region calculate bone hierarchy

            List<Model_Part_header> mp = Model_Parts_header_List;
            // reset real_parent_id to 0 (since the reflection binary_to_struct read the bytes wrongly)
            // TODO create a property for properties such as "real_parent_id" to not be taken into account by binary_to_struct()
            for (int i = 0; i < Model_Parts_header_List.Count; i++)
            {
                Model_Parts_header_List[i].real_parent_id = 0;
            }
            
            // calculate hirearchy
            for (int i = 0; i < Model_Parts_header_List.Count; i++)
            {

                // if bone_parent_id = last part, parent is null
                if (mp[i].bone_parent_id == Model_Parts_header_List.Count-1)
                {
                    Model_Parts_header_List[i].real_parent_id = -1;
                    continue;
                }

                // get parent id
                if (mp[i].bone_child_id == i + 1)
                {
                    Model_Parts_header_List[i + 1].real_parent_id = i;
                    if(mp[i].real_parent_id != 0)
                    continue;
                }

                
                if(i <= 0)
                {
                    continue;
                }
                

                // if shares parent with other bones
                if (mp[i].real_parent_id == 0 && mp[i-1].bone_parent_id == 0 && mp[i-1].bone_child_id == 0)
                {
                    // loop back until we find a model part with a root_id !=0
                    int p = i - 1;

                    while (p > -1)
                    {
                        // if bone_parent_id of previous bone is the same as i
                        if (mp[p].bone_parent_id == i)
                        {
                            // set same parent (they share the same parent)
                            Model_Parts_header_List[i].real_parent_id = Model_Parts_header_List[p].real_parent_id;
                            break;
                        }

                        p--;
                    }
                    continue;
                }
 
            }

            #endregion

            if(Model_Parts_header_List.Count == 0)
            {
                System.Windows.MessageBox.Show("Error: MDLB has 0 model parts.");
                return;
            }

            // if MDLB header specific head.materials_count > 0
            // get materials list
            for (int m = 0; m < header.materials_count; m++)
            {
                materials_List.Add((material)(Parsing.binary_to_struct(data, (Model_Parts_header_List[0].materials_list_offset + 16) + (m * 20), typeof(material))));
            }
           
            for (int v = 0; v < Model_Parts_header_List.Count; v++)
            {
                // get vertexblock header
                VertexBlock_header_List.Add((Vertex_triangles_buffers_header)(Parsing.binary_to_struct(data, Model_Parts_header_List[v].vertex_block_offset + 16, typeof(Vertex_triangles_buffers_header))));

                // if there is no materials list (after the triangles groups list)
                // search materials after the vertex/triangle header
                if(header.materials_count == 0)
                {
                    // get position after vertex/tris buffers header
                    int mats_start = Model_Parts_header_List[v].vertex_block_offset + 48;
                    Model_Parts_header_List[v].vtx_buffer_materials = new List<MDLB.material>();

                    // for each material
                    for (int m = 0; m < Model_Parts_header_List[v].materials_count; m++)
                    {
                        Model_Parts_header_List[v].vtx_buffer_materials.Add((MDLB.material)(Parsing.binary_to_struct(data, (mats_start) + (m * 20), typeof(MDLB.material))));
                    }


                }

            }
        }

        #region MDLB classes


        /// <summary>
        ///  MDLB header
        /// </summary>
        public class MDLB_header
        {
            private string header_name = new string(new Char[4], 0, 4);
            public string Header_name
            {
                get { return header_name; } // define string length 4 // new string(new Char[4], 0, 4);
                set { header_name = value; }
            }
            // public Int32 header_name { get; set; }
            public Int32 model_parts_count { get; set; } // HB blocks
            public Int32 materials_count { get; set; }
            public Int32 unk { get; set; }
        }


        /// <summary>
        /// Model part 
        /// </summary>
        /// <remarks>
        /// NDLB seem to have up to 21 different model paths.
        /// A Model_Part_header block (128 bytes) is used to define a model part.
        /// </remarks>
        public class Model_Part_header // 128 bytes
        {
            public Int32 vertex_block_offset { get; set; }  // pointer // // this value + 16 = start offset of VertexBlock_header
            public Int32 materials_list_offset { get; set; } // pointer // this value + 16 = start offset of list of material_groups
            public Int32 model_part_number { get; set; }
            public Int32 model_type { get; set; } // 0 = visual mesh // 1 = shadow mesh // 2 = bone // Int32.Max = shadow model

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



            // this isn't within the 128 bytes block
            // materials clusters are listed after the model parts headers
            //[NonSerialized]
            public Int32 real_parent_id { get; set; }
            public List<triangle_group> triangle_groups_List = new List<triangle_group>();

            // only used if  MDLB_header.materials_count = 0
            public List<material> vtx_buffer_materials = new List<material>();
        }

        /// <summary>
        /// (32 bytes) material group, defines triangles indices and material/color (aka HB block) to apply 
        /// </summary>
        public class triangle_group
        {
            public Int32 triangle_group_size { get; set; } // start index = (triangle_start_index /3) + (triangle_group_size -1)
            public Int32 triangle_start_index { get; set; } // divide this by 3
            public Int32 flag_8 { get; set; }
            public Int32 mesh_type { get; set; } // =texture id????

            public Int32 material_index { get; set; } // gives material number from materials list
            public Int32 mesh_type_1 { get; set; } // if = 1 is a bone?  // if = 0 is a visual mesh?
            public Int32 flag_24 { get; set; } //
            public Int32 flag_28 { get; set; } // if 1 = bone?
        }

        /// <summary>
        /// material (20 bytes)  (HB block)
        /// </summary>
        public class material // 20 bytes (HB block)
        {
            public color color { get; set; } // its BGRA instead of RGBA (invert color order)
            public Int32 shader_id { get; set; } // if this = FFFFFF color_0 is used // otherwise external material or texture?
            public Int32 unk_id2 { get; set; }
            public Int32 pad12 { get; set; }
            public float HB { get; set; }

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
        public class Vertex_triangles_buffers_header  // 32 bytes
        {
            public Int32 vertex_buffer_offset { get; set; } // add +16 to get real position
            public Int32 vertex_count { get; set; } // 
            public Int32 unk_flag { get; set; } // unknown flag that changes depending on the vertex_def_size
            public Int32 vertex_def_size { get; set; } // size of a vertex definition

            public Int32 triangles_buffer_offset { get; set; } // add +16 to get real position
            public Int32 triangles_count { get; set; } //  divide by 3 for triangle count
            public Int32 stripped_triangles { get; set; } // if triangles are stripped this = 1
            public Int32 unk7 { get; set; } // ?? 


        }

        #endregion

    }
}