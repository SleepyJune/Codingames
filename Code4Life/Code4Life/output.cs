
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inputs;
            int projectCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < projectCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');

                ScienceProject newProject = new ScienceProject();
                newProject.ProcessInputs(inputs);
            }

            Player.me = new Player();
            Player.enemy = new Player();

            // game loop
            while (true)
            {
                for (int i = 0; i < 2; i++)
                {
                    inputs = Console.ReadLine().Split(' ');

                    
                    if (i == 0)
                    {
                        Player.me.ProcessInputs(inputs);
                    }
                    else
                    {
                        Player.enemy.ProcessInputs(inputs);
                    }
                }

                inputs = Console.ReadLine().Split(' ');
                Molecule.ProcessInputs(inputs);

                int sampleCount = int.Parse(Console.ReadLine());
                for (int i = 0; i < sampleCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');

                    Sample newSample = new Sample();
                    newSample.ProcessInputs(inputs);

                    Sample.samples.Add(newSample.id, newSample);                    
                }

                Game.InitializeTurn();
                Game.MakeMove();
                Game.PrintActions();
                Game.CleanUp();
            }
        }
    }

    class Game
    {
        public static int gameTurn = 0;

        public static void InitializeFirstTurn()
        {
            float loopStartTime = Timer.TickCount;

            
            float loopTime = Timer.TickCount - loopStartTime;
            //Console.Error.WriteLine("Initialization Time: " + loopTime);
        }

        public static void InitializeTurn()
        {
            Player.Initialize();
            Sample.Initialize();
            Molecule.Initialize();
        }

        public static void MakeMove()
        {
            PrintStats();

            bool success =
            Strategy.MoveWait() &&
            Strategy.Collect() &&
            Strategy.Analyze() &&
            Strategy.Gather() &&
            Strategy.Produce();
        }

        public static void PrintActions()
        {
            Action.PrintActions();
        }

        public static void PrintStats()
        {
            Player.me.PrintPlayerStats();
            foreach (var sample in Player.me.samples)
            {
                sample.PrintStats();
            }
        }

        public static void CleanUp()
        {
            Action.CleanUp();
            Player.CleanUp();
            Sample.CleanUp();
            Molecule.CleanUp();

            gameTurn++;
        }
    }

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

    public enum MoveType
    {
        Goto,
        Connect,
        Wait,
    }

    public enum Module
    {
        Start,
        Samples,
        Diagnosis,
        Molecules,
        Laboratory,
    }

    class Action
    {
        public static List<Action> actions = new List<Action>();

        public MoveType move;
        public Module module;

        public Sample sample;
        public Molecule molecule;        
        public int rank;

        public Action()
        {
            this.move = MoveType.Wait;
        }

        public Action(Module module)
        {
            this.move = MoveType.Goto;
            this.module = module;
        }

        public Action(Sample sample)
        {
            this.move = MoveType.Connect;
            this.sample = sample;
            this.module = Player.me.module;
        }

        public Action(Molecule molecule)
        {
            this.move = MoveType.Connect;
            this.molecule = molecule;
            this.module = Player.me.module;
        }

        public Action(int rank)
        {
            this.move = MoveType.Connect;
            this.rank = rank;
            this.module = Player.me.module;
        }

        public static void AddAction(Action action)
        {            
            Action.actions.Add(action);
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void PrintActions()
        {
            string str = "";

            foreach (var action in actions)
            {
                if (action.move == MoveType.Goto)
                {
                    str += "GOTO " + action.module.ToString().ToUpper();
                }
                else if (action.move == MoveType.Connect)
                {
                    if (action.module == Module.Molecules)
                    {
                        str += "CONNECT " + action.molecule.name;
                    }
                    else if (action.module == Module.Samples)
                    {
                        str += "CONNECT " + action.rank;
                    }
                    else
                    {                        
                        str += "CONNECT " + action.sample.id;
                    }
                }
                else if (action.move == MoveType.Wait)
                {
                    str += "WAIT";
                }

                break;
            }

            Console.WriteLine(str);
        }
    }

    class Timer
    {
        private static DateTime loadTime = DateTime.Now;

        public static float TickCount
        {
            get
            {
                return (int)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
            }
        }
    }




}
