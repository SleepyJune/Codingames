using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    class Sample
    {
        public static Dictionary<int, Sample> samples = new Dictionary<int, Sample>();

        public Dictionary<Molecule, int> costs = new Dictionary<Molecule, int>();

        public int id;
        public int carriedBy;
        public int rank;
        public string expertiseGain;
        public int health;
        public int costA;
        public int costB;
        public int costC;
        public int costD;
        public int costE;

        public Dictionary<Molecule, int> molecules = new Dictionary<Molecule, int>();
        public Dictionary<Molecule, int> enemyMolecules = new Dictionary<Molecule, int>();
        public Dictionary<Molecule, int> enemyRefundMolecules = new Dictionary<Molecule, int>();

        public double similarity;

        public int totalCost;

        public Player player = null;

        public Sample()
        {

        }

        public void ProcessInputs(string[] inputs)
        {
            id = int.Parse(inputs[0]);
            carriedBy = int.Parse(inputs[1]);
            rank = int.Parse(inputs[2]);
            expertiseGain = inputs[3];
            health = int.Parse(inputs[4]);
            costA = int.Parse(inputs[5]);
            costB = int.Parse(inputs[6]);
            costC = int.Parse(inputs[7]);
            costD = int.Parse(inputs[8]);
            costE = int.Parse(inputs[9]);

            costs.Add(Molecule.a, costA);
            costs.Add(Molecule.b, costB);
            costs.Add(Molecule.c, costC);
            costs.Add(Molecule.d, costD);
            costs.Add(Molecule.e, costE);

            if (carriedBy == 0)
            {
                player = Player.me;
                player.samples.Add(this);

                CalculateCost();                
            }
            else if (carriedBy == 1)
            {
                player = Player.enemy;
                player.samples.Add(this);                
            }

            if (carriedBy != -1)
            {
                CalculateSimilarity();
            }

            if (health != -1)
            {
                CalculateMolecules();
                CalculateEnemyMolecules();
            }
        }

        public static List<Sample> GetAvailableSamples()
        {
            List<Sample> newList = new List<Sample>(Player.me.samples);
            newList.AddRange(samples.Values.Where(s => s.carriedBy == -1));

            return newList;
        }

        public bool isCompleted()
        {
            if (carriedBy == 0 || carriedBy == -1)
            {
                return !molecules.Any(p => p.Value > 0) && health != -1;
            }
            else
            {
                return !enemyMolecules.Any(p => p.Value > 0) && health != -1;
            }
        }

        public bool canResearch()
        {
            return !costs.Any(p => p.Value - player.expertises[p.Key] - player.storages[p.Key] > 5);//p.Key.count);
        }

        public bool canCompleteSample(bool isMe = true)
        {
            if (isMe)
            {
                return canCompleteSample(this.molecules);
            }
            else
            {
                return canCompleteSample(this.enemyMolecules);
            }
        }

        public bool canCompleteSample(Dictionary<Molecule, int> missingMolecules)
        {
            return !missingMolecules.Any(p => p.Key.count < p.Value);
        }

        public bool hardToResearch()
        {
            var pair = this.molecules.OrderByDescending(p => p.Value).First();
            var molecule = pair.Key;
            var count = pair.Value;
            if (count >= 4)
            {
                if (Player.me.samples.Any(s => s.id != this.id && s.costs[molecule] >= count))
                {
                    return true;
                }
            }

            return costs.Any(p => p.Value > 4 && p.Value - Player.me.expertises[p.Key] - Player.me.storages[p.Key] >= p.Key.count);
        }

        public void CalculateSimilarity()
        {
            similarity = 
                costs.Where(c => c.Value > 0)
                     .Sum(c => Math.Min(1, (player.storageA + player.expertiseA) / c.Value));
            similarity /= 5;
        }

        public void CalculateCost()
        {
            totalCost = costs.Sum(c => Math.Max(0, c.Value - player.expertises[c.Key] - player.storages[c.Key]));
        }

        public void CalculateMolecules()
        {
            var player = Player.me;

            foreach (var c in costs)
            {
                molecules.Add(c.Key, c.Value - player.expertises[c.Key] - player.storages[c.Key]);
            }
        }

        public void CalculateEnemyMolecules()
        {
            var player = Player.enemy;

            foreach (var c in costs)
            {
                enemyMolecules.Add(c.Key, c.Value - player.expertises[c.Key] - player.storages[c.Key]);
                enemyRefundMolecules.Add(c.Key, Math.Max(0,c.Value - player.expertises[c.Key]));
            }
        }

        public Dictionary<Molecule, int> CalculateMolecules(GameState state)
        {
            var moles = new Dictionary<Molecule, int>();
            moles.Add(Molecule.a, (int)Math.Max(0, costA - state.expertiseA - state.storageA + state.reservedA));
            moles.Add(Molecule.b, (int)Math.Max(0, costB - state.expertiseB - state.storageB + state.reservedB));
            moles.Add(Molecule.c, (int)Math.Max(0, costC - state.expertiseC - state.storageC + state.reservedC));
            moles.Add(Molecule.d, (int)Math.Max(0, costD - state.expertiseD - state.storageD + state.reservedD));
            moles.Add(Molecule.e, (int)Math.Max(0, costE - state.expertiseE - state.storageE + state.reservedE));

            return moles;
        }

        public static void Initialize()
        {

        }

        public void PrintStats()
        {
            Console.Error.WriteLine("Sample " + id);
            //Console.Error.WriteLine("Health: " + health);
            Console.Error.WriteLine("costA: " + costA);
            Console.Error.WriteLine("costB: " + costB);
            Console.Error.WriteLine("costC: " + costC);
            Console.Error.WriteLine("costD: " + costD);
            Console.Error.WriteLine("costE: " + costE);
            Console.Error.WriteLine("Gain: " + expertiseGain);
            Console.Error.WriteLine("");
        }

        public static void CleanUp()
        {
            samples = new Dictionary<int, Sample>();
        }
    }
}
