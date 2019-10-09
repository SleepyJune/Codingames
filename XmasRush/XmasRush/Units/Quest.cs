using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
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
