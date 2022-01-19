using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSRF_ModTool.DataFormats.JSRF
{
    class Texture_header
    {
        public Int32 Texture_ID { get; set; }
        public Int32 unk_4 { get; set; }
        public Int32 unk_8 { get; set; }
        public Int32 unk_12 { get; set; }

        public Int32 unk_16 { get; set; } 
        public Int32 resolution { get; set; }
        public byte compression_format { get; set; } // 5 = dxt1 | 6 = dxt3 |  24
        public byte has_alpha { get; set; } // 1 = alpha, 5 = no alpha  25
        public byte is_swizzled { get; set; } // 1 = swizzled 26
        public byte unk_27 { get; set; }

        public Int16 mipmap_count { get; set; } 
        public Int16 unk_30 { get; set; }


        public byte[] serialize()
        {
            List<byte[]> b = new List<byte[]>();
            b.Add(BitConverter.GetBytes(Texture_ID));
            b.Add(BitConverter.GetBytes(unk_4));
            b.Add(BitConverter.GetBytes(unk_8));
            b.Add(BitConverter.GetBytes(unk_12));

            b.Add(BitConverter.GetBytes(unk_16));
            b.Add(BitConverter.GetBytes(resolution));
            b.Add(new byte[] { compression_format } );
            b.Add(new byte[] { has_alpha });

            b.Add(new byte[] { is_swizzled });
            b.Add(new byte[] { unk_27 });
            b.Add(BitConverter.GetBytes(mipmap_count));
            b.Add(BitConverter.GetBytes(unk_30));

            return b.SelectMany(byteArr => byteArr).ToArray();
        }
    }
}
