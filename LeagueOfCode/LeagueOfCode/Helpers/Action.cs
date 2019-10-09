using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    public enum MoveType
    {
        Pass,
        Pick,
        Summon,
        Attack,
        DirectAttack,
        UseItem,
        DirectUseItem,
    }

    class Action
    {
        static List<Action> actions = new List<Action>();

        public MoveType type = MoveType.Pass;
        public Card attacker;
        public Card target;

        public int cardNum;

        public Action()
        {

        }

        public Action(int cardNum)
        {
            type = MoveType.Pick;
            this.cardNum = cardNum;
        }

        public Action(Card attacker)
        {
            type = MoveType.Summon;
            this.attacker = attacker;
        }

        public Action(MoveType moveType, Card attacker, Card target)
        {
            this.attacker = attacker;
            this.target = target;
            this.type = moveType;
        }

        public void ApplyAction()
        {
            actions.Add(this);

            if (type != MoveType.Summon && attacker != null)
            {
                attacker.used = true;                
                Player.me.usableCards.Remove(attacker);
            }

            if (type == MoveType.Summon)
            {
                Player.me.mana -= attacker.cost;
                Player.me.numCardsOnBoard += 1;

                if (attacker.hasCharge)
                {
                    Player.me.boardCards.Add(attacker);
                    Player.me.handCards.Remove(attacker);
                    attacker.location = CardLocation.PlayerBoard;
                }
            }
            else if (type == MoveType.Attack || type == MoveType.DirectAttack
                 || type == MoveType.UseItem || type == MoveType.DirectUseItem)
            {
                if (attacker.type == CardType.Creature)
                {
                    type = MoveType.Attack;

                    if (target == null)
                    {
                        type = MoveType.DirectAttack;
                        Player.enemy.health -= attacker.attack;
                    }
                    else
                    {
                        target.health -= attacker.attack;
                    }                  
                }
                else
                {
                    type = MoveType.UseItem;

                    if (target == null)
                    {
                        type = MoveType.DirectUseItem;
                    }
                    else
                    {
                        target.attack += attacker.attack;
                        target.health += attacker.health;

                        target.hasGuard = target.hasGuard && !attacker.hasGuard;
                    }
                }
            }
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void PrintActions()
        {
            if (actions.Count == 0)
            {
                Console.WriteLine("PASS");
                return;
            }

            string str = "";

            foreach (var action in actions)
            {
                if (str != "")
                {
                    str += ";";
                }

                switch (action.type)
                {
                    case MoveType.Pick:
                        str += "PICK " + action.cardNum;
                        break;
                    case MoveType.Summon:
                        str += "SUMMON " + action.attacker.id;
                        break;
                    case MoveType.Attack:
                        str += "ATTACK " + action.attacker.id + " " + action.target.id;
                        break;
                    case MoveType.DirectAttack:
                        str += "ATTACK " + action.attacker.id + " -1";
                        break;
                    case MoveType.DirectUseItem:
                        str += "USE " + action.attacker.id + " -1";
                        break;
                    case MoveType.UseItem:
                        str += "USE " + action.attacker.id + " " + action.target.id;
                        break;
                    default:
                        str += "";
                        break;
                }                
            }

            Console.WriteLine(str);
        }
    }
}
