using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    class Molecule
    {
        public static Molecule a = new Molecule("A");
        public static Molecule b = new Molecule("B");
        public static Molecule c = new Molecule("C");
        public static Molecule d = new Molecule("D");
        public static Molecule e = new Molecule("E");

        public static Dictionary<string, Molecule> molecules = new Dictionary<string, Molecule>()
        {
            {"A", Molecule.a},
            {"B", Molecule.b},
            {"C", Molecule.c},
            {"D", Molecule.d},
            {"E", Molecule.e},
        };

        public string name;
        public int count;

        public int myCount;

        public Molecule(string name)
        {
            this.name = name;
            //molecules.Add(name, this);
        }

        public static void ProcessInputs(string[] inputs)
        {
            Molecule.a.count = int.Parse(inputs[0]);
            Molecule.b.count = int.Parse(inputs[1]);
            Molecule.c.count = int.Parse(inputs[2]);
            Molecule.d.count = int.Parse(inputs[3]);
            Molecule.e.count = int.Parse(inputs[4]);
        }

        public static void Initialize()
        {
            CalculateStats();
        }

        public static void CalculateStats()
        {
            foreach (var sample in Player.me.samples)
            {
                Molecule.a.myCount += sample.costA;
                Molecule.b.myCount += sample.costB;
                Molecule.c.myCount += sample.costC;
                Molecule.d.myCount += sample.costD;
                Molecule.e.myCount += sample.costE;
            }
        }

        public static void CleanUp()
        {
            foreach (var molecule in molecules.Values)
            {
                molecule.myCount = 0;
            }
        }
    }
}
