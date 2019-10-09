using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class PathInfo
    {
        public Dictionary<Factory, int> shortestDistance = new Dictionary<Factory, int>();
        public Dictionary<Factory, Factory> previousFactory = new Dictionary<Factory, Factory>();

        public Factory start;

        public PathInfo(Factory start, Dictionary<Factory, int> dist, Dictionary<Factory, Factory> prev)
        {
            this.start = start;

            this.shortestDistance = dist;
            this.previousFactory = prev;
        }

        public int GetPathLength(Factory end)
        {
            if (start == end)
            {
                return 0;
            }

            Factory current = end;
            int totalDistance = previousFactory[current].links[current] + 1;

            while (previousFactory[current] != start)
            {
                totalDistance += previousFactory[current].links[current] + 1;
                current = previousFactory[current];
            }

            totalDistance += previousFactory[current].links[current] + 1;

            return totalDistance;
        }
    }
}
