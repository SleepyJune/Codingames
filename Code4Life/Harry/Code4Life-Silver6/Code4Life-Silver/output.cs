using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
//URL: SHOWING PROBLEM GOING BACK AND FORTH DOING NOTHING (May 15,2017):https://www.codingame.com/replay/220469805 -SOLVED(May 15,2017, 3:30pm)
//URL: output = GG situation(May 15,2017):https://www.codingame.com/replay/220473658 -SOLVED(May 17,2017, 3:30pm)
//URL: output = stuck at diagnosis sending and taking from cloud (May 15,2017)://URL: output = GG situation(May 15,2017):https://www.codingame.com/replay/220476205 -SOLVED(May 17,2017, 3:30pm)
//URL: https://www.codingame.com/replay/223257212 - PROBLEM around turn 300, moving back and forth betwn molcules and diagnosis (May 17,2017)


/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
namespace Code4Life_Silver
{


    class Player
    {
        static void Main(string[] args)
        {
            Game.Get_Projects();

            // game loop
            while (true)
            {
                Game.Initialize();
                Game.Act();
                Game.Print_Output();
                Game.TurnEnd();
            }
        }
    }

    class Game
    {
        public static Dictionary<Combination, combination_info> Combinations = new Dictionary<Combination, combination_info>();
        public static List<combination_info> combinations_sorted = new List<combination_info>();
        public static Dictionary<int, Sample> samples = new Dictionary<int, Sample>();
        public static Dictionary<int, Sample> mysamples = new Dictionary<int, Sample>();
        public static List<Project> proj = new List<Project>();
        public static availableM bank = new availableM();
        public static List<int> samples_sorted = new List<int>();
        public static List<int> Med_order = new List<int>();
        public static Ally ally = new Ally();
        public static Enemy enemy = new Enemy();
        public static int turn = 1;
        public static string output = "";
        public static string err_output = "";
        public static int[] availM = new int[5];
        public static void Get_Projects()
        {
            string[] inputs;
            int projectCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < projectCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int a = int.Parse(inputs[0]);
                int b = int.Parse(inputs[1]);
                int c = int.Parse(inputs[2]);
                int d = int.Parse(inputs[3]);
                int e = int.Parse(inputs[4]);
                Project P = new Project(a, b, c, d, e);
                proj.Add(P);
            }
        }
        public static void Initialize()
        {
            string[] inputs;
            //Fill players
            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string target = inputs[0];
                int eta = int.Parse(inputs[1]);
                int score = int.Parse(inputs[2]);
                int storageA = int.Parse(inputs[3]);
                int storageB = int.Parse(inputs[4]);
                int storageC = int.Parse(inputs[5]);
                int storageD = int.Parse(inputs[6]);
                int storageE = int.Parse(inputs[7]);
                int expertiseA = int.Parse(inputs[8]);
                int expertiseB = int.Parse(inputs[9]);
                int expertiseC = int.Parse(inputs[10]);
                int expertiseD = int.Parse(inputs[11]);
                int expertiseE = int.Parse(inputs[12]);
                if (i == 0)
                {
                    ally.SetValues(target, eta, score, storageA, storageB, storageC, storageD, storageE, expertiseA, expertiseB, expertiseC, expertiseD, expertiseE);
                    Console.Error.WriteLine("EXPA: " + ally.expertiseA + " EXPB: " + ally.expertiseB + " EXPC: " + ally.expertiseC + " EXPD: " + ally.expertiseD + " EXPE: " + ally.expertiseE);
                }
                else if (i == 1)
                {
                    enemy.SetValues(target, eta, score, storageA, storageB, storageC, storageD, storageE, expertiseA, expertiseB, expertiseC, expertiseD, expertiseE);
                }
            }
            //Fill available molcules
            inputs = Console.ReadLine().Split(' ');
            int availableA = int.Parse(inputs[0]);
            int availableB = int.Parse(inputs[1]);
            int availableC = int.Parse(inputs[2]);
            int availableD = int.Parse(inputs[3]);
            int availableE = int.Parse(inputs[4]);
            bank.set_values(availableA, availableB, availableC, availableD, availableE);
            int sampleCount = int.Parse(Console.ReadLine());
            //Fill samples
            for (int i = 0; i < sampleCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int sampleId = int.Parse(inputs[0]);
                int carriedBy = int.Parse(inputs[1]);
                int rank = int.Parse(inputs[2]);
                string expertiseGain = inputs[3];
                int health = int.Parse(inputs[4]);
                int costA = int.Parse(inputs[5]);
                int costB = int.Parse(inputs[6]);
                int costC = int.Parse(inputs[7]);
                int costD = int.Parse(inputs[8]);
                int costE = int.Parse(inputs[9]);
                Sample sample = new Sample();
                bool enough_Robo_storage = true;
                int space_left = 10 - ally.storageTot;
                int deltaA = ally.storageA - costA; //  "-" if  robot need more of it
                int deltaB = ally.storageB - costB;
                int deltaC = ally.storageC - costC;
                int deltaD = ally.storageD - costD;
                int deltaE = ally.storageE - costE;
                if (deltaA < 0)
                {
                    space_left += deltaA;
                }
                if (deltaB < 0)
                {
                    space_left += deltaB;
                }
                if (deltaC < 0)
                {
                    space_left += deltaC;
                }
                if (deltaD < 0)
                {
                    space_left += deltaD;
                }
                if (deltaE < 0)
                {
                    space_left += deltaE;
                }
                if (space_left < 0)
                {
                    enough_Robo_storage = false;
                }
                sample.SetValues(sampleId, carriedBy, rank, expertiseGain, health, costA, costB, costC, costD, costE, enough_Robo_storage);
                if (carriedBy == -1)
                {
                    //CARRIED BY CLOUD, apply expertise
                    sample.costA -= ally.expertiseA;
                    if (sample.costA < 0)
                    {
                        sample.costA = 0;
                    }
                    sample.costB -= ally.expertiseB;
                    if (sample.costB < 0)
                    {
                        sample.costB = 0;
                    }
                    sample.costC -= ally.expertiseC;
                    if (sample.costC < 0)
                    {
                        sample.costC = 0;
                    }
                    sample.costD -= ally.expertiseD;
                    if (sample.costD < 0)
                    {
                        sample.costD = 0;
                    }
                    sample.costE -= ally.expertiseE;
                    if (sample.costE < 0)
                    {
                        sample.costE = 0;
                    }
                    //add samples to sample list
                    samples.Add(sample.sampleId, sample);
                }
                else if (carriedBy == 0)
                {
                    //Carried by me
                    if (health == -1)
                    {

                        ally.undiagnosed_count++;
                        ally.undiagnosed_ids.Add(sampleId);

                    }
                    //apply expertise
                    sample.costA -= ally.expertiseA;
                    if (sample.costA < 0)
                    {
                        sample.costA = 0;
                    }
                    sample.costB -= ally.expertiseB;
                    if (sample.costB < 0)
                    {
                        sample.costB = 0;
                    }
                    sample.costC -= ally.expertiseC;
                    if (sample.costC < 0)
                    {
                        sample.costC = 0;
                    }
                    sample.costD -= ally.expertiseD;
                    if (sample.costD < 0)
                    {
                        sample.costD = 0;
                    }
                    sample.costE -= ally.expertiseE;
                    if (sample.costE < 0)
                    {
                        sample.costE = 0;
                    }
                    ally.carried_sample.Add(sampleId, sample);
                    //Console.Error.WriteLine(ally.carried_sample.Count);
                    Console.Error.WriteLine("sampleId: " + sample.sampleId + " carriedBy: " + sample.carriedBy + " rank: " + sample.rank + " expertiseGain: " + sample.gain + " health : " + sample.health + " costA: " + sample.costA + " costB: " + sample.costB + " costC: " + sample.costC + " costD: " + sample.costD + " costE: " + sample.costE);

                }
                else if (carriedBy == 1)
                {
                    enemy.carried_sample.Add(sampleId, sample);
                }
            }
            //Prioritize
            Prioritize();

