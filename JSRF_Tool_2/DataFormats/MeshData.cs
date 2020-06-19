using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
namespace JSRF_Tool_2.DataFormats
{
    /// <summary>
    /// custom XALM model class
    /// </summary>
    public class MeshData
    {
        public List<bone_weight> bone_weights = new List<bone_weight>();
        public Point3DCollection vertices = new Point3DCollection();
        public Vector3DCollection normals = new Vector3DCollection();
        public PointCollection UVs = new PointCollection();
        public Int32Collection triangles = new Int32Collection();

        public Point3D mesh_center = new Point3D(0, 0, 0);
        public Point3D mesh_bounds = new Point3D(0, 0, 0);
        public Point3D avg_distance = new Point3D(0, 0, 0);

        public Int32 texture_id = 0;

        public class bone_weight
        {
            public byte bone_id_0 { get; set; }
            public byte bone_id_1 { get; set; }
            public float bone_0_weight { get; set; }
            public float bone_1_weight { get; set; }

            public bone_weight(byte _bid0, byte _bid1, float _bw0, float _bw1)
            {
                this.bone_id_0 = _bid0;
                this.bone_id_1 = _bid1;
                this.bone_0_weight = _bw0;
                this.bone_1_weight = _bw1;
            }
        }
    }
}
