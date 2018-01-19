using System;
using System.Diagnostics;

namespace SNES_SPC
{
    public class SNES_SPC
    {
        //// Init
        // Must be called once before using
        public void init()
        {
            //memset(&m, 0, sizeof m);
            m = new spc_state_t();
            dsp.init(m.ram.ram);

            m.tempo = tempo_unit;

            // Most SPC music doesn't need ROM, and almost all the rest only rely
            // on these two bytes
            m.rom[0x3E] = 0xFF;
            m.rom[0x3F] = 0xC0;

            byte[] cycle_table = new byte[128]
            {//   01   23   45   67   89   AB   CD   EF
                0x28,0x47,0x34,0x36,0x26,0x54,0x54,0x68, // 0
                0x48,0x47,0x45,0x56,0x55,0x65,0x22,0x46, // 1
                0x28,0x47,0x34,0x36,0x26,0x54,0x54,0x74, // 2
                0x48,0x47,0x45,0x56,0x55,0x65,0x22,0x38, // 3
                0x28,0x47,0x34,0x36,0x26,0x44,0x54,0x66, // 4
                0x48,0x47,0x45,0x56,0x55,0x45,0x22,0x43, // 5
                0x28,0x47,0x34,0x36,0x26,0x44,0x54,0x75, // 6
                0x48,0x47,0x45,0x56,0x55,0x55,0x22,0x36, // 7
                0x28,0x47,0x34,0x36,0x26,0x54,0x52,0x45, // 8
                0x48,0x47,0x45,0x56,0x55,0x55,0x22,0xC5, // 9
                0x38,0x47,0x34,0x36,0x26,0x44,0x52,0x44, // A
                0x48,0x47,0x45,0x56,0x55,0x55,0x22,0x34, // B
                0x38,0x47,0x45,0x47,0x25,0x64,0x52,0x49, // C
                0x48,0x47,0x56,0x67,0x45,0x55,0x22,0x83, // D
                0x28,0x47,0x34,0x36,0x24,0x53,0x43,0x40, // E
                0x48,0x47,0x45,0x56,0x34,0x54,0x22,0x60, // F
            };

            // unpack cycle table
            for (int i = 0; i < 128; i++)
            {
                int n = cycle_table[i];
                m.cycle_table[i * 2 + 0] = (byte)(n >> 4);
                m.cycle_table[i * 2 + 1] = (byte)(n & 0x0F);
            }

            reset();
        }

        // Sample pairs generated per second
        public const int sample_rate = 32000;

        /***Emulator use***/

        // Sets IPL ROM data. Library does not include ROM data. Most SPC music files
        // don't need ROM, but a full emulator must provide this.
        public const int rom_size = 0x40;
        public void init_rom(byte[] romIn)
        {
            Array.Copy(romIn, m.rom, m.rom.Length);
        }

        // Sets destination for output samples
        public void set_output(short[] outBuffer, int size) // void SNES_SPC::set_output( sample_t* out, int size )
        {
            //require((size & 1) == 0); // size must be even
            if (size % 2 != 0)
            {
                throw new ArgumentException("set_output buffer size must be a multiple of 2", nameof(outBuffer));
            }

            m.extra_clocks &= clocks_per_sample - 1;
            if (outBuffer != null)
            {
                //sample_t const* out_end = out + size;
                var out_end = size;
                m.buf_begin = outBuffer; m.buf_begin_pointer = 0;
                m.buf_end = out_end;

                // Copy extra to output
                //sample_t const* in = m.extra_buf;
                var inPointer = 0;
                var outPointer = 0;
                //while ( in < m.extra_pos && out < out_end )
                //  *out++ = *in++;
                while (inPointer < m.extra_pos && outPointer < out_end)
                {
                    outBuffer[outPointer++] = m.extra_buf[inPointer++];
                }

                // Handle output being full already
                //if ( out >= out_end )
                if (outPointer >= out_end)
                {
                    // Have DSP write to remaining extra space
                    outBuffer = dsp.extra();
                    outPointer = 0;
                    out_end = outBuffer.Length;

                    // Copy any remaining extra samples as if DSP wrote them
                    while (inPointer < m.extra_pos)
                    {
                        //*out++ = *in++;
                        outBuffer[outPointer++] = m.extra_buf[inPointer++];
                    }
                    //assert( out <= out_end );
                }

                dsp.set_output(outBuffer, out_end - outPointer); // out_end - out );
            }
            else
            {
                reset_buf();
            }
        }


        // Number of samples written to output since last set
        public int sample_count()
        {
            return (m.extra_clocks >> 5) * 2;
        }

        // Resets SPC to power-on state. This resets your output buffer, so you must
        // call set_output() after this.
        public void reset()
        {
            //memset(RAM, 0xFF, 0x10000);
            for (int i = 0; i < m.ram.ram.Length; ++i)
            {
                m.ram.ram[i] = 0xFF;
            }
            ram_loaded();
            reset_common(0x0F);
            dsp.reset();
        }

        // Emulates pressing reset switch on SNES. This resets your output buffer, so
        // you must call set_output() after this.
        public void soft_reset()
        {
            reset_common(0);
            dsp.soft_reset();
        }

        // 1024000 SPC clocks per second, sample pair every 32 clocks
        public const int clock_rate = 1024000;
        public const int clocks_per_sample = 32;

        // Emulated port read/write at specified time
        public const int port_count = 4;
        public int read_port(int t, int port)
        {
            if (port >= port_count)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "read_port port is greater than port_count");
            }

