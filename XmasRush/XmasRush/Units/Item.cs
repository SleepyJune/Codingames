using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
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
}
