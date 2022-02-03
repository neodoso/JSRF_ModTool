using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using JSRF_ModTool.Functions;

namespace JSRF_ModTool.DataFormats.JSRF
{
    /// <summary>
    /// There 3 general types of file structures in JSRF
    /// -one MULT container  that containes multiplke NORM children
    /// -one NORM container (at the root) that has child items
    /// -No containers, sequential list of items,  I named these "indexed"
    /// </summary>
    public class File_Containers
    {
        #region Declarations

        public string filepath;
        public byte[] file_data { get; }

        public container_types type;
        // TODO : make this variable dynamic or of Object type
        // and cast type at runtime since it could be MULT, NORM or 'indexed' types
        public MULT MULT_root { get; set; }
        public NORM NORM_root { get; set; }
        public INDEXED INDX_root { get; set; }

        public bool root_node_is_NORM { get; }

        #endregion

        #region containers classes


        public enum container_types
        {
            unknown,
            indexed,
            MULT,
            NORM,
        };

        // NCAM is found in beat.bin 
        public enum item_data_type
        {
            empty,
            invalid,
            unkown,
            MDLB,
            Level_MDLB,
            Level_Model,
            Material,
            Texture,
            NCAM,
            Sound
        };


        // MULT container, contains list of NORM containers
        public class MULT
        {
            public List<NORM> items { get; set; }

            public MULT()
            {
                items = new List<NORM>();
            }
        }

        // NORM container, contains list of item(s)
        public class NORM
        {
            public List<item> items { get; set; }

            public NORM()
            {
                items = new List<item>();
            }
        }

        // indexed container, contains list of item(s)
        public class INDEXED
        {
            public List<item> items { get; set; }

            public INDEXED()
            {
                items = new List<item>();
            }
        }

        //List<this.item> items = new List<this.item>();

        
        public class item_indx
        {
            public item_data_type type { get; set; }
            public byte[] data { get; set; }

            public item_indx(item_data_type _type, byte[] _data)
            {
                this.type = _type;
                this.data = _data;
            }
        }
        

        public class item
        {
            public item_data_type type { get; set; }
            public byte[] data { get; set; }
            public File_Headers.Indexed_item indexed_item { get; set; }

            public item(item_data_type _type, byte[] _data, File_Headers.Indexed_item _indexed_item = null)
            {
                this.type = _type;
                this.data = _data;
                this.indexed_item = _indexed_item;
            }
        }

        #endregion

