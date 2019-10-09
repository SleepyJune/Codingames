using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    class PositionNode : IEquatable<PositionNode>
    {
        public Vector pos;
        public Hex hexPos;
        public int rotation;
        public int prevRotation;
        public int speed;
        
        public int step = 0;
        //public int distance = 999;

        public MoveType moveType;

        public List<Hex> checkList;
        public Dictionary<int, Hex> extraCheckList = new Dictionary<int, Hex>();

        public Dictionary<PositionNode, MoveType> neighbours = new Dictionary<PositionNode, MoveType>();

        public PositionNode parent;
        public double gScore = 9999;
        public double fScore = 9999;

        public PositionNode(Vector pos, int rotation, int speed)
        {
            this.pos = pos;
            this.rotation = rotation;
            this.speed = speed;

            this.hexPos = pos.ConvertHex();
            //this.prevRotation = prevRotation;

            this.checkList = new List<Hex>
            {
                (this.pos + Vector.directions[this.rotation]).ConvertHex(),
                (this.pos).ConvertHex(),
                (this.pos - Vector.directions[this.rotation]).ConvertHex(),
            };

            for (int i = 0; i < 6; i++)
            {
                Hex check = (this.pos + Vector.directions[i]).ConvertHex();
                extraCheckList.Add(i, check);                
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is PositionNode)
            {
                return Equals((PositionNode)this);
            }

            return false;
        }

        public bool Equals(PositionNode node)
        {
            return 
                    this.pos == node.pos
                 && this.rotation == node.rotation
                 && this.speed == node.speed;
        }

        public override int GetHashCode()
        {
            return this.speed    * 100 * 100 * 10 + 
                   this.rotation * 100 * 100 + 
                   this.pos.GetHashCode();
        }

        public static PositionNode GetPositionNode(Vector pos, int rotation, int speed)
        {
            if (pos.isInBound())
            {
                PositionNode node = new PositionNode(pos, rotation, speed);
                return Pathfinding.nodeTable[node];
            }
            else
            {
                return null;
            }
        }
    }
}
