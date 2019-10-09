using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life_Silver
{
    class Sample
    {
        public int sampleId;
        public int carriedBy;
        public int rank;
        public string gain;
        public int health;
        public int costA;
        public int costB;
        public int costC;
        public int costD;
        public int costE;
        public int costTot;
        public bool enough_M;
        public bool enough_Robo_storage;
        public bool valid;
        public Sample()
        {
            valid = true;
        }
        public void SetValues(
            int i_ampleId,
            int i_carriedBy,
            int i_rank,
            string i_gain,
            int i_health,
            int i_costA,
            int i_costB,
            int i_costC,
            int i_costD,
            int i_costE,
            bool i_enough_Robo_storage)
        {
            sampleId = i_ampleId;
            carriedBy = i_carriedBy;
            rank = i_rank;
            gain = i_gain;
            health = i_health;
            costA = i_costA;
            costB = i_costB;
            costC = i_costC;
            costD = i_costD;
            costE = i_costE;
            costTot = costA + costB + costC + costD + costE;
//            enough_M = i_enough_M;
            enough_Robo_storage = i_enough_Robo_storage;
            if(costA>5 || costB > 5 || costC > 5 || costD > 5 || costE > 5)
            {
                valid = false;
            }
        }
    }
}
