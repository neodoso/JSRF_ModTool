using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JSRF_ModTool.Vector;

namespace JSRF_ModTool.DataFormats._3D_Model_Formats
{
    class OBJ
    {
        public string filepath { get; set; }

        public obj_mesh mesh { get; set; }
        public List<mtl_material> mtl_materials_list { get; set; }
        public string mtl_filepath { get; set; }

        public bool imported_succeeded = false;

        public OBJ(string filepath)
        {
            this.filepath = filepath;
            this.mesh = new obj_mesh();
            this.mtl_materials_list = new List<mtl_material>();

            imported_succeeded = Import(filepath);
        }

        // TODO calculate processing time and see if it can be optimized
        // TODO instead of storing materials for each triangle, store material groups with starting triangle inxed to end triangle index
        public bool Import(string filepath)
        {
            #region read file

            if (!File.Exists(filepath))
            {
                throw new System.Exception(string.Format("OBJ importer: " + Path.GetFileName(filepath) + " file does not exist."));
            }

            this.filepath = filepath;
            List<String> lines = new List<string>();
            string line;

            // TODO try and handle execeptions for file streamreader
            System.IO.StreamReader file = new System.IO.StreamReader(filepath);


            while ((line = file.ReadLine()) != null)
            {
                // add line to list, while removing double spaces and trim start/end
                lines.Add(Regex.Replace(line, @"\s+", " ").Trim());
            }

            file.Close();
            file.Dispose();

            #endregion


            string material_name = "";
            int face_index = 0;

            // bounding box
            Vector.bounds bboxm = new Vector.bounds();
            bool mtllib_is_defined = false;

            // for every line of the .obj
            for (int i = 0; i < lines.Count; i++)
            {
                string l = lines[i]; // .ToLower();
                if (string.IsNullOrEmpty(l)) { continue; }

                #region vertex normals uvs

                // vertex
                if (l.StartsWith("v "))
                {
                    string[] v = l.Replace("v ", "").Split(' ');
                    Vector3 point = new Vector3(v[0], v[1], v[2]);
                    mesh.vertex_buffer.Add(point);
                    // sum point to calculate bounding box of the mesh
                    bboxm.add_point(point);
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

                #region mtllib

                // mtllib 
                if (l.StartsWith("mtllib "))
                {
                    mtllib_is_defined = true;
                    mtl_filepath = "";
                    mtl_filepath = l.Split(' ')[1];
                    // check if file path is valid
                    if (!File.Exists(mtl_filepath))
                    {
                        // if mtl_filepath is invalid, try using the obj's directory + mtl material name
                        mtl_filepath = Path.GetDirectoryName(filepath) + "\\" + mtl_filepath;
                        if (!File.Exists(mtl_filepath))
                        {
                            throw new System.Exception(string.Format("Error: OBJ importer could not find .mtl file named '" + Path.GetFileName(mtl_filepath) + "' in the obj file.\n\n" + mtl_filepath));
                        }
                    }

                    #region load .mtl materials file

                    // load material/texture infor from .mtl file
                    if (File.Exists(mtl_filepath))
                    {
                        List<string> mtl_lines = new List<String>();
                        System.IO.StreamReader mtl_file = new System.IO.StreamReader(mtl_filepath);
                        // parse all lines - clean up
                        while ((line = mtl_file.ReadLine()) != null)
                        {
                            // add line to list, while removing double spaces and trim start/end
                            mtl_lines.Add(Regex.Replace(line, @"\s+", " ").Trim());
                        }

                        mtl_file.Close();
                        mtl_file.Dispose();

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
                                mat.texture_filepath = (ll.ToLower().Replace("map_kd ", "")).Replace(@"\\", @"\");
                                mtl_materials_list.Add(mat);
                            }
                        }
                    }

                    #endregion

                    continue;
                }

                #endregion

                #region usemtl materials
                // material
                if (l.StartsWith("usemtl "))
                {
                    // if material library was not found
                    if (!mtllib_is_defined)
                    {
                        throw new System.Exception(string.Format("Error: the .obj file (" + Path.GetFileName(filepath) + ") doesn't have a material defined, please give it a material and make sure the .obj exports with a .mtl file.\n\n" + filepath));
                    }

                    material_name = l.Replace("usemtl ", "");
                    string texture_filepath = "";

    
                    // search material name in mtl_materials_list to get texture filename
                    for (int m = 0; m < mtl_materials_list.Count; m++)
                    {
                        if (mtl_materials_list[m].material_name == material_name)
                        {
                            material_name = Path.GetFileNameWithoutExtension(mtl_materials_list[m].texture_filepath);
                            //mtl_materials_list[m].material_name = Path.GetFileNameWithoutExtension(mtl_materials_list[m].texture_path);
                            texture_filepath = mtl_materials_list[m].texture_filepath; // Path.GetFileNameWithoutExtension(
                            break;
                        }
                    }
                    

                    // if mesh has material groups
                    if (mesh.material_groups.Count > 0)
                    {
                        bool mat_found = false;
                        // search in mesh material_groups to check if texture filename is already indexed
                        for (int e = 0; e < mesh.material_groups.Count; e++)
                        {
                            if(mesh.material_groups[e].texture_filepath == texture_filepath)
                            {
                                mesh.material_groups[e].mat_name = Path.GetFileNameWithoutExtension(texture_filepath);
                                mat_found = true;
                                break;
                            }
                        }


                        if (!mat_found)
                        {
                            mesh.material_groups.Add(new obj_mesh.material_group(Path.GetFileNameWithoutExtension(texture_filepath), texture_filepath, mesh.material_groups.Count, face_index, 0));   
                        }

                        // set material info, including the texture filename from the MTL material
                        //mesh.material_groups[mesh.material_groups.Count - 1].ID = mesh.material_groups.Count - 1;
                       // mesh.material_groups[mesh.material_groups.Count - 1].size = f_count - mesh.material_groups[mesh.material_groups.Count - 1].start_index;
                       // mesh.material_groups.Add(new obj_mesh.material_group(texture_filename, 0, mesh.material_groups[mesh.material_groups.Count - 1].start_index + mesh.material_groups[mesh.material_groups.Count - 1].size));

                        continue;

                    }
                    else
                    {   // if there is no material in mat groups add current material name
                        mesh.material_groups.Add(new obj_mesh.material_group(Path.GetFileNameWithoutExtension(texture_filepath), texture_filepath, 0, 0, 0));
                        //  mesh.material_groups[mesh.material_groups.Count - 1].size++;
                        continue;
                    }

                }
                #endregion

                #region faces
                // faces
                if (l.StartsWith("f "))
                {
                    if(mesh.material_groups.Count > 0)
                    {
                        //mesh.material_groups[mesh.material_groups.Count - 1].size++;
                    }
                    
                    string[] v = l.Replace("f ", "").Split(' ');

                    obj_mesh.face face = new obj_mesh.face(0,0,0);
                 
                    #region return if face is not triangulated

                    if (v.Length > 3)
                    {
                        throw new System.Exception(string.Format("Error: the OBJ mesh (" + Path.GetFileName(filepath) + ") must be triangulated.\n\n" + filepath));
                    }

                    #endregion

                    #region for each triangle get-set triangle indices, uv, normal

                    int tri_count = 0;
                    obj_mesh.tri tri = new obj_mesh.tri();
                    // for each triangle/face index
                    for (int e = 0; e < v.Length; e++)
                    {
                        face = new obj_mesh.face(0,0,0);
                        // mesh.triangles_Vindices.Add(e);
                        string[] args = new string[0];

                        /// if face is defined like so: f 1
                        /// there's no "uv(vt)" nor "normal(vn)"
                        if (!v[e].Contains("/"))
                        {
                            args = new string[] { v[0], v[1], v[2] };

                            mesh.face_indices.Add(Int32.Parse(v[0])); // face vert index
                            face.v = Int32.Parse(v[0]);
                            tri.a = new OBJ.obj_mesh.face(face.v, face.vt, face.vn);
                            mesh.materials.Add(material_name);

                            mesh.face_indices.Add(Int32.Parse(v[1])); // face vert index
                            face.v = Int32.Parse(v[1]);
                            tri.b = new OBJ.obj_mesh.face(face.v, face.vt, face.vn);
                            mesh.materials.Add(material_name);

                            mesh.face_indices.Add(Int32.Parse(v[2])); // face vert index
                            face.v = Int32.Parse(v[2]);
                            tri.c = new OBJ.obj_mesh.face(face.v, face.vt, face.vn);

                            mesh.materials.Add(material_name);

                            // set material
                            tri.mat_name = material_name;

                            mesh.triangles.Add(tri);
                            tri = new obj_mesh.tri();

                            e += 3;
                            if (e >= v.Length - 2) { break; }
                            continue;
                        } // if face is defined like so: f 1/2/3
                        else if(v[e].Contains("/"))
                        {
                            args = v[e].Split('/');

                            if (args.Length == 0)
                            {
                                mesh.face_indices.Add(Int32.Parse(v[e])); // face vert index
                                face.v = Int32.Parse(v[e]);
                                mesh.materials.Add(material_name);
                            }

                            if (args.Length == 2)
                            {
                                mesh.face_indices.Add(Int32.Parse(args[0])); // face vert index
                                mesh.uv_indices.Add(Int32.Parse(args[1]));  //uv vertex index

                                face.v = Int32.Parse(args[0]);
                                face.vt = Int32.Parse(args[1]);
                                mesh.materials.Add(material_name);
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

                                mesh.materials.Add(material_name);
                            }

                            // assign face data to tri.a b c depending on the vertex/triangle index
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

                                // set material
                                tri.mat_name = material_name;

                                mesh.triangles.Add(tri);
                                tri_count = 0;
                                tri = new obj_mesh.tri();
                            }

                            tri_count++;
                        }

                    }
                    #endregion

                    // add face count to last material_group
                    if (mesh.material_groups.Count > 0)
                    {
                        mesh.material_groups[mesh.material_groups.Count - 1].size++;
                    }

                    face_index++;

                    continue;
                }

                #endregion
            }

