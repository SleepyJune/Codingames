using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class MonsterTrading
    {
        public static void StartSimulation()
        {
            var start = Timer.TickCount;

            FindGroupSimulation(Player.enemy);
            Console.Error.WriteLine("Enemy Trade Groups: " + Player.enemy.tradeGroups.Count);
            
            FindGroupSimulation(Player.me);
            Console.Error.WriteLine("My Trade Groups: " + Player.me.tradeGroups.Count);

            SummonGroup.GetSummons();

            FindMyBestTradeGroup();

            Console.Error.WriteLine("End Simulation: " + (Timer.TickCount - start));
        }

        public static void FindMyBestTradeGroup()
        {
            var enemyTradeGroups = Player.enemy.tradeGroups.OrderByDescending(group => group.GetScore());

            foreach (var myGroup in Player.me.tradeGroups)
            {

                //Player.enemy.tradeGroups = new List<SimulationGroup>(); //refresh
                //FindGroupSimulation(Player.enemy, myGroup);

                var best = enemyTradeGroups.Where(group => PossibleGroupSelection(myGroup, group)).FirstOrDefault();
                //.Where(group => PossibleGroupSelection(myGroup, group))

                var bestSummon = SummonGroup.groups.Where(group => PossibleSummonSelection(myGroup, group))
                                                   .OrderByDescending(group => group.GetScore()).FirstOrDefault();

                myGroup.finalScore = myGroup.GetScore();

                if (best != null)
                {
                    myGroup.finalScore -= best.GetScore();
                    myGroup.enemyScore = best.GetScore();
                }

                if (bestSummon != null)
                {
                    //myGroup.finalScore += bestSummon.GetScore();
                    myGroup.bestSummonGroup = bestSummon;
                }
            }
        }

        public static bool PossibleGroupSelection(SimulationGroup myGroup, SimulationGroup enemyGroup)
        {
            foreach (var target in myGroup.targets)
            {
                if (enemyGroup.simulationCards.Contains(target))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool PossibleSummonSelection(SimulationGroup myGroup, SummonGroup summonGroup)
        {
            if (myGroup.simulatedManaCost + summonGroup.simulatedManaCost > Player.me.mana)
            {
                return false;
            }

            /*foreach (var target in summonGroup.)
            {
                if (enemyGroup.simulationCards.Contains(target))
                {
                    return false;
                }
            }*/

            return true;
        }

        public static void FindGroupSimulation(Player player, SimulationGroup parent = null)
        {           
            FindTrades(player, parent);

            //player.trades = player.trades.OrderBy(sim => sim is Trade ? 0 : 1).ToList();

            for (int i = 1; i <= 3 && i <= player.trades.Count; i++)
            {
                int[] myCards = new int[i];
                GetSimulationCombination(player, myCards, 0, player.trades.Count - 1, 0, i);
            }
        }

        public static void GetSimulationCombination(Player player, int[] data, int start, int end, int index, int numCards)
        {
            if (index == numCards)
            {
                SimulationGroup group = new SimulationGroup(player);
                                
                for (int i = 0; i < numCards; i++)
                {
                    var sim = player.trades[data[i]];
                    if (!group.AddSimulation(sim))
                    {
                        return;
                    }
                }

                var possible = group.ComputeScore();
                if (possible)
                {
                    player.tradeGroups.Add(group);

                    //Console.Error.WriteLine(printString);
                }

                return;
            }

            for (int i = start; i <= end && end - i + 1 >= numCards - index; i++)
            {
                data[index] = i;
                GetSimulationCombination(player, data, i + 1, end, index + 1, numCards);
            }
        }

        public static void FindTrades(Player player, SimulationGroup parent = null)
        {
            var enemyPlayer = player.isMe ? Player.enemy : Player.me;

            var enemyCards = enemyPlayer.boardCards
                .OrderBy(c => c.hasGuard ? 0 : 1)
                .ThenByDescending(c => c.health);

            /*var print = player.isMe ? "My " : "Enemy ";
            print += "Useable Cards: ";

            foreach(var card in player.usableCards)
            {
                print += card.id + " ";
            }

            Console.Error.WriteLine(print);*/

            List<Card> newUseableCards = parent != null ?
                player.usableCards.Where(c => !parent.targets.Contains(c)).ToList() : player.usableCards;

            foreach (var enemy in enemyCards)
            {
                if (parent != null)
                {
                    var target = SimulationCard.GetSimulationCard(enemy, parent);
                    if (target != null)
                    {
                        if (!(target.health > 0 && (target.attack > 0 || target.hasGuard)))
                        {
                            continue;
                        }
                    }
                }

                if (enemy.health > 0 && (enemy.attack > 0 || enemy.hasGuard))
                {
                    GetTradeOptions(newUseableCards, player, enemy, parent);
                }
            }
        }

        public static void GetTradeOptions(List<Card> usableCards, Player player, Card target, SimulationGroup parent, List<Trade> ret = null)
        {
            for (int i = 1; i < 4 && i <= usableCards.Count; i++)
            {
                int[] myCards = new int[i];
                GetTradeCombination(usableCards, player, target, myCards, 0, usableCards.Count - 1, 0, i, ret);
            }
        }

        public static void GetTradeCombination(List<Card> usableCards, Player player, Card target, int[] data, int start, int end, int index, int numCards, List<Trade> ret = null)
        {
            if (index == numCards)
            {
                Trade newTrade = new Trade(target);
                                
                for(int i=0;i<numCards;i++)
                {
                    var card = usableCards[data[i]];
                    newTrade.AddCard(card);                    
                }
                                
                var killable = newTrade.ComputeScore();
                if (killable)// && newTrade.GetScore() >= 0)
                {
                    if (ret != null)
                    {
                        ret.Add(newTrade);
                    }
                    else
                    {
                        player.trades.Add(newTrade);
                    }

                    //newTrade.Print();
                }

                return;
            }

            for (int i = start; i <= end && end - i+1 >= numCards - index; i++)
            {
                data[index] = i;
                GetTradeCombination(usableCards, player, target, data, i + 1, end, index + 1, numCards, ret);
            }
        }

        public static void CleanUp()
        {
            
        }
    }
}