            //int address = run_until_(t);
            run_until_(t);
            return m.smp_regs[0, (int)registers.r_cpuio0 + port]; //run_until_(t)[port]; // todo: this need to get fixed
        }
        public void write_port(int t, int port, byte data)
        {
            if (port >= port_count)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "write_port port is greater than port_count");
            }

            //int address = run_until_(t);
            run_until_(t);
            //run_until_(t)[0x10 + port] = data;
            m.smp_regs[1, (int)registers.r_cpuio0 + port] = data;
        }


        const int cpu_lag_max = 12 - 1; // DIV YA,X takes 12 clocks
        // Runs SPC to end_time and starts a new time frame at 0
        public void end_frame(int end_time)
        {
            // Catch CPU up to as close to end as possible. If final instruction
            // would exceed end, does NOT execute it and leaves m.spc_time < end.
            if (end_time > m.spc_time)
            {
                run_until_(end_time);
            }

            m.spc_time -= end_time;
            m.extra_clocks += end_time;

            // Greatest number of clocks early that emulation can stop early due to
            // not being able to execute current instruction without going over
            // allowed time.
            //assert(-cpu_lag_max <= m.spc_time && m.spc_time <= 0);
            if (!(-cpu_lag_max <= m.spc_time && m.spc_time <= 0))
            {
                //throw new Exception("assert(-cpu_lag_max <= m.spc_time && m.spc_time <= 0);");
            }

            // Catch timers up to CPU
            for (int i = 0; i < timer_count; i++)
            {
                run_timer(m.timers[i], 0);
            }

            // Catch DSP up to CPU
            if (m.dsp_time < 0)
            {
                RUN_DSP(0); //, max_reg_time);
            }

            // Save any extra samples beyond what should be generated
            if (m.buf_begin != null)
            {
                save_extra();
            }
        }

        /***Sound control***/

        // Mutes voices corresponding to non-zero bits in mask (issues repeated KOFF events).
        // Reduces emulation accuracy.
        public const int voice_count = 8;
        public void mute_voices(int mask)
        {
            dsp.mute_voices(mask);
        }

        // If true, prevents channels and global volumes from being phase-negated.
        // Only supported by fast DSP.
        public void disable_surround(bool disable = true)
        {
            dsp.disable_surround(disable);
        }

        // Sets tempo, where tempo_unit = normal, tempo_unit / 2 = half speed, etc.
        public const int tempo_unit = 0x100;
        public void set_tempo(int t)
        {
            m.tempo = t;
            const int timer2_shift = 4; // 64 kHz
            const int other_shift = 3; //  8 kHz

            if (t == 0)
            {
                t = 1;
            }
            const int timer2_rate = 1 << timer2_shift;
            int rate = (timer2_rate * tempo_unit + (t >> 1)) / t;
            if (rate < timer2_rate / 4)
            {
                rate = timer2_rate / 4; // max 4x tempo
            }
            m.timers[2].prescaler = rate;
            m.timers[1].prescaler = rate << other_shift;
            m.timers[0].prescaler = rate << other_shift;
        }

        /***SPC music files***/

        // Loads SPC data into emulator
        public const int spc_min_file_size = 0x10180;
        public const int spc_file_size = 0x10200;
        //char const SNES_SPC::signature[signature_size + 1] =
        //        "SNES-SPC700 Sound File Data v0.30\x1A\x1A";
        const string signature = "SNES-SPC700 Sound File Data v0.30\x1A\x1A";
        public string load_spc(byte[] data, long size)
        {
            //spc_file_t const* const spc = (spc_file_t const*) data;
            spc_file_t spc = LoadSPCFromByteArray(data);

            // be sure compiler didn't insert any padding into fle_t
            //assert(sizeof(spc_file_t) == spc_min_file_size + 0x80);

            // Check signature and file size
            if (size < signature_size || spc.signature != signature) //memcmp(spc, signature, 27))
            {
                return "Not an SPC file";
            }

            if (size < spc_min_file_size)
            {
                return "Corrupt SPC file";
            }

            // CPU registers
            m.cpu_regs.pc = spc.pch * 0x100 + spc.pcl;
            m.cpu_regs.a = spc.a;
            m.cpu_regs.x = spc.x;
            m.cpu_regs.y = spc.y;
            m.cpu_regs.psw = spc.psw;
            m.cpu_regs.sp = spc.sp;

            // RAM and registers
            //memcpy(RAM, spc->ram, 0x10000);
            Array.Copy(spc.ram, m.ram.ram, m.ram.ram.Length);
            ram_loaded();

            // DSP registers
            dsp.load(spc.dsp);

            reset_time_regs();

            return String.Empty;
        }

        private spc_file_t LoadSPCFromByteArray(byte[] data)
        {
            spc_file_t spc = new spc_file_t();
            int pointer = 0;

            spc.signature = System.Text.Encoding.ASCII.GetString(data, pointer, signature_size);
            pointer += signature_size;

            spc.has_id666 = data[pointer++];
            spc.version = data[pointer++];
            spc.pcl = data[pointer++];
            spc.pch = data[pointer++];
            spc.a = data[pointer++];
            spc.x = data[pointer++];
            spc.y = data[pointer++];
            spc.psw = data[pointer++];
            spc.sp = data[pointer++];
            Array.Copy(data, pointer, spc.text, 0, spc_text_size);
            pointer += spc_text_size;
            Array.Copy(data, pointer, spc.ram, 0, 0x10000);
            pointer += 0x10000;
            Array.Copy(data, pointer, spc.dsp, 0, 128);
            pointer += 128;
            Array.Copy(data, pointer, spc.unused, 0, 0x40);
            pointer += 0x40;
            Array.Copy(data, pointer, spc.ipl_rom, 0, 0x40);

            return spc;
        }

        // Clears echo region. Useful after loading an SPC as many have garbage in echo.
        public void clear_echo()
        {
            if ((dsp.read((int)SPC_DSP.GlobalRegisters.r_flg) & 0x20) == 0)
            {
                int addr = 0x100 * dsp.read((int)SPC_DSP.GlobalRegisters.r_esa);
                int end = addr + 0x800 * (dsp.read((int)SPC_DSP.GlobalRegisters.r_edl) & 0x0F);
                if (end > 0x10000)
                {
                    end = 0x10000;
                }
                //memset(&RAM[addr], 0xFF, end - addr);
                for (int i = addr; i < end; ++i)
                {
                    m.ram.ram[i] = 0xFF;
                }
            }
        }

        // Plays for count samples and write samples to out. Discards samples if out
        // is NULL. Count must be a multiple of 2 since output is stereo.
        public string play(int count, short[] output)
        {
            //require((count & 1) == 0); // must be even
            if ((count % 2) != 0)
            {
                throw new ArgumentException("count must be even", nameof(count));
            }
            if (count != 0)
            {
                set_output(output, count);
                end_frame(count * (clocks_per_sample / 2));
            }

            var err = m.cpu_error;
            m.cpu_error = String.Empty;
            return err;
        }

        // Skips count samples. Several times faster than play() when using fast DSP.
        public string skip(int count)
        {
            return play(count, null);
        }

        /***State save/load (only available with accurate DSP)***/

        // Saves/loads state
        public const int state_size = 67 * 1024; // maximum space needed when saving
        public void copy_state(byte[] io, SPC_DSP.copy_func_t copy)
        {
            SPC_State_Copier copier = new SPC_State_Copier(io, copy);

            // Make state data more readable by putting 64K RAM, 16 SMP registers,
            // then DSP (with its 128 registers) first

            // RAM
            enable_rom(0); // will get re-enabled if necessary in regs_loaded() below
            copier.copy(m.ram.ram, 0x10000);

            {
                // SMP registers
                byte[] out_ports = new byte[port_count];
                byte[] regs = new byte[reg_count];
                //memcpy(out_ports, &REGS[r_cpuio0], sizeof out_ports);
                for(int i=0; i<out_ports.Length; ++i)
                {
                    out_ports[i] = m.smp_regs[0, (int)registers.r_cpuio0 + i];
                }
                save_regs(regs, 0);
                copier.copy(regs, regs.Length);
                copier.copy(out_ports, out_ports.Length);
                load_regs(regs, 0);
                regs_loaded();
                //memcpy(&REGS[r_cpuio0], out_ports, sizeof out_ports);
                for(int i=0; i<out_ports.Length; ++i)
                {
                    m.smp_regs[0, (int)registers.r_cpuio0 + i] = out_ports[i];
                }
            }

            // CPU registers
            copier.SPC_COPY_uint16_t(ref m.cpu_regs.pc);
            copier.SPC_COPY_uint8_t(ref m.cpu_regs.a);
            copier.SPC_COPY_uint8_t(ref m.cpu_regs.x);
            copier.SPC_COPY_uint8_t(ref m.cpu_regs.y);
            copier.SPC_COPY_uint8_t(ref m.cpu_regs.psw);
            copier.SPC_COPY_uint8_t(ref m.cpu_regs.sp);
            copier.extra();

            copier.SPC_COPY_int16_t(ref m.spc_time);
            copier.SPC_COPY_int16_t(ref m.dsp_time);

            // DSP
            dsp.copy_state(io, copy);

            // Timers
            for (int i = 0; i < timer_count; i++)
            {
                Timer t = m.timers[i];
                copier.SPC_COPY_int16_t(ref t.next_time);
                copier.SPC_COPY_uint8_t(ref t.divider);
                copier.extra();
            }
            copier.extra();
        }

        // Writes minimal header to spc_out
        public static void init_header(spc_file_t spc_out)//void* spc_out)
        {
            //spc_file_t * const spc = (spc_file_t*)spc_out;
            var spc = spc_out;

            spc.has_id666 = 26; // has none
            spc.version = 30;
            spc.signature = signature; //memcpy(spc, signature, sizeof spc->signature);
            spc.text = new byte[spc_text_size]; //memset(spc->text, 0, sizeof spc->text);
        }

        // Saves emulator state as SPC file data. Writes spc_file_size bytes to spc_out.
        // Does not set up SPC header; use init_header() for that.
        public void save_spc(spc_file_t spc_out) //void* spc_out)
        {
            spc_file_t spc = spc_out;

            // CPU
            spc.pcl = (byte)(m.cpu_regs.pc >> 0);
            spc.pch = (byte)(m.cpu_regs.pc >> 8);
            spc.a = (byte)m.cpu_regs.a;
            spc.x = (byte)m.cpu_regs.x;
            spc.y = (byte)m.cpu_regs.y;
            spc.psw = (byte)m.cpu_regs.psw;
            spc.sp = (byte)m.cpu_regs.sp;

            // RAM, ROM
            //memcpy(spc->ram, RAM, sizeof spc->ram);
            Array.Copy(m.ram.ram, spc.ram, spc.ram.Length);
            if (m.rom_enabled != 0)
            {
                //memcpy(spc->ram + rom_addr, m.hi_ram, sizeof m.hi_ram);
                Array.Copy(m.hi_ram, 0, spc.ram, rom_addr, m.hi_ram.Length);
            }
            //memset(spc->unused, 0, sizeof spc->unused);
            spc.unused = new byte[spc_unused_size];
            //memcpy(spc->ipl_rom, m.rom, sizeof spc->ipl_rom);
            Array.Copy(m.rom, spc.ipl_rom, spc.ipl_rom.Length);

            // SMP registers
            save_regs(spc.ram, 0xF0);
            int i;
            for (i = 0; i < port_count; i++)
            {
                spc.ram[0xF0 + (int)registers.r_cpuio0 + i] = m.smp_regs[1, (int)registers.r_cpuio0 + i];
            }

            // DSP registers
            for (i = 0; i < SPC_DSP.register_count; i++)
            {
                spc.dsp[i] = (byte)dsp.read(i);
            }
        }

        // Returns true if new key-on events occurred since last check. Useful for
        // trimming silence while saving an SPC.
        public bool check_kon()
        {
            return dsp.check_kon();
        }

        public const int reg_count = 0x10;
        public const int timer_count = 3;
        public const int extra_size = 16;

        public const int signature_size = 35;

        SPC_DSP dsp = new SPC_DSP();
        public spc_state_t m;

        public const int rom_addr = 0xFFC0;

        public const int skipping_time = 127;

        // Value that padding should be filled with
        public const byte cpu_pad_fill = 0xFF;

        public enum registers
        {
            r_test = 0x0,
            r_control = 0x1,
            r_dspaddr = 0x2,
            r_dspdata = 0x3,
            r_cpuio0 = 0x4,
            r_cpuio1 = 0x5,
            r_cpuio2 = 0x6,
            r_cpuio3 = 0x7,
            r_f8 = 0x8,
            r_f9 = 0x9,
            r_t0target = 0xA,
            r_t1target = 0xB,
            r_t2target = 0xC,
            r_t0out = 0xD,
            r_t1out = 0xE,
            r_t2out = 0xF
        }

        // Timer registers have been loaded. Applies these to the timers. Does not
        // reset timer prescalers or dividers.
        private void timers_loaded()
        {
            int i;
            for (i = 0; i < timer_count; i++)
            {
                Timer t = m.timers[i];
                t.period = IF_0_THEN_256(m.smp_regs[0, (int)registers.r_t0target + i]);
                t.enabled = m.smp_regs[0, (int)registers.r_control] >> i & 1;
                t.counter = m.smp_regs[1, (int)registers.r_t0out + i] & 0x0F;
            }

            set_tempo(m.tempo);
        }

        private void enable_rom(int enable)
        {
            if (m.rom_enabled != enable)
            {
                m.rom_enabled = enable;
                if (enable != 0)
                {
                    //memcpy(m.hi_ram, &RAM[rom_addr], sizeof m.hi_ram);
                    Array.Copy(m.ram.ram, rom_addr, m.hi_ram, 0, m.hi_ram.Length);
                }
                //memcpy(&RAM[rom_addr], (enable ? m.rom : m.hi_ram), rom_size);
                Array.Copy((enable != 0 ? m.rom : m.hi_ram), 0, m.ram.ram, rom_addr, rom_size);
                // TODO: ROM can still get overwritten when DSP writes to echo buffer
            }
        }

        //// Sample output
        private void reset_buf()
        {
            // Start with half extra buffer of silence
            //sample_t * out = m.extra_buf;
            int index = 0;
            while (index < extra_size / 2) // out < &m.extra_buf[extra_size / 2] )
            {
                //*out++ = 0;
                m.extra_buf[index++] = 0;
            }

            m.extra_pos = index;
            m.buf_begin = null; m.buf_begin_pointer = -1;

            dsp.set_output(null, 0);
        }

        private void save_extra()
        {
            // Get end pointers
            //sample_t const* main_end = m.buf_end;     // end of data written to buf
            var main_end = m.buf_end;
            //sample_t const* dsp_end = dsp.out_pos(); // end of data written to dsp.extra()
            var dsp_end_pointer = dsp.out_pos();
            var dsp_end = dsp.out_buffer();
            if (m.buf_begin_pointer <= dsp_end_pointer && dsp_end_pointer <= main_end)
            {
                main_end = dsp_end_pointer;
                dsp_end = dsp.extra(); // nothing in DSP's extra
                dsp_end_pointer = 0;
            }

            // Copy any extra samples at these ends into extra_buf
            //sample_t * out = m.extra_buf;
            var output = m.extra_buf;
            //sample_t const* in;
            int outputPointer = 0;
            int inputPointer = m.buf_begin_pointer + sample_count();
            for (; inputPointer < main_end; inputPointer++)
            {
                output[outputPointer++] = m.buf_begin[inputPointer];
            }
            var inputBuffer = dsp.extra();
            inputPointer = 0;
            for (; inputPointer < dsp_end_pointer; inputPointer++)
            {
                output[outputPointer++] = inputBuffer[inputPointer];
            }

            m.extra_pos = outputPointer;
            //assert(out <= &m.extra_buf[extra_size]);
        }

        // Loads registers from unified 16-byte format
        private void load_regs(byte[] input, int offset) //uint8_t const in [reg_count] );
        {
            //if(input.Length != reg_count)
            //{
            //    throw new ArgumentException("input must be length of reg_count", nameof(input));
            //}
            //memcpy(REGS, in, reg_count);
            //memcpy(REGS_IN, REGS, reg_count);
            for (int i = 0; i < reg_count; ++i)
            {
                m.smp_regs[1, i] = m.smp_regs[0, i] = input[offset + i];
            }

            // These always read back as 0
            m.smp_regs[1, (int)registers.r_test] = 0;
            m.smp_regs[1, (int)registers.r_control] = 0;
            m.smp_regs[1, (int)registers.r_t0target] = 0;
            m.smp_regs[1, (int)registers.r_t1target] = 0;
            m.smp_regs[1, (int)registers.r_t2target] = 0;
        }

        // RAM was just loaded from SPC, with $F0-$FF containing SMP registers
        // and timer counts. Copies these to proper registers.
        private void ram_loaded()
        {
            m.rom_enabled = 0;
            load_regs(m.ram.ram, 0xF0);

            // Put STOP instruction around memory to catch PC underflow/overflow
            //memset(m.ram.padding1, cpu_pad_fill, sizeof m.ram.padding1);
            for (int i = 0; i < m.ram.padding1.Length; ++i)
            {
                m.ram.padding1[i] = cpu_pad_fill;
            }
            //memset(m.ram.padding2, cpu_pad_fill, sizeof m.ram.padding2);
            for (int i = 0; i < m.ram.padding2.Length; ++i)
            {
                m.ram.padding2[i] = cpu_pad_fill;
            }
        }

        // Registers were just loaded. Applies these new values.
        private void regs_loaded()
        {
            enable_rom(m.smp_regs[0, (int)registers.r_control] & 0x80);
            timers_loaded();
        }

        private void reset_time_regs()
        {
            m.cpu_error = String.Empty;
            m.echo_accessed = false;
            m.spc_time = 0;
            m.dsp_time = 0;

            for (int i = 0; i < timer_count; i++)
            {
                Timer t = m.timers[i];
                t.next_time = 1;
                t.divider = 0;
            }

            regs_loaded();

            m.extra_clocks = 0;
            reset_buf();
        }

        private void reset_common(int timer_counter_init)
        {
            int i;
            for (i = 0; i < timer_count; i++)
            {
                m.smp_regs[1, (int)registers.r_t0out + i] = (byte)timer_counter_init;
            }

            // Run IPL ROM
            //memset(&m.cpu_regs, 0, sizeof m.cpu_regs);
            ClearCPURegisters();
            m.cpu_regs.pc = rom_addr;

            m.smp_regs[0, (int)registers.r_test] = 0x0A;
            m.smp_regs[0, (int)registers.r_control] = 0xB0; // ROM enabled, clear ports
            for (i = 0; i < port_count; i++)
            {
                m.smp_regs[1, (int)registers.r_cpuio0 + i] = 0;
            }

            reset_time_regs();
        }

        private void ClearCPURegisters()
        {
            m.cpu_regs.pc = 0;
            m.cpu_regs.a = 0;
            m.cpu_regs.x = 0;
            m.cpu_regs.y = 0;
            m.cpu_regs.psw = 0;
            m.cpu_regs.sp = 0;
        }

        //#define TIMER_DIV( t, n ) ((n) / t->prescaler)
        public int TIMER_DIV(Timer t, int n)
        {
            return ((n) / t.prescaler);
        }
        //#define TIMER_MUL( t, n ) ((n) * t->prescaler)
        public int TIMER_MUL(Timer t, int n)
        {
            return ((n) * t.prescaler);
        }

        // (n ? n : 256)
        //#define IF_0_THEN_256( n ) ((uint8_t) ((n) - 1) + 1)
        public int IF_0_THEN_256(int n)
        {
            return ((byte)((n) - 1) + 1);
        }

        private Timer run_timer_(Timer t, int time)
        {
            int elapsed = TIMER_DIV(t, time - t.next_time) + 1;
            t.next_time += TIMER_MUL(t, elapsed);

            if (t.enabled != 0)
            {
                int remain = IF_0_THEN_256(t.period - t.divider);
                int divider = t.divider + elapsed;
                int over = elapsed - remain;
                if (over >= 0)
                {
                    int n = over / t.period;
                    t.counter = (t.counter + 1 + n) & 0x0F;
                    divider = over - n * t.period;
                }
                t.divider = (byte)divider;
            }
            return t;
        }

        private Timer run_timer(Timer t, int time)
        {
            if (time >= t.next_time)
            {
                t = run_timer_(t, time);
            }
            return t;
        }

        /*
        #define RUN_DSP( time, offset ) \
            {\
                int count = (time) - m.dsp_time;\
                if ( !SPC_MORE_ACCURACY || count )\
                {\
                    assert( count > 0 );\
                    m.dsp_time = (time);\
                    dsp.run( count );\
                }\
            }
        */
        internal void RUN_DSP(int time) //, int offset)
        {
            {
                int count = (time) - m.dsp_time;
                if (count != 0)
                {
                    m.dsp_time = (time);
                    dsp.run(count);
                }
            }

        }
        private int dsp_read(int time)
        {
            RUN_DSP(time); //, reg_times[REGS[r_dspaddr] & 0x7F]);

            int result = dsp.read(m.smp_regs[0, (int)registers.r_dspaddr] & 0x7F);

            return result;
        }

        private void dsp_write(int data, int time)
        {
            RUN_DSP(time); //, reg_times[REGS[r_dspaddr]])
            if (m.smp_regs[0, (int)registers.r_dspaddr] <= 0x7F)
            {
                dsp.write(m.smp_regs[0, (int)registers.r_dspaddr], data);
            }
            //else if (!SPC_MORE_ACCURACY)
            //    dprintf("SPC wrote to DSP register > $7F\n");

        }

        // divided into multiple functions to keep rarely-used functionality separate
        // so often-used functionality can be optimized better by compiler

        // If write isn't preceded by read, data has this added to it
        const int no_read_before_write = 0x2000;
        private void cpu_write_smp_reg_(int data, int time, int addr)
        {
            switch (addr)
            {
                case (int)registers.r_t0target:
                case (int)registers.r_t1target:
                case (int)registers.r_t2target:
                    Timer t = m.timers[addr - (int)registers.r_t0target];
                    int period = IF_0_THEN_256(data);
                    if (t.period != period)
                    {
                        t = run_timer(t, time);
                        t.period = period;
                    }
                    break;

                case (int)registers.r_t0out:
                case (int)registers.r_t1out:
                case (int)registers.r_t2out:
                    //if ( !SPC_MORE_ACCURACY )
                    //    dprintf( "SPC wrote to counter %d\n", (int) addr - r_t0out );

                    if (data < no_read_before_write / 2)
                    {
                        var temp_timer = run_timer(m.timers[addr - (int)registers.r_t0out], time - 1);
                        temp_timer.counter = 0;
                    }
                    break;

                // Registers that act like RAM
                case 0x8:
                case 0x9:
                    m.smp_regs[1, addr] = (byte)data;
                    break;

                case (int)registers.r_test:
                    //if ( (uint8_t) data != 0x0A )
                    //    dprintf( "SPC wrote to test register\n" );
                    break;

                case (int)registers.r_control:
                    // port clears
                    if ((data & 0x10) != 0)
                    {
                        m.smp_regs[1, (int)registers.r_cpuio0] = 0;
                        m.smp_regs[1, (int)registers.r_cpuio1] = 0;
                    }
                    if ((data & 0x20) != 0)
                    {
                        m.smp_regs[1, (int)registers.r_cpuio2] = 0;
                        m.smp_regs[1, (int)registers.r_cpuio3] = 0;
                    }

                    // timers
                    {
                        for (int i = 0; i < timer_count; i++)
                        {
                            Timer tempTimer = m.timers[i];
                            int enabled = data >> i & 1;
                            if (tempTimer.enabled != enabled)
                            {
                                tempTimer = run_timer(tempTimer, time);
                                tempTimer.enabled = enabled;
                                if (enabled != 0)
                                {
                                    tempTimer.divider = 0;
                                    tempTimer.counter = 0;
                                }
                            }
                        }
                    }
                    enable_rom(data & 0x80);
                    break;
            }
        }

        private void cpu_write_smp_reg(int data, int time, int addr)
        {
            if (addr == (int)registers.r_dspdata) // 99%
            {
                dsp_write(data, time);
            }
            else
            {
                cpu_write_smp_reg_(data, time, addr);
            }
        }

        private void cpu_write_high(int data, int i, int time)
        {
            if (i < rom_size)
            {
                m.hi_ram[i] = (byte)data;
                if (m.rom_enabled != 0)
                {
                    m.ram.ram[i + rom_addr] = m.rom[i]; // restore overwritten ROM
                }
            }
            else
            {
                //assert(RAM[i + rom_addr] == (uint8_t)data);
                if (m.ram.ram[i + rom_addr] != (byte)data)
                {
                    throw new Exception("m.ram.ram[i + rom_addr] != (byte)data");
                }
                m.ram.ram[i + rom_addr] = cpu_pad_fill; // restore overwritten padding
                cpu_write(data, i + rom_addr - 0x10000, time);
            }
        }

        const int bits_in_int = 32; // CHAR_BIT * sizeof(int);
        private void cpu_write(int data, int addr, int time)
        {
            //MEM_ACCESS(time, addr)
            if (!check_echo_access((ushort)addr))
            {
                // ?
            }

            // RAM
            m.ram.ram[addr] = (byte)data;
            int reg = addr - 0xF0;
            if (reg >= 0) // 64%
            {
                // $F0-$FF
                if (reg < reg_count) // 87%
                {
                    m.smp_regs[0, reg] = (byte)data;

                    // Ports
                    //# ifdef SPC_PORT_WRITE_HOOK
                    //                    if ((unsigned)(reg - r_cpuio0) < port_count)
                    //                        SPC_PORT_WRITE_HOOK(m.spc_time + time, (reg - r_cpuio0),
                    //                                (uint8_t)data, &REGS[r_cpuio0]);
                    //#endif

                    // Registers other than $F2 and $F4-$F7
                    if ( reg != 2 && reg != 4 && reg != 5 && reg != 6 && reg != 7 )
                    // TODO: this is a bit on the fragile side
                    //if (((~0x2F00 << (bits_in_int - 16)) << reg) < 0) // 36%
                    {
                        cpu_write_smp_reg(data, time, reg);
                    }
                }
                // High mem/address wrap-around
                else
                {
                    reg -= rom_addr - 0xF0;
                    if (reg >= 0) // 1% in IPL ROM area or address wrapped around
                    {
                        cpu_write_high(data, reg, time);
                    }
                }
            }
        }

        //// CPU read

        private int cpu_read_smp_reg(int reg, int time)
        {
            int result = m.smp_regs[1, reg];
            reg -= (int)registers.r_dspaddr;
            // DSP addr and data
            if ((UInt32)reg <= 1) // 4% 0xF2 and 0xF3
            {
                result = m.smp_regs[0, (int)registers.r_dspaddr];
                if ((UInt32)reg == 1)
                {
                    result = dsp_read(time); // 0xF3
                }
            }
            return result;
        }

        private int cpu_read(int addr, int time)
        {
            //MEM_ACCESS(time, addr)
            if (!check_echo_access((ushort)addr))
            {
                // ?
            }

            // RAM
            int result = m.ram.ram[addr];
            int reg = addr - 0xF0;
            if (reg >= 0) // 40%
            {
                reg -= 0x10;
                if ((UInt32)reg >= 0xFF00) // 21%
                {
                    reg += 0x10 - (int)registers.r_t0out;

                    // Timers
                    if ((UInt32)reg < timer_count) // 90%
                    {
                        Timer t = m.timers[reg];
                        if (time >= t.next_time)
                        {
                            t = run_timer_(t, time);
                        }
                        result = t.counter;
                        t.counter = 0;
                    }
                    // Other registers
                    else if (reg < 0) // 10%
                    {
                        result = cpu_read_smp_reg(reg + (int)registers.r_t0out, time);
                    }
                    else // 1%
                    {
                        //assert(reg + (r_t0out + 0xF0 - 0x10000) < 0x100);
                        if (reg + ((int)registers.r_t0out + 0xF0 - 0x10000) >= 0x100)
                        {
                            throw new Exception("assert(reg + (r_t0out + 0xF0 - 0x10000) < 0x100);");
                        }
                        result = cpu_read(reg + ((int)registers.r_t0out + 0xF0 - 0x10000), time);
                    }
                }
            }

            return result;
        }

        //#define MEM_BIT( rel ) CPU_mem_bit( pc, rel_time + rel )
        //#define READ_PC16( pc ) GET_LE16( pc )
        //#define READ(  time, addr )                 CPU_READ ( rel_time, TIME_ADJ(time), (addr) )
        //#define CPU_READ( time, offset, addr )\
        //    cpu_read(addr, time + offset )
        //#define TIME_ADJ( n )   (n)

        private UInt32 CPU_mem_bit(byte[] pc, int pcPointer, int rel_time) // uint8_t const* pc, rel_time_t );
        {
            int addr = (int)Util.READ_PC16(pc, pcPointer);
            //UInt32 t = READ(0, addr & 0x1FFF) >> (addr >> 13);
            UInt32 t = (UInt32)(cpu_read((int)(addr & 0x1FFF), rel_time) >> (addr >> 13));
            return t << 8 & 0x100;
        }

        private bool check_echo_access(int addr)
        {
            if ((dsp.read((int)SPC_DSP.GlobalRegisters.r_flg) & 0x20) == 0)
            {
                int start = 0x100 * dsp.read((int)SPC_DSP.GlobalRegisters.r_esa);
                int size = 0x800 * (dsp.read((int)SPC_DSP.GlobalRegisters.r_edl) & 0x0F);
                int end = start + (size != 0 ? size : 4);
                if (start <= addr && addr < end)
                {
                    if (!m.echo_accessed)
                    {
                        m.echo_accessed = true;
                        return true;
                    }
                }
            }
            return false;

        }
        //#define MEM_ACCESS( time, addr ) check( !check_echo_access( (uint16_t) addr ) );

        const int spc_text_size = 212;
        const int spc_ram_size = 0x10000;
        const int spc_dsp_size = 128;
        const int spc_unused_size = 0x40;
        const int spc_ipl_rom_size = 0x40;
        public class spc_file_t
        {
            public string signature;
            public byte has_id666;
            public byte version;
            public byte pcl, pch;
            public byte a;
            public byte x;
            public byte y;
            public byte psw;
            public byte sp;
            public byte[] text = new byte[spc_text_size];
            public byte[] ram = new byte[spc_ram_size];
            public byte[] dsp = new byte[spc_dsp_size];
            public byte[] unused = new byte[spc_unused_size];
            public byte[] ipl_rom = new byte[spc_ipl_rom_size];
        }

        private void save_regs(byte[] output, int offset)
        {
            // Use current timer counter values
            for (int i = 0; i < timer_count; i++)
            {
                //out [r_t0out + i] = m.timers[i].counter;
                output[offset + (int)registers.r_t0out + i] = (byte)m.timers[i].counter;
            }
    
            // Last written values
            //memcpy( out, REGS, r_t0out );
            for(int i=0; i<(int)registers.r_t0out; i++)
            {
                output[offset + i] = m.smp_regs[0, i];
            }
        }


        //#define SUSPICIOUS_OPCODE( name ) dprintf( "SPC: suspicious opcode: " name "\n" )
        void SUSPICIOUS_OPCODE(string name)
        {
            Debug.WriteLine($"SPC: suspicious opcode: {name}");
        }

        //#define CPU_READ( time, offset, addr )\
        //    cpu_read(addr, time + offset )
        int CPU_READ(int time, int offset, int addr)
        {
            return cpu_read(addr, time + offset);
        }

        //#define CPU_WRITE( time, offset, addr, data )\
        //    cpu_write(data, addr, time + offset )
        void CPU_WRITE(int time, int offset, int addr, int data)
        {
            cpu_write(data, addr, time + offset);
        }

        // timers are by far the most common thing read from dp
        //#define CPU_READ_TIMER( time, offset, addr_, out )\
        //{\
        //	rel_time_t adj_time = time + offset;\
        //	int dp_addr = addr_;\
        //	int ti = dp_addr - (r_t0out + 0xF0);\
        //	if ( (unsigned) ti<timer_count )\
        //	{\
        //		Timer* t = &m.timers[ti];\
        //		if ( adj_time >= t->next_time )\
        //			t = run_timer_(t, adj_time );\
        //		out = t->counter;\
        //		t->counter = 0;\
        //	}\
        //	else\
        //	{\
        //		out = ram[dp_addr];\
        //		int i = dp_addr - 0xF0;\
        //		if ( (unsigned) i< 0x10 )\
        //			out = cpu_read_smp_reg( i, adj_time );\
        //	}\
        //}
        void CPU_READ_TIMER(int time, int offset, int addr_, ref int output)
        {
            int adj_time = time + offset;
            int dp_addr = addr_;
            int ti = dp_addr - (((int)registers.r_t0out) + 0xF0);
            if((UInt32)ti < timer_count)
            {
                Timer t = m.timers[ti];
                if(adj_time >= t.next_time)
                {
                    t = run_timer_(t, adj_time);
                }
                output = t.counter;
                t.counter = 0;
            }
            else
            {
                output = m.ram.ram[dp_addr];
                int i = dp_addr - 0xF0;
                if((UInt32)i < 0x10)
                {
                    output = cpu_read_smp_reg(i, adj_time);
                }
            }
        }

        //#define TIME_ADJ( n )   (n)

        int TIME_ADJ(int n) { return n; }

        //#define READ_TIMER( time, addr, out )       CPU_READ_TIMER( rel_time, TIME_ADJ(time), (addr), out )
        void READ_TIMER(int rel_time, int time, int addr, ref int output)
        {
            CPU_READ_TIMER(rel_time, TIME_ADJ(time), (addr), ref output);
        }

        //#define READ(  time, addr )                 CPU_READ ( rel_time, TIME_ADJ(time), (addr) )
        int READ(int rel_time, int time, int addr)
        {
            return CPU_READ(rel_time, TIME_ADJ(time), (addr));
        }

        //#define WRITE( time, addr, data )           CPU_WRITE( rel_time, TIME_ADJ(time), (addr), (data) )
        void WRITE(int rel_time, int time, int addr, int data)
        {
            CPU_WRITE(rel_time, TIME_ADJ(time), (addr), (data));
        }

        //#define DP_ADDR( addr )                     (dp + (addr))
        int DP_ADDR(int dp, uint addr)
        {
            return (int)(dp + addr);
        }

        //#define READ_DP_TIMER(  time, addr, out )   CPU_READ_TIMER( rel_time, TIME_ADJ(time), DP_ADDR( addr ), out )
        void READ_DP_TIMER(int rel_time, int dp, int time, uint addr, ref int output)
        {
            CPU_READ_TIMER(rel_time, TIME_ADJ(time), DP_ADDR(dp, addr), ref output);
        }

        //#define READ_DP(  time, addr )              READ ( time, DP_ADDR( addr ) )
        int READ_DP(int rel_time, int dp, int time, int addr)
        {
            return READ(rel_time, time, DP_ADDR(dp, (uint)addr));
        }

        //#define WRITE_DP( time, addr, data )        WRITE( time, DP_ADDR( addr ), data )
        void WRITE_DP(int rel_time, int dp, int time, int addr, int data)
        {
            WRITE(rel_time, time, DP_ADDR(dp, (uint)addr), data);
        }

        //#define READ_PROG16( addr )                 GET_LE16( ram + (addr) )
        ushort READ_PROG16(int addr)
        {
            return Util.GET_LE16(m.ram.ram, addr);
        }

        int _pc = 0;
        //#define SET_PC( n )     (pc = ram + (n))
        void SET_PC(int n)
        {
            _pc = n;
        }

        //#define GET_PC()        (pc - ram)
        int GET_PC()
        {
            return _pc;
        }

        //#define READ_PC( pc )   (*(pc))
        byte READ_PC(int pc)
        {
            return m.ram.ram[pc];
        }

        //#define READ_PC16( pc ) GET_LE16( pc )
        ushort READ_PC16(int pc)
        {
            return Util.GET_LE16(m.ram.ram, pc);
        }

        int _sp = 0;
        //#define SET_SP( v )     (sp = ram + 0x101 + (v))
        void SET_SP(int v)
        {
            _sp = 0x101 + v;
        }

        //#define GET_SP()        (sp - 0x101 - ram)
        int GET_SP()
        {
            return _sp - 0x101;
        }

        /*
        #define PUSH16( data )\
        {\
            int addr = (sp -= 2) - ram;\
            if ( addr > 0x100 )\
            {\
                SET_LE16( sp, data );\
            }\
            else\
            {\
                ram [(uint8_t) addr + 0x100] = (uint8_t) data;\
                sp [1] = (uint8_t) (data >> 8);\
                sp += 0x100;\
            }\
        }
        */
        void PUSH16(uint data)
        {
            int addr = (_sp -= 2);
            if(addr > 0x100)
            {
                Util.SET_LE16(m.ram.ram, _sp, data);
            }
            else
            {
                m.ram.ram[(byte)addr + 0x100] = (byte)data;
                m.ram.ram[_sp + 1] = (byte)(data >> 8);
                _sp += 0x100;
            }
        }

        /*
        #define PUSH( data )\
        {\
            *--sp = (uint8_t) (data);\
            if ( sp - ram == 0x100 )\
                sp += 0x100;\
        }
        */
        void PUSH(int data)
        {
            m.ram.ram[--_sp] = (byte)(data);
            if(_sp == 0x100)
            {
                _sp += 0x100;
            }
        }

        /*
        #define POP( out )\
        {\
            out = *sp++;\
            if ( sp - ram == 0x201 )\
            {\
                out = sp [-0x101];\
                sp -= 0x100;\
            }\
        }
        */
        void POP(ref int output)
        {
            output = m.ram.ram[_sp++];
            if(_sp == 0x201)
            {
                output = m.ram.ram[_sp - 0x101];
                _sp -= 0x100;
            }
        }

        //#define MEM_BIT( rel ) CPU_mem_bit( pc, rel_time + rel )
        UInt32 MEM_BIT(int rel_time, int rel)
        {
            return CPU_mem_bit(m.ram.ram, _pc, rel_time + rel);
        }

        //// Status flag handling

        // Hex value in name to clarify code and bit shifting.
        // Flag stored in indicated variable during emulation
        public const int n80 = 0x80; // nz
        public const int v40 = 0x40; // psw
        public const int p20 = 0x20; // dp
        public const int b10 = 0x10; // psw
        public const int h08 = 0x08; // psw
        public const int i04 = 0x04; // psw
        public const int z02 = 0x02; // nz
        public const int c01 = 0x01; // c

        const int nz_neg_mask = 0x880; // either bit set indicates N flag set

        /*
        #define GET_PSW( out )\
        {\
            out = psw & ~(n80 | p20 | z02 | c01);\
            out |= c  >> 8 & c01;\
            out |= dp >> 3 & p20;\
            out |= ((nz >> 4) | nz) & n80;\
            if ( !(uint8_t) nz ) out |= z02;\
        }
        */
        int _psw = 0;
        int _dp = 0;
        int _nz = 0;
        public void GET_PSW(ref int output)
        {
            output = _psw & ~(n80 | p20 | z02 | c01);
            output |= _c >> 8 & c01;
            output |= _dp >> 3 & p20;
            output |= ((_nz >> 4) | _nz) & n80;
            if((byte)_nz == 0)
            {
                output |= z02;
            }
        }

        /*
        #define SET_PSW( in )\
        {\
            psw = in;\
            c   = in << 8;\
            dp  = in << 3 & 0x100;\
            nz  = (in << 4 & 0x800) | (~in & z02);\
        }
        */
        int _c = 0;
        private uint nz;

        void SET_PSW(int input)
        {
            _psw = input;
            _c = input << 8;
            _dp = input << 3 & 0x100;
            _nz = (input << 4 & 0x800) | (~input & z02);
        }

        /*
        #define BRANCH( cond )\
        {\
            pc++;\
            pc += (BOOST::int8_t)data;\
            if (cond)\
                goto loop;\
            pc -= (BOOST::int8_t)data;\
            rel_time -= 2;\
            goto loop;\
        }
        */
        void BRANCH_TOP(uint data)
        {
            _pc++;
            _pc += (sbyte)data;
        }

        void BRANCH_BOTTOM(ref int rel_time, uint data)
        {
            _pc -= (sbyte)data;
            rel_time -= 2;
        }

        // TODO: wtf....
        private int run_until_(int end_time)
        {
            //// Run

            int rel_time = m.spc_time;
            rel_time -= end_time;
            if (rel_time > 0)
            {
                throw new Exception("rel_time > 0");
            }
            m.spc_time = end_time;
            m.dsp_time += rel_time;
            m.timers[0].next_time += rel_time;
            m.timers[1].next_time += rel_time;
            m.timers[2].next_time += rel_time;

            byte[] ram = m.ram.ram;
            int a = m.cpu_regs.a;
            int x = m.cpu_regs.x;
            int y = m.cpu_regs.y;
            _pc = 0;
            _sp = 0;
            _psw = 0;
            _c = 0;
            _nz = 0;
            _dp = 0;

            int temp = 0;
            int addr = 0;
            int addr2 = 0;

            SET_PC(m.cpu_regs.pc);
            SET_SP(m.cpu_regs.sp);
            SET_PSW(m.cpu_regs.psw);

            goto loop;

        cbranch_taken_loop:
            _pc += (sbyte)ram[_pc];
        inc_pc_loop:
            _pc++;
        loop:
            {
                uint opcode;
                uint data;

                //check((unsigned)a < 0x100);
                //check((unsigned)x < 0x100);
                //check((unsigned)y < 0x100);

                // was caused by rewind, but let's leave this here just in case we find another problem
                //if(_pc == 0x857 && a == 0x2d && x == 0x46 && y == 0x9 && (uint)_c == 0xFFFFFFFB && _nz == 0x2D && _sp == 0x1D0 && _psw == 0x2)
                //{
                //    Debugger.Break();
                //}

                opcode = ram[_pc];

                if ((rel_time += m.cycle_table[opcode]) > 0)
                {
                    goto out_of_time;
                }

                // prepare to wait if you turn this on
                //opcodes.Trace((int)opcode, a, x, y, _sp, _dp, _pc, this, ram);

                // TODO: if PC is at end of memory, this will get wrong operand (very obscure)
                if (++_pc >= 0x10000)
                {
                    Debug.WriteLine("uh oh. gonna wrap the _pc");
                    data = 0;
                }
                else
                {
                    data = ram[_pc];
                }

                switch(opcode)
                {
                    // Common instructions

                    /*
                    #define BRANCH( cond )\
                    {\
                        pc++;\
                        pc += (BOOST::int8_t)data;\
                        if (cond)\
                            goto loop;\
                        pc -= (BOOST::int8_t)data;\
                        rel_time -= 2;\
                        goto loop;\
                    }
                    */
                    /*
                    BRANCH_TOP(data);
                    if(cond) goto loop;
                    BRANCH_BOTTOM(ref rel_time, data);
                    goto loop;
                    */
                    case 0xF0: // BEQ
                        BRANCH_TOP(data); // 89% taken
                        if ((byte)_nz == 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;

                    case 0xD0: // BNE
                        BRANCH_TOP(data);
                        if ((byte)_nz != 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;

                    case 0x3F:
                        {// CALL
                            uint old_addr = (uint)GET_PC() + 2;
                            SET_PC(READ_PC16(_pc));
                            PUSH16(old_addr);
                            goto loop;
                        }

                    case 0x6F:// RET
                        {
                            addr = _sp;
                            SET_PC(Util.GET_LE16(ram, _sp));
                            _sp += 2;
                            if (addr < 0x1FF)
                            {
                                goto loop;
                            }

                            SET_PC(ram[_sp - 0x101] * 0x100 + ram[(byte)addr + 0x100]);
                            _sp -= 0x100;
                        }
                        goto loop;

                    case 0xE4: // MOV a,dp
                        ++_pc;
                        // 80% from timer
                        READ_DP_TIMER(rel_time, _dp, 0, data, ref _nz);
                        a = _nz;
                        goto loop;

                    case 0xFA:
                    case 0x8F:
                        if(opcode == 0xFA) // fake the fallthrough
                        {// MOV dp,dp
                            temp = 0;
                            READ_DP_TIMER(rel_time, _dp, -2, data, ref temp);
                            data = (uint)(temp + no_read_before_write);
                        }
                    // fall through
                    //case 0x8F:
                        {// MOV dp,#imm
                            temp = READ_PC(_pc + 1);
                            _pc += 2;
                            {
                                int i = _dp + temp;
                                ram[i] = (byte)data;
                                i -= 0xF0;
                                if ((uint)i < 0x10) // 76%
                                {
                                    m.smp_regs[0, i] = (byte)data;

                                    // Registers other than $F2 and $F4-$F7
                                    //if ( i != 2 && i != 4 && i != 5 && i != 6 && i != 7 )
                                    if (((~0x2F00 << (bits_in_int - 16)) << i) < 0) // 12%
                                    {
                                        cpu_write_smp_reg((int)data, rel_time, i);
                                    }
                                }
                            }
                            goto loop;
                        }

                    case 0xC4: // MOV dp,a
                        ++_pc;
                        {
                            int i = (int)(_dp + data);
                            ram[i] = (byte)a;
                            i -= 0xF0;
                            if ((uint)i < 0x10) // 39%
                            {
                                uint sel = (uint)(i - 2);
                                m.smp_regs[0, i] = (byte)a;

                                if (sel == 1) // 51% $F3
                                {
                                    dsp_write(a, rel_time);
                                }
                                else if (sel > 1) // 1% not $F2 or $F3
                                {
                                    cpu_write_smp_reg_(a, rel_time, i);
                                }
                            }
                        }
                        goto loop;

                    /*
                    #define CASE( n )   case n:

                                // Define common address modes based on opcode for immediate mode. Execution
                                // ends with data set to the address of the operand.
                    #define ADDR_MODES_( op )\
                                CASE(op - 0x02) // (X) \
                            data = x + dp;\
                            pc--;\
                            goto end_##op;\
                        CASE(op + 0x0F) // (dp)+Y \
                            data = READ_PROG16(data + dp) + y;\
                            goto end_##op;\
                        CASE(op - 0x01) // (dp+X) \
                            data = READ_PROG16(((uint8_t)(data + x)) + dp);\
                            goto end_##op;\
                        CASE(op + 0x0E) // abs+Y \
                            data += y;\
                            goto abs_##op;\
                        CASE(op + 0x0D) // abs+X \
                            data += x;\
                        CASE(op - 0x03) // abs \
                        abs_##op:\
                            data += 0x100 * READ_PC(++pc);\
                            goto end_##op;\
                        CASE(op + 0x0C) // dp+X \
                            data = (uint8_t)(data + x);

                    #define ADDR_MODES_NO_DP( op )\
                                ADDR_MODES_(op)\
                            data += dp;\
                        end_##op:

                    #define ADDR_MODES( op )\
                                ADDR_MODES_(op)\
                        CASE(op - 0x04) // dp \
                            data += dp;\
                        end_##op:

                    */
                    // 1. 8-bit Data Transmission Commands. Group I
                    //ADDR_MODES_NO_DP(0xE8) // MOV A,addr
                    case 0xE8 - 0x02:
                        data = (uint)(x + _dp);
                        _pc--;
                        goto end_0xE8;
                    case 0xE8 + 0x0F:
                        data = (uint)(Util.get_le16(ram, ((int)(data + _dp))) + y);
                        goto end_0xE8;
                    case 0xE8 - 0x01:
                        data = (uint)Util.get_le16(ram, (((byte)(data + x)) + _dp));
                        goto end_0xE8;
                    case 0xE8 + 0x0E:
                        data += (uint)y;
                        goto abs_0xE8;
                    case 0xE8 + 0x0D:
                    case 0xE8 - 0x03:
                        if (opcode == 0xE8 + 0x0D) // fake fallthrough
                        {
                            data += (uint)x;
                        }
                    //case 0xE8 - 0x03:
                    abs_0xE8:
                        data += (uint)(0x100 * ram[++_pc]); // (*(++pc));
                            goto end_0xE8;
                    case 0xE8 + 0x0C:
                        data = (byte)(data + x);
                        data += (uint)_dp;
                    end_0xE8:
                        a = _nz = cpu_read(((int)data), rel_time + (0));
                        goto inc_pc_loop;


                    case 0xBF:
                        {// MOV A,(X)+
                            temp = x + _dp;
                            x = (byte)(x + 1);
                            a = _nz = READ(rel_time, -1, temp);
                            goto loop;
                        }

                    case 0xE8: // MOV A,imm
                        a = (int)data;
                        _nz = (int)data;
                        goto inc_pc_loop;

                    case 0xF9: // MOV X,dp+Y
                    case 0xF8: // MOV X,dp
                        if (opcode == 0xF9) // fake fallthrough
                        {
                            data = (byte)(data + y);
                        }
                    //case 0xF8: // MOV X,dp
                        READ_DP_TIMER(rel_time, _dp, 0, data, ref _nz);
                        x = _nz;
                        goto inc_pc_loop;

                    case 0xE9: // MOV X,abs
                    case 0xCD: // MOV X,imm
                        if (opcode == 0xE9) // fake fallthrough
                        {
                            data = (uint)READ_PC16(_pc);
                            ++_pc;
                            data = (uint)READ(rel_time, 0, (int)data);
                        }
                    //case 0xCD: // MOV X,imm
                        x = (int)data;
                        _nz = (int)data;
                        goto inc_pc_loop;

                    case 0xFB: // MOV Y,dp+X
                    case 0xEB: // MOV Y,dp
                        if (opcode == 0xFB)
                        {
                            data = (byte)(data + x);
                        }
                    //case 0xEB: // MOV Y,dp
                               // 70% from timer
                        _pc++;
                        READ_DP_TIMER(rel_time, _dp, 0, data, ref _nz);
                        y = _nz;
                        goto loop;

                    case 0xEC:
                        {// MOV Y,abs
                            temp = READ_PC16(_pc);
                            _pc += 2;
                            READ_TIMER(rel_time, 0, temp, ref _nz);
                            y = _nz;
                            //y = nz = READ( 0, temp );
                            goto loop;
                        }

                    case 0x8D: // MOV Y,imm
                        y = (int)data;
                        _nz = (int)data;
                        goto inc_pc_loop;

                    // 2. 8-BIT DATA TRANSMISSION COMMANDS, GROUP 2
                    case 0xC8 - 0x02:
                        data = (uint)(x + _dp);
                        _pc--;
                        goto end_0xC8;
                    case 0xC8 + 0x0F:
                        data = (uint)(Util.get_le16(ram, ((int)(data + _dp))) + y);
                        goto end_0xC8;
                    case 0xC8 - 0x01:
                        data = (uint)Util.get_le16(ram, (((byte)(data + x)) + _dp));
                        goto end_0xC8;
                    case 0xC8 + 0x0E:
                        data += (uint)y;
                        goto abs_0xC8;
                    case 0xC8 + 0x0D:
                    case 0xC8 - 0x03:
                        if (opcode == 0xC8 + 0x0D) // fake fallthrough
                        {
                            data += (uint)x;
                        }
                    //case 0xC8 - 0x03:
                    abs_0xC8:
                        data += (uint)(0x100 * ram[++_pc]); // (*(++pc));
                        goto end_0xC8;
                    case 0xC8 + 0x0C:
                        data = (byte)(data + x);
                        data += (uint)_dp;
                    end_0xC8:
                        cpu_write((a), ((int)data), rel_time + (0));
                        goto inc_pc_loop;

                    case 0xCC: // MOV abs,Y
                    case 0xC9: // MOV abs,X
                        temp = 0;
                        if (opcode == 0xCC)
                        {
                            temp = y;
                            goto mov_abs_temp;
                        }
                    //case 0xC9: // MOV abs,X
                        temp = x;
                    mov_abs_temp:
                        WRITE(rel_time, 0, READ_PC16(_pc), temp);
                        _pc += 2;
                        goto loop;

                    case 0xD9: // MOV dp+Y,X
                    case 0xD8: // MOV dp,X
                        if (opcode == 0xD9) // fake fallthrough
                        {
                            data = (byte)(data + y);
                        }
                    //case 0xD8: // MOV dp,X
                        WRITE(rel_time, 0, (int)(data + _dp), x);
                        goto inc_pc_loop;

                    case 0xDB: // MOV dp+X,Y
                    case 0xCB: // MOV dp,Y
                        if (opcode == 0xDB) // fake fallthrough
                        {
                            data = (byte)(data + x);
                        }
                    //case 0xCB: // MOV dp,Y
                        WRITE(rel_time, 0, (int)(data + _dp), y);
                        goto inc_pc_loop;

                    // 3. 8-BIT DATA TRANSMISSIN COMMANDS, GROUP 3.
                    case 0x7D: // MOV A,X
                        a = x;
                        _nz = x;
                        goto loop;

                    case 0xDD: // MOV A,Y
                        a = y;
                        _nz = y;
                        goto loop;

                    case 0x5D: // MOV X,A
                        x = a;
                        _nz = a;
                        goto loop;

                    case 0xFD: // MOV Y,A
                        y = a;
                        _nz = a;
                        goto loop;

                    case 0x9D: // MOV X,SP
                        x = _nz = GET_SP();
                        goto loop;

                    case 0xBD: // MOV SP,X
                        SET_SP(x);
                        goto loop;

                    //case 0xC6: // MOV (X),A (handled by MOV addr,A in group 2)

                    case 0xAF: // MOV (X)+,A
                        WRITE_DP(rel_time, _dp, 0, x, a + no_read_before_write);
                        x++;
                        goto loop;

                    // 5. 8-BIT LOGIC OPERATION COMMANDS

                    /*
                    #define LOGICAL_OP( op, func )\
                                ADDR_MODES(op) // addr \
                            data = READ(0, data);\
                        case op: // imm \
                            nz = a func##= data;\
                            goto inc_pc_loop;\
                        {
                                    unsigned addr;\
                        case op + 0x11: // X,Y \
                            data = READ_DP(-2, y);\
                            addr = x + dp;\
                            goto addr_##op;\
                        case op + 0x01: // dp,dp \
                            data = READ_DP(-3, data);\
                        case op + 0x10:{/dp,imm\
                            uint8_t const* addr2 = pc + 1;\
                            pc += 2;\
                            addr = READ_PC(addr2) + dp;\
                        }\
                        addr_##op:\
                            nz = data func READ(-1, addr );\
                            WRITE(0, addr, nz);\
                            goto loop;\
                        }
        */
                    // LOGICAL_OP(0x28, & ); // AND

                    case 0x28 - 0x02:
                        data = (uint)(x + _dp);
                        _pc--;
                        goto end_0x28;
                    case 0x28 + 0x0F:
                        data = (uint)(Util.get_le16(ram, ((int)(data + _dp))) + y);
                        goto end_0x28;
                    case 0x28 - 0x01:
                        data = (uint)Util.get_le16(ram, (((byte)(data + x)) + _dp));
                        goto end_0x28;
                    case 0x28 + 0x0E:
                        data += (uint)y;
                        goto abs_0x28;
                    case 0x28 + 0x0D:
                    case 0x28 - 0x03:
                        if (opcode == 0x28 + 0x0D) // fake fallthrough
                        {
                            data += (uint)x;
                        }
                        //case 0x28 - 0x03:
                    abs_0x28:
                        data += (uint)(0x100 * ram[++_pc]); // (*(++pc));
                        goto end_0x28;
                    case 0x28 + 0x0C:
                    case 0x28 - 0x04:
                    case 0x28:
                        if (opcode == 0x28 + 0x0C) // fake fallthrough
                        {
                            data = (byte)(data + x);
                        }
                    //case 0x28 - 0x04:
                        if (opcode == 0x28 + 0x0C || opcode == 0x28 - 0x04) // fake fallthrough
                        {
                            data += (uint)_dp;
                        }
                    end_0x28:
                        if (opcode != 0x28) // thanks c# for not allowing fallthrough
                        {
                            data = (uint)cpu_read(((int)data), rel_time + (0));
                        }
                    //case 0x28:
                        _nz = a &= (int)data;
                        goto inc_pc_loop;

                    case 0x28 + 0x11:
                        data = (uint)cpu_read(((_dp + (y))), rel_time + (-2));
                        addr = x + _dp;
                        goto addr_0x28;
                    case 0x28 + 0x01:
                    case 0x28 + 0x10:
                        if (opcode == 0x28 + 0x01)
                        {
                            data = (uint)cpu_read(((_dp + (int)(data))), rel_time + (-3));
                        }
                    //case 0x28 + 0x10:
                    {
                        //uint8_t const* addr2 = pc + 1;
                        addr2 = _pc + 1;
                        _pc += 2;
                        addr = ram[addr2] + _dp;  //(*(addr2)) + dp;
                    }
                    addr_0x28:
                        _nz = (int)(data & cpu_read((addr), rel_time + (-1)));
                        cpu_write((_nz), (addr), rel_time + (0));
                        goto loop;

                    //LOGICAL_OP(0x08, | ); // OR
                    case 0x08 - 0x02:
                        data = (uint)(x + _dp);
                        _pc--;
                        goto end_0x08;
                    case 0x08 + 0x0F:
                        data = (uint)(Util.get_le16(ram, ((int)(data + _dp))) + y);
                        goto end_0x08;
                    case 0x08 - 0x01:
                        data = (uint)Util.get_le16(ram, (((byte)(data + x)) + _dp));
                        goto end_0x08;
                    case 0x08 + 0x0E:
                        data += (uint)y;
                        goto abs_0x08;
                    case 0x08 + 0x0D:
                    case 0x08 - 0x03:
                        if (opcode == 0x08 + 0x0D)
                        {
                            data += (uint)x;
                        }
                    //case 0x08 - 0x03:
                        abs_0x08:
                        data += (uint)(0x100 * ram[++_pc]); // (*(++pc));
                        goto end_0x08;
                    case 0x08 + 0x0C:
                    case 0x08 - 0x04:
                    case 0x08:
                        if (opcode == 0x08 + 0x0C)
                        {
                            data = (byte)(data + x);
                        }
                    //case 0x08 - 0x04:
                        if (opcode == 0x08 + 0x0C || opcode == 0x08 - 0x04)
                        {
                            data += (uint)_dp;
                        }
                    end_0x08:
                        if (opcode != 0x08)
                        {
                            data = (uint)cpu_read(((int)data), rel_time + (0));
                        }
                    //case 0x08:
                        _nz = a |= (int)data;
                        goto inc_pc_loop;

                    case 0x08 + 0x11:
                        data = (uint)cpu_read(((_dp + (y))), rel_time + (-2));
                        addr = x + _dp;
                        goto addr_0x08;
                    case 0x08 + 0x01:
                    case 0x08 + 0x10:
                        if (opcode == 0x08 + 0x01)
                        {
                            data = (uint)cpu_read((((int)(_dp + (data)))), rel_time + (-3));
                        }
                    //case 0x08 + 0x10:
                        {
                            //uint8_t const* addr2 = pc + 1;
                            addr2 = _pc + 1;
                            _pc += 2;
                            addr = ram[addr2] + _dp;
                        }
                    addr_0x08:
                        _nz = (int)((int)data | cpu_read((addr), rel_time + (-1)));
                        cpu_write((_nz), (addr), rel_time + (0));
                        goto loop;


                    //LOGICAL_OP(0x48, ^ ); // EOR
                    case 0x48 - 0x02:
                        data = (uint)(x + _dp);
                        _pc--;
                        goto end_0x48;
                    case 0x48 + 0x0F:
                        data = (uint)(Util.get_le16(ram, ((int)(data + _dp))) + y);
                        goto end_0x48;
                    case 0x48 - 0x01:
                        data = (uint)Util.get_le16(ram, (((byte)(data + x)) + _dp));
                        goto end_0x48;
                    case 0x48 + 0x0E:
                        data += (uint)y;
                        goto abs_0x48;
                    case 0x48 + 0x0D:
                    case 0x48 - 0x03:
                        if (opcode == 0x48 + 0x0D)
                        {
                            data += (uint)x;
                        }
                    //case 0x48 - 0x03:
                        abs_0x48:
                        data += (uint)(0x100 * ram[++_pc]); // (*(++pc));
                        goto end_0x48;
                    case 0x48 + 0x0C:
                    case 0x48 - 0x04:
                    case 0x48:
                        if (opcode == 0x48 + 0x0C)
                        {
                            data = (byte)(data + x);
                        }
                    //case 0x48 - 0x04:
                        if (opcode == 0x48 + 0x0C || opcode == 0x48 - 0x04)
                        {
                            data += (uint)_dp;
                        }
                    end_0x48:
                        if (opcode != 0x48)
                        {
                            data = (uint)cpu_read(((int)data), rel_time + (0));
                        }
                    //case 0x48:
                        _nz = a ^= (int)data;
                        goto inc_pc_loop;

                    case 0x48 + 0x11:
                        data = (uint)cpu_read(((_dp + (y))), rel_time + (-2));
                        addr = x + _dp;
                        goto addr_0x48;
                    case 0x48 + 0x01:
                    case 0x48 + 0x10:
                        if (opcode == 0x48 + 0x01)
                        {
                            data = (uint)cpu_read(((_dp + ((int)data))), rel_time + (-3));
                        }
                    //case 0x48 + 0x10:
                        {
                            //uint8_t const* addr2 = pc + 1;
                            addr2 = _pc + 1;
                            _pc += 2;
                            addr = ram[addr2] + _dp;
                        }
                    addr_0x48:
                        _nz = (int)(data) ^ cpu_read((addr), rel_time + (-1));
                        cpu_write((_nz), (addr), rel_time + (0));
                        goto loop;

                    // 4. 8-BIT ARITHMETIC OPERATION COMMANDS
                    case 0x68 - 0x02:
                        data = (uint)(x + _dp);
                        _pc--;
                        goto end_0x68;
                    case 0x68 + 0x0F:
                        data = (uint)(Util.get_le16(ram, ((int)data + _dp)) + y);
                        goto end_0x68;
                    case 0x68 - 0x01:
                        data = (uint)Util.get_le16(ram, (((byte)(data + x)) + _dp));
                        goto end_0x68;
                    case 0x68 + 0x0E:
                        data += (uint)y;
                        goto abs_0x68;
                    case 0x68 + 0x0D:
                    case 0x68 - 0x03:
                        if (opcode == 0x68 + 0x0D)
                        {
                            data += (uint)x;
                        }
                    //case 0x68 - 0x03:
                    abs_0x68:
                        data += (uint)(0x100 * ram[++_pc]); // (*(++pc));
                        goto end_0x68;
                    case 0x68 + 0x0C:
                    case 0x68 - 0x04:
                    case 0x68: // CMP imm
                        if (opcode == 0x68 + 0x0C)
                        {
                            data = (byte)(data + x);
                        }
                    //case 0x68 - 0x04:
                        if (opcode == 0x68 + 0x0C || opcode == 0x68 - 0x04)
                        {
                            data += (uint)_dp;
                        }
                    end_0x68:
                        if (opcode != 0x68)
                        {
                            data = (uint)cpu_read(((int)data), rel_time + (0));
                        }
                    //case 0x68: // CMP imm
                        _nz = a - (int)data;
                        _c = ~_nz;
                        _nz &= 0xFF;
                        goto inc_pc_loop;

                    case 0x79: // CMP (X),(Y)
                        data = (uint)READ_DP(rel_time, _dp, -2, y);
                        nz = (uint)(READ_DP(rel_time, _dp, -1, x) - data);
                        _c = ~_nz;
                        _nz &= 0xFF;
                        goto loop;

                    case 0x69: // CMP dp,dp
                    case 0x78: // CMP dp,imm
                        if (opcode == 0x69)
                        {
                            data = (uint)READ_DP(rel_time, _dp, -3, (int)data);
                        }
                    //case 0x78: // CMP dp,imm
                        _nz = (int)(READ_DP(rel_time, _dp, -1, READ_PC(++_pc)) - data);
                        _c = ~_nz;
                        _nz &= 0xFF;
                        goto inc_pc_loop;

                    case 0x3E: // CMP X,dp
                        data += (uint)_dp;
                        goto cmp_x_addr;
                    case 0x1E: // CMP X,abs
                    case 0xC8: // CMP X,imm
                        if (opcode == 0x1E)
                        {
                            data = (uint)READ_PC16(_pc);
                            _pc++;
                        }
                    cmp_x_addr:
                        if (opcode != 0xC8)
                        {
                            data = (uint)READ(rel_time, 0, (int)data);
                        }
                    //case 0xC8: // CMP X,imm
                        _nz = (int)(x - data);
                        _c = ~_nz;
                        _nz &= 0xFF;
                        goto inc_pc_loop;

                    case 0x7E: // CMP Y,dp
                        data += (uint)_dp;
                        goto cmp_y_addr;
                    case 0x5E: // CMP Y,abs
                    case 0xAD: // CMP Y,imm
                        if (opcode == 0x5E)
                        {
                            data = (uint)READ_PC16(_pc);
                            _pc++;
                        }
                    cmp_y_addr:
                        if (opcode != 0xAD)
                        {
                            data = (uint)READ(rel_time, 0, (int)data);
                        }
                    //case 0xAD: // CMP Y,imm
                        _nz = (int)(y - data);
                        _c = ~_nz;
                        _nz &= 0xFF;
                        goto inc_pc_loop;

                    case 0xB9: // SBC (x),(y)
                    case 0x99: // ADC (x),(y)
                        _pc--; // compensate for inc later
                        data = (uint)READ_DP(rel_time, _dp, -2, y);
                        addr = x + _dp;
                        goto adc_addr;
                    case 0xA9: // SBC dp,dp
                    case 0x89: // ADC dp,dp
                    case 0xB8: // SBC dp,imm
                    case 0x98: // ADC dp,imm
                        if (opcode == 0xA9 || opcode == 0x89)
                        {
                            data = (uint)READ_DP(rel_time, _dp, -3, (int)data);
                        }
                    //case 0xB8: // SBC dp,imm
                    //case 0x98: // ADC dp,imm
                        addr = READ_PC(++_pc) + _dp;
                    adc_addr:
                        _nz = READ(rel_time, -1, addr);
                        goto adc_data;
                    /*
                    // catch ADC and SBC together, then decode later based on operand
                    #undef CASE
                    #define CASE( n ) case n: case (n) + 0x20:

                    ADDR_MODES(0x88) // ADC/SBC addr
                        data = READ(0, data);
                    */
                    case 0x88 - 0x02:
                    case (0x88 - 0x02) + 0x20:
                        data = (uint)(x + _dp);
                        _pc--;
                        goto end_0x88;
                    case 0x88 + 0x0F:
                    case (0x88 + 0x0F) + 0x20:
                        data = (uint)(Util.get_le16(ram, ((int)data + _dp)) + y);
                        goto end_0x88;
                    case 0x88 - 0x01:
                    case (0x88 - 0x01) + 0x20:
                        data = (uint)(Util.get_le16(ram, (((byte)(data + x)) + _dp)));
                        goto end_0x88;
                    case 0x88 + 0x0E:
                    case (0x88 + 0x0E) + 0x20:
                        data += (uint)y;
                        goto abs_0x88;
                    case 0x88 + 0x0D:
                    case (0x88 + 0x0D) + 0x20:
                    case 0x88 - 0x03:
                    case (0x88 - 0x03) + 0x20:
                        if (opcode == 0x88 + 0x0D || opcode == (0x88 + 0x0D) + 0x20)
                        {
                            data += (uint)x;
                        }
                    //case 0x88 - 0x03:
                    //case (0x88 - 0x03) + 0x20:
                    abs_0x88:
                        data += (uint)(0x100 * ram[++_pc]); // (*(++pc));
                        goto end_0x88;
                    case 0x88 + 0x0C:
                    case (0x88 + 0x0C) + 0x20:
                    case 0x88 - 0x04:
                    case (0x88 - 0x04) + 0x20:
                    case 0xA8: // SBC imm
                    case 0x88: // ADC imm
                        if (opcode == 0x88 + 0x0C || opcode == (0x88 + 0x0C) + 0x20)
                        {
                            data = (byte)(data + x);
                        }
                    //case 0x88 - 0x04:
                    //case (0x88 - 0x04) + 0x20:
                        if (opcode != 0xA8 && opcode != 0x88)
                        {
                            data += (uint)_dp;
                        }
                    end_0x88:
                        if (opcode != 0xA8 && opcode != 0x88)
                        {
                            data = (uint)cpu_read(((int)data), rel_time + (0));
                        }
                    //case 0xA8: // SBC imm
                    //case 0x88: // ADC imm
                        addr = -1; // A
                        _nz = a;
                    adc_data:
                    {
                        int flags;
                        if (opcode >= 0xA0) // SBC
                        {
                            data ^= 0xFF;
                        }

                        flags = (int)(data ^ _nz);
                        _nz += (int)data + (_c >> 8 & 1);
                        flags ^= _nz;

                        _psw = (_psw & ~(v40 | h08)) | (flags >> 1 & h08) | ((flags + 0x80) >> 2 & v40);
                        _c = _nz;
                        if (addr < 0)
                        {
                            a = (byte)_nz;
                            goto inc_pc_loop;
                        }
                        WRITE(rel_time, 0, addr, _nz);
                        goto inc_pc_loop;

                    }

                    // 6. ADDITION & SUBTRACTION COMMANDS
                    /*
                    #define INC_DEC_REG( reg, op )\
                                nz = reg op;\
                            reg = (uint8_t)nz;\
                            goto loop;
                    
                    */
                    // case 0xBC: INC_DEC_REG(a, +1) // INC A
                    case 0xBC:
                        _nz = a + 1;
                        a = (byte)_nz;
                        goto loop;
                    // case 0x3D: INC_DEC_REG(x, +1) // INC X
                    case 0x3D:
                        _nz = x + 1;
                        x = (byte)_nz;
                        goto loop;
                    // case 0xFC: INC_DEC_REG(y, +1) // INC Y
                    case 0xFC:
                        _nz = y + 1;
                        y = (byte)_nz;
                        goto loop;

                    // case 0x9C: INC_DEC_REG(a, -1) // DEC A
                    case 0x9C:
                        _nz = a - 1;
                        a = (byte)_nz;
                        goto loop;
                    // case 0x1D: INC_DEC_REG(x, -1) // DEC X
                    case 0x1D:
                        _nz = x - 1;
                        x = (byte)_nz;
                        goto loop;
                    // case 0xDC: INC_DEC_REG(y, -1) // DEC Y
                    case 0xDC:
                        _nz = y - 1;
                        y = (byte)_nz;
                        goto loop;

                    case 0x9B: // DEC dp+X
                    case 0xBB: // INC dp+X
                    case 0x8B: // DEC dp
                    case 0xAB: // INC dp
                        if (opcode == 0x9B || opcode == 0xBB)
                        {
                            data = (byte)(data + x);
                        }
                    //case 0x8B: // DEC dp
                    //case 0xAB: // INC dp
                        data += (uint)_dp;
                        goto inc_abs;

                    case 0x8C: // DEC abs
                    case 0xAC: // INC abs
                        data = (uint)READ_PC16(_pc);
                        _pc++;
                    inc_abs:
                        _nz = (int)((opcode >> 4 & 2) - 1);
                        _nz += READ(rel_time, -1, (int)data);
                        WRITE(rel_time, 0, (int)data, _nz);
                        goto inc_pc_loop;

                    // 7. SHIFT, ROTATION COMMANDS
                    case 0x5C: // LSR A
                    case 0x7C:
                        if (opcode == 0x5C)
                        {
                            _c = 0;
                        }
                    //case 0x7C:
                        {// ROR A
                            _nz = (_c >> 1 & 0x80) | (a >> 1);
                            _c = a << 8;
                            a = _nz;
                            goto loop;
                        }

                    case 0x1C: // ASL A
                    case 0x3C:
                        if (opcode == 0x1C)
                        {
                            _c = 0;
                        }
                    //case 0x3C:
                        {// ROL A
                            temp = _c >> 8 & 1;
                            _c = a << 1;
                            _nz = _c | temp;
                            a = (byte)_nz;
                            goto loop;
                        }

                    case 0x0B: // ASL dp
                        _c = 0;
                        data += (uint)_dp;
                        goto rol_mem;

                    case 0x1B: // ASL dp+X
                    case 0x3B: // ROL dp+X
                    case 0x2B: // ROL dp
                        if (opcode == 0x1B)
                        {
                            _c = 0;
                        }
                        //case 0x3B: // ROL dp+X
                        if (opcode == 0x1B || opcode == 0x3B)
                        {
                            data = (byte)(data + x);
                        }
                    //case 0x2B: // ROL dp
                        data += (uint)_dp;
                        goto rol_mem;

                    case 0x0C: // ASL abs
                    case 0x2C: // ROL abs
                        if (opcode == 0x0C)
                        {
                            _c = 0;
                        }
                    //case 0x2C: // ROL abs
                        data = (uint)READ_PC16(_pc);
                        _pc++;
                    rol_mem:
                        _nz = _c >> 8 & 1;
                        _nz |= (_c = READ(rel_time, -1, (int)data) << 1);
                        WRITE(rel_time, 0, (int)data, _nz);
                        goto inc_pc_loop;

                    case 0x4B: // LSR dp
                        _c = 0;
                        data += (uint)_dp;
                        goto ror_mem;
                    case 0x5B: // LSR dp+X
                    case 0x7B: // ROR dp+X
                    case 0x6B: // ROR dp
                        if (opcode == 0x5B)
                        {
                            _c = 0;
                        }
                    //case 0x7B: // ROR dp+X
                        if (opcode == 0x5B || opcode == 0x7B)
                        {
                            data = (byte)(data + x);
                        }
                    //case 0x6B: // ROR dp
                        data += (uint)_dp;
                        goto ror_mem;
                    case 0x4C: // LSR abs
                    case 0x6C: // ROR abs
                        if (opcode == 0x4C)
                        {
                            _c = 0;
                        }
                    //case 0x6C: // ROR abs
                        data = (uint)READ_PC16(_pc);
                        _pc++;
                    ror_mem:
                        {
                            temp = READ(rel_time, -1, (int)data);
                            _nz = (_c >> 1 & 0x80) | (temp >> 1);
                            _c = temp << 8;
                            WRITE(rel_time, 0, (int)data, _nz);
                            goto inc_pc_loop;
                        }

                    case 0x9F: // XCN
                        _nz = a = (a >> 4) | (byte)(a << 4);
                        goto loop;

                    // 8. 16-BIT TRANSMISION COMMANDS
                    case 0xBA: // MOVW YA,dp
                        a = READ_DP(rel_time, _dp, -2, (int)data);
                        _nz = (a & 0x7F) | (a >> 1);
                        y = READ_DP(rel_time, _dp, 0, (byte)(data + 1));
                        _nz |= y;
                        goto inc_pc_loop;

                    case 0xDA: // MOVW dp,YA
                        WRITE_DP(rel_time, _dp, -1, (int)data, a);
                        WRITE_DP(rel_time, _dp, 0, (byte)(data + 1), y + no_read_before_write);
                        goto inc_pc_loop;

                    // 9. 16-BIT OPERATION COMMANDS

                    case 0x3A: // INCW dp
                    case 0x1A:
                        {// DECW dp
                            //int temp;
                            // low byte
                            data += (uint)_dp;
                            temp = READ(rel_time, -3, (int)data);
                            temp += ((int)opcode >> 4 & 2) - 1; // +1 for INCW, -1 for DECW
                            _nz = ((temp >> 1) | temp) & 0x7F;
                            WRITE(rel_time, -2, (int)data, temp);

                            // high byte
                            data = (uint)((byte)(data + 1) + _dp);
                            temp = (byte)((temp >> 8) + READ(rel_time, -1, (int)data));
                            _nz |= temp;
                            WRITE(rel_time, 0, (int)data, temp);

                            goto inc_pc_loop;
                        }

                    case 0x7A: // ADDW YA,dp
                    case 0x9A:
                        {// SUBW YA,dp
                            int lo = READ_DP(rel_time, _dp, -2, (int)data);
                            int hi = READ_DP(rel_time, _dp, 0, (byte)(data + 1));
                            int result;
                            int flags;

                            if (opcode == 0x9A) // SUBW
                            {
                                lo = (lo ^ 0xFF) + 1;
                                hi ^= 0xFF;
                            }

                            lo += a;
                            result = y + hi + (lo >> 8);
                            flags = hi ^ y ^ result;

                            _psw = (_psw & ~(v40 | h08)) | (flags >> 1 & h08) | ((flags + 0x80) >> 2 & v40);
                            _c = result;
                            a = (byte)lo;
                            result = (byte)result;
                            y = result;
                            _nz = (((lo >> 1) | lo) & 0x7F) | result;

                            goto inc_pc_loop;
                        }

                    case 0x5A:
                        { // CMPW YA,dp
                            temp = a - READ_DP(rel_time, _dp, -1, (int)data);
                            _nz = ((temp >> 1) | temp) & 0x7F;
                            temp = y + (temp >> 8);
                            temp -= READ_DP(rel_time, _dp, 0, (byte)(data + 1));
                            _nz |= temp;
                            _c = ~temp;
                            _nz &= 0xFF;
                            goto inc_pc_loop;
                        }

                    // 10. MULTIPLICATION & DIVISON COMMANDS

                    case 0xCF:
                        { // MUL YA
                            temp = y * a;
                            a = (byte)temp;
                            //_nz = ((temp >> 1) | temp) & 0x7F;
                            y = temp >> 8;
                            //_nz = y; // todo: match bsnes for now
                            _nz = (temp < 0) ? n80 : 0;
                            _nz |= y != 0 ? z02 : 0;
                            goto loop;
                        }

                    case 0x9E: // DIV YA,X
                        {
                            int ya = y * 0x100 + a;

                            _psw &= ~(h08 | v40);

                            if (y >= x)
                                _psw |= v40;

                            if ((y & 15) >= (x & 15))
                                _psw |= h08;

                            if (y < x * 2)
                            {
                                a = ya / x;
                                y = ya - a * x;
                            }
                            else
                            {
                                a = 255 - (ya - x * 0x200) / (256 - x);
                                y = x + (ya - x * 0x200) % (256 - x);
                            }

                            _nz = (byte)a;
                            a = (byte)a;

                            goto loop;
                        }

                    // 11. DECIMAL COMPENSATION COMMANDS

                    case 0xDF: // DAA
                        SUSPICIOUS_OPCODE("DAA");
                        if (a > 0x99 || (_c & 0x100) != 0)
                        {
                            a += 0x60;
                            _c = 0x100;
                        }

                        if ((a & 0x0F) > 9 || (_psw & h08) != 0)
                        {
                            a += 0x06;
                        }

                        _nz = a;
                        a = (byte)a;
                        goto loop;

                    case 0xBE: // DAS
                        SUSPICIOUS_OPCODE("DAS");
                        if (a > 0x99 || (_c & 0x100) == 0)
                        {
                            a -= 0x60;
                            _c = 0;
                        }

                        if ((a & 0x0F) > 9 || (_psw & h08) == 0)
                        {
                            a -= 0x06;
                        }

                        _nz = a;
                        a = (byte)a;
                        goto loop;

                    // 12. BRANCHING COMMANDS

                    case 0x2F: // BRA rel
                        _pc += (sbyte)data;
                        goto inc_pc_loop;

                    /*
                    BRANCH_TOP(data);
                    if(cond) goto loop;
                    BRANCH_BOTTOM(ref rel_time, data);
                    goto loop;
                    */
                    case 0x30: // BMI
                        //BRANCH((nz & nz_neg_mask))
                        BRANCH_TOP(data);
                        if ((_nz & nz_neg_mask) != 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;


                    case 0x10: // BPL
                        //BRANCH(!(nz & nz_neg_mask))
                        BRANCH_TOP(data);
                        if ((_nz & nz_neg_mask) == 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;

                    case 0xB0: // BCS
                        //BRANCH(c & 0x100)
                        BRANCH_TOP(data);
                        if ((_c & 0x100) != 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;

                    case 0x90: // BCC
                        //BRANCH(!(c & 0x100))
                        BRANCH_TOP(data);
                        if ((_c & 0x100) == 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;

                    case 0x70: // BVS
                        //BRANCH(psw & v40)
                        BRANCH_TOP(data);
                        if ((_psw & v40) != 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;

                    case 0x50: // BVC
                        //BRANCH(!(psw & v40))
                        BRANCH_TOP(data);
                        if ((_psw & v40) == 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;

                    /*
                    #define CBRANCH( cond )\
                            {\
                        pc++;\
                        if (cond)\
                            goto cbranch_taken_loop;\
                        rel_time -= 2;\
                        goto inc_pc_loop;\
                    }
                    */
                    case 0x03: // BBS dp.bit,rel
                    case 0x23:
                    case 0x43:
                    case 0x63:
                    case 0x83:
                    case 0xA3:
                    case 0xC3:
                    case 0xE3:
                        //CBRANCH(READ_DP(-4, data) >> (opcode >> 5) & 1)
                        {
                            _pc++;
                            if((READ_DP(rel_time, _dp, -4, (int)data) >> (int)(opcode >> 5) & 1) != 0) { goto cbranch_taken_loop; }
                            rel_time -= 2;
                            goto inc_pc_loop;
                        }

                    case 0x13: // BBC dp.bit,rel
                    case 0x33:
                    case 0x53:
                    case 0x73:
                    case 0x93:
                    case 0xB3:
                    case 0xD3:
                    case 0xF3:
                        //CBRANCH(!(READ_DP(-4, data) >> (opcode >> 5) & 1))
                        {
                            _pc++;
                            if ((READ_DP(rel_time, _dp, -4, (int)data) >> (int)(opcode >> 5) & 1) == 0) { goto cbranch_taken_loop; }
                            rel_time -= 2;
                            goto inc_pc_loop;
                        }

                    case 0xDE: // CBNE dp+X,rel
                    case 0x2E:
                        if (opcode == 0xDE)
                        {
                            data = (byte)(data + x);
                        }
                    // fall through
                    //case 0x2E:
                        {// CBNE dp,rel
                            //int temp;
                            // 61% from timer
                            READ_DP_TIMER(rel_time, _dp, -4, data, ref temp);
                            //CBRANCH(temp != a)
                            {
                                _pc++;
                                if (temp != a) { goto cbranch_taken_loop; }
                                rel_time -= 2;
                                goto inc_pc_loop;
                            }

                        }

                    case 0x6E:
                        { // DBNZ dp,rel
                            temp = READ_DP(rel_time, _dp, -4, (int)data) - 1;
                            WRITE_DP(rel_time, _dp, -3, (byte)data, temp + no_read_before_write);
                            //CBRANCH(temp)
                            {
                                _pc++;
                                if (temp != 0) { goto cbranch_taken_loop; }
                                rel_time -= 2;
                                goto inc_pc_loop;
                            }
                        }

                    case 0xFE: // DBNZ Y,rel
                        y = (byte)(y - 1);
                        //BRANCH(y)
                        BRANCH_TOP(data);
                        if (y != 0) goto loop;
                        BRANCH_BOTTOM(ref rel_time, data);
                        goto loop;

                    case 0x1F: // JMP [abs+X]
                    case 0x5F: // JMP abs
                        if (opcode == 0x1F)
                        {
                            SET_PC(READ_PC16(_pc) + x);
                        }
                    // fall through
                    //case 0x5F: // JMP abs
                        SET_PC(READ_PC16(_pc));
                        goto loop;

                    // 13. SUB-ROUTINE CALL RETURN COMMANDS

                    case 0x0F:
                        {// BRK
                            //int temp;
                            uint ret_addr = (uint)GET_PC();
                            SUSPICIOUS_OPCODE("BRK");
                            SET_PC(READ_PROG16(0xFFDE)); // vector address verified
                            PUSH16(ret_addr);
                            GET_PSW(ref temp);
                            _psw = (_psw | b10) & ~i04;
                            PUSH(temp);
                            goto loop;
                        }

                    case 0x4F:
                        {// PCALL offset
                            uint ret_addr = (uint)GET_PC() + 1;
                            SET_PC(0xFF00 | (int)data);
                            PUSH16(ret_addr);
                            goto loop;
                        }

                    case 0x01: // TCALL n
                    case 0x11:
                    case 0x21:
                    case 0x31:
                    case 0x41:
                    case 0x51:
                    case 0x61:
                    case 0x71:
                    case 0x81:
                    case 0x91:
                    case 0xA1:
                    case 0xB1:
                    case 0xC1:
                    case 0xD1:
                    case 0xE1:
                    case 0xF1:
                        {
                            uint ret_addr = (uint)GET_PC();
                            SET_PC(READ_PROG16(0xFFDE - ((int)opcode >> 3)));
                            PUSH16(ret_addr);
                            goto loop;
                        }

                    // 14. STACK OPERATION COMMANDS

                    case 0x7F: // RET1
                        temp = ram[_sp]; // *sp;
                        SET_PC(Util.GET_LE16(ram, _sp + 1));
                        _sp += 3;
                        goto set_psw;
                    case 0x8E: // POP PSW
                        POP(ref temp);
                    set_psw:
                        SET_PSW(temp);
                        goto loop;

                    case 0x0D:
                        { // PUSH PSW
                            //int temp;
                            GET_PSW(ref temp);
                            PUSH(temp);
                            goto loop;
                        }

                    case 0x2D: // PUSH A
                        PUSH(a);
                        goto loop;

                    case 0x4D: // PUSH X
                        PUSH(x);
                        goto loop;

                    case 0x6D: // PUSH Y
                        PUSH(y);
                        goto loop;

                    case 0xAE: // POP A
                        POP(ref a);
                        goto loop;

                    case 0xCE: // POP X
                        POP(ref x);
                        goto loop;

                    case 0xEE: // POP Y
                        POP(ref y);
                        goto loop;

                    // 15. BIT OPERATION COMMANDS

                    case 0x02: // SET1
                    case 0x22:
                    case 0x42:
                    case 0x62:
                    case 0x82:
                    case 0xA2:
                    case 0xC2:
                    case 0xE2:
                    case 0x12: // CLR1
                    case 0x32:
                    case 0x52:
                    case 0x72:
                    case 0x92:
                    case 0xB2:
                    case 0xD2:
                    case 0xF2:
                        {
                            int bit = 1 << ((int)opcode >> 5);
                            int mask = ~bit;
                            if ((opcode & 0x10) != 0)
                                bit = 0;
                            data += (uint)_dp;
                            WRITE(rel_time, 0, (int)data, (READ(rel_time, -1, (int)data) & mask) | bit);
                            goto inc_pc_loop;
                        }

                    case 0x0E: // TSET1 abs
                    case 0x4E: // TCLR1 abs
                        data = (uint)READ_PC16(_pc);
                        _pc += 2;
                        {
                            temp = READ(rel_time, -2, (int)data);
                            nz = (byte)(a - temp);
                            temp &= ~a;
                            if (opcode == 0x0E)
                            {
                                temp |= a;
                            }
                            WRITE(rel_time, 0, (int)data, temp);
                        }
                        goto loop;

                    case 0x4A: // AND1 C,mem.bit
                        _c &= (int)MEM_BIT(rel_time, 0);
                        _pc += 2;
                        goto loop;

                    case 0x6A: // AND1 C,/mem.bit
                        _c &= (int)~MEM_BIT(rel_time, 0);
                        _pc += 2;
                        goto loop;

                    case 0x0A: // OR1 C,mem.bit
                        _c |= (int)MEM_BIT(rel_time, -1);
                        _pc += 2;
                        goto loop;

                    case 0x2A: // OR1 C,/mem.bit
                        _c |= (int)~MEM_BIT(rel_time, -1);
                        _pc += 2;
                        goto loop;

                    case 0x8A: // EOR1 C,mem.bit
                        _c ^= (int)MEM_BIT(rel_time, -1);
                        _pc += 2;
                        goto loop;

                    case 0xEA: // NOT1 mem.bit
                        data = (uint)READ_PC16(_pc);
                        _pc += 2;
                        {
                            temp = READ(rel_time, -1, (int)data & 0x1FFF);
                            temp ^= 1 << ((int)data >> 13);
                            WRITE(rel_time, 0, (int)data & 0x1FFF, temp);
                        }
                        goto loop;

                    case 0xCA: // MOV1 mem.bit,C
                        data = (uint)READ_PC16(_pc);
                        _pc += 2;
                        {
                            temp = READ(rel_time, -2, (int)data & 0x1FFF);
                            int bit = (int)data >> 13;
                            temp = (temp & ~(1 << bit)) | ((_c >> 8 & 1) << bit);
                            WRITE(rel_time, 0, (int)data & 0x1FFF, temp + no_read_before_write);
                        }
                        goto loop;

                    case 0xAA: // MOV1 C,mem.bit
                        _c = (int)MEM_BIT(rel_time, 0);
                        _pc += 2;
                        goto loop;

                    // 16. PROGRAM PSW FLAG OPERATION COMMANDS

                    case 0x60: // CLRC
                        _c = 0;
                        goto loop;

                    case 0x80: // SETC
                        _c = ~0;
                        goto loop;

                    case 0xED: // NOTC
                        _c ^= 0x100;
                        goto loop;

                    case 0xE0: // CLRV
                        _psw &= ~(v40 | h08);
                        goto loop;

                    case 0x20: // CLRP
                        _dp = 0;
                        goto loop;

                    case 0x40: // SETP
                        _dp = 0x100;
                        goto loop;

                    case 0xA0: // EI
                        SUSPICIOUS_OPCODE("EI");
                        _psw |= i04;
                        goto loop;

                    case 0xC0: // DI
                        SUSPICIOUS_OPCODE("DI");
                        _psw &= ~i04;
                        goto loop;

                    // 17. OTHER COMMANDS

                    case 0x00: // NOP
                        goto loop;

                    case 0xFF:
                    case 0xEF: // SLEEP
                        if(opcode == 0xFF)
                        {// STOP
                         // handle PC wrap-around
                            addr = GET_PC() - 1;
                            if (addr >= 0x10000)
                            {
                                addr &= 0xFFFF;
                                SET_PC(addr);
                                //dprintf("SPC: PC wrapped around\n");
                                Debug.WriteLine("SPC: PC wrapped around");
                                goto loop;
                            }
                        }
                    // fall through
                    //case 0xEF: // SLEEP
                        SUSPICIOUS_OPCODE("STOP/SLEEP");
                            --_pc;
                            rel_time = 0;
                            m.cpu_error = "SPC emulation error";
                            goto stop;
                } // switch

                throw new Exception("Unhandled opcode");
            }

            // Main loop
        out_of_time:
            rel_time -= m.cycle_table[ram[_pc]]; //*pc]; // undo partial execution of opcode
        stop:

            // Uncache registers
            if ( GET_PC() >= 0x10000 )
            {
                //dprintf( "SPC: PC wrapped around\n" );
                Debug.WriteLine("SPC: PC wrapped around");
            }
            m.cpu_regs.pc = (ushort) GET_PC();
            m.cpu_regs.sp = (byte) GET_SP();
            m.cpu_regs.a  = (byte) a;
            m.cpu_regs.x  = (byte) x;
            m.cpu_regs.y  = (byte) y;
            {
                //int temp;
                GET_PSW(ref temp);
                m.cpu_regs.psw = (byte) temp;
            }

            /*
            #define SPC_CPU_RUN_FUNC_END \
                m.spc_time += rel_time;\
                m.dsp_time -= rel_time;\
                m.timers [0].next_time -= rel_time;\
                m.timers [1].next_time -= rel_time;\
                m.timers [2].next_time -= rel_time;\
                assert( m.spc_time <= end_time );\
                return &REGS [r_cpuio0];\
            }
            */
            m.spc_time += rel_time;
            m.dsp_time -= rel_time;
            m.timers[0].next_time -= rel_time;
            m.timers[1].next_time -= rel_time;
            m.timers[2].next_time -= rel_time;
            //assert(m.spc_time <= end_time);
            if(m.spc_time > end_time)
            {
                throw new Exception("assert(m.spc_time <= end_time);");
            }

            //return m.smp_regs[0, (int)registers.r_cpuio0];
            return (int)registers.r_cpuio0; // m.smp_regs[0, (int)registers.r_cpuio0]; // &REGS[r_cpuio0];
        }
    }
}
