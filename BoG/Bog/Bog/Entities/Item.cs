using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Item
    {
        public string name;
        public float cost;
        public float damage;
        public float health;
        public float maxHealth;
        public float mana;
        public float maxMana;
        public float speed;
        public float manaRegen;

        public bool isPotion;

        public float itemValue;

        public void CalculateItemValue()
        {
            itemValue =
                (10 * damage + health / 10 + mana / 10 + speed / 3.5f + manaRegen * 50)
                * (1 / cost);
        }

        public void PrintStats()
        {
            Console.Error.WriteLine(name + " (" + cost + ")" + ": " + itemValue);

            string str = "";

            if (damage > 0)
            {
                str += "Dmg: " + damage + " ";
            }

            if (health > 0)
            {
                str += "Hp: " + health + " ";
            }

            if (mana > 0)
            {
                str += "Mana: " + mana + " ";
            }

            if (speed > 0)
            {
                str += "Speed: " + speed + " ";
            }

            if (manaRegen > 0)
            {
                str += "ManaRegen: " + manaRegen;
            }

            Console.Error.WriteLine(str);
        }
    }
}