            #region set material_group texture filepath

            // for each material group
            for (int m = 0; m < mesh.material_groups.Count; m++)
            {
                // for each mtl material
                for (int i = 0; i < mtl_materials_list.Count; i++)
                {
                    // if mesh material name matches the material name in the mtl-materials
                    if (mesh.material_groups[m].mat_name == mtl_materials_list[i].material_name)
                    {
                        mesh.material_groups[m].texture_filepath = mtl_materials_list[i].texture_filepath;
                        break;
                    }
                }
            }

            #endregion

            // set bounding box (bbBox_A is lower, bbBox_B is higher)
            mesh.bbox_A = bboxm.bounding_box.A;
            mesh.bbox_B = bboxm.bounding_box.B;



           // sort_triangles();

            // group_triangles_by_material();
            /*
            if(!check_model_validity())
            {
                return false;
            }
            */

            return true;
        }

        public bool Export(string filepath, string object_name)
        {
            /*
            if (mesh.uv_buffer.Count == 0) { return false; }
            if (mesh.triangles.Count == 0) { return false; }

            List<string> lines = new List<string>();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            lines.Add("# JSRF ModTool" + version + " OBJ Exporter");
            lines.Add("");

            lines.Add("o " + object_name);
            lines.Add("");

            lines.Add("# Vertices (" + mesh.vertex_buffer.Count + ")");

            for (int i = 0; i < mesh.vertex_buffer.Count; i++)
            {
                Vector3 v = mesh.vertex_buffer[i];
                lines.Add("v " + v.X + " " + v.Y + " " + v.Z);
            }

            lines.Add("");
            lines.Add("# UVs coordinates (" + mesh.uv_buffer.Count + ")");

            for (int i = 0; i < mesh.uv_buffer.Count; i++)
            {
                Vector2 v = mesh.uv_buffer[i];
                lines.Add("vt " + v.X + " " + v.Y);
            }


            lines.Add("");
            lines.Add("# Triangles (" + mesh.triangles.Count + ")");

            for (int i = 0; i < mesh.triangles.Count; i++)
            {
                obj_mesh.tri t = mesh.triangles[i];
                lines.Add("f " + t.a);
                lines.Add("f " + t.b);
                lines.Add("f " + t.c);
            }

            try
            {
                System.IO.File.WriteAllLines(filepath, lines);
                return true;
            } 
            catch (Exception ex)
            {
                throw ex;
                return false;
            }

            
            */
            return false;
        }

