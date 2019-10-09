using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class SimulationGroup
    {
        public List<Trade> simulations;

        public HashSet<Card> simulationCards;

        HashSet<Card> tauntCardsKilled;

        public HashSet<Card> targets;

        public int simulatedManaCost;

        public SummonGroup bestSummonGroup;

        float score;
        public float enemyScore;
        public float finalScore;
        
        int heroHealth;

        int attackHeroScore;

        Player player;

        public SimulationGroup(Player player)
        {
            this.player = player;

            simulations = new List<Trade>();
            simulationCards = new HashSet<Card>();

            tauntCardsKilled = new HashSet<Card>();

            targets = new HashSet<Card>();

            heroHealth = player.health;

            attackHeroScore = player.maxAttackHeroScore;
        }

        public bool AddSimulation(Trade trade)
        {
            if (trade != null && simulationCards.Contains(trade.target))
            {
                return false;
            }

            if (simulationCards.Overlaps(trade.GetCards()))
            {
                return false;
            }

            if (tauntCardsKilled.Count < player.opponent.tauntCards.Count)
            {
                //enemy has taunt cards and player has not killed them all yet

                if (trade != null)
                {
                    if (player.opponent.tauntCards.Contains(trade.target))
                    {
                        tauntCardsKilled.Add(trade.target);
                    }
                    else
                    {
                        if (trade.myCards.Count == 1 && trade.myCards[0].isSpellCard)
                        {
                            //Console.Error.WriteLine("SpellCard Taunt " + trade.target.id + ": " + trade.GetScore());
                            //spell cards can go through taunt
                        }
                        else
                        {
                            return false; //cannot attack target without killing taunt cards
                        }
                    }
                }
            }

            attackHeroScore -= trade.totalAttack;
            simulatedManaCost += trade.simulatedManaCost;

            if (simulatedManaCost > Player.me.mana)
            {
                return false;
            }

            targets.Add(trade.target);
            simulations.Add(trade);

            foreach (var card in trade.GetCards())
            {
                simulationCards.Add(card);
            }

            score += trade.GetScore();

            return true;
        }

        public void Print()
        {
            string printString = "Group: ";

            foreach (var sim in simulations)
            {
                foreach (var card in sim.GetCards())
                {
                    printString += card.id + " ";
                }

                printString += ", ";
            }

            printString += "Final: " + finalScore + " S1: " + score + " S2: " + enemyScore + " H: " + attackHeroScore;

            if (bestSummonGroup != null)
            {
                printString += " Summon: " + bestSummonGroup.score;
            }

            Console.Error.WriteLine(printString);
        }

        public bool ComputeScore()
        {
            if (tauntCardsKilled.Count >= player.opponent.tauntCards.Count) //killed all taunts
            {
                //Kill hero in this turn
                if (attackHeroScore >= player.opponent.health)
                {
                    score = 9999;
                }
                else
                {
                    if (!Player.enemy.hasLethal)
                    {
                        score += attackHeroScore;
                    }
                }
            }
            else
            {
                attackHeroScore = 0;
            }

            return true;
        }

        public float GetScore()
        {
            return score;
        }

        public HashSet<Card> GetCards()
        {
            return simulationCards;
        }
    }
}
