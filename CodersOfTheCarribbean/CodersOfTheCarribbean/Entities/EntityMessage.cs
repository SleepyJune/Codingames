using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
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

        public int x;
        public int y;

        public Hex pos;

        public EntityMessage(string str)
        {
            var inputs = str.Split(' ');

            id = int.Parse(inputs[0]);
            entityType = inputs[1];
            x = int.Parse(inputs[2]);
            y = int.Parse(inputs[3]);
            arg1 = int.Parse(inputs[4]);
            arg2 = int.Parse(inputs[5]);
            arg3 = int.Parse(inputs[6]);
            arg4 = int.Parse(inputs[7]);

            pos = new Hex(x, y);
        }

        public void ProcessMessage()
        {            
            if (entityType == "SHIP")
            {
                Entity ship = Ship.GetShipByID(id);
                ship.ProcessMessage(this);
            }

            if (entityType == "BARREL")
            {
                Entity barrel = Barrel.GetBarrelByID(id);
                barrel.ProcessMessage(this);
            }

            if (entityType == "MINE")
            {
                Entity mine = Mine.GetMineByID(id);
                mine.ProcessMessage(this);
            }

            if (entityType == "CANNONBALL")
            {
                Entity cannon = Cannon.GetCannonByID(id);
                cannon.ProcessMessage(this);
            }
        }
    }
}