        private bool check_model_validity()
        {
            if(mesh.uv_indices.Count != mesh.vertex_buffer.Count)
            {
                throw new System.Exception(string.Format("Error while importing OBJ ("+ Path.GetFileName(filepath) +") \n\nThe number of UVs and vertices is not equal." +
                                                                       "\n\nMake sure the mesh doesn't have vertices that share multiple UVs, mark edges as sharp and use the EdgeSplit modifier or disconnect edges so that vertices only have one UV per vertex."));
            }
            return true;
        }

        /*
        public void flip_model_for_Stage()
        {

            // TODO determine what is the proper scaling to flip the stage
           
            for (int i = 0; i < mesh.vertex_buffer.Count; i++)
            {
                float x = mesh.vertex_buffer[i].X;
                float y = mesh.vertex_buffer[i].Y;
                float z = mesh.vertex_buffer[i].Z;
                
                mesh.vertex_buffer[i].X = x;
                mesh.vertex_buffer[i].Y = y;
                mesh.vertex_buffer[i].Z = z;
            }
        }
        */


        #region OBJ Classes

        private class shared_material_groups
        {
            public string mat_name { get; set; }
            public Int32 texture_id { get; set; }
            public string texture_filepath { get; set; }
            public List<obj_mesh.material_group> mat_groups { get; set; } = new List<obj_mesh.material_group>();

