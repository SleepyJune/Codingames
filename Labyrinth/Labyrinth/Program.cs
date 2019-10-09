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
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int R = int.Parse(inputs[0]); // number of rows.
        int C = int.Parse(inputs[1]); // number of columns.
        int A = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

        var map = new char[C, R];

        var steps = 0;

        // game loop
        while (true)
        {
            if (steps > 300)
            {
                break;
            }

            inputs = Console.ReadLine().Split(' ');
            int KR = int.Parse(inputs[0]); // row where Kirk is located.
            int KC = int.Parse(inputs[1]); // column where Kirk is located.
            for (int i = 0; i < R; i++)
            {
                string ROW = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).

                //Console.Error.WriteLine(ROW);

                for (int j = 0; j < C; j++)
                {
                    map[j, i] = ROW[j];
                }
            }

            Game.map = map;

            var startNode = new Node(KC, KR);

            Game.currentNode = startNode;
            Node nextNode = null;

            var qResult = BFS.useBFS(map, startNode, '?');

            if (qResult == null) //no more ? to explore
            {
                Game.exploring = false;
            }
            else
            {
                nextNode = qResult.nextStep; //exploring phase
                Console.Error.WriteLine("Exploring phase");
            }

            if (Game.exploring == false)
            {
                var cResult = BFS.useBFS(map, startNode, 'C');
                nextNode = cResult.nextStep; //getting to control room phase
                Console.Error.WriteLine("Control room phase");
            }

            if (Game.deactivated)
            {
                var tResult = BFS.useBFS(map, startNode, 'T');
                nextNode = tResult.nextStep; //returning phase
                Console.Error.WriteLine("Retruning phase");
            }

            Game.GoToNode(nextNode);
        }
    }
}

public class Game
{
    public static int steps = 0;
    public static Node currentNode = null;

    public static bool deactivated = false;
    public static bool exploring = true;

    public static char[,] map;

    public static void GoToNode(Node nextNode)
    {
        string nextStep = "";

        if (Math.Abs(nextNode.x - currentNode.x) > 0)
        {
            nextStep = (nextNode.x - currentNode.x > 0) ? "RIGHT" : "LEFT";
        }

        if (Math.Abs(nextNode.y - currentNode.y) > 0)
        {
            nextStep = (nextNode.y - currentNode.y > 0) ? "DOWN" : "UP";
        }

        Console.Error.WriteLine("Next step: " + nextNode.x + ", " + nextNode.y);
        Console.WriteLine(nextStep); // Kirk's next move (UP DOWN LEFT or RIGHT).

        var nextSquare = map[nextNode.x, nextNode.y];
        if (nextSquare == 'C')
        {
            deactivated = true;
            exploring = false;
        }

        steps++;
    }
}

public class BFSResult
{
    public Node goal;
    public Node nextStep;
    public int steps;

    public BFSResult(Node goal, Node nextStep, int steps)
    {
        this.goal = goal;
        this.nextStep = nextStep;
        this.steps = steps;
    }
}

public class Node
{
    public int x;
    public int y;
    public Node parent;

    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(Object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Node n = (Node)obj;
        return (x == n.x) && (y == n.y);
    }

    public override int GetHashCode()
    {
        return x ^ y;
    }
}

class BFS
{

    public static BFSResult useBFS(char[,] map, Node startNode, char goal)
    {
        //BFS start

        Hashtable set = new Hashtable();
        Queue<Node> q = new Queue<Node>();

        q.Enqueue(startNode); //queue the current position

        Node current = null;

        // bfs loop
        while (q.Count() > 0)
        {
            current = q.Dequeue();

            if (map[current.x, current.y] == goal)
            {
                Console.Error.WriteLine("Found " + goal + " at " + current.x + ", " + current.y);
                break;
            }

            //Console.Error.WriteLine("Current at " + current.x + ", " + current.y);

            var neighbours = new List<Node>(); //grab the neighbours
            for (int i = 0; i < 4; i++)
            {
                int x = current.x;
                int y = current.y;
                switch (i)
                {
                    case 0:
                        x = x + 1;
                        break;
                    case 1:
                        x = x - 1;
                        break;
                    case 2:
                        y = y + 1;
                        break;
                    case 3:
                        y = y - 1;
                        break;
                }

                var square = map[x, y];

                if (square != '#')
                {
                    neighbours.Add(new Node(x, y));
                }
            }

            foreach (Node node in neighbours)
            {
                //Console.Error.WriteLine(node.index);

                if (set[node.x + " " + node.y] == null)
                {
                    set[node.x + " " + node.y] = true;

                    node.parent = current;
                    q.Enqueue(node);
                }
            }
        }

        if (map[current.x, current.y] != goal)
        {
            return null;
        }

        //Found goal, now traceback
        int numSteps = 0;
        var trace = current;
        while (trace.parent != null && trace.parent != startNode)
        {
            trace = trace.parent;
            numSteps++;
        }

        return new BFSResult(current, trace, numSteps);
    }
}