using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    class SimulatePush
    {
        public static HashSet<Tile> changedTile = new HashSet<Tile>();

        public static void ResetChangedTiles()
        {
            Player.me.tileAlt = Player.me.tile;
            Player.enemy.tileAlt = Player.enemy.tile;

            foreach (var tile in changedTile)
            {
                tile.neighboursAlt = new Dictionary<Tile, Direction>();
                foreach (var neighbour in tile.neighbours)
                {
                    tile.neighboursAlt.Add(neighbour.Key, neighbour.Value);
                }
            }
        }

        public static Tile InsertTile(int number, Direction pushDirection)
        {
            var newTile = GetPushTile(number, pushDirection);
            
            return InsertTile(newTile, pushDirection);
        }

        public static Tile GetPushTile(int number, Direction pushDirection)
        {
            var tile = Player.me.holdingTile;

            int x = -1;
            int y = -1;

            if (pushDirection == Direction.Down || pushDirection == Direction.Up)
            {
                x = number;

                if (pushDirection == Direction.Up)
                {
                    y = 7;
                }
            }
            else
            {
                y = number;

                if (pushDirection == Direction.Left)
                {
                    x = 7;
                }
            }

            tile.pos = new Position(x, y);
                        
            return tile;
        }

        public static Tile InsertTile(Tile newTile, Direction pushDirection)
        {
            ResetChangedTiles();
            changedTile = new HashSet<Tile>();

            var firstTileToBePushed = Tile.GetTile(newTile, pushDirection);
            if (newTile.directionString[(int)pushDirection] == '1')
            {
                //has connection check the other tile
                AddNeighbour(firstTileToBePushed, pushDirection.Reverse(), newTile);
            }

            var originalTile = newTile;
            var nextPos = Tile.GetPos(newTile, pushDirection);

            while (isInBound(nextPos))
            {
                var nextTile = Tile.GetTile(nextPos);

                //Console.Error.WriteLine(nextPos + " " + pushDirection.ToString());

                if (pushDirection == Direction.Up || pushDirection == Direction.Down)
                {
                    RecheckTile(nextTile, Direction.Left, originalTile);
                    RecheckTile(nextTile, Direction.Right, originalTile);
                }
                else
                {
                    RecheckTile(nextTile, Direction.Up, originalTile);
                    RecheckTile(nextTile, Direction.Down, originalTile);
                }

                originalTile = nextTile;
                nextPos = Tile.GetPos(nextTile, pushDirection);
            }

            var lastTile = originalTile;
            RemoveNeighbour(lastTile, pushDirection);

            if (Player.me.tile == lastTile)
            {
                Player.me.tileAlt = newTile;
            }
            else if (Player.enemy.tile == lastTile)
            {
                Player.enemy.tileAlt = newTile;
            }

            return lastTile;
        }

        public static Tile RemoveNeighbour(Tile tile, Direction dir)
        {
            Tile neighbour;
            if (tile.neighbourByDirection.TryGetValue(dir, out neighbour))
            {
                tile.neighboursAlt.Remove(neighbour);
                neighbour.neighboursAlt.Remove(tile);

                changedTile.Add(tile);
                changedTile.Add(neighbour);

                return neighbour;
            }

            return null;
        }

        public static void AddNeighbour(Tile newTile, Direction checkDir, Tile neighbour)
        {
            if (newTile.directionString[(int)checkDir] == '1')
            {
                newTile.neighboursAlt.Add(neighbour, checkDir);
                neighbour.neighboursAlt.Add(newTile, checkDir.Reverse());

                changedTile.Add(newTile);
                changedTile.Add(neighbour);
            }
        }

        public static void CheckAddNeighbour(Tile origin, Direction checkDir, Tile newTile)
        {
            if (newTile.directionString[(int)checkDir] == '1')
            {
                var neighbour = Tile.GetTile(origin, checkDir);

                if (neighbour != null && neighbour.directionString[(int)checkDir.Reverse()] == '1') //both have paths
                {
                    newTile.neighboursAlt.Add(neighbour, checkDir);
                    neighbour.neighboursAlt.Add(newTile, checkDir.Reverse());

                    changedTile.Add(newTile);
                    changedTile.Add(neighbour);
                }
            }
        }

        public static void RecheckTile(Tile origin, Direction checkDir, Tile newTile)
        {
            var neighbour = RemoveNeighbour(origin, checkDir);
            if (neighbour != null) //link back the old neighbour
            {
                AddNeighbour(newTile, checkDir, neighbour);
            }
            else
            {
                CheckAddNeighbour(origin, checkDir, newTile);
            }
        }

        public static bool isInBound(Position pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x <= 6 && pos.y <= 6;
        }
    }
}
