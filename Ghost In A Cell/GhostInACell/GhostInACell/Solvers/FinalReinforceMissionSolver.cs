using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class FinalReinforceMissionSolver : MissionSolver
    {
        public Mission mission;

        public FinalReinforceMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            mission.EnlistTroops(mission.type == MissionType.FinalReinforce);

            var troopsNeeded = mission.acceptedMission.Sum(t => t.count); //send all

            mission.successRating = MissionSuccessRating.Guaranteed;
            mission.finalEnlistedTroops = mission.acceptedMission.ToList();
            mission.missionEndTime = Game.gameTime;//mission.finalEnlistedTroops.Last().endTime;
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
            if (mission.type == MissionType.IncSupport)
            {
                int turns = mission.missionEndTime;

                double finalScore = 1 / (Math.Pow(turns, 1.6) * 5);
                mission.reward = finalScore;
            }
        }
    }
}
