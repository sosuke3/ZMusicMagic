using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZMusicMagic
{
    public enum SongState { Stopped, Paused, Playing, Shutdown };
    public class SongPlayer
    {
        volatile SongState state;
        public SongState State { get { return state; } }

        ConcurrentQueue<SongPlayerMessage> messageQueue = new ConcurrentQueue<SongPlayerMessage>();

        Thread thread;

        BufferedWaveProvider bufferedWaveProvider;
        IWavePlayer waveOut;
        VolumeWaveProvider16 volumeProvider;

        SNES_SPC.SNES_SPC spc;
        SNES_SPC.SPC_Filter filter;

        const int samplerate = 32000;
        const int bits = 16;
        const int channels = 2;

        const int bufferProviderLength = 20; // seconds

        const int buffersize = 2048;

        public SongPlayer()
        {
            SetupNSPC();
            SetupNAudio();
        }

        void SetupNSPC()
        {
            spc = new SNES_SPC.SNES_SPC();
            filter = new SNES_SPC.SPC_Filter();

            spc.init();

            var spcfile = File.ReadAllBytes("loz 1.spc"); // todo: fix this so we can build one from rom we loaded
            spc.load_spc(spcfile, spcfile.Length);

            spc.clear_echo();
            filter.clear();

        }

        void SetupNAudio()
        {
            WaveFormat waveFormat = new WaveFormat(samplerate, bits, channels);

            bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
            bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(bufferProviderLength);

            waveOut = new WaveOut();
            waveOut.PlaybackStopped += OnPlaybackStopped;
            volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
            volumeProvider.Volume = 1.0f;
            waveOut.Init(volumeProvider);

        }

        public void Play(int index)
        {
            if (state != SongState.Paused)
            {
                state = SongState.Stopped;

                if (thread == null)
                {
                    thread = new Thread(new ParameterizedThreadStart(threadLoop));
                    thread.IsBackground = true;
                    thread.Start(index);
                }
            }
            messageQueue.Enqueue(new SongPlayerPlayMessage(index));
            waveOut.Play();
        }

        public void Pause()
        {
            waveOut.Pause();
            messageQueue.Enqueue(new SongPlayerPauseMessage());
        }

        public void Stop()
        {
            waveOut.Stop();
            messageQueue.Enqueue(new SongPlayerStopMessage());
        }

        bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                    bufferedWaveProvider.BufferedDuration > TimeSpan.FromSeconds(bufferProviderLength - 1);
                       //bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       //< bufferProviderLength; // bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }

        void threadLoop(object obj)
        {
            try
            {
                int index = (int)obj;
                state = SongState.Playing;

                spc.write_port(0, 0, 0xF0);
                spc.write_port(0, 0, (byte)index);

                var buffer = new short[buffersize];
                var byteBuffer = new byte[buffersize * 2];

                while (true)
                {
                    if(state == SongState.Shutdown)
                    {
                        break;
                    }
                    while(messageQueue.IsEmpty == false)
                    {
                        SongPlayerMessage message; 
                        if(messageQueue.TryDequeue(out message))
                        {
                            switch(message) // todo: switch to vs2017 so these don't show as errors in intellisense...
                            {
                                case SongPlayerPauseMessage pause:
                                    state = SongState.Paused;
                                    break;
                                case SongPlayerPlayMessage play:
                                    // start a new
                                    if (state != SongState.Paused)
                                    {
                                        spc.write_port(0, 0, 0xF0); // stop the previous song
                                        spc.write_port(0, 1, 0x00);
                                        spc.play(buffer.Length, buffer);
                                        spc.write_port(0, 0, (byte)play.SongIndex);
                                        spc.write_port(0, 1, 0x00);
                                        bufferedWaveProvider.ClearBuffer();
                                    }
                                    state = SongState.Playing;
                                    break;
                                case SongPlayerStopMessage stop:
                                    state = SongState.Stopped;
                                    bufferedWaveProvider.ClearBuffer();
                                    spc.write_port(0, 0, 0xF0); // stop the song
                                    spc.write_port(0, 1, 0x00);
                                    spc.play(buffer.Length, buffer);
                                    break;
                            }
                        }
                    }

                    if (state != SongState.Playing)
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    if (IsBufferNearlyFull)
                    {
                        Debug.WriteLine("Buffer getting full, taking a break");
                        Thread.Sleep(500);
                        continue;
                    }

                    var error = spc.play(buffersize, buffer);
                    if (error != String.Empty)
                    {
                        Debug.WriteLine(error);
                    }

                    filter.run(buffer, buffersize);

                    Buffer.BlockCopy(buffer, 0, byteBuffer, 0, byteBuffer.Length);
                    bufferedWaveProvider.AddSamples(byteBuffer, 0, byteBuffer.Length);

                    //Thread.Sleep(500);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("Playback Stopped");
            if (e.Exception != null)
            {
                Debug.WriteLine(String.Format("Playback Error {0}", e.Exception.Message));
            }
        }

        public void Shutdown()
        {
            state = SongState.Shutdown;
        }
    }

    internal abstract class SongPlayerMessage
    {

    }

    internal class SongPlayerStopMessage : SongPlayerMessage
    {

    }

    internal class SongPlayerPlayMessage : SongPlayerMessage
    {
        public int SongIndex { get; set; }

        public SongPlayerPlayMessage(int index)
        {
            SongIndex = index;
        }
    }

    internal class SongPlayerPauseMessage : SongPlayerMessage
    {

    }
}
