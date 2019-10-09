using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    class ScienceProject
    {
        public static List<ScienceProject> projects = new List<ScienceProject>();
        public Dictionary<Molecule, int> costs = new Dictionary<Molecule, int>();

        public ScienceProject()
        {

        }

        public void ProcessInputs(string[] inputs)
        {
            var costA = int.Parse(inputs[0]);
            var costB = int.Parse(inputs[1]);
            var costC = int.Parse(inputs[2]);
            var costD = int.Parse(inputs[3]);
            var costE = int.Parse(inputs[4]);

            costs.Add(Molecule.a, costA);
            costs.Add(Molecule.b, costB);
            costs.Add(Molecule.c, costC);
            costs.Add(Molecule.d, costD);
            costs.Add(Molecule.e, costE);

            projects.Add(this);
        }

        public int GetPointsLeft()
        {
            return costs.Sum(p => Math.Max(0, p.Value - Player.me.expertises[p.Key]));
        }
    }
}
