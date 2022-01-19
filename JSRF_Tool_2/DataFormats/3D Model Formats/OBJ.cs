using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JSRF_ModTool.Vector;

namespace JSRF_ModTool.DataFormats._3D_Model_Formats
{
    class OBJ
    {
        public string FilePath { get; set; }

        public List<obj_mesh> meshes { get; set; }
        public List<mtl_material> mtl_materials_list { get; set; }
        public string mtl_filepath { get; set; }

        public OBJ(string filepath)
        {
            this.FilePath = filepath;
            this.meshes = new List<obj_mesh>();
            this.mtl_materials_list = new List<mtl_material>();

            Import(filepath);
        }

        public void Import(string filepath)
        {
            if (!File.Exists(filepath))
            {
                System.Windows.MessageBox.Show("Model importer: " + Path.GetFileName(filepath) + " file does not exist");
                return;
            }

            this.FilePath = filepath;
            List<String> lines = new List<string>();
            string line;

            System.IO.StreamReader file = new System.IO.StreamReader(filepath);
            while ((line = file.ReadLine()) != null)
            {
                // add line to list, while removing double spaces and trim start/end
                lines.Add(Regex.Replace(line, @"\s+", " ").Trim());
            }

            obj_mesh mesh = new obj_mesh();
            string material_name = "";

            int f_count = 0; int f_cnt_mat = 0;
            string last_material_name = "";
            // for every line of the .obj
            for (int i = 0; i < lines.Count; i++)
            {
                string l = lines[i].ToLower();
                if (string.IsNullOrEmpty(l)) { continue; }

                #region vertex normals uvs

                // vertex
                if (l.StartsWith("v "))
                {
                    string[] v = l.Replace("v ", "").Split(' ');
                    mesh.vertex_buffer.Add(new Vector3(v[0], v[1], v[2]));
                    continue;
                }

                // vertex normals
                if (l.StartsWith("vn "))
                {
                    string[] v = l.Replace("vn ", "").Split(' ');
                    mesh.normals_buffer.Add(new Vector3(v[0], v[1], v[2]));
                    continue;
                }

                // vertex texture
                if (l.StartsWith("vt "))
                {
                    string[] v = l.Replace("vt ", "").Split(' ');
                    mesh.uv_buffer.Add(new Vector2(v[0], v[1]));
                    continue;
                }

                #endregion


                // if object
                if (l.StartsWith("mtllib "))
                {
                    mtl_filepath = "";
                    mtl_filepath = l.Split(' ')[1];
                    // check if file path is valid
                    if (!File.Exists(mtl_filepath))
                    {
                        mtl_filepath = Path.GetDirectoryName(filepath) + "\\" + mtl_filepath;
                        // if mtl_filepath is invalid, try using the obj's directory + mtl material name
                        if (!File.Exists(mtl_filepath))
                        {
                            System.Windows.Forms.MessageBox.Show("Error: could not find .mtl file named '" + mtl_filepath + "' in the obj file.");
                            return;
                        }
                    }



                    #region load .mtl materials file

                    // load material/texture infor from .mtl file
                    if (File.Exists(mtl_filepath))
                    {
                        List<string> mtl_lines = new List<String>();
                        System.IO.StreamReader mtl_file = new System.IO.StreamReader(mtl_filepath);
                        while ((line = mtl_file.ReadLine()) != null)
                        {
                            // add line to list, while removing double spaces and trim start/end
                            mtl_lines.Add(Regex.Replace(line, @"\s+", " ").Trim());
                        }

                        string mat_name = "";
                        for (int e = 0; e < mtl_lines.Count; e++)
                        {
                            String ll = mtl_lines[e];

                            if (ll.StartsWith("newmtl "))
                            {
                                mat_name = ll.Split(' ')[1];
                            }

                            if (ll.ToLower().StartsWith("map_kd "))
                            {
                                mtl_material mat = new mtl_material();
                                mat.material_name = mat_name;
                                mat.texture_path = ll.Split(' ')[1];
                                mtl_materials_list.Add(mat);
                            }
                        }
                    }

                    #endregion


                continue;
                }

                // if object
                if (l.StartsWith("o "))
                {
                    mesh = new obj_mesh();
                    mesh.name = l.Split('o')[1];
                    continue;
                }

                #region usemtl material // material groups indexing
                // material
                if (l.StartsWith("usemtl "))
                {
                    material_name = l.Replace("usemtl ", "");

                    if (mesh.material_groups.Count > 0)
                    {
                        // if we reach a face with a different triangle
                        // we create a new material
                        if (material_name != mesh.material_groups[mesh.material_groups.Count - 1].mat_name)
                        {
                            // if it's the first material group set the face count
                            if (mesh.material_groups.Count == 1)
                            {
                                mesh.material_groups[0].size = f_count;
                            }

                            mesh.material_groups[mesh.material_groups.Count - 1].ID = mesh.material_groups.Count - 1;
                            mesh.material_groups[mesh.material_groups.Count - 1].size = f_count - mesh.material_groups[mesh.material_groups.Count - 1].start_index;
                            mesh.material_groups.Add(new obj_mesh.material_group(material_name, 0, mesh.material_groups[mesh.material_groups.Count - 1].start_index + mesh.material_groups[mesh.material_groups.Count - 1].size));

                            continue;
                        }
                    }
                    else
                    {   // if there is no material in mat groups add current material name
                        mesh.material_groups.Add(new obj_mesh.material_group(material_name, 0, 0));
                        mesh.material_groups[mesh.material_groups.Count - 1].size++;
                        continue;
                    }
                    

                    last_material_name = material_name;
                    continue;
                }
                #endregion

                #region faces
                // faces
                if (l.StartsWith("f "))
                {
                    if(mesh.material_groups.Count > 0)
                    {
                        mesh.material_groups[mesh.material_groups.Count - 1].size++;
                    }
                    
                    f_count++; f_cnt_mat++;
                    string[] v = l.Replace("f ", "").Split(' ');

                    obj_mesh.face face = new obj_mesh.face();

                    #region return if face is not triangulated

                    if (v.Length > 3)
                    {
                        System.Windows.Forms.MessageBox.Show("Error: the OBJ mesh must be triangulated.");
                        return;
                    }

                    #endregion

                    #region for each triangle get-set triangle indices, uv, normal

                    int tri_count = 0;
                    obj_mesh.tri tri = new obj_mesh.tri();
                    // for each triangle/face index
                    for (int e = 0; e < v.Length; e++)
                    {
                        face = new obj_mesh.face();
                        // mesh.triangles_Vindices.Add(e);
                        string[] args = v[e].Split('/');
                        if (args.Length == 0)
                        {
                            mesh.face_indices.Add(Int32.Parse(v[e])); // face vert index
                            face.v = Int32.Parse(v[e]);
                        }

                        if (args.Length == 2)
                        {
                            mesh.face_indices.Add(Int32.Parse(args[0])); // face vert index
                            mesh.uv_indices.Add(Int32.Parse(args[1]));  //uv vertex index

                            face.v = Int32.Parse(args[0]);
                            face.vt = Int32.Parse(args[1]);
                        }

                        if (args.Length == 3)
                        {
                            // face vertex index
                            mesh.face_indices.Add(Int32.Parse(args[0]));
                            face.v = Int32.Parse(args[0]);

                            // UV: only add if UV is not empty
                            if (args[1] != "")
                            {
                                mesh.uv_indices.Add(Int32.Parse(args[1]));
                                face.vt = Int32.Parse(args[1]);
                            }
                            // vertex normals
                            mesh.normals_indices.Add(Int32.Parse(args[2]));
                            face.vn = Int32.Parse(args[2]);
                        }
                          

                        if (tri_count == 0)
                        {
                            tri.a = face;
                        }

                        if (tri_count == 1)
                        {
                            tri.b = face;
                        }

                        if (tri_count == 2)
                        {
                            tri.c = face;
                            mesh.triangles.Add(tri);
                            tri_count = 0;
                            tri = new obj_mesh.tri();
                        }
                        tri_count++;
                    }
                    #endregion
                    
                    continue;
                }

                #endregion
            }

            if(mesh.material_groups.Count > 0)
            {
                if (mesh.material_groups[mesh.material_groups.Count - 1].size == 0)
                {
                    mesh.material_groups[mesh.material_groups.Count - 1].size += f_cnt_mat + 1;
                }

                mesh.material_groups[mesh.material_groups.Count - 1].size++;
            }

            meshes.Add(mesh); 

        }

