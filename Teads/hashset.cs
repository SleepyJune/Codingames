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
    public static Dictionary<int, HashSet<int>> nodes = new Dictionary<int, HashSet<int>>();
    public static Dictionary<int, HashSet<int>> loneNodes = new Dictionary<int, HashSet<int>>();

    public static HashSet<int> AddNode(int number)
    {
        HashSet<int> node;

        if (nodes.TryGetValue(number, out node))
        {
            loneNodes.Remove(number); //can't possibly be a lone node
            return node;
        }
        else
        {
            var newNode = new HashSet<int>();
            nodes.Add(number, newNode);
            loneNodes.Add(number, newNode); //assuming it's a lone node

            return newNode;
        }
    }

    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations

        Console.Error.WriteLine("Number of nodes: " + n);

        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            var xNode = AddNode(xi);
            var yNode = AddNode(yi);

            xNode.Add(yi); //x add connection to y
            yNode.Add(xi); //y add connection to x

            //Console.Error.WriteLine(xi + " connects " + yi);
        }

        int length = 0;
        while (loneNodes.Count > 1)
        {
            Dictionary<int, HashSet<int>> loneNodesNext = new Dictionary<int, HashSet<int>>();

            foreach (var pair in loneNodes)
            {
                int id = pair.Key;
                HashSet<int> node = pair.Value;

                //Console.Error.WriteLine(id + ": " + string.Join(" ", node));               

                int neighbourID = node.FirstOrDefault();
                HashSet<int> neighbourConnections = nodes[neighbourID];

                neighbourConnections.Remove(id);

                if (neighbourConnections.Count == 1) //it's a lone node
                {
                    //Console.Error.WriteLine("Add: " + xNodeId);
                    loneNodesNext.Add(neighbourID, neighbourConnections);
                }
            }

            loneNodes = loneNodesNext;
            length++;

            //Console.Error.WriteLine("Depth + 1");
        }

        Console.WriteLine(length);
    }
}