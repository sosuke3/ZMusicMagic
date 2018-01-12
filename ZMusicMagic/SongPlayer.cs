using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZMusicMagic
{
    public enum SongState { Stopped, Paused, Playing };
    public class SongPlayer
    {
        public SongState State { get; set; }

        Thread thread;

        SNES_SPC.SNES_SPC spc = new SNES_SPC.SNES_SPC();

        const int samplerate = 32000;
        const int bits = 16;
        const int channels = 2;

        WaveFormat waveFormat = new WaveFormat(samplerate, bits, channels);

        const int buffersize = 2048;

        public void Play(int index)
        {
            if(thread != null)
            {
                thread.Abort();
            }
            State = SongState.Playing;

            thread = new Thread(new ParameterizedThreadStart(threadLoop));
            thread.Start(index);
        }

        private void threadLoop(object obj)
        {
            spc.write_port(0, 0, (byte)obj);

            while(true)
            {
                Thread.Sleep(500);
            }
        }
    }
}
