using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class Algorithm
    {
        public static void CalculateShortestPaths(Factory start)
        {
            Dictionary<Factory, int> dist = new Dictionary<Factory, int>();
            Dictionary<Factory, Factory> prev = new Dictionary<Factory, Factory>();

            HashSet<Factory> q = new HashSet<Factory>();

            foreach (var factory in Factory.factories) //initialize
            {
                dist.Add(factory, 999);
                prev.Add(factory, null);

                q.Add(factory);
            }

            dist[start] = 0;

            while (q.Count > 0)
            {
                Factory factory = q.OrderBy(f => dist[f]).First();
                q.Remove(factory);

                foreach (var pair in factory.links)
                {
                    Factory neighbour = pair.Key;
                    int distance = pair.Value;

                    int alternativeDistance = dist[factory] + distance + 1;

                    if (alternativeDistance < dist[neighbour])
                    {
                        dist[neighbour] = alternativeDistance;
                        prev[neighbour] = factory;
                    }
                }
            }

            start.shortestPaths = new PathInfo(start, dist, prev);
        }

        public static void CalculateOptimalPaths(Factory start)
        {
            Dictionary<Factory, int> dist = new Dictionary<Factory, int>();
            Dictionary<Factory, int> inter = new Dictionary<Factory, int>();
            Dictionary<Factory, Factory> prev = new Dictionary<Factory, Factory>();

            HashSet<Factory> q = new HashSet<Factory>();

            foreach (var factory in Factory.factories) //initialize
            {
                inter.Add(factory, 0);
                dist.Add(factory, 999);
                prev.Add(factory, null);

                q.Add(factory);
            }

            dist[start] = 0;
            
            while (q.Count > 0)
            {
                Factory factory = q.OrderBy(f => dist[f]).First();
                q.Remove(factory);

                foreach (var pair in factory.links)
                {
                    Factory neighbour = pair.Key;
                    int distance = pair.Value;

                    int alternativeDistance = dist[factory] + distance;
                    int alternativeInter = inter[factory] + 1;

                    if (alternativeDistance <= dist[neighbour] && alternativeInter >= inter[neighbour])
                    {
                        inter[neighbour] = alternativeInter;
                        dist[neighbour] = alternativeDistance;
                        prev[neighbour] = factory;
                    }
                }
            }

            start.optimalPaths = new PathInfo(start, dist, prev);
        }

        public static void CalculateFactoryValue(Factory start)
        {
            foreach (var factory in Factory.factories)
            {
                Factory end = factory;
                
                var prevFactory = start.optimalPaths.previousFactory;
                Factory parent = prevFactory[end];
                                

                while (parent != start && prevFactory[end] != null)
                {                    
                    end.pathValue++;
                    end = parent;
                    parent = prevFactory[end];
                }
                end.pathValue++;
            }
        }

        public static Troop UseBFS(Troop troop, Factory end, bool getDirectPath = false)
        {
            Factory start = troop.start;

            foreach (var factory in Factory.factories) //initialize
            {
                factory.shortestDistance = 999;
            }

            HashSet<int> set = new HashSet<int>();
            Queue<Factory> q = new Queue<Factory>();

            q.Enqueue(start);
            start.shortestDistance = 0;

            Factory current;

            // bfs loop
            while (q.Count > 0)
            {
                current = q.Dequeue();

                foreach (var pair in current.links.OrderBy(p => p.Value))
                {
                    Factory node = pair.Key;
                    int distance = pair.Value;

                    int shortest = Math.Max(current.shortestDistance, distance);

                    if (node == end) //found goal
                    {
                        if (end.shortestDistance > shortest)
                        {

                            end.shortestDistance = shortest;
                            end.parent = current;
                        }
                    }
                    else if (!set.Contains(node.id))
                    {
                        set.Add(node.id);

                        node.parent = current;
                        node.shortestDistance = shortest;

                        //Console.Error.WriteLine(current.id+"-"+node.id + ": " + node.shortestDistance);

                        q.Enqueue(node);
                    }
                    else if (node.shortestDistance > shortest)
                    {
                        node.parent = current;
                        node.shortestDistance = shortest;
                    }
                }
            }

            Factory n = end;
            int totalDistance = 0;

            while (n.parent != start)
            {
                totalDistance += n.links[n.parent] + 1;
                n = n.parent;
            }
            totalDistance += n.links[n.parent] + 1;

            int directDistance = 0;
            if (start.links.TryGetValue(end, out directDistance))
            {
                if (directDistance * 1.5 <= totalDistance || getDirectPath)
                {
                    troop.turns = directDistance + 1;
                    troop.endTime = Game.gameTime + troop.turns;
                    troop.end = end;

                    return troop;
                }
            }

            troop.turns = totalDistance + 1;
            troop.endTime = Game.gameTime + troop.turns;
            troop.end = n;

            return troop;
        }
    }

}
