using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class Player
    {
        public static Player me = new Player();
        public static Player enemy = new Player();

        public int health;
        public int mana;
        public int numCards;
        public int rune;

        public int numCardsOnBoard;

        public bool hasLethal;

        public List<Card> cards = new List<Card>();
        public List<Card> boardCards = new List<Card>();
        public List<Card> handCards = new List<Card>();

        public List<Card> usableCards = new List<Card>();

        public List<Card> tauntCards = new List<Card>();

        public Card highestCardOnBoard;

        public List<Trade> trades = new List<Trade>();
        public List<SimulationGroup> tradeGroups = new List<SimulationGroup>();

        public Player opponent;

        public int maxAttackHeroScore;

        public bool isMe
        {
            get
            {
                return this == Player.me;
            }
        }

        public static void ProcessPlayers()
        {
            Player.me.opponent = Player.enemy;
            Player.enemy.opponent = Player.me;

            Player.enemy.CheckLethal();
            Player.me.HighestBoardCard();

            GetMaxAttackHeroScore(Player.me);
            GetMaxAttackHeroScore(Player.enemy);
        }

        public static void GetMaxAttackHeroScore(Player player)
        {
            player.usableCards = player.usableCards.OrderBy(c => c.id).ToList();

            int attack = 0;
            foreach (var card in player.usableCards)
            {
                attack += card.attack;
            }

            player.maxAttackHeroScore = attack;
        }

        public void HighestBoardCard()
        {
            highestCardOnBoard = Player.me.boardCards.OrderByDescending(c => c.cost).FirstOrDefault();
        }

        public void CheckLethal()
        {
            int totalAttack = Player.enemy.boardCards.Sum(c => c.attack);
            
            if (totalAttack >= Player.me.health)
            {
                Player.enemy.hasLethal = true;
            }
            else
            {
                Player.enemy.hasLethal = false;
            }
        }

        public static void CleanUp()
        {
            for (int i = 0; i < 2; i++)
            {
                Player player;
                if (i == 0)
                {
                    player = me;
                }
                else
                {
                    player = enemy;
                }
                                
                player.cards = new List<Card>();
                player.boardCards = new List<Card>();
                player.handCards = new List<Card>();
                player.usableCards = new List<Card>();
                player.numCardsOnBoard = 0;

                player.tauntCards = new List<Card>();
                //player.highestCardOnBoard = new List<Card>();

                player.trades = new List<Trade>();
                player.tradeGroups = new List<SimulationGroup>();
            }
        }
    }
}
