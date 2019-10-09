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
        public static List<Bomb> oldBombs = new List<Bomb>();

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

            oldBombs.RemoveAll(bomb => bomb.endTime >= Game.gameTime);
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

                if (Game.gameTime == 1)
                {
                    end = Factory.ally.First();
                    turns = end.links[start];
                    endTime = Game.gameTime + turns;

                    oldBombs.Add(this);
                }
            }

            start = Factory.GetFactoryByID(message.arg2);
            //DeduceBomb();
        }

        public static void DeduceBomb()
        {
            if (enemyBombs.Count == 1 && oldBombs.Count == 1)
            {
                var enemyBomb = enemyBombs.First();
                var oldBomb = oldBombs.First();

                //enemyBomb = oldBomb;


            }

            /*if (enemyBombs.Count == 1 && oldBombs.Count == 0)
            {
                Bomb enemyBomb1 = enemyBombs.Values.First();
                oldBombs.Add(enemyBomb1.id, enemyBomb1);
                Bomb.enemyBombCount++;
            }

            if (enemyBombs.Count == 1 && oldBombs.Count == 1)
            {
                Bomb enemyBomb1 = enemyBombs.Values.First();
                Bomb oldBomb1 = oldBombs.Values.First();
                enemyBomb1 = oldBomb1;
            }*/
        }

    }
}
