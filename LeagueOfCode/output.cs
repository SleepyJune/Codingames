
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

    class Drafting
    {
        public static List<Card> draftCards = new List<Card>();

        public static Dictionary<int, int> manaCurve = new Dictionary<int, int>();

        public static bool initMana;

        public static int[] manaDist = new int[]
        {
            1,//0
            1,//1
            2,//2
            3,//3
            3,//4
            2,//5
            1,//6
            1,//7
        };

        public static int manaMax;

        public static void InitManaCurve()
        {
            for (int i = 0; i < 8; i++)
            {
                manaCurve.Add(i, 0);
            }

            manaMax = manaDist.Sum(c => c);
        }

        public static void DraftOld()
        {
            Card card = draftCards.OrderBy(c => c.type == CardType.Creature ? 0 : 1).ThenByDescending(c => c.weightedValue).First();

            int index = draftCards.IndexOf(card);

            var newAction = new Action(index);
            newAction.ApplyAction();
        }

        public static void Draft()
        {
            if (!initMana)
            {
                InitManaCurve();
                initMana = true;
            }

            Card card = draftCards.OrderByDescending(c => c.weightedValue).First();

            int index = draftCards.IndexOf(card);

            var newAction = new Action(index);
            newAction.ApplyAction();

            var cost = card.cost >= 7 ? 7 : card.cost;
            manaCurve[cost] += 1;
        }

        public static void Draft2()
        {
            if (!initMana)
            {
                InitManaCurve();
                initMana = true;
            }

            float[] cardPercentage = new float[3];
            for (int i = 0; i < 3; i++)
            {
                var card = draftCards[i];
                var cost = card.cost >= 7 ? 7 : card.cost;

                float estimatedCards = 30f * manaDist[cost]/manaMax;
                float cardLeftWeight = 1f - manaCurve[cost] / estimatedCards;

                cardPercentage[i] = manaDist[cost] * cardLeftWeight;
            }

            float max = cardPercentage.Sum();
            float totalPercentage = 0;

            Random rand = new Random();
            var number = rand.NextDouble();

            for (int i = 0; i < 3; i++)
            {
                totalPercentage += cardPercentage[i] / max;

                if (number < totalPercentage)
                {
                    var newAction = new Action(i);
                    newAction.ApplyAction();

                    var card = draftCards[i];
                    var cost = card.cost >= 7 ? 7 : card.cost;
                    manaCurve[cost] += 1;

                    break;
                }
            }
            
            /*Card card = draftCards.OrderBy(c => c.type == CardType.Creature ? 0 : 1).ThenByDescending(c => GetDraftCardValue(c)).First();

            int index = draftCards.IndexOf(card);

            var newAction = new Action(index);
            newAction.ApplyAction();

            */
        }

        static float GetDraftCardValue(Card card)
        {
            var cost = card.cost >= 7 ? 7 : card.cost;

            //Math.Round(30 * (float)manaDist[cost] / manaMax);

            float estimatedCards = 30f * manaDist[cost]/ (float)manaMax;
            float cardLeftWeight = 2f - manaCurve[cost] / estimatedCards;
            
            var ret = card.weightedValue * cardLeftWeight;
            Console.Error.WriteLine("Card " + card.cost + ": " + ret);

            return ret;
        }

        public static void CleanUp()
        {
            draftCards = new List<Card>();
        }

    }

    class Game
    {
        public static void MakeMove()
        {
            //InitRound();

            Strategy.MakeMove();
            Action.PrintActions();

            Game.CleanUp();
        }

        public static void CleanUp()
        {
            Drafting.CleanUp();
            Action.CleanUp();
            Strategy.CleanUp();
            Player.CleanUp();
            MonsterTrading.CleanUp();
            SummonGroup.CleanUp();
        }
    }

    class MonsterTrading
    {
        public static void StartSimulation()
        {
            var start = Timer.TickCount;

            FindGroupSimulation(Player.enemy);
            Console.Error.WriteLine("Enemy Trade Groups: " + Player.enemy.tradeGroups.Count);
            
            FindGroupSimulation(Player.me);
            Console.Error.WriteLine("My Trade Groups: " + Player.me.tradeGroups.Count);

            SummonGroup.GetSummons();

            FindMyBestTradeGroup();

            Console.Error.WriteLine("End Simulation: " + (Timer.TickCount - start));
        }

        public static void FindMyBestTradeGroup()
        {
            var enemyTradeGroups = Player.enemy.tradeGroups.OrderByDescending(group => group.GetScore());

            foreach (var myGroup in Player.me.tradeGroups)
            {

                //Player.enemy.tradeGroups = new List<SimulationGroup>(); //refresh
                //FindGroupSimulation(Player.enemy, myGroup);

                var best = enemyTradeGroups.Where(group => PossibleGroupSelection(myGroup, group)).FirstOrDefault();
                //.Where(group => PossibleGroupSelection(myGroup, group))

                var bestSummon = SummonGroup.groups.Where(group => PossibleSummonSelection(myGroup, group))
                                                   .OrderByDescending(group => group.GetScore()).FirstOrDefault();

                myGroup.finalScore = myGroup.GetScore();

                if (best != null)
                {
                    myGroup.finalScore -= best.GetScore();
                    myGroup.enemyScore = best.GetScore();
                }

                if (bestSummon != null)
                {
                    //myGroup.finalScore += bestSummon.GetScore();
                    myGroup.bestSummonGroup = bestSummon;
                }
            }
        }

        public static bool PossibleGroupSelection(SimulationGroup myGroup, SimulationGroup enemyGroup)
        {
            foreach (var target in myGroup.targets)
            {
                if (enemyGroup.simulationCards.Contains(target))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool PossibleSummonSelection(SimulationGroup myGroup, SummonGroup summonGroup)
        {
            if (myGroup.simulatedManaCost + summonGroup.simulatedManaCost > Player.me.mana)
            {
                return false;
            }

            /*foreach (var target in summonGroup.)
            {
                if (enemyGroup.simulationCards.Contains(target))
                {
                    return false;
                }
            }*/

            return true;
        }

        public static void FindGroupSimulation(Player player, SimulationGroup parent = null)
        {           
            FindTrades(player, parent);

            //player.trades = player.trades.OrderBy(sim => sim is Trade ? 0 : 1).ToList();

            for (int i = 1; i <= 3 && i <= player.trades.Count; i++)
            {
                int[] myCards = new int[i];
                GetSimulationCombination(player, myCards, 0, player.trades.Count - 1, 0, i);
            }
        }

        public static void GetSimulationCombination(Player player, int[] data, int start, int end, int index, int numCards)
        {
            if (index == numCards)
            {
                SimulationGroup group = new SimulationGroup(player);
                                
                for (int i = 0; i < numCards; i++)
                {
                    var sim = player.trades[data[i]];
                    if (!group.AddSimulation(sim))
                    {
                        return;
                    }
                }

                var possible = group.ComputeScore();
                if (possible)
                {
                    player.tradeGroups.Add(group);

                    //Console.Error.WriteLine(printString);
                }

                return;
            }

            for (int i = start; i <= end && end - i + 1 >= numCards - index; i++)
            {
                data[index] = i;
                GetSimulationCombination(player, data, i + 1, end, index + 1, numCards);
            }
        }

        public static void FindTrades(Player player, SimulationGroup parent = null)
        {
            var enemyPlayer = player.isMe ? Player.enemy : Player.me;

            var enemyCards = enemyPlayer.boardCards
                .OrderBy(c => c.hasGuard ? 0 : 1)
                .ThenByDescending(c => c.health);

            /*var print = player.isMe ? "My " : "Enemy ";
            print += "Useable Cards: ";

            foreach(var card in player.usableCards)
            {
                print += card.id + " ";
            }

            Console.Error.WriteLine(print);*/

            List<Card> newUseableCards = parent != null ?
                player.usableCards.Where(c => !parent.targets.Contains(c)).ToList() : player.usableCards;

            foreach (var enemy in enemyCards)
            {
                if (parent != null)
                {
                    var target = SimulationCard.GetSimulationCard(enemy, parent);
                    if (target != null)
                    {
                        if (!(target.health > 0 && (target.attack > 0 || target.hasGuard)))
                        {
                            continue;
                        }
                    }
                }

                if (enemy.health > 0 && (enemy.attack > 0 || enemy.hasGuard))
                {
                    GetTradeOptions(newUseableCards, player, enemy, parent);
                }
            }
        }

        public static void GetTradeOptions(List<Card> usableCards, Player player, Card target, SimulationGroup parent, List<Trade> ret = null)
        {
            for (int i = 1; i < 4 && i <= usableCards.Count; i++)
            {
                int[] myCards = new int[i];
                GetTradeCombination(usableCards, player, target, myCards, 0, usableCards.Count - 1, 0, i, ret);
            }
        }

        public static void GetTradeCombination(List<Card> usableCards, Player player, Card target, int[] data, int start, int end, int index, int numCards, List<Trade> ret = null)
        {
            if (index == numCards)
            {
                Trade newTrade = new Trade(target);
                                
                for(int i=0;i<numCards;i++)
                {
                    var card = usableCards[data[i]];
                    newTrade.AddCard(card);                    
                }
                                
                var killable = newTrade.ComputeScore();
                if (killable)// && newTrade.GetScore() >= 0)
                {
                    if (ret != null)
                    {
                        ret.Add(newTrade);
                    }
                    else
                    {
                        player.trades.Add(newTrade);
                    }

                    //newTrade.Print();
                }

                return;
            }

            for (int i = start; i <= end && end - i+1 >= numCards - index; i++)
            {
                data[index] = i;
                GetTradeCombination(usableCards, player, target, data, i + 1, end, index + 1, numCards, ret);
            }
        }

        public static void CleanUp()
        {
            
        }
    }

    class Strategy
    {
        public static void Initialize()
        {
            Player.ProcessPlayers();
        }

        public static bool isDraftPhase
        {
            get
            {
                return Player.me.mana == 0;
            }
        }

        public static void MakeMove()
        {
            Initialize();

            if (isDraftPhase)
            {
                Drafting.Draft();
            }
            else
            {
                Attack();
                DefaultAttack();
                Summon();
                DefaultSummon();
                DefaultAttack();
                UseItem();
            }
        }

        public static void Summon()
        {
            var bestSummon = SummonGroup.groups.OrderByDescending(group => group.GetScore()).FirstOrDefault();

            if (bestSummon != null)
            {
                BestSummon(bestSummon);
            }
        }

        public static void DefaultSummon()
        {
            foreach (var card in Player.me.handCards.OrderByDescending(c => c.cost))
            {
                if (Player.me.numCardsOnBoard >= 6)
                {
                    return;
                }

                if (card.type == CardType.Creature)
                {
                    if (Player.me.mana >= card.cost)
                    {
                        var newAction = new Action(card);
                        newAction.ApplyAction();
                    }
                }
            }
        }

        public static void UseItem()
        {
            foreach (var card in Player.me.handCards.OrderByDescending(c => c.cost))
            {
                if (card.type != CardType.Creature)
                {
                    if (Player.me.mana >= card.cost)
                    {
                        //Action.actions.Add(new Action(card));
                        if (card.type == CardType.Blue)
                        {
                            var newAction = new Action(MoveType.DirectUseItem, card, null);
                            newAction.ApplyAction();
                        }
                        /*else if (card.type == CardType.Green)
                        {
                            var target = Player.me.cards.OrderByDescending(c => c.location == CardLocation.PlayerBoard ? 0 : 1).FirstOrDefault();

                            if (target != null)
                            {
                                var newAction = new Action(MoveType.UseItem, card, target);
                                newAction.ApplyAction();
                            }
                        }
                        else if (card.type == CardType.Red)
                        {
                            var target = Player.enemy.cards.FirstOrDefault();

                            if (target != null)
                            {
                                var newAction = new Action(MoveType.UseItem, card, target);
                                newAction.ApplyAction();
                            }
                        }*/
                    }
                }
            }
        }

        public static void DefaultAttack()
        {
            var enemyCards = Player.enemy.cards
                .OrderBy(c => c.hasGuard ? 0 : 1)
                .ThenBy(c => c.health);

            foreach (var card in Player.me.boardCards.OrderBy(c => c.cost))
            {
                if (card.used)
                {
                    continue;
                }

                if (Player.enemy.numCardsOnBoard > 0 && card.attack > 0)
                {
                    foreach (var enemyCard in enemyCards)
                    {
                        //Action.actions.Add(new Action(card, enemyCard));

                        if (enemyCard.health <= 0)
                        {
                            continue;
                        }

                        /*if (enemyCard.hasGuard)
                        {
                            var newAction = new Action(MoveType.Attack, card, enemyCard);
                            newAction.ApplyAction();
                            break;
                        }*/
                    }
                }

                if (!card.used)
                {
                    var newAction = new Action(MoveType.DirectAttack, card, null);
                    newAction.ApplyAction();
                }
            }
        }

        public static void Attack()
        {
            MonsterTrading.StartSimulation();

            //Execute Simulation

            var orderedSimulations = Player.me.tradeGroups.OrderByDescending(sim => sim.finalScore);
            var best = orderedSimulations.FirstOrDefault();

            /*foreach (var sim in orderedSimulations)
            {
                sim.Print();
            }*/

            if (best != null && best.GetScore() > 0)
            {
                foreach (var trade in best.simulations)
                {
                    if (trade.spellCardTarget != null && trade.spellCardTarget.location == CardLocation.PlayerHands)
                    {
                        var summonAction = new Action(trade.spellCardTarget);
                        summonAction.ApplyAction();
                    }

                    foreach (var card in trade.myCards)
                    {
                        if (card.location == CardLocation.PlayerHands)
                        {
                            if (card.type == CardType.Creature && card.hasCharge)
                            {
                                var summonAction = new Action(card);
                                summonAction.ApplyAction();
                            }
                            else if (card.type == CardType.Green && trade.spellCardTarget != null)
                            {
                                var newSpell = new Action(MoveType.UseItem, card, trade.spellCardTarget);
                                newSpell.ApplyAction();
                                continue;
                            }
                        }

                        var newAction = new Action(MoveType.Attack, card, trade.target);
                        newAction.ApplyAction();
                    }
                }

                best.Print();

                if (best.bestSummonGroup != null)
                {
                    BestSummon(best.bestSummonGroup);
                }
            }
        }

        public static void BestSummon(SummonGroup group)
        {
            foreach (var card in group.myCards)
            {
                if (Player.me.numCardsOnBoard >= 6)
                {
                    return;
                }

                if (card.type == CardType.Creature)
                {
                    if (Player.me.mana >= card.cost)
                    {
                        var newAction = new Action(card);
                        newAction.ApplyAction();
                    }
                }
            }
        }

        public static void CleanUp()
        {

        }
    }

    public enum CardLocation
    {
        PlayerHands = 0,
        PlayerBoard = 1,
        EnemyBoard = -1,
    }

    public enum CardType
    {
        Creature = 0,
        Green = 1,
        Red = 2,
        Blue = 3,
    }

    public enum Ability
    {
        Breakthrough,
        Charge,
        Drain,
        Guard,
        Lethal,
        Ward,
    }

    class Card
    {
        public int number;
        public int id;
        public CardLocation location;
        public CardType type;
        public int cost;
        public int attack;
        public int health;
        public string abilities;
        public int myHealthChange;
        public int enemyHealthChange;
        public int cardDraw;

        public float cardValue;
        public float weightedValue;
        public float bonusValue;
        
        public bool hasBreakthrough;
        public bool hasCharge;
        public bool hasDrain;
        public bool hasGuard;
        public bool hasLethal;
        public bool hasWard;

        public bool isSpellCard;

        public bool used;

        public Dictionary<Trade, SimulationCard> simulationCards = new Dictionary<Trade,SimulationCard>();

        public void ProcessCard()
        {
            GetCardAbilities();
            ComputeCardValue();
        }

        public void ComputeCardValue()
        {            
            if (type == CardType.Creature)
            {
                cardValue = 1.4f * attack + health;

                if (hasLethal)
                {
                    bonusValue += 4;
                }

                if (hasGuard)
                {
                    bonusValue += 1;
                }

                if (hasWard)
                {
                    bonusValue += Math.Min(4,attack);
                }

                cardValue += bonusValue;
            }
            else
            {                
                if (type == CardType.Green)
                {
                    cardValue = attack + .5f * health;

                    if (hasLethal)
                    {
                        cardValue += 5;
                    }

                    if (hasWard)
                    {
                        cardValue += 5;
                    }

                    if (hasCharge)
                    {
                        cardValue += 2;
                    }
                }
                else if (type == CardType.Red)
                {
                    cardValue = Math.Abs(attack) + 2 * Math.Abs(health);
                }
                else if (type == CardType.Blue)
                {
                    cardValue = 0;// 2 * Math.Abs(health);
                }

                cardValue += cardDraw * 3;
            }

            weightedValue = cardValue / (cost+1);
        }

        public bool isCardUsable()
        {
            if (cost > Player.me.mana)
            {
                return false;
            }

            if (type == CardType.Creature)
            {
                if (health > 0)
                {
                    if (location != CardLocation.PlayerHands)
                    {
                        return true;
                    }
                    else
                    {
                        if (hasCharge)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (location == CardLocation.PlayerHands)
                {
                    isSpellCard = true;

                    return true;
                }
            }

            return false;
        }

        public void GetCardAbilities()
        {
            if (abilities[0] == 'B')
            {
                hasBreakthrough = true;
            }

            if (abilities[1] == 'C')
            {
                hasCharge = true;
            }

            if (abilities[2] == 'D')
            {
                hasDrain = true;
            }

            if (abilities[3] == 'G')
            {
                hasGuard = true;

                if (location == CardLocation.EnemyBoard)
                {                    
                    Player.enemy.tauntCards.Add(this);
                }
                else if (location == CardLocation.PlayerBoard)
                {
                    Player.me.tauntCards.Add(this);
                }
            }

            if (abilities[4] == 'L')
            {
                hasLethal = true;
            }

            if (abilities[5] == 'W')
            {
                hasWard = true;
            }
        }
    }

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

    class SimulationCard
    {
        public Card parent;

        public int attack;
        public int health;

        //public bool hasBreakthrough;
        //public bool hasCharge;
        //public bool hasDrain;
        public bool hasGuard;
        public bool hasLethal;
        public bool hasWard;

        public SimulationCard(Card card)
        {
            parent = card;

            attack = card.attack;
            health = card.health;

            hasGuard = card.hasGuard;
            hasLethal = card.hasLethal;
            hasWard = card.hasWard;
        }

        public SimulationCard(SimulationCard card)
        {
            parent = card.parent;

            attack = card.attack;
            health = card.health;

            hasGuard = card.hasGuard;
            hasLethal = card.hasLethal;
            hasWard = card.hasWard;
        }

        public static SimulationCard GetSimulationCard(Card target, SimulationGroup group)
        {
            foreach (var trade in group.simulations)
            {
                SimulationCard simTarget;
                if (target.simulationCards.TryGetValue(trade, out simTarget))
                {
                    return simTarget;
                }
            }

            return null;
        }

        public static SimulationCard NewSimulationCard(Card target, Trade playerTrade)
        {
            SimulationCard simTarget;
            if (playerTrade != null && target.simulationCards.TryGetValue(playerTrade, out simTarget))
            {
                return new SimulationCard(simTarget);
            }
            else
            {
                return new SimulationCard(target);
            }
        }
    }

    public enum MoveType
    {
        Pass,
        Pick,
        Summon,
        Attack,
        DirectAttack,
        UseItem,
        DirectUseItem,
    }

    class Action
    {
        static List<Action> actions = new List<Action>();

        public MoveType type = MoveType.Pass;
        public Card attacker;
        public Card target;

        public int cardNum;

        public Action()
        {

        }

        public Action(int cardNum)
        {
            type = MoveType.Pick;
            this.cardNum = cardNum;
        }

        public Action(Card attacker)
        {
            type = MoveType.Summon;
            this.attacker = attacker;
        }

        public Action(MoveType moveType, Card attacker, Card target)
        {
            this.attacker = attacker;
            this.target = target;
            this.type = moveType;
        }

        public void ApplyAction()
        {
            actions.Add(this);

            if (type != MoveType.Summon && attacker != null)
            {
                attacker.used = true;                
                Player.me.usableCards.Remove(attacker);
            }

            if (type == MoveType.Summon)
            {
                Player.me.mana -= attacker.cost;
                Player.me.numCardsOnBoard += 1;

                if (attacker.hasCharge)
                {
                    Player.me.boardCards.Add(attacker);
                    Player.me.handCards.Remove(attacker);
                    attacker.location = CardLocation.PlayerBoard;
                }
            }
            else if (type == MoveType.Attack || type == MoveType.DirectAttack
                 || type == MoveType.UseItem || type == MoveType.DirectUseItem)
            {
                if (attacker.type == CardType.Creature)
                {
                    type = MoveType.Attack;

                    if (target == null)
                    {
                        type = MoveType.DirectAttack;
                        Player.enemy.health -= attacker.attack;
                    }
                    else
                    {
                        target.health -= attacker.attack;
                    }                  
                }
                else
                {
                    type = MoveType.UseItem;

                    if (target == null)
                    {
                        type = MoveType.DirectUseItem;
                    }
                    else
                    {
                        target.attack += attacker.attack;
                        target.health += attacker.health;

                        target.hasGuard = target.hasGuard && !attacker.hasGuard;
                    }
                }
            }
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void PrintActions()
        {
            if (actions.Count == 0)
            {
                Console.WriteLine("PASS");
                return;
            }

            string str = "";

            foreach (var action in actions)
            {
                if (str != "")
                {
                    str += ";";
                }

                switch (action.type)
                {
                    case MoveType.Pick:
                        str += "PICK " + action.cardNum;
                        break;
                    case MoveType.Summon:
                        str += "SUMMON " + action.attacker.id;
                        break;
                    case MoveType.Attack:
                        str += "ATTACK " + action.attacker.id + " " + action.target.id;
                        break;
                    case MoveType.DirectAttack:
                        str += "ATTACK " + action.attacker.id + " -1";
                        break;
                    case MoveType.DirectUseItem:
                        str += "USE " + action.attacker.id + " -1";
                        break;
                    case MoveType.UseItem:
                        str += "USE " + action.attacker.id + " " + action.target.id;
                        break;
                    default:
                        str += "";
                        break;
                }                
            }

            Console.WriteLine(str);
        }
    }

    class Timer
    {
        private static DateTime loadTime = DateTime.Now;

        public static float TickCount
        {
            get
            {
                return (int)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
            }
        }
    }




    class SimulationGroup
    {
        public List<Trade> simulations;

        public HashSet<Card> simulationCards;

        HashSet<Card> tauntCardsKilled;

        public HashSet<Card> targets;

        public int simulatedManaCost;

        public SummonGroup bestSummonGroup;

        float score;
        public float enemyScore;
        public float finalScore;
        
        int heroHealth;

        int attackHeroScore;

        Player player;

        public SimulationGroup(Player player)
        {
            this.player = player;

            simulations = new List<Trade>();
            simulationCards = new HashSet<Card>();

            tauntCardsKilled = new HashSet<Card>();

            targets = new HashSet<Card>();

            heroHealth = player.health;

            attackHeroScore = player.maxAttackHeroScore;
        }

        public bool AddSimulation(Trade trade)
        {
            if (trade != null && simulationCards.Contains(trade.target))
            {
                return false;
            }

            if (simulationCards.Overlaps(trade.GetCards()))
            {
                return false;
            }

            if (tauntCardsKilled.Count < player.opponent.tauntCards.Count)
            {
                //enemy has taunt cards and player has not killed them all yet

                if (trade != null)
                {
                    if (player.opponent.tauntCards.Contains(trade.target))
                    {
                        tauntCardsKilled.Add(trade.target);
                    }
                    else
                    {
                        if (trade.myCards.Count == 1 && trade.myCards[0].isSpellCard)
                        {
                            //Console.Error.WriteLine("SpellCard Taunt " + trade.target.id + ": " + trade.GetScore());
                            //spell cards can go through taunt
                        }
                        else
                        {
                            return false; //cannot attack target without killing taunt cards
                        }
                    }
                }
            }

            attackHeroScore -= trade.totalAttack;
            simulatedManaCost += trade.simulatedManaCost;

            if (simulatedManaCost > Player.me.mana)
            {
                return false;
            }

            targets.Add(trade.target);
            simulations.Add(trade);

            foreach (var card in trade.GetCards())
            {
                simulationCards.Add(card);
            }

            score += trade.GetScore();

            return true;
        }

        public void Print()
        {
            string printString = "Group: ";

            foreach (var sim in simulations)
            {
                foreach (var card in sim.GetCards())
                {
                    printString += card.id + " ";
                }

                printString += ", ";
            }

            printString += "Final: " + finalScore + " S1: " + score + " S2: " + enemyScore + " H: " + attackHeroScore;

            if (bestSummonGroup != null)
            {
                printString += " Summon: " + bestSummonGroup.score;
            }

            Console.Error.WriteLine(printString);
        }

        public bool ComputeScore()
        {
            if (tauntCardsKilled.Count >= player.opponent.tauntCards.Count) //killed all taunts
            {
                //Kill hero in this turn
                if (attackHeroScore >= player.opponent.health)
                {
                    score = 9999;
                }
                else
                {
                    if (!Player.enemy.hasLethal)
                    {
                        score += attackHeroScore;
                    }
                }
            }
            else
            {
                attackHeroScore = 0;
            }

            return true;
        }

        public float GetScore()
        {
            return score;
        }

        public HashSet<Card> GetCards()
        {
            return simulationCards;
        }
    }

    class SummonGroup
    {
        public static List<SummonGroup> groups = new List<SummonGroup>();
        public static Dictionary<Card, float> summonValues = new Dictionary<Card, float>();

        public List<Card> myCards = new List<Card>();

        public int simulatedManaCost = 0;

        public float score;

        public int numSummons;

        public SummonGroup()
        {

        }

        public bool AddCard(Card card)
        {
            //already summoned

            if (Player.me.boardCards.Count + numSummons >= 6)
            {
                return false; //max summons on field
            }

            simulatedManaCost += card.cost;

            if (simulatedManaCost > Player.me.mana)
            {
                return false;
            }

            numSummons += 1;
            myCards.Add(card);

            score += card.cardValue;// -card.cost * 2;

            float summonValue;
            if (summonValues.TryGetValue(card, out summonValue))
            {
                score -= summonValue;
            }

            return true;
        }

        public void Print()
        {
            string printString = "SummonGroup: ";

            foreach (var card in myCards)
            {
                printString += card.id + " ";
            }

            printString += "Score: " + score;

            Console.Error.WriteLine(printString);
        }

        public float GetScore()
        {
            return score;
        }

        public static void GetSummonValues(List<Card> summons)
        {
            foreach (var card in summons)
            {
                List<Trade> possibleTrades = new List<Trade>();

                //Get enemy possible trade against this summon
                MonsterTrading.GetTradeOptions(Player.enemy.usableCards, Player.enemy, card, null, possibleTrades);

                var best = possibleTrades.OrderByDescending(t => t.GetScore()).FirstOrDefault();
                if (best != null)
                {
                    summonValues.Add(card, best.GetScore());
                }
            }
        }

        public static void GetSummons()
        {
            var summons = Player.me.handCards.Where(c=> c.cost <= Player.me.mana && 
                                                        c.location == CardLocation.PlayerHands && 
                                                        c.type == CardType.Creature).ToList();
            GetSummonValues(summons);

            for (int i = 1; i <= summons.Count; i++)
            {
                int[] myCards = new int[i];
                GetSummonsCombination(summons, myCards, 0, summons.Count - 1, 0, i);
            }
        }

        public static void GetSummonsCombination(List<Card> summons, int[] data, int start, int end, int index, int numCards)
        {
            if (index == numCards)
            {
                SummonGroup newSummonGroup = new SummonGroup();

                for (int i = 0; i < numCards; i++)
                {
                    var card = summons[data[i]];
                    if (!newSummonGroup.AddCard(card))
                    {
                        return; //cannot summon
                    }
                }

                newSummonGroup.Print();

                groups.Add(newSummonGroup);

                return;
            }

            for (int i = start; i <= end && end - i + 1 >= numCards - index; i++)
            {
                data[index] = i;
                GetSummonsCombination(summons, data, i + 1, end, index + 1, numCards);
            }
        }

        public static void CleanUp()
        {
            groups = new List<SummonGroup>();
            summonValues = new Dictionary<Card, float>();
        }
    }

    class Trade
    {
        public List<Card> myCards = new List<Card>();
        public Card target;

        public Trade playerTrade;

        public float myCost;
        public float enemyCost;
        public float bonusScore;
                
        float score = 0;

        public bool targetKilleable = false;
        public bool overkill = false;

        public int totalAttack = 0;
        public int numSummons = 0;

        HashSet<Card> simulationCards;

        public int simulatedManaCost = 0;

        public Card spellCard;
        public Card spellCardTarget;

        public Trade(Card target)
        {
            this.target = target;

            simulationCards = new HashSet<Card>();
            simulationCards.Add(target);
        }

        public void AddCard(Card card)
        {
            totalAttack += card.attack;
            myCards.Add(card);

            if (card.location == CardLocation.PlayerHands)
            {
                simulatedManaCost += card.cost;

                if (card.type == CardType.Green)
                {
                    if (spellCard != null)
                    {
                        simulatedManaCost = 9999; //cannot compute more than 1 spell card
                    }

                    spellCard = card;
                }
                else if (card.type == CardType.Creature)
                {
                    numSummons += 1;
                }
            }
        }

        public bool ComputeScore()
        {
            if (simulatedManaCost > Player.me.mana)
            {
                return false;
            }

            if (target.hasWard)
            {
                myCards = myCards.OrderByDescending(c => c.isSpellCard)
                                 .ThenBy(c => c.attack)
                                 .ThenByDescending(c => c.hasLethal).ToList();
            }
            else
            {
                myCards = myCards.OrderByDescending(c => c.type == CardType.Creature && c.hasLethal)
                                 .ThenByDescending(c => c.isSpellCard)
                                 .ThenByDescending(c => c.attack).ToList();
            }

            if (spellCard != null)
            {
                float highestScore = -999;

                List<SimulationCard> highestSimulatedCards = null;

                foreach (var card in myCards)
                {
                    if (card.type != CardType.Creature)
                    {
                        continue;
                    }

                    List<SimulationCard> newSimulatedCards = new List<SimulationCard>();

                    var ret = ComputeScoreHelper(card, newSimulatedCards);
                    if (ret)
                    {
                        if (score >= highestScore)
                        {
                            highestScore = score;
                            spellCardTarget = card;
                            highestSimulatedCards = newSimulatedCards;
                        }
                    }
                }

                if (highestSimulatedCards != null)
                {
                    foreach (var card in highestSimulatedCards)
                    {
                        card.parent.simulationCards.Add(this, card);
                    }
                }

                if (highestScore == -999)
                {
                    return false;
                }
            }
            else
            {
                return ComputeScoreHelper();
            }

            return true;
        }

        public bool ComputeScoreHelper(Card spellTarget = null, List<SimulationCard> storedSimulatedCards = null)
        {
            enemyCost = 0;
            myCost = 0;
            bonusScore = 0;

            SimulationCard sTarget = SimulationCard.NewSimulationCard(target, playerTrade);
            
            foreach (var card in myCards)
            {
                if (sTarget.health <= 0) //overkill
                {
                    overkill = true;
                    break;
                }
                                
                simulationCards.Add(card);

                SimulationCard sCard = SimulationCard.NewSimulationCard(card, playerTrade);
                if (storedSimulatedCards != null)
                {
                    storedSimulatedCards.Add(sCard);
                }
                else
                {
                    card.simulationCards.Add(this, sCard);
                }              
                
                if (card.type == CardType.Creature)
                {
                    //check if enough room for summon
                    if (card.hasCharge && card.location == CardLocation.PlayerHands)
                    {

                    }

                    if (spellTarget != null && spellTarget == card)
                    {
                        if (spellCard.hasWard)
                        {
                            sCard.hasWard = true;
                        }

                        if (spellCard.hasLethal)
                        {
                            sCard.hasLethal = true;
                        }

                        sCard.attack += spellCard.attack;
                        sCard.health += spellCard.health;
                    }


                    if (sTarget.hasWard)// && card.attack > 0)
                    {
                        sTarget.hasWard = false;
                    }
                    else
                    {
                        if (sCard.hasLethal)
                        {
                            sTarget.health = 0;
                        }
                        else
                        {
                            sTarget.health -= sCard.attack;
                        }
                    }

                    var cardHealthLeft = sCard.health - sTarget.attack;
                    if (!sCard.hasWard)
                    {
                        if (sTarget.hasLethal)
                        {
                            cardHealthLeft = 0;
                        }

                        if (cardHealthLeft <= 0)
                        {
                            myCost += sCard.health + sCard.attack;
                        }
                        else
                        {
                            myCost += sCard.health - cardHealthLeft;
                            //bonusScore += cardAttack;
                        }
                    }
                }
                else
                {
                    if (card.type == CardType.Red || card.type == CardType.Blue)
                    {
                        myCost += card.cost * 2;

                        bonusScore += card.cardDraw * 3;

                        if (card.health < 0)
                        {
                            if (sTarget.hasWard)
                            {
                                sTarget.hasWard = false;
                            }
                            else
                            {
                                sTarget.health += card.health;
                            }
                        }                        
                        
                        sTarget.attack += card.attack;

                        if (card.hasWard)
                        {
                            sTarget.hasWard = false;
                        }

                        if (card.hasLethal)
                        {
                            sTarget.hasLethal = false;
                        }

                        if (card.hasGuard) //lazy coding
                        {
                            if (target.hasGuard)
                            {
                                //score = target.health;
                                //return true;
                            }
                        }
                    }
                }
            }

            enemyCost = target.health + target.attack + target.bonusValue;

            /*if (target.hasLethal)
            {
                if (Player.me.highestCardOnBoard != null)
                {
                    enemyCost += Player.me.highestCardOnBoard.health + Player.me.highestCardOnBoard.attack;
                }
            }*/

            score = enemyCost - myCost + bonusScore;
                        
            if (sTarget.health <= 0 && overkill == false)
            {
                targetKilleable = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /*public void AddSimulatedCard(Card target, SimulationCard card)
        {
            List<SimulationCard> cardList;
            if (simulatedCards.TryGetValue(target, out cardList))
            {
                cardList.Add(card);
            }
            else
            {
                cardList = new List<SimulationCard>();
                cardList.Add(card);
                simulatedCards.Add(target, cardList);
            }
        }*/

        public void Print()
        {
            string printString = "Target: " + target.id + " Attackers: ";

            foreach (var card in myCards)
            {
                printString += card.id + " ";
            }

            printString += "Score: " + score;

            Console.Error.WriteLine(printString);
        }

        public float GetScore()
        {
            return score;
        }

        public HashSet<Card> GetCards()
        {
            return simulationCards;
        }
    }

}
