using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class Strategy
    {
        public static List<Mission> missions = new List<Mission>();
        public static List<Mission> curExecuted = new List<Mission>();
        public static List<Mission> prevExecuted = new List<Mission>();

        public static void AddMissions()
        {
            var orderedFactories = Factory.factories
                .OrderByDescending(factory =>
                    factory.stats.allyScore * factory.oldProduction);

            foreach (var factory in orderedFactories)
            {
                //if the factory will always be ally's
                if (factory.isAlly && FactoryState.WillFactoryBeAlly(factory.states))
                {
                    int nearbyEnemyThreatCount = factory.GetNearbyEnemyThreats();
                    if (nearbyEnemyThreatCount == 0)
                        //&& Factory.globalStats.totalAllyCount - 10 >= (2*Factory.globalStats.totalEnemyCount/3.0))
                    {
                        if (factory.oldProduction < 3 && factory.armyAvailable >= 10)
                        {
                            Mission newMission = new Mission(MissionType.Inc, factory);
                            missions.Add(newMission);
                        }
                        else if (factory.oldProduction < 3) //send inc support before becoming ally's?
                        {
                            Mission newMission = new Mission(MissionType.IncSupport, factory);
                            missions.Add(newMission);
                        }
                    }
                    else
                    {
                        if (nearbyEnemyThreatCount > 0)
                        {
                            Mission newMission = new Mission(MissionType.Reinforce, factory);
                            newMission.troopsNeeded = nearbyEnemyThreatCount;
                            missions.Add(newMission);
                        }                        
                    }
                }
                else
                {
                    if (Game.bomb > 0 && Game.gameTime > 0)
                    {
                        Mission bombMission = new Mission(MissionType.Bomb, factory);
                        missions.Add(bombMission);
                    }

                    if (factory.isAlly)
                    {
                        Mission newMission = new Mission(MissionType.Defend, factory);
                        missions.Add(newMission);
                    }
                    else
                    {
                        if (!FactoryState.WillFactoryBeAlly(factory.states))
                        {
                            Mission newMission = new Mission(MissionType.Capture, factory);
                            missions.Add(newMission);
                        }
                    }


                    //double missionScore = factory.stats.allyScore * factory.oldProduction;
                    //Console.Error.WriteLine(factory.id + ": " + missionScore);
                    //return;
                }
            }
        }

        public static void SolveMissions()
        {
            foreach (var mission in missions)
            {
                if (mission.missionExecuted)
                {
                    continue;
                }

                if (mission.type == MissionType.Inc || mission.type == MissionType.IncSupport)
                {
                    if (missions.Any(m => m.type == MissionType.Reinforce))
                    {
                        mission.successRating = MissionSuccessRating.Impossible;
                        continue;
                    }
                }

                foreach (var prevMission in prevExecuted)
                {
                    if (prevMission == mission)
                    {
                        mission.prevMission = prevMission;
                    }
                }

                mission.CleanUp();
                mission.solver.Solve();
                mission.solver.RateMission();
                mission.solver.CalculateReward();
                //calculate reward if waited 1 turn
            }
        }

        public static bool ClearedPrereq(Mission mission)
        {
            foreach (var prereq in mission.prereqs.Keys)
            {
                if (!missions.Any(m => m == prereq))
                {
                    return false;
                }
            }

            return true;
        }

        public static void ExecuteFinalReinforcement()
        {
            foreach (var mission in missions)
            {
                if (mission.missionExecuted)
                {
                    continue;
                }

                Mission newMission =
                    mission.factory.isAlly ?
                    new Mission(MissionType.FinalReinforce, mission.factory) :
                    new Mission(MissionType.FinalReinforce, mission.factory.GetNearestAlly());

                if (mission.factory != null)
                {
                    missions.Add(newMission);
                    newMission.solver.Solve();

                    newMission.missionExecuted = true;
                    newMission.solver.Execute();

                    Console.Error.WriteLine("Final Reinforce: " + newMission.factory.id);
                    return;
                }
            }
        }

        public static bool CheckLaterRewards(int index)
        {
            while (index + 1 >= missions.Count)
            {
                double currentReward = missions[index].reward;
                double nextReward = missions[index + 1].reward;

                return currentReward >= 2 * nextReward;
            }

            return false;
        }

        public static bool CheckBomb(Mission mission)
        {
            if (mission.type == MissionType.Bomb)
            {
                if (missions.Any(m => m.type != MissionType.Bomb && m.isPossible()))
                {
                    //return false;
                }
            }

            return true;
        }

        public static bool ExecuteMissions()
        {
            Strategy.SolveMissions();

            missions.Sort();
            missions.Reverse(); //highest reward first

            foreach (var mission in missions)
            {
                Console.Error.WriteLine(mission.type.ToString() + " " + mission.factory.id + ": " + mission.successRating.ToString()
                    + " " + mission.reward);
            }

            for (int i = 0; i < missions.Count; i++)
            {
                Mission mission = missions[i];

                if (mission.missionExecuted)
                {
                    continue;
                }

                if (mission.isPossible())
                {
                    Console.Error.WriteLine("Execute " + mission.type.ToString() + " " + mission.factory.id);// + "\n");
                    
                    mission.missionExecuted = true;
                    mission.solver.Execute();
                    curExecuted.Add(mission);
                  

                    //string message = mission.type.ToString() + " " + mission.factory.id;
                    //Action.AddAction(new Action(message));

                    return true;
                }

                /*if (Strategy.CheckLaterRewards(i))
                {
                    break;
                }*/
            }

            ExecuteFinalReinforcement();

            return false;
        }

        public static void CleanUp()
        {
            missions = new List<Mission>();

            prevExecuted = curExecuted;
            curExecuted = new List<Mission>();
        }
    }
}
