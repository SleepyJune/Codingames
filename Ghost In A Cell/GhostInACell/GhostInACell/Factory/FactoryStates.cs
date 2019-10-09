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
        public bool isNeutral;

        public int count;
        public int production;

        public int oldProduction;
        public int disabledEndTime;

        public int gameTime = 0;

        public Troop troop;

        public FactoryState(Factory factory, int gameTurns)
        {
            this.id = factory.id;
            SetTeam(factory.team);

            this.count = factory.count;
            this.production = factory.production;
            this.gameTime = gameTurns;

            this.oldProduction = factory.oldProduction;
            this.disabledEndTime = Game.gameTime + factory.disabledTurns;
        }

        public FactoryState(FactoryState factory, Troop troop, bool sameTime)
        {
            id = factory.id;
            count = factory.count;
            gameTime = troop.endTime;
            production = gameTime >= factory.disabledEndTime ? factory.oldProduction : 0;
            oldProduction = factory.oldProduction;
            this.troop = troop;
            disabledEndTime = factory.disabledEndTime;
            SetTeam(factory.team); //team stays the same (default = neutral)
                        
            if (factory.team != Team.Neutral)
            {
                int timeDiff = troop.endTime - factory.gameTime;
                this.count += timeDiff * production;
            }

            if (troop is Bomb) //same time bomb?
            {
                int bombCount = Math.Max(this.count / 2, 10);
                this.count -= bombCount;
                this.count = Math.Max(0, this.count);

                this.oldProduction = Math.Max(oldProduction, production);
                this.production = 0;
                this.disabledEndTime = troop.endTime + 5;

                //Console.Error.WriteLine("Bomb " + troop.id + ": " + this.count);
            }
            else
            {
                this.count += (troop.team == factory.team) ? troop.count : -troop.count;

                if (this.count < 0 && sameTime == false) //if the troop captured the factory
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

            List<Troop> troopList = troops.ToList();

            for (int i = 0; i < troopList.Count; i++)
            {
                Troop troop = troopList[i];
                bool sameTime = false;

                if (i + 1 < troopList.Count && troop.endTime == troopList[i + 1].endTime) //if troop came at same time
                {
                    sameTime = true;
                    Troop nextTroop = troopList[i + 1];
                    Troop combinedTroop = new Troop
                    {
                        start = troop.start,
                        end = troop.end,
                        count = Math.Abs(troop.count+(troop.team==nextTroop.team?1:-1)*nextTroop.count),
                        team = troop.count>=nextTroop.count?troop.team:nextTroop.team,
                        isAlly = troop.count>=nextTroop.count?troop.isAlly:nextTroop.isAlly,
                        isEnemy = troop.count>=nextTroop.count?troop.isEnemy:nextTroop.isEnemy,
                        turns = troop.turns - Game.gameTime,
                        endTime = troop.endTime,
                    };

                    troop = combinedTroop;
                    i++;
                }

                FactoryState newState = new FactoryState(lastState, troop, false);
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
                if (state.gameTime == testTroop.endTime)
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
                isNeutral = false;
            }
            else if (team == Team.Enemy)
            {
                isAlly = false;
                isEnemy = true;
                isNeutral = false;
            }
            else
            {
                isAlly = false;
                isEnemy = false;
                isNeutral = true;
            }
        }
    }
}
