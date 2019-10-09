using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    public enum Team
    {
        Ally,
        Enemy,
    }

    abstract class Entity
    {
        public Hex hexPos { get; set; }
        public Vector pos { get; set; }
        public Team team { get; set; }

        public abstract void ProcessMessage(EntityMessage message);
    }
}
