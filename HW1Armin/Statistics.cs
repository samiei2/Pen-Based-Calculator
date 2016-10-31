using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW1Armin
{
    class Statistics
    {
        public static double median(List<double> x)
        {
            List<double> temp = x;
            temp.Sort(); // n * log(n)
            return temp[temp.Count / 2];
        }
    }
}
