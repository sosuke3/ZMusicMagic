namespace SNES_SPC
{

        public class Ram
        {
            // padding to neutralize address overflow
            public byte[] padding1 = new byte[0x100];
            public byte[] ram = new byte[0x10000];
            public byte[] padding2 = new byte[0x100];
        }
}