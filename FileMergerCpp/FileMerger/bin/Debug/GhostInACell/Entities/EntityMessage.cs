using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class EntityMessage
    {
        public int id;
        public string entityType;
        public int arg1;
        public int arg2;
        public int arg3;
        public int arg4;
        public int arg5;

        public EntityMessage(string str)
        {
            var inputs = str.Split(' ');
            this.id = int.Parse(inputs[0]);
            this.entityType = inputs[1];
            this.arg1 = int.Parse(inputs[2]);
            this.arg2 = int.Parse(inputs[3]);
            this.arg3 = int.Parse(inputs[4]);
            this.arg4 = int.Parse(inputs[5]);
            this.arg5 = int.Parse(inputs[6]);
        }

        public void ProcessMessage()
        {
            if (entityType == "FACTORY")
            {
                Entity factory = Factory.GetFactoryByID(id);
                factory.ProcessMessage(this);
            }

            if (entityType == "TROOP")
            {
                Entity troop = Troop.GetTroopByID(id);
                troop.ProcessMessage(this);
            }

            if (entityType == "BOMB")
            {
                Entity bomb = Bomb.GetBombByID(id);
                bomb.ProcessMessage(this);
            }
        }
    }
}
