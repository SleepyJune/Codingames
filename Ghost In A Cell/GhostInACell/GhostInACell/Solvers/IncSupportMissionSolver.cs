using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class IncSupportMissionSolver : MissionSolver
    {
        public Mission mission;

        public IncSupportMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            mission.EnlistTroops();

            int troopsNeeded = 10 - mission.factory.armyAvailable;

            List<Troop> finalEnlisted = new List<Troop>();

            int enlistedCount = 0;
            foreach (var mockTestTroop in mission.acceptedMission)
            {
                var turns = mockTestTroop.turns;
                var production = turns * mission.factory.production;

                mission.missionEndTime = mockTestTroop.endTime;

                if (production >= troopsNeeded)
                {
                    mission.successRating = MissionSuccessRating.Guaranteed;
                    mission.finalEnlistedTroops = finalEnlisted;
                    return; //nothing to do here
                }

                enlistedCount += mockTestTroop.count;
                finalEnlisted.Add(mockTestTroop);
                
                if (enlistedCount + production >= troopsNeeded)
                {                    
                    mockTestTroop.count -= Math.Max(0,(enlistedCount - (troopsNeeded + production)));

                    mission.successRating = MissionSuccessRating.Guaranteed;
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

            if (mission.type == MissionType.IncSupport)
            {
                mission.factory.armyAvailable = 0;//u called for support, don't even try to send army
            }
        }

        public void RateMission()
        {

        }

        public void CalculateReward()
        {
            if (mission.type == MissionType.IncSupport)
            {
                int turns = mission.missionEndTime + 11;

                double value = 1 + 0.01 * mission.factory.armyAvailable;

                double finalScore = value / (Math.Pow(11, 2) * 10);
                mission.reward = finalScore;
            }
        }
    }
}