            public shared_material_groups(string _mat_name, Int32 _texture_id, string _texture_filepath, obj_mesh.material_group _mat_groups)
            {

                this.texture_id = _texture_id;
                this.texture_filepath = _texture_filepath;

                this.mat_name = _mat_name;
                this.mat_groups.Add(_mat_groups);
            }
        }   

        public class obj_mesh
        {
            public string name                  { get; set; }
            public List<Vector3> vertex_buffer  { get; set; }           = new List<Vector3>();
            public List<Vector3> normals_buffer { get; set; }           = new List<Vector3>();
            public List<Vector2> uv_buffer      { get; set; }           = new List<Vector2>();
            public List<Int32> face_indices     { get; set; }           = new List<Int32>();
            public List<Int32> uv_indices       { get; set; }           = new List<Int32>();
            public List<Int32> normals_indices  { get; set; }           = new List<Int32>();
            public List<material_group> material_groups { get; set; }   = new List<material_group>();
            public List<String> materials       { get; set; }           = new List<String>();
            public List<tri> triangles          { get; set; }           = new List<tri>();

            public Vector3 bbox_A               { get; set; }           = new Vector3();
            public Vector3 bbox_B               { get; set; }           = new Vector3();

            public obj_mesh()
            {
            }

            public class material_group
            {
                public Int32 start_index { get; set; }
                public Int32 size { get; set; }
                public string mat_name { get; set; }
                public Int32 texture_id { get; set; }
                public string texture_filepath { get; set; }
                public Int32 ID { get; set; }

                public material_group(string _name, string _texture_filepath, int _texture_id, int _start_index = 0, int _size = 0)
                {
                    this.mat_name = _name;
                    this.texture_filepath = _texture_filepath;
                    this.texture_id = _texture_id;
                    this.start_index = _start_index;
                    this.size = _size;
                }
            }

            public class tri
            {
                public face a { get; set; }
                public face b { get; set; }
                public face c { get; set; }

                public string mat_name { get; set; }
            }

            public class face
            {
                public int v  { get; set; }
                public int vt { get; set; }
                public int vn { get; set; }

                public face(int _v, int _vt, int _vn)
                {
                    v =  _v;
                    vt = _vt;
                    vn = _vn;
                }
            }
        }

        public class mtl_material
        {
            public string material_name { get; set; }
            public string texture_filepath { get; set; }
            public Int32 ID { get; set; }
        }

        #endregion

    }
}
