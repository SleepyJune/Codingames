using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    struct GameState
    {
        //Player State
        public int storageA;
        public int storageB;
        public int storageC;
        public int storageD;
        public int storageE;

        public int reservedA;
        public int reservedB;
        public int reservedC;
        public int reservedD;
        public int reservedE;

        public int expertiseA;
        public int expertiseB;
        public int expertiseC;
        public int expertiseD;
        public int expertiseE;

        public int moleculeA;
        public int moleculeB;
        public int moleculeC;
        public int moleculeD;
        public int moleculeE;

        public GameState(Player player)
        {
            var me = player;

            storageA = me.storageA;
            storageB = me.storageB;
            storageC = me.storageC;
            storageD = me.storageD;
            storageE = me.storageE;

            reservedA = 0;
            reservedB = 0;
            reservedC = 0;
            reservedD = 0;
            reservedE = 0;

            expertiseA = me.expertiseA;
            expertiseB = me.expertiseB;
            expertiseC = me.expertiseC;
            expertiseD = me.expertiseD;
            expertiseE = me.expertiseE;

            moleculeA = Molecule.a.count;
            moleculeB = Molecule.b.count;
            moleculeC = Molecule.c.count;
            moleculeD = Molecule.d.count;
            moleculeE = Molecule.e.count;
        }

        public bool canCompleteSample(Dictionary<Molecule, int> missingMolecules)
        {
            return
                   moleculeA >= missingMolecules[Molecule.a]
                && moleculeB >= missingMolecules[Molecule.b]
                && moleculeC >= missingMolecules[Molecule.c]
                && moleculeD >= missingMolecules[Molecule.d]
                && moleculeE >= missingMolecules[Molecule.e];
        }

        public int CountMolecules()
        {
            return storageA + storageB + storageC + storageD + storageE;
        }

        public void AddRefundSample(Sample sample)
        {
            moleculeA += sample.enemyRefundMolecules[Molecule.a];
            moleculeB += sample.enemyRefundMolecules[Molecule.b];
            moleculeC += sample.enemyRefundMolecules[Molecule.c];
            moleculeD += sample.enemyRefundMolecules[Molecule.d];
            moleculeE += sample.enemyRefundMolecules[Molecule.e];
        }

        public static GameState DecreaseMolecules(GameState state, Sample sample, Dictionary<Molecule, int> missingMolecules)
        {
            foreach (var pair in missingMolecules)
            {
                var molecule = pair.Key;
                int count = pair.Value;
                                
                switch (molecule.name)
                {
                    case "A":
                        state.storageA += count;
                        state.moleculeA -= count;
                        state.reservedA += Math.Max(0, sample.costA - state.expertiseA);
                        break;
                    case "B":
                        state.storageB += count;
                        state.moleculeB -= count;
                        state.reservedB += Math.Max(0, sample.costB - state.expertiseB);
                        break;
                    case "C":
                        state.storageC += count;
                        state.moleculeC -= count;
                        state.reservedC += Math.Max(0, sample.costC - state.expertiseC);
                        break;
                    case "D":
                        state.storageD += count;
                        state.moleculeD -= count;
                        state.reservedD += Math.Max(0, sample.costD - state.expertiseD);
                        break;
                    case "E":
                        state.storageE += count;
                        state.moleculeE -= count;
                        state.reservedE += Math.Max(0, sample.costE - state.expertiseE);
                        break;
                }
            }

            switch (sample.expertiseGain)
            {
                case "A":
                    state.expertiseA++;
                    break;
                case "B":
                    state.expertiseB++;
                    break;
                case "C":
                    state.expertiseC++;
                    break;
                case "D":
                    state.expertiseD++;
                    break;
                case "E":
                    state.expertiseE++;
                    break;
            }

            return state;
        }

        public void PrintStateStats()
        {
            //var player = this;

            Console.Error.WriteLine("State");
            Console.Error.WriteLine("A: " + this.storageA);
            Console.Error.WriteLine("B: " + this.storageB);
            Console.Error.WriteLine("C: " + this.storageC);
            Console.Error.WriteLine("D: " + this.storageD);
            Console.Error.WriteLine("E: " + this.storageE);
            Console.Error.WriteLine("");
        }
    }
}
