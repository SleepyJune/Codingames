using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    class SampleCombination
    {
        public List<Sample> samples;
        public GameState state;

        public SampleCombination(List<Sample> samples, GameState state)
        {
            this.samples = samples;
            this.state = state;
        }

        public Dictionary<Molecule, int> CalculateMolecules()
        {
            var moles = new Dictionary<Molecule, int>();
            moles.Add(Molecule.a, Math.Max(0, state.storageA - Player.me.storageA));
            moles.Add(Molecule.b, Math.Max(0, state.storageB - Player.me.storageB));
            moles.Add(Molecule.c, Math.Max(0, state.storageC - Player.me.storageC));
            moles.Add(Molecule.d, Math.Max(0, state.storageD - Player.me.storageD));
            moles.Add(Molecule.e, Math.Max(0, state.storageE - Player.me.storageE));

            state.PrintStateStats();

            return moles;
        }
    }

    class SampleOptimization
    {
        public static List<SampleCombination> TrySampleCombinations(GameState state, List<Sample> samples, List<Sample> prev)
        {
            var possibleCombinations = new List<SampleCombination>();

            foreach (var sample in samples)
            {
                var newList = new List<Sample>(prev); //add at least 1
                newList.Add(sample);

                var missingMolecules = sample.CalculateMolecules(state);

                if (state.canCompleteSample(missingMolecules))
                {
                    if (state.CountMolecules() + missingMolecules.Sum(p => p.Value) <= 10)
                    {
                        var newState = GameState.DecreaseMolecules(state, sample, missingMolecules);

                        
                        possibleCombinations.Add(new SampleCombination(newList, newState));

                        var sampleLists = TrySampleCombinations(newState, samples.Where(s => s != sample).ToList(), newList);
                                             
                        possibleCombinations.AddRange(sampleLists);
                    }
                }
            }

            return possibleCombinations;
        }
    }
}