            //print and check if prioritize works 
            Console.Error.WriteLine("||" + samples_sorted.Count + " " + samples.Count + "||");
            foreach (int a in samples_sorted)
            {
                Console.Error.WriteLine(samples[a].sampleId + ": " + samples[a].health + " " + samples[a].costTot);
            }

        }
        public static void Prioritize()
        {
            //sort by biggest to smallest health, then cost total
            samples_sorted = samples.Values.OrderByDescending(s => s.health).ThenBy(s => s.costTot).ThenByDescending(s => s.valid).Select(s => s.sampleId).ToList();

        }
        public static void TurnEnd()
        {
            turn++;
            ally.carried_sample.Clear();
            enemy.carried_sample.Clear();
            samples.Clear();
            samples_sorted.Clear();
            Med_order.Clear();
            ally.undiagnosed_count = 0;
            ally.undiagnosed_ids.Clear();
            Combinations.Clear();
            output = "";
            err_output = "";
        }
        public static void Act()
        {
            if (ally.target == "START_POS")
            {
                output = "GOTO SAMPLES";
            }
            else if (ally.target == "SAMPLES")
            {
                AT_Samples();
            }
            else if (ally.target == "DIAGNOSIS")
            {
                AT_Diagnosis();
            }
            else if (ally.target == "MOLECULES")
            {
                AT_Molecules();
            }
            else if (ally.target == "LABORATORY")
            {
                if (ally.carried_sample.Count == 0)
                {
                    err_output = ("RESTOCK");
                    output = "GOTO SAMPLES";
                }
                else
                {
                    int A = ally.storageA;
                    int B = ally.storageB;
                    int C = ally.storageC;
                    int D = ally.storageD;
                    int E = ally.storageE;
                    bool CanGetMed = false;

                    //iterate till you find what you have
                    foreach (Sample a in ally.carried_sample.Values)
                    {
                        if (A - a.costA < 0)
                        {
                            continue;
                        }
                        else if (B - a.costB < 0)
                        {
                            continue;
                        }
                        else if (C - a.costC < 0)
                        {
                            continue;
                        }
                        else if (D - a.costD < 0)
                        {
                            continue;
                        }
                        else if (E - a.costE < 0)
                        {
                            continue;
                        }
                        //Can get this Med
                        CanGetMed = true;
                        output = "CONNECT " + a.sampleId;
                        break;
                    }
                    //Can't get any med, but still carries data
                    if (!CanGetMed)
                    {
                        if (A + B + C + D + E == 10)
                        {
                            //You are #$%@ed GG
                            output = "GG";
                        }
                        else
                        {
                            //Fill_Combinations();
                            if (Combinations.Count > 0)
                            {
                                //goto molcules to get stocked
                                err_output = ("AT LAB, but  NO MED");
                                output = "GOTO MOLECULES";
                            }
                            else
                            {
                                output = "GOTO SAMPLES";
                            }
                        }

                    }
                }

            }
        }
        public static int Get_Updated_soonX(int soonX, int availableX_tobe)
        {
            int ans = soonX;
            if (availableX_tobe < 0)
            {
                ans = soonX + availableX_tobe;
            }
            //shouldn't need this
            if (ans < 0)
            {
                ans = 0;
            }
            return ans;
        }
        public static int Get_Updated_Ally_Storage(int X_needed, int availableX_inAlly)
        {
            int ans = 0;
            if (X_needed < 0)
            {
                ans = availableX_inAlly - X_needed;
            }
            else
            {
                ans = 0;
            }
            return ans;

        }
        public static String Get_First_OUTPUT(int A_needed, int B_needed, int C_needed, int D_needed, int E_needed, int availableA, int availableB, int availableC, int availableD, int availableE)
        {
            string ans = "";
            //Molecules has enough materials, and I have enough space for them. Get them starting with the most scarce molcule
            string most_scarce = "";
            int lowest_stored_amount = 20;
            if (A_needed > 0)
            {
                lowest_stored_amount = availableA;
                most_scarce = "A";
            }
            if (B_needed > 0 && lowest_stored_amount > availableB)
            {
                lowest_stored_amount = availableB;
                most_scarce = "B";
            }
            if (C_needed > 0 && lowest_stored_amount > availableC)
            {
                lowest_stored_amount = availableC;
                most_scarce = "C";
            }
            if (D_needed > 0 && lowest_stored_amount > availableD)
            {
                lowest_stored_amount = availableD;
                most_scarce = "D";
            }
            if (E_needed > 0 && lowest_stored_amount > availableE)
            {
                lowest_stored_amount = availableE;
                most_scarce = "E";
            }
            if (most_scarce != "")
            {
                ans = "CONNECT " + most_scarce;
            }
            else
            {
                ans = "DONE";
            }
            return ans;
        }
        public static String Get_First_OUTPUT_With_WAIT(int A_needed, int B_needed, int C_needed, int D_needed, int E_needed, int availableA, int availableB, int availableC, int availableD, int availableE)
        {
            //What's different? Grab the first scarce item available or wait
            string ans = "";
            //Molecules has enough materials, and I have enough space for them. Get them starting with the most scarce molcule
            string most_scarce = "";
            int lowest_stored_amount = 0;
            if (A_needed > 0 && availableA > 0)
            {
                lowest_stored_amount = availableA;
                most_scarce = "A";
            }
            if (B_needed > 0 && lowest_stored_amount > availableB && availableB > 0)
            {
                lowest_stored_amount = availableB;
                most_scarce = "B";
            }
            if (C_needed > 0 && lowest_stored_amount > availableC && availableC > 0)
            {
                lowest_stored_amount = availableC;
                most_scarce = "C";
            }
            if (D_needed > 0 && lowest_stored_amount > availableD && availableD > 0)
            {
                lowest_stored_amount = availableD;
                most_scarce = "D";
            }
            if (E_needed > 0 && lowest_stored_amount > availableE && availableE > 0)
            {
                lowest_stored_amount = availableE;
                most_scarce = "E";
            }
            if (most_scarce != "")
            {
                ans = "CONNECT " + most_scarce;
            }
            else
            {
                ans = "WAIT";
            }
            return ans;
        }
        public static void Print_Output()
        {
            ally.Print_Status(ally.carried_sample, ally.undiagnosed_count, ally.expertiseTot);
            Console.Error.WriteLine(err_output);
            Console.WriteLine(output);
        }
        public static void AT_Samples()
        {

            //Got 3 samples
            if (ally.carried_sample.Count == 3)
            {
                output = "GOTO DIAGNOSIS";
            }
            //don't have 3 samples
            else
            {
                //when have no sample
                if (ally.carried_sample.Count == 0)
                {
                    if (ally.expertiseTot > 12)
                    {
                        output = "CONNECT " + 3;
                    }
                    else if (ally.expertiseTot > 2)
                    {
                        output = "CONNECT " + 2;
                    }
                    else
                    {
                        output = "CONNECT " + 1;
                    }
                }
                //When have one or more samples
                else
                {
                    //When have 1 sample only
                    if (ally.carried_sample.Count == 1)
                    {
                        //robot storage is full
                        if (ally.storageTot > 6)
                        {
                            if (ally.expertiseTot > 15)
                            {
                                output = "CONNECT " + 3;
                            }
                            else if (ally.expertiseTot > 5)
                            {
                                output = "CONNECT " + 2;
                            }
                            else
                            {
                                output = "CONNECT " + 1;
                            }
                        }
                        else
                        {
                            if (ally.expertiseTot > 15)
                            {
                                output = "CONNECT " + 3;
                            }
                            else if (ally.expertiseTot > 3)
                            {
                                output = "CONNECT " + 2;
                            }
                            else
                            {
                                output = "CONNECT " + 1;
                            }
                        }


                    }
                    //When have 2 samples
                    else if (ally.carried_sample.Count == 2)
                    {
                        //robot storage is full
                        if (ally.storageTot > 9)
                        {
                            if (ally.expertiseTot > 20)
                            {
                                output = "CONNECT " + 3;
                            }
                            else if (ally.expertiseTot > 9)
                            {
                                output = "CONNECT " + 2;
                            }
                            else
                            {
                                output = "CONNECT " + 1;
                            }
                        }
                        else
                        {
                            if (ally.expertiseTot > 15)
                            {
                                output = "CONNECT " + 3;
                            }
                            else if (ally.expertiseTot > 2)
                            {
                                output = "CONNECT " + 2;
                            }
                            else
                            {
                                output = "CONNECT " + 1;
                            }
                        }

                    }
                }
            }
        }
        public static void AT_Diagnosis()
        {
            //First of all diagnose any unidentified samples
            if (ally.undiagnosed_count > 0)
            {
                err_output = ("HAVE UNDIAGNOSED SAMPLE/S, DIAGNOSE THEM");
                output = "CONNECT " + ally.undiagnosed_ids[0];
            }
            //all samples diagnosed
            else
            {
                //Calculate possible combinations
                Find_Combinations();
                combinations_sorted = Combinations.Values.OrderByDescending(s => s.Health_GainTot).ThenBy(s => s.S_count).ThenBy(s => s.storageTot).ToList();
                foreach (combination_info a in combinations_sorted)
                {
                    if (a != null)
                    {
                        Console.Error.WriteLine("COMBINATION: " + a.Combo.S1 + " " + a.Combo.S2 + " " + a.Combo.S3 + " Can_Sample_Count: " + a.S_count + "  Health_GainTot: " + a.Health_GainTot + " expertiseA: " + a.expertiseA + " expertiseB: " + a.expertiseB + " expertiseC: " + a.expertiseC + " expertiseD: " + a.expertiseD + " expertiseE: " + a.expertiseE);
                    }
                }
                if (ally.carried_sample.Count == 3)
                {
                    if (combinations_sorted[0].S_count == 3)
                    {
                        //Great, Get on to Molecules
                        output = "GOTO MOLECULES";
                    }
                    else if (combinations_sorted[0].S_count == 2)
                    {
                        int RemoveID = -1;
                        Combination bo = combinations_sorted[0].Combo;
                        //Check the sample not included in the combination
                        foreach (Sample a in ally.carried_sample.Values)
                        {
                            if (bo.S1 != a.sampleId && bo.S2 != a.sampleId && bo.S3 != a.sampleId)
                            {
                                RemoveID = a.sampleId;
                            }
                        }
                        if (ally.carried_sample[RemoveID].valid)
                        {
                            output = "GOTO MOLECULES";
                        }
                        else
                        {
                            //remove it
                            output = "CONNECT " + RemoveID;
                        }
                    }
                }
                else if (ally.carried_sample.Count == 2)
                {
                    //check out cloud
                    if (samples.Count != 0)
                    {
                        //check out samples in it
                        if (samples[samples_sorted[0]].valid)
                        {
                            //ASK for it
                            output = "CONNECT " + samples_sorted[0];
                        }
                        else
                        {
                            output = "GOTO MOLECULES";
                        }
                    }
                }
            }
        }
        public static void AT_Molecules()
        {

        }
        public static void Find_Combinations()
        {
            //Find out if enemy will release some molecules for you
            int[] willbe_available_M = new int[5] { 0, 0, 0, 0, 0 };
            int[] availableM = new int[5] { bank.A, bank.B, bank.C, bank.D, bank.E };

            if (enemy.target == "LABORATORY")
            {
                //Find out what molecules WILL be available
                foreach (Sample a in enemy.carried_sample.Values)
                {
                    int[] dM = new int[5] { a.costA - enemy.expertiseA, a.costB - enemy.expertiseB, a.costC - enemy.expertiseC, a.costD - enemy.expertiseD, a.costE - enemy.expertiseE };
                    if (enemy.storageA - dM[0] >= 0 && enemy.storageB - dM[1] >= 0 && enemy.storageC - dM[2] >= 0 && enemy.storageD - dM[3] >= 0 && enemy.storageE - dM[4] >= 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            //Enemy will use this sample
                            willbe_available_M[i] += dM[i];
                        }
                    }
                }
            }
            //Fill Combination
            List<int> ID_list = new List<int>();

            foreach (Sample a in ally.carried_sample.Values)
            {
                ID_list.Add(a.sampleId);
            }
            foreach (Sample a in ally.carried_sample.Values)
            {
                Combination Combo = new Combination();
                combination_info Combo_info = new combination_info();
                Combo_info.storageTot = ally.storageTot;

                for (int i = 0; i < 5; i++)
                {
                    //Initialize avail
                    availM[i] = 0;
                    //Then set availM
                    availM[i] = willbe_available_M[i] + availableM[i];
                }
                //Set values in convo info
                Combo_info.availableA_inAlly = ally.storageA;
                Combo_info.availableB_inAlly = ally.storageB;
                Combo_info.availableC_inAlly = ally.storageC;
                Combo_info.availableD_inAlly = ally.storageD;
                Combo_info.availableE_inAlly = ally.storageE;
                Console.Error.WriteLine("First sample" + a.sampleId);
                //Find the IDs of the other samples
                List<int> other_samples = new List<int>();
                foreach (int b in ID_list)
                {
                    if (b != a.sampleId)
                    {
                        other_samples.Add(b);
                        Console.Error.WriteLine("OTHER SAMPLES" + b);
                    }
                }
                //Check sample validity(S1)
                Check_Sample(a, Combo, Combo_info);
                //Check next 2
                foreach (int b in other_samples)
                {
                    Combination Combo2 = new Combination();
                    combination_info Combo_info2 = new combination_info();
                    Combo2.S1 = Combo.S1;
                    Console.Error.WriteLine("SECOND SAMPLE: " + b + " Combo2.S1=" + Combo2.S1);
                    Combo_info2.set_comb_info(Combo2, Combo_info.Health_GainTot, Combo_info.expertiseA, Combo_info.expertiseB, Combo_info.expertiseC, Combo_info.expertiseD, Combo_info.expertiseE, Combo_info.S_count, Combo_info.storageTot, Combo_info.availableA_inAlly, Combo_info.availableB_inAlly, Combo_info.availableC_inAlly, Combo_info.availableD_inAlly, Combo_info.availableE_inAlly);
                    //Find 3rd sample
                    int sample3_ID = -1;
                    foreach (int c in other_samples)
                    {
                        if (c != b)
                        {
                            sample3_ID = c;
                        }
                    }

                    //Try second sample
                    Check_Sample(ally.carried_sample[b], Combo2, Combo_info2);
                    Console.Error.WriteLine("Thrid SAMPLE: " + sample3_ID + " Combo2.S1=" + Combo2.S1 + "Combo.S2=" + Combo2.S2);
                    //Then 3rd
                    Check_Sample(ally.carried_sample[sample3_ID], Combo2, Combo_info2);
                }
            }

        }
        public static void Check_Sample(Sample a, Combination Combo, combination_info Combo_info)
        {
            int A_needed = ally.carried_sample[a.sampleId].costA - Combo_info.availableA_inAlly - Combo_info.expertiseA;
            int B_needed = ally.carried_sample[a.sampleId].costB - Combo_info.availableB_inAlly - Combo_info.expertiseB;
            int C_needed = ally.carried_sample[a.sampleId].costC - Combo_info.availableC_inAlly - Combo_info.expertiseC;
            int D_needed = ally.carried_sample[a.sampleId].costD - Combo_info.availableD_inAlly - Combo_info.expertiseD;
            int E_needed = ally.carried_sample[a.sampleId].costE - Combo_info.availableE_inAlly - Combo_info.expertiseE;
            if (A_needed < 0)
            {
                A_needed = 0;
            }
            if (B_needed < 0)
            {
                B_needed = 0;
            }
            if (C_needed < 0)
            {
                C_needed = 0;
            }
            if (D_needed < 0)
            {
                D_needed = 0;
            }
            if (E_needed < 0)
            {
                E_needed = 0;
            }

            int needed_Tot = A_needed + B_needed + C_needed + D_needed + E_needed;

            if (A_needed <= availM[0] && B_needed <= availM[1] && C_needed <= availM[2] && D_needed <= availM[3] && E_needed <= availM[4] && needed_Tot <= 10 - Combo_info.storageTot)
            {
                //Molecules has enough materials or you alrdy have it
                //update info
                Combo_info.S_count++;
                Combo_info.storageTot += needed_Tot;
                availM[0] -= A_needed;
                availM[1] -= B_needed;
                availM[2] -= C_needed;
                availM[3] -= D_needed;
                availM[4] -= E_needed;

                Combo_info.availableA_inAlly = Get_Updated_Ally_Storage(A_needed, Combo_info.availableA_inAlly);
                Combo_info.availableB_inAlly = Get_Updated_Ally_Storage(B_needed, Combo_info.availableB_inAlly);
                Combo_info.availableC_inAlly = Get_Updated_Ally_Storage(C_needed, Combo_info.availableC_inAlly);
                Combo_info.availableD_inAlly = Get_Updated_Ally_Storage(D_needed, Combo_info.availableD_inAlly);
                Combo_info.availableE_inAlly = Get_Updated_Ally_Storage(E_needed, Combo_info.availableE_inAlly);
                Combo_info.Health_GainTot += ally.carried_sample[a.sampleId].health;
                if (a.gain == "A")
                {
                    Combo_info.expertiseA++;
                }
                else if (a.gain == "B")
                {
                    Combo_info.expertiseB++;
                }
                else if (a.gain == "C")
                {
                    Combo_info.expertiseC++;
                }
                else if (a.gain == "D")
                {
                    Combo_info.expertiseD++;
                }
                else if (a.gain == "E")
                {
                    Combo_info.expertiseE++;
                }
                //Set Combo
                if (Combo.S1 == -1)
                {
                    Combo.S1 = a.sampleId;
                }
                else if (Combo.S2 == -1)
                {
                    Combo.S2 = a.sampleId;
                }
                else if (Combo.S3 == -1)
                {
                    Combo.S3 = a.sampleId;
                }
                //Add to combinations
                //make new combination to get unique hashcode
                Combination combAdd = new Combination();
                combAdd.S1 = Combo.S1;
                combAdd.S2 = Combo.S2;
                combAdd.S3 = Combo.S3;
                //Print Combination added
                Combo.Print_sampleIDs();
                if (!Combinations.ContainsKey(combAdd))
                {
                    Combinations.Add(combAdd, Combo_info);
                }
            }
        }
    }

    class Ally
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
        public Ally()
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
        public void Print_Status(Dictionary<int, Sample> c, int count , int total_expertise)
        {
            Console.Error.WriteLine("Target: "+target+ " StorageTot: "+ storageTot);
            Console.Error.WriteLine();
            Console.Error.WriteLine("Carried_Samples: Count= "+c.Count + " Undiagnosed: "+ count);
            Console.Error.WriteLine("ExpertiseTot: " + total_expertise);
            foreach (Sample a in c.Values)
            {
                Console.Error.WriteLine("SAMPLE: "+ a.sampleId + " HEALTH: "+a.health+ " COST_T: "+a.costTot + " RANK: "+a.rank +" Enough Materials? "+a.enough_M);
            }
        }
    }

    class availableM
    {
        public int A;
        public int B;
        public int C;
        public int D;
        public int E;
        public availableM()
        {
        }
        public void set_values(int a, int b, int c, int d, int e)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
        }
    }

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

    class Project
    {
        public int A;
        public int B;
        public int C;
        public int D;
        public int E;

        public Project(int a, int b, int c, int d, int e)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
        }
    }

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
