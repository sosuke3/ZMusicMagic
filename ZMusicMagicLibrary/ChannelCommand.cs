using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class ChannelCommand
    {
        public byte Command { get; set; }
        public List<byte> Parameters { get; set; } = new List<byte>();

        public static List<ChannelCommand> GetChannelCommandsFromRawData(byte[] rawData, int address = 0)
        {
            var commands = new List<ChannelCommand>();

            var index = address;
            while (index < rawData.Length)
            {
                if (rawData[index] == (byte)NSPC.Track.Command._00_Return)
                {
                    // stop/return
                    commands.Add(new ReturnCommand() { Command = rawData[index] });
                    break;
                }
                else if (rawData[index] >= (byte)NSPC.Track.Command._80_C1 && rawData[index] <= (byte)NSPC.Track.Command._C9_Rest)
                {
                    // note
                    commands.Add(new NoteCommand() { Command = rawData[index] });
                }
                else if (rawData[index] >= (byte)NSPC.Track.Command._CA_Percussion0 && rawData[index] <= (byte)NSPC.Track.Command._DF_Percussion21)
                {
                    // percussion
                    commands.Add(new ChannelCommand() { Command = rawData[index] });

                    // in theory we should never hit this
                    Debugger.Break();
                }
                else if(rawData[index] == (byte)NSPC.Track.Command._EF_CallLoop)
                {
                    // command
                    ChannelCommand command = new CallLoopCommand();
                    command.Command = rawData[index];
                    for (int i = 0; i < NSPC.Track.CommandParameterCount[(NSPC.Track.Command)rawData[index]]; i++)
                    {
                        command.Parameters.Add(rawData[index + i + 1]);
                    }
                    index += NSPC.Track.CommandParameterCount[(NSPC.Track.Command)rawData[index]];
                    commands.Add(command);
                }
                else if (rawData[index] >= (byte)NSPC.Track.Command._E0_SetInstrument)
                {
                    // command
                    ChannelCommand command = new SettingCommand();
                    command.Command = rawData[index];
                    for (int i = 0; i < NSPC.Track.CommandParameterCount[(NSPC.Track.Command)rawData[index]]; i++)
                    {
                        command.Parameters.Add(rawData[index + i + 1]);
                    }
                    index += NSPC.Track.CommandParameterCount[(NSPC.Track.Command)rawData[index]];
                    commands.Add(command);
                }
                else
                {
                    // note "parameter" - duration
                    commands.Add(new DurationCommand() { Command = rawData[index] });

                    if (index + 1 < rawData.Length)
                    {
                        if (rawData[index + 1] >= 0x01 && rawData[index + 1] < 0x80)
                        {
                            // second "parameter" - staccato/velocity
                            index++;
                            commands.Add(new VelocityCommand() { Command = rawData[index] });
                        }
                    }
                }

                index++;
            }

            return commands;
        }
    }

    public class DurationCommand : ChannelCommand
    {

    }

    public class VelocityCommand : ChannelCommand
    {

    }

    public class NoteCommand : ChannelCommand
    {

    }

    public class SettingCommand : ChannelCommand
    {

    }

    public class CallLoopCommand : SettingCommand
    {
        public CallLoopPart LoopPart { get; set; }

        public int LoopAddress
        {
            get
            {
                if(this.Parameters.Count < 3)
                {
                    throw new Exception("CallLoopCommand found with insufficient parameters");
                }

                int address = Parameters[0] | (Parameters[1] << 8);

                return address;
            }
        }

        public int LoopCount
        {
            get
            {
                if (this.Parameters.Count < 3)
                {
                    throw new Exception("CallLoopCommand found with insufficient parameters");
                }

                return Parameters[2];
            }
        }
    }

    public class ReturnCommand : ChannelCommand
    {

    }
}
