using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSRF_ModTool.Vector;
using JSRF_ModTool.Functions;

namespace JSRF_ModTool.DataFormats.JSRF
{
    class mission_dat
    {
        // containes list of 3D models
        public List<model> models { get; set; }
        private string filename;

        public mission_dat(string filepath)
        {
            filename = System.IO.Path.GetFileNameWithoutExtension(filepath);
            this.models = new List<model>();

            byte[] data = Parsing.FileToByteArray(filepath, 0);

            int i = 0;
            model mdl;
            while (i < data.Length)
            {
                // if file is empty (if it starts with: FFFFFFFF 000000000000000000000000)
                if (BitConverter.ToInt32(data, i) == -1)
                {
                    break;
                }
                mdl =  new model();
                // load binary to class 
                mdl = (model)(Parsing.binary_to_struct(data, i, typeof(model)));

                List<string> verts_str_list = new List<string>();
                int vert_startoff = i + 272;
                // load vertices to list
                for (int v = vert_startoff; v < vert_startoff + (mdl._248_vertex_count * mdl._256_vertex_def_size); v+= mdl._256_vertex_def_size)
                {
                    mdl.vertices.Add((Vector3)(Parsing.binary_to_struct(data, v + 4, typeof(Vector3))));
                    verts_str_list.Add(mdl.vertices[mdl.vertices.Count - 1].X + " " + mdl.vertices[mdl.vertices.Count - 1].Y + " " + mdl.vertices[mdl.vertices.Count - 1].Z);
                }

                System.IO.File.WriteAllLines(@"C:\Users\Mike\Desktop\JSRF\research\mission_dat\vertices.txt", verts_str_list);

                int tri_startoff = i + 96 + mdl._260_triangle_buffer_startoff;

                // load triangle indices to list
                for (int t = tri_startoff; t < (tri_startoff + mdl._264_triangle_count * 2) ; t +=2)
                {
                    mdl.triangles.Add(BitConverter.ToInt16(data, t));
                }

                this.models.Add(mdl);

                i += mdl._04_total_size;

            }

            export_models(@"C:\Users\Mike\Desktop\JSRF\research\mission_dat_models\");

        }

        public void export_models(string directory)
        {
            List<string> obj;
            for (int i = 0; i < models.Count; i++)
            {
                model mdl = models[i];
                obj = new List<string>();

                obj.Add("o " + filename + "__" + i);

                for (int v = 0; v < mdl.vertices.Count; v++)
                {
                    obj.Add("v " + mdl.vertices[v].X + " " + mdl.vertices[v].Y + " " + mdl.vertices[v].Z);
                }


                for (int t = 0; t < mdl.triangles.Count -2; t+=3)
                {
                    obj.Add("f " + (int)(mdl.triangles[t] + 1) + " " + (int)(mdl.triangles[t + 1] + 1) + " "  + (int)(mdl.triangles[t + 2] + 1));
                }

                System.IO.Directory.CreateDirectory(directory + filename + "\\" + filename);
                System.IO.File.WriteAllLines(directory + filename + "\\" + filename + "_" + i + ".obj", obj);
            }


        }

        public class model
        {
            public Int32 _00_ID { get; set; }
            public Int32 _04_total_size { get; set; } // size of whole block of data header + vertex/index buffers
            public Int32 _08_count { get; set; }
            public Int32 _12_unk { get; set; }

            public Vector4 v0 { get; set; }
            public Vector4 v1 { get; set; }
            public Vector4 v2 { get; set; }
            public Vector4 v3 { get; set; }


            public Int32 _080_unk { get; set; }
            public Int32 _084_unk { get; set; } // set to 1
            public Int32 _088_unk { get; set; }
            public Int32 _092_unk { get; set; }

            public Int32 _096_unk { get; set; } // set to 148
            public Int32 _100_unk { get; set; } // set to 128
            public Int32 _104_unk { get; set; }
            public Int32 _108_unk { get; set; }

            public Int32 _112_unk { get; set; } // unk might be two int16 the second giving a count or size? or an int32 ID
            public Vector3 _116_unk { get; set; }

            //public byte[] padding { get; set; }

            ///  padding

            public Int32 _99_a { get; set; } // = padding
            public Int32 _999_b { get; set; } // = padding
            public Int32 _999_c { get; set; } // = padding
            public Int32 _999_d { get; set; } // = padding

            public Int32 _99_ab { get; set; } // = padding
            public Int32 _999_bb { get; set; } // = padding
            public Int32 _999_cb { get; set; } // = padding
            public Int32 _999_db { get; set; } // = padding

            public Int32 _99_ac { get; set; } // = padding
            public Int32 _999_cc { get; set; } // = padding
            public Int32 _999_ccg { get; set; } // = padding
            public Int32 _999_dc { get; set; } // = padding

            public Int32 _99_ad { get; set; } // = padding
            public Int32 _999_bd { get; set; } // = padding
            public Int32 _999_cd { get; set; } // = padding
            public Int32 _999_dd { get; set; } // = padding

            public Int32 _99_ae { get; set; } // = padding
            public Int32 _999_be { get; set; } // = padding
            public Int32 _999_ce { get; set; } // = padding
            public Int32 _999_de { get; set; } // = padding

            public Int32 _99_af { get; set; } // = padding
            public Int32 _999_bf { get; set; } // = padding
            public Int32 _999_cf { get; set; } // = padding
            public Int32 _999_df { get; set; } // = padding





            public Int32 _224_flag{ get; set; } // = 0xFFFFFFFF
            public Int32 _228_unk { get; set; } 
            public Int32 _232_unk { get; set; }
            public Int32 _236_unk { get; set; }

            // vertex / triangles buffers headers / info
            public Int32 _240_unk { get; set; } 
            public Int32 _244_unk { get; set; } // 180
            public Int32 _248_vertex_count { get; set; }
            public Int32 _252_unk { get; set; }

            public Int32 _256_vertex_def_size { get; set; } // often 32
            public Int32 _260_triangle_buffer_startoff { get; set; } // add + 128 to get real pos
            public Int32 _264_triangle_count{ get; set; } // multiply by 2 to get real bytes size
            public Int32 _268_unk { get; set; }


            public List<Vector3> vertices { get; set; }
            public List<Int16> triangles { get; set; }

            public model()
            {
                vertices = new List<Vector3>();
                triangles = new List<short>();
               // this.padding = new byte[96];
            }

        }


    }
}
