using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSRF_Tool_2.MDLB_Import
{
    public class Materials
    {
        public enum materials
        {
            Standard = 1328128,
            Skin = 1360896,
            Metal_reflective = 6726306,
            Lambert = 16711680,
            Metal_reflective_1 = 6693602,
            Metal = 1122978, 
            Semi_Transparent = 4612770,
            Self_illuminated = 524287,
           // UV_scroll_X_ticking = 4661255,
           // UV_scroll_X_sliding = 459263,
        }


        /// <summary>
        /// RGBA color
        /// </summary>
        public class color
        {
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }
            public byte A { get; set; }

            public color(byte _R, byte _G, byte _B, byte _A)
            {
                this.R = _R;
                this.G = _G;
                this.B = _B;
                this.A = _A;
            }
        }
    }
}
