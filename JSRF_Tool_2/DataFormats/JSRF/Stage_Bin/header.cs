using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSRF_ModTool.DataFormats.JSRF.Stage_Bin
{
    public class header
    {
        public Int32 block_00_start_offset { get; set; } // 
        public Int32 block_00_size { get; set; } // 59640

        public Int32 block_01_start_offset { get; set; } // 
        public Int32 block_01_size { get; set; } // 

        public Int32 block_02_start_offset { get; set; } // 
        public Int32 block_02_size { get; set; } // 

        public Int32 Stage_Models_count { get; set; } // 
        public Int32 unk { get; set; } // zero

        // stage number
        public Int32 unk_32 { get; set; } // zero
        public Int32 unk_36 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_40 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_44 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_48 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_52 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_56 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_60 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_64 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_68 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_72 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_76 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_80 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_84 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_88 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_92 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_96 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_100 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_104 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_108 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_112 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_116 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_120 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_124 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_128 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_132 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_136 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_140 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_144 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_148 { get; set; } // number of StgXX_XX parts 
        public Int32 unk_152 { get; set; } // number of StgXX_XX parts 

        public Int32 unk_156 { get; set; } // NaN (indicates end of block or header??)
        // END HEADER??
    }
}
