using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNES_SPC
{
    public class SPC_DSP
    {
        public delegate byte[] copy_func_t(byte[] io, byte[] state, int size);
        /*
#define GET_LE16SA( addr )      ((BOOST::int16_t) GET_LE16( addr ))
#define GET_LE16A( addr )       GET_LE16( addr )
#define SET_LE16A( addr, data ) SET_LE16( addr, data )
         */

        static readonly byte[] initial_regs = new byte[SPC_DSP.register_count]
        {
            0x45,0x8B,0x5A,0x9A,0xE4,0x82,0x1B,0x78,0x00,0x00,0xAA,0x96,0x89,0x0E,0xE0,0x80,
            0x2A,0x49,0x3D,0xBA,0x14,0xA0,0xAC,0xC5,0x00,0x00,0x51,0xBB,0x9C,0x4E,0x7B,0xFF,
            0xF4,0xFD,0x57,0x32,0x37,0xD9,0x42,0x22,0x00,0x00,0x5B,0x3C,0x9F,0x1B,0x87,0x9A,
            0x6F,0x27,0xAF,0x7B,0xE5,0x68,0x0A,0xD9,0x00,0x00,0x9A,0xC5,0x9C,0x4E,0x7B,0xFF,
            0xEA,0x21,0x78,0x4F,0xDD,0xED,0x24,0x14,0x00,0x00,0x77,0xB1,0xD1,0x36,0xC1,0x67,
            0x52,0x57,0x46,0x3D,0x59,0xF4,0x87,0xA4,0x00,0x00,0x7E,0x44,0x9C,0x4E,0x7B,0xFF,
            0x75,0xF5,0x06,0x97,0x10,0xC3,0x24,0xBB,0x00,0x00,0x7B,0x7A,0xE0,0x60,0x12,0x0F,
            0xF7,0x74,0x1C,0xE5,0x39,0x3D,0x73,0xC1,0x00,0x00,0x7A,0xB3,0xFF,0x4E,0x7B,0xFF
        };

        // if ( io < -32768 ) io = -32768;
        // if ( io >  32767 ) io =  32767;
        int CLAMP16(int io)
        {
            if((short)io != io)
            {
                io = (io >> 31) ^ 0x7FFF;
            }
            
            return io;
        }

        // Access global DSP register
        //#define REG(n)      m.regs [r_##n]
        // GlobalRegisters.r_xxxx

        // Access voice DSP register
        //#define VREG(r,n)   r [v_##n]
        // r[VoiceRegisters.v_xxx]

        /*
        #define WRITE_SAMPLES( l, r, out ) \
        {\
            out [0] = l;\
            out [1] = r;\
            out += 2;\
            if ( out >= m.out_end )\
            {\
                check( out == m.out_end );\
                check( m.out_end != &m.extra [extra_size] || \
                    (m.extra <= m.out_begin && m.extra < &m.extra [extra_size]) );\
                out       = m.extra;\
                m.out_end = &m.extra [extra_size];\
            }\
        }\
         */

        /***Setup***/

        // Initializes DSP and has it use the 64K RAM provided
        public void init(byte[] ram_64k)
        {
            m.ram = ram_64k;
            mute_voices(0);
            disable_surround(false);
            set_output(null, 0);
            reset();

            //#ifndef NDEBUG
            //            // be sure this sign-extends
            //            assert((int16_t)0x8000 == -0x8000);

            //            // be sure right shift preserves sign
            //            assert((-1 >> 1) == -1);

            //            // check clamp macro
            //            int i;
            //            i = +0x8000; CLAMP16(i); assert(i == +0x7FFF);
            //            i = -0x8001; CLAMP16(i); assert(i == -0x8000);

            //            blargg_verify_byte_order();
            //#endif
        }

        // Sets destination for output samples. If out is NULL or out_size is 0,
        // doesn't generate any.
        public void set_output(short[] output, int out_size)
        {
            if (out_size % 2 != 0)
            {
                throw new ArgumentException("set_output buffer size must be a multiple of 2", nameof(out_size));
            }

            if(output == null)
            {
                output = m.extra;
                out_size = extra_size;
            }

            m.output = output;
        }

        // Volume registers and efb are signed! Easy to forget int8_t cast.
        // Prefixes are to avoid accidental use of locals with same names.

        // Gaussian interpolation

        static readonly short[] gauss = new short[512] 
        {
           0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
           1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   2,   2,   2,   2,
           2,   2,   3,   3,   3,   3,   3,   4,   4,   4,   4,   4,   5,   5,   5,   5,
           6,   6,   6,   6,   7,   7,   7,   8,   8,   8,   9,   9,   9,  10,  10,  10,
          11,  11,  11,  12,  12,  13,  13,  14,  14,  15,  15,  15,  16,  16,  17,  17,
          18,  19,  19,  20,  20,  21,  21,  22,  23,  23,  24,  24,  25,  26,  27,  27,
          28,  29,  29,  30,  31,  32,  32,  33,  34,  35,  36,  36,  37,  38,  39,  40,
          41,  42,  43,  44,  45,  46,  47,  48,  49,  50,  51,  52,  53,  54,  55,  56,
          58,  59,  60,  61,  62,  64,  65,  66,  67,  69,  70,  71,  73,  74,  76,  77,
          78,  80,  81,  83,  84,  86,  87,  89,  90,  92,  94,  95,  97,  99, 100, 102,
         104, 106, 107, 109, 111, 113, 115, 117, 118, 120, 122, 124, 126, 128, 130, 132,
         134, 137, 139, 141, 143, 145, 147, 150, 152, 154, 156, 159, 161, 163, 166, 168,
         171, 173, 175, 178, 180, 183, 186, 188, 191, 193, 196, 199, 201, 204, 207, 210,
         212, 215, 218, 221, 224, 227, 230, 233, 236, 239, 242, 245, 248, 251, 254, 257,
         260, 263, 267, 270, 273, 276, 280, 283, 286, 290, 293, 297, 300, 304, 307, 311,
         314, 318, 321, 325, 328, 332, 336, 339, 343, 347, 351, 354, 358, 362, 366, 370,
         374, 378, 381, 385, 389, 393, 397, 401, 405, 410, 414, 418, 422, 426, 430, 434,
         439, 443, 447, 451, 456, 460, 464, 469, 473, 477, 482, 486, 491, 495, 499, 504,
         508, 513, 517, 522, 527, 531, 536, 540, 545, 550, 554, 559, 563, 568, 573, 577,
         582, 587, 592, 596, 601, 606, 611, 615, 620, 625, 630, 635, 640, 644, 649, 654,
         659, 664, 669, 674, 678, 683, 688, 693, 698, 703, 708, 713, 718, 723, 728, 732,
         737, 742, 747, 752, 757, 762, 767, 772, 777, 782, 787, 792, 797, 802, 806, 811,
         816, 821, 826, 831, 836, 841, 846, 851, 855, 860, 865, 870, 875, 880, 884, 889,
         894, 899, 904, 908, 913, 918, 923, 927, 932, 937, 941, 946, 951, 955, 960, 965,
         969, 974, 978, 983, 988, 992, 997,1001,1005,1010,1014,1019,1023,1027,1032,1036,
        1040,1045,1049,1053,1057,1061,1066,1070,1074,1078,1082,1086,1090,1094,1098,1102,
        1106,1109,1113,1117,1121,1125,1128,1132,1136,1139,1143,1146,1150,1153,1157,1160,
        1164,1167,1170,1174,1177,1180,1183,1186,1190,1193,1196,1199,1202,1205,1207,1210,
        1213,1216,1219,1221,1224,1227,1229,1232,1234,1237,1239,1241,1244,1246,1248,1251,
        1253,1255,1257,1259,1261,1263,1265,1267,1269,1270,1272,1274,1275,1277,1279,1280,
        1282,1283,1284,1286,1287,1288,1290,1291,1292,1293,1294,1295,1296,1297,1297,1298,
        1299,1300,1300,1301,1302,1302,1303,1303,1303,1304,1304,1304,1304,1304,1305,1305,
        };

        // Number of samples written to output since it was last set, always
        // a multiple of 2. Undefined if more samples were generated than
        // output buffer could hold.
        public int sample_count()
        {
            return 0; //todo:  m.output - m.out_begin;
        }

        /***Emulation***/

        // Resets DSP to power-on state
        public void reset()
        {
            load(initial_regs);
        }

        // Emulates pressing reset switch on SNES
        public void soft_reset()
        {
            m.regs[(int)GlobalRegisters.r_flg] = 0xE0;
            soft_reset_common();
        }

        // Reads/writes DSP registers. For accuracy, you must first call run()
        // to catch the DSP up to present.
        public int read(int addr)
        {
            if(addr >= register_count)
            {
                throw new ArgumentOutOfRangeException(nameof(addr), "read addr is greater than register_count");
            }

            return m.regs[addr];
        }

        public void write(int addr, int data)
        {
            if (addr >= register_count)
            {
                throw new ArgumentOutOfRangeException(nameof(addr), "write addr is greater than register_count");
            }

            m.regs[addr] = (byte)data;
            switch (addr & 0x0F)
            {
                case (int)VoiceRegisters.v_envx:
                    m.envx_buf = (byte)data;
                    break;

                case (int)VoiceRegisters.v_outx:
                    m.outx_buf = (byte)data;
                    break;

                case 0x0C:
                    if (addr == (int)GlobalRegisters.r_kon)
                    {
                        m.new_kon = (byte)data;
                    }

                    if (addr == (int)GlobalRegisters.r_endx) // always cleared, regardless of data written
                    {
                        m.endx_buf = 0;
                        m.regs[(int)GlobalRegisters.r_endx] = 0;
                    }
                    break;
            }

        }

        // Runs DSP for specified number of clocks (~1024000 per second). Every 32 clocks
        // a pair of samples is be generated.
        public void run(int clocks_remain)
        {
            if(clocks_remain <= 0)
            {
                throw new ArgumentException("clocks_remain must be greater than 0", nameof(clocks_remain));
            }

            int phase = m.phase;
            m.phase = (phase + clocks_remain) & 31;

            do
            {
                // thanks c# for not having fall-through case statements!
                if (phase <= 0)
                {
                    phase0(); if (--clocks_remain == 0) break;
                }
                if (phase <= 1)
                {
                    phase1(); if (--clocks_remain == 0) break;
                }
                if (phase <= 2)
                {
                    phase2(); if (--clocks_remain == 0) break;
                }
                if (phase <= 3)
                {
                    phase3(); if (--clocks_remain == 0) break;
                }
                if (phase <= 4)
                {
                    phase4(); if (--clocks_remain == 0) break;
                }
                if (phase <= 5)
                {
                    phase5(); if (--clocks_remain == 0) break;
                }
                if (phase <= 6)
                {
                    phase6(); if (--clocks_remain == 0) break;
                }
                if (phase <= 7)
                {
                    phase7(); if (--clocks_remain == 0) break;
                }
                if (phase <= 8)
                {
                    phase8(); if (--clocks_remain == 0) break;
                }
                if (phase <= 9)
                {
                    phase9(); if (--clocks_remain == 0) break;
                }
                if (phase <= 10)
                {
                    phase10(); if (--clocks_remain == 0) break;
                }
                if (phase <= 11)
                {
                    phase11(); if (--clocks_remain == 0) break;
                }
                if (phase <= 12)
                {
                    phase12(); if (--clocks_remain == 0) break;
                }
                if (phase <= 13)
                {
                    phase13(); if (--clocks_remain == 0) break;
                }
                if (phase <= 14)
                {
                    phase14(); if (--clocks_remain == 0) break;
                }
                if (phase <= 15)
                {
                    phase15(); if (--clocks_remain == 0) break;
                }
                if (phase <= 16)
                {
                    phase16(); if (--clocks_remain == 0) break;
                }
                if (phase <= 17)
                {
                    phase17(); if (--clocks_remain == 0) break;
                }
                if (phase <= 18)
                {
                    phase18(); if (--clocks_remain == 0) break;
                }
                if (phase <= 19)
                {
                    phase19(); if (--clocks_remain == 0) break;
                }
                if (phase <= 20)
                {
                    phase20(); if (--clocks_remain == 0) break;
                }
                if (phase <= 21)
                {
                    phase21(); if (--clocks_remain == 0) break;
                }
                if (phase <= 22)
                {
                    phase22(); if (--clocks_remain == 0) break;
                }
                if (phase <= 23)
                {
                    phase23(); if (--clocks_remain == 0) break;
                }
                if (phase <= 24)
                {
                    phase24(); if (--clocks_remain == 0) break;
                }
                if (phase <= 25)
                {
                    phase25(); if (--clocks_remain == 0) break;
                }
                if (phase <= 26)
                {
                    phase26(); if (--clocks_remain == 0) break;
                }
                if (phase <= 27)
                {
                    phase27(); if (--clocks_remain == 0) break;
                }
                if (phase <= 28)
                {
                    phase28(); if (--clocks_remain == 0) break;
                }
                if (phase <= 29)
                {
                    phase29(); if (--clocks_remain == 0) break;
                }
                if (phase <= 30)
                {
                    phase30(); if (--clocks_remain == 0) break;
                }
                if (phase <= 31)
                {
                    phase31();
                }
            } while (--clocks_remain > 0);
        }

        void phase0()
        {
            voice_V5(m.voices[0]);
            voice_V2(m.voices[1]);
        }
        void phase1()
        {
            voice_V6(m.voices[0]);
            voice_V3(m.voices[1]);
        }
        void phase2()
        {
            voice_V7_V4_V1(m.voices, 0);
        }
        void phase3()
        {
            voice_V8_V5_V2(m.voices, 0);
        }
        void phase4()
        {
            voice_V9_V6_V3(m.voices, 0);
        }
        void phase5()
        {
            voice_V7_V4_V1(m.voices, 1);
        }
        void phase6()
        {
            voice_V8_V5_V2(m.voices, 1);
        }
        void phase7()
        {
            voice_V9_V6_V3(m.voices, 1);
        }
        void phase8()
        {
            voice_V7_V4_V1(m.voices, 2);
        }
        void phase9()
        {
            voice_V8_V5_V2(m.voices, 2);
        }
        void phase10()
        {
            voice_V9_V6_V3(m.voices, 2);
        }
        void phase11()
        {
            voice_V7_V4_V1(m.voices, 3);
        }
        void phase12()
        {
            voice_V8_V5_V2(m.voices, 3);
        }
        void phase13()
        {
            voice_V9_V6_V3(m.voices, 3);
        }
        void phase14()
        {
            voice_V7_V4_V1(m.voices, 4);
        }
        void phase15()
        {
            voice_V8_V5_V2(m.voices, 4);
        }
        void phase16()
        {
            voice_V9_V6_V3(m.voices, 4);
        }
        void phase17()
        {
            voice_V1(m.voices[0]);
            voice_V7(m.voices[5]);
            voice_V4(m.voices[6]);
        }
        void phase18()
        {
            voice_V8_V5_V2(m.voices, 5);
        }
        void phase19()
        {
            voice_V9_V6_V3(m.voices, 5);
        }
        void phase20()
        {
            voice_V1(m.voices[1]);
            voice_V7(m.voices[6]);
            voice_V4(m.voices[7]);
        }
        void phase21()
        {
            voice_V8(m.voices[6]);
            voice_V5(m.voices[7]);
            voice_V2(m.voices[0]);
        }
        void phase22()
        {
            voice_V3a(m.voices[0]);
            voice_V9(m.voices[6]);
            voice_V6(m.voices[7]);
            echo_22();
        }
        void phase23()
        {
            voice_V7(m.voices[7]);
            echo_23();
        }
        void phase24()
        {
            voice_V8(m.voices[7]);
            echo_24();
        }
        void phase25()
        {
            voice_V3b(m.voices[0]);
            voice_V9(m.voices[7]);
            echo_25();
        }
        void phase26()
        {
            echo_26();
        }
        void phase27()
        {
            misc_27();
            echo_27();
        }
        void phase28()
        {
            misc_28();
            echo_28();
        }
        void phase29()
        {
            misc_29();
            echo_29();
        }
        void phase30()
        {
            misc_30();
            voice_V3c(m.voices[0]);
            echo_30();
        }
        void phase31()
        {
            voice_V4(m.voices[0]);
            voice_V1(m.voices[2]);
        }

        /***Sound control***/

        // Mutes voices corresponding to non-zero bits in mask (issues repeated KOFF events).
        // Reduces emulation accuracy.
        internal const int voice_count = 8;
        public void mute_voices(int mask)
        {
            m.mute_mask = mask;
        }

        /***State***/

        // Resets DSP and uses supplied values to initialize registers
        internal const int register_count = 128;
        public void load(byte[] regs) // ( uint8_t const regs [register_count] );
        {
            Array.Copy(regs, m.regs, m.regs.Length);
            ClearState();

            // Internal state
            for (int i = voice_count; --i >= 0;)
            {
                m.voices[i].brr_offset = 1;
                m.voices[i].vbit = 1 << i;
                m.voices[i].regs = m.regs; // &m.regs[i * 0x10]; // this is a problem
                m.voices[i].regs_offset = i * 0x10;
            }
            m.new_kon = m.regs[(int)GlobalRegisters.r_kon];
            m.t_dir = m.regs[(int)GlobalRegisters.r_dir];
            m.t_esa = m.regs[(int)GlobalRegisters.r_esa];

            soft_reset_common();
        }
        void ClearState()
        {
            //	memset( &m.regs [register_count], 0, offsetof (state_t,ram) - register_count );
            for(int i=0; i<m.echo_hist.Rank; ++i)
            {
                for(int j=0; j<m.echo_hist.GetLength(i); ++j)
                {
                    m.echo_hist[i, j] = 0;
                }
            }
            m.echo_hist_pos = 0; // todo: this needs to be modified

            m.every_other_sample = 0;
            m.kon = 0;
            m.noise = 0;
            m.counter = 0;
            m.echo_offset = 0;
            m.echo_length = 0;
            m.phase = 0;
            m.kon_check = false;

            m.new_kon = 0;
            m.endx_buf = 0;
            m.envx_buf = 0;
            m.outx_buf = 0;

            m.t_pmon = 0;
            m.t_non = 0;
            m.t_eon = 0;
            m.t_dir = 0;
            m.t_koff = 0;

            m.t_brr_next_addr = 0;
            m.t_adsr0 = 0;
            m.t_brr_header = 0;
            m.t_brr_byte = 0;
            m.t_srcn = 0;
            m.t_esa = 0;
            m.t_echo_enabled = 0;

            m.t_dir_addr = 0;
            m.t_pitch = 0;
            m.t_output = 0;
            m.t_looped = 0;
            m.t_echo_ptr = 0;

            for(int i=0; i<m.t_main_out.Length; ++i)
            {
                m.t_main_out[i] = 0;
            }
            for (int i = 0; i < m.t_echo_out.Length; ++i)
            {
                m.t_echo_out[i] = 0;
            }
            for (int i = 0; i < m.t_echo_in.Length; ++i)
            {
                m.t_echo_in[i] = 0;
            }

            for(int i=0; i<m.voices.Length; ++i)
            {
                m.voices[i] = new voice_t();
            }
        }

        // Saves/loads exact emulator state
        internal const int state_size = 640; // maximum space needed when saving
        public void copy_state( byte[] io, copy_func_t copy )
        {
            SPC_State_Copier copier = new SPC_State_Copier(io, copy);

            // DSP registers
            copier.copy( m.regs, register_count );

            // Internal state

            // Voices
            for (int i = 0; i < voice_count; ++i)
            {
                voice_t v = m.voices[i];

                // BRR buffer
                for(int j=0; j < brr_buf_size; ++j)
                {
                    int s = v.buf[j];
                    copier.SPC_COPY_int16_t(ref s);
                    v.buf[j] = v.buf[j + brr_buf_size] = s;
                }
                
                copier.SPC_COPY_uint16_t(ref v.interp_pos);
                copier.SPC_COPY_uint16_t(ref v.brr_addr);
                copier.SPC_COPY_uint16_t(ref v.env);
                copier.SPC_COPY_int16_t(ref v.hidden_env);
                copier.SPC_COPY_uint8_t(ref v.buf_pos);
                copier.SPC_COPY_uint8_t(ref v.brr_offset);
                copier.SPC_COPY_uint8_t(ref v.kon_delay);
                {
                    int envmode = (int)v.env_mode;
                    copier.SPC_COPY_uint8_t(ref envmode);
                    v.env_mode = (env_mode_t)envmode;
                }
                copier.SPC_COPY_uint8_t(ref v.t_envx_out);

                copier.extra();
            }
            
            // Echo history
            for (int i = 0; i < echo_hist_size; i++ )
            {
                for (int j = 0; j < 2; j++ )
                {
                    int s = m.echo_hist[m.echo_hist_pos + i, j];
                    copier.SPC_COPY_int16_t(ref s);
                    m.echo_hist[i, j] = s; // write back at offset 0
                }
            }
            m.echo_hist_pos = 0;
            //memcpy( &m.echo_hist [echo_hist_size], m.echo_hist, echo_hist_size * sizeof m.echo_hist [0] );
            Array.Copy(m.echo_hist, 0, m.echo_hist, echo_hist_size, echo_hist_size);

            // Misc
            copier.SPC_COPY_uint8_t(ref m.every_other_sample );
            copier.SPC_COPY_uint8_t(ref m.kon );

            copier.SPC_COPY_uint16_t(ref m.noise );
            copier.SPC_COPY_uint16_t(ref m.counter );
            copier.SPC_COPY_uint16_t(ref m.echo_offset );
            copier.SPC_COPY_uint16_t(ref m.echo_length );
            copier.SPC_COPY_uint8_t(ref m.phase );

            copier.SPC_COPY_uint8_t(ref m.new_kon );
            copier.SPC_COPY_uint8_t(ref m.endx_buf );
            copier.SPC_COPY_uint8_t(ref m.envx_buf );
            copier.SPC_COPY_uint8_t(ref m.outx_buf );

            copier.SPC_COPY_uint8_t(ref m.t_pmon );
            copier.SPC_COPY_uint8_t(ref m.t_non );
            copier.SPC_COPY_uint8_t(ref m.t_eon );
            copier.SPC_COPY_uint8_t(ref m.t_dir );
            copier.SPC_COPY_uint8_t(ref m.t_koff );

            copier.SPC_COPY_uint16_t(ref m.t_brr_next_addr );
            copier.SPC_COPY_uint8_t(ref m.t_adsr0 );
            copier.SPC_COPY_uint8_t(ref m.t_brr_header );
            copier.SPC_COPY_uint8_t(ref m.t_brr_byte );
            copier.SPC_COPY_uint8_t(ref m.t_srcn );
            copier.SPC_COPY_uint8_t(ref m.t_esa );
            copier.SPC_COPY_uint8_t(ref m.t_echo_enabled );

            copier.SPC_COPY_int16_t(ref m.t_main_out [0] );
            copier.SPC_COPY_int16_t(ref m.t_main_out [1] );
            copier.SPC_COPY_int16_t(ref m.t_echo_out [0] );
            copier.SPC_COPY_int16_t(ref m.t_echo_out [1] );
            copier.SPC_COPY_int16_t(ref m.t_echo_in  [0] );
            copier.SPC_COPY_int16_t(ref m.t_echo_in  [1] );

            copier.SPC_COPY_uint16_t(ref m.t_dir_addr );
            copier.SPC_COPY_uint16_t(ref m.t_pitch );
            copier.SPC_COPY_int16_t(ref m.t_output );
            copier.SPC_COPY_uint16_t(ref m.t_echo_ptr );
            copier.SPC_COPY_uint8_t(ref m.t_looped );

            copier.extra();
        }

        // Returns non-zero if new key-on events occurred since last call
        public bool check_kon()
        {
            bool old = m.kon_check;
            m.kon_check = false;
            return old;
        }

        /***DSP register addresses***/

        // Global registers
        public enum GlobalRegisters
        {
            r_mvoll = 0x0C,
            r_mvolr = 0x1C,
            r_evoll = 0x2C,
            r_evolr = 0x3C,
            r_kon = 0x4C,
            r_koff = 0x5C,
            r_flg = 0x6C,
            r_endx = 0x7C,
            r_efb = 0x0D,
            r_pmon = 0x2D,
            r_non = 0x3D,
            r_eon = 0x4D,
            r_dir = 0x5D,
            r_esa = 0x6D,
            r_edl = 0x7D,
            r_fir = 0x0F // 8 coefficients at 0x0F, 0x1F ... 0x7F
        }

        // Voice registers
        public enum VoiceRegisters
        {
            v_voll = 0x00,
            v_volr = 0x01,
            v_pitchl = 0x02,
            v_pitchh = 0x03,
            v_srcn = 0x04,
            v_adsr0 = 0x05,
            v_adsr1 = 0x06,
            v_gain = 0x07,
            v_envx = 0x08,
            v_outx = 0x09
        }

        internal const int extra_size = 16;
        internal short[] extra() { return m.extra; }
        internal int out_pos() { return m.out_pointer; } //m.output; }
        internal short[] out_buffer() { return m.output; }
        public void disable_surround(bool disable) { } // not supported

        internal const int echo_hist_size = 8;

        public enum env_mode_t { env_release, env_attack, env_decay, env_sustain };
        internal const int brr_buf_size = 12;
        public class voice_t
        {
            public int[] buf = new int[brr_buf_size * 2];// decoded samples (twice the size to simplify wrap handling)
            public int buf_pos;            // place in buffer where next samples will be decoded
            public int interp_pos;         // relative fractional position in sample (0x1000 = 1.0)
            public int brr_addr;           // address of current BRR block
            public int brr_offset;         // current decoding offset in BRR block
            public byte[] regs;            // pointer to voice's DSP registers
            public int regs_offset;        // fake the C pointer that would be at regs
            public int vbit;               // bitmask for voice: 0x01 for voice 0, 0x02 for voice 1, etc.
            public int kon_delay;          // KON delay/current setup phase
            public env_mode_t env_mode;
            public int env;                // current envelope level
            public int hidden_env;         // used by GAIN mode 7, very obscure quirk
            public byte t_envx_out;
        }

        const int brr_block_size = 9;



        internal class state_t
        {
            public byte[] regs = new byte[register_count];

            // Echo history keeps most recent 8 samples (twice the size to simplify wrap handling)
            public int[,] echo_hist = new int[echo_hist_size * 2, 2];
            public int echo_hist_pos; //int (*echo_hist_pos) [2]; // &echo_hist [0 to 7]

            public int every_other_sample; // toggles every sample
            public int kon;                // KON value when last checked
            public int noise;
            public int counter;
            public int echo_offset;        // offset from ESA in echo buffer
            public int echo_length;        // number of bytes that echo_offset will stop at
            public int phase;              // next clock cycle to run (0-31)
            public bool kon_check;         // set when a new KON occurs

            // Hidden registers also written to when main register is written to
            public int new_kon;
            public byte endx_buf;
            public byte envx_buf;
            public byte outx_buf;

            // Temporary state between clocks

            // read once per sample
            public int t_pmon;
            public int t_non;
            public int t_eon;
            public int t_dir;
            public int t_koff;

            // read a few clocks ahead then used
            public int t_brr_next_addr;
            public int t_adsr0;
            public int t_brr_header;
            public int t_brr_byte;
            public int t_srcn;
            public int t_esa;
            public int t_echo_enabled;

            // internal state that is recalculated every sample
            public int t_dir_addr;
            public int t_pitch;
            public int t_output;
            public int t_looped;
            public int t_echo_ptr;

            // left/right sums
            public int[] t_main_out = new int[2];
            public int[] t_echo_out = new int[2];
            public int[] t_echo_in = new int[2];

            public voice_t[] voices = new voice_t[voice_count] { new voice_t(), new voice_t(), new voice_t(), new voice_t(), new voice_t(), new voice_t(), new voice_t(), new voice_t() };

            // non-emulation state
            public byte[] ram; // 64K shared RAM between DSP and SMP
            public int mute_mask;
            public short[] output;
            public int out_pointer;
            public int out_end; //sample_t* out_end;
            public int out_begin;//sample_t* out_begin;
            public short[] extra = new short[extra_size];
        }
        state_t m;

        //// Counters

        const int simple_counter_range = 2048 * 5 * 3; // 30720

        static readonly int[] counter_rates = new int[32]
        {
           simple_counter_range + 1, // never fires
                  2048, 1536,
            1280, 1024,  768,
             640,  512,  384,
             320,  256,  192,
             160,  128,   96,
              80,   64,   48,
              40,   32,   24,
              20,   16,   12,
              10,    8,    6,
               5,    4,    3,
                     2,
                     1
        };

        static readonly int[] counter_offsets = new int[32]
        {
              1, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
            536, 0, 1040,
                 0,
                 0
        };

        void init_counter()
        {
            m.counter = 0;
        }

        void run_counters()
        {
            if (--m.counter < 0)
            {
                m.counter = simple_counter_range - 1;
            }
        }

        int read_counter(int rate)
        {
            return ((int)m.counter + counter_offsets[rate]) % counter_rates[rate];
        }

        int interpolate(voice_t v )
        {
            // Make pointers into gaussian based on fractional position between samples
            int offset = v.interp_pos >> 4 & 0xFF;  // int offset = v->interp_pos >> 4 & 0xFF;
            int fwd = 255 - offset;                 // short const* fwd = gauss + 255 - offset;
            int rev = offset;                       // short const* rev = gauss       + offset; // mirror left half of gaussian

            int index = v.interp_pos >> 12 + v.buf_pos;             // int const* in = &v->buf [(v->interp_pos >> 12) + v->buf_pos];
            int output;                                             // int out;
            output = (gauss[fwd + 0] * v.buf[index + 0]) >> 11;     // out  = (fwd [  0] * in [0]) >> 11;
            output += (gauss[fwd + 256] * v.buf[index + 1]) >> 11;  // out += (fwd [256] * in [1]) >> 11;
            output += (gauss[rev + 256] * v.buf[index + 2]) >> 11;  // out += (rev [256] * in [2]) >> 11;
            output = (short)output;                                 // out = (int16_t) out;
            output += (gauss[rev + 0] * v.buf[index + 3]) >> 11;    // out += (rev [  0] * in [3]) >> 11;

            output = CLAMP16(output);                               // CLAMP16( out );
            output &= ~1;                                           // out &= ~1;

            return output;                                          // return out;
        }

        //// Envelope

        void run_envelope(voice_t v )
        {
            int env = v.env;
            if (v.env_mode == env_mode_t.env_release) // 60%
            {
                if ((env -= 0x8) < 0)
                {
                    env = 0;
                }
                v.env = env;
            }
            else
            {
                int rate;
                int env_data = v.regs[(int)VoiceRegisters.v_adsr1];
                if ((m.t_adsr0 & 0x80) != 0) // 99% ADSR
                {
                    if (v.env_mode >= env_mode_t.env_decay) // 99%
                    {
                        env--;
                        env -= env >> 8;
                        rate = env_data & 0x1F;
                        if (v.env_mode == env_mode_t.env_decay) // 1%
                        {
                            rate = (m.t_adsr0 >> 3 & 0x0E) + 0x10;
                        }
                    }
                    else // env_attack
                    {
                        rate = (m.t_adsr0 & 0x0F) * 2 + 1;
                        env += rate < 31 ? 0x20 : 0x400;
                    }
                }
                else // GAIN
                {
                    int mode;
                    env_data = v.regs[(int)VoiceRegisters.v_gain];
                    mode = env_data >> 5;
                    if (mode < 4) // direct
                    {
                        env = env_data * 0x10;
                        rate = 31;
                    }
                    else
                    {
                        rate = env_data & 0x1F;
                        if (mode == 4) // 4: linear decrease
                        {
                            env -= 0x20;
                        }
                        else if (mode < 6) // 5: exponential decrease
                        {
                            env--;
                            env -= env >> 8;
                        }
                        else // 6,7: linear increase
                        {
                            env += 0x20;
                            if (mode > 6 && (UInt32)v.hidden_env >= 0x600)
                            {
                                env += 0x8 - 0x20; // 7: two-slope linear increase
                            }
                        }
                    }
                }

                // Sustain level
                if ((env >> 8) == (env_data >> 5) && v.env_mode == env_mode_t.env_decay)
                {
                    v.env_mode = env_mode_t.env_sustain;
                }

                v.hidden_env = env;

                // unsigned cast because linear decrease going negative also triggers this
                if ((UInt32)env > 0x7FF)
                {
                    env = (env < 0 ? 0 : 0x7FF);
                    if (v.env_mode == env_mode_t.env_attack)
                    {
                        v.env_mode = env_mode_t.env_decay;
                    }
                }

                if (read_counter(rate) != 0)
                {
                    v.env = env; // nothing else is controlled by the counter
                }
            }
        }

        //// BRR Decoding

        void decode_brr(voice_t v)
        {
            // Arrange the four input nybbles in 0xABCD order for easy decoding
            int nybbles = m.t_brr_byte * 0x100 + m.ram[(v.brr_addr + v.brr_offset + 1) & 0xFFFF];

            int header = m.t_brr_header;

            // Write to next four samples in circular buffer
            int pos = v.buf_pos;
            int end;
            if ((v.buf_pos += 4) >= brr_buf_size)
            {
                v.buf_pos = 0;
            }

            // Decode four samples
            for (end = pos + 4; pos < end; pos++, nybbles <<= 4)
            {
                // Extract nybble and sign-extend
                int s = (short)nybbles >> 12;

                // Shift sample based on header
                int shift = header >> 4;
                s = (s << shift) >> 1;
                if (shift >= 0xD) // handle invalid range
                {
                    s = (s >> 25) << 11; // same as: s = (s < 0 ? -0x800 : 0)
                }

                // Apply IIR filter (8 is the most commonly used)
                int filter = header & 0x0C;
                int p1 = v.buf[pos + brr_buf_size - 1];
                int p2 = v.buf[pos + brr_buf_size - 2] >> 1;
                if (filter >= 8)
                {
                    s += p1;
                    s -= p2;
                    if (filter == 8) // s += p1 * 0.953125 - p2 * 0.46875
                    {
                        s += p2 >> 4;
                        s += (p1 * -3) >> 6;
                    }
                    else // s += p1 * 0.8984375 - p2 * 0.40625
                    {
                        s += (p1 * -13) >> 7;
                        s += (p2 * 3) >> 4;
                    }
                }
                else if (filter != 0) // s += p1 * 0.46875
                {
                    s += p1 >> 1;
                    s += (-p1) >> 5;
                }

                // Adjust and write sample
                s = CLAMP16(s);
                s = (short)(s * 2);
                v.buf[pos + brr_buf_size] = v.buf[pos + 0] = s; // second copy simplifies wrap-around
            }

        }

        void misc_27()
        {
            m.t_pmon = m.regs[(int)GlobalRegisters.r_pmon] & 0xFE; // voice 0 doesn't support PMON
        }

        void misc_28()
        {
            m.t_non = m.regs[(int)GlobalRegisters.r_non];
            m.t_eon = m.regs[(int)GlobalRegisters.r_eon];
            m.t_dir = m.regs[(int)GlobalRegisters.r_dir];
        }

        void misc_29()
        {
            if ((m.every_other_sample ^= 1) != 0)
            {
                m.new_kon &= ~m.kon; // clears KON 63 clocks after it was last read
            }
        }

        void misc_30()
        {
            if (m.every_other_sample != 0)
            {
                m.kon = m.new_kon;
                m.t_koff = m.regs[(int)GlobalRegisters.r_koff] | m.mute_mask;
            }

            run_counters();

            // Noise
            if (read_counter(m.regs[(int)GlobalRegisters.r_flg] & 0x1F) == 0)
            {
                int feedback = (m.noise << 13) ^ (m.noise << 14);
                m.noise = (feedback & 0x4000) ^ (m.noise >> 1);
            }
        }

        void voice_output(voice_t v, int ch)
        {
            // Apply left/right volume
            int amp = (m.t_output * (char)v.regs[(int)VoiceRegisters.v_voll + ch]) >> 7;

            // Add to output total
            m.t_main_out[ch] += amp;
            m.t_main_out[ch] = CLAMP16(m.t_main_out[ch]);

            // Optionally add to echo total
            if ((m.t_eon & v.vbit) != 0)
            {
                m.t_echo_out[ch] += amp;
                m.t_echo_out[ch] = CLAMP16(m.t_echo_out[ch]);
            }
        }

        void voice_V1(voice_t v)
        {
            m.t_dir_addr = m.t_dir * 0x100 + m.t_srcn * 4;
            m.t_srcn = v.regs[(int)VoiceRegisters.v_srcn];
        }
        void voice_V2(voice_t v)
        {
            // Read sample pointer (ignored if not needed)
            int entry = m.t_dir_addr; //uint8_t const* entry = &m.ram [m.t_dir_addr];
            if (v.kon_delay == 0)
            {
                entry += 2;
            }
            m.t_brr_next_addr = Util.GET_LE16A(m.ram, entry);

            m.t_adsr0 = v.regs[(int)VoiceRegisters.v_adsr0];

            // Read pitch, spread over two clocks
            m.t_pitch = v.regs[(int)VoiceRegisters.v_pitchl];
        }
        void voice_V3(voice_t v)
        {
            voice_V3a(v);
            voice_V3b(v);
            voice_V3c(v);
        }
        void voice_V3a(voice_t v)
        {
            m.t_pitch += (v.regs[(int)VoiceRegisters.v_pitchh] & 0x3F) << 8;

        }
        void voice_V3b(voice_t v)
        {
            // Read BRR header and byte
            m.t_brr_byte = m.ram[(v.brr_addr + v.brr_offset) & 0xFFFF];
            m.t_brr_header = m.ram[v.brr_addr]; // brr_addr doesn't need masking
        }
        void voice_V3c(voice_t v)
        {
            // Pitch modulation using previous voice's output
            if ((m.t_pmon & v.vbit) != 0)
            {
                m.t_pitch += ((m.t_output >> 5) * m.t_pitch) >> 10;
            }

            if (v.kon_delay != 0)
            {
                // Get ready to start BRR decoding on next sample
                if (v.kon_delay == 5)
                {
                    v.brr_addr = m.t_brr_next_addr;
                    v.brr_offset = 1;
                    v.buf_pos = 0;
                    m.t_brr_header = 0; // header is ignored on this sample
                    m.kon_check = true;
                }

                // Envelope is never run during KON
                v.env = 0;
                v.hidden_env = 0;

                // Disable BRR decoding until last three samples
                v.interp_pos = 0;
                if ((--v.kon_delay & 3) != 0)
                {
                    v.interp_pos = 0x4000;
                }

                // Pitch is never added during KON
                m.t_pitch = 0;
            }

            // Gaussian interpolation
            {
                int output = interpolate(v);

                // Noise
                if ((m.t_non & v.vbit) != 0)
                {
                    output = (short)(m.noise * 2);
                }

                // Apply envelope
                m.t_output = (output * v.env) >> 11 & ~1;
                v.t_envx_out = (byte)(v.env >> 4);
            }

            // Immediate silence due to end of sample or soft reset
            if ((m.regs[(int)GlobalRegisters.r_flg] & 0x80) != 0 || (m.t_brr_header & 3) == 1)
            {
                v.env_mode = env_mode_t.env_release;
                v.env = 0;
            }

            if (m.every_other_sample != 0)
            {
                // KOFF
                if ((m.t_koff & v.vbit) != 0)
                {
                    v.env_mode = env_mode_t.env_release;
                }

                // KON
                if ((m.kon & v.vbit) != 0)
                {
                    v.kon_delay = 5;
                    v.env_mode = env_mode_t.env_attack;
                }
            }

            // Run envelope for next sample
            if (v.kon_delay == 0)
            {
                run_envelope(v);
            }
        }
        void voice_V4(voice_t v)
        {
            // Decode BRR
            m.t_looped = 0;
            if (v.interp_pos >= 0x4000)
            {
                decode_brr(v);

                if ((v.brr_offset += 2) >= brr_block_size)
                {
                    // Start decoding next BRR block
                    if(v.brr_offset != brr_block_size) // assert(v->brr_offset == brr_block_size);
                    {
                        throw new Exception("v.brr_offset != brr_block_size");
                    }

                    v.brr_addr = (v.brr_addr + brr_block_size) & 0xFFFF;
                    if ((m.t_brr_header & 1) != 0)
                    {
                        v.brr_addr = m.t_brr_next_addr;
                        m.t_looped = v.vbit;
                    }
                    v.brr_offset = 1;
                }
            }

            // Apply pitch
            v.interp_pos = (v.interp_pos & 0x3FFF) + m.t_pitch;

            // Keep from getting too far ahead (when using pitch modulation)
            if (v.interp_pos > 0x7FFF)
            {
                v.interp_pos = 0x7FFF;
            }

            // Output left
            voice_output(v, 0);
        }
        void voice_V5(voice_t v)
        {
            // Output right
            voice_output(v, 1);

            // ENDX, OUTX, and ENVX won't update if you wrote to them 1-2 clocks earlier
            int endx_buf = m.regs[(int)GlobalRegisters.r_endx] | m.t_looped;

            // Clear bit in ENDX if KON just began
            if (v.kon_delay == 5)
            {
                endx_buf &= ~v.vbit;
            }
            m.endx_buf = (byte)endx_buf;
        }
        void voice_V6(voice_t v)
        {
            m.outx_buf = (byte)(m.t_output >> 8);
        }
        void voice_V7(voice_t v)
        {
            // Update ENDX
            m.regs[(int)GlobalRegisters.r_endx] = m.endx_buf;

            m.envx_buf = v.t_envx_out;
        }
        void voice_V8(voice_t v)
        {
            // Update OUTX
            v.regs[(int)VoiceRegisters.v_outx] = m.outx_buf;
        }
        void voice_V9(voice_t v)
        {
            // Update ENVX
            v.regs[(int)VoiceRegisters.v_envx] = m.envx_buf;
        }
        // Common combinations of voice steps on different voices. This greatly reduces
        // code size and allows everything to be inlined in these functions.
        void voice_V7_V4_V1(voice_t[] v, int offset)
        {
            // voice_V7(v); voice_V1(v+3); voice_V4(v+1);
            voice_V7(v[offset + 0]);
            voice_V1(v[offset + 3]);
            voice_V4(v[offset + 1]);
        }
        void voice_V8_V5_V2(voice_t[] v, int offset)
        {
            // voice_V8(v); voice_V5(v+1); voice_V2(v+2);
            voice_V8(v[offset + 0]);
            voice_V5(v[offset + 1]);
            voice_V2(v[offset + 2]);
        }
        void voice_V9_V6_V3(voice_t[] v, int offset)
        {
            // voice_V9(v); voice_V6(v+1); voice_V3(v+2);
            voice_V9(v[offset + 0]);
            voice_V6(v[offset + 1]);
            voice_V3(v[offset + 2]);
        }

        //// Echo

        // Current echo buffer pointer for left/right channel
//#define ECHO_PTR( ch )      (&m.ram [m.t_echo_ptr + ch * 2])

        // Sample in echo history buffer, where 0 is the oldest
//#define ECHO_FIR( i )       (m.echo_hist_pos [i])

        // Calculate FIR point for left/right channel
//#define CALC_FIR( i, ch )   ((ECHO_FIR( i + 1 ) [ch] * (int8_t) REG(fir + i * 0x10)) >> 6)

//#define ECHO_CLOCK( n ) inline void SPC_DSP::echo_##n()

        void echo_read(int ch)
        {
            int ptr = m.t_echo_ptr + ch * 2;
            int s = Util.GET_LE16SA(m.ram, ptr);
            // second copy simplifies wrap-around handling
            m.echo_hist[m.echo_hist_pos + 0,ch] = m.echo_hist[m.echo_hist_pos + 8,ch] = s >> 1;
        }
        int echo_output(int ch)
        {
            int output = (short)((m.t_main_out[ch] * (char)m.regs[(int)GlobalRegisters.r_mvoll + ch * 0x10]) >> 7) +
                    (short)((m.t_echo_in[ch] * (char)m.regs[(int)GlobalRegisters.r_evoll + ch * 0x10]) >> 7);
            output = CLAMP16(output);
            return output;
        }
        void SET_LE16A(byte[] buffer, int addr, short value)
        {
            buffer[addr] = (byte)(value & 0xFF);
            buffer[addr+1] = (byte)((value >> 8) & 0xFF);
        }
        void echo_write(int ch)
        {
            if ((m.t_echo_enabled & 0x20) == 0)
            {
                SET_LE16A(m.ram, m.t_echo_ptr + ch * 2, (short)m.t_echo_out[ch]);
            }
            m.t_echo_out[ch] = 0;
        }
        int CALC_FIR(int i, int ch)
        {
            //#define ECHO_FIR( i )       (m.echo_hist_pos [i])
            //#define CALC_FIR( i, ch )   ((ECHO_FIR( i + 1 ) [ch] * (int8_t) REG(fir + i * 0x10)) >> 6)
            return ((m.echo_hist[m.echo_hist_pos + i + 1, ch] * (char)m.regs[(int)GlobalRegisters.r_fir + i * 0x10]) >> 6);
        }
        void echo_22()
        {
            // History
            if (++m.echo_hist_pos >= echo_hist_size) // &m.echo_hist[echo_hist_size])
            {
                m.echo_hist_pos = 0; // m.echo_hist;
            }

            m.t_echo_ptr = (m.t_esa * 0x100 + m.echo_offset) & 0xFFFF;
            echo_read(0);

            // FIR (using l and r temporaries below helps compiler optimize)
            int l = CALC_FIR(0, 0);
            int r = CALC_FIR(0, 1);

            m.t_echo_in[0] = l;
            m.t_echo_in[1] = r;
        }
        void echo_23()
        {
            int l = CALC_FIR(1, 0) + CALC_FIR(2, 0);
            int r = CALC_FIR(1, 1) + CALC_FIR(2, 1);

            m.t_echo_in[0] += l;
            m.t_echo_in[1] += r;

            echo_read(1);
        }
        void echo_24()
        {
            int l = CALC_FIR(3, 0) + CALC_FIR(4, 0) + CALC_FIR(5, 0);
            int r = CALC_FIR(3, 1) + CALC_FIR(4, 1) + CALC_FIR(5, 1);

            m.t_echo_in[0] += l;
            m.t_echo_in[1] += r;
        }
        void echo_25()
        {
            int l = m.t_echo_in[0] + CALC_FIR(6, 0);
            int r = m.t_echo_in[1] + CALC_FIR(6, 1);

            l = (short)l;
            r = (short)r;

            l += (short)CALC_FIR(7, 0);
            r += (short)CALC_FIR(7, 1);

            l = CLAMP16(l);
            r = CLAMP16(r);

            m.t_echo_in[0] = l & ~1;
            m.t_echo_in[1] = r & ~1;
        }
        void echo_26()
        {
            // Left output volumes
            // (save sample for next clock so we can output both together)
            m.t_main_out[0] = echo_output(0);

            // Echo feedback
            int l = m.t_echo_out[0] + (short)((m.t_echo_in[0] * (char)m.regs[(int)GlobalRegisters.r_efb]) >> 7);
            int r = m.t_echo_out[1] + (short)((m.t_echo_in[1] * (char)m.regs[(int)GlobalRegisters.r_efb]) >> 7);

            l = CLAMP16(l);
            r = CLAMP16(r);

            m.t_echo_out[0] = l & ~1;
            m.t_echo_out[1] = r & ~1;
        }

        /*
#define WRITE_SAMPLES( l, r, out ) \
{\
out [0] = l;\
out [1] = r;\
out += 2;\
if ( out >= m.out_end )\
{\
    check( out == m.out_end );\
    check( m.out_end != &m.extra [extra_size] || \
        (m.extra <= m.out_begin && m.extra < &m.extra [extra_size]) );\
    out       = m.extra;\
    m.out_end = &m.extra [extra_size];\
}\
}\
         */
        short[] WRITE_SAMPLES(short l, short r, short[] output)
        {
            int index = 0;
            output[index] = l;
            output[index+1] = r;
            index += 2;
            if(index >= m.out_end)
            {
                //check(out == m.out_end);\
                if (index == m.out_end) { }
                //check(m.out_end != &m.extra[extra_size] || \
                //    (m.extra <= m.out_begin && m.extra < &m.extra[extra_size]));\
                if (m.out_end != extra_size || (m.extra != m.output)) { } // kinda hard to do this pointer checking
                output = m.extra;
                m.out_end = extra_size;
            }
            return output;
        }
        void echo_27()
        {
            // Output
            int l = m.t_main_out[0];
            int r = echo_output(1);
            m.t_main_out[0] = 0;
            m.t_main_out[1] = 0;

            // TODO: global muting isn't this simple (turns DAC on and off
            // or something, causing small ~37-sample pulse when first muted)
            if ((m.regs[(int)GlobalRegisters.r_flg] & 0x40) != 0)
            {
                l = 0;
                r = 0;
            }

            /*
	        // Output sample to DAC
	        #ifdef SPC_DSP_OUT_HOOK
		        SPC_DSP_OUT_HOOK( l, r );
	        #else
		        sample_t* out = m.out;
		        WRITE_SAMPLES( l, r, out );
		        m.out = out;
	        #endif
            */
            m.output = WRITE_SAMPLES((short)l, (short)r, m.output);
        }
        void echo_28()
        {
            m.t_echo_enabled = m.regs[(int)GlobalRegisters.r_flg];
        }
        void echo_29()
        {
            m.t_esa = m.regs[(int)GlobalRegisters.r_esa];

            if (m.echo_offset == 0)
            {
                m.echo_length = (m.regs[(int)GlobalRegisters.r_edl] & 0x0F) * 0x800;
            }

            m.echo_offset += 4;
            if (m.echo_offset >= m.echo_length)
            {
                m.echo_offset = 0;
            }

            // Write left echo
            echo_write(0);

            m.t_echo_enabled = m.regs[(int)GlobalRegisters.r_flg];
        }
        void echo_30()
        {
            // Write right echo
            echo_write(1);
        }

        void soft_reset_common()
        {
            if(m.ram == null)
            {
                throw new Exception("init must be run before soft_reset_common");
            }
            //require(m.ram); // init() must have been called already

            m.noise = 0x4000;
            m.echo_hist_pos = 0;
            m.every_other_sample = 1;
            m.echo_offset = 0;
            m.phase = 0;

            init_counter();
        }

        /*
// Execute clock for a particular voice
#define V( clock, voice )   voice_##clock( &m.voices [voice] );

/* The most common sequence of clocks uses composite operations
for efficiency. For example, the following are equivalent to the
individual steps on the right:

V(V7_V4_V1,2) -> V(V7,2) V(V4,3) V(V1,5)
V(V8_V5_V2,2) -> V(V8,2) V(V5,3) V(V2,4)
V(V9_V6_V3,2) -> V(V9,2) V(V6,3) V(V3,4) 

// Voice      0      1      2      3      4      5      6      7
#define GEN_DSP_TIMING \
PHASE( 0)  V(V5,0)V(V2,1)\
PHASE( 1)  V(V6,0)V(V3,1)\
PHASE( 2)  V(V7_V4_V1,0)\
PHASE( 3)  V(V8_V5_V2,0)\
PHASE( 4)  V(V9_V6_V3,0)\
PHASE( 5)         V(V7_V4_V1,1)\
PHASE( 6)         V(V8_V5_V2,1)\
PHASE( 7)         V(V9_V6_V3,1)\
PHASE( 8)                V(V7_V4_V1,2)\
PHASE( 9)                V(V8_V5_V2,2)\
PHASE(10)                V(V9_V6_V3,2)\
PHASE(11)                       V(V7_V4_V1,3)\
PHASE(12)                       V(V8_V5_V2,3)\
PHASE(13)                       V(V9_V6_V3,3)\
PHASE(14)                              V(V7_V4_V1,4)\
PHASE(15)                              V(V8_V5_V2,4)\
PHASE(16)                              V(V9_V6_V3,4)\
PHASE(17)  V(V1,0)                            V(V7,5)V(V4,6)\
PHASE(18)                                     V(V8_V5_V2,5)\
PHASE(19)                                     V(V9_V6_V3,5)\
PHASE(20)         V(V1,1)                            V(V7,6)V(V4,7)\
PHASE(21)                                            V(V8,6)V(V5,7)  V(V2,0)  /* t_brr_next_addr order dependency
PHASE(22)  V(V3a,0)                                  V(V9,6)V(V6,7)  echo_22();\
PHASE(23)                                                   V(V7,7)  echo_23();\
PHASE(24)                                                   V(V8,7)  echo_24();\
PHASE(25)  V(V3b,0)                                         V(V9,7)  echo_25();\
PHASE(26)                                                            echo_26();\
PHASE(27) misc_27();                                                 echo_27();\
PHASE(28) misc_28();                                                 echo_28();\
PHASE(29) misc_29();                                                 echo_29();\
PHASE(30) misc_30();V(V3c,0)                                         echo_30();\
PHASE(31)  V(V4,0)       V(V1,2)\
        */
    }
}
