using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    class MultiGoalPath
    {
        public Path path;
        
        public MultiGoalPath()
        {

        }

        public bool AddPath(Path newPath)
        {
            //Console.Error.WriteLine("Add: " + newPath.waypoints.First() +" to " + newPath.waypoints.Last());

            if (path == null)
            {
                path = newPath;
                return true;
            }
            else
            {
                var newSteps = newPath.waypoints.Count;

                if (newSteps > 1 && newSteps + path.waypoints.Count <= 20 &&
                    path.waypoints.Last() == newPath.waypoints.First())
                {
                    var waypoints = newPath.waypoints.ToList();
                    waypoints.RemoveAt(0);

                    path.waypoints.AddRange(waypoints);
                    return true;
                }
            }

            return false;
        }

        public Tile GetLast()
        {
            if (path == null)
            {
                return Player.me.tile;
            }
            else
            {
                return path.waypoints.Last();
            }
        }

        public void Print()
        {
            if (path == null)
            {
                Console.WriteLine("PASS");
                return;
            }

            path.PrintPath();
        }
    }
}
