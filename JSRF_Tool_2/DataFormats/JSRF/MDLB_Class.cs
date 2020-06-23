using System;
using System.Collections.Generic;
using System.Linq;
using JSRF_Tool_2.Vector;

namespace JSRF_ModTool_2.DataFormats.JSRF
{
    public partial class MDLB
    {
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
