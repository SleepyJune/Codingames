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

        public void GetTroopPath(Troop troop, Factory end)
        {
            Factory start = troop.start;
            int totalDistance = 0;

            int directDistance = 0;
            if (start.links.TryGetValue(end, out directDistance))
            {
                if (directDistance <= 3)
                {
                    troop.end = end;
                    troop.turns = directDistance + 1;
                    troop.endTime = Game.gameTime + troop.turns;
                    return;
                }
            }

            var prevFactory = start.optimalPaths.previousFactory;
            Factory parent = prevFactory[end];

            while (parent != start)
            {
                totalDistance += end.links[parent] + 1;
                end = parent;
                parent = prevFactory[end];
            }
            totalDistance += end.links[parent] + 1;

            troop.turns = totalDistance + 1;
            troop.endTime = Game.gameTime + troop.turns;
            troop.end = end;
        }

        public void AddPrereq(Troop troop)
        {
            Mission newMission = new Mission(MissionType.Capture, troop.end);
            HashSet<Troop> reqTroops;

            if (Strategy.curExecuted.Contains(newMission)
                || FactoryState.WillFactoryBeAlly(troop.end.states))
            {
                return; //already cleared prereq
            }

            if (mission.prereqs.TryGetValue(newMission, out reqTroops))
            {
                reqTroops.Add(troop);
            }
            else
            {
                mission.prereqs.Add(newMission, new HashSet<Troop> { troop });
            }
        }

        public void AddInTransitTroops()
        {
            foreach (var troop in Troop.ally)
            {
                if (troop.isAlly && troop.end != mission.factory 
                    && troop.end.isAlly && troop.original == null)
                {
                    Troop incomingTroop = new Troop
                    {
                        start = troop.end,
                        end = mission.factory,
                        count = troop.armyAvailable,
                        team = Team.Ally,
                        isAlly = true,
                        isEnemy = false,
                        inTransit = true,
                        original = troop,
                    };

                    GetTroopPath(incomingTroop, mission.factory);

                    incomingTroop.end = troop.end;
                    incomingTroop.start = troop.start;
                    incomingTroop.turns = troop.turns + incomingTroop.turns + 1;
                    incomingTroop.endTime = Game.gameTime + incomingTroop.turns;

                    mission.acceptedMission.Add(incomingTroop);
                }
            }
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

            if (mission.factory == ally)
            {
                return;
            }

            var nearbyThreats = ally.GetNearbyEnemyThreats2(mission.factory);
            if (nearbyThreats > 0 && ally != mission.factory)
            {
                //mockTroop.count -= Math.Max(0, nearbyThreats);
            }

            GetTroopPath(mockTroop, mission.factory);

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
                    AddPrereq(mockTroop);

                    if (isReinforcement)
                    {
                        return;
                    }

                }
            }

            if (ally.oldProduction < 3 && isReinforcement)
            {
                mockTroop.count -= 10;
            }

            if (mockTroop.count > 0)// && ally.GetNearbyEnemyThreats() == 0)
            {
                mission.acceptedMission.Add(mockTroop); //ally factory accepts the mission
            }

            if (mission.prevMission != null)
            {

            }            
        }
    }
}
