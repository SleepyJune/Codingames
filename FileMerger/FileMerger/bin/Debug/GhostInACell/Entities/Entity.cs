using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    public enum Team
    {
        Neutral,
        Ally,
        Enemy,
    }   

    interface Entity
    {
        void ProcessMessage(EntityMessage message);
    }
}
