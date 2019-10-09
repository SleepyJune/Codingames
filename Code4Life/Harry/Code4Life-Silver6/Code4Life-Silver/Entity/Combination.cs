using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life_Silver
{
    class Combination
    {
        public int S1 = -1;
        public int S2 = -1;
        public int S3 = -1;

        public Combination()
        {
            S1 = -1;
            S2 = -1;
            S3 = -1;
        }
        public void Set_Comb(int one,int two,int three)
        {
            S1 = one;
            S2 = two;
            S3 = three;
        }
        public void Print_sampleIDs()
        {
            Console.Error.WriteLine(S1 +" "+S2 +" "+S3);
        }
    }
}
