using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    public enum CardLocation
    {
        PlayerHands = 0,
        PlayerBoard = 1,
        EnemyBoard = -1,
    }

    public enum CardType
    {
        Creature = 0,
        Green = 1,
        Red = 2,
        Blue = 3,
    }

    public enum Ability
    {
        Breakthrough,
        Charge,
        Drain,
        Guard,
        Lethal,
        Ward,
    }

    class Card
    {
        public int number;
        public int id;
        public CardLocation location;
        public CardType type;
        public int cost;
        public int attack;
        public int health;
        public string abilities;
        public int myHealthChange;
        public int enemyHealthChange;
        public int cardDraw;

        public float cardValue;
        public float weightedValue;
        public float bonusValue;
        
        public bool hasBreakthrough;
        public bool hasCharge;
        public bool hasDrain;
        public bool hasGuard;
        public bool hasLethal;
        public bool hasWard;

        public bool isSpellCard;

        public bool used;

        public Dictionary<Trade, SimulationCard> simulationCards = new Dictionary<Trade,SimulationCard>();

        public void ProcessCard()
        {
            GetCardAbilities();
            ComputeCardValue();
        }

        public void ComputeCardValue()
        {            
            if (type == CardType.Creature)
            {
                cardValue = 1.4f * attack + health;

                if (hasLethal)
                {
                    bonusValue += 4;
                }

                if (hasGuard)
                {
                    bonusValue += 1;
                }

                if (hasWard)
                {
                    bonusValue += Math.Min(4,attack);
                }

                cardValue += bonusValue;
            }
            else
            {                
                if (type == CardType.Green)
                {
                    cardValue = attack + .5f * health;

                    if (hasLethal)
                    {
                        cardValue += 5;
                    }

                    if (hasWard)
                    {
                        cardValue += 5;
                    }

                    if (hasCharge)
                    {
                        cardValue += 2;
                    }
                }
                else if (type == CardType.Red)
                {
                    cardValue = Math.Abs(attack) + 2 * Math.Abs(health);
                }
                else if (type == CardType.Blue)
                {
                    cardValue = 0;// 2 * Math.Abs(health);
                }

                cardValue += cardDraw * 3;
            }

            weightedValue = cardValue / (cost+1);
        }

        public bool isCardUsable()
        {
            if (cost > Player.me.mana)
            {
                return false;
            }

            if (type == CardType.Creature)
            {
                if (health > 0)
                {
                    if (location != CardLocation.PlayerHands)
                    {
                        return true;
                    }
                    else
                    {
                        if (hasCharge)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (location == CardLocation.PlayerHands)
                {
                    isSpellCard = true;

                    return true;
                }
            }

            return false;
        }

        public void GetCardAbilities()
        {
            if (abilities[0] == 'B')
            {
                hasBreakthrough = true;
            }

            if (abilities[1] == 'C')
            {
                hasCharge = true;
            }

            if (abilities[2] == 'D')
            {
                hasDrain = true;
            }

            if (abilities[3] == 'G')
            {
                hasGuard = true;

                if (location == CardLocation.EnemyBoard)
                {                    
                    Player.enemy.tauntCards.Add(this);
                }
                else if (location == CardLocation.PlayerBoard)
                {
                    Player.me.tauntCards.Add(this);
                }
            }

            if (abilities[4] == 'L')
            {
                hasLethal = true;
            }

            if (abilities[5] == 'W')
            {
                hasWard = true;
            }
        }
    }
}
