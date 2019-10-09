using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    public enum TurnType
    {
        Push,
        Move,
    }

    class Strategy
    {
        public static TurnType turnType = TurnType.Push;

        public static MultiGoalPath mainPath;

        public static bool finishedAQuest = false;

        public static void Initialize()
        {
            Pathfinding.Initialize();
            mainPath = new MultiGoalPath();
            finishedAQuest = false;

            Player.me.Initialize();
            Player.enemy.Initialize();
        }

        public static void MakeMove()
        {
            Initialize();

            if (turnType == TurnType.Move)
            {
                WalkStrategy();
            }
            else
            {
                PushStrategy();
            }
        }

        public static void WalkToQuests()
        {
            foreach (var questItem in Player.me.questItems.OrderBy(item => item.tile.Distance(Player.me.tile)))
            {
                if (questItem.isOnHand)
                {
                    continue;
                }

                Console.Error.WriteLine(questItem.tile.pos);

                var path = Pathfinding.CalculateShortestPath(mainPath.GetLast(), questItem.tile);
                if (path != null)
                {
                    //path.PrintPath();
                    //return;

                    mainPath.AddPath(path);
                    questItem.isTaken = true;
                    finishedAQuest = true;
                }
            }
        }

        public static void WalkStrategy()
        {
            WalkToQuests();

            if (finishedAQuest)
            {
                foreach (var item in Item.items.Values)
                {
                    if (item.team == Team.Player && !item.isTaken)
                    {
                        item.tile.potentialQuestScore += 1;

                        //varies depend on how many finished quests this turn
                    }
                }
            }

            var walkableTiles = Pathfinding.GetWalkableTiles(Player.me.tile);

            for (int i = 0; i < 7; i++)
            {
                foreach (var dir in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    SimulatePush.InsertTile(i, dir);
                    //Console.Error.WriteLine("Push " + i + " " + dir);

                    foreach (var walkableTile in walkableTiles)
                    {
                        var playerWalkables = Pathfinding.GetWalkableTiles(walkableTile, true);
                        walkableTile.walkScore += playerWalkables.Count;
                    }

                    foreach (var questItem in Player.me.questItems)
                    {
                        if (questItem.isTaken)
                        {
                            continue;
                        }

                        var walkableQuests = Pathfinding.GetWalkableTiles(questItem.tile, true);

                        //Console.Error.WriteLine("  Quest " + questItem.tile);
                        /*foreach (var neighbour in questItem.tile.neighboursAlt)
                        {
                            Console.Error.WriteLine("    " + neighbour.Key);
                        }*/

                        //Console.Error.WriteLine(i + " " + dir + questItem.tile + ": " + walkableQuests.Count);
                        foreach (var tile in walkableTiles)
                        {
                            if (walkableQuests.Contains(tile))
                            {
                                //Console.Error.WriteLine("walkable " + i + dir + ": " + tile);
                                tile.questScore += (1.0f/tile.Distance(questItem.tile));
                            }
                        }
                    }
                }
            }

            var sortedScores = walkableTiles.OrderByDescending(tile => tile.questScore)
                                        .ThenByDescending(tile => tile.walkScore)
                                        .ThenByDescending(tile => tile.potentialQuestScore);

            foreach (var tile in sortedScores)
            {
                Console.Error.WriteLine(tile + ": quest-" + tile.questScore + ", walk-" + tile.walkScore + ", potential-" + tile.potentialQuestScore);
            }

            var bestTile = sortedScores.FirstOrDefault();
                        
            if (bestTile != null && bestTile != Player.me.tile)
            {
            
                var path = Pathfinding.CalculateShortestPath(mainPath.GetLast(), bestTile);
                if (path != null)
                {
                    //path.PrintPath();
                    //return;

                    mainPath.AddPath(path);
                }
            }

            //Console.WriteLine("PASS");
            mainPath.Print();
        }

        public static void PushStrategy()
        {
            PushConflict.CheckPushFailed();

            List<PushResult> pushResults = new List<PushResult>();

            for (int i = 0; i < 7; i++)
            {
                foreach (var dir in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    if (PushConflict.willPushFail(i, dir))
                    {
                        continue;
                    }

                    var lastTile = SimulatePush.InsertTile(i, dir);

                    //Console.Error.WriteLine(i + " " + dir + ": " + walkableTiles.Count);

                    PushResult newResult = new PushResult(i, dir, lastTile);
                    newResult.CalculateScore();

                    pushResults.Add(newResult);
                }
            }

            var sortedResults = pushResults.OrderByDescending(result => result.pushScore);

            foreach (var result in sortedResults)
            {
                if (PushConflict.willPushFail(result.num, result.direction))
                {
                    continue;
                }

                PrintPush(result.num, result.direction);
                return;
            }

            int numLoops = 25;
            while (!RandomPush(numLoops))
            {
                numLoops -= 1;
            }
        }

        public static bool RandomPush(int numLoops)
        {
            var random = new System.Random();
            var num = random.Next(7);
            var randDirection = (Direction)random.Next(4);

            SimulatePush.InsertTile(num, randDirection);

            if (numLoops > 0)
            {
                if (PushConflict.willPushFail(num, randDirection))
                {
                    return false;
                }

                foreach (var quest in Player.enemy.quests)
                {
                    foreach (var questItem in quest.GetQuestItems())
                    {
                        var path = Pathfinding.CalculateShortestPath(Player.enemy.tileAlt, questItem.tile, true);
                        if (path != null)
                        {
                            Console.Error.WriteLine("Bad random push.");
                            return false;
                        }
                    }
                }
            }

            PrintPush(num, randDirection);
            return true;
        }

        public static void PrintPush(int num, Direction pushDirection)
        {
            PushConflict.StorePushData(num, pushDirection);

            Console.WriteLine("PUSH " + num + " " + pushDirection.ToString().ToUpper());
        }
    }
}
