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
        public byte[] rom = new byte[SNES_SPC.rom_size];
        public byte[] hi_ram = new byte[SNES_SPC.rom_size];

        public byte[] cycle_table = new byte[256];

        public Ram ram = new Ram();
    }
}