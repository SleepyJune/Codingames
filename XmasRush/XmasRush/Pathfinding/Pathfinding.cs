using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    class Pathfinding
    {
        public static void Initialize()
        {
            Tile.InitializeNeighbours();
        }

        public static void ResetNodes()
        {
            foreach (var node in Tile.tiles.Values)
            {
                node.gScore = 9999;
                node.fScore = 9999;
                node.parent = null;
            }
        }

        public static Path CalculateShortestPath(Tile start, Tile end, bool useAltNeighbours = false)
        {
            //Console.Error.WriteLine("start: " + start);
            //Console.Error.WriteLine("end: " + end);

            HashSet<Tile> closedSet = new HashSet<Tile>();
            HashSet<Tile> openSet = new HashSet<Tile>();

            ResetNodes();

            start.gScore = 0;
            start.fScore = start.Distance(end);
            start.parent = null;

            openSet.Add(start);

            Tile current = null;
            int loops = 0;

            //Console.Error.WriteLine("start: " + start);

            while (openSet.Count > 0)
            {
                current = openSet.OrderBy(n => n.fScore).FirstOrDefault();

                //Console.Error.WriteLine(current);

                if (current.pos == end.pos)
                {
                    return new Path(start, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                var neighbourDictionary = useAltNeighbours ? current.neighboursAlt : current.neighbours;
                foreach (var pair in neighbourDictionary)
                {
                    var neighbour = pair.Key;
                    var move = pair.Value;

                    //Console.Error.WriteLine("  " + neighbour);

                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int cost = 1;
                    
                    var alternativeDistance = current.gScore + cost;
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else if (alternativeDistance >= neighbour.gScore)
                    {
                        continue;
                    }

                    neighbour.parent = current;
                    neighbour.gScore = alternativeDistance;
                    neighbour.fScore = alternativeDistance + neighbour.Distance(end);
                    //neighbour.moveType = move;
                }

                loops++;
            }

            //Console.Error.WriteLine("Loops: " + loops);

            return null;
        }

        public static HashSet<Tile> GetWalkableTiles(Tile start, bool useAltNeighbours = false)
        {
            HashSet<Tile> visited = new HashSet<Tile>();

            Queue<Tile> queue = new Queue<Tile>();

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();

                var neighbours = useAltNeighbours ? tile.neighboursAlt.Keys : tile.neighbours.Keys;
                foreach (var neighbour in neighbours)
                {
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return visited;
        }
    }
}
