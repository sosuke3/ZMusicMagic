using System;

namespace ZMusicMagicLibrary.NSPC
{

    public class Chunk
    {
        public int ChunkOffsetAddress { get; set; }
        public int ChunkLength { get; set; }
        public int ChunkARAMAddress { get; set; }

        public byte[] ChunkData { get; set; }

        public Chunk(Rom romData, int chunkOffsetAddress)
        {
            ChunkLength = (romData.RomData[chunkOffsetAddress + 1] << 8) + romData.RomData[chunkOffsetAddress];
            ChunkARAMAddress = (romData.RomData[chunkOffsetAddress + 3] << 8) + romData.RomData[chunkOffsetAddress + 2];
            ChunkOffsetAddress = chunkOffsetAddress + 4;

            ChunkData = romData.GetDataChunk(chunkOffsetAddress + 4, ChunkLength);
        }
    }
}