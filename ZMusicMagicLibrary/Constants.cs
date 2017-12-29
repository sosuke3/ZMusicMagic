using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public static class Constants
    {
        public const int EnemizerFileLength = 0x400000;
        public const int ChecksumComplimentAddress = 0x7FDC;
        public const int ChecksumAddress = 0x7FDE;

        public const string SharpCharacter = "♯";
        public const string FlatCharacter = "♭";

        public const string MultiRestCharacter = "𝄺";
        public const string FullRestCharacter = "𝄻";
        public const string HalfRestCharacter = "𝄼";
        public const string QuarterRestCharacter = "𝄽";
        public const string EigthRestCharacter = "𝄾";
        public const string SixteenthRestCharacter = "𝄿";
        public const string ThirtysecondRestCharacter = "𝅀";
        public const string SixtyfourthRestCharacter = "𝅁";
        public const string OneHundredTwentyEigthRestCharacter = "𝅂";
        // ♩ ♬♩ ♪ ♫ ♫
        // 𝄽
    }
}
