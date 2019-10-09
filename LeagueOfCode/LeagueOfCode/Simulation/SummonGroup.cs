using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class SummonGroup
    {
        public static List<SummonGroup> groups = new List<SummonGroup>();
        public static Dictionary<Card, float> summonValues = new Dictionary<Card, float>();

        public List<Card> myCards = new List<Card>();

        public int simulatedManaCost = 0;

        public float score;

        public int numSummons;

        public SummonGroup()
        {

        }

        public bool AddCard(Card card)
        {
            //already summoned

            if (Player.me.boardCards.Count + numSummons >= 6)
            {
                return false; //max summons on field
            }

            simulatedManaCost += card.cost;

            if (simulatedManaCost > Player.me.mana)
            {
                return false;
            }

            numSummons += 1;
            myCards.Add(card);

            score += card.cardValue;// -card.cost * 2;

            float summonValue;
            if (summonValues.TryGetValue(card, out summonValue))
            {
                score -= summonValue;
            }

            return true;
        }

        public void Print()
        {
            string printString = "SummonGroup: ";

            foreach (var card in myCards)
            {
                printString += card.id + " ";
            }

            printString += "Score: " + score;

            Console.Error.WriteLine(printString);
        }

        public float GetScore()
        {
            return score;
        }

        public static void GetSummonValues(List<Card> summons)
        {
            foreach (var card in summons)
            {
                List<Trade> possibleTrades = new List<Trade>();

                //Get enemy possible trade against this summon
                MonsterTrading.GetTradeOptions(Player.enemy.usableCards, Player.enemy, card, null, possibleTrades);

                var best = possibleTrades.OrderByDescending(t => t.GetScore()).FirstOrDefault();
                if (best != null)
                {
                    summonValues.Add(card, best.GetScore());
                }
            }
        }

        public static void GetSummons()
        {
            var summons = Player.me.handCards.Where(c=> c.cost <= Player.me.mana && 
                                                        c.location == CardLocation.PlayerHands && 
                                                        c.type == CardType.Creature).ToList();
            GetSummonValues(summons);

            for (int i = 1; i <= summons.Count; i++)
            {
                int[] myCards = new int[i];
                GetSummonsCombination(summons, myCards, 0, summons.Count - 1, 0, i);
            }
        }

        public static void GetSummonsCombination(List<Card> summons, int[] data, int start, int end, int index, int numCards)
        {
            if (index == numCards)
            {
                SummonGroup newSummonGroup = new SummonGroup();

                for (int i = 0; i < numCards; i++)
                {
                    var card = summons[data[i]];
                    if (!newSummonGroup.AddCard(card))
                    {
                        return; //cannot summon
                    }
                }

                newSummonGroup.Print();

                groups.Add(newSummonGroup);

                return;
            }

            for (int i = start; i <= end && end - i + 1 >= numCards - index; i++)
            {
                data[index] = i;
                GetSummonsCombination(summons, data, i + 1, end, index + 1, numCards);
            }
        }

        public static void CleanUp()
        {
            groups = new List<SummonGroup>();
            summonValues = new Dictionary<Card, float>();
        }
    }
}
