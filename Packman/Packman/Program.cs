using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packman
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    public class Node
    {
        public int x;
        public int y;
        public Node parent;
        public Node next;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Node n = (Node)obj;
            return (x == n.x) && (y == n.y);
        }

        public override int GetHashCode()
        {
            return 100 * x + y;
        }
    }

    public class BFSResult
    {
        public Node start;
        public Node goal;
        public Node nextStep;
        public int steps;

        public BFSResult(Node start, Node goal, Node nextStep, int steps)
        {
            this.goal = goal;
            this.nextStep = nextStep;
            this.steps = steps;
            this.start = start;
        }
    }

    class BFS
    {
        public static int PositionToEnemy(Node position, Dictionary<int, BFSResult> enemyResults)
        {
            int distance = 999;

            foreach (var results in enemyResults.Values)
            {
                Node enemyPosition = results.start;

                int d = Math.Abs(enemyPosition.x - position.x) + Math.Abs(enemyPosition.y - position.y);
                distance = Math.Min(distance, d);
            }

            return distance;
        }


        public static void AddBlockedList(HashSet<Node> blockedList, int step, Dictionary<int, BFSResult> enemyResults)
        {
            if (enemyResults != null)
            {
                foreach (var results in enemyResults.Values)
                {
                    Node enemyPosition = results.start;
                    blockedList.Add(enemyPosition);

                    for (int i = 0; i < step && enemyPosition.next != null; i++)
                    {
                        enemyPosition = enemyPosition.next;
                        blockedList.Add(enemyPosition);
                    }
                }
            }
        }

        public static BFSResult useBFS(Block[,] map, Node startNode, Node endNode,
            Dictionary<int, BFSResult> enemyResults = null)
        {
            Block backup = map[endNode.x, endNode.y];
            map[endNode.x, endNode.y] = Block.Goal;

            var result = useBFS(map, startNode, Block.Goal, enemyResults);
            map[endNode.x, endNode.y] = backup; //put it back
            return result;
        }

        public static BFSResult useBFS(Block[,] map, Node startNode, Block goal,
            Dictionary<int, BFSResult> enemyResults = null)
        {
            //BFS start

            Hashtable set = new Hashtable();
            Queue<Node> q = new Queue<Node>();

            List<Node> nextExpand = new List<Node>();
            int step = 1;

            HashSet<Node> blockedList = new HashSet<Node>();
            AddBlockedList(blockedList, step, enemyResults);

            q.Enqueue(startNode); //queue the current position

            Node current = null;

            //iterative bfs loop
            while (q.Count() > 0)
            {
                current = q.Dequeue();

                if (map[current.x, current.y] == goal)
                {
                    break;
                }

                //Console.Error.WriteLine("Current at " + current.x + ", " + current.y);

                var neighbours = new List<Node>(); //grab the neighbours
                for (int i = 0; i < 4; i++)
                {
                    int x = current.x;
                    int y = current.y;
                    switch (i)
                    {
                        case 0:
                            x = x + 1;
                            break;
                        case 1:
                            x = x - 1;
                            break;
                        case 2:
                            y = y + 1;
                            break;
                        case 3:
                            y = y - 1;
                            break;
                    }

                    if (x >= 0 && y >= 0 && x < map.GetLength(0) && y < map.GetLength(1))
                    {
                        var square = map[x, y];

                        if (square == Block.Empty || square == Block.Unknown || square == goal)
                        {
                            /*if (enemyResults != null && (square == Block.Unknown || square == goal))
                            {
                                if (PositionToEnemy(current, enemyResults) <= 3)
                                {
                                    continue;
                                }
                            }*/

                            Node newNode = new Node(x, y);
                            if (!blockedList.Contains(newNode))
                            {
                                neighbours.Add(newNode);
                            }
                        }
                    }
                }

                foreach (Node node in neighbours)
                {
                    //Console.Error.WriteLine(node.index);

                    if (set[node.x + " " + node.y] == null)
                    {
                        set[node.x + " " + node.y] = true;
                        node.parent = current;
                        nextExpand.Add(node);
                        //q.Enqueue(node);
                    }
                }

                if (q.Count == 0 && nextExpand.Count > 0) //if bfs is done expanding current step and next expand has items
                {

                    nextExpand.ForEach(n => q.Enqueue(n)); //put all list items into queue
                    nextExpand.Clear();
                    step++; //add 1 more step to count the distance

                    AddBlockedList(blockedList, step, enemyResults);
                }
            }

            if (map[current.x, current.y] != goal)
            {
                return null;
            }

            //Found goal, now traceback
            int numSteps = 0;
            var trace = current;
            while (trace.parent != null && trace.parent != startNode)
            {
                trace.parent.next = trace;
                trace = trace.parent;
                numSteps++;
            }

            startNode.next = trace;

            return new BFSResult(startNode, current, trace, numSteps);
        }
    }

    class Inputs
    {
        public string input1;
        public string input2;
        public string input3;
        public string input4;

        public List<int> input5 = new List<int>();
        public List<int> input6 = new List<int>();

        public Inputs(string in1, string in2, string in3, string in4)
        {
            this.input1 = in1;
            this.input2 = in2;
            this.input3 = in3;
            this.input4 = in4;
        }
    }

    class Player
    {
        public int x;
        public int y;

        public int id;

        public Player(int id)
        {
            this.id = id;
        }

        public void MoveTo(int xPos, int yPos)
        {
            string direction = "B";
            if (xPos > x)
            {
                direction = "A";
            }
            else if (xPos < x)
            {
                direction = "E";
            }

            if (yPos > y)
            {
                direction = "D";
            }
            else if (yPos < y)
            {
                direction = "C";
            }

            Console.WriteLine(direction);
        }
    }

    enum Action
    {
        right,
        hold,
        up,
        down,
        left
    }

    enum Block
    {
        Unknown,
        Wall,
        Empty,
        Enemy,
        Hero,
        Goal
    }

    class Game
    {
        int width;
        int height;

        public Block[,] map;

        public Player hero;
        public Dictionary<int, Player> players;

        public static Random rand = new Random();

        public Node randomGoal;

        public Game(int width, int height)
        {
            this.width = width;
            this.height = height;

            map = new Block[width, height];
        }

        public void SetBlock(int x, int y, string str)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            switch (str)
            {
                case "#":
                    map[x, y] = Block.Wall;
                    break;
                case "_":
                    map[x, y] = Block.Empty;
                    break;
                default:
                    Console.WriteLine("Unidentified Block: " + str);
                    break;
            }
        }

        public void MakeMove()
        {

            Dictionary<int, BFSResult> enemyResults = new Dictionary<int, BFSResult>();
            for (int i = 0; i < players.Count - 1; i++)//check enemy path to player
            {
                Player enemy = players[i];
                Node start = new Node(enemy.x, enemy.y);

                var res = BFS.useBFS(map, start, Block.Hero);
                if (res != null)
                {
                    enemyResults.Add(i, res);
                }
            }

            Node startNode = new Node(hero.x, hero.y);

            var qResult = BFS.useBFS(map, startNode, Block.Unknown, enemyResults);

            if (qResult != null && IsValidPosition(qResult.nextStep.x, qResult.nextStep.y))
            {
                hero.MoveTo(qResult.nextStep.x, qResult.nextStep.y);
            }
            else
            {
                for (int randomTries = 0; randomTries < 50; randomTries++)
                {
                    if (randomGoal == null || !IsValidPosition(randomGoal.x, randomGoal.y)
                        || (randomGoal.x == hero.x && randomGoal.y == hero.y))
                    {
                        int randX = rand.Next(0, width - 1);
                        int randY = rand.Next(0, height - 1);
                        while (!IsValidPosition(randX, randY))
                        {
                            randX = rand.Next(0, width - 1);
                            randY = rand.Next(0, height - 1);
                        }
                        randomGoal = new Node(randX, randY);
                        Console.Error.WriteLine("RANDOM: " + randomTries);
                    }

                    qResult = BFS.useBFS(map, startNode, randomGoal, enemyResults);

                    if (qResult != null && IsValidPosition(qResult.nextStep.x, qResult.nextStep.y))
                    {
                        break;
                    }
                    else
                    {
                        randomGoal = null;
                    }
                }

                if (qResult != null && IsValidPosition(qResult.nextStep.x, qResult.nextStep.y))
                {
                    hero.MoveTo(qResult.nextStep.x, qResult.nextStep.y);
                }
                else
                {
                    hero.MoveTo(hero.x, hero.y);
                    randomGoal = null;
                }
            }
        }

        public bool IsValidPosition(int x, int y)
        {
            for (int i = 0; i < players.Count - 1; i++)
            {
                Player player = players[i];
                if (x == player.x && y == player.y)
                {
                    return false;
                }
            }

            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return false;
            }

            if (map[x, y] == Block.Wall)
            {
                return false;
            }

            return true;
        }

        public void PrintMap()
        {
            int xCameraView = 25;
            int yCameraView = 5;
            int xCameraMin = Math.Max(0, hero.x - xCameraView);
            int xCameraMax = Math.Min(width, hero.x + xCameraView);
            int yCameraMin = Math.Max(0, hero.y - yCameraView);
            int yCameraMax = Math.Min(height, hero.y + yCameraView);

            for (int y = yCameraMin; y < yCameraMax; y++)
            {
                string s = "";
                for (int x = xCameraMin; x < xCameraMax; x++)
                {
                    switch (map[x, y])
                    {
                        case Block.Unknown:
                            s += "~";
                            break;
                        case Block.Empty:
                            s += " ";
                            break;
                        case Block.Wall:
                            s += "#";
                            break;
                        case Block.Hero:
                            s += "H";
                            break;
                        case Block.Enemy:

                            foreach (var player in players.Values)
                            {
                                if (player.x == x && player.y == y)
                                {
                                    s += player.id;
                                    break;
                                }
                            }

                            break;
                        default:
                            s += ((int)map[x, y] - 3);
                            break;
                    }
                }
                Console.Error.WriteLine(s);
            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            int height = int.Parse(Console.ReadLine());
            int width = int.Parse(Console.ReadLine());
            int num_players = int.Parse(Console.ReadLine());

            Game game = new Game(width, height);

            Dictionary<int, Player> players = new Dictionary<int, Player>();

            for (int i = 0; i < num_players; i++)
            {
                Player player = new Player(i);
                players.Add(i, player);
            }

            Player hero = players[num_players - 1];
            game.hero = hero;
            game.players = players;

            // game loop
            while (true)
            {
                string north_block = Console.ReadLine();
                string east_block = Console.ReadLine();
                string south_block = Console.ReadLine();
                string west_block = Console.ReadLine();

                for (int i = 0; i < num_players; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int x = int.Parse(inputs[0]);
                    int y = int.Parse(inputs[1]);

                    game.map[players[i].x, players[i].y] = Block.Empty;

                    players[i].x = x;
                    players[i].y = y;
                }

                game.SetBlock(hero.x, hero.y - 1, north_block);
                game.SetBlock(hero.x + 1, hero.y, east_block);
                game.SetBlock(hero.x, hero.y + 1, south_block);
                game.SetBlock(hero.x - 1, hero.y, west_block);

                for (int i = 0; i < num_players; i++)
                {
                    int x = players[i].x;
                    int y = players[i].y;

                    if (i + 1 == num_players)
                    {
                        game.map[x, y] = Block.Hero;
                    }
                    else
                    {
                        game.map[x, y] = Block.Enemy;
                    }
                }

                game.MakeMove();
                game.PrintMap();
            }
        }
    }
}
