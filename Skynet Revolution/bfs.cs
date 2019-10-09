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
class Player
{
    public class Node
    {
        public int index;
        public Node parent;

        public Node(int index)
        {
            this.index = index;
        }
    }

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
        int L = int.Parse(inputs[1]); // the number of links
        int E = int.Parse(inputs[2]); // the number of exit gateways

        //Console.Error.WriteLine("Exit Gateway: " + N);

        Dictionary<int, List<Node>> connections = new Dictionary<int, List<Node>>();

        for (int i = 0; i < L; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int N1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
            int N2 = int.Parse(inputs[1]);

            //Console.Error.WriteLine("Connection: " + N1 + "-" + N2);

            List<Node> nList1;
            List<Node> nList2;
            if (connections.TryGetValue(N1, out nList1))
            {
                nList1.Add(new Node(N2));
            }
            else
            {
                connections.Add(N1, new List<Node>(new Node[] { new Node(N2) }));
            }

            if (connections.TryGetValue(N2, out nList2))
            {
                nList2.Add(new Node(N1));
            }
            else
            {
                connections.Add(N2, new List<Node>(new Node[] { new Node(N1) }));
            }
        }

        Hashtable exitNodes = new Hashtable();

        for (int i = 0; i < E; i++)
        {
            int EI = int.Parse(Console.ReadLine()); // the index of a gateway node
            exitNodes[EI] = true;

            Console.Error.WriteLine("Exit Gateway: " + EI);
        }

        // game loop
        while (true)
        {
            int SI = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn


            //BFS start

            Hashtable set = new Hashtable();
            Queue<Node> q = new Queue<Node>();

            q.Enqueue(new Node(SI));

            Node current;

            // bfs loop
            while (true)
            {
                current = q.Dequeue();

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                if (exitNodes[current.index] != null)
                {
                    Console.Error.WriteLine("Found exit: " + current.index);
                    break;
                }

                var nodes = connections[current.index];
                foreach (Node node in nodes)
                {
                    //Console.Error.WriteLine(node.index);

                    if (set[node.index] == null)
                    {
                        set[node.index] = true;


                        node.parent = current;
                        q.Enqueue(node);
                    }
                }


            }

            //Got the SI node, now traceback
            Console.WriteLine(current.index + " " + current.parent.index);


            // Example: 0 1 are the indices of the nodes you wish to sever the link between

        }
    }
}