        // load file and its contrainers into class: MULT | NORM | INDEXED
        public File_Containers(string filepath = "")
        {
            if (filepath == "") { return; }
            if (!File.Exists(filepath)) { return; }

            this.filepath = filepath;

            this.file_data = Parsing.FileToByteArray(filepath, 0);

            if (this.file_data.Length == 0)
            {
                MessageBox.Show("File is empty");
                return;
            }

            byte[] type = new byte[4];
            Array.Copy(this.file_data, 0, type, 0, 4);

            string header_type = System.Text.Encoding.UTF8.GetString(type);
            this.type = container_types.unknown;



            if (header_type == "MULT")
            {
                this.type = container_types.MULT;

                byte[] childs_buffer = new byte[file_data.Length]; //file_data.Length - 16]
                Array.Copy(file_data, 0, childs_buffer, 0, file_data.Length); //Array.Copy(file_data, 16, childs_buffer, 0, file_data.Length);

                List<byte[]> NORMs_buffers = get_children(childs_buffer);

                List<NORM> NORM_list = new List<NORM>();

                // for each NORM contained in the MULT
                // get the children data
                foreach (var NORM_buff in NORMs_buffers)
                {
                    List<byte[]> items_data = get_children(NORM_buff);

                    NORM n = new NORM();
                    // for each item contained in the NORM
                    // get the children data
                    foreach (var item_data in items_data)
                    {
                        // add item to NORM n
                        n.items.Add(new item(get_item_data_type(item_data), item_data));
                    }
                    NORM_list.Add(n);
                }

                this.MULT_root = new MULT();
                this.MULT_root.items = NORM_list;
            }


            if (header_type == "NORM")
            {
                this.type = container_types.NORM;

                byte[] childs_buffer = new byte[file_data.Length]; //file_data.Length - 16]
                Array.Copy(file_data, 0, childs_buffer, 0, file_data.Length); //Array.Copy(file_data, 16, childs_buffer, 0, file_data.Length);

                List<byte[]> items_data = get_children(childs_buffer);

                List<NORM> NORM_list = new List<NORM>();

                NORM n = new NORM();
                // for each item contained in the NORM
                // get the children data
                foreach (var item_data in items_data)
                {
                    // add item to NORM n
                    n.items.Add(new item(get_item_data_type(item_data), item_data));
                }

                this.NORM_root = new NORM();
                this.NORM_root = n;
            }

            // indexed (no header name)
            if ((file_data[0] == 1 & file_data[1] == 0 & file_data[2] == 0 & file_data[3] == 0) || (file_data[0] == 0 & file_data[1] == 0 & file_data[2] == 0 & file_data[3] == 0))
            {
                this.type = container_types.indexed;

                File_Headers.Indexed_container indexed_items = new File_Headers.Indexed_container();
                indexed_items.get_Indexed(file_data);

                INDX_root = new INDEXED();
                INDX_root.items = new List<item>();

                Int32 offset = 0;

                try
                {


                    for (int i = 0; i < indexed_items.childs.Count; i++)
                    {
                        item indx_item;
                        if (indexed_items.childs[i].block_end < 0) { MessageBox.Show("Error: invalid file structure."); break; }
                        byte[] childs_buffer = new byte[indexed_items.childs[i].block_size]; // 

                        // copy block of data from 'file_data' to 'childs_buffer'
                        Array.Copy(file_data, indexed_items.childs[i].block_start, childs_buffer, 0, indexed_items.childs[i].block_size);

                        indx_item = new item(indexed_items.childs[i].item_type, childs_buffer, indexed_items.childs[i]);
                        INDX_root.items.Add(indx_item);

                        offset += indexed_items.childs[i].block_size + indexed_items.childs[i].block_start;
                    }
                } catch { 

                }
            }
        }




        /// <summary>
        /// class returned when searching items
        /// specifies position of item in list
        /// </summary>
        public class item_match
        {
            public item item { get; }

            public int x { get; }
            public int y { get; }

            public item_match(item _item, int _x, int _y)
            {
                this.item = _item;
                this.x = _x;
                this.y = _y;
            }
        }

        /// <summary>
        /// returns list of items of type t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public List<item_match> find_items_ofType(item_data_type t)
        {

            List<item_match> matches = new List<item_match>();

            if (this.type == container_types.MULT)
            {
                for (int x = 0; x < this.MULT_root.items.Count; x++)
                {
                    for (int y = 0; y < this.MULT_root.items[x].items.Count; y++)
                    {
                        if(this.MULT_root.items[x].items[y].type == t)
                        {
                            matches.Add(new item_match(this.MULT_root.items[x].items[y], x, y));
                        }
                    }
                }
            }

            if (this.type == container_types.NORM)
            {
                for (int x = 0; x < this.NORM_root.items.Count - 1; x++)
                {
                    if (this.NORM_root.items[x].type == t)
                    {
                        matches.Add(new item_match(this.NORM_root.items[x], x, -1));
                    }
                }
            }

            if (this.type == container_types.indexed)
            {
                for (int x = 0; x < this.INDX_root.items.Count - 1; x++)
                {
                    if (this.INDX_root.items[x].type == t)
                    {
                        matches.Add(new item_match(this.INDX_root.items[x], x, -1));
                    }
                }
            }

            return matches;
        }



