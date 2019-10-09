using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    class Player
    {
        public static Player me;
        public static Player enemy;

        public int eta;
        public int score;
        public int storageA;
        public int storageB;
        public int storageC;
        public int storageD;
        public int storageE;
        public int expertiseA;
        public int expertiseB;
        public int expertiseC;
        public int expertiseD;
        public int expertiseE;

        public List<Sample> samples = new List<Sample>();
        public Dictionary<Molecule, int> expertises = new Dictionary<Molecule, int>();
        public Dictionary<Molecule, int> storages = new Dictionary<Molecule, int>();

        public SampleCombination bestCombination = null;

        public Module module;

        public Player()
        {
            expertises.Add(Molecule.a, 0);
            expertises.Add(Molecule.b, 0);
            expertises.Add(Molecule.c, 0);
            expertises.Add(Molecule.d, 0);
            expertises.Add(Molecule.e, 0);

            storages.Add(Molecule.a, 0);
            storages.Add(Molecule.b, 0);
            storages.Add(Molecule.c, 0);
            storages.Add(Molecule.d, 0);
            storages.Add(Molecule.e, 0);
        }

        public void ProcessInputs(string[] inputs)
        {
            eta = int.Parse(inputs[1]);
            score = int.Parse(inputs[2]);
            storageA = int.Parse(inputs[3]);
            storageB = int.Parse(inputs[4]);
            storageC = int.Parse(inputs[5]);
            storageD = int.Parse(inputs[6]);
            storageE = int.Parse(inputs[7]);
            expertiseA = int.Parse(inputs[8]);
            expertiseB = int.Parse(inputs[9]);
            expertiseC = int.Parse(inputs[10]);
            expertiseD = int.Parse(inputs[11]);
            expertiseE = int.Parse(inputs[12]);

            expertises[Molecule.a] = expertiseA;
            expertises[Molecule.b] = expertiseB;
            expertises[Molecule.c] = expertiseC;
            expertises[Molecule.d] = expertiseD;
            expertises[Molecule.e] = expertiseE;

            storages[Molecule.a] = storageA;
            storages[Molecule.b] = storageB;
            storages[Molecule.c] = storageC;
            storages[Molecule.d] = storageD;
            storages[Molecule.e] = storageE;

            SetModule(inputs[0]);
        }

        public int CountExpertise()
        {
            return expertises.Sum(e => e.Value);
        }

        public int CountMolecules()
        {
            return storageA + storageB + storageC + storageD + storageE;
        }

        public void SetModule(string target)
        {
            if (target == "START_POS")
            {
                module = Module.Start;
            }
            else if (target == "SAMPLES")
            {
                module = Module.Samples;
            }
            else if (target == "DIAGNOSIS")
            {
                module = Module.Diagnosis;
            }
            else if (target == "MOLECULES")
            {
                module = Module.Molecules;
            }
            else if (target == "LABORATORY")
            {
                module = Module.Laboratory;
            }
        }

        public static void Initialize()
        {

        }

        public void PrintPlayerStats()
        {
            var player = this;

            Console.Error.WriteLine("Player");
            Console.Error.WriteLine("A: " + player.storageA);
            Console.Error.WriteLine("B: " + player.storageB);
            Console.Error.WriteLine("C: " + player.storageC);
            Console.Error.WriteLine("D: " + player.storageD);
            Console.Error.WriteLine("E: " + player.storageE);
            Console.Error.WriteLine("");
        }

        public static void CleanUp()
        {
            Player.me.samples = new List<Sample>();
            Player.enemy.samples = new List<Sample>();
        }
    }
}
