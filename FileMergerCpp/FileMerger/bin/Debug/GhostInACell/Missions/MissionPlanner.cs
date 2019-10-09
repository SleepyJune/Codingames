using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class MissionPlanner
    {
        public Mission mission;

        public MissionPlanner(Mission mission)
        {
            this.mission = mission;
        }

        public void MakeMockTroop(Factory ally, bool isReinforcement = false)
        {
            Troop mockTroop = new Troop
            {
                start = ally,
                count = ally.armyAvailable,
                team = Team.Ally,
                isAlly = true,
                isEnemy = false,
            };

            if (mission.factory.team == Team.Neutral)
            {
                Algorithm.UseBFS(mockTroop, mission.factory, true);
            }
            else
            {
                Algorithm.UseBFS(mockTroop, mission.factory);
            }

            int distance = ally.links[mockTroop.end];
            if (Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory && b.turns > distance))
            {
                return;
            }

            if (mockTroop.end.team != Team.Ally && mockTroop.end != mission.factory)
            {
                //troop travel through enemy land
                if (mockTroop.end.count != 0)
                {
                    Mission newMission = new Mission(MissionType.Capture, mockTroop.end);

                    if (!mission.prereqs.Contains(newMission))
                    {
                        mission.prereqs.Add(newMission);
                    }
                    //return;
                }
            }

            if (ally.oldProduction < 3 && isReinforcement)
            {
                mockTroop.count -= 10;
            }

            if (mockTroop.count > 0)
            {
                mission.acceptedMission.Add(mockTroop); //ally factory accepts the mission
            }

            if (mission.prevMission != null)
            {

            }

            foreach (var troop in mockTroop.end.incoming)
            {
                if (troop.isAlly && troop.start == ally)
                {
                    int distanceTraveled = mockTroop.end.links[ally] - troop.turns;

                    Troop incomingTroop = new Troop
                    {
                        start = ally,
                        end = mockTroop.end,
                        count = troop.count,
                        team = Team.Ally,
                        isAlly = true,
                        isEnemy = false,
                        turns = mockTroop.turns - distanceTraveled - 1,
                        endTime = mockTroop.endTime - distanceTraveled - 1,
                        inTransit = true,
                    };

                    mission.acceptedMission.Add(incomingTroop);
                }
            }
        }
    }
}
