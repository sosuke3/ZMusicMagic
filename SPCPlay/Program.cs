using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNES_SPC;
using NAudio;
using NAudio.Wave;
using System.Diagnostics;

namespace SPCPlay
{
    class Program
    {
        static void Main(string[] args)
        {
            var spc = new SNES_SPC.SNES_SPC();
            spc.init();

            var spcfile = File.ReadAllBytes("loz 1.spc");
            spc.load_spc(spcfile, spcfile.Length);

            spc.clear_echo();

            const int bitrate = 32000;
            const int bits = 16;
            const int channels = 2;

            WaveFormat waveFormat = new WaveFormat(bitrate, bits, channels);
            using (WaveFileWriter writer = new WaveFileWriter("test.wav", waveFormat))
            {
                const int seconds = 2; // 20;
                const int buffersize = 2048;

                int sample_count = 0;
                while (sample_count < seconds * bitrate * channels)
                {
                    sample_count += buffersize;

                    short[] output = new short[buffersize];

                    var error = spc.play(buffersize, output);
                    if(error != String.Empty)
                    {
                        Debug.WriteLine(error);
                    }

                    writer.WriteSamples(output, 0, output.Length);
                }

                writer.Flush();
            }
        }
    }
}
