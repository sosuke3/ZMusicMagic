namespace SNES_SPC
{
    internal class dsp_state_t
    {
        public byte[] regs = new byte[SPC_DSP.register_count];

        // Echo history keeps most recent 8 samples (twice the size to simplify wrap handling)
        public int[,] echo_hist = new int[SPC_DSP.echo_hist_size * 2, 2];
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

        public voice_t[] voices = new voice_t[SPC_DSP.voice_count] { new voice_t(), new voice_t(), new voice_t(), new voice_t(), new voice_t(), new voice_t(), new voice_t(), new voice_t() };

        // non-emulation state
        public byte[] ram; // 64K shared RAM between DSP and SMP
        public int mute_mask;
        public short[] output;
        public int out_pointer;
        public int out_end; //sample_t* out_end;
        //public int out_begin;//sample_t* out_begin;
        public short[] extra = new short[SPC_DSP.extra_size];
    }
}