        public bool has_items()
        {
            if (this.type == container_types.MULT)
            {
                if (this.MULT_root.items.Count > 0) { return true; }
            }

            if (this.type == container_types.NORM)
            {
                if (this.NORM_root.items.Count > 0) { return true; }
            }

            if (this.type == container_types.indexed)
            {
                if (this.INDX_root.items.Count > 0) { return true; }
            }

            return false;
        }

        #region item/node operation


        public item get_item(int x, int y)
        {
            if (this.type == container_types.MULT)
            {
                if (this.MULT_root.items[x].items.Count == 0) { return new item(item_data_type.empty, new byte[0]); }
                if (y > this.MULT_root.items[x].items.Count) { return this.MULT_root.items[x].items[this.MULT_root.items[x].items.Count-1]; }
                return this.MULT_root.items[x].items[y];
            }

            if (this.type == container_types.NORM)
            {
                return this.NORM_root.items[y];
            }

            if (this.type == container_types.indexed)
            {
                return this.INDX_root.items[y];
            }

            // return empty item if unkown
            return new item(item_data_type.invalid, new byte[0]);
        }

        /// <summary>
        /// returns item data from container(s) based on indices
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte[] get_item_data(int x, int y)
        {
            if(this.type == container_types.MULT)
            {
                return this.MULT_root.items[x].items[y].data;
            }

            if (this.type == container_types.NORM)
            {
                return this.NORM_root.items[y].data;
            }

            if (this.type == container_types.indexed)
            {
                return this.INDX_root.items[y].data;
            }

            // return empty item if unkown
            //return new item(item_data_type.invalid, new byte[0]);

            return new byte[0];
        }



        /// <summary>
        /// returns item list of container
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public List<item> get_node_items(int x)
        {
            if (this.type == container_types.MULT)
            {
                return this.MULT_root.items[x].items;
            }

            if (this.type == container_types.NORM)
            {
                return this.NORM_root.items;
            }

            if (this.type == container_types.indexed)
            {
                return this.INDX_root.items;
            }

            return new List<item>();
        }


        /// <summary>
        /// sets container node's list of items
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public void set_node_items(int x, List<item> items)
        {
            if (this.type == container_types.MULT)
            {
               this.MULT_root.items[x].items = items;
            }

            if (this.type == container_types.NORM)
            {
                this.NORM_root.items = items;
            }

            if (this.type == container_types.indexed)
            {
                this.INDX_root.items = items;
            }
        }


        /// <summary>
        ///  remove all child items
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void remove_child_items(int x)
        {
            if (this.type == container_types.MULT)
            {
                this.MULT_root.items[x].items.Clear();
            }

            if (this.type == container_types.NORM)
            {
                this.NORM_root.items.Clear();
            }

            if (this.type == container_types.indexed)
            {
                this.INDX_root.items.Clear();
            }
        }


        /// <summary>
        /// sets item data 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void set_item_data(int x, int y, byte[] data)
        {
            if (this.type == container_types.MULT)
            {
                this.MULT_root.items[x].items[y].data = data;
            }

            if (this.type == container_types.NORM)
            {
               this.NORM_root.items[y].data = data;
            }

            if (this.type == container_types.indexed)
            {
                this.INDX_root.items[y].data = data;
            }
        }

        /// <summary>
        /// returns item data from container(s) based on indices
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int get_item_data_length(int x, int y)
        {
            if (this.type == container_types.MULT)
            {
                return this.MULT_root.items[x].items[y].data.Length;
            }

            if (this.type == container_types.NORM)
            {
                return this.NORM_root.items[y].data.Length;
            }

            if (this.type == container_types.indexed)
            {
                return this.INDX_root.items[y].data.Length;
            }

            // return empty item if unkown
            //return new item(item_data_type.invalid, new byte[0]);

            return 0;
        }


