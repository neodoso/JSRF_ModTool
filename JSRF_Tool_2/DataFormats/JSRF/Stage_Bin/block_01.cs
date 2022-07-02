using JSRF_ModTool.Functions;
using JSRF_ModTool.Vector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JSRF_ModTool.DataFormats.JSRF.Stage_Bin
{
    public class block_01
    {
        // header
        public Int32 unk_id { get; set; }
        // 8192 bytes list of [8 bytes blocks] of item count + start offset (1024 items slots)
        public List<item_header> items { get; set; }


        public block_01(byte[] data)
        {
            unk_id = BitConverter.ToInt32(data, 0);
            items = new List<item_header>();

            for (int i = 4; i < 8196; i += 8) // 1024 items max
            {
                items.Add(new item_header(data, BitConverter.ToInt32(data, i), BitConverter.ToInt32(data, i + 4)));
                //items[this.items.Count - 1].items_size = items[this.items.Count - 1].items_count * 4;
            }
        }

        /// <summary>
        /// item_header is a list of [ [item count] and [start offset] ]
        /// each item_header
        /// </summary>
        public class item_header
        {
            /// <summary>
            /// file structure:
            /// [int32] item count
            /// [int32] start offset

            public Int32 start_offset { get; set; }
            public Int32 items_count { get; set; } // doesn't seen to match the item size/count between this.start_offset and next.item_header.start_offset
            // total items size = items_count * 4

            // offsets of grind_path objects
            //public List<Int32> grind_path_headers_offsets_List { get; set; }
            public List<grind_path_header> grind_path_header_List { get; set; }

            public item_header(byte[] data, Int32 _start_offset, Int32 _items_count)
            {
                this.start_offset = _start_offset;
                this.items_count = _items_count;
                grind_path_header_List = new List<grind_path_header>();

                if(this.items_count == 0)
                {
                    return;
                }

                // for each grind_path_header pointer
                for (int i = 0; i < this.items_count; i++)
                {
                    // load binary data to grind_path_header class intance
                    block_01.item_header.grind_path_header grind_path_head = (block_01.item_header.grind_path_header)(Parsing.binary_to_struct(data, BitConverter.ToInt32(data, this.start_offset + i * 4), typeof(block_01.item_header.grind_path_header)));
                    grind_path_head.grind_path_points = new List<grind_path_header.grind_path_point>();
                    // read each grind path point and add it to grind_path_head.grind_path_points[] list
                    for (int p = 0; p < grind_path_head.grind_points_count; p++)
                    {
                        // read from binary and Vector3 position and Vector3 normal
                        grind_path_header.grind_path_point point = new grind_path_header.grind_path_point((Vector3)Parsing.binary_to_struct(data, grind_path_head.grind_points_list_start_offset + p * 24, typeof(Vector3)), (Vector3)Parsing.binary_to_struct(data, grind_path_head.grind_points_list_start_offset + p * 24 + 12, typeof(Vector3)));

                        grind_path_head.grind_path_points.Add(point);
                    }

                    // add  grind_path_header to list
                    grind_path_header_List.Add(grind_path_head);
                }
            }


            public class grind_path_header
            {
                public Int32 grind_points_list_start_offset { get; set; }
                public Int32 grind_points_count { get; set; }
                public Int16 flag_A { get; set; }
                public Int16 flag_B { get; set; }
                public Vector3 bbox_A { get; set; } // bounding box point A
                public Vector3 bbox_B { get; set; } // bounding box point B

                public List<grind_path_point> grind_path_points { get; set; }

                public class grind_path_point
                {
                    public Vector3 position { get; set; } // point position
                    public Vector3 normal { get; set; } // point orientation

                    public grind_path_point(Vector3 _pos, Vector3 _norm)
                    {
                        this.position = _pos;
                        this.normal = _norm;
                    }
                }
            }
        }

        public void export_data_struct_info(string filepath)
        {
            List<string> lines = new List<string>();
            int empty_space_counter = 0;
            int offset = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].items_count > 0)
                {
                    if(empty_space_counter > 0)
                    {
                        lines.Add("");
                        lines.Add("+" + empty_space_counter + " bytes of empty slots");
                        lines.Add("");
                        empty_space_counter = 0;
                    }
                   

                    int size = 0;
                    if (i + 1 < items.Count)
                    {
                        size = items[i + 1].start_offset - items[i].start_offset;
                    }
                    if (size == 0)
                    {
                        lines.Add(items[i].start_offset + " " + items[i].items_count);
                    }
                    else
                    {
                        lines.Add(items[i].start_offset + " " + items[i].items_count + " " + size);
                    }
                } else {
                    empty_space_counter += 8;
                }
                offset += 8;
            }

            System.IO.File.WriteAllLines(filepath, lines);
        }


        public void export_grind_path_data(string filepath)
        {
            List<string> lines = new List<string>();

            int item_count = 0;
            int grind_path_item_count = 0;

            List<Int32> grind_points_list_offset = new List<Int32>();

            foreach (var item in items)
            {
                if (item.items_count == 0) { continue; }
                //lines.Add("Grind Path Group [" + item_count + "]");
                // create model object
                foreach (var grind_path_item in item.grind_path_header_List)
                {
                    // if grind points list has already been exported, skip
                    if (grind_points_list_offset.Contains(grind_path_item.grind_points_list_start_offset))
                    {
                        continue;
                    }

                    grind_points_list_offset.Add(grind_path_item.grind_points_list_start_offset);

                    lines.Add("[" + item_count + ":" + grind_path_item_count + ":" + grind_path_item.flag_A + " " + grind_path_item.flag_B + "]"); //Grind Path SubGroup

                    // creatre point
                    foreach (var points in grind_path_item.grind_path_points)
                    {
                        lines.Add(points.position.X + " " + points.position.Y + " " + points.position.Z + " " + points.normal.X + " " + points.normal.Y + " " + points.normal.Z);
                    }
                    grind_path_item_count++;

                    lines.Add("end");
                }
                grind_path_item_count = 0;
                item_count++;
            }

            System.IO.File.WriteAllLines(filepath, lines);

        }

        public void export_grind_path_data_blender(string filepath)
        {
            List<string> lines = new List<string>();

            int item_count = 0;
            int grind_path_item_count = 0;

            List<Int32> grind_points_list_offset = new List<Int32>();
            int line_point_index = 0;
            foreach (var item in items)
            {
                if (item.items_count > 0)
                {
                    //lines.Add("Grind Path Group [" + item_count + "]");
                    // create model object
                    //  for (int i = 0; i < length; i++)
                    //foreach (var grind_path_item in item.grind_path_header_List)
                    for (int f = 0; f < item.grind_path_header_List.Count; f++)
                    {
                        item_header.grind_path_header grind_path_item = item.grind_path_header_List[f];

                        // if grind points list has already been exported, skip
                        if (grind_points_list_offset.Contains(grind_path_item.grind_points_list_start_offset))
                        {
                            continue;
                        }

                        grind_points_list_offset.Add(grind_path_item.grind_points_list_start_offset);

                        lines.Add("o gp_" + item_count + "_" + grind_path_item_count); //Grind Path SubGroup 
                        lines.Add("");
                        // creatre point
                        foreach (var points in grind_path_item.grind_path_points)
                        {
                            decimal px = Decimal.Parse(points.position.X.ToString(), System.Globalization.NumberStyles.Any);
                            decimal py = Decimal.Parse(points.position.Y.ToString(), System.Globalization.NumberStyles.Any);
                            decimal pz = Decimal.Parse(points.position.Z.ToString(), System.Globalization.NumberStyles.Any);

                            lines.Add("v " + px + " " + py + " " + pz);
                        }



                        for (int i = 0; i < grind_path_item.grind_path_points.Count; i += 2)
                        {
                            lines.Add("l " + (line_point_index + i + 1) + " " + (line_point_index + i + 2));

                            if (i < grind_path_item.grind_path_points.Count - 2)
                            {
                                lines.Add("l " + (line_point_index + i + 2) + " " + (line_point_index + i + 3));
                            }

                            // if last line
                            if (i == grind_path_item.grind_path_points.Count - 1)
                            {
                                lines[lines.Count - 1] = "l " + (line_point_index + i) + " " + (line_point_index + i + 1);
                            }
                        }
                        line_point_index += grind_path_item.grind_path_points.Count;

                        lines.Add("");
                        grind_path_item_count++;
                    }
                }

                item_count++;
            }

            System.IO.File.WriteAllLines(filepath, lines);
        }
    }
}
