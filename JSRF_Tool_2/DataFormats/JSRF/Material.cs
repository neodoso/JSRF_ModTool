using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSRF_ModTool.DataFormats.JSRF
{

    /// <summary>
    /// JSRF Material definition
    /// </summary>
    /// 
    /// <remarks>
    /// JSRF materials contain list of texture IDs, or IDs that are unknown (possibly shader id, or material id)
    /// if there's more than one id its usually the second id that's the texture id
    /// some materials seem to have a List of ids rather than just one or two
    /// still need to figure out if this material structure defines RGB values for the shader color
    /// or if its just a shader ID for a predefined sets of shaders hard-coded in the game executable file (XBE)
    /// most likely models such as menus UI widgets are colored by a texture used as palette, but not sure
    ///
    /// Structure of a material in game file
    ///  0x0 : Int32 number_of_texture_ids
    ///  0x4 : Int32 texture_id
    ///  0x8 : Int32 texture_id
    /// .... etc
    /// </remarks>
    class Material
    {
        // Int32 ids_count = 0;
        public List<Int32> texture_id = new List<Int32>();

        /// <summary>
        /// read byte array as materials list
        /// </summary>
        public Material(byte[] data)
        {
            int ids_count = BitConverter.ToInt32(data, 0);

            for (int i = 4; i < (ids_count * 4) +4 ; i+=4)
            {
                if (i + 4 >= data.Length) { return; }
                texture_id.Add(BitConverter.ToInt32(data, i));
            }

        }
    }
}

