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

        public void FixDurations()
        {
            int currentTime = 0;
            int currentDuration = 0;

            foreach(var c in Commands)
            {
                c.StartTime = currentTime;
                if (c is DurationCommand)
                {
                    currentDuration = c.Command;
                }
                if(c is NoteCommand)
                {
                    c.Duration = currentDuration;

                    currentTime += currentDuration;
                }
                if(c is CallLoopCommand)
                {
                    var loop = c as CallLoopCommand;
                    var loopDuration = 0;
                    for(int i = 0; i < loop.LoopCount; ++i)
                    {
                        loopDuration = loop.CalculateDuration(currentDuration);
                    }

                    loop.Duration = loopDuration;
                    currentTime += loopDuration * loop.LoopCount;
                }
            }
        }
    }
}
