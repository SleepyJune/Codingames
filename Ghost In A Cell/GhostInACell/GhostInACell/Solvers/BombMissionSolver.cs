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

            mission.finalEnlistedTroops = new List<Troop>();

            if (Game.bomb <= 0)
            {
                mission.successRating = MissionSuccessRating.Impossible;
                return;
            }

            foreach (var pair in factory.links.OrderBy(p => p.Value))
            {
                Factory neighbour = pair.Key;
                int distance = pair.Value;

                int missionEndTime = Game.gameTime + distance + 1;

                FactoryState lastState = FactoryState.GetFactoryState(factory, missionEndTime);

                if (lastState.team == Team.Enemy && neighbour.isAlly
                    && !Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory)) //no bombs going there
                {
                    int finalArmyCount = lastState.count + (distance + 1) * lastState.production;


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

                    /*int inTransitEnemyCount = 0;
                    foreach (var troop in Troop.enemy.Where(t=>t.end == mission.factory).OrderBy(t=>t.endTime))
                    {
                        if (distance == troop.turns)
                        {
                            inTransitEnemyCount += troop.count;
                        }
                    }*/

                    if (lastState.production >= 2)//Factory.globalStats.enemyTotalProduction / 2
                        //|| lastState.count >= Factory.globalStats.totalEnemyCount / 2)
                    {
                        bomb.count = lastState.count;
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

        public void CalculateReward()
        {
            if (mission.finalEnlistedTroops.Count > 0)
            {
                Troop bomb = mission.finalEnlistedTroops.First();

                //pow(production,3)/Time_To_Bomb
                mission.reward = Math.Pow(mission.factory.production, 3) / bomb.turns;
                //bomb.count
            }
            else
            {
                mission.reward = 0;
            }            
        }
    }
}
