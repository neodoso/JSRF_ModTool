using System;

namespace JSRF_ModTool
{
   public class ByteReader
    {

        // Int16 to Byte Array
        public static byte[] ConvertIntToByteArray(Int16 I16)
        {
            return BitConverter.GetBytes(I16);
        }

        // Int32 to Byte array
        public static byte[] ConvertInt32ToByteArray(Int32 I32)
        {
            return BitConverter.GetBytes(I32);
        }

        //  Int64 to Byte Array
        public static byte[] ConvertIntToByteArray(Int64 I64)
        {
            return BitConverter.GetBytes(I64);
        }

        // int to Byte Array
        public static byte[] ConvertIntToByteArray(int I)
        {
            return BitConverter.GetBytes(I);
        }

        // Byte to Int32
        public static int ConvertByteToInt32(byte[] b)
        {
            return BitConverter.ToInt32(b, 0);
        }

    }
}
