using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    public class Node
    {
        public List<Node> connections;

        public int index;
        public bool isExit;

        public int freeMoves;
        public int finalscore;

        public int connectionsToGateway;

        public Node(int index)
        {
            this.index = index;
            this.connections = new List<Node>();
            this.isExit = false;

            this.connectionsToGateway = 0;

            this.freeMoves = 0;
            this.finalscore = 0;
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

            var xNode = AddNodeHash(nodeHash, N1);
            var yNode = AddNodeHash(nodeHash, N2);

            xNode.connections.Add(yNode);
            yNode.connections.Add(xNode);
        }

        for (int i = 0; i < E; i++)
        {
            int EI = int.Parse(Console.ReadLine()); // the index of a gateway node

            var exitNode = nodeHash[EI];
            exitNode.isExit = true;

            foreach (Node neighbour in exitNode.connections)
            {
                neighbour.connectionsToGateway++;
            }
        }

        // game loop
        while (true)
        {
            // The index of the node on which the Skynet agent is positioned this turn
            int SI = int.Parse(Console.ReadLine());

            Hashtable set = new Hashtable();
            Queue<Node> q = new Queue<Node>();

            Node current = nodeHash[SI];
            current.freeMoves = 0;

            q.Enqueue(current);
            set[current.index] = true;

            int bestScore = int.MinValue;
            Tuple<Node, Node> bestConnection = null;

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
                            if (current.finalscore > bestScore)
                            {
                                bestScore = current.finalscore;
                                bestConnection = new Tuple<Node, Node>(node, current);
                            }
                        }
                        else if (bestConnection == null || node.connectionsToGateway > 0)
                        {
                            set[node.index] = true;

                            node.freeMoves = current.freeMoves + (node.connectionsToGateway > 0 ? 0 : 1);
                            node.finalscore = node.connectionsToGateway - node.freeMoves;
                            q.Enqueue(node);
                        }
                    }
                }
            }

            CutConnection(bestConnection.Item1, bestConnection.Item2);
        }
    }
}