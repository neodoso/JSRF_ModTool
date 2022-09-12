using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JSRF_ModTool.Vector
{
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2()
        {
        }

        // TODO test and if not needed, remove float.Parse() since setVals() already converts string to single
        public Vector2(string x, string y)
        {
            setVals(float.Parse(x, CultureInfo.CreateSpecificCulture("en-US")), float.Parse(y, CultureInfo.CreateSpecificCulture("en-US")));
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
        }

        public void round(int digits)
        {
            this.X = (float)Math.Round(this.X, digits, MidpointRounding.AwayFromZero);
            this.Y = (float)Math.Round(this.Y, digits, MidpointRounding.AwayFromZero);
            this.Z = (float)Math.Round(this.Z, digits, MidpointRounding.AwayFromZero);
        }


        // as string
        public Vector3(string x, string y, string z)
        {
            setVals(float.Parse(x, CultureInfo.CreateSpecificCulture("en-US")), float.Parse(y, CultureInfo.CreateSpecificCulture("en-US")), float.Parse(z, CultureInfo.CreateSpecificCulture("en-US")));
        }

        // as float
        public Vector3(float x, float y, float z)
        {
            setVals(x, y, z);
        }

        // Vector3 substraction
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        }
        /*
        // Vector3 addition
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(b.X + a.X, b.Y + a.Y, b.Z + a.Z);
        }
        */

        // Vector3 division by float
        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.X / d, a.Y / d, a.Z / d);
        }

        public override string ToString()
        {
            return this.X.ToString() + " " + this.Y.ToString() + " " + this.Z.ToString();
        }


        /// <summary>
        /// converts float to string, also truncate the float precision and use dot instead of comma(with CultureInfo)
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
                float x = (float)(Math.PI / 180) * this.X;
                float y = (float)(Math.PI / 180) * this.Y;
                float z = (float)(Math.PI / 180) * this.Z;
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

        public byte[] Serialize()
        {
            List<byte[]> b = new List<byte[]>();
            b.Add(BitConverter.GetBytes(this.X)); b.Add(BitConverter.GetBytes(this.Y)); b.Add(BitConverter.GetBytes(this.Z));
            return b.SelectMany(byteArr => byteArr).ToArray();
        }
    }

    public class Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
    }

    // class used to calculate bounding box for a list of points 
    // points are fed through add_point()
    public class bounds
    {
        // min max to calculate the bounding box
        public float Xmin { get; set; } = float.PositiveInfinity;
        public float Ymin { get; set; } = float.PositiveInfinity;
        public float Zmin { get; set; } = float.PositiveInfinity;
        public float Xmax { get; set; } = float.NegativeInfinity;
        public float Ymax { get; set; } = float.NegativeInfinity;
        public float Zmax { get; set; } = float.NegativeInfinity;

        private int point_count { get; set; } // count points added, to calculate the center of thee mesh by dividing the sum of XYZ's components

        private Vector3 mesh_center = new Vector3();

        // center of the mesh
        public Vector3 center
        {
            get 
            {
                if (mesh_center != null)
                {
                    return mesh_center / point_count; //new Vector3(mesh_center.X / point_count, mesh_center.Y / point_count, mesh_center.Z / point_count);
                } else
                {
                    return new Vector3(0f, 0f, 0f);
                }
               
            }
            set { } 
    }

        public bounds()
        {
            this.center = new Vector3();
        }

        // returns calculated bounding box based on axes min/max
        public bbox bounding_box
        {
            get { return new bbox(new Vector3(Xmin, Ymin, Zmin), new Vector3(Xmax, Ymax, Zmax)); }
        }

        // given a point position add/sub min/max values
        // to get the bounds of a list of points
        public void add_point(Vector3 pnt)
        {
            Xmin = pnt.X < Xmin ? Xmin = pnt.X : Xmin;
            Ymin = pnt.Y < Ymin ? Ymin = pnt.Y : Ymin;
            Zmin = pnt.Z < Zmin ? Zmin = pnt.Z : Zmin;
            Xmax = pnt.X > Xmax ? Xmax = pnt.X : Xmax;
            Ymax = pnt.Y > Ymax ? Ymax = pnt.Y : Ymax;
            Zmax = pnt.Z > Zmax ? Zmax = pnt.Z : Zmax;

            mesh_center = new Vector3(mesh_center.X + pnt.X, mesh_center.Y + pnt.Y, mesh_center.Z + pnt.Z);
            point_count++;
        }

        // calculate the average/global bounding box from multiple bounding boxes summed up with this method
        // sets bounds based on the min/max A/B of a bounding box
        // pass multiple bounding boxes's A and B points and keeps the min/max (A B) values
        public void add_AB(Vector3 A, Vector3 B)
        {
            Xmin = A.X < Xmin ? Xmin = A.X : Xmin;
            Ymin = A.Y < Ymin ? Ymin = A.Y : Ymin;
            Zmin = A.Z < Zmin ? Zmin = A.Z : Zmin;
            Xmax = B.X > Xmax ? Xmax = B.X : Xmax;
            Ymax = B.Y > Ymax ? Ymax = B.Y : Ymax;
            Zmax = B.Z > Zmax ? Zmax = B.Z : Zmax;
        }

        // bounding box
        public class bbox
        {
            public Vector3 A { get; set; } = new Vector3();
            public Vector3 B { get; set; } = new Vector3();

            public bbox(Vector3 A, Vector3 B)
            {
                this.A = A;
                this.B = B;
            }
        }
    }
}

