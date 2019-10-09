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

        public Node parent;

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

    public static void RecalculateScore(Dictionary<int, Node> nodeHash, Dictionary<int, Node> exitNodes)
    {
        foreach (var node in nodeHash.Values)
        {
            node.score = 0;
        }

        foreach (var exitNode in exitNodes.Values)
        {
            var closedSet = new Hashtable();

            exitNode.score = 2;
            AddScore(exitNode, closedSet, 2, 1);
        }

        foreach (var pair in nodeHash)
        {
            var node = pair.Value;
            Console.Error.WriteLine("Node: " + node.index + " - " + node.score);
        }
    }

    /*public static void RecalculateScore2(Dictionary<int, Node> nodeHash, Dictionary<int, Node> exitNodes)
    {
        //BFS start

        Hashtable set = new Hashtable();
        Queue<Node> q = new Queue<Node>();

        List<Node> nextExpand = new List<Node>();

        nodeHash.Values.ForEach(node => node.score = 0);

        foreach (var node in exitNodes.Values)
        {
            node.score = 2;
            q.Enqueue(node);
            set[node.index] = true;
        }

        Node current;

        int steps = 1;

        // bfs loop
        while (q.Count > 0)
        {
            current = q.Dequeue();

            if (current.score == 0)
            {

            }

            foreach (Node node in current.connections)
            {
                if (set[node.index] == null)
                {
                    set[node.index] = true;
                    node.score = current + 1;

                    nextExpand.Add(node);
                }
            }

            if (q.Count == 0 && nextExpand.Count > 0)
            {
                nextExpand.ForEach(n => q.Enqueue(n));
                nextExpand.Clear();
                steps++;
            }
        }

    }*/

    public static string GetConnectionString(Node x, Node y)
    {
        /*int min = Math.Min(x.index, y.index);
        int max = Math.Max(x.index, y.index);

        return min + " " + max;*/

        return x.index + "" + y.index;
    }

    public static void AddScore(Node current, Hashtable closedSet, int score, int add)
    {
        if (score == 0)
        {
            return;
        }
        else
        {
            current.score = Math.Max(0, current.score + add);

            foreach (var neighbour in current.connections)
            {
                string cStr = GetConnectionString(current, neighbour);
                if (closedSet[cStr] == null && !neighbour.isExit)
                {
                    //Console.Error.WriteLine("Node: " + current.index + "-" + neighbour.index +
                    //                       " Score: " + current.score + "-" + neighbour.score);

                    closedSet[cStr] = true;
                    AddScore(neighbour, closedSet, current.score - add, add);
                }
            }
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

    public static void CutConnection(Node exitNode)
    {
        var neighbour = exitNode.parent;

        Console.WriteLine(exitNode.index + " " + neighbour.index);

        //Console.Error.WriteLine(neighbour.index + ": " + neighbour.score);

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

        RecalculateScore(nodeHash, exitNodes);

        // game loop
        while (true)
        {
            int SI = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn

            //BFS start

            Hashtable set = new Hashtable();
            Queue<Node> q = new Queue<Node>();

            List<Node> nextExpand = new List<Node>();
            List<Node> foundExits = new List<Node>();

            Node current = nodeHash[SI];
            q.Enqueue(current);
            current.finalscore = current.score;

            set[current.index] = true;

            int steps = 1;

            // bfs loop
            while (q.Count > 0)
            {
                current = q.Dequeue();

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                if (current.isExit)
                {
                    //Console.Error.WriteLine("Found exit: " + current.index);
                    Console.Error.WriteLine("Found exit: " + current.index);
                    //Console.Error.WriteLine("Node: " + current.parent.index + " - " + current.parent.score);
                    foundExits.Add(current);
                }

                foreach (Node node in current.connections)
                {

                    if (set[node.index] == null)
                    {
                        int finalscore = node.score - steps;
                        //Console.Error.WriteLine(node.index + ": " + node.score + ", " + finalscore);

                        if (foundExits.Count == 0 || finalscore > 0 || node.isExit)
                        {
                            set[node.index] = true;

                            node.parent = current;
                            node.finalscore = finalscore;
                            nextExpand.Add(node);
                            //q.Enqueue(node);
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

            //Got the SI node, now traceback
            //Console.WriteLine(current.index + " " + current.parent.index);

            Node finalNode = foundExits.OrderByDescending(exit => exit.parent.finalscore).FirstOrDefault();
            CutConnection(finalNode);
            RecalculateScore(nodeHash, exitNodes);

        }
    }
}