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
        FinalReinforce,
    }

    class Mission : IComparable<Mission>
    {
        public MissionType type;
        public Factory factory;
        public int troopsNeeded;

        public SortedSet<Troop> acceptedMission;
        public List<Troop> finalEnlistedTroops;

        public double reward;

        public Dictionary<Mission, HashSet<Troop>> prereqs;

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
            this.prereqs = new Dictionary<Mission, HashSet<Troop>>();

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
                    solver = new IncSupportMissionSolver(this);
                    break;
                case MissionType.Reinforce:
                    solver = new ReinforceMissionSolver(this);
                    break;
                case MissionType.FinalReinforce:
                    solver = new FinalReinforceMissionSolver(this);
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
            prereqs = new Dictionary<Mission, HashSet<Troop>>();

            acceptedMission = new SortedSet<Troop>();
            successRating = MissionSuccessRating.Impossible;
            finalEnlistedTroops = new List<Troop>();
        }

        public void EnlistTroops(bool isReinforcement = false)
        {
            foreach (var ally in Factory.ally)
            {
                if (factory != ally)
                {
                    planner.MakeMockTroop(ally, isReinforcement);
                }
            }

            planner.AddInTransitTroops();
        }

        public bool isPossible()
        {
            return successRating >= MissionSuccessRating.Possible;
        }

        public int CompareTo(Mission missionB)
        {
            return this.reward.CompareTo(missionB.reward);            
        }

        public override bool Equals(object obj)
        {
            Mission mission = obj as Mission;
            return this.type == mission.type && this.factory == mission.factory;
        }

        public override int GetHashCode()
        {
            return ((int)type * 100) + factory.id;
        }
    }
}
