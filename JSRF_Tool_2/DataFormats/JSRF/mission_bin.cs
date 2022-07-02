using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSRF_ModTool.DataFormats.JSRF
{
    class Mission_bin
    {
        // 544 bytes
        public struct mission_header
        {
            // main header seems to be 544 bytes
            public Int32 unk_00_stage_id { get; set; } // // 
            public Int32 unk_04 { get; set; } // 
            public Int32 unk_08 { get; set; }  // 
            public Int32 unk_12 { get; set; } // 
            public Int32 unk_16 { get; set; } // 
            public Int32 unk_20 { get; set; } // 
            public Int32 unk_24 { get; set; } // 
            public Int32 unk_28 { get; set; } // 
            public Int32 unk_32 { get; set; } //
            public Int32 unk_36 { get; set; } //
            public Int32 unk_40 { get; set; } //
            public Int32 unk_44 { get; set; } //
            public Int32 unk_48 { get; set; } //



            // values after commenr are data from (mssn0101.bin) //

            // block block size 32
            public Int32 unk_52_block_offset { get; set; } // 544 (mssn0101.bin)
            public Int32 unk_56_block_count { get; set; } // 33


            // unk size  (player spawn point? stage enter position?? )
            // position where the player comes-in the stage
            // i.e. if you come from 99th, you'll spawn on the garage but rolling in from the highway into the garage
            public Int32 unk_60_block_offset { get; set; } // 1600
            public Int32 unk_64_block_count { get; set; } // 15


            // block size 8
            // seems to give an offset + count
            // structure: int (offset) int (count)
            // which point to blocks of 12 bytes (int(0) int(count) int(offset))
            public Int32 unk_68_block_offset { get; set; } // 19932
            public Int32 unk_72_block_count { get; set; } // 3

            public Int32 unk_76_block_offset { get; set; } //
            public Int32 unk_80_block_count { get; set; } //

            public Int32 unk_84_offset { get; set; } //
            public Int32 unk_88_count { get; set; } //

            public Int32 unk_92_offset { get; set; } //
            public Int32 unk_96_count { get; set; } //


            // stage exit bounds (at start)
            // position of things like spray cans (int (item_id) vector3(position)) 
            //(item_id  0 = yellow can  1 = blue can   2 = health can  3 = green can 4= multiplayer bonus

            // 1232
            public Int32 unk_100_offset { get; set; } // 19956
            public Int32 unk_104_count { get; set; } // 5

            public Int32 unk_108_offset { get; set; } //
            public Int32 unk_112_count { get; set; } //

            public Int32 unk_116_offset { get; set; } //
            public Int32 unk_120_count { get; set; } //

            public Int32 unk_124_offset { get; set; } //
            public Int32 unk_128_count { get; set; } //


            //confirmed StgObj placements // 
            //total block size 296 (for mission_0101 - garage tutorial)
            //structure; int? int?(count? or id?) vector3(position) int(offset?) int(? or int16 + int16) int vector3(scale)
            public Int32 unk_132_offset { get; set; } // 21188
            public Int32 unk_136_count { get; set; } // 1



            // characters script (kinda)
            // give position, rotation and character model
            // its like a sequence of how the character models will be placed
            // as it looks like some are repeated
            // collision blocks to patch out holes?
            // block size 476 
            //structure (int int (these two define the character model ID) int (unk) vector3 vector3 vector3 vector3 vector3(character position)
            public Int32 unk_140_offset { get; set; } // 21484 
            public Int32 unk_144_count { get; set; } // 2



            public Int32 unk_148_offset { get; set; } //
            public Int32 unk_152_count { get; set; } //

            public Int32 unk_156_offset { get; set; } //
            public Int32 unk_160_count { get; set; } //

            // block size 368
            // dialogue camera data?? (camera animations when you get close and talk to a character?)
            // dialogue triggers? (nulling float data to 0 made dialogues no longer appear)
            public Int32 unk_164_offset { get; set; } // 22436
            public Int32 unk_168_count { get; set; } // 13

            public Int32 unk_172_offset { get; set; } //
            public Int32 unk_176_count { get; set; } //

            public Int32 unk_180_offset { get; set; } //
            public Int32 unk_184_count { get; set; } //

            public Int32 unk_188_offset { get; set; } //
            public Int32 unk_192_count { get; set; } //

            // block size 160 (stage switching triggers)
            // vectors 3 define bounding box or trigger area
            // some seem to be repeated, or near the same vector 3 
            //(like 2 or 3 points in the same location for each diagonal  of the box)
            // there's also an int to define which stage to switch to
            // one point position which may define at which point it loads the next stage or pushes the player back
            // some other smaller floats
            public Int32 unk_196_offset { get; set; } // 27220
            public Int32 unk_200_count { get; set; } // 5

            public Int32 unk_204_offset { get; set; } //
            public Int32 unk_208_count { get; set; } //

            // block size 204  (stage switching triggers and other triggers?)
            // 6- vectors 3 define bounding box or trigger area
            public Int32 unk_212_offset { get; set; } // 28020
            public Int32 unk_216_count { get; set; } // 23

            public Int32 unk_220_offset { get; set; } //
            public Int32 unk_224_count { get; set; } //

            public Int32 unk_228_offset { get; set; } //
            public Int32 unk_232_count { get; set; } //

            // block size 76 (??levsl switch position, there's a vector 3 that's a point right where the stage switching happens) 
            // (stage bounding box?)
            // structure as follows
            // int (id? maybe points to other blocks for tirgger?) vector3 (position)
            // int (id? maybe points to other blocks for tirgger?) vector3 (position)
            // int (id? maybe points to other blocks for tirgger?) vector3 (position)
            // int (id? maybe points to other blocks for tirgger?) vector3 (position)
            // vector3 (??)
            public Int32 unk_236_offset { get; set; } // 32712
            public Int32 unk_240_count { get; set; } // 4

            // block size 48 (npc location? roboy in garage?) 
            //(points to a bounding box coordinates? so maybe if its outside of it, it gets removed ingame)
            public Int32 unk_244_offset { get; set; } // 33016
            public Int32 unk_248_count { get; set; } // 5

            public Int32 unk_252_offset { get; set; } //
            public Int32 unk_256_count { get; set; } //

            public Int32 unk_260_offset { get; set; } //
            public Int32 unk_264_count { get; set; } //

            public Int32 unk_268_offset { get; set; } //
            public Int32 unk_272_count { get; set; } //

            public Int32 unk_276_offset { get; set; } //
            public Int32 unk_280_count { get; set; } //

            public Int32 unk_284_offset { get; set; } //
            public Int32 unk_288_count { get; set; } //

            public Int32 unk_292_offset { get; set; } //
            public Int32 unk_296_count { get; set; } //

            public Int32 unk_300_offset { get; set; } //
            public Int32 unk_304_count { get; set; } //

            public Int32 unk_308_offset { get; set; } //
            public Int32 unk_312_count { get; set; } //

            public Int32 unk_316_offset { get; set; } //
            public Int32 unk_320_count { get; set; } //

            public Int32 unk_324_offset { get; set; } //
            public Int32 unk_328_count { get; set; } //

            public Int32 unk_332_offset { get; set; } //
            public Int32 unk_336_count { get; set; } //

            public Int32 unk_340_offset { get; set; } //
            public Int32 unk_344_count { get; set; } //

            public Int32 unk_348_offset { get; set; } //
            public Int32 unk_352_count { get; set; } //

            public Int32 unk_356_offset { get; set; } //
            public Int32 unk_360_count { get; set; } //

            public Int32 unk_364_offset { get; set; } //
            public Int32 unk_368_count { get; set; } //

            // block size 84 2268 (dialogue data? points to dialogue header??)
            public Int32 unk_372_offset { get; set; } // 33256
            public Int32 unk_376_count { get; set; } // 27

            // block size 84  (dialogue data? points to dialogue header??) ( another language?)
            public Int32 unk_380_offset { get; set; } // 35524
            public Int32 unk_384_count { get; set; } // 23

            // block size 84  (dialogue data? points to dialogue header??) ( another language?)
            public Int32 unk_388_offset { get; set; } // 37456
            public Int32 unk_392_count { get; set; } // 56

            // block size 84  (dialogue data? points to dialogue header??) ( another language?)
            public Int32 unk_396_offset { get; set; } // 42160
            public Int32 unk_400_count { get; set; } // 14

            // block size 68
            public Int32 unk_404_offset { get; set; } // 43336
            public Int32 unk_408_count { get; set; } // 4

            public Int32 unk_412_offset { get; set; } //
            public Int32 unk_416_count { get; set; } //

            public Int32 unk_420_offset { get; set; } //
            public Int32 unk_424_count { get; set; } //

            public Int32 unk_428_offset { get; set; } //
            public Int32 unk_432_count { get; set; } //

            public Int32 unk_436_offset { get; set; } //
            public Int32 unk_440_count { get; set; } //

            public Int32 unk_444_offset { get; set; } //
            public Int32 unk_448_count { get; set; } //

            public Int32 unk_452_offset { get; set; } //
            public Int32 unk_456_count { get; set; } //

            public Int32 unk_460_offset { get; set; } //
            public Int32 unk_464_count { get; set; } //

            public Int32 unk_468_offset { get; set; } //
            public Int32 unk_472_count { get; set; } //

            public Int32 unk_476_offset { get; set; } //
            public Int32 unk_480_count { get; set; } //

            public Int32 unk_484_offset { get; set; } //
            public Int32 unk_488_count { get; set; } //

            public Int32 unk_492_offset { get; set; } //
            public Int32 unk_496_count { get; set; } //

            public Int32 unk_500_offset { get; set; } //
            public Int32 unk_504_count { get; set; } //

            public Int32 unk_508_offset { get; set; } //
            public Int32 unk_512_count { get; set; } //

            public Int32 unk_516_offset { get; set; } //
            public Int32 unk_520_count { get; set; } //

            public Int32 unk_524_offset { get; set; } //
            public Int32 unk_528_count { get; set; } //

            public Int32 unk_532_offset { get; set; } //
            public Int32 unk_536_count { get; set; } //

            public Int32 unk_540_offset { get; set; } //
            public Int32 unk_544_count { get; set; } //



        }

    }
}
