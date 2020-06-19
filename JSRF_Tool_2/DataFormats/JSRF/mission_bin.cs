using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSRF_Tool_2.DataFormats.JSRF
{
    class mission_bin
    {

        class header
        {
            Int16 level_id;
            Int16 level_id1;

            Int32 unk;


            // long list of offset + count, offset/count is set to zero if there are no blocks for that type (seems mission bin has around 68 different types of blocks)
        }
    }
}
