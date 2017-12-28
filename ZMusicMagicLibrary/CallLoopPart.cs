using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class CallLoopPart
    {
        public int Address { get; set; }
        public List<ChannelCommand> Commands { get; set; } = new List<ChannelCommand>();
    }
}
