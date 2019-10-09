using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
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
}
