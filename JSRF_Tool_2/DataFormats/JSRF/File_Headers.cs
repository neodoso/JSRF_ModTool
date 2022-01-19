using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace JSRF_ModTool.DataFormats.JSRF
{
    /// <summary>
    /// JSRF Container classes
    /// </summary>
    ///
    /// <remarks>
    ///  Most JSRF .bin and .dat files are tree structured files 
    /// (those which start with the MULT header that points to NORMs headers, which point to blocks of data of various types)
    /// The NORM header gives the size and end offset of the blocks
    ///
    /// Its not clear if the NORM headers define the type of data its pointing to, although there are some unknown flags in the header.
    /// It seems that some of these files don't have common headers (MULT/NORM) but instead just binary hearders without an ASCII name such as NORM/MULT,
    ///
    /// Some have a main header MULT which then contains NORMs children that contain children blocks of data,
    /// by pointing to blocks of data, defined by starting offset and the size of the block of data.
    ///
    /// Some other files do not have MULT/NORM headers, but they still have headers (without ascii names)
    /// that are very similar to MULT/NORM in that they point to blocks of data in the same or similar manner
    /// I have named these the "indexed" because they don't have nested/tree data blocks, its all sequential.
    ///
    /// The game executable has string definitions(MULT / NORM) for each of the these bin/dat files.
    /// So perhaps the file type to know how to read each of these files is hard coded in the game executable (XBE)
    /// Which would maybe explain "how" it can know what data format corresponds to files that don't have MULT/NORM headers.
    ///  (unless the data type is defined by a flag in the MULT/NORM or indexed headers?)
    ///  
    /// TODO (maybe?) MULT and NORM headers seem to have the same  structure (start, end, int32 int32)
    /// perhaps it would be better/easier to create a generic class that gets used for MULT and NORM.
    /// </remarks>
    public class File_Headers
    {

        #region generic header
        /// <summary>
        ///  JSRF file header (32 bytes)
        /// </summary>
        public class HEAD
        {
            public byte[] header;
            public Int32 child_count;
            public Int32 unk8; // null?
            public Int32 unk12; // null?

            // read/load header instance from byte buffer
            public void get_head(byte[] buff)
            {          
                header = new byte[4];
                Array.Copy(buff, 0, header, 0, 4);
                child_count = BitConverter.ToInt32(buff, 4);
                unk8 = BitConverter.ToInt32(buff, 8);
                unk12 = BitConverter.ToInt32(buff, 12);
            }
        }

        #endregion


        #region MULT container

        /// <summary>
        ///  JSRF MULT header (32 bytes)
        /// </summary>
        /// 
        /// <remarks>
        ///  MULT + NORM indexed file type
        ///  MULT is always found at the start of the file (actual ASCII code if you check in a hex editor)
        ///  NORM is children of MULT
        ///  Example of these files:
        ///  Media\Player\beat.dat
        ///  Media\StgObj\CarObj01.dat
        /// </remarks>

        public class MULT_head
        {
            public Int32 start { get; set; }
            public Int32 end { get; set; }
            public Int32 size { get; set; }
            public Int32 unk12 { get; set; } // is this a flag? defining something about (maybe what type of data)  the NORM it points to??

            public NORM_head NORM { get; set; }
        }

        /// <summary>
        ///   MULT List<>
        /// </summary>
        public class MULT_list : List<MULT_head>
        {
            // read byte array and loads MULT blocks into a list of MULT_head
            public void get_MULTs(byte[] buff, int mstart)
            {
                int offset = mstart;
                while (offset < buff.Length - 1)
                {
                    MULT_head nMULT = new MULT_head();

                    int start = BitConverter.ToInt32(buff, offset);
                    int end = BitConverter.ToInt32(buff, offset + 4);

                    nMULT.start = start;
                    nMULT.end = end;
                    nMULT.size = BitConverter.ToInt32(buff, offset + 8);
                    nMULT.unk12 = BitConverter.ToInt32(buff, offset + 12);

                    offset = nMULT.end;
                    nMULT.NORM = new NORM_head();
                    nMULT.NORM.get_NORMs(buff, start, end);

                    this.Add(nMULT);
                } 
            }
        }

        #endregion

        #region NORM container

        /// <summary>
        ///  JSRF NORM header (32 bytes)
        /// </summary>
        ///
        /// <remarks>
        /// Some files can also only start by and only have one NORM, this is rather rare
        /// example of this type of file:
        /// Media\Font\jetfont.dat
        /// Media\Progress\Progress.dat
        /// </remarks>
        public class NORM_head
        {
            public byte[] header { get; set; }
            public Int32 child_count { get; set; }
            public Int32 unk8 { get; set; }
            public Int32 unk12 { get; set; }

            public NORM_child_list childs { get; set; }


            public void get_NORMs(byte[] buff, int start, int end)
            {

                int offset = start;
                // get header data
                header = new byte[4];
                Array.Copy(buff, start, header, 0, 4);
                child_count = BitConverter.ToInt32(buff, start + 4);
                unk8 = BitConverter.ToInt32(buff, start + 8);
                unk12 = BitConverter.ToInt32(buff, start + 12);

                // get childs
                childs = new NORM_child_list();
                childs.get_Childs(buff, start, end);
            }
        }


        // materials, textures, models, etc
        #region NORM child

        /// <summary>
        ///  NORM child header (32 bytes) (usually contains MDLB, Materials, Textures...)
        /// </summary>
        public class NORM_child
        {
            public Int32 start { get; set; } // start of data
            public Int32 end { get; set; } // start of next header + data
            public Int32 size { get; set; } // not sure what this is, maybe defines type of data, 12 = material ? or size of header?
            public Int32 unk12 { get; set; }
        }


        /// <summary>
        /// NORM Childs List
        /// </summary>
        public class NORM_child_list : List<NORM_child>
        {
            public void get_Childs(byte[] buff, int start, int end)
            {
                int offset = start + 16;
                while (offset < end - 1)
                {
                    NORM_child nNORM = new NORM_child();

                    nNORM.start = BitConverter.ToInt32(buff, offset); // +16 ?
                    nNORM.end = BitConverter.ToInt32(buff, offset + 4);
                    nNORM.size = BitConverter.ToInt32(buff, offset + 8);
                    nNORM.unk12 = BitConverter.ToInt32(buff, offset + 12);

                    offset = nNORM.end + start;

                    this.Add(nNORM);
                }
            }
        }

        #endregion

        #endregion

        #region Indexed container

        /// <summary>
        /// JSRF "indexed" header
        /// </summary>
        /// <remarks>
        /// Note: I named this type of file/container "indexed" for lack of a better term
        /// since this filetype doesn't have an ASCII header name like "MULT" or "NORM"
        /// Examples of this File type can be found in files_original\Media\Stage\ and other folders
        /// They do not have "MULT" or "NORM" string names in the headers, just binary.
        /// </remarks>
        public class Indexed_container
        {
            public Indexed_child_list childs { get; set; }

            public void get_Indexed(byte[] buff)
            {
                // get childs
                childs = new Indexed_child_list();
                childs.get_Childs(buff);
            }

            public void add_child()
            {
                
            }
        }


        /// <summary>
        ///  "indxed" Child header (24 bytes)
        /// </summary>
        public class Indexed_item
        {
            //public Int32 type { get; set; } 
            public Int32 ID { get; set; } 
            public Int32 block_size { get; set; } 
            public Int32 unk_ID { get; set; }
            public Int32 textures_IDs_count { get; set; }
            //public List<Int32> textures_IDs { get; set; }

            public File_Containers.item_data_type item_type { get; set; }
            public Int32 block_type { get; set; }
            public Int32 block_start { get; set; }
            public Int32 block_end { get; set; }
            //public Int32 header_offset { get; set; }
        }

        /// <summary>
        /// "Indexed" Childs (MDLB, Level models, Materials, Textures...)
        /// </summary>
        public class Indexed_child_list : List<Indexed_item>
        {
            public void get_Childs(byte[] buff)
            {

                int offset = 0;
                while (offset < buff.Length - 28)
                {
                    // if offset value is negative, exit
                    if (offset < 0) { System.Windows.Forms.MessageBox.Show("Error on getting childs, invalid offset.\n see get_Childs(byte[] buff, int start, int end)"); break;  }

                    Indexed_item child = new Indexed_item();

                    child.block_type = BitConverter.ToInt32(buff, offset);

                    // level model
                    if (child.block_type == 1)
                    {
                        child.ID = BitConverter.ToInt32(buff, offset + 4);
                        child.block_size = BitConverter.ToInt32(buff, offset + 8);
                        child.unk_ID = BitConverter.ToInt32(buff, offset + 12);
                        child.block_type = 1;

                        // numnber of item's IDs as int32 (starting at offset 20)
                         child.textures_IDs_count = BitConverter.ToInt32(buff, offset + 16);

                        if(child.textures_IDs_count == 0)
                        {
                            System.Windows.Forms.MessageBox.Show("Error: Indexed_child_list > get_Childs():\n\n child.items_count = 0, please report the error, thanks."); break;
                        }

                        if( child.textures_IDs_count > buff.Length) { return; }
                        Int32 check_MDLB = BitConverter.ToInt32(buff, offset + 20 + (child.textures_IDs_count * 4));

                        if (check_MDLB == 1112294477)  // MDLB
                        {
                            child.item_type = File_Containers.item_data_type.Level_MDLB;
                            child.block_start = offset + 16 ; //20 + (child.textures_IDs_count * 4)
                            child.block_end = offset + child.block_size + 20 + (child.textures_IDs_count * 4);
                            child.block_size = child.block_end - child.block_start;    

                        } else { // level model

                            child.item_type = File_Containers.item_data_type.Level_Model;
                            child.block_start = offset +16;//+ 20 + (child.textures_IDs_count * 4); 
                            child.block_end = offset + child.block_size + 20 + (child.textures_IDs_count * 4);
                            child.block_size = child.block_end - child.block_start;
                        }

                        

                    } // Texture: if child.type == 0  then this is the start of a list of textures
                      // the first block has a different (8 bytes) header: [int32 = 0]  [int32 = block_size]
                    else if (child.block_type == 0)
                    {
                        child.block_type = 0;
                        child.block_start = offset + 8;
                        child.block_size = BitConverter.ToInt32(buff, offset + 4);
                        child.block_end = offset + child.block_size + 8;
                        child.item_type = File_Containers.get_item_data_type(buff, child.block_start);
                    }
                    else  //  Texture: 4 byte header defining block_size
                    {
                        child.block_type = 2;
                        child.block_start = offset + 4;
                        child.block_size = BitConverter.ToInt32(buff, offset);
                        child.block_end = offset + child.block_size + 4;
                        child.item_type = File_Containers.get_item_data_type(buff, child.block_start);
                    }


                    this.Add(child);
                    offset = child.block_end;
                }
            }


            public void add_child(File_Containers.item_data_type type, byte[] buff)
            {
                Indexed_item child = new Indexed_item();


            }
        }

        #endregion


       

    }
}

