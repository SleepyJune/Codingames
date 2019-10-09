using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    class Path
    {
        public List<Tile> waypoints = new List<Tile>();

        public Path(Tile start, Tile end)
        {
            MakePath(start, end);
        }

        public void MakePath(Tile start, Tile end)
        {
            List<Tile> points = new List<Tile>();

            Tile current = end;
            while (current.parent != null && start.pos != end.pos)
            {
                points.Add(current);
                current = current.parent;
            }

            points.Add(start);

            points.Reverse();
            waypoints = points;
        }

        public void PrintPath()
        {
            string output = "Move";

            Tile previous = null;
            foreach (var point in waypoints)
            {
                if (previous != null)
                {
                    var dir = previous.neighbours[point];
                    output += " " + dir.ToString();
                }
                previous = point; 
            }

            Console.WriteLine(output.ToUpper());
        }
    }
}
