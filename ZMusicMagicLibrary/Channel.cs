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
            LoadFromRawTrackData(track.RawTrackData);
        }

        void LoadFromRawTrackData(byte[] rawData)
        {
            this.RawData = rawData;
            this.Commands = new List<ChannelCommand>();

            var index = 0;
            while(index < rawData.Length)
            {
                if (rawData[index] == (byte)NSPC.Track.Command._00_Return)
                {
                    // stop/return
                    this.Commands.Add(new ReturnCommand() { Command = rawData[index] });
                    break;
                }
                else if(rawData[index] >= (byte)NSPC.Track.Command._80_C1 && rawData[index] <= (byte)NSPC.Track.Command._C9_Rest)
                {
                    // note
                    this.Commands.Add(new NoteCommand() { Command = rawData[index] });
                }
                else if (rawData[index] >= (byte)NSPC.Track.Command._CA_Percussion0 && rawData[index] <= (byte)NSPC.Track.Command._DF_Percussion21)
                {
                    // percussion
                    this.Commands.Add(new ChannelCommand() { Command = rawData[index] });

                    // in theory we should never hit this
                    Debugger.Break();
                }
                else if (rawData[index] >= (byte)NSPC.Track.Command._E0_SetInstrument)
                {
                    // command
                    ChannelCommand command = new SettingCommand();
                    command.Command = rawData[index];
                    for(int i = 0; i < NSPC.Track.CommandParameterCount[(NSPC.Track.Command)rawData[index]]; i++)
                    {
                        command.Parameters.Add(rawData[index + i + 1]);
                    }
                    index += NSPC.Track.CommandParameterCount[(NSPC.Track.Command)rawData[index]];
                    this.Commands.Add(command);
                }
                else
                {
                    // note "parameter" - duration
                    this.Commands.Add(new DurationCommand() { Command = rawData[index] });

                    if (index + 1 < rawData.Length)
                    {
                        if(rawData[index + 1] >= 0x01 && rawData[index + 1] < 0x80)
                        {
                            // second "parameter" - staccato/velocity
                            index++;
                            this.Commands.Add(new VelocityCommand() { Command = rawData[index] });
                        }
                    }
                }

                index++;
            }
        }
    }
}
