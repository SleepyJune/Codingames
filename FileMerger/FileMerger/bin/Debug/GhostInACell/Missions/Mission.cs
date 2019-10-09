using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    public enum MissionSuccessRating
    {
        Impossible,
        HasPrereq,
        NotEnoughTroops,
        Unlikely,
        Possible,
        Guaranteed,
    }

    public enum MissionType
    {
        Bomb,
        Defend,
        Capture,
        Inc,
        IncSupport,
        Reinforce,
    }

    class Mission : IComparable<Mission>
    {
        public MissionType type;
        public Factory factory;
        public int troopsNeeded;

        public SortedSet<Troop> acceptedMission;
        public List<Troop> finalEnlistedTroops;

        public MissionReward reward;

        public HashSet<Mission> prereqs;

        public FactoryState finalState;

        public Mission prevMission;

        public int troopsUsed;

        public bool missionExecuted = false;

        public MissionSuccessRating successRating;
        public int missionEndTime;

        public MissionSolver solver;
        public MissionPlanner planner;

        public Mission(MissionType type, Factory factory)
        {
            this.type = type;
            this.factory = factory;
            this.acceptedMission = new SortedSet<Troop>();
            this.prereqs = new HashSet<Mission>();

            this.planner = new MissionPlanner(this);

            switch (type)
            {
                case MissionType.Capture:
                    solver = new CaptureMissionSolver(this);
                    break;
                case MissionType.Defend:
                    solver = new CaptureMissionSolver(this);
                    break;
                case MissionType.IncSupport:
                    solver = new ReinforceMissionSolver(this);
                    break;
                case MissionType.Reinforce:
                    solver = new ReinforceMissionSolver(this);
                    break;
                case MissionType.Inc:
                    solver = new IncreaseMissionSolver(this);
                    break;
                case MissionType.Bomb:
                    solver = new BombMissionSolver(this);
                    break;
                default:
                    break;
            }
        }

        public void CleanUp()
        {
            prereqs = new HashSet<Mission>();

            acceptedMission = new SortedSet<Troop>();
            successRating = MissionSuccessRating.Impossible;
        }

        public void EnlistTroops(bool isReinforcement = false)
        {
            foreach (var ally in Factory.ally)
            {
                if (factory != ally && ally.armyAvailable > 0)
                {
                    planner.MakeMockTroop(ally, isReinforcement);
                }
            }
        }

        public bool isPossible()
        {
            return successRating >= MissionSuccessRating.Possible;
        }

        public int CompareTo(Mission missionB)
        {
            Mission missionA = this;

            MissionReward rewardA = missionA.reward;
            MissionReward rewardB = missionB.reward;

            return rewardA.combinedReward.CompareTo(rewardB.combinedReward);
        }

        public override bool Equals(object obj)
        {
            Mission mission = obj as Mission;
            return this.type == mission.type && this.factory == mission.factory;
        }

        public override int GetHashCode()
        {
            return (int)type * factory.id;
        }
    }
}
