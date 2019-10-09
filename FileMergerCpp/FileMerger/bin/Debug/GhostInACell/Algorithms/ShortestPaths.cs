using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class ShortestPaths
    {
        public Dictionary<Factory, int> shortestDistance = new Dictionary<Factory, int>();
        public Dictionary<Factory, Factory> previousFactory = new Dictionary<Factory, Factory>();

        public ShortestPaths(Dictionary<Factory, int> dist, Dictionary<Factory, Factory> prev)
        {
            this.shortestDistance = dist;
            this.previousFactory = prev;
        }
    }
}
