using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class IncreaseMissionSolver : MissionSolver
    {
        public Mission mission;

        public IncreaseMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            Factory ally = mission.factory;

            SortedSet<Troop> mockTroops = new SortedSet<Troop>(ally.incoming);
            Troop testTroop = new Troop
            {
                start = ally,
                end = ally,
                count = 10,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = 1,
                endTime = Game.gameTime + 1,
            };
            mockTroops.Add(testTroop);

            var mockStates = FactoryState.CalculateFactoryState(ally, mockTroops);
            //FactoryState lastState = mockStates.Last();

            if (FactoryState.IsFactoryCaptured(mockStates) == false)
            {
                mission.successRating = MissionSuccessRating.Guaranteed;
                mission.acceptedMission.Add(testTroop);
                mission.missionEndTime = Game.gameTime + 1;
            }
        }

        public void Execute()
        {
            Action newAction = new Action(MoveType.Inc, mission.factory);
            Action.AddAction(newAction);
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
