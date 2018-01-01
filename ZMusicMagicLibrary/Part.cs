using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class Part
    {
        public List<Channel> Channels { get; set; } = new List<Channel>(8);

        public void FixDurations()
        {
            foreach(var c in Channels)
            {
                c.FixDurations();
            }
        }
    }
}
