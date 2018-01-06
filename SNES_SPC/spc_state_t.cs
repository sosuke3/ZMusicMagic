namespace SNES_SPC
{
    public class spc_state_t
    {
        public Timer[] timers = new Timer[SNES_SPC.timer_count] { new Timer(), new Timer(), new Timer() };

        public byte[,] smp_regs = new byte[2, SNES_SPC.reg_count];

        public cpu_regs cpu_regs = new cpu_regs();

        public int dsp_time;
        public int spc_time;
        public bool echo_accessed;

        public int tempo;
        public int skipped_kon;
        public int skipped_koff;
        public string cpu_error; // byte[]?

        public int extra_clocks;
        public short[] buf_begin; // sample_t*   buf_begin; really just the buffer because we don't have pointers in c#
        public int buf_begin_pointer;
        // sample_t const* buf_end;
        public int buf_end;
        // sample_t*   extra_pos;
        public int extra_pos;
        public short[] extra_buf = new short[SNES_SPC.extra_size];

        public int rom_enabled;
        public byte[] rom = new byte[SNES_SPC.rom_size] { 0xCD, 0xEF, 0xBD, 0xE8, 0x00, 0xC6, 0x1D, 0xD0, 0xFC, 0x8F, 0xAA, 0xF4, 0x8F, 0xBB, 0xF5, 0x78, 0xCC, 0xF4, 0xD0, 0xFB, 0x2F, 0x19, 0xEB, 0xF4, 0xD0, 0xFC, 0x7E, 0xF4, 0xD0, 0x0B, 0xE4, 0xF5, 0xCB, 0xF4, 0xD7, 0x00, 0xFC, 0xD0, 0xF3, 0xAB, 0x01, 0x10, 0xEF, 0x7E, 0xF4, 0x10, 0xEB, 0xBA, 0xF6, 0xDA, 0x00, 0xBA, 0xF4, 0xC4, 0xF4, 0xDD, 0x5D, 0xD0, 0xDB, 0x1F, 0x00, 0x00, 0xC0, 0xFF };
        public byte[] hi_ram = new byte[SNES_SPC.rom_size];

        public byte[] cycle_table = new byte[256];

        public Ram ram = new Ram();
    }
}