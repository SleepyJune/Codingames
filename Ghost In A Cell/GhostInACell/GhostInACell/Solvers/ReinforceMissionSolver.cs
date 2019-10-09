using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class ReinforceMissionSolver : MissionSolver
    {
        public Mission mission;

        public ReinforceMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            mission.EnlistTroops();

            mission.troopsNeeded = mission.factory.GetNearbyEnemyThreats();

            if (mission.troopsNeeded <= 0)
            {
                mission.successRating = MissionSuccessRating.Impossible;
                return;
            }

            List<Troop> finalEnlisted = new List<Troop>();

            foreach (var troop in mission.acceptedMission)
            {
                mission.troopsNeeded -= troop.count;
                finalEnlisted.Add(troop);

                if (mission.troopsNeeded <= 0)
                {
                    troop.count = troop.count + mission.troopsNeeded;
                    mission.successRating = MissionSuccessRating.Possible;
                    mission.finalEnlistedTroops = finalEnlisted;
                    return;
                }
            }            
        }

        public void Execute()
        {
            foreach (Troop troop in mission.finalEnlistedTroops)
            {
                if (troop.inTransit == false)
                {
                    Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
                    Action.AddAction(newAction, troop);
                }
            }
        }

        public void RateMission()
        {

        }

        public void CalculateReward()
        {
            double value = mission.factory.oldProduction
                + 0.01 * Math.Pow(mission.factory.pathValue, 0.5) + 0.1;

            int turns = 0;//mission.missionEndTime - Game.gameTime;

            foreach (var ally in Factory.ally)
            {
                turns = Math.Max(turns, ally.shortestPaths.shortestDistance[mission.factory]);
            }

            double finalScore = value / (Math.Pow(turns, 2));

            mission.reward = finalScore;
        }
    }
}
