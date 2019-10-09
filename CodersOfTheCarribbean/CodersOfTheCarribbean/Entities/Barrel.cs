using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    class Barrel : Entity, IEquatable<Barrel>
    {
        public static HashSet<Barrel> barrels = new HashSet<Barrel>();

        public int rum;

        public Barrel()
        {

        }

        public override void ProcessMessage(EntityMessage message)
        {
            this.hexPos = message.pos;
            this.pos = message.pos.ConvertCube();

            this.rum = message.arg1;
        }

        public static void CleanUp()
        {
            barrels = new HashSet<Barrel>();
        }

        public static Barrel GetBarrelByID(int id)
        {
            Barrel barrel = new Barrel();
            barrels.Add(barrel);
            return barrel;
        }

        public override bool Equals(object obj)
        {
            if (obj is Barrel)
            {
                return Equals((Barrel)this);
            }

            return false;
        }

        public bool Equals(Barrel barrel)
        {
            return barrel.pos == this.pos;
        }

        public override int GetHashCode()
        {
            return this.pos.GetHashCode();
        }
    }
}
