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

                bool allyLock = factory.isAlly;
                int armyAvailable = allyLock ? factory.count : 0;

                var ordered = mockTroops.ToList();

                //iterate through the mock states
                for (int i = 0; i < ordered.Count; i++)
                {
                    Troop troop = ordered[i];
                    FactoryState newState = new FactoryState(lastState, troop);

                    //if two troops arrives at the same time
                    if (i + 1 < ordered.Count && troop.endTime == ordered[i + 1].endTime)
                    {
                        lastState = newState;
                        continue;
                    }

                    if (allyLock && newState.isEnemy)
                    {
                        allyLock = false;
                        break;
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

                if (allyLock)//mission is possible
                {
                    int finalStateCount = mockTestTroop.count - armyAvailable + 1;

                    if (finalStateCount <= 0)
                    {
                        return; //we didn't need to send anything
                    }

                    if (mission.type == MissionType.Capture)//mission.factory.team == Team.Enemy)
                    {
                        if (MissionReward.GetCaptureProbability(mission, 1, mission.missionEndTime - Game.gameTime))
                        {
                            mockTestTroop.count = mockTestTroop.count - armyAvailable + 1; //don't oversend
                            finalStateCount = 1;
                            lastState.count = 1;
                        }
                        else
                        {
                            if (!MissionReward.GetCaptureProbability(mission, finalStateCount, mission.missionEndTime - Game.gameTime))
                            {
                                mission.successRating = MissionSuccessRating.Unlikely;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        mockTestTroop.count = mockTestTroop.count - armyAvailable + 1; //don't oversend
                    }

                    mission.troopsUsed += -armyAvailable + 1;
                    mission.finalState = lastState;

                    mission.successRating = mission.prereqs.Count > 0 ?
                        MissionSuccessRating.HasPrereq :
                        MissionSuccessRating.Possible;

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
            }
        }

        public void RateMission()
        {
            if (mission.type == MissionType.Capture)
            {
                if (mission.missionEndTime - Game.gameTime > 6)
                {
                    mission.successRating = MissionSuccessRating.Unlikely;
                }

                int defendingCount = mission.finalState != null ? mission.finalState.count : 1;
                if (!MissionReward.GetCaptureProbability(mission, defendingCount, mission.missionEndTime - Game.gameTime))
                {
                    mission.successRating = MissionSuccessRating.Unlikely;
                }
            }
        }

        public int CalculateReward()
        {
            //Atk neutral
            //Value[id]/(pow(Turns to get there,2)*Required_To_Take_Neutral(id,Turns to get there))

            //Atk enemy
            //Value[id]/(pow(Turns to get there,2)*8)

            return 0;
        }

    }
}
