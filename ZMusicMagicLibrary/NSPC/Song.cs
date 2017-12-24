using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary.NSPC
{
    public class Song
    {
        public int Address { get; set; }
        public int MaxLength { get; set; } = -1;
        public int LoopAddress { get; set; } = -1;

        public bool IsEmpty { get { return Address == 0x0 || MaxLength == 0x0 || MaxLength == -1; } }

        List<SongPart> Parts { get; set; } = new List<SongPart>();

        public void LoadParts(byte[] aramBuffer)
        {
            if (IsEmpty)
            {
                // can't load something that doesn't exist
                return;
            }

            LoadPartAddresses(aramBuffer);
            LoadPartData(aramBuffer);
        }

        private void LoadPartData(byte[] aramBuffer)
        {
            foreach(var p in Parts)
            {
                p.LoadTracks(aramBuffer);
            }
        }

        void LoadPartAddresses(byte[] aramBuffer)
        {
            int lowestAddress = 0xFFFF;
            int index = this.Address; // starting address for parts pointer table for this song

            while (index < lowestAddress && index < 0xFFFF)
            {
                int address = aramBuffer[index] | (aramBuffer[index + 1] << 8);
                int loopCount = 0;

                if (address == 0x0000)
                {
                    // stop
                    Parts.Add(new SongPart() { PartPointerAddress = index, SongPartTrackTableAddress = 0x0000 });
                    break;
                }
                else if (address == 0x00FF)
                {
                    // next address is loop address
                    this.LoopAddress = aramBuffer[index + 2] | (aramBuffer[index + 3] << 8);
                    Parts.Add(new SongPart() { PartPointerAddress = index, SongPartTrackTableAddress = address, GotoPartAddress = LoopAddress });
                    index += 4;
                    continue;
                }
                else if (address < 0x00FF)
                {
                    // loop count
                    loopCount = address;
                    index += 2;
                    address = aramBuffer[index] | (aramBuffer[index + 1] << 8);
                }

                if (address > this.Address && address < lowestAddress)
                {
                    lowestAddress = address;
                }

                // todo: add real class with loop count
                Parts.Add(new SongPart()
                {
                    PartPointerAddress = index,
                    SongPartTrackTableAddress = address,
                    LoopCount = loopCount
                });

                index += 2;
            }
        }
    }
}