        /// <summary>
        /// insert item after
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void insert_item_after(int x, int y, item_data_type type, byte[] data)
        {
            if (this.type == container_types.MULT)
            {
                this.MULT_root.items[x].items.Insert(y, new item(type, data));
            }

            if (this.type == container_types.NORM)
            {
                this.NORM_root.items.Insert(y, new item(type, data));
            }

            if (this.type == container_types.indexed)
            {
                this.INDX_root.items.Insert(y, new item(type, data));
            }
        }


        /// <summary>
        /// remove item at position x y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void remove_item(int x, int y)
        {
            if (this.type == container_types.MULT)
            {
                this.MULT_root.items[x].items.RemoveAt(y);
            }

            if (this.type == container_types.NORM)
            {
                this.NORM_root.items.RemoveAt(y);
            }

            if (this.type == container_types.indexed)
            {
                this.INDX_root.items.RemoveAt(y);
            }
        }

        /// <summary>
        /// empty item at position x y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void empty_item_data(int x, int y)
        {
            if (this.type == container_types.MULT)
            {
                this.MULT_root.items[x].items[y].data = new byte[0];
                this.MULT_root.items[x].items[y].type = item_data_type.empty;
            }

            if (this.type == container_types.NORM)
            {
                this.NORM_root.items[y].data = new byte[0];
                this.NORM_root.items[y].type = item_data_type.empty;
            }

            if (this.type == container_types.indexed)
            {
                this.INDX_root.items[y].data = new byte[0];
                this.INDX_root.items[y].type = item_data_type.empty;
            }
        }


        /// <summary>
        /// set item data
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public void set_item(int x, int y, byte[] data)
        {
            if (this.type == container_types.MULT)
            {
                this.MULT_root.items[x].items[y].data = data;
            }

            if (this.type == container_types.NORM)
            {
                this.NORM_root.items[y].data = data;
            }

            if (this.type == container_types.indexed)
            {
                this.INDX_root.items[y].data = data;
            }
        }

        #endregion


        #region file loading methods

        /// <summary>
        /// Read buffer and gets list of items
        /// Returns list of items(byte arrays)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private List<byte[]> get_children(byte[] buffer)
        {
            List<byte[]> items = new List<byte[]>();
            // if type = 0 MULT  // 1 = NORM // 2 = item
            //if (type == 0) { items = new List<Object>(); }

            // skip header
            int offset = 16;
            // for each block of data
            while (offset < buffer.Length - 1)
            {
                // read header
                int child_start_offset = BitConverter.ToInt32(buffer, offset);
                int next_item_offset = BitConverter.ToInt32(buffer, offset + 4);
                int size = BitConverter.ToInt32(buffer, offset + 8);
                int flag = BitConverter.ToInt32(buffer, offset + 12);
                int real_size = next_item_offset - child_start_offset;

                // if block is marked as empty
                if (flag == 1)
                {

#if DEBUG
                    if (size > 0)
                    {
                        MessageBox.Show("Unexpected data block flagged as empty but size > 0.");
                    }
#endif
                    // add empty block of data
                    items.Add(new byte[0]);
                    offset = next_item_offset;

                    continue;
                }

                // copy block of data into new array
                byte[] block = new byte[real_size];
                Array.Copy(buffer, child_start_offset, block, 0, real_size);
                // add block of data to items list
                items.Add(block);

                offset = next_item_offset;
            }

            return items;
        }




