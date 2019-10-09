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
        public Node parent;
        public string number;
        public List<Node> children;

        public Node(string number)
        {
            this.number = number;
            children = new List<Node>();
        }

        public static int CountChildren(Node node)
        {
            return node.children.Select(n => Node.CountChildren(n)).Sum() + 1;
        }

        public static void PrintChildren(Node node)
        {
            Console.Error.WriteLine(PrintChildren_Helper(node));
        }

        public static string PrintChildren_Helper(Node node)
        {
            return node.number + " " + string.Join("\n", node.children.Select(n => Node.PrintChildren_Helper(n)));
        }
    }

    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());

        Node memory = new Node("Start");

        for (int i = 0; i < N; i++)
        {
            string telephone = Console.ReadLine();

            var prev = memory;
            foreach (var c in telephone)
            {
                string number = "" + c;

                //if (prev.number != number)
                Node child = prev.children.FirstOrDefault(n => n.number == number);
                if (child != null)
                {
                    prev = child;
                }
                else
                {
                    Node newNumber = new Node(number);
                    prev.children.Add(newNumber);
                    prev = newNumber;
                }
            }
        }

        Node.PrintChildren(memory);

        Console.WriteLine(Node.CountChildren(memory) - 1);
    }
}