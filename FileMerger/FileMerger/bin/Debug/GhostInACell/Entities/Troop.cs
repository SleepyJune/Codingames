using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class Troop : Entity, IEquatable<Troop>, IComparable<Troop>
    {
        public static HashSet<Troop> troops = new HashSet<Troop>();
        public static HashSet<Troop> ally = new HashSet<Troop>();
        public static HashSet<Troop> enemy = new HashSet<Troop>();

        public static Dictionary<string, Troop> hashTroops = new Dictionary<string, Troop>();

        public static int idCount = 0;

        public static int lastEndTime = 0;

        public int id;

        public Team team;
        public Factory start;
        public Factory end;
        public int count;
        public int turns;

        public int endTime;

        public bool isAlly;
        public bool isEnemy;

        public bool inTransit;

        public Troop()
        {
            this.id = idCount++;
        }

        public static void CleanUp()
        {
            troops = new HashSet<Troop>();
            ally = new HashSet<Troop>();
            enemy = new HashSet<Troop>();

            lastEndTime = 0;
        }

        public void ProcessMessage(EntityMessage message)
        {
            if (message.arg1 == 1)
            {
                SetTeam(Team.Ally);
            }
            else
            {
                SetTeam(Team.Enemy);
            }

            start = Factory.GetFactoryByID(message.arg2);
            end = Factory.GetFactoryByID(message.arg3);
            count = message.arg4;
            turns = message.arg5;
            endTime = Game.gameTime + turns;

            inTransit = true;

            end.incoming.Add(this);
            //hashTroops.Add(this.GetStringKey(), this);

            lastEndTime = Math.Max(lastEndTime, endTime); //get the last troop endTime;
        }

        public void SetTeam(Team team)
        {
            this.team = team;
            if (team == Team.Ally)
            {
                ally.Add(this);
                isAlly = true;
                isEnemy = false;

                Factory.globalStats.totalAllyCount += this.count;
            }

            if (team == Team.Enemy)
            {
                enemy.Add(this);
                isAlly = false;
                isEnemy = true;

                Factory.globalStats.totalEnemyCount += this.count;
            }
        }

        public static Troop GetTroopByID(int id)
        {
            Troop troop = new Troop();
            troops.Add(troop);
            return troop;
        }

        public string GetStringKey()
        {
            return this.start.id + " " + this.end.id + " " + this.endTime + " " + this.count;
        }

        public override bool Equals(object obj)
        {
            Troop troop = obj as Troop;
            return troop.id == this.id;
        }

        public override int GetHashCode()
        {
            return this.id;
        }

        public bool Equals(Troop troop)
        {
            return troop.id == this.id;
        }

        public int CompareTo(Troop troop)
        {
            var compare = this.endTime.CompareTo(troop.endTime);
            if (compare == 0) //if end time is the same
            {
                return this.id.CompareTo(troop.id);
            }
            else
            {
                return compare;
            }
        }
    }
}
