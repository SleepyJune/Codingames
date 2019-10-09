using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left,
    }

    class Tile
    {
        public static Dictionary<Position, Tile> tiles = new Dictionary<Position, Tile>();
       
        public Position pos;

        public string directionString;

        public Dictionary<Tile, Direction> neighbours = new Dictionary<Tile, Direction>();
        public Dictionary<Tile, Direction> neighboursAlt = new Dictionary<Tile, Direction>();
        public Dictionary<Direction, Tile> neighbourByDirection = new Dictionary<Direction, Tile>();

        public Tile parent;
        public double gScore = 9999;
        public double fScore = 9999;

        public double questScore = 0;
        public double walkScore = 0;
        public double potentialQuestScore = 0;

        public Tile(int x, int y, string directions)
        {
            pos = new Position(x, y);
            directionString = directions;
        }
        
        public static void InitializeNeighbours()
        {
            foreach (var tile in tiles.Values)
            {
                //Console.Error.WriteLine(tile + tile.directionString);

                foreach (var dir in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {                    
                    if (tile.directionString[(int)dir] == '0')
                    {
                        continue;
                    }

                    var pos = GetPos(tile, dir);
                    
                    var neighbour = GetTile(pos);
                    if (neighbour != null)
                    {
                        if (neighbour.directionString[(int)dir.Reverse()] == '1') //check if neighbour connects too
                        {
                            //Console.Error.WriteLine(" " + neighbour + neighbour.directionString);
                            tile.neighbours.Add(neighbour, dir);
                            tile.neighboursAlt.Add(neighbour, dir);

                            tile.neighbourByDirection.Add(dir, neighbour);
                        }
                    }
                }
            }
        }

        public static void AddTile(Tile newTile)
        {
            tiles.Add(newTile.pos, newTile);
        }

        public static Position GetPos(Tile tile, Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    return new Position(tile.pos.x, tile.pos.y - 1);
                case Direction.Down:
                    return new Position(tile.pos.x, tile.pos.y + 1);
                case Direction.Left:
                    return new Position(tile.pos.x - 1, tile.pos.y);
                case Direction.Right:
                    return new Position(tile.pos.x + 1, tile.pos.y);
            }

            return new Position(-1, -1);
        }

        public static Tile GetTile(Tile tile, Direction dir)
        {
            var pos = GetPos(tile, dir);

            return GetTile(pos);
        }

        public static Tile GetTile(int x, int y)
        {
            return GetTile(new Position(x, y));
        }

        public static Tile GetTile(Position pos)
        {
            Tile tile;
            if (tiles.TryGetValue(pos, out tile))
            {
                return tile;
            }

            return null;
        }

        public double Distance(Tile tile)
        {
            return (float)Math.Sqrt(
              Math.Pow(pos.x - tile.pos.x, 2)
            + Math.Pow(pos.y - tile.pos.y, 2));
        }

        public override bool Equals(object obj)
        {
            if (obj is Tile)
            {
                return Equals((Tile)this);
            }

            return false;
        }

        public bool Equals(Tile other)
        {
            return this.pos == other.pos;
        }

        public override int GetHashCode()
        {
            return this.pos.GetHashCode();
        }

        public override string ToString()
        {
            return pos.ToString();
        }

        public static void CleanUp()
        {
            tiles = new Dictionary<Position, Tile>();
        }

        public void PrintNeighbours()
        {
            //foreach(var neigh)
        }
    }
}
