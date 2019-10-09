using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class Timer
    {
        private static DateTime loadTime = DateTime.Now;

        public static float TickCount
        {
            get
            {
                return (int)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
            }
        }
    }
}
