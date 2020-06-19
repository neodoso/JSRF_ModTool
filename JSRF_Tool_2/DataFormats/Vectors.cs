using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSRF_HD_Tool
{

    namespace Vectors
    {



      

        public class Vector3
        {
            public float X;
            public float Y;
            public float Z;


            public Vector3(string x, string y, string z)
            {
                setVals(x,y,z);
            }

            public Vector3(float x, float y, float z)
            {
                setVals(x, y, z);
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

            /*
            public void set(object x, object y, object z)
            {
                if (x is string) { X = Convert.ToSingle(x.ToString().Replace('.', ',')); }
                if (y is string) { Y = Convert.ToSingle(y.ToString().Replace('.', ',')); }
                if (z is string) { Z = Convert.ToSingle(z.ToString().Replace('.', ',')); }

                if ((x is float) || (x is Single) || (x is int)) { X = Convert.ToSingle(x); }
                if ((y is float) || (y is Single) || (y is int)) { Y = Convert.ToSingle(y); }
                if ((z is float) || (z is Single) || (z is int)) { Z = Convert.ToSingle(z); }
            }
             * 
             * */
        }



        public class Vector2
        {
        public float X { get; set; }
        public float Y { get; set; }

        public void set(object x, object y)
        {
            if (x is string) { X = Convert.ToSingle(x.ToString().Replace('.', ',')); }
            if (y is string) { Y = Convert.ToSingle(y.ToString().Replace('.', ',')); }
        }
    }

        /*
     public Vector3 AddVector(Vector3 a, Vector3 b)
    {
        Vector3 result = new Vector3();
        result.set(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        me = result;
    }
         * */
   
}
}
