using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNES_SPC
{
    public class SPC_State_Copier
    {
        SPC_DSP.copy_func_t func;
        byte[] buf;

        public SPC_State_Copier(byte[] p, SPC_DSP.copy_func_t f) { func = f; buf = p; }
        public void copy(byte[] state, int size)
        {
            func(buf, state, size);
        }
        public int copy_int(int state, int size)
        {
            byte[] s = new byte[2];
            Util.SET_LE16(s, 0, state);
            func(buf, s, size);
            return Util.GET_LE16(s, 0);
        }
        public void skip(int count)
        {
            if (count > 0)
            {
                byte[] temp = new byte[64];
                //memset(temp, 0, sizeof temp);
                do
                {
                    int n = temp.Length;
                    if (n > count)
                    {
                        n = count;
                    }
                    count -= n;
                    func(buf, temp, n);
                }
                while (count > 0);
            }
        }

        public void extra()
        {
            int n = 0;
            //SPC_State_Copier & copier = *this;
            //SPC_COPY(uint8_t, n);
            SPC_COPY_uint8_t(ref n);
            if((byte)n != n)
            {
                throw new Exception("(byte)n != n");
            }
            skip(n);
        }

        public void SPC_COPY_uint8_t(ref byte n)
        {
            n = (byte)copy_int(n, sizeof(byte));
        }

        public void SPC_COPY_uint8_t(ref int n)
        {
            n = (byte)copy_int(n, sizeof(byte));
        }

        public void SPC_COPY_int16_t(ref int n)
        {
            n = (short)copy_int(n, sizeof(short));
        }

        public void SPC_COPY_uint16_t(ref int n)
        {
            n = (ushort)copy_int(n, sizeof(ushort));
        }

        /*
#define SPC_COPY( type, state )\
{\
	state = (BOOST::type) copier.copy_int( state, sizeof (BOOST::type) );\
	assert( (BOOST::type) state == state );\
}
         */
    }
}
