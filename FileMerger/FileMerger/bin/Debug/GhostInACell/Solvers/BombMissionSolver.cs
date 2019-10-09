using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class BombMissionSolver : MissionSolver
    {
        public Mission mission;

        public BombMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            Factory factory = mission.factory;

            foreach (var pair in factory.links.OrderBy(p => p.Value))
            {
                Factory neighbour = pair.Key;
                int distance = pair.Value;

                int missionEndTime = Game.gameTime + distance + 1;

                FactoryState lastState = FactoryState.GetFactoryState(factory, missionEndTime);

                if (lastState.team == Team.Enemy && neighbour.isAlly
                    && factory.production == 3
                    && !Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory)) //no bombs going there
                {
                    int finalArmyCount = lastState.count + (distance + 1) * lastState.production;

                    //Console.Error.WriteLine("Bomb" + lastState.id + (closestTroop.end == factory));

                    if (finalArmyCount >= Factory.globalStats.totalEnemyCount / 2
                        || lastState.production >= Factory.globalStats.enemyTotalProduction / 2)
                    {
                        Bomb bomb = new Bomb
                        {
                            isAlly = true,
                            isEnemy = false,
                            team = Team.Ally,
                            start = neighbour,
                            end = mission.factory,
                            turns = distance + 1,
                            endTime = missionEndTime,
                        };

                        mission.finalEnlistedTroops = new List<Troop>();
                        mission.finalEnlistedTroops.Add(bomb);

                        mission.successRating = MissionSuccessRating.Guaranteed;
                        break;
                    }
                }
            }
        }

        public void Execute()
        {
            if (mission.finalEnlistedTroops.Count > 0)
            {
                Troop troop = mission.finalEnlistedTroops.First();

                Action newAction = new Action(MoveType.Bomb, troop.start, mission.factory, 1);
                Action.AddAction(newAction, troop);
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
