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
                if (FactoryState.WillFactoryBeAlly(factory.states))
                {
                    if (factory.oldProduction < 3 && factory.armyAvailable >= 10)
                    {
                        Mission newMission = new Mission(MissionType.Inc, factory);
                        missions.Add(newMission);
                    }
                    else if (factory.oldProduction < 3)
                    {
                        Mission newMission = new Mission(MissionType.IncSupport, factory);
                        missions.Add(newMission);
                    }
                }
                else
                {
                    if (Game.bomb > 0 && Game.gameTime > 1)
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
                        Mission newMission = new Mission(MissionType.Capture, factory);
                        missions.Add(newMission);
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
            foreach (var prereq in mission.prereqs)
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
                if (mission.reward.combinedReward > 0 &&
                    mission.successRating == MissionSuccessRating.NotEnoughTroops)
                {
                    return;
                }
            }

            var ordered = Factory.ally.OrderBy(ally => ally.nearestEnemyDistance)
                        .ThenByDescending(ally => ally.oldProduction);

            if (ordered.First() != null)
            {
                Mission newMission = new Mission(MissionType.Reinforce, ordered.First());

                missions.Add(newMission);
                newMission.solver.Solve();

                newMission.missionExecuted = true;
                newMission.solver.Execute();
            }
        }

        public static void ExecuteFinalReinforcement2()
        {
            foreach (var mission in missions)
            {
                if (mission.missionExecuted)
                {
                    continue;
                }

                Mission newMission =
                    mission.factory.isAlly ?
                    new Mission(MissionType.Reinforce, mission.factory) :
                    new Mission(MissionType.Reinforce, mission.factory.GetNearestAlly());

                missions.Add(newMission);
                newMission.solver.Solve();

                newMission.missionExecuted = true;
                newMission.solver.Execute();

                //Console.Error.WriteLine("Final Reinforce: " + newMission.factory.id);
                return;
            }
        }

        public static bool CheckLaterRewards(int index)
        {
            double rewards = 0;

            if (index + 1 >= missions.Count)
            {
                return false;
            }

            for (int i = index + 1; i < missions.Count; i++)
            {
                rewards += Math.Max(0, missions[i].reward.combinedReward);
            }

            double currentReward = missions[index].reward.combinedReward;

            return rewards >= currentReward;
        }

        public static bool ExecuteMissions()
        {
            Strategy.SolveMissions();

            missions.Sort();
            missions.Reverse(); //highest reward first

            foreach (var mission in missions)
            {
                Console.Error.WriteLine(mission.type.ToString() + " " + mission.factory.id + ": " + mission.successRating.ToString()
                    + " " + mission.reward.combinedReward);
            }

            for (int i = 0; i < missions.Count; i++)
            {
                Mission mission = missions[i];

                if (mission.missionExecuted)
                {
                    continue;
                }

                if (mission.isPossible() && ClearedPrereq(mission))
                {
                    mission.missionExecuted = true;
                    mission.solver.Execute();
                    curExecuted.Add(mission);

                    Console.Error.WriteLine("Execute " + mission.type.ToString() + " " + mission.factory.id + "\n");

                    //string message = mission.type.ToString() + " " + mission.factory.id;
                    //Action.AddAction(new Action(message));

                    return true;
                }

                if (mission.successRating == MissionSuccessRating.NotEnoughTroops)
                {
                    if (Strategy.CheckLaterRewards(i) == false)
                    {
                        break;
                    }
                }
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
