using System;
using System.Collections.Generic;
using System.Globalization;

namespace JSRF_ModTool.Vector
{

        public class Vector2
        {
            public float X { get; set; }
            public float Y { get; set; }

            public Vector2()
            {
            }

            public Vector2(string x, string y)
            {
                setVals(float.Parse(x), float.Parse(y));
            }

            public Vector2(float x, float y)
            {
                setVals(x, y);
            }

            private void setVals(object x, object y)
            {
                if (x is string) { X = Convert.ToSingle(x.ToString().Replace('.', ',')); }
                if (y is string) { Y = Convert.ToSingle(y.ToString().Replace('.', ',')); }

                if ((x is float) || (x is Single) || (x is int)) { X = Convert.ToSingle(x); }
                if ((y is float) || (y is Single) || (y is int)) { Y = Convert.ToSingle(y); }
            }
        }

        public class Vector3
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }

        public Vector3()
        {
            X = 0f;
            Y = 0f;
            Z = 0f;
        }

            public Vector3(string x, string y, string z)
            {
                setVals(float.Parse(x),float.Parse(y),float.Parse(z));
            }

            public Vector3(float x, float y, float z)
            {
                setVals(x, y, z);
            }


            /// <summary>
            /// converts float to string also truncate the float precision and use dot instead of comma(with CultureInfo)
            /// </summary>
            public List<String> Vector_to_StringList(int float_precision, bool to_radians)
            {
                    List<String> v = new List<String>();

                    if (!to_radians)
                    {
                        v.Add(this.X.ToString("N" + float_precision.ToString(), CultureInfo.CreateSpecificCulture("en-US")));
                        v.Add(this.Y.ToString("N" + float_precision.ToString(), CultureInfo.CreateSpecificCulture("en-US")));
                        v.Add(this.Z.ToString("N" + float_precision.ToString(), CultureInfo.CreateSpecificCulture("en-US")));
                    }
                    else
                    {
                        double x = (Math.PI / 180) * this.X;
                        double y = (Math.PI / 180) * this.Y;
                        double z = (Math.PI / 180) * this.Z;
                        v.Add(x.ToString("N" + float_precision.ToString(), CultureInfo.CreateSpecificCulture("en-US")));
                        v.Add(y.ToString("N" + float_precision.ToString(), CultureInfo.CreateSpecificCulture("en-US")));
                        v.Add(z.ToString("N" + float_precision.ToString(), CultureInfo.CreateSpecificCulture("en-US")));
                    }

                    return v;
            }

            private void setVals(object x, object y, object z)
            {
                if (x is string) { X = Convert.ToSingle(x.ToString().Replace('.', ',')); }
                if (y is string) { Y = Convert.ToSingle(y.ToString().Replace('.', ',')); }
                if (z is string) { Z = Convert.ToSingle(z.ToString().Replace('.', ',')); }

                if ((x is float) || (x is Single) || (x is int)) { X = Convert.ToSingle(x); }
                if ((y is float) || (y is Single) || (y is int)) { Y = Convert.ToSingle(y); }
                if ((z is float) || (z is Single) || (z is int)) { Z = Convert.ToSingle(z); }
            }

        }

        public class Vector4
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float W { get; set; }
        }
}
