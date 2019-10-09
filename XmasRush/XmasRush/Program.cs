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
}