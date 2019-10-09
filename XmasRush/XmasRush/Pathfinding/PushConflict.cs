using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    class PushConflict
    {
        public static string playerTileStr;
        public static string opponentTileStr;

        public Tile oldTile;
        public int num;
        public Direction pushDirection;
        public List<string> pushData;

        public static bool pushFailed = false;

        public static PushConflict previousPush;

        public PushConflict(int num, Direction pushDirection, Tile tile, List<string> data)
        {
            this.num = num;
            this.pushDirection = pushDirection;

            this.oldTile = tile;

            this.pushData = data;
        }

        public static bool willPushFail(int num, Direction dir)
        {
            if (PushConflict.pushFailed &&
                PushConflict.previousPush.num == num)
            {
                if (PushConflict.previousPush.pushDirection == dir ||
                    PushConflict.previousPush.pushDirection.Reverse() == dir)
                {
                    return true;
                }
            }

            return false;
        }

        public static void StorePushData(int num, Direction pushDirection)
        {
            List<string> pushData = new List<string>();

            var newTile = SimulatePush.GetPushTile(num, pushDirection);

            var originalTile = newTile;
            var nextPos = Tile.GetPos(newTile, pushDirection);

            while (SimulatePush.isInBound(nextPos))
            {
                var nextTile = Tile.GetTile(nextPos);

                pushData.Add(nextTile.directionString);

                originalTile = nextTile;
                nextPos = Tile.GetPos(nextTile, pushDirection);
            }

            previousPush = new PushConflict(num, pushDirection, newTile, pushData);
        }

        public static bool CheckPushRow()
        {
            var num = previousPush.num;
            var pushDirection = previousPush.pushDirection;
            var newTile = previousPush.oldTile;

            var originalTile = newTile;
            var nextPos = Tile.GetPos(newTile, pushDirection);

            int i=0;
            while (SimulatePush.isInBound(nextPos))
            {
                var nextTile = Tile.GetTile(nextPos);

                if (nextTile.directionString != previousPush.pushData[i])
                {
                    return false;
                }

                originalTile = nextTile;
                nextPos = Tile.GetPos(nextTile, pushDirection);
                i++;
            }

            return true;
        }

        public static void CheckPushFailed()
        {
            pushFailed = false;

            if (previousPush != null)
            {
                if (Player.me.tileString == playerTileStr &&
                    Player.enemy.tileString == opponentTileStr)
                {                    
                    if (CheckPushRow())
                    {
                        Console.Error.WriteLine("Push failed");
                        pushFailed = true;
                    }
                }
            }

            playerTileStr = Player.me.tileString;
            opponentTileStr = Player.enemy.tileString;
        }
    }
}
