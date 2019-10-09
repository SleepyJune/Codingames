using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    public static void AddConnection(Dictionary<int, List<int>> connections, int x, int y)
    {
        List<int> xConnections;

        if (connections.TryGetValue(x, out xConnections))
        {
            xConnections.Add(y);
        }
        else
        {
            xConnections = new List<int>();
            xConnections.Add(y);

            connections.Add(x, xConnections);
        }
    }

    static void Main(string[] args)
    {
        Console.SetError(TextWriter.Null);

        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations

        Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();

        List<int> points = new List<int>();

        //Console.Write(n);
        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            Console.Error.WriteLine("Number: " + xi + " Number2: " + yi);

            AddConnection(connections, xi, yi);
            AddConnection(connections, yi, xi);

            points.Add(i);
        }
        points.Add(n);

        int min = int.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            int fp = points[i];
            int count = Q(fp, connections); //get the max distance through bfs

            Console.Error.WriteLine("Number: " + fp + " Max: " + count);

            min = Math.Min(min, count); //get the min value
        }

        Console.Write(min);
    }

    public static int Q(int fp, Dictionary<int, List<int>> connections)
    {
        int parent = 0;
        int child = 0;
        int count = 0;

        Hashtable set = new Hashtable();
        set[fp] = true; //add first number in set as already visited

        Queue<int> Q = new Queue<int>();

        Q.Enqueue(fp);

        int step = 0;
        int lastNeighbour = fp;
        int lastLock = fp;

        while (Q.Count != 0)
        {
            int v = Q.Dequeue();
            
            List<int> neighbours;
            if (connections.TryGetValue(v, out neighbours)) //get all neighbours of v
            {
                foreach (int neighbour in neighbours)
                {
                    if (set[neighbour] == null) //if neighbour is not in set
                    {
                        set[neighbour] = true; //add neighbour to set

                        Q.Enqueue(neighbour); //add neighbour into queue

                        lastNeighbour = neighbour;

                        Console.Error.WriteLine(neighbour);
                    }
                }
            }

            if (v == lastLock)
            {
                Console.Error.WriteLine(v + " step+1");

                step++; //add 1 more step to count the distance

                lastLock = lastNeighbour;
            }

        }

        return step - 1;
    }
}