        /// <summary>
        /// reads first bytes of buffer to determine type of data
        /// returns type
        /// </summary>
        /// <param name="buffer"></param>
        /// <remarks>
        /// Note: its unclear how JSRF determines the type of data, in some cases such as for textures
        /// we use a pretty hacky way to test if its a texture or not... this can sometimes return the wrong type
        /// 
        /// TODO: research to figure out if NORM headers have a flag defines what type child of data blocks it contains? instead of doing this?
        /// </remarks>
        public static item_data_type get_item_data_type(byte[] buff, Int32 offset = 0)
        {
            if (buff == null) { return item_data_type.invalid; }
            Int32 size = buff.Length;
            if (size <= 0) { return item_data_type.unkown; }
            if ((size < 17)) { return item_data_type.Material; }
            // if (start >= buff.Length) { return "invalid-over buffer"; }

            // texture resolutions
            int[] texture_resolutions = new int[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };

            Int32 head = BitConverter.ToInt32(buff, offset + 0);
            Int32 head_4 = BitConverter.ToInt32(buff, offset + 4);
            Int32 head_8 = BitConverter.ToInt32(buff, offset + 8);
            Int16 head_20 = BitConverter.ToInt16(buff, offset + 20);
            Int16 head_22 = BitConverter.ToInt16(buff, offset + 22);
            Int32 head_24 = BitConverter.ToInt32(buff, offset + 24);
            Int32 head_28 = BitConverter.ToInt32(buff, offset + 28);

            /*
            // if file is indexed, the texture headers position are shifted!
            if (Main.jsrf_file.GetType() == typeof(Indexed_head))
            {
                // todo
                // get type of data for Indexed file structures (level model, textures in level files)
            }
            */

            if (head == 1179011410) // sound
            {
                return item_data_type.Sound;
            }

            else if ((head == 1112294477) || (head_24 == 1112294477))  // MDLB
            {
                return item_data_type.MDLB;
            }


            // hacky! :/ 
            // texture, if head > that, its probably a texture id at offset 0 ("head")
            // we also check "head20" (value at offset 20) which is the offset/value defining texture resolution for textures
            // check if  multiple of 8 (for textures resolution 8 16 32 64 128 256 512 1024 ...)
            // else if ((head > 100000000) || (head_8 > 100000000) && ((head_20 % 8) == 0) && (size > 32))
            // if resolution value is multiple of 8
            else if ((head != 0) && (head_4 != 1) && texture_resolutions.Contains(head_20) || (texture_resolutions.Contains(head_28)))
            {
                // make sure DXT compression type is within range
                //if ((head_28 < 128) && (head_28 >= 0))
                return item_data_type.Texture;
            }

            // generally material = 16 bytes data block
            else if ((size < 256) & (head < 100) & (head > 0)) // Material
            {
                return item_data_type.Material;
            }

            else if ((head == 0) && (head_4 == 1)) // not MDLB
            {
                return item_data_type.Level_Model;
            }

            // else
            return item_data_type.unkown;
        }

        #endregion


        #region file builder
        /// <summary>
        /// rebuilds JSRF container file structure and its data (based on MULT items list) and writes to binary file
        /// </summary>
        /// <param name="filepath_output"></param>
        public void rebuild_file(string filepath_output)
        {

            List<byte[]> file_buffer = new List<byte[]>();

            #region build for MULT file

            if(this.type == container_types.MULT)
            {

                #region build for MULT header

                // build MULT header (16 bytes)
                file_buffer.Add(Encoding.ASCII.GetBytes("MULT"));
                // number of children (NORMs)
                file_buffer.Add(BitConverter.GetBytes((Int32)this.MULT_root.items.Count));
                // padding
                file_buffer.Add(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 });

                #endregion


                int mult_child_offset = 32;

                // for each NORM inside MULT
                for (int n = 0; n < this.MULT_root.items.Count; n++)
                {
                    NORM nNORM = this.MULT_root.items[n];

                    // merge item data
                    byte[] item_data = build_NORM_children(nNORM);

                    #region build MULT child header

                    // start offset
                    file_buffer.Add(BitConverter.GetBytes((Int32)mult_child_offset));
                    // end offset (item_data length + item header length)
                    file_buffer.Add(BitConverter.GetBytes((Int32)mult_child_offset + item_data.Length + 16));
                    // size
                    file_buffer.Add(BitConverter.GetBytes((Int32)item_data.Length + 16));
                    // flag
                    file_buffer.Add(BitConverter.GetBytes((Int32)0));

                    #endregion


                    #region build NORM header

                    // build MULT header (16 bytes)
                    file_buffer.Add(Encoding.ASCII.GetBytes("NORM"));
                    // number of children (NORMs)
                    file_buffer.Add(BitConverter.GetBytes((Int32)nNORM.items.Count));
                    // padding
                    file_buffer.Add(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 });

                    #endregion

                    // write item data
                    file_buffer.Add(item_data);

                    // increase position (header size + data size)
                    mult_child_offset += item_data.Length + 32;
                }

            }

