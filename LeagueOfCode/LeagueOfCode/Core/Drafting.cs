using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class Drafting
    {
        public static List<Card> draftCards = new List<Card>();

        public static Dictionary<int, int> manaCurve = new Dictionary<int, int>();

        public static bool initMana;

        public static int[] manaDist = new int[]
        {
            1,//0
            1,//1
            2,//2
            3,//3
            3,//4
            2,//5
            1,//6
            1,//7
        };

        public static int manaMax;

        public static void InitManaCurve()
        {
            for (int i = 0; i < 8; i++)
            {
                manaCurve.Add(i, 0);
            }

            manaMax = manaDist.Sum(c => c);
        }

        public static void DraftOld()
        {
            Card card = draftCards.OrderBy(c => c.type == CardType.Creature ? 0 : 1).ThenByDescending(c => c.weightedValue).First();

            int index = draftCards.IndexOf(card);

            var newAction = new Action(index);
            newAction.ApplyAction();
        }

        public static void Draft()
        {
            if (!initMana)
            {
                InitManaCurve();
                initMana = true;
            }

            Card card = draftCards.OrderByDescending(c => c.weightedValue).First();

            int index = draftCards.IndexOf(card);

            var newAction = new Action(index);
            newAction.ApplyAction();

            var cost = card.cost >= 7 ? 7 : card.cost;
            manaCurve[cost] += 1;
        }

        public static void Draft2()
        {
            if (!initMana)
            {
                InitManaCurve();
                initMana = true;
            }

            float[] cardPercentage = new float[3];
            for (int i = 0; i < 3; i++)
            {
                var card = draftCards[i];
                var cost = card.cost >= 7 ? 7 : card.cost;

                float estimatedCards = 30f * manaDist[cost]/manaMax;
                float cardLeftWeight = 1f - manaCurve[cost] / estimatedCards;

                cardPercentage[i] = manaDist[cost] * cardLeftWeight;
            }

            float max = cardPercentage.Sum();
            float totalPercentage = 0;

            Random rand = new Random();
            var number = rand.NextDouble();

            for (int i = 0; i < 3; i++)
            {
                totalPercentage += cardPercentage[i] / max;

                if (number < totalPercentage)
                {
                    var newAction = new Action(i);
                    newAction.ApplyAction();

                    var card = draftCards[i];
                    var cost = card.cost >= 7 ? 7 : card.cost;
                    manaCurve[cost] += 1;

                    break;
                }
            }
            
            /*Card card = draftCards.OrderBy(c => c.type == CardType.Creature ? 0 : 1).ThenByDescending(c => GetDraftCardValue(c)).First();

            int index = draftCards.IndexOf(card);

            var newAction = new Action(index);
            newAction.ApplyAction();

            */
        }

        static float GetDraftCardValue(Card card)
        {
            var cost = card.cost >= 7 ? 7 : card.cost;

            //Math.Round(30 * (float)manaDist[cost] / manaMax);

            float estimatedCards = 30f * manaDist[cost]/ (float)manaMax;
            float cardLeftWeight = 2f - manaCurve[cost] / estimatedCards;
            
            var ret = card.weightedValue * cardLeftWeight;
            Console.Error.WriteLine("Card " + card.cost + ": " + ret);

            return ret;
        }

        public static void CleanUp()
        {
            draftCards = new List<Card>();
        }

    }
}
