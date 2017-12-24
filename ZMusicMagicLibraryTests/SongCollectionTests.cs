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

        [Fact]
        public void loads_song_banks()
        {
            Rom rom = new Rom();
            rom.LoadRom("zelda.sfc");

        }
    }
}
