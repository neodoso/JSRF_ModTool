﻿using System;
using System.Text;
using System.Reflection;
using JSRF_Tool_2.Vector;

using System.Windows.Media.Media3D;

namespace JSRF_Tool_2
{

    public class DynamicParser 
    {

        /// <summary>
        /// Goes through a class/struct's properties and reads a byte array as typeof, for each property in the class/struct.
        /// </summary>
        /// <remaks>
        /// This is used to load most JSRF classes dynamically.
        /// 
        /// This uses reflection, so the downside is that it may not be as fast as a hard coded method sequential reading of a MemoryStream or BinaryStream.
        /// However the advantage of using this class to load file structures and data is that this allows flexibility since we can modify/restructure the 
        /// class/struct properties at any time and it will still load with this method.
        /// 
        /// Once the JSRF classes are fully mapped and finalized it might be better (because possibly faster to load) to code a method within the class that 
        /// uses a stream reader and a hard coded sequence of the properties and type of values to read.
        /// 
        /// TODO: maybe find a way to get the 'typeof' property and automatically convert as type, rather than getting the name as string and going through 
        /// a bunch of switch statements... this probably slows things down a bit right now.
        /// </remaks>
        /// <param name="bytes">byte array</param>
        /// <param name="start_pos">starting position</param>
        /// <param name="objType">Class/Struct to load byte array as</param>
        /// <returns></returns>
        public static Object binary_to_struct(byte[] bytes, int start_pos, Type objType)
        {
            // create instance of object from objType
            Object obj = Activator.CreateInstance(objType);

            int i = start_pos;

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                Object value = new Object();
                value = prop.GetValue(obj, null);
                string t = prop.PropertyType.Name;

                //if (i > bytes.Length) { System.Windows.Forms.MessageBox.Show("binary_to_struct() : Index out of range."); return obj; }

                switch (t)
                {
                    case "Int32":
                        prop.SetValue(obj, BitConverter.ToInt32(bytes, i), null);
                        i += 4;
                        break;
                    case "UInt32":
                        prop.SetValue(obj, BitConverter.ToUInt32(bytes, i), null);
                        i += 4;
                        break;

                    case "Int16":
                        prop.SetValue(obj, BitConverter.ToInt16(bytes, i), null);
                        i += 2;
                        break;
                    case "UInt16":
                        prop.SetValue(obj, BitConverter.ToUInt16(bytes, i), null);
                        i += 4;
                        break;

                    case "Single":
                        prop.SetValue(obj, BitConverter.ToSingle(bytes, i), null);
                        i += 4;
                        break;

                    case "float":
                        prop.SetValue(obj, BitConverter.ToSingle(bytes, i), null);
                        i += 4;
                        break;

                    case "Byte":
                        prop.SetValue(obj, bytes[i], null);
                        i += 1;
                        break;

                    case "Point3D":
                        prop.SetValue(obj, brReadPoint3D(bytes, 3, i), null);
                        i += 12;
                        break;

                    case "Vector3D":
                        prop.SetValue(obj, brReadVector(bytes, 3, i), null);
                        i += 12;
                        break;

                    case "Vector3":
                        prop.SetValue(obj, brReadVector(bytes, 3, i), null);
                        i += 12;
                        break;
                    case "Vector2":
                        prop.SetValue(obj, brReadVector(bytes, 2, i), null);
                        i += 8;
                        break;
                    case "Vector4":
                        prop.SetValue(obj, brReadVector(bytes, 4, i), null);
                        i += 16;
                        break;

                    case "color":
                        prop.SetValue(obj, brReadColor(bytes, i), null);
                        i += 4;
                        break;

                    case "String":
                        int length = prop.GetValue(obj, null).ToString().Length;
                        if(bytes.Length == 0)
                        {
                            break;
                        }
                        prop.SetValue(obj, Encoding.UTF8.GetString(bytes, i, length).Trim('\0'), null);
                        i += length;
                        break;
                }
            }

            return obj;
        }



        /// <summary>
        /// read byte array as as floats and returns as a Point3D
        /// </summary>
        /// <param name="bytes">byte array</param>
        /// <param name="vNum">vector type</param>
        /// <param name="i">position</param>
        /// <returns></returns>
        public static object brReadPoint3D(byte[] bytes, int vNum, int i)
        {
            if (vNum == 3)
            {
                Point3D tmpvector = new Point3D(0, 0, 0);
                tmpvector.X = BitConverter.ToSingle(bytes, i);
                tmpvector.Y = BitConverter.ToSingle(bytes, i + 4);
                tmpvector.Z = BitConverter.ToSingle(bytes, i + 8);

                return tmpvector;
            }
            else
            {
                Point3D tmpvector = new Point3D(0, 0, 0);
                return tmpvector;
            }
        }

    
        /// <summary>
        /// read byte array as as floats and returns as a Vector3D
        /// </summary>
        /// <param name="bytes">byte array</param>
        /// <param name="i">position</param>
        /// <returns></returns>
        public static Vector3D brReadVector3D(byte[] bytes, int i)
        {
           return new Vector3D(BitConverter.ToSingle(bytes, i),  BitConverter.ToSingle(bytes, i + 4), BitConverter.ToSingle(bytes, i + 8));
        }

        /// <summary>
        /// read byte array as series floats and returns as a VectorT 
        /// </summary>
        /// <param name="bytes">byte array</param>
        /// <param name="vNum">type of Vector</param>
        /// <param name="i">position</param>
        /// <returns></returns>
        public static object brReadVector(byte[] bytes, int vNum, int i)
        {
            if (vNum == 3)
            {
                Vector3 tmpvector = new Vector3(0,0,0);
                tmpvector.X = BitConverter.ToSingle(bytes, i);
                tmpvector.Y = BitConverter.ToSingle(bytes, i + 4);
                tmpvector.Z = BitConverter.ToSingle(bytes, i + 8);

                return tmpvector;
            }
            else if (vNum == 4)
            {
                Vector4 tmpvector = new Vector4();
                tmpvector.X = BitConverter.ToSingle(bytes, i);
                tmpvector.Y = BitConverter.ToSingle(bytes, i + 4);
                tmpvector.Z = BitConverter.ToSingle(bytes, i + 8);
                tmpvector.W = BitConverter.ToSingle(bytes, i + 12);

                return tmpvector;
            }
            else if (vNum == 2)
            {

                Vector2 tmpvector = new Vector2();
                tmpvector.X = BitConverter.ToSingle(bytes, i);
                tmpvector.Y = BitConverter.ToSingle(bytes, i + 4);

                return tmpvector;
            }
            else
            {

                return new Vector3(0,0,0);
            }
        }


        public static DataFormats.JSRF.MDLB.color brReadColor(byte[] bytes, int i)
        {
            return new DataFormats.JSRF.MDLB.color(bytes[i], bytes[i+1], bytes[i+2], bytes[i+3]);
        }

    }
}