        // goes through triangle groups/materials and merges triangles using the same material into a single
        public void merge_triangles_by_material()
        {
        

            for (int i = 0; i < meshes.Count; i++)
            {
                obj_mesh mesh = meshes[i];
                List<shared_material_groups> shared_mat_groups = new List<shared_material_groups>();

                #region re-arrange material groups so that the groups sharing the same texture_id will be merged

                for (int m = 0; m < mesh.material_groups.Count; m++)
                {
                    bool already_index = false;
                    for (int g = 0; g < shared_mat_groups.Count; g++)
                    {
                        if(shared_mat_groups[g].texture_id == mesh.material_groups[m].texture_id) //if(shared_mat_groups[g].mat_name == mesh.material_groups[m].mat_name)
                        {
                            shared_mat_groups[g].mat_groups.Add(mesh.material_groups[m]);
                            already_index = true;
                        }
                    }

                    if(!already_index)
                    {
                        shared_mat_groups.Add(new shared_material_groups(mesh.material_groups[m].mat_name, mesh.material_groups[m].texture_id, mesh.material_groups[m]));
                    }
                }

                #endregion

                obj_mesh new_mesh = new obj_mesh();
                new_mesh = mesh;
                new_mesh.normals_indices = new List<int>();
                new_mesh.uv_indices = new List<int>();
                new_mesh.face_indices = new List<int>();
                new_mesh.material_groups = new List<obj_mesh.material_group>();
                int total_size = 0; int offset = 0;
                for (int s = 0; s < shared_mat_groups.Count; s++)
                {
   
                    
                    if(new_mesh.material_groups.Count > 0)
                    {
                        new_mesh.material_groups[new_mesh.material_groups.Count - 1].size += total_size;
                    }

                    new_mesh.material_groups.Add(new obj_mesh.material_group(shared_mat_groups[s].mat_name, shared_mat_groups[s].texture_id, offset));

                    total_size = 0;
                    int mat_group_size = 0;
                    for (int e = 0; e < shared_mat_groups[s].mat_groups.Count; e++)
                    {
                        obj_mesh.material_group mg = shared_mat_groups[s].mat_groups[e];

                        mat_group_size += mg.size;
                        // for reach triangle indice
                        for (int f = mg.start_index; f < (mg.start_index + mg.size); f++)
                        {
                            offset++;
                            total_size++;
                            //if (f / 2 >= mesh.triangles.Count) { break; }
                            new_mesh.face_indices.Add(mesh.triangles[f/2].a.v);
                            new_mesh.uv_indices.Add(mesh.triangles[f/2].a.vt);
                            new_mesh.normals_indices.Add(mesh.triangles[f/2].a.vn);

                            new_mesh.face_indices.Add(mesh.triangles[f/2].b.v);
                            new_mesh.uv_indices.Add(mesh.triangles[f/2].b.vt);
                            new_mesh.normals_indices.Add(mesh.triangles[f/2].b.vn);

                            new_mesh.face_indices.Add(mesh.triangles[f/2].c.v);
                            new_mesh.uv_indices.Add(mesh.triangles[f/2].c.vt);
                            new_mesh.normals_indices.Add(mesh.triangles[f/2].c.vn);
                        }

                    }

                    new_mesh.material_groups[new_mesh.material_groups.Count - 1].size = mat_group_size;
                }

                meshes[i] = new_mesh;

            }
        }

