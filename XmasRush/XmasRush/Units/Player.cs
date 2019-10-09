using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
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
}
