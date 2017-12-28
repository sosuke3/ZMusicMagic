using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class Channel
    {
        internal byte[] RawData { get; set; }

        public List<ChannelCommand> Commands { get; set; }

        public void LoadFromNSPCTrack(NSPC.Track track)
        {
            this.RawData = track.RawTrackData;
            this.Commands = ChannelCommand.GetChannelCommandsFromRawData(track.RawTrackData);
        }

    }
}
