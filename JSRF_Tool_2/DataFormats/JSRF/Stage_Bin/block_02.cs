using JSRF_ModTool.Functions;
using JSRF_ModTool.Vector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JSRF_ModTool.DataFormats.JSRF.Stage_Bin
{
    public struct block_02
    {
        public header head { get; set; }

        public class header
        {
            //TODO : stg10 seems to have 35 block types // this is an old note, check if there's really 35 blocks on stg10

            public Int32 blocks_count { get; set; } // number of items

            public Int32 potential_visibility_sets_starto { get; set; } // start offset of blocks
            public Int32 potential_visibility_sets_count { get; set; } // number of blocks

            public Int32 block_01_starto { get; set; } // start offset of blocks
            public Int32 block_01_count { get; set; } // number of blocks

            public Int32 block_02_decals_starto { get; set; } // start offset of blocks
            public Int32 block_02_decals_count { get; set; } // number of blocks

            public Int32 block_03_decals_starto { get; set; } // start offset of blocks
            public Int32 block_03_decals_count { get; set; } // number of blocks

            public Int32 block_04_decals_starto { get; set; } // start offset of blocks
            public Int32 block_04_decals_count { get; set; } // number of blocks

            public Int32 block_05_props_starto { get; set; } // start offset of blocks
            public Int32 block_05_props_count { get; set; } // number of blocks

            public Int32 block_06_decals_starto { get; set; } // start offset of blocks
            public Int32 block_06_decals_count { get; set; } // number of blocks

            public Int32 block_07_props_starto { get; set; } // start offset of blocks
            public Int32 block_07_props_count { get; set; } // number of blocks

            public Int32 block_08_MDLB_starto { get; set; } // start offset of blocks
            public Int32 block_08_MDLB_count { get; set; } // number of blocks

            public Int32 block_09_prop_starto { get; set; } // start offset of blocks
            public Int32 block_09_prop_count { get; set; } // number of blocks

            public Int32 block_10_starto { get; set; } // start offset of blocks
            public Int32 block_10_count { get; set; } // number of blocks

            public Int32 block_11_decals_starto { get; set; } // start offset of blocks
            public Int32 block_11_decals_count { get; set; } // number of blocks

            public Int32 block_12_decals_starto { get; set; } // start offset of blocks
            public Int32 block_12_decals_count { get; set; } // number of blocks
        }





        // potential_visibility_sets, a sort of bounding box with 4 points plane + 2 height value
        // manages which stage model and props are actively being rendered or not drawing
        // potential_visibility_set has a list (pvs_links) which is a list of IDs defining other potential_visibility_set(s)
        // if a PVS is in the pvs_links list, then that PVS region will be rendering while we're inside, this, current PVS
        public List<potential_visibility_set> potential_visibility_sets_list { get; set; } 

        public List<object_spawn> block_01_MDLB_list { get; set; }
        public List<object_spawn> block_02_decals_list { get; set; }
        public List<object_spawn> block_03_decals_list { get; set; }
        public List<object_spawn> block_04_decals_list { get; set; }
        public List<object_spawn> block_05_props_list { get; set; }
        public List<object_spawn> block_06_decals_list { get; set; }
        public List<object_spawn> block_07_props_list { get; set; }
        public List<object_spawn> block_08_MDLB_list { get; set; } // more props, small fences under ufo in Garage stage
        public List<object_spawn> block_09_prop_list { get; set; } // basket ball props  in Garage stage
        public List<object_spawn> block_10_list { get; set; }
        public List<object_spawn> block_11_decals_list { get; set; }
        public List<object_spawn> block_12_decals_list { get; set; } // seems like larger decals

        /// <summary>
        /// load list of data blocks into class instance
        /// </summary>
        public block_02(byte[] data)
        {
           //this =  (block_02)Parsing.binary_to_struct(data, 0, typeof(block_02));

            this.head = (block_02.header)Parsing.binary_to_struct(data, 0, typeof(block_02.header));

            List<string> txt_lines = new List<string>();
            potential_visibility_sets_list = new List<potential_visibility_set>();

            for (int i = 0; i < head.potential_visibility_sets_count; i++)
            {
                byte[] block = new byte[340];
                Array.Copy(data, head.potential_visibility_sets_starto + (i * 340), block, 0, 340);

                potential_visibility_set potential_visibility_set =  (potential_visibility_set)(Parsing.binary_to_struct(block, 0, typeof(potential_visibility_set)));

                // if potential_visibility_set.unk_0_a_count greater than 0
                // read/get and add "draw_dist_bounds" to "potential_visibility_set."
                if (potential_visibility_set.pvs_bounds_count > 0)
                {
                    byte[] blockd = new byte[144];
                    Array.Copy(data, potential_visibility_set.pvs_bounds_offset, blockd, 0, 144);
                    potential_visibility_set.pvs_bounds = (PVS_bounds)(Parsing.binary_to_struct(blockd, 0, typeof(PVS_bounds)));
                }
                
                // get ddr_links
                // it's a list of Int32 probably linking ddr or visual models for rendering areas selectively depending on where the
                // player is standing
                for (int d = potential_visibility_set.pvs_links_offset; d < potential_visibility_set.pvs_links_offset + (potential_visibility_set.pvs_links_count * 4); d+=4)
                {
                    if(d >= data.Length)
                    {
                        break;
                    }
                    potential_visibility_set.pvs_links.Add( BitConverter.ToUInt32(data, d));
                }

                potential_visibility_sets_list.Add(potential_visibility_set);
            }


            #region objects_spawns list

            block_01_MDLB_list = new List<object_spawn>();

            for (int i = 0; i < head.block_01_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, head.block_01_starto + (i * 80), block, 0, 80);
                block_01_MDLB_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B01_" + i + "|" + block_01_MDLB_list[i].export_data());
            }

            block_02_decals_list = new List<object_spawn>();

            for (int i = 0; i < head.block_02_decals_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, head.block_02_decals_starto + (i * 80), block, 0, 80);
                block_02_decals_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B02_" + i + "|" + block_02_decals_list[i].export_data());
            }

            block_03_decals_list = new List<object_spawn>();

            for (int i = 0; i < head.block_03_decals_count; i++)
            {
                if (head.block_03_decals_starto < 0) { continue; }
                byte[] block = new byte[80];
                Array.Copy(data, head.block_03_decals_starto + (i * 80), block, 0, 80);
                block_03_decals_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B03_" + i + "|" + block_03_decals_list[i].export_data());
            }

            block_04_decals_list = new List<object_spawn>();

            for (int i = 0; i < head.block_04_decals_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, head.block_04_decals_starto + (i * 80), block, 0, 80);
                block_04_decals_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B04_" + i + "|" + block_04_decals_list[i].export_data());
            }

            block_05_props_list = new List<object_spawn>();

            if (head.block_05_props_count < 90000)
            {
                for (int i = 0; i < head.block_05_props_count; i++)
                {
                    byte[] block = new byte[80];
                    Array.Copy(data, head.block_05_props_starto + (i * 80), block, 0, 80);
                    block_05_props_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                    txt_lines.Add("B05_" + i + "|" + block_05_props_list[i].export_data());
                }
            }

            block_06_decals_list = new List<object_spawn>();

            for (int i = 0; i < head.block_06_decals_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, head.block_06_decals_starto + (i * 80), block, 0, 80);
                block_06_decals_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B06_" + i + "|" + block_06_decals_list[i].export_data());
            }

            block_07_props_list = new List<object_spawn>();

            for (int i = 0; i < head.block_07_props_count; i++)
            {
                if (head.block_07_props_starto < 0) { continue; }
                byte[] block = new byte[80];
                Array.Copy(data, head.block_07_props_starto + (i * 80), block, 0, 80);
                block_07_props_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B07_" + i + "|" + block_07_props_list[i].export_data());
            }


            block_08_MDLB_list = new List<object_spawn>();

            for (int i = 0; i < head.block_08_MDLB_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, head.block_08_MDLB_starto + (i * 80), block, 0, 80);
                block_08_MDLB_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B08_" + i + "|" + block_08_MDLB_list[i].export_data());
            }

            block_09_prop_list = new List<object_spawn>();

            if (head.block_09_prop_count < 90000) // TODO : this is just a temporary workaround, the structure of this block seems to be different in some stages and data is shifted, giving wrong data //////////////////////////////////////////////////////
            {
                for (int i = 0; i < head.block_09_prop_count; i++)
                {
                    byte[] block = new byte[80];
                    if (block.Length > head.block_09_prop_starto + (i * 80)) // TODO : this is just a temporary workaround, the structure of this block seems to be different in some stages and data is shifted, giving wrong data //////////////////////////////////////////////////////
                    {
                        Array.Copy(data, head.block_09_prop_starto + (i * 80), block, 0, 80);
                        block_09_prop_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                        txt_lines.Add("B09_" + i + "|" + block_09_prop_list[i].export_data());
                    }
                }
            }


            block_10_list = new List<object_spawn>();

            if (head.block_10_count < 90000) // TODO : this is just a temporary workaround, the structure of this block seems to be different in some stages and data is shifted, giving wrong data //////////////////////////////////////////////////////
            {
                for (int i = 0; i < head.block_10_count; i++)
                {
                    byte[] block = new byte[80];
                    if (block.Length > head.block_10_starto + (i * 80)) // TODO : this is just a temporary workaround, the structure of this block seems to be different in some stages and data is shifted, giving wrong data //////////////////////////////////////////////////////
                    {
                        Array.Copy(data, head.block_10_starto + (i * 80), block, 0, 80);
                        block_10_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                        txt_lines.Add("B10_" + i + "|" + block_10_list[i].export_data());
                    }
                }
            }

            block_11_decals_list = new List<object_spawn>();

            for (int i = 0; i < head.block_11_decals_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, head.block_11_decals_starto + (i * 80), block, 0, 80);
                block_11_decals_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B11_" + i + "|" + block_11_decals_list[i].export_data());
            }

            block_12_decals_list = new List<object_spawn>();

            for (int i = 0; i < head.block_12_decals_count; i++)
            {
                byte[] block = new byte[80];
                Array.Copy(data, head.block_12_decals_starto + (i * 80), block, 0, 80);
                block_12_decals_list.Add((object_spawn)(Parsing.binary_to_struct(block, 0, typeof(object_spawn))));
                txt_lines.Add("B12_" + i + "|" + block_12_decals_list[i].export_data());
            }

            #endregion

            // rounds the PVS Vector3
            //round_vectors();
        }

        /*
        public void round_vectors()
        {
            for (int i = 0; i < this.potential_visibility_set.Count; i++)
            {
                this.potential_visibility_set[i].v00.round(5);
                this.potential_visibility_set[i].v01.round(5);
                this.potential_visibility_set[i].v02.round(5);
                this.potential_visibility_set[i].v03.round(5);
                this.potential_visibility_set[i].v04.round(5);
                this.potential_visibility_set[i].v05.round(5);
                this.potential_visibility_set[i].v06.round(5);
                this.potential_visibility_set[i].v07.round(5);
                this.potential_visibility_set[i].v08.round(5);
                this.potential_visibility_set[i].v09.round(5);
                this.potential_visibility_set[i].v10.round(5);
                this.potential_visibility_set[i].v11.round(5);
                this.potential_visibility_set[i].v12.round(5);
            }
        }
        */

        /// <summary>
        /// generic class of object (model/decal) spawn
        /// </summary>
        /// <remarks>
        /// Rotation matrix is defined by multiple vector3
        /// </remarks>
        public class object_spawn // 80 bytes
        {
            public Vector3 v0 { get; set; }
            public float padding_0 { get; set; }

            public Vector3 v1 { get; set; }
            public float padding_1 { get; set; }

            public Vector3 v2 { get; set; }
            public float padding_2 { get; set; }

            public Vector3 v3 { get; set; }
            public float padding_3 { get; set; }

            public Int32 resource_ID { get; set; } // can be the ID/number of an MDLB or texture in the StgXX_XX.dat (possibly StgObj too)
            public Int32 num_b { get; set; }
            public Int32 num_c { get; set; }
            public Int32 num_d { get; set; }

            public string export_data()
            {
                /*
                string pos= v3.X + " " + v3.Y + " " + v3.Z + "|";
                string m0 = v0.X + " " + v0.Y + " " + v0.Z + "|";
                string m1 = v1.X + " " + v1.Y + " " + v1.Z + "|";
                string m2 = v2.X + " " + v2.Y + " " + v2.Z + "|";
                string data = resource_ID + " " + num_b + " " + num_c + " " + num_d;

                return pos + m0 + m1 + m2 + data;
                */
                return "";

            }
        }

        // PVS: bounding boxes + list of IDs to other PVS, to define which level's visual meshes parts are rendering while standing inside a PVS
        // potential_visibility_sets, a sort of bounding box with 4 points plane + 2 height value
        // manages which stage model and props are actively being rendered or not drawing
        // potential_visibility_set has a list (pvs_links) which is a list of IDs defining other potential_visibility_set(s)
        // if a PVS is in the pvs_links list, then that PVS region will be rendering while we're inside, this, current PVS
        public class potential_visibility_set // 340 bytes
        {
            // bounding box, 4 Vector3 form a 4 sided plane, v04_height gives the box height
            public Vector3 v00 { get; set; } // plane point
            public Vector3 v01 { get; set; } // plane point
            public Vector3 v02 { get; set; } // plane point
            public Vector3 v03 { get; set; } // plane point
            public Vector3 v04 { get; set; } // plane point
            public Vector3 v05 { get; set; } // (point aligned with v04) but v05.Y gives the height of the bbox

            // not sure if floats if so, rotations or matrix, else might be int16 or flags
            public Vector3 v06 { get; set; }
            public Vector3 v07 { get; set; }
            public Vector3 v08 { get; set; }
            public Vector3 v09 { get; set; }

            public Vector3 v10 { get; set; }
            public Vector3 v11 { get; set; }
            

            public Int32 pvs_bounds_offset { get; set; } // points to start offset for blocks of type "pvs_bounds", which seem to contain the same bounding box / matrix data contained in this (potential_visibility_set.v00 v01 etc) 
            public Int32 pvs_bounds_count { get; set; }  // number of pvs blocks
            public Int32 pvs_links_offset { get; set; } //  points to end block that is a list of integers (probably pvs block numbers, or stage model number)
            public Int32 pvs_links_count { get; set; }  // number of "links" aka list of PVS numbers/ids of pvs that will be drawn while we're inside this pvs/bounding box (stage models, MDLB, decals etc that are inside a pvs/bounding box)


            public Vector3 v12 { get; set; }
            public Int32 unk_2 { get; set; } // same value as unk_4  // same value on all PVS of the same stage
            public Int32 unk_3 { get; set; }
            public Int32 unk_4 { get; set; } // same value as unk_2  // same value on all PVS of the same stage
            public Int32 unk_5 { get; set; }
            public Vector3 v13 { get; set; }
            public Int32 unk_6 { get; set; } // id? // same value on all PVS of the same stage
            public Int32 unk_7 { get; set; } // id? // same value on all PVS of the same stage

            public Int32 unk_8_padding { get; set; }
            public Int32 unk_9_padding { get; set; }
            public Int32 unk_10_padding { get; set; }
            public Int32 unk_11_padding { get; set; }

            public Int32 unk_12_padding { get; set; }
            public Int32 unk_13_padding { get; set; }
            public Int32 unk_14_padding { get; set; }
            public Int32 unk_15_padding { get; set; }

            public Int32 unk_16_block02_size { get; set; } // block 02 total size
            public Int32 unk_17 { get; set; }

            public Int32 unk_18_block02_size { get; set; } // block 02 total size
            public Int32 unk_19 { get; set; }

            public Int32 unk_20_block02_size { get; set; } // block 02 total size
            public Int32 unk_21 { get; set; }

            public Int32 unk_22_block02_size { get; set; } // block 02 total size
            public Int32 unk_23 { get; set; }

            public Int32 unk_24_padding { get; set; }
            public Int32 unk_25_padding { get; set; }
            public Int32 unk_26_padding { get; set; }
            public Int32 unk_27_padding { get; set; }
            public Int32 unk_28_padding { get; set; }
            public Int32 unk_29_padding { get; set; }
            public Int32 unk_31_padding { get; set; }
            public Int32 unk_32_padding { get; set; }
            public Int32 unk_33_padding { get; set; }
            public Int32 unk_34_padding { get; set; }
            public Int32 unk_35_padding { get; set; }
            public Int32 unk_36_padding { get; set; }
            public Int32 unk_37_padding { get; set; }
            public Int32 unk_38_padding { get; set; }
            public Int32 unk_39_padding { get; set; }
            public Int32 unk_40_padding { get; set; }
            public Int32 unk_41_padding { get; set; }


            public PVS_bounds pvs_bounds { get; set; }
            // list of IDs (numbers from 0 to 30 or more ) probably a list of how PVS are interconnected
            // to draw or hide the level's visual meshes based on the position of the player relative to a PVS block bounding box
            // list of PVS number/id that render while we're inside this one
            public List<UInt32> pvs_links { get; set; } = new List<UInt32>();
        }

        // same list of Vector3 that define a 4 point plane + height point
        // these values doesn't seem to effect the "potential_visibility_set"
        public class PVS_bounds  // 144 bytes
        {
            public Vector3 v00 { get; set; }
            public Vector3 v01 { get; set; }

            public Vector3 v02 { get; set; }
            public Vector3 v03 { get; set; }

            public Vector3 v04 { get; set; }
            public Vector3 v05 { get; set; }

            public Vector3 vs00 { get; set; }
            public Vector3 vs01 { get; set; }

            public Vector3 vs02 { get; set; }
            public Vector3 vs03 { get; set; }

            public Vector3 vs04 { get; set; }
            public Vector3 vs05 { get; set; }
        }

        public void export_PVS_data(string export_dir)
        {
            List<String> lines = new List<string>();

            for (int i = 0; i < this.potential_visibility_sets_list.Count; i++)
            {
                lines = new List<string>();
                potential_visibility_set pvs = this.potential_visibility_sets_list[i];

                lines.Add("start");
                lines.Add(pvs.v00.ToString());
                lines.Add(pvs.v01.ToString());
                lines.Add(pvs.v02.ToString());
                lines.Add(pvs.v03.ToString());
                lines.Add(pvs.v04.ToString());
                lines.Add(pvs.v05.ToString());

                lines.Add("");
                lines.Add(pvs.v06.ToString());
                lines.Add(pvs.v07.ToString());
                lines.Add(pvs.v08.ToString());
                lines.Add(pvs.v09.ToString());
                lines.Add(pvs.v10.ToString());
                lines.Add(pvs.v11.ToString());

                lines.Add("");

                string links = String.Empty;
                for (int d = 0; d < pvs.pvs_links.Count; d++)
                {
                    // if last, do not add the : separator
                    if (d == pvs.pvs_links.Count - 1 && pvs.pvs_links.Count > 1)
                    {
                        //lines.Add(ddr.ddr_links[d].ToString());
                        links = links + ":" + pvs.pvs_links[d];
                        break;
                    }
                    else if (d > 0 && pvs.pvs_links.Count > 1)
                    {
                        links = links + ":" + pvs.pvs_links[d];
                    }
                    else if (d == 0)
                    {
                        links = pvs.pvs_links[d].ToString();
                    }
                }
                // add links line
                if (pvs.pvs_links.Count > 0)
                {
                    lines.Add("pvs_Links:" + links);
                }

                /*
                lines.Add("");
                lines.Add("################################## data");
                lines.Add("unk_2 " + pvs.unk_2.ToString());
                lines.Add("unk_4 " + pvs.unk_4.ToString());
                lines.Add("unk_6 " + pvs.unk_6.ToString());
                lines.Add("unk_7 " + pvs.unk_7.ToString());
                */

                if (pvs.pvs_bounds != null)
                {
                    lines.Add("");
                    lines.Add("pvs_bounds a");
                    lines.Add(pvs.pvs_bounds.v00.ToString());
                    lines.Add(pvs.pvs_bounds.v01.ToString());
                    lines.Add(pvs.pvs_bounds.v02.ToString());
                    lines.Add(pvs.pvs_bounds.v03.ToString());
                    lines.Add(pvs.pvs_bounds.v04.ToString());
                    lines.Add(pvs.pvs_bounds.v05.ToString());
                    lines.Add("");
                    lines.Add("pvs_bounds a");
                    lines.Add(pvs.pvs_bounds.vs00.ToString());
                    lines.Add(pvs.pvs_bounds.vs01.ToString());
                    lines.Add(pvs.pvs_bounds.vs02.ToString());
                    lines.Add(pvs.pvs_bounds.vs03.ToString());
                    lines.Add(pvs.pvs_bounds.vs04.ToString());
                    lines.Add(pvs.pvs_bounds.vs05.ToString());
                    
                }

                lines.Add("");
                lines.Add("");

                if (!Directory.Exists(export_dir + "\\PVS\\"))
                {
                    Directory.CreateDirectory(export_dir + "\\PVS\\");
                }
                    

                System.IO.File.WriteAllLines(export_dir + "\\PVS\\" + "PVS_" + i + ".txt", lines);
            }
        }
    }
}
