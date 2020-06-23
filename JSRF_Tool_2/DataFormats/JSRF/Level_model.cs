using System;
using System.Collections.Generic;
using System.Linq;

namespace JSRF_ModTool.DataFormats.JSRF
{
    public class Level_Model
    {
        MDLBL_header header;
        header_second header_sec;
        List<Int32> texture_ids;
        List<material> materials;
        List<triangle_group> triangle_groups;
        vertex_buffer_header vtx_head;

        /// <summary>
        /// Load level model
        /// This is a work in progress, the level model headers can vary in size
        // TODO: figure out why there sometimes is +12 bytes of data vefire triangle_group list
        // TODO: find if there is a flag in the headers for when there is +12 bytes (couldnt find any flag so far)
        /// TODO: it seems the vertex buffer starts with series of vector3 and the real mesh points start a bit after
        /// </summary>
        /// <param name="data">Input data</param>
        public Level_Model(byte[] data)
        {
            Int32 offset;
            texture_ids = new List<Int32>();
            materials = new List<material>();
            triangle_groups = new List<triangle_group>();
            header = (MDLBL_header)(Parsing.binary_to_struct(data, 0, typeof(MDLBL_header)));

            // get list of textures IDs
            for (int i = 20; i < 20 + header.texture_ids_count * 4; i+=4)
            {
                texture_ids.Add(BitConverter.ToInt32(data, i));
            }

            offset = 20 + header.texture_ids_count * 4;

            // get second header part
            header_sec = (header_second)(Parsing.binary_to_struct(data, offset, typeof(header_second)));
            offset += 144;

            // get list of materials
            for (int i = offset; i < offset + header_sec.x124_mat_count * 20; i += 20)
            {
                materials.Add((material)(Parsing.binary_to_struct(data, i, typeof(material))));
            }

            offset += header_sec.x124_mat_count * 20;
            // vertex buffer header
            vtx_head = (vertex_buffer_header)(Parsing.binary_to_struct(data, offset, typeof(vertex_buffer_header)));
            offset += 20;

            #region determine if model has extra 12 bytes of data
            /*
            // in some MDLBL, there's some extra data after the vertex_buffer_header
            // there doesn't seem to be any flags indicating that there +12 bytes of data
            // we need to take this into account, its 3 integers
            Int32 test_A = BitConverter.ToInt32(data, offset);
            Int32 test_B = BitConverter.ToInt32(data, offset + 4);
            Int32 test_C = BitConverter.ToInt32(data, offset + 8);
            Int32 test_D = BitConverter.ToInt32(data, offset + 16) + BitConverter.ToInt32(data, offset + 20) + BitConverter.ToInt32(data, offset + 24) + BitConverter.ToInt32(data, offset + 28);
            if (test_A > 0 && test_B == 0 || test_B == 1 && test_C == 0)
            {
                if(test_D == 0)
                offset += 12;
            }
            */
            #endregion

            for (int i = 0; i < header_sec.x132_tri_groups_count * 32; i +=32)
            {
                triangle_groups.Add((triangle_group)(Parsing.binary_to_struct(data, offset +i, typeof(triangle_group))));
            }

        }

        #region classes


        /// <summary>
        ///  MDLBL header
        /// </summary>
        public class MDLBL_header
        {
            public Int32 unk_id { get; set; }
            public Int32 model_number { get; set; }
            public Int32 size { get; set; }
            public Int32 unk_number { get; set; }
            // number of texture ids
            public Int32 texture_ids_count { get; set; }
        }

        /// <summary>
        ///  second header 144 bytes
        /// </summary>
        public class header_second
        {
            public Int32 x000_unk { get; set; }
            public Int32 x004_unk { get; set; } // always = 1
            public Int32 x008_unk { get; set; }
            public Int32 x012_unk { get; set; }

            public Int32 x016_unk { get; set; }
            public Int32 x020_unk { get; set; } // always 128
            public Int32 x024_unk { get; set; }
            public Int32 x00028_unk { get; set; }

            public float x032_unk { get; set; }
            public float x036_unk { get; set; } 
            public float x040_unk { get; set; }
            public float x044_unk { get; set; }

            public Int32 x048_unk { get; set; }
            public Int32 x052_unk { get; set; }
            public Int32 x056_unk { get; set; }
            public Int32 x060_unk { get; set; }

            public Int32 x064_unk { get; set; }
            public Int32 x068_unk { get; set; }
            public Int32 x072_unk { get; set; }
            public Int32 x076_unk { get; set; }

            public float x080_unk { get; set; }
            public float x084_unk { get; set; }
            public float x088_unk { get; set; }
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

            public Int32 x136_unk { get; set; }
            public Int32 x140_unk { get; set; }
        }

        /// <summary>
        ///  vertex buffer header
        /// </summary>
        public class vertex_buffer_header
        {
            public Int32 x000_unk { get; set; }

            public Int32 x004_vertex_count { get; set; } 
            public Int32 x008_vertex_struct { get; set; }
            public Int32 x012_vertex_def_size { get; set; }
            public Int32 x016_vertex_buffer_size { get; set; }
        }

        /// <summary>
        /// (32 bytes) triangle group, defines triangles indices and material to apply 
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
            public DataFormats.JSRF.MDLB.color color { get; set; } // its BGRA instead of RGBA (invert color order)
            public Int32 shader_id { get; set; } // if this = FFFFFF color_0 is used // otherwise external material or texture?
            public Int32 unk_id2 { get; set; }
            public Int32 pad12 { get; set; }
            public float HB { get; set; }
        }

        #endregion

    }
}
