using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    public static void AddConnection(Dictionary<int, List<int>> connections, int x, int y)
    {
        List<int> xConnections;

        if (connections.TryGetValue(x, out xConnections)) //check if the number is in dictionary
        {
            xConnections.Add(y);
        }
        else
        {
            xConnections = new List<int>(); //if not, make new list
            xConnections.Add(y);

            connections.Add(x, xConnections); //add number into dictionary
        }
    }

    static void Main(string[] args)
    {
        Console.SetError(TextWriter.Null); //block all error messages from outputing

        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations

        Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();

        List<int> points = new List<int>();

        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            Console.Error.WriteLine("Number: " + xi + " Number2: " + yi);

            AddConnection(connections, xi, yi); //add y as a connection to x
            AddConnection(connections, yi, xi); //add x as a connection to y

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
        Hashtable set = new Hashtable();
        set[fp] = true; //add first number in set as already visited

        Queue<int> Q = new Queue<int>();
        List<int> nextExpand = new List<int>();

        Q.Enqueue(fp);

        int step = 0;

        while (Q.Count != 0)
        {
            int v = Q.Dequeue();

            List<int> neighbours;
            if (connections.TryGetValue(v, out neighbours)) //get all neighbours of v, using the dictionary
            {
                foreach (int neighbour in neighbours)
                {
                    if (set[neighbour] == null) //if neighbour is not in set
                    {
                        set[neighbour] = true; //add neighbour to set as already visited

                        nextExpand.Add(neighbour); //add neighbour into next expand queue

                        Console.Error.WriteLine(neighbour);
                    }
                }
            }

            if (Q.Count == 0 && nextExpand.Count > 0) //if bfs is done expanding current step and next expand has items
            {
                nextExpand.ForEach(n => Q.Enqueue(n)); //put all list items into queue
                nextExpand.Clear();

                Console.Error.WriteLine("step+1");

                step++; //add 1 more step to count the distance
            }

        }

        return step;
    }
}