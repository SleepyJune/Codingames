using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    class Cannon : Entity, IEquatable<Cannon>
    {
        public static HashSet<Cannon> cannons = new HashSet<Cannon>();

        public Ship ship;
        public int turns;
                
        public Cannon()
        {

        }

        public static void CleanUp()
        {
            cannons = new HashSet<Cannon>();
        }

        public override void ProcessMessage(EntityMessage message)
        {
            this.ship = Ship.GetShipByID(message.arg1);
            this.turns = message.arg2;
            this.hexPos = message.pos;
            this.pos = message.pos.ConvertCube();

            Console.Error.WriteLine(this.pos.toStr() + " -> " + this.turns);
        }

        public static void GetExplodingMines()
        {
            List<Cannon> cannonList = new List<Cannon>();

            foreach (var cannon in Cannon.cannons)
            {
                foreach (var mine in Mine.mines)
                {
                    if (mine.pos == cannon.pos)
                    {
                        foreach (var dir in Vector.directions)
                        {
                            Cannon newCannon = new Cannon();

                            newCannon.ship = cannon.ship;
                            newCannon.turns = cannon.turns;
                            newCannon.hexPos = cannon.hexPos;
                            newCannon.pos = cannon.pos;

                            cannonList.Add(newCannon);
                        }                        
                    }
                }
            }

            cannonList.ForEach(c => Cannon.cannons.Add(c));
        }

        public static Cannon GetCannonByID(int id)
        {
            Cannon cannon = new Cannon();
            cannons.Add(cannon);
            return cannon;
        }

        public override bool Equals(object obj)
        {
            if (obj is Cannon)
            {
                return Equals((Cannon)this);
            }

            return false;
        }

        public bool Equals(Cannon cannon)
        {
            return cannon.pos == this.pos && cannon.ship == this.ship;
        }

        public override int GetHashCode()
        {
            return this.turns * 100 * 100 + this.pos.GetHashCode();
        }
    }

    public static class CannonExtensions
    {

    }
}
