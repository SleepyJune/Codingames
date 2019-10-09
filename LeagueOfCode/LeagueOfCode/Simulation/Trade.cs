using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class Trade
    {
        public List<Card> myCards = new List<Card>();
        public Card target;

        public Trade playerTrade;

        public float myCost;
        public float enemyCost;
        public float bonusScore;
                
        float score = 0;

        public bool targetKilleable = false;
        public bool overkill = false;

        public int totalAttack = 0;
        public int numSummons = 0;

        HashSet<Card> simulationCards;

        public int simulatedManaCost = 0;

        public Card spellCard;
        public Card spellCardTarget;

        public Trade(Card target)
        {
            this.target = target;

            simulationCards = new HashSet<Card>();
            simulationCards.Add(target);
        }

        public void AddCard(Card card)
        {
            totalAttack += card.attack;
            myCards.Add(card);

            if (card.location == CardLocation.PlayerHands)
            {
                simulatedManaCost += card.cost;

                if (card.type == CardType.Green)
                {
                    if (spellCard != null)
                    {
                        simulatedManaCost = 9999; //cannot compute more than 1 spell card
                    }

                    spellCard = card;
                }
                else if (card.type == CardType.Creature)
                {
                    numSummons += 1;
                }
            }
        }

        public bool ComputeScore()
        {
            if (simulatedManaCost > Player.me.mana)
            {
                return false;
            }

            if (target.hasWard)
            {
                myCards = myCards.OrderByDescending(c => c.isSpellCard)
                                 .ThenBy(c => c.attack)
                                 .ThenByDescending(c => c.hasLethal).ToList();
            }
            else
            {
                myCards = myCards.OrderByDescending(c => c.type == CardType.Creature && c.hasLethal)
                                 .ThenByDescending(c => c.isSpellCard)
                                 .ThenByDescending(c => c.attack).ToList();
            }

            if (spellCard != null)
            {
                float highestScore = -999;

                List<SimulationCard> highestSimulatedCards = null;

                foreach (var card in myCards)
                {
                    if (card.type != CardType.Creature)
                    {
                        continue;
                    }

                    List<SimulationCard> newSimulatedCards = new List<SimulationCard>();

                    var ret = ComputeScoreHelper(card, newSimulatedCards);
                    if (ret)
                    {
                        if (score >= highestScore)
                        {
                            highestScore = score;
                            spellCardTarget = card;
                            highestSimulatedCards = newSimulatedCards;
                        }
                    }
                }

                if (highestSimulatedCards != null)
                {
                    foreach (var card in highestSimulatedCards)
                    {
                        card.parent.simulationCards.Add(this, card);
                    }
                }

                if (highestScore == -999)
                {
                    return false;
                }
            }
            else
            {
                return ComputeScoreHelper();
            }

            return true;
        }

        public bool ComputeScoreHelper(Card spellTarget = null, List<SimulationCard> storedSimulatedCards = null)
        {
            enemyCost = 0;
            myCost = 0;
            bonusScore = 0;

            SimulationCard sTarget = SimulationCard.NewSimulationCard(target, playerTrade);
            
            foreach (var card in myCards)
            {
                if (sTarget.health <= 0) //overkill
                {
                    overkill = true;
                    break;
                }
                                
                simulationCards.Add(card);

                SimulationCard sCard = SimulationCard.NewSimulationCard(card, playerTrade);
                if (storedSimulatedCards != null)
                {
                    storedSimulatedCards.Add(sCard);
                }
                else
                {
                    card.simulationCards.Add(this, sCard);
                }              
                
                if (card.type == CardType.Creature)
                {
                    //check if enough room for summon
                    if (card.hasCharge && card.location == CardLocation.PlayerHands)
                    {

                    }

                    if (spellTarget != null && spellTarget == card)
                    {
                        if (spellCard.hasWard)
                        {
                            sCard.hasWard = true;
                        }

                        if (spellCard.hasLethal)
                        {
                            sCard.hasLethal = true;
                        }

                        sCard.attack += spellCard.attack;
                        sCard.health += spellCard.health;
                    }


                    if (sTarget.hasWard)// && card.attack > 0)
                    {
                        sTarget.hasWard = false;
                    }
                    else
                    {
                        if (sCard.hasLethal)
                        {
                            sTarget.health = 0;
                        }
                        else
                        {
                            sTarget.health -= sCard.attack;
                        }
                    }

                    var cardHealthLeft = sCard.health - sTarget.attack;
                    if (!sCard.hasWard)
                    {
                        if (sTarget.hasLethal)
                        {
                            cardHealthLeft = 0;
                        }

                        if (cardHealthLeft <= 0)
                        {
                            myCost += sCard.health + sCard.attack;
                        }
                        else
                        {
                            myCost += sCard.health - cardHealthLeft;
                            //bonusScore += cardAttack;
                        }
                    }
                }
                else
                {
                    if (card.type == CardType.Red || card.type == CardType.Blue)
                    {
                        myCost += card.cost * 2;

                        bonusScore += card.cardDraw * 3;

                        if (card.health < 0)
                        {
                            if (sTarget.hasWard)
                            {
                                sTarget.hasWard = false;
                            }
                            else
                            {
                                sTarget.health += card.health;
                            }
                        }                        
                        
                        sTarget.attack += card.attack;

                        if (card.hasWard)
                        {
                            sTarget.hasWard = false;
                        }

                        if (card.hasLethal)
                        {
                            sTarget.hasLethal = false;
                        }

                        if (card.hasGuard) //lazy coding
                        {
                            if (target.hasGuard)
                            {
                                //score = target.health;
                                //return true;
                            }
                        }
                    }
                }
            }

            enemyCost = target.health + target.attack + target.bonusValue;

            /*if (target.hasLethal)
            {
                if (Player.me.highestCardOnBoard != null)
                {
                    enemyCost += Player.me.highestCardOnBoard.health + Player.me.highestCardOnBoard.attack;
                }
            }*/

            score = enemyCost - myCost + bonusScore;
                        
            if (sTarget.health <= 0 && overkill == false)
            {
                targetKilleable = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /*public void AddSimulatedCard(Card target, SimulationCard card)
        {
            List<SimulationCard> cardList;
            if (simulatedCards.TryGetValue(target, out cardList))
            {
                cardList.Add(card);
            }
            else
            {
                cardList = new List<SimulationCard>();
                cardList.Add(card);
                simulatedCards.Add(target, cardList);
            }
        }*/

        public void Print()
        {
            string printString = "Target: " + target.id + " Attackers: ";

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

        public HashSet<Card> GetCards()
        {
            return simulationCards;
        }
    }
}
