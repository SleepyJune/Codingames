using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life_Silver
{
    class Enemy
    {
        public string target;
        public int eta;
        public int score;
        public int storageA;
        public int storageB;
        public int storageC;
        public int storageD;
        public int storageE;
        public int storageTot;
        public int expertiseA;
        public int expertiseB;
        public int expertiseC;
        public int expertiseD;
        public int expertiseE;
        public int expertiseTot;
        public int undiagnosed_count;
        public Dictionary<int, Sample> carried_sample;
        public List<int> undiagnosed_ids;
        public Enemy()
        {
            carried_sample = new Dictionary<int, Sample>();
            undiagnosed_ids = new List<int>();
            undiagnosed_count = 0;
        }

        public void SetValues(
        string i_target,
        int i_eta,
        int i_score,
        int i_storageA,
        int i_storageB,
        int i_storageC,
        int i_storageD,
        int i_storageE,
        int i_expertiseA,
        int i_expertiseB,
        int i_expertiseC,
        int i_expertiseD,
        int i_expertiseE
        )
        {
            target = i_target;
            eta = i_eta;
            score = i_score;
            storageA = i_storageA;
            storageB = i_storageB;
            storageC = i_storageC;
            storageD = i_storageD;
            storageE = i_storageE;
            storageTot = storageA + storageB + storageC + storageD + storageE;
            expertiseA = i_expertiseA;
            expertiseB = i_expertiseB;
            expertiseC = i_expertiseC;
            expertiseD = i_expertiseD;
            expertiseE = i_expertiseE;
            expertiseTot = expertiseA + expertiseB + expertiseC + expertiseD + expertiseE;
        }
        public void Print_Status(Dictionary<int, Sample> c, int count, int total_expertise)
        {
            Console.Error.WriteLine("Target: " + target + " StorageTot: " + storageTot);
            Console.Error.WriteLine("Carried_Samples: Count= " + c.Count + " Undiagnosed: " + count);
            Console.Error.WriteLine("ExpertiseTot: " + total_expertise);
            foreach (Sample a in c.Values)
            {
                Console.Error.WriteLine("SAMPLE: " + a.sampleId + " HEALTH: " + a.health + " COST_T: " + a.costTot + " RANK: " + a.rank + " Enough Materials? " + a.enough_M);
            }
        }
    }
}
