using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSRF_ModTool.Vector;

namespace JSRF_ModTool.MDLB_Import
{
    public class MDLB_classes
    {
        #region MDLB classes

        /// <summary>
        ///  MDLB header
        /// </summary>
        public class header
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
        public class Model_Part_header // 128 bytes
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
        public class vertex_def
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
