using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSRF_ModTool.DataFormats.Xbox
{
    public static class TextureSwizzleParsing
    {
        /* This should be pretty straightforward.
 * It creates a bit pattern like ..zyxzyxzyx from ..xxx, ..yyy and ..zzz
 * If there are no bits left from any component it will pack the other masks
 * more tighly (Example: zzxzxzyx = Fewer x than z and even fewer y)
 */
        public static Mask generate_swizzle_masks(UInt32 width,
                                   UInt32 height,
                                   UInt32 depth)
        {
            UInt32 x = 0, y = 0, z = 0;
            UInt32 bit = 1;
            UInt32 mask_bit = 1;
            bool done;
            do
            {
                done = true;
                if (bit < width) { x |= mask_bit; mask_bit <<= 1; done = false; }
                if (bit < height) { y |= mask_bit; mask_bit <<= 1; done = false; }
                if (bit < depth) { z |= mask_bit; mask_bit <<= 1; done = false; }
                bit <<= 1;
            } while (!done);
            if (!((((x ^ y) ^ z) == (mask_bit - 1)))) throw new Exception();
            return new Mask { x = x, y = y, z = z };
        }
        /* This fills a pattern with a value if your value has bits abcd and your
 * pattern is 11010100100 this will return: 0a0b0c00d00
 */
        public static UInt32 fill_pattern(UInt32 pattern, UInt32 value)
        {
            UInt32 result = 0;
            UInt32 bit = 1;
            while (value != 0)
            {
                if (pattern != 0 & bit != 0)
                {
                    /* Copy bit to result */
                    result |= (value != 0) & true ? bit : 0;
                    value >>= 1;
                }
                bit <<= 1;
            }
            return result;
        }

        public static UInt32 get_swizzled_offset(
            UInt32 x, UInt32 y, UInt32 z,
            UInt32 mask_x, UInt32 mask_y, UInt32 mask_z,
            UInt32 bytes_per_pixel)
        {
            return bytes_per_pixel * (fill_pattern(mask_x, x)
                                   | fill_pattern(mask_y, y)
                                   | fill_pattern(mask_z, z));
        }
        public static byte[] unswizzle_box(
            byte[] src_buf,
            UInt32 width,
            UInt32 height,
            UInt32 depth,

            UInt32 row_pitch,
            UInt32 slice_pitch,
            UInt32 bytes_per_pixel)
        {
            Mask mask = generate_swizzle_masks(width, height, depth);
            UInt32 sliceOffset = 0;
            UInt32 x, y, z;
            byte[] dst_buf = new byte[src_buf.Length];
            for (z = 0; z < depth; z++)
            {
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        uint iSrc =
                            get_swizzled_offset(x, y, z, mask.x, mask.y, mask.z,
                                                  bytes_per_pixel);
                        uint iDst = y * row_pitch + x * bytes_per_pixel;
                        byte[] pixel = new byte[bytes_per_pixel];
                        Array.Copy(src_buf, iSrc, dst_buf, iDst, bytes_per_pixel);
                    }
                }
                sliceOffset += slice_pitch;
            }
            return dst_buf;
        }
        public static byte[] unswizzle_rect(
                     byte[] src_buf,
                     UInt32 width,
                     UInt32 height,

                     UInt32 pitch,
                     UInt32 bytes_per_pixel)
        {
            return unswizzle_box(src_buf, width, height, 1, pitch, 0, bytes_per_pixel);
        }
    }

    public struct Mask
    {
        public UInt32 x;
        public UInt32 y;
        public UInt32 z;
    }
}
