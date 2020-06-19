using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace JSRF_Tool_2.DataFormats.JSRF
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
    public class JSRF_Container
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

            public void get_Indexed(byte[] buff, int start, int end)
            {
                // get childs
                childs = new Indexed_child_list();
                childs.get_Childs(buff, start, end);
            }
        }


        /// <summary>
        ///  "indxed" Child header (24 bytes)
        /// </summary>
        public class Indexed_child
        {
            public Int32 type { get; set; } 
            public Int32 ID { get; set; } 
            public Int32 size { get; set; } 
            public Int32 ID_parent { get; set; }
            public Int32 block_start { get; set; }
            public Int32 block_end { get; set; }
        }

        /// <summary>
        /// "Indexed" Childs (MDLB, Level models, Materials, Textures...)
        /// </summary>
        public class Indexed_child_list : List<Indexed_child>
        {
            public void get_Childs(byte[] buff, int start, int end)
            {
                int offset = start;
                while (offset < end -24)
                {
                    // if negative value exit
                    if (offset < 0) { System.Windows.Forms.MessageBox.Show("Error on getting childs, invalid offset.\n see get_Childs(byte[] buff, int start, int end)"); break;  }

                    Indexed_child nIndexed = new Indexed_child();

              
                    nIndexed.type = BitConverter.ToInt32(buff, offset); // +16 ?
                    nIndexed.block_start = offset;

                    if (nIndexed.type == 1)
                    {
                        nIndexed.ID = BitConverter.ToInt32(buff, offset + 4);
                        nIndexed.size = BitConverter.ToInt32(buff, offset + 8);
                        nIndexed.ID_parent = BitConverter.ToInt32(buff, offset + 12);
                        nIndexed.block_end = nIndexed.size + offset;

             
                         int header_bytes_count = BitConverter.ToInt32(buff, offset + 16) * 4;
                         nIndexed.block_end =  nIndexed.block_end + header_bytes_count + 20;
                         int block_size = nIndexed.block_end - offset;

                    } else {

                        nIndexed.block_end = BitConverter.ToInt32(buff, offset + 4) + offset + 4;
                    }

                    this.Add(nIndexed);
                    offset = nIndexed.block_end;
                }
            }
        }

        #endregion


        #region Functions

        /// <summary>
        /// returns type of data block (material, texture, model ...)
        /// </summary>
        /// <remarks>
        /// Note: its unclear how JSRF determines the type of data, in some cases such as for textures
        /// we use a pretty hacky way to test if its a texture or not... this can sometimes return the wrong type
        /// 
        /// TODO: research to figure out if NORM headers have a flag defines what type child of data blocks it contains? instead of doing this?
        /// </remarks>
        public static string get_block_header_type(byte[] buff)
        {
            if (buff == null) { return ""; }
            Int32 size = buff.Length;
            if (size <= 0) { return "empty"; }
            if ((size < 17)) { return "Material"; }
            // if (start >= buff.Length) { return "invalid-over buffer"; }

            // texture resolutions
            int[] texture_resolutions = new int[] { 8,16,32,64,128,256,512,1024,2048,4096 };

            Int32 head = BitConverter.ToInt32(buff, 0);
            Int32 head_8 = BitConverter.ToInt32(buff, 8);
            Int16 head_20 = BitConverter.ToInt16(buff, 20);
            Int16 head_22 = BitConverter.ToInt16(buff, 22);
            Int32 head_24 = BitConverter.ToInt32(buff, 24);
            Int32 head_28 = BitConverter.ToInt32(buff, 28);

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
                return "Sound";
            }

            else if ((head == 1112294477) || (head_24 == 1112294477))  // MDLB
            {
                return "MDLB";
            }

            
            // hacky! :/ 
            // texture, if head > that, its probably a texture id at offset 0 ("head")
            // we also check "head20" (value at offset 20) which is the offset/value defining texture resolution for textures
            // check if  multiple of 8 (for textures resolution 8 16 32 64 128 256 512 1024 ...)
            // else if ((head > 100000000) || (head_8 > 100000000) && ((head_20 % 8) == 0) && (size > 32))
            // if resolution value is multiple of 8
            else if (texture_resolutions.Contains(head_20) || (texture_resolutions.Contains(head_28)))
            {   
                // make sure DXT compression type is within range
                //if ((head_28 < 128) && (head_28 >= 0))
                return "Texture";
            }

            // generally material = 16 bytes data block
            else if ((size < 256) & (head < 100) & (head > 0)) // Material
            {
                return "Material";
            }

            else if ((head == 1) && (head_24 != 1112294477)) // not MDLB
            {
                return "Level Model";
            }

            // else
             return "unknown";
        }

        /// <summary>
        /// returns the global starting/ending offset of data block relative to the file (and not the parent containter NORM or MULT that contains the block of data)
        /// </summary>
        /// <remarks>
        /// TODO: should probably refactor this to be a function built within the NORM/MULT classes so the real offset is already given in the class properties...
        /// </remarks>
        public static int get_real_offset(object file_struct, int norm_index, int child_index, bool shift_24, string head_type)
        {
            int start_offset_real = -1;
        
            // MULT container
            if (file_struct.GetType() == typeof(MULT_list))
            {

                MULT_list MULT = (MULT_list)file_struct;

                if (shift_24)
                {
 
                    start_offset_real = MULT[norm_index].start + MULT[norm_index].NORM.childs[child_index].end;
                }
                else
                {
                    start_offset_real = MULT[norm_index].start + MULT[norm_index].NORM.childs[child_index].start;
                }
            }

            // NORM container
            if (file_struct.GetType() == typeof(NORM_head))
            {

                NORM_head NORM = (NORM_head)file_struct;
                if (shift_24)
                {
                    start_offset_real = NORM.childs[child_index].end;
                }
                else
                {
                    start_offset_real = NORM.childs[child_index].start;
                }
            }

            // Indexed container
            if (file_struct.GetType() == typeof(Indexed_container))
            {
               // shift_24 = true;
                Indexed_container Indexed = (Indexed_container)file_struct;
                if (shift_24)
                {
                    start_offset_real = Indexed.childs[child_index].block_end;
                }
                else
                {
                    start_offset_real = Indexed.childs[child_index].block_start; //+16; //+24
                }
            }
       
            return start_offset_real;
        }

        #endregion

    }
}

