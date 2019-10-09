using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class Bomb : Troop, Entity
    {
        public static Dictionary<int, Bomb> bombs = new Dictionary<int, Bomb>();

        public static Dictionary<int, Bomb> enemyBombs = new Dictionary<int, Bomb>();
        public static List<Bomb> oldEnemyBombs = new List<Bomb>();

        public static int enemyBombCount = 0;

        public Bomb()
        {
            this.id = idCount++;
        }

        public static Bomb GetBombByID(int id)
        {
            Bomb troop = new Bomb();
            troops.Add(troop);
            bombs.Add(troop.id, troop);
            return troop;
        }

        public new static void CleanUp()
        {
            bombs = new Dictionary<int, Bomb>();
            enemyBombs = new Dictionary<int, Bomb>();

            oldEnemyBombs.RemoveAll(bomb => bomb.endTime >= Game.gameTime);
        }

        public static int isBombIncoming(Factory factory)
        {
            foreach (var bomb in enemyBombs.Values)
            {
                if (bomb.end != null)
                {
                    return bomb.endTime;
                }
            }

            return -1;
        }

        public void AddMockBombTroop()
        {
            Troop testTroop = new Troop
            {
                start = this.start,
                end = this.end,
                count = 0,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = this.turns + 4,
                endTime = this.endTime + 4,
            };

            Troop testTroop2 = new Troop
            {
                start = this.start,
                end = this.end,
                count = 0,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = this.turns + 5,
                endTime = this.endTime + 5,
            };

            this.end.incoming.Add(testTroop);
            this.end.incoming.Add(testTroop2);
        }

        public new void ProcessMessage(EntityMessage message)
        {
            if (message.arg1 == 1)
            {
                isAlly = true;
                isEnemy = false;

                team = Team.Ally;
                start = Factory.GetFactoryByID(message.arg2);
                end = Factory.GetFactoryByID(message.arg3);

                turns = message.arg4;
                endTime = Game.gameTime + turns;

                end.incoming.Add(this);
                AddMockBombTroop();
            }
            else
            {
                isAlly = false;
                isEnemy = true;

                turns = 0;
                endTime = Game.gameTime;

                team = Team.Enemy;

                enemyBombs.Add(id, this);

                start = Factory.GetFactoryByID(message.arg2);                

                this.DeduceBomb();
            }

            start = Factory.GetFactoryByID(message.arg2);            
        }

        public void DeduceBomb()
        {
            if (oldEnemyBombs.Count == 0)
            {
                if (Game.gameTime == 1)
                {
                    end = Factory.ally.First();
                    turns = end.links[start];
                    endTime = Game.gameTime + turns;

                    oldEnemyBombs.Add(this);
                }


            }
        }

        public static void AddOldBombs()
        {
            if (enemyBombs.Count > 0)
            {
                foreach (var bomb in oldEnemyBombs)
                {
                    if (bomb.end != null)
                    {
                        bomb.turns = bomb.endTime - Game.gameTime;
                        bomb.end.incoming.Add(bomb);
                    }                    
                }
            }
        }

    }
}
