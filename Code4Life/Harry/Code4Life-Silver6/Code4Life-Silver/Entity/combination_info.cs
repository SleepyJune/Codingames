using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life_Silver
{
    class combination_info
    {
        public Combination Combo;
        public int Health_GainTot;
        public int expertiseA;
        public int expertiseB;
        public int expertiseC;
        public int expertiseD;
        public int expertiseE;
        public int CostTot;
        public bool WAIT;
        public string output;
        public int S_count;
        public int storageTot;
        public int availableA_inAlly;
        public int availableB_inAlly;
        public int availableC_inAlly;
        public int availableD_inAlly;
        public int availableE_inAlly;
        public combination_info()
        {
            Combination Combo = new Combination();
            expertiseA = 0;
            expertiseB = 0;
            expertiseC = 0;
            expertiseD = 0;
            expertiseE = 0;
            CostTot = 0;
            S_count = 0;
            storageTot = 0;
            Health_GainTot = 0;
            availableA_inAlly = 0;
            availableB_inAlly = 0;
            availableC_inAlly = 0;
            availableD_inAlly = 0;
            availableE_inAlly = 0;
        }
        public void set_comb_info(Combination i_Combo, int i_Health_GainTot, int i_expertiseA, int i_expertiseB, int i_expertiseC, int i_expertiseD, int i_expertiseE, int i_S_count, int i_storageTot, int i_availableA_inAlly, int i_availableB_inAlly, int i_availableC_inAlly, int i_availableD_inAlly, int i_availableE_inAlly)
        {
            Combo = i_Combo;
            Health_GainTot = i_Health_GainTot;
            expertiseA = i_expertiseA;
            expertiseB = i_expertiseB;
            expertiseC = i_expertiseC;
            expertiseD = i_expertiseD;
            expertiseE = i_expertiseE;
            S_count = i_S_count;
            storageTot = i_storageTot;
            availableA_inAlly = i_availableA_inAlly;
            availableB_inAlly = i_availableB_inAlly;
            availableC_inAlly = i_availableC_inAlly;
            availableD_inAlly = i_availableD_inAlly;
            availableE_inAlly = i_availableE_inAlly;
        }
    }
}
