using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZMusicMagicLibrary.NSPC
{
    public class NSPC
    {
        const int NSPCBaseAddress = 0xC8000; // 0xCFBCA; // cfbca is the start of the 'driver' // this is only for vanilla

        Rom romData;
        public List<Chunk> Chunks { get; set; } = new List<Chunk>();
        int baseAddress;
        internal byte[] aramBuffer;

        public List<Song> Songs { get; set; } = new List<Song>();

        public void LoadRom(Rom romData, int startingAddress = NSPCBaseAddress)
        {
            this.romData = romData;

            LoadChunks(startingAddress);
            ProcessChunks();
        }

        void LoadChunks(int startingAddress)
        {
            aramBuffer = new byte[0xFFFF]; // fake NSPC audio ram

            int lastChunkLength = 0xffff;
            int nextAddress = startingAddress;
            this.baseAddress = startingAddress;

            while (lastChunkLength > 0)
            {
                var chunk = new Chunk(romData, nextAddress);
                nextAddress = chunk.ChunkOffsetAddress + chunk.ChunkLength;
                lastChunkLength = chunk.ChunkLength;

                Array.Copy(chunk.ChunkData, 0, aramBuffer, chunk.ChunkARAMAddress, chunk.ChunkLength);

                Chunks.Add(chunk);
            }
        }

        void ProcessChunks()
        {
            LoadSongs();
        }

        void LoadSongs()
        {
            this.Songs = LoadSongAddresses();

            FindMaxLengths(this.Songs);

            LoadSongParts(this.Songs);
        }

        List<Song> LoadSongAddresses()
        {
            var songs = new List<Song>();

            int lowestAddress = 0xFFFF;
            int index = 0xD000; // starting address for music pointer table

            while (index < lowestAddress && index < 0xFFFF)
            {
                int address = aramBuffer[index] | (aramBuffer[index + 1] << 8);

                // ignore anything in the lower "bank"
                if (address > 0xD000 && address < lowestAddress)
                {
                    lowestAddress = address;
                }

                songs.Add(new Song() { Address = address });

                index += 2;
            }

            return songs;
        }

        // probably don't even need this???
        void FindMaxLengths(List<Song> songAddresses)
        {
            var addressLengths = songAddresses.Select(x => x.Address).Distinct().OrderBy(x => x).ToDictionary(x => x, x => -1);

            int lastAddress = addressLengths.FirstOrDefault().Key;

            foreach (var address in addressLengths.Keys.ToList())
            {
                if (lastAddress == address)
                {
                    continue;
                }

                addressLengths[lastAddress] = CalculateSongMaxAddress(lastAddress, address);

                lastAddress = address;
            }

            addressLengths[lastAddress] = CalculateSongMaxAddress(lastAddress, 0xFFFF);

            foreach(var song in songAddresses)
            {
                song.MaxLength = addressLengths[song.Address];
            }
        }

        private static int CalculateSongMaxAddress(int lastAddress, int address, int lowerBankUpperBound = 0x3C00)
        {
            int length;
            if(lastAddress == 0)
            {
                return 0;
            }

            if (lastAddress < 0xD000 && address > 0xD000)
            {
                length = lowerBankUpperBound - lastAddress;
            }
            else
            {
                length = address - lastAddress;
            }

            return length;
        }

        void LoadSongParts(List<Song> songs)
        {
            foreach (var s in songs)
            {
                s.LoadAddresses(aramBuffer);
            }

            foreach (var s in songs)
            {
                s.LoadPartData(aramBuffer, songs);
            }
        }

        #region ExtracAllBRR
        public void ExtractAllBRR(string path)
        {
            var instrumentTable = Chunks.FirstOrDefault(x => x.ChunkARAMAddress == 0x3D00);
            var sampleTable = Chunks.FirstOrDefault(x => x.ChunkARAMAddress == 0x3C00);
            var sampleData = Chunks.FirstOrDefault(x => x.ChunkARAMAddress == 0x4000);

            if (instrumentTable == null || sampleTable == null || sampleData == null)
            {
                throw new Exception("Didn't load a chunk needed. Probably loaded the wrong base address.");
            }

            List<Sample> samples = new List<Sample>();
            for (int i = 0; i < sampleTable.ChunkLength; i += 4)
            {
                var aramAddress = sampleTable.ChunkData[i] + (sampleTable.ChunkData[i + 1] << 8);
                var loopAddress = sampleTable.ChunkData[i + 2] + (sampleTable.ChunkData[i + 3] << 8);
                var offset = aramAddress - 0x4000;
                var loopOffset = loopAddress - 0x4000;

                samples.Add(new Sample(sampleData.ChunkData, i / 4, offset, loopOffset, aramAddress, loopAddress));
            }

            foreach (var sample in samples.Where(x => x.BRR.IsValid))
            {
                sample.BRR.WriteFile(Path.Combine(path, $"{sample.SampleNumber} ({(sample.BRR.IsValid ? "Valid" : "Invalid")}) (0x{sample.SampleARAMAddress}).brr"));
            }
        }
        #endregion
    }
}
