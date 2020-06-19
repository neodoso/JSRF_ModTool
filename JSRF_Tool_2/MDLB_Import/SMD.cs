using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using JSRF_Tool_2.Vector;

namespace JSRF_Tool_2
{
    public class SMD
    {
        public string FilePath { get; set; }
        public int version { get; set; }
        public List<node> nodes { get; set; }
        public List<skeleton_node> skeleton_nodes { get; set; }
        public List<triangle> SMD_triangles { get; set; }

        public List<vertex> vertices_list { get; set; } // converted to JSRF format
        public List<UInt16> triangles_list { get; set; } // converted to JSRF format
        public List<material_group> mat_groups_list { get; set; }  // converted to JSRF format
        //public List<String> mats_list { get; set; }    


        public SMD(string filepath)
        {
            this.version = 1;
            this.SMD_triangles = new List<triangle>();
            this.nodes = new List<node>();
            this.skeleton_nodes = new List<skeleton_node>();
            this.mat_groups_list = new List<material_group>();
            //this.mats_list = new List<string>();

            Import(filepath);
            this.ConvertModel();
        }

        /// <summary>
        /// Import SMD model
        /// </summary>
        public void Import(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    System.Windows.MessageBox.Show("Model importer: " + Path.GetFileName(filepath) + " file does not exist");
                    return;
                }

                this.FilePath = filepath;

                List<String> lines = new List<string>();

                string line;
                string[] args;
                System.IO.StreamReader file = new System.IO.StreamReader(filepath);
                while ((line = file.ReadLine()) != null)
                {
                    // add line to list, while removing double spaces and trim start/end
                    lines.Add(Regex.Replace(line, @"\s+", " ").Trim()); //Regex.Replace(line, " +", " ")
                }

                file.Close();

                for (int i = 0; i < lines.Count; i++)
                {
                    string l = lines[i].ToLower();
                    if (string.IsNullOrEmpty(l)) { continue; }

                    if (l.Contains("version"))
                    {
                        this.version = Convert.ToInt16(l.Split(' ')[1]);
                    }

                    #region nodes

                    if (l.Contains("nodes"))
                    {
                        i++; // next line
                        l = lines[i].ToLower();
                        while (l != "end")
                        {
                            args = lines[i].Split(' ');//(Regex.Replace(lines[i], @"\s+", " ").Trim()).Split(' ');

                            // if number objects inferior to what we expect, return
                            // TODO throw an exception
                            if (args.Length < 3)
                            {
                                System.Windows.MessageBox.Show("SMD Importer Error: unexpected number of node definition arguments.");
                                return;
                            }
                            nodes.Add(new node(Convert.ToInt32(args[0]), Convert.ToInt32(args[2]), args[1].Replace("\"", string.Empty)));
                            i++;
                            l = lines[i];
                        }
                    }

                    #endregion

                    #region skeleton

                    if (l.Contains("skeleton"))
                    {

                        while (!l.ToLower().StartsWith("time "))
                        {
                            i++;
                            l = lines[i];
                        }

                        if (l.Split(' ')[1] == "0")
                        {
                            i++; l = lines[i];


                            while (l.ToLower() != "end")
                            {
                                args = l.Split(' ');
                                // if number objects inferior to what we expect, return
                                // TODO throw an exception
                                if (args.Length < 6)
                                {
                                    System.Windows.MessageBox.Show("SMD Importer Error: unexpected number of skeleton definition arguments.");
                                    return;
                                }

                                skeleton_nodes.Add(new skeleton_node(args[0], new Vector3(args[1], args[2], args[3]), new Vector3(args[4], args[5], args[6])));

                                i++; l = lines[i];
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("SMD Importer Error: could not find skeleton definition for 'time 0'.");
                        }

                    }

                    #endregion

                    #region get triangles

                    // triangles
                    if (l.Contains("triangles"))
                    {
                        // for each triangle
                        for (int t = i + 1; t < lines.Count; t += 4)
                        {
                            triangle tri = new triangle();
                            tri.mat_name = lines[t];

                            #region get vertices in this triangle

                            // for each triangle
                            for (int v = 1; v < 4; v++)
                            {
                                // split string into string array for each space char sperator
                                args = lines[t + v].Split(' ');

                                if (args[0] == "end") { break; }

                                // if number objects inferior to what we expect, return
                                // TODO throw an exception
                                if (args.Length < 10)
                                {
                                    System.Windows.MessageBox.Show("SMD Importer Error: unexpected number of vertex definition arguments (" + args.Length + ") \nIn triangle at line number " + t);
                                    return;
                                }

                                // load values into vertex class
                                vertex vtx = new vertex();
                                vtx.parent_bone = Convert.ToInt32(args[0]);
                                vtx.pos = new Vector3(args[1], args[2], args[3]);
                                vtx.norm = new Vector3(args[4], args[5], args[6]);
                                vtx.uv = new Vector2(args[7], args[8]);

                                // for each bone assigned to this vertex ( *2 two args bone_id/weight )
                                int vtx_weight_count = Convert.ToInt16(args[9]) * 2;

                                // for each bone weighted to this vertex
                                for (int w = 0; w < vtx_weight_count; w += 2)
                                {
                                    vtx.bone_weights.Add(new bone_weight(Convert.ToInt32(args[10 + w]), Convert.ToSingle(args[11 + w])));
                                }


                                // add vertex data to triangle object
                                tri.verts.Add(vtx);
                            }

                            #endregion
                            // add triangle to list
                            SMD_triangles.Add(tri);
                        }
                    }
                    #endregion

                }
            }
            catch { }
        }



