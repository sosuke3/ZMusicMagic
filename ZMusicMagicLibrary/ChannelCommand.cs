using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class ChannelCommand
    {
        public byte Command { get; set; }
        public List<byte> Parameters { get; set; } = new List<byte>();
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

    public class ReturnCommand : ChannelCommand
    {

    }
}
