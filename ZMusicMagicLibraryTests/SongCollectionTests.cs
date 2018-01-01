using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using ZMusicMagicLibrary;

namespace ZMusicMagicLibraryTests
{
    public class SongCollectionTests
    {
        readonly ITestOutputHelper output;

        public SongCollectionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        //[Fact]
        //public void enum_test()
        //{
        //    ZMusicMagicLibrary.NSPC.Track.Command command;

        //    for(int i = 0; i < 0xFF; i++)
        //    {
        //        command = (ZMusicMagicLibrary.NSPC.Track.Command)i;

        //        output.WriteLine($"{command.GetDescription()}");
        //    }
        //}

        [Fact]
        public void loads_song_banks()
        {
            VerifyRomLoad("zelda.sfc");

            //VerifyRomLoad("zeldaUS.sfc");
        }

        void VerifyRomLoad(string filename)
        {
            Rom rom = new Rom();
            rom.LoadRom(filename);

            //output.WriteLine($"BaseNSPC");
            //foreach (var s in rom.BaseNSPC.Songs)
            //{
            //    output.WriteLine($"Song - Address:{s.Address.ToString("X")}, Loop Address:{s.LoopAddress.ToString("X")}, Part Count: {s.Parts.Count}");
            //    foreach (var p in s.Parts)
            //    {
            //        output.WriteLine($"Part - Address:{p.PartPointerAddress.ToString("X")}, Track Table Address:{p.SongPartTrackTableAddress.ToString("X")}, Loop Count:{p.LoopCount}, Goto Address:{p.GotoPartAddress.ToString("X")}, Track Count:{p.Tracks.Count(x => x.Address != 0x0)}");
            //        foreach (var t in p.Tracks)
            //        {
            //            output.WriteLine($"Track - Address:{t.Address.ToString("X")}, Raw Data Length:{t.RawTrackData.Length}");
            //        }
            //    }
            //}

            //output.WriteLine($"IndoorNSPC");
            //foreach (var s in rom.IndoorNSPC.Songs)
            //{
            //    output.WriteLine($"Song - Address:{s.Address.ToString("X")}, Loop Address:{s.LoopAddress.ToString("X")}, Part Count: {s.Parts.Count}");
            //    foreach (var p in s.Parts)
            //    {
            //        output.WriteLine($"Part - Address:{p.PartPointerAddress.ToString("X")}, Track Table Address:{p.SongPartTrackTableAddress.ToString("X")}, Loop Count:{p.LoopCount}, Goto Address:{p.GotoPartAddress.ToString("X")}, Track Count:{p.Tracks.Count(x => x.Address != 0x0)}");
            //        foreach (var t in p.Tracks)
            //        {
            //            output.WriteLine($"Track - Address:{t.Address.ToString("X")}, Raw Data Length:{t.RawTrackData.Length}");
            //        }
            //    }
            //}

            SongCollection songCollection = new SongCollection();
            songCollection.LoadFromNspc(rom.BaseNSPC);

            SongCollection outdoorCollection = new SongCollection();
            outdoorCollection.LoadFromNspc(rom.OverworldNSPC);

            SongCollection indoorCollection = new SongCollection();
            indoorCollection.LoadFromNspc(rom.IndoorNSPC);

            SongCollection endingCollection = new SongCollection();
            endingCollection.LoadFromNspc(rom.EndingNSPC);

            songCollection.FixDurations();
            outdoorCollection.FixDurations();
            indoorCollection.FixDurations();
            endingCollection.FixDurations();

            int songNumber = 0;
            foreach(var s in songCollection.Songs)
            {
                output.WriteLine($"song {songNumber++}");
                int partNumber = 0;
                foreach(var p in s.Parts)
                {
                    output.WriteLine($"part {partNumber++}");
                    int channelNumber = 0;
                    foreach (var c in p.Channels)
                    {
                        output.WriteLine($"channel {channelNumber++}");
                        foreach (var cmd in c.Commands)
                        {
                            output.WriteLine($"cmd: {cmd.CommandType.GetDescription()} - startTime: {cmd.StartTime} - duration: {cmd.Duration} - parameters: {String.Join(",", cmd.Parameters)}");
                        }
                    }
                }
            }

            // because I'm curious
            Assert.Equal(rom.BaseNSPC.Songs.Count, outdoorCollection.Songs.Count);

            Assert.Equal(rom.BaseNSPC.Songs.Count, songCollection.Songs.Count);
            Assert.Equal(rom.OverworldNSPC.Songs.Count, outdoorCollection.Songs.Count);
            Assert.Equal(rom.IndoorNSPC.Songs.Count, indoorCollection.Songs.Count);
            Assert.Equal(rom.EndingNSPC.Songs.Count, endingCollection.Songs.Count);
        }
    }
}
