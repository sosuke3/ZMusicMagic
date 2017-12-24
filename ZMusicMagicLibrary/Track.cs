using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class Track
    {
        public List<TrackChannel> Channels { get; set; } = new List<TrackChannel>(8);
    }
}
