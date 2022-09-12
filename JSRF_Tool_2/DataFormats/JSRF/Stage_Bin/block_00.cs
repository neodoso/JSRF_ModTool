using JSRF_ModTool.Functions;
using JSRF_ModTool.Vector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JSRF_ModTool.DataFormats.JSRF.Stage_Bin
{
    public class block_00
    {
		public class collision_models_list  // (32 bytes)
		{
			public Vector3 bbox_A { get; set; }
			public Vector3 bbox_B { get; set; }

			public int models_list_start_offset { get; set; }
			public int models_list_count { get; set; }

			public List<collision_model> coll_model_list { get; set; }

			public collision_models_list()
			{
				coll_model_list = new List<collision_model>();
			}
		}

		public class collision_model  // (112 bytes)
		{
			public Vector3 bbox_A { get; set; }
			public Vector3 bbox_B { get; set; }

			// v3 v4 v5 are a transform matrix
			public Vector3 v3 { get; set; }
			public float w3 { get; set; }

			public Vector3 v4 { get; set; }
			public float w4 { get; set; }

			public Vector3 v5 { get; set; }
			public float w5 { get; set; }

			public Vector3 v7_position { get; set; }

			public float f { get; set; }

			public int vertices_start_offset { get; set; }
			public int vertices_count { get; set; }

			public int triangles_start_offset { get; set; }
			public int triangle_count { get; set; }

			public int unk_104 { get; set; }
			public int unk_108 { get; set; }

			public List<coll_vertex> vertex_list { get; set; }
			public List<coll_triangle> triangles_list { get; set; }

			public collision_model()
			{
				vertex_list = new List<coll_vertex>();
				triangles_list = new List<coll_triangle>();
			}
		}

		public struct coll_vertex
		{
			public Vector3 vert { get; set; }

			public int unk { get; set; }
		}

		public class AABB
		{
			public Vector3 min { get; set; } = new Vector3();
			public Vector3 max { get; set; } = new Vector3();

			public static AABB calculate_triangle_aabb(Vector3 v1, Vector3 v2, Vector3 v3)
			{
				AABB ret = new AABB();
				ret.min.X = System.Math.Min(v1.X, System.Math.Min(v2.X, v3.X));
				ret.min.Y = System.Math.Min(v1.Y, System.Math.Min(v2.Y, v3.Y));
				ret.min.Z = System.Math.Min(v1.Z, System.Math.Min(v2.Z, v3.Z));

				ret.max.X = System.Math.Max(v1.X, System.Math.Max(v2.X, v3.X));
				ret.max.Y = System.Math.Max(v1.Y, System.Math.Max(v2.Y, v3.Y));
				ret.max.Z = System.Math.Max(v1.Z, System.Math.Max(v2.Z, v3.Z));
				return ret;
			}

			public override bool Equals(System.Object obj)
			{
				if ((obj == null) || !this.GetType().Equals(obj.GetType()))
				{
					return false;
				}
				AABB other = (AABB)obj;
				return min.X == other.min.X && min.Y == other.min.Y && min.Z == other.min.Z && max.X == other.max.X && max.Y == other.max.Y && max.Z == other.max.Z;
			}
		}


		public class coll_triangle
		{
			public coll_triangle(raw in_raw)
			{
				raw_data = in_raw;
			}

			public struct raw
			{
				public System.UInt32 indices_raw { get; set; }

				// Bounds indices and surface flags
				public System.UInt32 other_data { get; set; }
			}


			private raw raw_data;

			// Member access 
			public System.UInt32 index1
			{
				get { return raw_data.indices_raw & 0x3ffU; }
				set { SafelyCheckIndex(value); raw_data.indices_raw &= ~0x3ffU; raw_data.indices_raw |= value; }
			}
			public System.UInt32 index2
			{
				get { return (raw_data.indices_raw >> 10) & 0x3ffU; }
				set { SafelyCheckIndex(value); raw_data.indices_raw &= ~(0x3ffU << 10); raw_data.indices_raw |= (value << 10); }
			}
			public System.UInt32 index3
			{
				get { return (raw_data.indices_raw >> 20) & 0x3ffU; }
				set { SafelyCheckIndex(value); raw_data.indices_raw &= ~(0x3ffU << 20); raw_data.indices_raw |= (value << 20); }
			}
			public System.UInt32 indices_raw_leftover
			{
				get { return (raw_data.indices_raw >> 30) & 0x3U; }
				set { SafelyCheckInteger(value, 2); raw_data.indices_raw &= ~(0x3U << 30); raw_data.indices_raw |= (value << 30); }
			}

			public System.UInt16 surface_properties
			{
				get { return (System.UInt16)(raw_data.other_data & 0xffffU); }
				set { raw_data.other_data &= ~0xffffU; raw_data.other_data |= value; }
			}

			public System.Byte index_of_vertex_with_min_x
			{
				get { return SafelyGetBoundsIndex((raw_data.other_data >> 16) & 3U); }
				set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 16); raw_data.other_data |= ((value & 0x3U) << 16); }
			}

			public System.Byte index_of_vertex_with_max_x
			{
				get { return SafelyGetBoundsIndex((raw_data.other_data >> 18) & 3U); }
				set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 18); raw_data.other_data |= ((value & 0x3U) << 18); }
			}
			public System.Byte index_of_vertex_with_min_y
			{
				get { return SafelyGetBoundsIndex((raw_data.other_data >> 20) & 3U); }
				set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 20); raw_data.other_data |= ((value & 0x3U) << 20); }
			}
			public System.Byte index_of_vertex_with_max_y
			{
				get { return SafelyGetBoundsIndex((raw_data.other_data >> 22) & 3U); }
				set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 22); raw_data.other_data |= ((value & 0x3U) << 22); }
			}
			public System.Byte index_of_vertex_with_min_z
			{
				get { return SafelyGetBoundsIndex((raw_data.other_data >> 24) & 3U); }
				set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 24); raw_data.other_data |= ((value & 0x3U) << 24); }
			}
			public System.Byte index_of_vertex_with_max_z
			{
				get { return SafelyGetBoundsIndex((raw_data.other_data >> 26) & 3U); }
				set { SafelyCheckBoundsIndex(value); raw_data.other_data &= ~(0x3U << 26); raw_data.other_data |= ((value & 0x3U) << 26); }
			}
			public System.Byte other_data_leftover
			{
				get { return SafelyGetBoundsIndex((raw_data.other_data >> 28) & 3U); }
				set { SafelyCheckInteger(value, 4); raw_data.other_data &= ~(0xfU << 28); raw_data.other_data |= ((value & 0xfU) << 28); }
			}

			// Helpers

			public System.UInt32 GetVertexIndex(System.Byte index)
			{
				SafelyCheckBoundsIndex(index);
				int shift = 10 * index;
				return (raw_data.indices_raw >> shift) & 0x3ffU;
			}

			public static System.Byte SafelyGetBoundsIndex(System.UInt32 value)
			{
				SafelyCheckBoundsIndex((System.Byte)value);
				return (System.Byte)value;
			}
			public static void SafelyCheckInteger(System.UInt32 value, System.UInt32 bit_size)
			{
				var limit = (1U << (int)bit_size) - 1;
				if (value > limit)
				{
					throw new System.IndexOutOfRangeException(string.Format("{0} is > {1}", value, limit));
				}
			}

			public static void SafelyCheckBoundsIndex(System.Byte value)
			{
				if (value > 2)
				{
					throw new System.IndexOutOfRangeException("Index can only be 0, 1 or 2. Triangle has only three indices");
				}
			}

			public static void SafelyCheckIndex(System.UInt32 value)
			{
				if (value > 0x3ffU)
				{
					throw new System.IndexOutOfRangeException(string.Format("Index can be max 1023, but was {0}", value));
				}
			}

			public string ToTableRow()
			{

				return string.Format("{0,6} {1,6} {2,6} {3,8} {4,8} {5,8} {6,8} {7,8} {8,8} {9,9}", index1, index2, index3, index_of_vertex_with_min_x,
					index_of_vertex_with_max_x, index_of_vertex_with_min_y, index_of_vertex_with_max_y, index_of_vertex_with_min_z, index_of_vertex_with_max_z, surface_properties);
			}

			// surface property type, collision mesh triangles get tagged with one of these
			// to define what type of surface it is and how the player controller behaves when making contact / colliding
			public class surface_properties_list
			{
				public List<surfprop_item> items { get; set; }

				public surface_properties_list()
				{
					this.items = new List<surfprop_item>();
					items.Add(new surfprop_item(0,    "surfprop_pass_through",  "90 45 109"));
					items.Add(new surfprop_item(1,    "surfprop_floor",			"65 138 214"));
					items.Add(new surfprop_item(2,	  "surfprop_unknown_2",		"126 147 147"));
					items.Add(new surfprop_item(4,    "surfprop_wall",			"214 138 65"));
					items.Add(new surfprop_item(8,	  "surfprop_unknown_8",		"126 147 147"));
					items.Add(new surfprop_item(16,	  "surfprop_stairs",		"218 225 40"));
					items.Add(new surfprop_item(32,   "surfprop_billboard",		"44 225 40"));
					items.Add(new surfprop_item(128,  "surfprop_halfpipe",		"23 23 132"));
					items.Add(new surfprop_item(256,  "surfprop_ceiling",		"0 254 232"));
					items.Add(new surfprop_item(512,  "surfprop_untouchable",	"225 40 40"));
					//items.Add(new surfprop_item(1024, "surfprop_unknown_1024", "90 45 109"));
					items.Add(new surfprop_item(8192, "surfprop_ramp",			"205 40 225"));
				}

				public struct surfprop_item
				{
					public int num { get; set; }
					public string name { get; set; }
					public string color { get; set; }

					public surfprop_item(int _num, string _name, string _color)
					{
						this.num = _num;
						this.name = _name;
						this.color = _color;
					}
				}

				public string get_Name_byID(int ID)
				{
					switch (ID)
					{
						case 0:
							return "surfprop_passthrough";

						case 1:
							return "surfprop_floor";

						case 2:
							return "surfprop_unknown_2";

						case 4:
							return "surfprop_wall";
						case 8:
							return "surfprop_unknown_8";

						case 16:
							return "surfprop_stairs";

						case 32:
							return "surfprop_billboard";

						case 128:
							return "surfprop_halfpipe";

						case 256:
							return "surfprop_ceiling";

						case 512:
							return "surfprop_untouchable";

						case 8192:
							return "surfprop_ramp";
					}

					return "unknown";
				}
			}
		}

		public int coll_headers_A_offset { get; set; }
		public int coll_headers_A_chunk_count { get; set; }

		public int unk_08 { get; set; }
		public int unk_12 { get; set; }

		public List<collision_models_list> block_A_headers_list { get; set; }


		#region debug

		public void export_all_collision_meshes(string dir)
		{
			Directory.CreateDirectory(dir);
			List<String> transform_lines = new List<string>();

			coll_triangle.surface_properties_list SurfProps = new coll_triangle.surface_properties_list();

			// for each collision model: read vertex buffer and export collision data to text files
			for (int i = 0; i < this.block_A_headers_list.Count; i++) //block_00.block_A_headers_list.Count
			{
				// for each coll_model_header header for this block_A_header item
				for (int j = 0; j < this.block_A_headers_list[i].coll_model_list.Count; j++)
				{
					block_00.collision_model coll_head = this.block_A_headers_list[i].coll_model_list[j];

					transform_lines = new List<string>();
					transform_lines.Add(coll_head.v7_position.X + " " + coll_head.v7_position.Y + " " + coll_head.v7_position.Z);

					transform_lines.Add(coll_head.v3.X + " " + coll_head.v3.Y + " " + coll_head.v3.Z);
					transform_lines.Add(coll_head.v4.X + " " + coll_head.v4.Y + " " + coll_head.v4.Z);
					transform_lines.Add(coll_head.v5.X + " " + coll_head.v5.Y + " " + coll_head.v5.Z);

					List<string> obj_lines = new List<string>();
					List<string> mtl_lines = new List<string>();

					obj_lines.Add("mtllib " + "coll_" + i + "_" + j + ".mtl");
					obj_lines.Add("");

					obj_lines.Add("o " + "coll_" + i + "_" + j);
					obj_lines.Add("");


					// for each vertex in this group
					for (int v = 0; v < coll_head.vertex_list.Count; v++)
					{
						block_00.coll_vertex vert = coll_head.vertex_list[v];

						obj_lines.Add("v " + vert.vert.X + " " + vert.vert.Y + " " + vert.vert.Z);
					}

					obj_lines.Add("");

					// previous surface_property_number
					ushort prev_surface_property_num = 0;
					coll_triangle.surface_properties_list sp = new coll_triangle.surface_properties_list();
					List<string> mat_names = new List<string>();
					string surfprop_name = "default_material";

					// for each triangle
					for (int t = 0; t < coll_head.triangles_list.Count; t++)
					{
						block_00.coll_triangle tri = coll_head.triangles_list[t];

						// if previous surface_property != current triangle's surface_property
						if (prev_surface_property_num != tri.surface_properties)
						{
							prev_surface_property_num = tri.surface_properties;
							surfprop_name = SurfProps.get_Name_byID(tri.surface_properties);
							string color = "128 128 128";

							// search surface_property by ID in sp.items aka: (coll_triangle.surface_properties_list)
							// and get surface property name and color
							for (int s = 0; s < sp.items.Count; s++)
							{
								if (sp.items[s].num == tri.surface_properties)
								{
									surfprop_name = sp.items[s].name;
									color = sp.items[s].color;
									break;
								}
							}

							// check if material is already registered in mtl
							bool mat_already_registered = false;
							foreach (string item in mat_names)
							{
								if (item.Contains(surfprop_name))
                                {
									mat_already_registered = true;
									break;
								}
							}

							// if material hasn't been yet registered, add to .mtl
							if (!mat_already_registered)
							{
								string[] c = color.Split(' ');

								// write mtl material
								mtl_lines.Add("newmtl " + surfprop_name);
								mtl_lines.Add("Ka 0.3 0.3 0.3");
								mtl_lines.Add("Kd " + Math.Round((float.Parse(c[0]) / 255f), 6) + " " + Math.Round((float.Parse(c[1]) / 255f), 6) + " " + Math.Round((float.Parse(c[2]) / 255f), 6));
								mtl_lines.Add("Ks 0.1 0.1 0.1");
								mtl_lines.Add("Ns 50");
								mtl_lines.Add("Tr 0");
								mtl_lines.Add("illum 2");
								mtl_lines.Add(@"map_Kd C:\Users\Mike\Desktop\JSRF\Stg_Demo_Assets\dev\" + surfprop_name + ".png");
								mtl_lines.Add("illum 2");
								mtl_lines.Add("");

								mat_names.Add(surfprop_name);
							}
						}

						// write material used by triangle
						obj_lines.Add("usemtl "+ surfprop_name);
						// write triangle
						obj_lines.Add("f " + (tri.index1 + 1) + " " + (tri.index2 + 1) + " " + (tri.index3 + 1));
					}

					// export stage model

					System.IO.File.Delete(dir + "coll_" + i + "_" + j + ".mtl");
					System.IO.File.AppendAllLines(dir + "coll_" + i + "_" + j + ".mtl", mtl_lines);

					System.IO.File.Delete(dir + "coll_" + i + "_" + j + ".obj");
					System.IO.File.AppendAllLines(dir + "coll_" + i + "_" + j + ".obj", obj_lines);

					///transform_lines.Add("scl:" + coll_head. + " " + coll_head.v7_position.Y + " " + coll_head.v7_position.Z);
					System.IO.File.WriteAllLines(dir + "coll_" + i + "_" + j + ".xyz", transform_lines);
				}
			}
		}


		public void export_all_collision_meshes_Gurten(string dir)
		{
			Directory.CreateDirectory(dir);
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			for (int i = 0; i < block_A_headers_list.Count; i++)
			{
				for (int j = 0; j < block_A_headers_list[i].coll_model_list.Count; j++)
				{
					collision_model collision_model = block_A_headers_list[i].coll_model_list[j];
					list2 = new List<string>();
					list2.Add(collision_model.v7_position.X + " " + collision_model.v7_position.Y + " " + collision_model.v7_position.Z);
					list2.Add(collision_model.v3.X + " " + collision_model.v3.Y + " " + collision_model.v3.Z);
					list2.Add(collision_model.v4.X + " " + collision_model.v4.Y + " " + collision_model.v4.Z);
					list2.Add(collision_model.v5.X + " " + collision_model.v5.Y + " " + collision_model.v5.Z);
					List<string> list3 = new List<string>();
					list3.Add("o coll_" + i + "_" + j);
					list3.Add("");
					for (int k = 0; k < collision_model.vertex_list.Count; k++)
					{
						coll_vertex coll_vertex = collision_model.vertex_list[k];
						list3.Add("v " + coll_vertex.vert.X + " " + coll_vertex.vert.Y + " " + coll_vertex.vert.Z);
					}
					list3.Add("");
					for (int l = 0; l < collision_model.triangles_list.Count; l++)
					{
						coll_triangle coll_triangle = collision_model.triangles_list[l];
						list3.Add("f " + (coll_triangle.index1 + 1) + " " + (coll_triangle.index2 + 1) + " " + (coll_triangle.index3 + 1));
					}
					File.Delete(dir + "coll_" + i + "_" + j + ".obj");
					File.AppendAllLines(dir + "coll_" + i + "_" + j + ".obj", list3);
					File.WriteAllLines(dir + "coll_" + i + "_" + j + ".xyz", list2);
				}
			}
		}

		public void export_single_coll_mesh(string dir, int a, int b)
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			for (int i = a; i < a + 1; i++)
			{
				for (int j = b; j < b + 1; j++)
				{
					collision_model collision_model = block_A_headers_list[i].coll_model_list[j];
					list2 = new List<string>();
					list2.Add(collision_model.v7_position.X + " " + collision_model.v7_position.Y + " " + collision_model.v7_position.Z);
					list2.Add(collision_model.v3.X + " " + collision_model.v3.Y + " " + collision_model.v3.Z);
					list2.Add(collision_model.v4.X + " " + collision_model.v4.Y + " " + collision_model.v4.Z);
					list2.Add(collision_model.v5.X + " " + collision_model.v5.Y + " " + collision_model.v5.Z);
					List<string> list3 = new List<string>();
					list3.Add("o coll_" + i + "_" + j);
					list3.Add("");
					for (int k = 0; k < collision_model.vertex_list.Count; k++)
					{
						coll_vertex coll_vertex = collision_model.vertex_list[k];
						list3.Add("v " + coll_vertex.vert.X + " " + coll_vertex.vert.Y + " " + coll_vertex.vert.Z);
					}
					list3.Add("");
					for (int l = 0; l < collision_model.triangles_list.Count; l++)
					{
						coll_triangle coll_triangle = collision_model.triangles_list[l];
						list3.Add("f " + (coll_triangle.index1 + 1) + " " + (coll_triangle.index2 + 1) + " " + (coll_triangle.index3 + 1));
					}
					File.Delete(dir + "coll_" + i + "_" + j + ".obj");
					File.AppendAllLines(dir + "coll_" + i + "_" + j + ".obj", list3);
					File.WriteAllLines(dir + "coll_" + i + "_" + j + ".xyz", list2);
				}
			}
		}

		public void verify_geometry()
		{
			for (int i = 0; i < block_A_headers_list.Count; i++)
			{
				for (int j = 0; j < block_A_headers_list[i].coll_model_list.Count; j++)
				{
					collision_model collision_model = block_A_headers_list[i].coll_model_list[j];
					bool has_errors = false;
					for (int l = 0; l < collision_model.triangles_list.Count; l++)
					{
						coll_triangle coll_triangle = collision_model.triangles_list[l];

						AABB uncompressed_aabb = new AABB();
						uncompressed_aabb.min.X = collision_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_min_x)].vert.X;
						uncompressed_aabb.min.Y = collision_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_min_y)].vert.Y;
						uncompressed_aabb.min.Z = collision_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_min_z)].vert.Z;
						uncompressed_aabb.max.X = collision_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_max_x)].vert.X;
						uncompressed_aabb.max.Y = collision_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_max_y)].vert.Y;
						uncompressed_aabb.max.Z = collision_model.vertex_list[(int)coll_triangle.GetVertexIndex(coll_triangle.index_of_vertex_with_max_z)].vert.Z;

						if (!uncompressed_aabb.Equals(AABB.calculate_triangle_aabb(collision_model.vertex_list[(int)coll_triangle.index1].vert, collision_model.vertex_list[(int)coll_triangle.index2].vert, collision_model.vertex_list[(int)coll_triangle.index3].vert)))
						{
							//throw new System.Exception();
							System.Console.WriteLine(string.Format("Triangle {0}, Model {1}, Header {2} has incorrect data.", l, j, i));
							has_errors = true;
							break;
						}
					}
					if (!has_errors && collision_model.triangles_list.Count > 0)
					{
						System.Console.WriteLine(string.Format("Model {0}, Header {1} passed geometry check.", j, i));
					}
					else if (collision_model.triangles_list.Count == 0)
					{
						System.Console.WriteLine(string.Format("Model {0}, Header {1} has no geometry.", j, i));
					}
				}
			}
		}

        #endregion
    }
}
