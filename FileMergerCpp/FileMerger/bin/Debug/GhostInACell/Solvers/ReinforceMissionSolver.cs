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
            mission.EnlistTroops(mission.type == MissionType.Reinforce);

            int troopsNeeded = 0;
            if (mission.type == MissionType.IncSupport)
            {
                troopsNeeded += 10; //need 10 to increase production
            }

            if (mission.type == MissionType.Reinforce)
            {
                troopsNeeded = mission.acceptedMission.Sum(t => t.count); //send all

                mission.successRating = MissionSuccessRating.Guaranteed;
                mission.finalEnlistedTroops = mission.acceptedMission.ToList();
                mission.missionEndTime = Game.gameTime;//mission.finalEnlistedTroops.Last().endTime;
                return;
            }

            List<Troop> finalEnlisted = new List<Troop>();

            int enlistedCount = 0;
            foreach (var mockTestTroop in mission.acceptedMission)
            {
                enlistedCount += mockTestTroop.count;
                finalEnlisted.Add(mockTestTroop);

                if (enlistedCount >= troopsNeeded)
                {
                    mockTestTroop.count -= (enlistedCount - troopsNeeded);

                    mission.successRating = MissionSuccessRating.Guaranteed;
                    mission.finalEnlistedTroops = finalEnlisted;
                    mission.missionEndTime = mockTestTroop.endTime;

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

            if (mission.type == MissionType.IncSupport)
            {
                mission.factory.armyAvailable = 0;//u called for support, don't even try to send army
            }
        }

        public void RateMission()
        {

        }

        public int CalculateReward()
        {
            return 0;
        }
    }
}
