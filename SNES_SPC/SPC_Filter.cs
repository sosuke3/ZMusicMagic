using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNES_SPC
{
    public class SPC_Filter
    {
        public SPC_Filter()
        {
            gain = gain_unit;
            bass = bass_norm;
            clear();
        }
        public void run(short[] io, int count)
        {
            //require((count & 1) == 0); // must be even
            if(count % 2 != 0)
            {
                throw new ArgumentException("buffer size must be even.", nameof(count));
            }

            int gain = this.gain;
            int bass = this.bass;
            //chan_t* c = &ch[2];
            int index = 2;
            int iooffset = 0;
            do
            {
                // cache in registers
                int sum = ch[--index].sum; // (--c)->sum;
                int pp1 = ch[index].pp1; // c->pp1;
                int p1 = ch[index].p1; // c->p1;

                for (int i = 0; i < count; i += 2)
                {
                    // Low-pass filter (two point FIR with coeffs 0.25, 0.75)
                    int f = io[iooffset + i] + p1;
                    p1 = io[iooffset + i] * 3;

                    // High-pass filter ("leaky integrator")
                    int delta = f - pp1;
                    pp1 = f;
                    int s = sum >> (gain_bits + 2);
                    sum += (delta * gain) - (sum >> bass);

                    // Clamp to 16 bits
                    if ((short)s != s)
                        s = (s >> 31) ^ 0x7FFF;

                    io[iooffset + i] = (short)s;
                }

                ch[index].p1 = p1;
                ch[index].pp1 = pp1;
                ch[index].sum = sum;
                ++iooffset;
            }
            while (index != 0);
        }

        public void clear()
        {
            ch[0].p1 = 0;
            ch[0].pp1 = 0;
            ch[0].sum = 0;
            ch[1].p1 = 0;
            ch[1].pp1 = 0;
            ch[1].sum = 0;
        }

        public const int gain_unit = 0x100;
        public void set_gain(int gain)
        {
            this.gain = gain;
        }

        public const int bass_none = 0;
        public const int bass_norm = 8;
        public const int bass_max = 31;
        public void set_bass(int bass)
        {
            this.bass = bass;
        }

        const int gain_bits = 8;
        int gain;
        int bass;

        struct chan_t { public int p1, pp1, sum; }

        chan_t[] ch = new chan_t[2] { new chan_t(), new chan_t() };
    }
}
