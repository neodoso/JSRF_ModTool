using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSRF_ModTool.DataFormats
{
    public class Bmp
    {
        public int Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Int16 Planes { get; set; }
        public Int16 BitsPerPixel { get; set; }
        public CompressionMethod Compression { get; set; }
        public int DataOffset { get; set; }
        public int DibHeaderSize { get; set; }
        public int DataSize { get; set; }
        public int XPelsPerMeter { get; set; }
        public int YPelsPerMeter { get; set; }
        public int ColorCount { get; set; }
        public int ImportantColorCount { get; set; }
        public byte[] Data { get; set; }
        public Bmp(byte[] data, int resolution)
        {
            Data = data;
            DibHeaderSize = 40;
            DataSize = data.Length;
            DataOffset = 10 + DibHeaderSize;
            Size = DataOffset + DataSize;
            Width = resolution;
            Height = resolution;
            Planes = 1;
            BitsPerPixel = (Int16)(DataSize * 8 / resolution / resolution);
            Compression = CompressionMethod.BI_RGB;
            XPelsPerMeter = 2835;
            YPelsPerMeter = 2835;
            ColorCount = 0;
            ImportantColorCount = 0;
        }
        public byte[] ToByteArray()
        {
            var ms = new MemoryStream();
            ms.Write(new byte[] { 0x42, 0x4D }, 0, 2);
            ms.Write(BitConverter.GetBytes(Size), 0, 4);
            ms.Write(new byte[4], 0, 4);
            ms.Write(BitConverter.GetBytes(DataOffset), 0, 4);
            ms.Write(BitConverter.GetBytes(DibHeaderSize), 0, 4);
            ms.Write(BitConverter.GetBytes(Width), 0, 4);
            ms.Write(BitConverter.GetBytes(Height), 0, 4);
            ms.Write(BitConverter.GetBytes(Planes), 0, 2);
            ms.Write(BitConverter.GetBytes(BitsPerPixel), 0, 2);
            ms.Write(BitConverter.GetBytes((int)Compression), 0, 4);
            ms.Write(BitConverter.GetBytes(DataSize), 0, 4);
            ms.Write(BitConverter.GetBytes(XPelsPerMeter), 0, 4);
            ms.Write(BitConverter.GetBytes(YPelsPerMeter), 0, 4);
            ms.Write(BitConverter.GetBytes(ColorCount), 0, 4);
            ms.Write(BitConverter.GetBytes(ImportantColorCount), 0, 4);
            ms.Write(Data, 0, DataSize);
            return ms.ToArray();
        }
        public enum CompressionMethod
        {
            BI_RGB = 0,
            BI_RLE8 = 1,
            BI_RLE4 = 2,
            BI_BITFIELDS = 3,
            BI_JPEG = 4,
            BI_PNG = 5,
            BI_ALPHABITFIELDS = 6,
            BI_CMYK = 11,
            BI_CMYKRLE8 = 12,
            BI_CMYKRLE4 = 13
        }
    }
}
