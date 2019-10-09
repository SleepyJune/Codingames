using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonPanic2
{
    public enum MoveType
    {
        Elevator,
        Wait,
        Block,
    }

    class Node : IEquatable<Node>
    {
        public Node parent;
        public double gScore = 9999;
        public double fScore = 9999;

        public Dictionary<Node, MoveType> neighbours = new Dictionary<Node, MoveType>();

        public Node next;
        public bool isExecuted = false;

        public int floor;
        public int pos;

        public int direction;
        public bool isElevator;

        public MoveType moveType;

        public int elevatorsLeft;

        public Node(int floor, int pos, int direction, int elevatorsLeft)
        {
            this.floor = floor;
            this.pos = pos;
            this.direction = direction;
            this.elevatorsLeft = elevatorsLeft;
        }

        public void Print()
        {
            Console.Error.WriteLine("Floor: " + floor + " Pos: " + pos + " Dir: " + direction + " Elevators: " + elevatorsLeft + " - " + moveType.ToString());
        }

        public static Node GetNode(int floor, int pos, int direction, int elevatorsLeft)
        {
            Node testNode = new Node(floor, pos, direction, elevatorsLeft);
            Node realNode;

            if (Pathfinding.nodeTable.TryGetValue(testNode, out realNode))
            {
                return realNode;
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node)
            {
                return Equals((Node)this);
            }

            return false;
        }

        public bool Equals(Node node)
        {
            return
                    this.floor == node.floor
                 && this.pos == node.pos
                 && this.direction == node.direction
                 && this.elevatorsLeft == node.elevatorsLeft;
        }

        public override int GetHashCode()
        {
            return this.floor * 100 * 10 * 10 +
                   this.pos * 10 * 10 +
                   this.direction * 10 +
                   this.elevatorsLeft;
        }
    }
}
