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
}