        private int IndexOf_Vector3(List<Vector3> list, Vector3 coords)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if(list[i] == coords)
                {
                    return i;
                }
            }

            return -1;
        }

        private int IndexOf_Vector2(List<Vector2> list, Vector2 coords)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == coords)
                {
                    return i;
                }
            }

            return -1;
        }



        private class shared_material_groups
        {
            public string mat_name { get; set; }
            public Int32 texture_id { get; set; }
            public List<obj_mesh.material_group> mat_groups { get; set; }

            public shared_material_groups(string _mat_name, Int32 _texture_id, obj_mesh.material_group _mat_groups)
            {
                if(this.mat_groups == null)
                {
                    this.mat_groups = new List<obj_mesh.material_group>();
                }
                this.texture_id = _texture_id;

                this.mat_name = _mat_name;
                this.mat_groups.Add(_mat_groups);
            }
        }   

        public class obj_mesh
        {
            public string name { get; set; }
            public List<Vector3> vertex_buffer { get; set; }
            public List<Vector3> normals_buffer { get; set; }
            public List<Vector2> uv_buffer { get; set; }
            public List<Int32> face_indices { get; set; }
            public List<Int32> uv_indices { get; set; }
            public List<Int32> normals_indices { get; set; }
            public List<material_group> material_groups { get; set; }
            public List<tri> triangles { get; set; }

            public class material_group
            {
                public Int32 start_index { get; set; }
                public Int32 size { get; set; }
                public string mat_name { get; set; }
                public Int32 texture_id { get; set; }
                public Int32 ID { get; set; }

                public material_group(string _name, int _texture_id, int _start_index)
                {
                    this.mat_name = _name;
                    this.texture_id = _texture_id;
                    this.start_index = _start_index;
                }
            }

            public class tri
            {
                public face a { get; set; }
                public face b { get; set; }
                public face c { get; set; }
            }

            public class face
            {
                public int v { get; set; }
                public int vt { get; set; }
                public int vn { get; set; }
            }


            public obj_mesh()
            {
                this.vertex_buffer = new List<Vector3>();
                this.normals_buffer = new List<Vector3>();
                this.uv_buffer = new List<Vector2>();

                this.face_indices = new List<Int32>();
                this.uv_indices = new List<Int32>();
                this.normals_indices = new List<Int32>();
                this.material_groups = new List<material_group>();
                this.triangles = new List<tri>();
            }
        }

        public class mtl_material
        {
            public string material_name { get; set; }
            public string texture_path { get; set; }
            public Int32 ID { get; set; }
        }

    }
}
