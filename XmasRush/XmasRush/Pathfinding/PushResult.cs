using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    class PushResult
    {
        public int num;
        public Direction direction;

        public Tile lastTile;

        public double pushScore = 0;

        public PushResult(int num, Direction direction, Tile lastTile)
        {
            this.num = num;
            this.direction = direction;

            this.lastTile = lastTile;
        }

        public int GetDistanceToEdge(Tile tile)
        {
            bool isAffected = false;

            if (direction == Direction.Down || direction == Direction.Up)
            {
                if (tile.pos.x == num)
                {
                    isAffected = true;
                }
            }
            else
            {
                if (tile.pos.y == num)
                {
                    isAffected = true;
                }
            }

            var pos = isAffected ? Tile.GetPos(tile, direction) : tile.pos;
                        
            int[] array = { pos.x, 6 - pos.x, pos.y, 6 - pos.y };
            return array.Min();
        }

        public void CalculateScore()
        {
            var playerWalkables = Pathfinding.GetWalkableTiles(Player.me.tileAlt, true);
            pushScore += playerWalkables.Count;

            foreach (var questItem in Player.me.questItems)
            {
                if (questItem.tile == lastTile)
                {
                    pushScore += 25;
                }
                else
                {
                    pushScore += 15.0f/(GetDistanceToEdge(questItem.tile)+1);
                }

                var path = Pathfinding.CalculateShortestPath(Player.me.tileAlt, questItem.tile, true);
                if (path != null)
                {
                    pushScore += 99 * (12.0f / Player.me.numQuests) * (1.0f / path.waypoints.Count);
                }
                else
                {
                    var walkableQuests = Pathfinding.GetWalkableTiles(questItem.tile, true);

                    pushScore += walkableQuests.Count;
                }
            }

            var walkableTiles = Pathfinding.GetWalkableTiles(Player.me.tileAlt, true);
            pushScore += walkableTiles.Count / 5.0f;

            foreach (var questItem in Player.enemy.questItems)
            {
                var path = Pathfinding.CalculateShortestPath(Player.enemy.tileAlt, questItem.tile, true);
                if (path != null)
                {
                    pushScore -= 99 * (12.0f / Player.enemy.numQuests) * (1/5.0f);
                }
            }

            //Console.Error.WriteLine(this);
        }

        public override string ToString()
        {
            return "Push " + num + " " + direction + ": " + pushScore;
        }
    }
}
