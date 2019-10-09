using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    class Strategy
    {
        public static bool MoveWait()
        {
            //Wait
            if (Player.me.eta > 0)
            {
                Action newAction = new Action();
                Action.AddAction(newAction);
                return false;
            }

            return true;
        }

        public static bool Collect()
        {
            //Collect
            if (Player.me.module == Module.Start)
            {
                Action newAction = new Action(Module.Samples);
                Action.AddAction(newAction);
                return false;
            }

            var completedSamplesAvailable = Sample.samples.Values.Where(s => s.carriedBy == -1 && s.isCompleted()).Count();
            if (Player.me.module == Module.Samples && Player.me.samples.Count + completedSamplesAvailable < 3)
            {
                int exp = Player.me.CountExpertise();
                int samplesLeft = 3 - Player.me.samples.Count;

                int rank = 1;

                if (exp >= 6)
                {
                    rank = 2;
                }

                if (exp >= 12 && Player.me.CountMolecules() == 0)
                {
                    if(Player.enemy.module == Module.Molecules ||
                        Player.enemy.module == Module.Laboratory)
                    {
                        rank = 3;
                    }
                }

                var turnsLeft = 200 - Game.gameTurn;
                if (turnsLeft < 50)
                {
                    rank = 2;
                }

                Action newAction = new Action(rank);
                Action.AddAction(newAction);
                return false;
            }

            return true;
        }

        public static bool Analyze()
        {
            //Analyze
            if (Player.me.module == Module.Samples)
            {
                Action newAction = new Action(Module.Diagnosis);
                Action.AddAction(newAction);
                return false;
            }

            if (Player.me.module == Module.Diagnosis)
            {
                //getting samples from cloud
                if (Player.me.samples.Count < 3)
                {
                    foreach (var sample in Sample.samples.Values
                                                .Where(s => s.carriedBy == -1)
                                                .OrderByDescending(s => s.isCompleted()))
                    {
                        if (!sample.hardToResearch() && sample.canCompleteSample())
                        {
                            Action newAction = new Action(sample);
                            Action.AddAction(newAction);
                            return false;
                        }
                        else if (sample.isCompleted())
                        {
                            Action newAction = new Action(sample);
                            Action.AddAction(newAction);
                            return false;
                        }
                    }
                }

                //analyze undiagnosed samples
                foreach (var sample in Player.me.samples)
                {
                    if (sample.health == -1)
                    {
                        Action newAction = new Action(sample);
                        Action.AddAction(newAction);
                        return false;
                    }
                    else
                    {
                        if (!sample.canResearch())//if (sample.hardToResearch())
                        {
                            Action newAction = new Action(sample);
                            Action.AddAction(newAction);
                            return false;
                        }
                    }
                }
                
                //Can't complete any of the samples, return the samples back into the cloud
                if (!Player.me.samples.Any(s => s.canCompleteSample()) && Player.me.samples.Count > 1)
                {
                    foreach (var sample in Player.me.samples.OrderByDescending(s => s.costs.Max(m => m.Value)))
                    {
                        Action newAction = new Action(sample);
                        Action.AddAction(newAction);
                        return false;
                    }
                }

                if (Player.me.samples.Count <= 1)
                {
                    Action newAction = new Action(Module.Samples);
                    Action.AddAction(newAction);
                    return false;
                }
            }

            if (Player.me.module == Module.Diagnosis)
            {
                Action newAction = new Action(Module.Molecules);
                Action.AddAction(newAction);
                return false;
            }

            return true;
        }

        public static bool Gather()
        {
            //Gather
            if (Player.me.module == Module.Molecules && Player.me.CountMolecules() < 10)
            {                
                //Stop enemy sample 1
                foreach (var sample in Player.enemy.samples)
                {
                    if (sample.canResearch())
                    {
                        foreach (var pair in sample.costs.Where(p => p.Value - Player.enemy.expertises[p.Key] - Player.enemy.storages[p.Key] == p.Key.count))
                        {
                            var molecule = pair.Key;
                            int count = pair.Value;

                            if (count > 4 && molecule.count > 0)
                            {
                                Action newAction = new Action(molecule);
                                Action.AddAction(newAction);
                                return false;
                            }
                        }
                    }
                }

                var combinations = SampleOptimization.TrySampleCombinations(new GameState(Player.me), Player.me.samples, new List<Sample>());
                if (combinations.Count > 0)
                {
                    //Get the longest combination, then by lowest total molecule count
                    var bestCombination =
                        combinations
                        .OrderByDescending(c => c.samples.Count)
                        .ThenBy(c => c.state.CountMolecules()).First();

                    //if enemy is already at lab, wait for molecules/try better combination
                    //maybe move this up
                    if (Player.enemy.module == Module.Laboratory)
                    {
                        var refundGameState = new GameState(Player.me);
                        foreach (var sample in Player.enemy.samples.Where(s => s.isCompleted()))
                        {
                            refundGameState.AddRefundSample(sample);
                        }

                        var refundCombination = SampleOptimization.TrySampleCombinations(refundGameState, Player.me.samples, new List<Sample>())
                                                .OrderByDescending(c => c.samples.Count)
                                                .ThenBy(c => c.state.CountMolecules()).First();

                        if (refundCombination.samples.Count > bestCombination.samples.Count)
                        {
                            bestCombination = refundCombination;
                        }
                    }

                    Player.me.bestCombination = bestCombination;

                    //grab the missing molecules of the best combination
                    var missingMolecules = bestCombination.CalculateMolecules();
                    foreach (var pair in missingMolecules.OrderByDescending(p => p.Key.myCount))
                    {
                        var molecule = pair.Key;
                        int count = pair.Value;

                        if (count > 0 && molecule.count > 0)
                        {
                            Action newAction = new Action(molecule);
                            Action.AddAction(newAction);
                            return false;
                        }
                    }

                    //Stop enemy sample 2
                    foreach (var sample in Player.enemy.samples)
                    {
                        if (sample.canResearch())
                        {
                            foreach (var pair in sample.costs.OrderBy(p => p.Value))
                            {
                                var molecule = pair.Key;
                                int count = pair.Value;

                                if (count > 3 && molecule.count > 0)
                                {
                                    Action newAction = new Action(molecule);
                                    Action.AddAction(newAction);
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Player.me.bestCombination = null;
                }

                if (Player.me.module == Module.Molecules)
                {
                    if (!Player.me.samples.Any(s => s.isCompleted())) //no sample is completed
                    {
                        if (Player.me.samples.Count <= 1)
                        {
                            Action newAction = new Action(Module.Samples);
                            Action.AddAction(newAction);
                            return false;
                        }
                        else
                        {
                            Action newAction = new Action(Module.Diagnosis);
                            Action.AddAction(newAction);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool Produce()
        {
            //Produce
            if (Player.me.module == Module.Molecules)
            {
                Action newAction = new Action(Module.Laboratory);
                Action.AddAction(newAction);
                return false;
            }

            if (Player.me.module == Module.Laboratory)
            {
                if (Player.me.bestCombination != null)
                {
                    foreach (var tSample in Player.me.bestCombination.samples)
                    {
                        var sample = Player.me.samples.FirstOrDefault(s => s.id == tSample.id);

                        if (sample != null && sample.isCompleted() && sample.health != -1)
                        {
                            Action newAction = new Action(sample);
                            Action.AddAction(newAction);
                            return false;
                        }
                    }
                }
                else //no best combination, use default action
                {
                    foreach (var sample in Player.me.samples.OrderByDescending(s => s.totalCost))
                    {
                        if (sample.isCompleted() && sample.health != -1)
                        {
                            Action newAction = new Action(sample);
                            Action.AddAction(newAction);
                            return false;
                        }

                    }
                }
            }

            //Go Back Collecting
            if (Player.me.module == Module.Laboratory)
            {
                //if enemy is already at lab
                if (Player.me.samples.Count > 0 && Player.enemy.module == Module.Laboratory)
                {
                    var refundGameState = new GameState(Player.me);
                    foreach (var sample in Player.enemy.samples.Where(s => s.isCompleted()))
                    {
                        refundGameState.AddRefundSample(sample);
                    }

                    var refundCombination = SampleOptimization.TrySampleCombinations(refundGameState, Player.me.samples, new List<Sample>());

                    if (refundCombination.Count > 0)
                    {
                        Action newAction = new Action(Module.Molecules);
                        Action.AddAction(newAction);
                        return false;
                    }
                }

                if (Player.me.samples.Count > 0 && Player.me.samples.Any(s => s.canCompleteSample()))
                {
                    Action newAction = new Action(Module.Molecules);
                    Action.AddAction(newAction);
                    return false;
                }
                else if (Player.me.samples.Count < 3)
                {
                    Action newAction = new Action(Module.Samples);
                    Action.AddAction(newAction);
                    return false;
                }

            }

            return true;
        }
    }
}
