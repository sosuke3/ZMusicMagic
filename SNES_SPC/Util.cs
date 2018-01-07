using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNES_SPC
{
    public static class Util
    {
        /*
inline void set_le16( void* p, unsigned n )
{
	((unsigned char*) p) [1] = (unsigned char) (n >> 8);
	((unsigned char*) p) [0] = (unsigned char) n;
}
inline unsigned get_le16( void const* p )
{
	return  (unsigned) ((unsigned char const*) p) [1] << 8 |
			(unsigned) ((unsigned char const*) p) [0];
}
#ifndef GET_LE16
	#define GET_LE16( addr )        get_le16( addr )
	#define SET_LE16( addr, data )  set_le16( addr, data )
#endif
         */

        public static ushort get_le16(byte[] buffer, int index)
        {
            if (buffer.Length < index+1)
            {
                throw new IndexOutOfRangeException("get_le16 index is outside the bounds of buffer");
            }
            return (ushort)((buffer[index + 1] << 8) | (buffer[index]));
        }
        public static ushort GET_LE16A(byte[] buffer, int index)
        {
            return get_le16(buffer, index);
        }
        public static ushort GET_LE16(byte[] buffer, int index)
        {
            return get_le16(buffer, index);
        }
        public static short GET_LE16SA(byte[] buffer, int index)
        {
            return (short)get_le16(buffer, index);
        }
        public static ushort READ_PC16(byte[] buffer, int index)
        {
            return get_le16(buffer, index);
        }

        public static void set_le16(byte[] buffer, int index, uint value)
        {
            if (buffer.Length < index + 1)
            {
                throw new IndexOutOfRangeException("get_le16 index is outside the bounds of buffer");
            }
            buffer[index + 1] = (byte)(value >> 8);
            buffer[index] = (byte)(value);
        }
        public static void SET_LE16(byte[] buffer, int index, uint value)
        {
            set_le16(buffer, index, value);
        }
    }
}
