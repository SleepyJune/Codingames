using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    class Mine : Entity, IEquatable<Mine>
    {
        public static HashSet<Mine> mines = new HashSet<Mine>();
        public static HashSet<Hex> minePos = new HashSet<Hex>();

        public Mine()
        {

        }

        public override void ProcessMessage(EntityMessage message)
        {
            this.hexPos = message.pos;
            this.pos = message.pos.ConvertCube();

            if (!minePos.Contains(this.hexPos))
            {
                minePos.Add(this.hexPos);
            }
        }

        public static void CleanUp()
        {
            mines = new HashSet<Mine>();
            minePos = new HashSet<Hex>();
        }

        public static Mine GetMineByID(int id)
        {
            Mine mine = new Mine();
            mines.Add(mine);
            return mine;
        }

        public override bool Equals(object obj)
        {
            if (obj is Mine)
            {
                return Equals((Mine)this);
            }

            return false;
        }

        public bool Equals(Mine obj)
        {
            return obj.pos == this.pos;
        }

        public override int GetHashCode()
        {
            return this.pos.GetHashCode();
        }
    }
}
