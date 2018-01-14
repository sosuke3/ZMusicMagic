﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public class Song
    {
        public List<Part> Parts { get; set; } = new List<Part>();
        public string DisplayName { get; set; }
        public int SongIndex { get; set; }

        public void FixDurations()
        {
            foreach(var p in Parts)
            {
                p.FixDurations();
            }
        }
    }
}
