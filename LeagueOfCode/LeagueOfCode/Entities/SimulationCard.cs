using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class SimulationCard
    {
        public Card parent;

        public int attack;
        public int health;

        //public bool hasBreakthrough;
        //public bool hasCharge;
        //public bool hasDrain;
        public bool hasGuard;
        public bool hasLethal;
        public bool hasWard;

        public SimulationCard(Card card)
        {
            parent = card;

            attack = card.attack;
            health = card.health;

            hasGuard = card.hasGuard;
            hasLethal = card.hasLethal;
            hasWard = card.hasWard;
        }

        public SimulationCard(SimulationCard card)
        {
            parent = card.parent;

            attack = card.attack;
            health = card.health;

            hasGuard = card.hasGuard;
            hasLethal = card.hasLethal;
            hasWard = card.hasWard;
        }

        public static SimulationCard GetSimulationCard(Card target, SimulationGroup group)
        {
            foreach (var trade in group.simulations)
            {
                SimulationCard simTarget;
                if (target.simulationCards.TryGetValue(trade, out simTarget))
                {
                    return simTarget;
                }
            }

            return null;
        }

        public static SimulationCard NewSimulationCard(Card target, Trade playerTrade)
        {
            SimulationCard simTarget;
            if (playerTrade != null && target.simulationCards.TryGetValue(playerTrade, out simTarget))
            {
                return new SimulationCard(simTarget);
            }
            else
            {
                return new SimulationCard(target);
            }
        }
    }
}
