using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSRF_Tool_2.DataFormats.JSRF
{
    class Level_model
    {
        /// <summary>
        ///  level model header
        /// </summary>
        public class Level_model_header
        {
            public Int32 file_type { get; set; }
            public Int32 level_part_id { get; set; }
            public Int32 unk_size { get; set; }
            public Int32 unk_number { get; set; }

            public Int32 textures_list_count { get; set; }
            public List<Int32> texture_ids { get; set; }


            public Int32 unk_0 { get; set; } // 0
            public Int32 unk_1 { get; set; } // 1

            public Int32 unk_2 { get; set; } // 0
            public Int32 unk_3 { get; set; } // 0
            public Int32 unk_4 { get; set; } // 148
            public Int32 unk_5 { get; set; } // 128

            public Int32 unk_6z { get; set; } // 0
            public Int32 unk_7z { get; set; } // 0
            public Vector.Vector4 vect_4 { get; set; }
            public Int32 unk_8 { get; set; } // 0
            public Int32 unk_9 { get; set; } // 0

            public Int32 unk_10 { get; set; } // 0
            public Int32 unk_11 { get; set; } // 0
            public Int32 unk_12 { get; set; } // 0
            public Int32 unk_13 { get; set; } // 0

            public Int32 unk_14 { get; set; } // 0
            public Int32 unk_15 { get; set; } // 0
            public Vector.Vector3 vect_3 { get; set; } // 1 1 1

            public Int32 unk_16 { get; set; } // 0
            public Int32 unk_17 { get; set; } // 0
            public Int32 unk_18 { get; set; } // 0

            public Int32 unk_19 { get; set; } // 0
            public Int32 unk_20 { get; set; } // 0
            public Int32 unk_21 { get; set; } // 0
            public Int32 unk_22 { get; set; } // 0

            public Int32 unk_23 { get; set; } // 0
            public Int32 unk_24 { get; set; } // 1
            public Int32 unk_25 { get; set; } // 180
            public Int32 unk_26 { get; set; } // 1

            public Int32 unk_27 { get; set; } // 180
            public Int32 unk_28 { get; set; } // 1


   

    }

        public class block_1
        {
            public Int32 unk_0 { get; set; } // always = 0?
            public Int32 unk_4 { get; set; } // always = 1?
            public Int32 unk_8 { get; set; } // always = 0?
            public Int32 unk_12 { get; set; } // always = 0?
            public Int32 unk_16 { get; set; } // size of next block?
            public Int32 unk_20 { get; set; } // always = 128
            public Int32 unk_24 { get; set; } // always = 0?
            public Int32 unk_28 { get; set; } // always = 0?

            public Vector.Vector3 unk_32_v3 { get; set; }
            public Vector.Vector3 unk_44_v3 { get; set; }
        }
    }
}
