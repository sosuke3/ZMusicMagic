using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNES_SPC;
using NAudio;
using NAudio.Wave;

namespace SPCPlay
{
    class Program
    {
        static void Main(string[] args)
        {
            var spc = new SNES_SPC.SNES_SPC();
            spc.init();

            var spcfile = File.ReadAllBytes("loz.spc");
            spc.load_spc(spcfile, spcfile.Length);

            spc.clear_echo();

            WaveFormat waveFormat = new WaveFormat(32000, 16, 2);
            using (WaveFileWriter writer = new WaveFileWriter("test.wav", waveFormat))
            {
                int sample_count = 0;
                while (sample_count < 20 * 32000 * 2)
                {
                    sample_count += 2048;

                    int count = 2048;
                    short[] output = new short[count];
                    spc.play(count, output);

                    writer.WriteSamples(output, 0, output.Length);
                }

                writer.Flush();
            }
        }
    }
}
