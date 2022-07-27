using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace JSRF_ModTool.DataFormats.Xbox
{
    public static class TextureSwizzle
    {

        public static byte[] QuadtreeUnswizzle(byte[] Body, int Resolution)
        {
            var length = Body.Length; //Body.Length - 0x20;
            var pitch = length / Resolution;
            var bytesPerPixel = pitch / Resolution;
            var size = (int)Math.Sqrt(length / bytesPerPixel);
            var depth = (int)Math.Log(size, 2);
            var tree = new byte[length, depth];
            byte[] result = new byte[length];
            for (int i = 0; i < length; i += bytesPerPixel)
            {
                var iOut = UnswizzleOffset(i, size, bytesPerPixel);
                for (int b = 0; b < bytesPerPixel; b++)
                {
                    result[iOut + b] = Body[i + b]; //Body[0x20 + i + b];
                }
            }
            return result;
        }

        public static int UnswizzleOffset(int offset, int size, int bytesPerPixel)
        {
            var ordinal = offset / bytesPerPixel;
            var depth = (int)Math.Log(size, 2);
            if (offset % bytesPerPixel != 0) throw new DataMisalignedException("Offset does not align with start of pixel.");
            if (ordinal < 0 || ordinal >= size * size) throw new IndexOutOfRangeException($"Expected offset in [0, {size * size * bytesPerPixel - 1}], got {offset}.");
            var tree = new byte[depth];
            var index = depth - 1;
            if (ordinal == 0)
                return 0;

            while (ordinal != 0)
            {
                int remainder = (ordinal % 4);
                tree[index--] = (byte)remainder;
                ordinal = ordinal / 4;
            }

            var step = size / 2;
            var x = 0;
            var y = 0;
            for (int i = 0; i < depth; i++)
            {
                x += (tree[i] % 2 == 1) ? step : 0;
                y += (tree[i] > 1) ? step : 0;
                step /= 2;
            }

            return (x + y * size) * bytesPerPixel;
        }
    }
}
