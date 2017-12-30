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
        public NSPC.Track.Command CommandType { get { return (NSPC.Track.Command)Command; } }
        public int StartTime { get; set; }
        public int Duration { get; set; }
        public List<byte> Parameters { get; set; } = new List<byte>();

        public static bool NoteIsSharp(NSPC.Track.Command command)
        {
            switch(command)
            {
                case NSPC.Track.Command._81_Db1:
                case NSPC.Track.Command._83_Eb1:
                case NSPC.Track.Command._86_Gb1:
                case NSPC.Track.Command._88_Ab1:
                case NSPC.Track.Command._8A_Bb1:
                case NSPC.Track.Command._8D_Db2:
                case NSPC.Track.Command._8F_Eb2:
                case NSPC.Track.Command._92_Gb2:
                case NSPC.Track.Command._94_Ab2:
                case NSPC.Track.Command._96_Bb2:
                case NSPC.Track.Command._99_Db3:
                case NSPC.Track.Command._9B_Eb3:
                case NSPC.Track.Command._9E_Gb3:
                case NSPC.Track.Command._A0_Ab3:
                case NSPC.Track.Command._A2_Bb3:
                case NSPC.Track.Command._A5_Db4:
                case NSPC.Track.Command._A7_Eb4:
                case NSPC.Track.Command._AA_Gb4:
                case NSPC.Track.Command._AC_Ab4:
                case NSPC.Track.Command._AE_Bb4:
                case NSPC.Track.Command._B1_Db5:
                case NSPC.Track.Command._B3_Eb5:
                case NSPC.Track.Command._B6_Gb5:
                case NSPC.Track.Command._B8_Ab5:
                case NSPC.Track.Command._BA_Bb5:
                case NSPC.Track.Command._BD_Db6:
                case NSPC.Track.Command._BF_Eb6:
                case NSPC.Track.Command._C2_Gb6:
                case NSPC.Track.Command._C4_Ab6:
                case NSPC.Track.Command._C6_Bb6:
                    return true;
            }
            return false;
        }
        public static List<ChannelCommand> GetChannelCommandsFromRawData(byte[] rawData, int address = 0)
        {
            var commands = new List<ChannelCommand>();

            var index = address;
            var time = 0;
            var currentDuration = 0;
            while (index < rawData.Length)
            {
                if (rawData[index] == (byte)NSPC.Track.Command._00_Return)
                {
                    // stop/return
                    commands.Add(new ReturnCommand() { Command = rawData[index], StartTime = time });
                    break;
                }
                else if (rawData[index] >= (byte)NSPC.Track.Command._80_C1 && rawData[index] <= (byte)NSPC.Track.Command._C9_Rest)
                {
                    // note
                    commands.Add(new NoteCommand() { Command = rawData[index], StartTime = time, Duration = currentDuration });
                    time += currentDuration;
                }
                else if (rawData[index] >= (byte)NSPC.Track.Command._CA_Percussion0 && rawData[index] <= (byte)NSPC.Track.Command._DF_Percussion21)
                {
                    // percussion
                    commands.Add(new ChannelCommand() { Command = rawData[index], StartTime = time, Duration = currentDuration });
                    time += currentDuration; //?

                    // in theory we should never hit this
                    Debugger.Break();
                }
                else if(rawData[index] == (byte)NSPC.Track.Command._EF_CallLoop)
                {
                    // command
                    ChannelCommand command = new CallLoopCommand();
                    command.Command = rawData[index];
                    command.StartTime = time;
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
                    command.StartTime = time;
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
                    commands.Add(new DurationCommand() { Command = rawData[index], StartTime = time });
                    currentDuration = rawData[index];

                    if (index + 1 < rawData.Length)
                    {
                        if (rawData[index + 1] >= 0x01 && rawData[index + 1] < 0x80)
                        {
                            // second "parameter" - staccato/velocity
                            index++;
                            commands.Add(new VelocityCommand() { Command = rawData[index], StartTime = time });
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
