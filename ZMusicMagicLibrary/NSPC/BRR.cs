using System;
using System.Collections.Generic;
using System.IO;

namespace ZMusicMagicLibrary.NSPC
{

    public class BRR
    {
        const int BRRChunkSize = 9;

        public byte[] Data { get; set; }
        public int LoopOffset { get; set; }
        public bool Loops { get; set; }
        public bool IsValid { get; set; }

        int startOffset;
        int endOffset;

        public BRR(byte[] sampleData, int offset, int loopOffset)
        {
            IsValid = false;

            List<byte> data = new List<byte>();
            startOffset = offset;

            for (int index = offset; ; index += BRRChunkSize)
            {
                if (index >= sampleData.Length)
                {
                    data.Clear();
                    break;
                }

                byte[] chunk = new byte[BRRChunkSize];
                Array.Copy(sampleData, index, chunk, 0, 9);
                data.AddRange(chunk);

                if ((chunk[0] & 0x01) == 0x01)
                {
                    if ((chunk[0] & 0x02) != 0)
                    {
                        LoopOffset = loopOffset;
                        Loops = true;
                    }
                    endOffset = index + BRRChunkSize;
                    break;
                }
            }

            if (IsValidAddresses())
            {

            }

            this.Data = data.ToArray();
        }

        bool IsValidAddresses()
        {
            if (startOffset >= endOffset)
            {
                return false;
            }

            if (Loops)
            {
                if ((LoopOffset - startOffset) % BRRChunkSize != 0)
                {
                    return false;
                }

                if (LoopOffset < startOffset || LoopOffset >= endOffset)
                {
                    return false;
                }
            }

            IsValid = true;

            return IsValid;
        }

        public void WriteFile(string path)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                bw.Write(Data);
            }
        }
    }
}