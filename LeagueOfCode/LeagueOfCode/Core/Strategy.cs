using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class Strategy
    {
        public static void Initialize()
        {
            Player.ProcessPlayers();
        }

        public static bool isDraftPhase
        {
            get
            {
                return Player.me.mana == 0;
            }
        }

        public static void MakeMove()
        {
            Initialize();

            if (isDraftPhase)
            {
                Drafting.Draft();
            }
            else
            {
                Attack();
                DefaultAttack();
                Summon();
                DefaultSummon();
                DefaultAttack();
                UseItem();
            }
        }

        public static void Summon()
        {
            var bestSummon = SummonGroup.groups.OrderByDescending(group => group.GetScore()).FirstOrDefault();

            if (bestSummon != null)
            {
                BestSummon(bestSummon);
            }
        }

        public static void DefaultSummon()
        {
            foreach (var card in Player.me.handCards.OrderByDescending(c => c.cost))
            {
                if (Player.me.numCardsOnBoard >= 6)
                {
                    return;
                }

                if (card.type == CardType.Creature)
                {
                    if (Player.me.mana >= card.cost)
                    {
                        var newAction = new Action(card);
                        newAction.ApplyAction();
                    }
                }
            }
        }

        public static void UseItem()
        {
            foreach (var card in Player.me.handCards.OrderByDescending(c => c.cost))
            {
                if (card.type != CardType.Creature)
                {
                    if (Player.me.mana >= card.cost)
                    {
                        //Action.actions.Add(new Action(card));
                        if (card.type == CardType.Blue)
                        {
                            var newAction = new Action(MoveType.DirectUseItem, card, null);
                            newAction.ApplyAction();
                        }
                        /*else if (card.type == CardType.Green)
                        {
                            var target = Player.me.cards.OrderByDescending(c => c.location == CardLocation.PlayerBoard ? 0 : 1).FirstOrDefault();

                            if (target != null)
                            {
                                var newAction = new Action(MoveType.UseItem, card, target);
                                newAction.ApplyAction();
                            }
                        }
                        else if (card.type == CardType.Red)
                        {
                            var target = Player.enemy.cards.FirstOrDefault();

                            if (target != null)
                            {
                                var newAction = new Action(MoveType.UseItem, card, target);
                                newAction.ApplyAction();
                            }
                        }*/
                    }
                }
            }
        }

        public static void DefaultAttack()
        {
            var enemyCards = Player.enemy.cards
                .OrderBy(c => c.hasGuard ? 0 : 1)
                .ThenBy(c => c.health);

            foreach (var card in Player.me.boardCards.OrderBy(c => c.cost))
            {
                if (card.used)
                {
                    continue;
                }

                if (Player.enemy.numCardsOnBoard > 0 && card.attack > 0)
                {
                    foreach (var enemyCard in enemyCards)
                    {
                        //Action.actions.Add(new Action(card, enemyCard));

                        if (enemyCard.health <= 0)
                        {
                            continue;
                        }

                        /*if (enemyCard.hasGuard)
                        {
                            var newAction = new Action(MoveType.Attack, card, enemyCard);
                            newAction.ApplyAction();
                            break;
                        }*/
                    }
                }

                if (!card.used)
                {
                    var newAction = new Action(MoveType.DirectAttack, card, null);
                    newAction.ApplyAction();
                }
            }
        }

        public static void Attack()
        {
            MonsterTrading.StartSimulation();

            //Execute Simulation

            var orderedSimulations = Player.me.tradeGroups.OrderByDescending(sim => sim.finalScore);
            var best = orderedSimulations.FirstOrDefault();

            /*foreach (var sim in orderedSimulations)
            {
                sim.Print();
            }*/

            if (best != null && best.GetScore() > 0)
            {
                foreach (var trade in best.simulations)
                {
                    if (trade.spellCardTarget != null && trade.spellCardTarget.location == CardLocation.PlayerHands)
                    {
                        var summonAction = new Action(trade.spellCardTarget);
                        summonAction.ApplyAction();
                    }

                    foreach (var card in trade.myCards)
                    {
                        if (card.location == CardLocation.PlayerHands)
                        {
                            if (card.type == CardType.Creature && card.hasCharge)
                            {
                                var summonAction = new Action(card);
                                summonAction.ApplyAction();
                            }
                            else if (card.type == CardType.Green && trade.spellCardTarget != null)
                            {
                                var newSpell = new Action(MoveType.UseItem, card, trade.spellCardTarget);
                                newSpell.ApplyAction();
                                continue;
                            }
                        }

                        var newAction = new Action(MoveType.Attack, card, trade.target);
                        newAction.ApplyAction();
                    }
                }

                best.Print();

                if (best.bestSummonGroup != null)
                {
                    BestSummon(best.bestSummonGroup);
                }
            }
        }

        public static void BestSummon(SummonGroup group)
        {
            foreach (var card in group.myCards)
            {
                if (Player.me.numCardsOnBoard >= 6)
                {
                    return;
                }

                if (card.type == CardType.Creature)
                {
                    if (Player.me.mana >= card.cost)
                    {
                        var newAction = new Action(card);
                        newAction.ApplyAction();
                    }
                }
            }
        }

        public static void CleanUp()
        {

        }
    }
}
