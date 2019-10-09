using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    public enum MoveType
    {
        Wait,
        Move,
        Attack,
        Attack_Nearest,
        Move_Attack,
        Buy,
        Sell,
        Spell,
    }

    class Action
    {
        public static List<Action> actions = new List<Action>();

        public Vector position;
        public Entity target;
        public UnitType targetType;
        public Item item;
        public Spell spell;

        public Hero hero;

        public string extraComment = "";

        public MoveType moveType = MoveType.Wait;

        public Action()
        {

        }

        public Action(Entity target)
        {
            moveType = MoveType.Attack;
            this.target = target;

            var hero = GetOwner();
            
            hero.target = target;
            hero.moveAttackTime = hero.GetMoveAttackTime(target);
            //this.target.health -= hero.damage;
        }

        public Action(Vector pos)
        {
            moveType = MoveType.Move;
            this.position = pos;
        }

        public Action(Vector pos, Entity target)
        {
            moveType = MoveType.Move_Attack;
            this.position = pos;
            this.target = target;

            var hero = GetOwner();
            hero.target = target;
            hero.moveAttackTime = hero.GetMoveAttackTime(pos, target);
            //this.target.health -= hero.damage;
        }

        public Action(MoveType moveType, Item item)
        {
            this.moveType = moveType;
            this.item = item;

            if (moveType == MoveType.Buy)
            {
                Strategy.myGold -= (int)item.cost;

                var hero = GetOwner();

                List<Item> itemList;

                if (Strategy.myHeroItems.TryGetValue(hero.id, out itemList))
                {
                    itemList.Add(item);
                }
                else
                {
                    itemList = new List<Item>();
                    itemList.Add(item);

                    Strategy.myHeroItems.Add(hero.id, itemList);
                }
            }
            else
            {
                Strategy.myGold += (int)Math.Round(item.cost * .5f);

                var hero = GetOwner();

                List<Item> itemList;

                if (Strategy.myHeroItems.TryGetValue(hero.id, out itemList))
                {
                    itemList.Remove(item);
                }
            }
        }

        public Action(Spell spell)
        {
            this.moveType = MoveType.Spell;
            this.spell = spell;
        }

        public Action(Spell spell, Vector pos)
        {
            this.moveType = MoveType.Spell;
            this.spell = spell;
            this.position = pos;
        }

        public Action(Spell spell, Entity target)
        {
            this.moveType = MoveType.Spell;
            this.spell = spell;
            this.target = target;
        }

        public static Hero GetOwner()
        {
            if (Action.actions.Count == 0)
            {
                return Strategy.myHeros.First();
            }
            else
            {
                return Strategy.myHeros[1];
            }
        }

        public static void AddExtraText(string str)
        {
            var lastAction = Action.actions.Last();
            if (lastAction != null)
            {
                lastAction.extraComment = str;
            }
        }

        public static void AddAction(Action action)
        {
            var owner = GetOwner();
            action.hero = owner;

            Action.actions.Add(action);            
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void RoundPosition(Action action)
        {
            action.position = new Vector((int)Math.Round(action.position.x),
                                         (int)Math.Round(action.position.y),0);
        }

        public static void PrintActions()
        {
            string str = "";
                      
            foreach (var action in actions)
            {
                RoundPosition(action);

                switch (action.moveType)
                {
                    case MoveType.Move:
                        str = "MOVE " + action.position.x + " " + action.position.y;
                        break;
                    case MoveType.Attack:
                        str = "ATTACK " + action.target.id;
                        break;
                    case MoveType.Attack_Nearest:
                        str = "ATTACK_NEAREST " + action.targetType.ToString();
                        break;
                    case MoveType.Move_Attack:
                        str = "MOVE_ATTACK " + action.position.x + " " + action.position.y
                                             + " " + action.target.id;
                        break;
                    case MoveType.Buy:
                        str = "BUY " + action.item.name;
                        break;
                    case MoveType.Sell:
                        str = "SELL " + action.item.name;
                        break;
                    case MoveType.Spell:
                        str = action.spell.spellName;

                        if (action.spell.spellType == SpellType.Targeted)
                        {
                            str += " " + action.target.id;
                        }
                        else if (action.spell.spellType == SpellType.Position)
                        {
                            str += " " + action.position.x + " " + action.position.y;
                        }

                        //str += ";" + action.spell.spellName;
                        break;
                    default:
                        str = "WAIT";
                        break;
                }

                str += ";" + action.extraComment;
                //str += ";" + action.hero.heroType.ToString();

                Console.WriteLine(str);

                if (actions.Count != Strategy.myHeros.Count)
                {
                    Console.Error.WriteLine("INCORRECT NUMBER OF OUTPUT!!!");
                }
            }
        }
    }
}
