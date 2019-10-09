
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    class Solution
    {
        static void Main(string[] args)
        {
            string[] inputs;

            // game loop
            while (true)
            {
                int turnType = int.Parse(Console.ReadLine());

                Strategy.turnType = turnType == 0 ? TurnType.Push : TurnType.Move;

                for (int i = 0; i < 7; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    for (int j = 0; j < 7; j++)
                    {
                        string tile = inputs[j];

                        Tile newTile = new Tile(j, i, tile);
                        Tile.AddTile(newTile);
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int numPlayerCards = int.Parse(inputs[0]); // the total number of quests for a player (hidden and revealed)
                    int playerX = int.Parse(inputs[1]);
                    int playerY = int.Parse(inputs[2]);
                    string playerTile = inputs[3];

                    Team team = (Team)i;
                    Player newPlayer = new Player(playerX, playerY, numPlayerCards, playerTile, team);

                    if (team == Team.Player)
                    {
                        Player.me = newPlayer;
                    }
                    else
                    {
                        Player.enemy = newPlayer;
                    }
                }
                int numItems = int.Parse(Console.ReadLine()); // the total number of items available on board and on player tiles
                for (int i = 0; i < numItems; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    string itemName = inputs[0];
                    int itemX = int.Parse(inputs[1]);
                    int itemY = int.Parse(inputs[2]);
                    int itemPlayerId = int.Parse(inputs[3]);

                    Item newItem = new Item(itemX, itemY, itemName, (Team)itemPlayerId);
                    Item.AddItem(newItem);
                }
                int numQuests = int.Parse(Console.ReadLine()); // the total number of revealed quests for both players
                for (int i = 0; i < numQuests; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    string questItemName = inputs[0];
                    int questPlayerId = int.Parse(inputs[1]);

                    Quest newQuest = new Quest(questItemName, (Team)questPlayerId);
                    if (newQuest.team == Team.Player)
                    {
                        Player.me.quests.Add(newQuest);
                    }
                    else
                    {
                        Player.enemy.quests.Add(newQuest);
                    }
                }

                Game.MakeMove();
            }
        }
    }

    class Game
    {
        public static void MakeMove()
        {
            //InitRound();

            Strategy.MakeMove();
            //Action.PrintActions();

            Game.CleanUp();
        }

        public static void CleanUp()
        {
            Tile.CleanUp();
            Item.CleanUp();
        }
    }

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

    public static class HelperExtensions
    {
        public static Direction Reverse(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                default:
                    return Direction.Up;
            }
        }
    }




    class MultiGoalPath
    {
        public Path path;
        
        public MultiGoalPath()
        {

        }

        public bool AddPath(Path newPath)
        {
            //Console.Error.WriteLine("Add: " + newPath.waypoints.First() +" to " + newPath.waypoints.Last());

            if (path == null)
            {
                path = newPath;
                return true;
            }
            else
            {
                var newSteps = newPath.waypoints.Count;

                if (newSteps > 1 && newSteps + path.waypoints.Count <= 20 &&
                    path.waypoints.Last() == newPath.waypoints.First())
                {
                    var waypoints = newPath.waypoints.ToList();
                    waypoints.RemoveAt(0);

                    path.waypoints.AddRange(waypoints);
                    return true;
                }
            }

            return false;
        }

        public Tile GetLast()
        {
            if (path == null)
            {
                return Player.me.tile;
            }
            else
            {
                return path.waypoints.Last();
            }
        }

        public void Print()
        {
            if (path == null)
            {
                Console.WriteLine("PASS");
                return;
            }

            path.PrintPath();
        }
    }

    class Path
    {
        public List<Tile> waypoints = new List<Tile>();

        public Path(Tile start, Tile end)
        {
            MakePath(start, end);
        }

        public void MakePath(Tile start, Tile end)
        {
            List<Tile> points = new List<Tile>();

            Tile current = end;
            while (current.parent != null && start.pos != end.pos)
            {
                points.Add(current);
                current = current.parent;
            }

            points.Add(start);

            points.Reverse();
            waypoints = points;
        }

        public void PrintPath()
        {
            string output = "Move";

            Tile previous = null;
            foreach (var point in waypoints)
            {
                if (previous != null)
                {
                    var dir = previous.neighbours[point];
                    output += " " + dir.ToString();
                }
                previous = point; 
            }

            Console.WriteLine(output.ToUpper());
        }
    }

    class Pathfinding
    {
        public static void Initialize()
        {
            Tile.InitializeNeighbours();
        }

        public static void ResetNodes()
        {
            foreach (var node in Tile.tiles.Values)
            {
                node.gScore = 9999;
                node.fScore = 9999;
                node.parent = null;
            }
        }

        public static Path CalculateShortestPath(Tile start, Tile end, bool useAltNeighbours = false)
        {
            //Console.Error.WriteLine("start: " + start);
            //Console.Error.WriteLine("end: " + end);

            HashSet<Tile> closedSet = new HashSet<Tile>();
            HashSet<Tile> openSet = new HashSet<Tile>();

            ResetNodes();

            start.gScore = 0;
            start.fScore = start.Distance(end);
            start.parent = null;

            openSet.Add(start);

            Tile current = null;
            int loops = 0;

            //Console.Error.WriteLine("start: " + start);

            while (openSet.Count > 0)
            {
                current = openSet.OrderBy(n => n.fScore).FirstOrDefault();

                //Console.Error.WriteLine(current);

                if (current.pos == end.pos)
                {
                    return new Path(start, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                var neighbourDictionary = useAltNeighbours ? current.neighboursAlt : current.neighbours;
                foreach (var pair in neighbourDictionary)
                {
                    var neighbour = pair.Key;
                    var move = pair.Value;

                    //Console.Error.WriteLine("  " + neighbour);

                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int cost = 1;
                    
                    var alternativeDistance = current.gScore + cost;
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else if (alternativeDistance >= neighbour.gScore)
                    {
                        continue;
                    }

                    neighbour.parent = current;
                    neighbour.gScore = alternativeDistance;
                    neighbour.fScore = alternativeDistance + neighbour.Distance(end);
                    //neighbour.moveType = move;
                }

                loops++;
            }

            //Console.Error.WriteLine("Loops: " + loops);

            return null;
        }

        public static HashSet<Tile> GetWalkableTiles(Tile start, bool useAltNeighbours = false)
        {
            HashSet<Tile> visited = new HashSet<Tile>();

            Queue<Tile> queue = new Queue<Tile>();

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();

                var neighbours = useAltNeighbours ? tile.neighboursAlt.Keys : tile.neighbours.Keys;
                foreach (var neighbour in neighbours)
                {
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return visited;
        }
    }

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

    class Item
    {
        public static Dictionary<Tile, Item> items = new Dictionary<Tile, Item>();

        public string name;
        public Tile tile;
        public Team team;
        
        public bool isTaken = false;

        public bool isOnHand = false;

        public Item(int x, int y, string name, Team team)
        {
            this.name = name;
            this.team = team;

            if (x < 0)
            {
                isOnHand = true;

                if(x == -1)
                {
                    tile = Player.me.holdingTile;
                }
                else
                {
                    tile = Player.enemy.holdingTile;
                }
            }
            else
            {
                tile = Tile.GetTile(x, y);
            }
        }

        public static void AddItem(Item newItem)
        {
            items.Add(newItem.tile, newItem);
        }

        public static Item GetItem(Tile tile)
        {
            Item item;
            if (items.TryGetValue(tile, out item))
            {
                return item;
            }

            return null;
        }

        public static void CleanUp()
        {
            items = new Dictionary<Tile, Item>();
        }

    }

    public enum Team
    {
        Player,
        Opponent,
    }

    class Player
    {
        public static Player me;
        public static Player enemy;

        public Tile tile;
        public Tile tileAlt;

        public Tile holdingTile;

        public int numQuests;

        public List<Quest> quests = new List<Quest>();
        public List<Item> questItems = new List<Item>();

        public string tileString;

        public Team team;

        public Player(int x, int y, int numQuests, string tileString, Team team)
        {
            this.numQuests = numQuests;
            this.tileString = tileString;

            tile = Tile.GetTile(x, y);

            int pos = team == Team.Player ? -1 : -2;

            holdingTile = new Tile(pos, pos, tileString);

            this.team = team;
        }

        public void Initialize()
        {
            //MakeQuests();
            GetQuestItems();
        }

        public void MakeQuests()
        {
            //Console.Error.WriteLine("Quests: " + numQuests);
            //less than 3 quests, all items are quests
            //only works whil moving

            if (numQuests <= 3)
            {
                foreach (var item in Item.items.Values.Where(i=>i.team == this.team))
                {
                    if (!quests.Any(q => q.name == item.name))
                    {
                        Quest newQuest = new Quest(item.name, this.team);
                        quests.Add(newQuest);
                    }
                }
            }
        }

        public void GetQuestItems()
        {            
            foreach (var quest in quests)
            {
                questItems.AddRange(quest.GetQuestItems());
            }
        }
    }

    struct Position
    {
        public int x;
        public int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Position value1, Position value2)
        {
            return value1.x == value2.x
                && value1.y == value2.y;
        }

        public static bool operator !=(Position value1, Position value2)
        {
            return !(value1 == value2);
        }

        public override bool Equals(object obj)
        {
            return (obj is Position) ? this == (Position)obj : false;
        }

        public bool Equals(Position other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (int)(this.x + this.y * 1000);
        }

        public override string ToString()
        {
            return "{" + this.x + ", " + this.y + "}";
        }
    }

    class Quest
    {
        public string name;
        public Team team;

        public Quest(string name, Team team)
        {
            this.name = name;
            this.team = team;
        }

        public List<Item> GetQuestItems()
        {
            return Item.items.Values.Where(item => 
                item.name == name && 
                item.team == team).ToList();
        }
    }

}
