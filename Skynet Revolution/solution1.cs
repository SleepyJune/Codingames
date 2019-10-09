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
        public List<Node> connections;

        public int index;
        public int steps;
        public bool isExit;

        public int score;
        public int finalscore;

        public int connectionsToGateway;

        public Node(int index)
        {
            this.index = index;
            this.connections = new List<Node>();
            this.steps = 0;
            this.isExit = false;

            this.connectionsToGateway = 0;

            this.score = 0;
            this.finalscore = 0;
        }

        public void AddConnection(Node node)
        {
            connections.Add(node);
        }
    }

    public static Node AddNodeHash(Dictionary<int, Node> hash, int number)
    {
        Node node;

        if (hash.TryGetValue(number, out node))
        {
            return node;
        }
        else
        {
            var newNode = new Node(number);
            hash.Add(number, newNode);
            return newNode;
        }
    }

    public static void CutConnection(Node exitNode, Node neighbour)
    {
        Console.WriteLine(exitNode.index + " " + neighbour.index);

        exitNode.connections.Remove(neighbour);
        neighbour.connections.Remove(exitNode);
        neighbour.connectionsToGateway--;
    }

    static void Main(string[] args)
    {
        Console.SetError(TextWriter.Null); //block all error messages from outputing

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
        int L = int.Parse(inputs[1]); // the number of links
        int E = int.Parse(inputs[2]); // the number of exit gateways

        Dictionary<int, Node> nodeHash = new Dictionary<int, Node>();

        for (int i = 0; i < L; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int N1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
            int N2 = int.Parse(inputs[1]);

            //Console.Error.WriteLine("Connection: " + N1 + "-" + N2);

            var xNode = AddNodeHash(nodeHash, N1);
            var yNode = AddNodeHash(nodeHash, N2);

            xNode.AddConnection(yNode);
            yNode.AddConnection(xNode);
        }


        var exitNodes = new Dictionary<int, Node>();

        for (int i = 0; i < E; i++)
        {
            int EI = int.Parse(Console.ReadLine()); // the index of a gateway node


            var exitNode = nodeHash[EI];
            exitNode.isExit = true;
            exitNodes.Add(EI, exitNode);

            foreach (Node neighbour in exitNode.connections)
            {
                neighbour.connectionsToGateway++;
            }

            Console.Error.WriteLine("Exit Gateway: " + EI);
        }
        
        // game loop
        while (true)
        {
            int SI = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn

            //BFS start

            Hashtable set = new Hashtable();
            Queue<Node> q = new Queue<Node>();

            List<Node> nextExpand = new List<Node>();
            List<Tuple<Node, Node>> foundExits = new List<Tuple<Node, Node>>();

            Node current = nodeHash[SI];
            q.Enqueue(current);

            set[current.index] = true;

            int steps = 1;

            current.score = 0;

            // bfs loop
            while (q.Count > 0)
            {
                current = q.Dequeue();
                
                foreach (Node node in current.connections)
                {

                    if (set[node.index] == null)
                    {
                        if (node.isExit)
                        {
                            Console.Error.WriteLine("Found exit: " + node.index + " steps: " + (steps+1));

                            node.steps = steps+1;
                            foundExits.Add(new Tuple<Node, Node>(node, current));
                        }
                        else if(foundExits.Count == 0 || node.connectionsToGateway > 0)
                        {
                            set[node.index] = true;

                            node.score = current.score + (node.connectionsToGateway > 0 ? 0 : 1);
                            node.finalscore = node.score - node.connectionsToGateway;
                            nextExpand.Add(node);

                            Console.Error.WriteLine(node.index + ": " + node.connectionsToGateway);
                        }
                    }
                }

                if (q.Count == 0 && nextExpand.Count > 0)
                {
                    nextExpand.ForEach(n => q.Enqueue(n));
                    nextExpand.Clear();
                    steps++;
                }
            }

            var sortedExits = foundExits
                .OrderByDescending(connection => (connection.Item2.index == SI) ? 999 : 0)
                .ThenByDescending(connection => connection.Item2.connectionsToGateway)
                .ThenBy(connection => connection.Item2.score);

            Tuple<Node, Node> finalConnection = sortedExits.FirstOrDefault();
            CutConnection(finalConnection.Item1, finalConnection.Item2);
        }
    }
}