        /// <summary>
        /// converts SMD Triangles to generic vertex-index based triangle list
        /// </summary>
        public void ConvertModel()
        {
            vertices_list = new List<vertex>();
            triangles_list = new List<UInt16>();

            //List<string> test = new List<string>();

            int tri_counter = 0;

            //for each trianle in  SMD_triangles
            for (int t = 0; t < SMD_triangles.Count; t++)
            {
                bool vert_already_indexed = false;
                // for each vertex
                foreach (var vt in SMD_triangles[t].verts)
                {
                    vert_already_indexed = false;

                    // for each vertex
                    for (int v = 0; v < vertices_list.Count; v++)
                    {
                        // if the vertex is already indexed ni "vertices list", add vert index to triangles_list
                        if ((vt.pos.X == vertices_list[v].pos.X) && (vt.pos.Y == vertices_list[v].pos.Y) && (vt.pos.Z == vertices_list[v].pos.Z) && (vt.uv.X == vertices_list[v].uv.X))  //&& (vt.uv.Y == vertices_list[v].uv.Y)
                        {
                            vert_already_indexed = true;
                            triangles_list.Add((UInt16)v);
                            tri_counter++;
                            break;
                        }
                    }

                    // if vertex doesn't exist in "vertices_list" add it and also add vertex index in the triangles_list
                    if (!vert_already_indexed)
                    {
                        vertices_list.Add(vt);
                        triangles_list.Add((UInt16)(vertices_list.Count - 1));
                        tri_counter++;
                    }

                    #region store triangle material groups

                    // for every three vertices that make up a triangle
                    // store the triangle indices that are followed by the same material 
                    if (tri_counter == 3)
                    {
                        if (mat_groups_list.Count > 0)
                        {
                            // if material of current triangle is equal to previous mat_group triangle >> add triangl to the triangle/material list
                            if (mat_groups_list[mat_groups_list.Count - 1].material_name == SMD_triangles[t].mat_name)
                            {
                                mat_groups_list[mat_groups_list.Count - 1].triangle_count += 3;
                            }
                            else// if material of current triangle is not equal to previous mat_group triangle >> create and add new mat_group
                            {
                                mat_groups_list.Add(new material_group((UInt16)(triangles_list.Count - 3), 2, SMD_triangles[t].mat_name));
                                //mat_groups_list.Add(new material_group((UInt16)(triangles_list.Count), 2, SMD_triangles[t].mat_name));
                            }

                        } // if first triangle/mat_group 
                        else if (mat_groups_list.Count == 0)
                        {
                            mat_groups_list.Add(new material_group((UInt16)(triangles_list.Count - 3), 2, SMD_triangles[t].mat_name));
                        }
                        //reset triangle counter
                        tri_counter = 0;
                    }

                    #endregion
                }
            }

            flip_model();
        }

        // flips bones and vertices in x axis
        // flips uv vertically
        private void flip_model()
        {
            for (int i = 0; i < this.skeleton_nodes.Count; i++)
            {
                //this.skeleton_nodes[i].pos.X = this.skeleton_nodes[i].pos.X * -1;
            }

            for (int i = 0; i < this.vertices_list.Count; i++)
            {
                //this.vertices_list[i].pos.X = this.vertices_list[i].pos.X * -1;
                //this.vertices_list[i].norm.X = this.vertices_list[i].norm.X * -1;
                //this.vertices_list[i].norm.Z = this.vertices_list[i].norm.Z * -1;
                //this.vertices_list[i].norm.Y = this.vertices_list[i].norm.Y * -1;
                this.vertices_list[i].uv.Y = (-1 * (this.vertices_list[i].uv.Y)) - 1;
            }

        }




        public class material_group
        {
            public UInt32 triangle_start_index { get; set; }
            public UInt32 triangle_count { get; set; }
            public String material_name { get; set; }

            public material_group(UInt32 _tri_start_index, UInt32 _triangle_count, string _mat_name)
            {
                this.triangle_start_index = _tri_start_index;
                this.triangle_count = _triangle_count;
                this.material_name = _mat_name;
            }
        }

        #region SMD class definitions

        public class node
        {
            public int ID { get; set; }
            public int parent_ID { get; set; }
            public string bone_name { get; set; }

            public node(int _ID, int _parent_ID, string _bone_name)
            {
                this.ID = _ID;
                this.parent_ID = _parent_ID;
                this.bone_name = _bone_name;

            }
        }

        public class skeleton_node
        {
            public int ID { get; set; }
            public Vector3 pos { get; set; }
            public Vector3 rot { get; set; }

            public skeleton_node(string _ID, Vector3 _pos, Vector3 _rot)
            {
                this.ID = Convert.ToInt32(_ID);
                this.pos = _pos;
                this.rot = _rot;
            }
        }

        public class vertex
        {
            public vertex()
            {
                this.bone_weights = new List<bone_weight>();
            }

            public int parent_bone { get; set; }
            public Vector.Vector3 pos { get; set; }
            public Vector.Vector3 norm { get; set; }
            public Vector.Vector2 uv { get; set; }

            // number of bones weighted to this vert
            public int links { get; set; }
            public List<bone_weight> bone_weights { get; set; }
        }

        public struct bone_weight
        {
            public bone_weight(int _id, float _weight)
            {
                this.bone_ID = _id;
                this.weight = _weight;
            }

            public int bone_ID { get; set; }
            public float weight { get; set; }
        }

        public class triangle
        {
            public triangle()
            {
                this.verts = new List<vertex>();
            }

            public string mat_name { get; set; }
            public List<vertex> verts { get; set; }
        }

        #endregion
    }
}
