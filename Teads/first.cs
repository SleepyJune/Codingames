using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    public class Node
    {
        public List<Node> connections;
        public int number;
        public int length;

        public Node(int number)
        {
            this.number = number;
            this.connections = new List<Node>();
            this.length = 0;
        }

        public void AddConnection(Node node)
        {
            connections.Add(node);
            //length = Math.Max(length,node.GetSteps(this));

            //Console.Error.WriteLine(number + ":" + length);

            //UpdateLength(this);

        }

        public void UpdateLength(Node fromNode)
        {
            foreach (var n in connections)
            {
                if(fromNode.number != n.number)
                    n.UpdateLength(this);
            }
            length = GetSteps(this) - 1;

            //Console.Error.WriteLine(number + ":" + length);
        }

        public int GetSteps(Node fromNode)
        {
            if (connections.Count() == 0)
            {
                return 1;
            }

            //Console.Error.WriteLine(connections.Max(n => fromNode.number != this.number ? GetSteps(steps, this) : 0));

            return connections.Max(n => fromNode.number != n.number? n.GetSteps(this):0) + 1;
        }
    }

    public static Node AddNodeHash(Hashtable hash, int number){
        if (hash[number] == null)
        {
            var newNode = new Node(number);
            hash[number] = newNode;
            return newNode;
        }
        else
        {
            return (Node)hash[number];
        }
    }

    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations

        Console.Error.WriteLine(n);

        var nodeHash = new Hashtable();
        Node randomNode = null;
        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            var xNode = AddNodeHash(nodeHash, xi);
            var yNode = AddNodeHash(nodeHash, yi);

            xNode.AddConnection(yNode);
            yNode.AddConnection(xNode);

            randomNode = xNode;
        }

        randomNode.UpdateLength(randomNode);

        int min = int.MaxValue;
        foreach (DictionaryEntry obj in nodeHash)
        {
            var node = (Node)obj.Value;
            min = Math.Min(min, node.length);
        }

        Console.WriteLine(min);
    }
}