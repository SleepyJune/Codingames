using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class FactoryState
    {
        public int id;
        public Team team;
        public bool isAlly;
        public bool isEnemy;

        public int count;
        public int production;

        public int oldProduction;
        public int disabledTime;

        public int gameTime = 0;

        public Troop troop;

        public FactoryState(Factory factory, int gameTurns)
        {
            this.id = factory.id;
            SetTeam(factory.team);

            this.count = factory.count;
            this.production = factory.production;
            this.gameTime = gameTurns;

            this.oldProduction = factory.production;
            this.disabledTime = factory.disabledTurns;
        }

        public FactoryState(FactoryState factory, Troop troop)
        {
            id = factory.id;
            count = factory.count;
            production = factory.disabledTime > 0 ? 0 : factory.oldProduction;
            oldProduction = factory.oldProduction;
            gameTime = troop.endTime;
            this.troop = troop;
            disabledTime = Math.Max(0, factory.disabledTime--);
            SetTeam(factory.team); //team stays the same (default = neutral)

            if (factory.team != Team.Neutral)
            {
                int timeDiff = troop.endTime - factory.gameTime;
                this.count += timeDiff * production;
            }

            if (troop is Bomb)
            {
                int bombCount = Math.Max(this.count / 2, 10);
                this.count -= bombCount;
                this.count = Math.Max(0, this.count);

                this.oldProduction = Math.Max(oldProduction, production);
                this.production = 0;
                this.disabledTime = troop.endTime + 3;

                //Console.Error.WriteLine("Bomb " + troop.id + ": " + this.count);
            }
            else
            {
                this.count += (troop.team == factory.team) ? troop.count : -troop.count;

                if (this.count < 0) //if the troop captured the factory
                {
                    this.count *= -1;
                    SetTeam(troop.team); //team become the troops team
                }
            }
        }

        public static List<FactoryState> CalculateFactoryState(Factory factory, SortedSet<Troop> troops, bool mockState = true)
        {
            List<FactoryState> states = new List<FactoryState>();

            FactoryState lastState
                    = new FactoryState(factory, Game.gameTime);

            states.Add(lastState); //add atleast 1 state to the list

            foreach (var troop in troops)
            {
                FactoryState newState = new FactoryState(lastState, troop);
                states.Add(newState);

                if (mockState == false)
                {
                    if (newState.isAlly)
                    {
                        factory.armyAvailable = Math.Min(factory.armyAvailable, newState.count);
                    }
                    else
                    {
                        factory.armyAvailable = Math.Min(factory.armyAvailable, -newState.count);
                    }
                }

                lastState = newState;
            }

            return states;
        }

        public static bool IsFactoryCaptured(List<FactoryState> states)
        {
            Team originalTeam = states.First().team;
            for (int i = 0; i < states.Count; i++)
            {
                FactoryState state = states[i];

                if (i + 1 < states.Count && state.gameTime == states[i + 1].gameTime) //if states occur at same time
                {
                    continue;
                }

                if (originalTeam != state.team)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool WillFactoryBeAlly(List<FactoryState> states)
        {
            FactoryState firstState = states.First();
            bool allyLock = firstState.isAlly;

            for (int i = 0; i < states.Count; i++)
            {
                FactoryState state = states[i];

                if (i + 1 < states.Count && state.gameTime == states[i + 1].gameTime) //if states occur at same time
                {
                    continue;
                }

                if (allyLock && state.isEnemy)
                {
                    return false;
                }

                if (allyLock == false && state.isAlly)
                {
                    allyLock = true;
                }
            }

            return allyLock;
        }

        public static FactoryState GetFactoryState(Factory factory, int gameTime)
        {
            Troop testTroop = new Troop
            {
                start = factory,
                end = factory,
                count = 0,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = gameTime - Game.gameTime,
                endTime = gameTime,
            };

            return GetFactoryState(factory, testTroop);
        }

        public static FactoryState GetFactoryState(Factory factory, Troop testTroop)
        {
            SortedSet<Troop> mockTroops = new SortedSet<Troop>(factory.incoming);
            mockTroops.Add(testTroop);

            var states = FactoryState.CalculateFactoryState(factory, mockTroops);
            foreach (var state in states)
            {
                if (state.troop != null && state.troop.id == testTroop.id)
                {
                    return state;
                }
            }

            return null;
        }

        public void SetTeam(Team team)
        {
            this.team = team;
            if (team == Team.Ally)
            {
                isAlly = true;
                isEnemy = false;
            }
            else if (team == Team.Enemy)
            {
                isAlly = false;
                isEnemy = true;
            }
            else
            {
                isAlly = false;
                isEnemy = true;
            }
        }
    }
}
