namespace SNES_SPC
{

        // Time relative to m_spc_time. Speeds up code a bit by eliminating need to
        // constantly add m_spc_time to time from CPU. CPU uses time that ends at
        // 0 to eliminate reloading end time every instruction. It pays off.
        //typedef int rel_time_t;

        public class Timer
        {
            public int next_time; // time of next event
            public int prescaler;
            public int period;
            public int divider;
            public int enabled;
            public int counter;
        };
}