using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class Program
    {
        public static float loopStartTime = 0;

        static void Main(string[] args)
        {
            string[] inputs;

            // game loop
            while (true)
            {
                loopStartTime = Timer.TickCount;

                for (int i = 0; i < 2; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int playerHealth = int.Parse(inputs[0]);
                    int playerMana = int.Parse(inputs[1]);
                    int playerDeck = int.Parse(inputs[2]);
                    int playerRune = int.Parse(inputs[3]);

                    var player = i == 0 ? Player.me : Player.enemy;

                    player.health = playerHealth;
                    player.mana = playerMana;
                    player.numCards = playerDeck;
                    player.rune = playerRune;
                }

                int opponentHand = int.Parse(Console.ReadLine());

                int cardCount = int.Parse(Console.ReadLine());

                Console.Error.WriteLine("CardCount: " + cardCount);

                for (int i = 0; i < cardCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int cardNumber = int.Parse(inputs[0]);
                    int instanceId = int.Parse(inputs[1]);
                    int location = int.Parse(inputs[2]);
                    int cardType = int.Parse(inputs[3]);
                    int cost = int.Parse(inputs[4]);
                    int attack = int.Parse(inputs[5]);
                    int defense = int.Parse(inputs[6]);
                    string abilities = inputs[7];
                    int myHealthChange = int.Parse(inputs[8]);
                    int opponentHealthChange = int.Parse(inputs[9]);
                    int cardDraw = int.Parse(inputs[10]);

                    var cardLocation = (CardLocation)location;
                    
                    Card newCard = new Card()
                    {
                        number = cardNumber,
                        id = instanceId,
                        location = cardLocation,
                        type = (CardType) cardType,
                        cost = cost,
                        attack = attack,
                        health = defense,
                        abilities = abilities,
                        myHealthChange = myHealthChange,
                        enemyHealthChange = opponentHealthChange,
                        cardDraw = cardDraw,
                    };

                    newCard.ProcessCard();

                    if (Strategy.isDraftPhase)
                    {
                        Drafting.draftCards.Add(newCard);
                    }
                    else
                    {
                        if (cardLocation != CardLocation.EnemyBoard)
                        {
                            if (cardLocation == CardLocation.PlayerBoard)
                            {
                                Player.me.numCardsOnBoard += 1;
                                Player.me.boardCards.Add(newCard);
                            }
                            else
                            {
                                Player.me.handCards.Add(newCard);
                            }

                            Player.me.cards.Add(newCard);

                            if (newCard.isCardUsable())
                            {
                                Player.me.usableCards.Add(newCard);
                            }
                        }
                        else
                        {
                            Player.enemy.numCardsOnBoard += 1;
                            Player.enemy.cards.Add(newCard);
                            Player.enemy.boardCards.Add(newCard);
                            Player.enemy.usableCards.Add(newCard);
                        }
                    }
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                //Console.WriteLine("PASS");

                Game.MakeMove();
            }
        }
    }
}
