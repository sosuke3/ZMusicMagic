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
            //byte[] testbuff = new byte[] { 0xFF, 0xFF, 0x00, 0xFF };
            //var s1 = Util.GET_LE16SA(testbuff, 0);
            //var s2 = Util.GET_LE16SA(testbuff, 1);
            //var s3 = Util.GET_LE16SA(testbuff, 2);
            //var us1 = Util.GET_LE16A(testbuff, 0);
            //var us2 = Util.GET_LE16A(testbuff, 1);
            //var us3 = Util.GET_LE16A(testbuff, 2);

            var filter = new SNES_SPC.SPC_Filter();
            var spc = new SNES_SPC.SNES_SPC();
            spc.init();

            var spcfile = File.ReadAllBytes("loz 1.spc");
            spc.load_spc(spcfile, spcfile.Length);

            spc.clear_echo();
            filter.clear();

            const int samplerate = 32000;
            const int bits = 16;
            const int channels = 2;

            WaveFormat waveFormat = new WaveFormat(samplerate, bits, channels);
            using (WaveFileWriter writer = new WaveFileWriter("test.wav", waveFormat))
            {
                const int seconds = 20; // 20;
                const int buffersize = 2048;

                int sample_count = 0;
                while (sample_count < seconds * samplerate * channels)
                {
                    sample_count += buffersize;

                    short[] output = new short[buffersize];

                    var error = spc.play(buffersize, output);
                    if(error != String.Empty)
                    {
                        Debug.WriteLine(error);
                    }

                    filter.run(output, buffersize);

                    writer.WriteSamples(output, 0, output.Length);
                }

                //spc.write_port(0, 0, 0xF1);

                //sample_count = 0;
                //while (sample_count < 1 * samplerate * channels)
                //{
                //    sample_count += buffersize;

                //    short[] output = new short[buffersize];

                //    var error = spc.play(buffersize, output);
                //    if (error != String.Empty)
                //    {
                //        Debug.WriteLine(error);
                //    }

                //    filter.run(output, buffersize);

                //    writer.WriteSamples(output, 0, output.Length);
                //}


                spc.write_port(0, 0, 0x02);
                //spc.write_port(0, 1, 0x0);
                //spc.write_port(0, 2, 0x0);
                //spc.write_port(0, 3, 0x0);


                sample_count = 0;
                while (sample_count < seconds * samplerate * channels)
                {
                    sample_count += buffersize;

                    short[] output = new short[buffersize];

                    var error = spc.play(buffersize, output);
                    if (error != String.Empty)
                    {
                        Debug.WriteLine(error);
                    }

                    filter.run(output, buffersize);

                    writer.WriteSamples(output, 0, output.Length);
                }

                writer.Flush();
            }
        }
    }
}
