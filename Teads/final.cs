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
    public class Node
    {
        public int number;
        public int length;
        public int neighbours;
        public List<Node> connections;

        public static Dictionary<int, Node> nodeHash = new Dictionary<int, Node>();
        public static Dictionary<int, Node> addHash = new Dictionary<int,Node>();

        public Node(int number)
        {
            this.number = number;
            this.length = 0;
            this.neighbours = 0;
            this.connections = new List<Node>();
        }
    }

    public static Node AddNodeHash(int number)
    {
        Node node;

        if (Node.nodeHash.TryGetValue(number, out node))
        {
            node.neighbours += 1;

            if(node.neighbours > 1){
                Node.addHash.Remove(number);
            }

            return node;
        }
        else
        {
            var newNode = new Node(number);
            newNode.neighbours = 1;
            Node.nodeHash.Add(number, newNode);
            Node.addHash.Add(number,newNode);

            return newNode;
        }
    }

    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations

        Console.Error.WriteLine(n);
                
        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi

            var xNode = AddNodeHash(xi);
            var yNode = AddNodeHash(yi);

            xNode.connections.Add(yNode);
            yNode.connections.Add(xNode);
            
            //Console.Error.WriteLine(xi + " connects " + yi);
        }

        Node lastNode = null;
        while (Node.addHash.Count() > 0)
        {
            Dictionary<int, Node> addHashNext = new Dictionary<int, Node>();

            foreach (var node in Node.addHash.Values)
            {
                //Console.Error.WriteLine(node.number + ": " + node.neighbours);

                if (node.neighbours == 0)
                {
                    lastNode = node;
                    break;
                }

                Node xNode = node.connections.FirstOrDefault();

                //Console.Error.WriteLine(node.number + " connects " + xNode.number);

                xNode.connections.Remove(node);
                xNode.neighbours -= 1;
                xNode.length = Math.Max(xNode.length, node.length + 1);

                if (xNode.neighbours == 1)
                {
                    addHashNext.Add(xNode.number, xNode);
                }
            }

            Node.addHash = addHashNext;
        }

        Console.WriteLine(lastNode.length);
    }
}