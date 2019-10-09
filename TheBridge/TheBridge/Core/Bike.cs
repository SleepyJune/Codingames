using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBridge
{
    class Bike
    {
        public int speed;
        public int active;

        public int x;
        public int y;

        public Bike(int active, int speed, int x, int y)
        {
            this.speed = speed;
            this.active = active;

            this.x = x;
            this.y = y;
        }

        public Bike(Bike oldBike)
        {
            this.speed = oldBike.speed;
            this.active = oldBike.active;

            this.x = oldBike.x;
            this.y = oldBike.y;
        }
    }
}
