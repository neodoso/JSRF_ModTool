using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSRF_ModTool.DataFormats.JSRF
{
    class mission_dat
    {
        // series of blocks with floats list  and then list of ints (triangles indices???) seems to be 4 floats (0 XYZ position)  ++ 4 floats (0? XYZ) maybe radian rotation
        // no idea if these are models or animation

        public class block
        {
            Int32 _00_ID { get; set; }
            Int32 _01_size { get; set; }
            Int32 _02_unk { get; set; }
            Int32 _03_unk { get; set; }

            Int32 _04_ID_long { get; set; }
            Int32 _05_unk { get; set; } // zero
            float _06_unk { get; set; }
            float _07_unk { get; set; }


            // offset 32
            float _08_unk { get; set; }
            float _09_unk { get; set; }
            float _010_unk { get; set; }
            float _011_unk { get; set; }

            float _012_unk { get; set; }
            float _013_unk { get; set; }
            float _014_unk { get; set; }
            float _015_unk { get; set; }


            // offset 64
            float _016_unk { get; set; }
            float _017_unk { get; set; }
            float _018_unk { get; set; }
            float _019_unk { get; set; }

            float _020_unk { get; set; } // zero (padding? )
            float _021_unk { get; set; } // zero (padding? )
            float _022_unk { get; set; } // zero (padding? )
            float _023_unk { get; set; } // zero (padding? )



            // offset 96
            Int32 _024_unk { get; set; }
            float _025_unk { get; set; }
            float _026_unk { get; set; }
            float _027_unk { get; set; }

            float _028_unk { get; set; } // zero (padding? )
            float _029_unk { get; set; } // zero (padding? )
            float _030_unk { get; set; } // zero (padding? )
            float _031_unk { get; set; } // zero (padding? )
        }


    }
}
