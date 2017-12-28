﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMusicMagicLibrary
{
    public static class Utilities
    {
        public static int SnesToPCAddress(int addr)
        {
            int temp = (addr & 0x7FFF) + ((addr / 2) & 0xFF8000);
            return (temp);
        }

        public static int PCToSnesAddress(int addr)
        {
            byte[] b = BitConverter.GetBytes(addr);
            b[2] = (byte)(b[2] * 2);
            if (b[1] >= 0x80)
                b[2] += 1;
            else
                b[1] += 0x80;

            return BitConverter.ToInt32(b, 0);
        }

        public static byte[] PCAddressToSnesByteArray(int pos)
        {
            int addr = PCToSnesAddress(pos);

            return new byte[] { (byte)(addr >> 16), ((byte)(addr >> 8)), ((byte)addr) };
        }

        public static int SnesToPCAddress(byte bank, byte high, byte low)
        {
            return SnesToPCAddress(SnesByteArrayTo24bitSnesAddress(new byte[] { low, high, bank }));
        }
        public static int SnesByteArrayTo24bitSnesAddress(byte[] addressBytes)
        {
            if (addressBytes.Length != 3)
            {
                throw new Exception($"SnesByteArrayTo24bitSnesAddress requires 3 bytes of input. {addressBytes.Length} were passed in.");
            }
            return (addressBytes[2] << 16) | (addressBytes[1] << 8) | (addressBytes[0]);
        }

        public static int SnesByteArrayTo16bitSnesAddress(byte[] addressBytes)
        {
            if (addressBytes.Length != 2)
            {
                throw new Exception($"SnesByteArrayTo16bitSnesAddress requires 2 bytes of input. {addressBytes.Length} were passed in.");
            }
            return (addressBytes[1] << 8) | (addressBytes[0]);
        }

        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
