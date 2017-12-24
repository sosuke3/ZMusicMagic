namespace ZMusicMagicLibrary.NSPC
{

    public class Sample
    {
        public int SampleOffset { get; set; }
        public int LoopOffset { get; set; }
        public int SampleARAMAddress { get; set; }
        public int SampleLoopAddress { get; set; }
        public int SampleNumber { get; set; }
        public BRR BRR { get; set; }

        public Sample(byte[] sampleData, int sampleNumber, int offset, int loopOffset, int aramAddress, int loopAddress)
        {
            this.SampleOffset = offset;
            this.LoopOffset = loopOffset;
            this.SampleARAMAddress = aramAddress;
            this.SampleLoopAddress = loopAddress;
            this.SampleNumber = sampleNumber;

            this.BRR = new BRR(sampleData, SampleOffset, LoopOffset);
        }
    }
}