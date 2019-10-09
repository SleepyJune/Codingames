using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class CaptureMissionSolver : MissionSolver
    {
        public Mission mission;

        public CaptureMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            mission.EnlistTroops();

            if (mission.acceptedMission.Count == 0)
            {
                mission.successRating = MissionSuccessRating.Impossible;
                return;
            }

            SortedSet<Troop> mockTroops = new SortedSet<Troop>(mission.factory.incoming);
            List<Troop> finalEnlisted = new List<Troop>();
            Factory factory = mission.factory;

            foreach (var mockTestTroop in mission.acceptedMission)
            {
                mockTroops.Add(mockTestTroop);
                finalEnlisted.Add(mockTestTroop);
                mission.troopsUsed += mockTestTroop.count;
                mission.missionEndTime = mockTestTroop.endTime;

                FactoryState lastState
                        = new FactoryState(factory, Game.gameTime);

                bool turnedEnemy = factory.isEnemy;
                bool allyLock = factory.isAlly;
                int armyAvailable = allyLock ? factory.count : 0;

                var ordered = mockTroops.ToList();

                if (mission.prereqs.Values.Any(r => r.Contains(mockTestTroop)))
                {
                    mission.successRating = MissionSuccessRating.HasPrereq;
                    mission.finalEnlistedTroops = finalEnlisted;
                    break;
                }

                //iterate through the mock states
                for (int i = 0; i < ordered.Count; i++)
                {
                    Troop troop = ordered[i];
                    FactoryState newState = new FactoryState(lastState, troop, false);

                    //if two troops arrives at the same time
                    if (i + 1 < ordered.Count && troop.endTime == ordered[i + 1].endTime)
                    {
                        newState = new FactoryState(lastState, troop, true);
                        lastState = newState;
                        continue;
                    }

                    if (newState.isEnemy)
                    {
                        turnedEnemy = true;

                        if (allyLock)
                        {
                            allyLock = false;
                            //break;
                        }
                    }

                    if (allyLock == false && newState.isAlly)
                    {
                        allyLock = true;
                        armyAvailable = newState.count;
                    }

                    if (factory.isAlly && troop == mockTestTroop)
                    {
                        armyAvailable = newState.count;
                    }

                    armyAvailable = Math.Min(armyAvailable, newState.count);
                    lastState = newState;
                }

                if (lastState.isAlly)//mission is possible
                {
                    int finalStateCount = mockTestTroop.count - armyAvailable + 1;

                    if (finalStateCount <= 0)
                    {
                        return; //we didn't need to send anything
                    }

                    if (mission.type == MissionType.Capture && turnedEnemy)
                    {
                        if (mission.acceptedMission.Sum(troop => troop.count) >= mission.factory.count)
                        {
                            mission.successRating = MissionSuccessRating.Possible;

                            mission.finalEnlistedTroops = new List<Troop>(mission.acceptedMission);
                            return;
                        }
                    }
                    else
                    {
                        mockTestTroop.count = mockTestTroop.count - armyAvailable + 1; //don't oversend
                    }


                    mission.troopsUsed += -armyAvailable + 1;
                    mission.finalState = lastState;

                    mission.successRating = MissionSuccessRating.Possible;

                    mission.finalEnlistedTroops = finalEnlisted;

                    return; //job's done
                }
                else
                {
                    mission.successRating = MissionSuccessRating.NotEnoughTroops;
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
                else
                {
                    troop.original.armyAvailable = Math.Max(0, troop.original.armyAvailable - troop.count);
                }
            }
        }

        public void RateMission()
        {
            if (mission.type == MissionType.Capture)
            {
                /*if (mission.missionEndTime - Game.gameTime > 8)
                {
                    mission.successRating = MissionSuccessRating.Unlikely;
                }*/

                /*if (mission.successRating == MissionSuccessRating.Possible
                    && mission.factory.isEnemy
                    && !GetCaptureProbability())
                {
                    mission.successRating = MissionSuccessRating.Unlikely;
                }*/
            }
        }

        public bool GetCaptureProbability()
        {
            int attackingArmy = mission.finalEnlistedTroops.Sum(t => t.end == mission.factory ? t.count : 0);
            int maxEnemyTroopCount = 0;
            int turns = mission.missionEndTime - Game.gameTime;
            Troop nearestAttacker = mission.finalEnlistedTroops.FirstOrDefault(t=> t.end == mission.factory);

            if (nearestAttacker != null)
            {
                turns = nearestAttacker.turns;
            }

            foreach (var pair in mission.factory.shortestPaths.shortestDistance)
            {
                Factory neighbour = pair.Key;
                int distance = pair.Value;

                if (distance <= turns)
                {
                    if (neighbour.team == Team.Enemy)
                    {
                        int production = Math.Max(0, neighbour.production * (distance - turns - 1));
                        maxEnemyTroopCount += neighbour.count + production;
                    }
                }
            }

            return attackingArmy > maxEnemyTroopCount;
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

            if (mission.type == MissionType.Capture)
            {
                if (mission.factory.isNeutral)
                {
                    //Atk neutral
                    //Value[id]/(pow(Turns to get there,2)*Required_To_Take_Neutral(id,Turns to get there))

                    double troopUsed = (double)(Math.Max(1, mission.factory.states.Last().count)) / (mission.factory.oldProduction + 1);

                    double finalScore = value / (Math.Pow(turns, 3) * troopUsed);

                    mission.reward = finalScore;

                }
                else
                {
                    //Atk enemy
                    //Value[id]/(pow(Turns to get there,2)*8)

                    /*int enemyProduction = 0;
                    foreach (var pair in mission.factory.links)
                    {
                        var enemy = pair.Key;
                        int distance = pair.Value;

                        if (enemy.isEnemy && distance < turns)
                        {
                            enemyProduction += enemy.production;
                        }
                    }

                    double finalTroopUsed = enemyProduction + mission.acceptedMission.Sum(T => T.count);
                    */



                    double finalScore = value / (Math.Pow(turns, 3) * 8);

                    mission.reward = finalScore;
                }
            }

            if (mission.type == MissionType.Defend)
            {
                //Defend
                //Value[id]/(pow(Turns to get there,2)*Needed_Reinforcements(id,Turns to get there+5))

                double finalScore = value / (Math.Pow(turns, 2) * mission.troopsUsed);

                mission.reward = finalScore;
            }

        }

    }
}