            #endregion



            #region build for NORM root file

            if (this.type == container_types.NORM)
            {
                NORM nNORM = this.NORM_root;

                // merge item data
                byte[] item_data = build_NORM_children(nNORM);

                #region build NORM header

                // build MULT header (16 bytes)
                file_buffer.Add(Encoding.ASCII.GetBytes("NORM"));
                // number of children (NORMs)
                file_buffer.Add(BitConverter.GetBytes((Int32)nNORM.items.Count));
                // padding
                file_buffer.Add(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 });

                #endregion

                // write item data
                file_buffer.Add(item_data);
            }

            #endregion




            #region build for Indexed root file

            if (this.type == container_types.indexed)
            {
                // get byte array of each item
                byte[] item_data = build_Indexed_children(this.INDX_root);

                // write item data
                file_buffer.Add(item_data);
            }

            #endregion


            File.WriteAllBytes(filepath_output, file_buffer.SelectMany(a => a).ToArray());

        }

        /// <summary>
        /// takes in list of items contained in a NORM node and builds JSRF binary of NORM children
        /// </summary>
        /// <param name="nNORM"></param>
        /// <returns></returns>
        public byte[] build_NORM_children(NORM nNORM)
        {
            List<byte[]> item_data_list = new List<byte[]>();
            int item_data_offset = 32;
            // for each child item inside nNORM
            for (int c = 0; c < nNORM.items.Count; c++)
            {
                item cItem = nNORM.items[c];

                #region build item header

                // start offset // headers size + data length of previous items
                item_data_list.Add(BitConverter.GetBytes((Int32)item_data_offset)); //(c+1) * 16
                                                                                    // end offset
                item_data_list.Add(BitConverter.GetBytes((Int32)item_data_offset + cItem.data.Length));
                // size??
                item_data_list.Add(BitConverter.GetBytes((Int32)cItem.data.Length));
                // flag
                if (cItem.data.Length > 0)
                {
                    item_data_list.Add(BitConverter.GetBytes((Int32)0));
                }
                else // if empty set this to 1
                {
                    item_data_list.Add(BitConverter.GetBytes((Int32)1));
                }

                #endregion

                // add dataa
                item_data_list.Add(cItem.data);

                // increase position (header size + data size)
                item_data_offset += 16 + cItem.data.Length;
            }

            // merge list items into array and return
            return item_data_list.SelectMany(a => a).ToArray();
        }

        /// <summary>
        /// takes in list of items contained in a NORM node and builds JSRF binary of NORM children
        /// </summary>
        /// <param name="nNORM"></param>
        /// <returns></returns>
        public byte[] build_Indexed_children(INDEXED nIndexed)
        {
            List<byte[]> item_data_list = new List<byte[]>();

            // for each child item inside nNORM
            for (int c = 0; c < nIndexed.items.Count; c++)
            {
                item item = nIndexed.items[c];
               
                #region header

                // TODO support MDLB

                // Level model
                if (item.indexed_item.block_type == 1)
                {    
                    item_data_list.Add(BitConverter.GetBytes((Int32)1)); // Int32 : item_type // item.indexed_item.type
                    item_data_list.Add(BitConverter.GetBytes((Int32)c)); // Int32 : item ID

                    // get texture ids count from item data
                    int textures_ids_count = BitConverter.ToInt32(item.data, 0);
                    // Int32 : data block size 
                    // data block size minus length of header [texture_ids_counts + texture_ids_list]
                    item_data_list.Add(BitConverter.GetBytes((Int32)item.data.Length - ( 4 + textures_ids_count * 4))); 
                    item_data_list.Add(BitConverter.GetBytes((Int32)item.indexed_item.unk_ID)); // unk_ID
                    /*
                    item_data_list.Add(BitConverter.GetBytes((Int32)item.indexed_item.textures_IDs_count)); // texture ids count

                    //  add each texture ID (of list) to item_data_list
                    for (int i = 0; i < item.indexed_item.textures_IDs_count; i++)
                    {
                        //item_data_list.Add(BitConverter.GetBytes((Int32)item.indexed_item.textures_IDs[i]));
                        item_data_list.Add(BitConverter.GetBytes(BitConverter.ToInt32(item.data, 4 + 4 * i)));
                    }
                    */
                }
                // Texture: if child.type == 0  then this is the start of a list of textures
                // the first block has a different (8 bytes) header: [int32 = 0]  [int32 = block_size]
                else if (item.indexed_item.block_type == 0)
                {
                    item_data_list.Add(BitConverter.GetBytes((Int32)0)); // Int32 : item_type
                    item_data_list.Add(BitConverter.GetBytes((Int32)item.data.Length)); //  Int32 : size
                }

                // Texture: 4 byte header defining block_size
                else if (item.indexed_item.block_type == 2)
                {
                    item_data_list.Add(BitConverter.GetBytes((Int32)item.data.Length)); //  Int32 : size
                }

                #endregion

                // add item data
                item_data_list.Add(item.data);
            }

            // merge list of byte arrays into a single array and return it
            return item_data_list.SelectMany(a => a).ToArray();
        }


        public byte[] build_Indexed_file(INDEXED nIndexed)
        {
            List<byte[]> item_data_list = new List<byte[]>();

            bool first_texture = true;

            // for each child item inside nNORM
            for (int c = 0; c < nIndexed.items.Count; c++)
            {
                item item = nIndexed.items[c];

                #region header

                // TODO support MDLB

                // Level model
                if (item.type == File_Containers.item_data_type.Level_Model)
                {
                    item_data_list.Add(BitConverter.GetBytes((Int32)1)); // Int32 : item_type // item.indexed_item.type
                    item_data_list.Add(BitConverter.GetBytes((Int32)c)); // Int32 : item ID

                    // get texture ids count from item data
                    int textures_ids_count = BitConverter.ToInt32(item.data, 0);
                    // Int32 : data block size 
                    // data block size minus length of header [texture_ids_counts + texture_ids_list]
                    item_data_list.Add(BitConverter.GetBytes((Int32)item.data.Length - (4 + textures_ids_count * 4)));
                    item_data_list.Add(BitConverter.GetBytes(0)); // unk_ID

                }
                // Texture: if child.type == 0  then this is the start of a list of textures
                // the first block has a different (8 bytes) header: [int32 = 0]  [int32 = block_size]
                else if (item.type == File_Containers.item_data_type.Texture)
                {
                    if (first_texture)
                    {
                        item_data_list.Add(BitConverter.GetBytes((Int32)0)); // Int32 : item_type
                        item_data_list.Add(BitConverter.GetBytes((Int32)item.data.Length)); //  Int32 : size
                        first_texture = false;
                    } 
                    else
                    {   // Texture: 4 byte header defining block_size
                        item_data_list.Add(BitConverter.GetBytes((Int32)item.data.Length)); //  Int32 : size
                    }
                }



                #endregion

                // add item data
                item_data_list.Add(item.data);
            }

            // merge list of byte arrays into a single array and return it
            return item_data_list.SelectMany(a => a).ToArray();
        }

        #endregion


    }
}
