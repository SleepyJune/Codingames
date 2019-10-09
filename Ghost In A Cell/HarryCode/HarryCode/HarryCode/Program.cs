using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        List<int> F_ToBeBombed = new List<int>();
        bool Bomb = false;
        int BCoolDown = 0;
        int Blow_est = 0;
        int Bomb_stock = 2;
        int Bomb_sent = 0;
        int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
        Dictionary<int,List<int>> link0 = new Dictionary<int,List<int>>();
        Dictionary<string, int> Dist = new Dictionary<string, int>();
        //Make dictionaries from inputs
        SetLink_Dist(link0, Dist, linkCount);
        bool go = false;
        // game loop
        while (true)
        {
            List<string> outputs = new List<string>();
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
            //Declare Dictionaries
            Dictionary<int,Factory> allyF = new Dictionary<int, Factory>();Dictionary<int, Troop> allyT = new Dictionary<int, Troop>();Dictionary<int, BOMB> allyB = new Dictionary<int, BOMB>();
            Dictionary<int, Factory> enemyF = new Dictionary<int, Factory>();Dictionary<int, Troop> enemyT = new Dictionary<int, Troop>();Dictionary<int, BOMB> enemyB = new Dictionary<int, BOMB>();
            Dictionary<int, Factory> neutralF = new Dictionary<int, Factory>();
            //Fill Dictionaries
            Fill_Dictionary(allyF, enemyF, neutralF, allyT, enemyT, allyB, enemyB,entityCount, link0);
            

            //Check for incoming bomb and make list of best target candidates
            if (enemyB.Count != 0&&Blow_est==0)
            {
                Bomb = true;
                F_ToBeBombed = BOMB_Check(enemyF,enemyB, allyF,F_ToBeBombed);
                Blow_est = Dist[F_ToBeBombed[0]+""+enemyB.ElementAt(0).Value.F_from];
            }
            else if (enemyB.Count==0&&Bomb)
            {
                Bomb = false;
                Blow_est = 0;
                F_ToBeBombed.Clear();
            }
            else if (Blow_est>0)
            {
                Blow_est--;
            }
            //For each Factory
            foreach (Factory f in allyF.Values)
            {
                //if Bomb==true, and f is candidate evacuate to nearest factory
                if (Bomb&&Blow_est==1)
                {
                    if (F_ToBeBombed.Contains(f.ID))
                        outputs.Add(Evacuate(enemyF, allyF, neutralF, f, Dist));
                }
                else
                {
                    //The BIG part. 
                    //Find all adj Factories and make lists
                    Sort_adj adj = new Sort_adj(f.adj, allyF,enemyF,neutralF,f);
                    //Check your safety first
                    int IncomingT = IncomingEnemies(f.ID, enemyT);
                    int canspare = f.numCyborg - IncomingT;

                    if(!(IncomingT>f.numCyborg))
                    {
                        //if sufficient, increase production
                        bool sufficient = Can_IncP(IncomingT,f.numCyborg);
                        if (sufficient&&f.Fproduction<3)
                        {
                            outputs.Add("INC "+f.ID);
                        }
                    }
//find nearby candidate in neutralF 
                    for(int i=0;i<adj.Nadj.Count;i++)
                    {
                        Factory N = neutralF[adj.Nadj[i]];
                        //Let's judge them by production, distance and troops in it.
                        int p=N.Fproduction;
                        int n=N.numCyborg;
                        int d=Dist[N.ID + "" + f.ID];
                        int allyTtoF = AllyT_To_F(N.ID,allyT);
                        int situation = n + IncomingEnemies(N.ID, enemyT)-allyTtoF;
                        //JUDGE!SEND!
                        if(p==3&&d<6)
                            //outputs.Add("MSG " + situation);
                            if (situation >= 0&&canspare>=situation)//we are losing
                            {
                                    outputs.Add("MOVE " + f.ID + " " + N.ID + " " + (situation+1));
                                    canspare -= (situation + 1);
                            }
                        if (p == 2 && d < 5 && (n-allyTtoF) * 2 < f.numCyborg)
                            if (situation >= 0&&canspare + 1 > situation)//we are losing
                            {
                                    outputs.Add("MOVE " + f.ID + " " + N.ID + " " + (situation + 1));
                                    canspare -= (situation + 1);
                            }
                        if(p==1 && d<4 && (n - allyTtoF) * 2<f.numCyborg)
                            if (situation > 0&&canspare + 1 > situation)//we are losing
                            {
                                    outputs.Add("MOVE " + f.ID + " " + N.ID + " " + (situation + 1));
                                    canspare -= (situation + 1);
                            }
                        if (p == 1 && d < 6 &&d>3&& (n - allyTtoF) * 2 < f.numCyborg)
                            if (situation >= 0&&canspare + 1 > situation)//we are losing
                            {
                                    outputs.Add("MOVE " + f.ID + " " + N.ID + " " + (situation + 1));
                                    canspare -= (situation + 1);
                            }
                        if (p==0 && n==0&&d<6)
                            if (situation >= 0&&canspare + 1 > situation)//we are losing
                            {
                                    outputs.Add("MOVE " + f.ID + " " + N.ID + " " + (situation + 1));
                                    canspare -= (situation + 1);
                            }

                    }
//Amend ally
                    for (int i = 0; i < adj.Aadj.Count; i++)    
                    {
                        Factory N = allyF[adj.Aadj[i]];
                        //Let's judge them by production, distance and troops in it.
                        int p = N.Fproduction;
                        int n = N.numCyborg;
                        int d = Dist[N.ID + "" + f.ID];
                        //check their status
                        int allyTtoF = AllyT_To_F(N.ID, allyT);
                        int situation = IncomingEnemies(N.ID, enemyT) - allyTtoF-n;
                        if(situation>0)//This allly needs help
                        {
                            //Check if there is closer ally that can help
                            bool There_is_closer_help = Check_for_CloserCapable_ally();
                            if (canspare+1>situation&&p>0)
                            {
                                  outputs.Add("MOVE " + f.ID + " " + N.ID + " " + (situation + 1));
                                  canspare -= (situation + 1);
                            }
                        }
                            
                        if(canspare>40&&f.Fproduction==3&&p!=3&&d<7)//you can help other Factory to increase capacity
                        {
                            //Do this later: examine threats(Eadj troop numbers) before sending help
                            outputs.Add("MOVE " + f.ID + " " + N.ID + " " + 10);
                            canspare -= 10;
                        }
                    }
//BOMB
                    if(Bomb_stock!=0&&BCoolDown==0)
                    {
                        //send bomb to highest enemy base
                        //Find best Bomb_target
                        BCoolDown = 3;
                        int target = Find_BOMB_target(enemyF,enemyT);
                        //Find closes ally Factory\
                        int source = 50;
                        if (enemyF.ContainsKey(target))
                            source = Find_nearestAF(enemyF[target], Dist, allyF);
                        else if(neutralF.ContainsKey(target))
                            source = Find_nearestAF(neutralF[target], Dist, allyF);
                        if(source!=50)
                        {
                            outputs.Add("BOMB "+source+" "+target);
                            Bomb_stock--;
                        }
                    }
                    else if(BCoolDown==3|| BCoolDown ==2)
                    {
                        foreach (BOMB B in allyB.Values)
                        {
                            if(B.ID!=Bomb_sent)
                            {
                                outputs.Add("MOVE " + B.F_from + " " + B.F_to + " " + 3);
                                canspare -= 3;
                                BCoolDown--;
                            
                                if(BCoolDown==1)
                                    Bomb_sent = B.ID;
                            }
                        }

                    }
                    else if(BCoolDown!=0)
                    {
                        BCoolDown--;
                    }
                    //Atk enemy
                    
                    if(allyF.Count>enemyF.Count||go)
                    {
                        go = true;
                        for (int i = 0; i < adj.Eadj.Count; i++)
                        {
                            Factory N = enemyF[adj.Eadj[i]];
                            int p = N.Fproduction;
                            int n = N.numCyborg;
                            int d = Dist[N.ID + "" + f.ID];
                            //find nearest enemy base
                            int enemy =Find_nearestEF(allyF[f.ID], Dist, enemyF);
                            int ally = Find_nearestAF(enemyF[enemy], Dist, allyF);
                            if(f.ID!=ally&&(f.Fproduction==3)&&canspare>0)
                            {
                                if (Dist[f.ID + "" + enemy] >= Dist[f.ID + "" + ally])
                                    outputs.Add("MOVE " + f.ID + " " + ally + " " + canspare/2);
                                    outputs.Add("MSG " + "sending to ally to atk");
                                    canspare -= canspare/2;
                            }
                            else if(canspare>0)
                            {
                                outputs.Add("MOVE " + f.ID + " " + enemy + " " + (canspare/2));
                                outputs.Add("MSG " + "sending directly");
                                canspare -= canspare / 2;
                            }
                                
                        }
                    }
                    
                }
            }

            //Print outputs
            Print(outputs);       
        }
    }
    public static bool Check_for_CloserCapable_ally()
    {
        return false;
    }
    public static int Find_BOMB_target(Dictionary<int, Factory> enemyF, Dictionary<int, Troop> enemyT)
    {
        int bestT = 0;
        int value = 0;
        foreach(Factory f in enemyF.Values)
        {
            int valuef = f.Fproduction * f.numCyborg + f.Fproduction;
            if (valuef > value)
            {
                 
                bestT = f.ID;
                value = valuef;
                Console.Write(bestT);
            }   
        }
        foreach(Troop t in enemyT.Values)
        {
            int valuef = enemyF[t.F_to].Fproduction * t.Num + enemyF[t.F_to].Fproduction;
            if( valuef> value)
            {
                bestT = t.F_to;
                value = valuef;
            }

        }
        Console.Write(bestT);
        return bestT;
    }
    public static int Find_nearestAF(Factory f, Dictionary<string, int> Dist, Dictionary<int, Factory> allyF)
    {
        int nearestD = 25;
        int nearestF = 0;
        foreach (int a in f.adj)
        {
            int distance = Dist[f.ID + "" + a];
            if (nearestD > distance&&allyF.ContainsKey(a))
            {
                nearestD = distance;
                nearestF = a;
            }
        }
        return nearestF;
    }
    public static int Find_nearestEF(Factory f, Dictionary<string, int> Dist, Dictionary<int, Factory> enemyF)
    {
        int nearestD = 25;
        int nearestF = 0;
        foreach (int a in f.adj)
        {
            int distance = Dist[f.ID + "" + a];
            if (nearestD > distance && enemyF.ContainsKey(a))
            {
                nearestD = distance;
                nearestF = a;
            }
        }
        return nearestF;
    }
    public static bool Can_IncP(int IncomingT,int numCyborg)
    {
        if (IncomingT + 10 < numCyborg && numCyborg > 11)
            return true;
        else
            return false;
    }
    public static int IncomingEnemies(int fID, Dictionary<int, Troop> enemyT)
    {
        int output = 0;
        foreach (Troop a in enemyT.Values)
            if (a.F_to == fID)
                output += a.Num;
        return output;
    }
    public static void SetLink_Dist(Dictionary<int, List<int>> link0, Dictionary<string, int> Dist, int linkCount)
    {
        string[] inputs;
        for (int i = 0; i < linkCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int factory1 = int.Parse(inputs[0]);
            int factory2 = int.Parse(inputs[1]);
            int distance = int.Parse(inputs[2]);
            if (link0.ContainsKey(factory1))
                link0[factory1].Add(factory2);
            else
                link0.Add(factory1, new List<int> { factory2 });
            if (link0.ContainsKey(factory2))
                link0[factory2].Add(factory1);

            else
                link0.Add(factory2, new List<int> { factory1 });
            if (!Dist.ContainsKey(factory1 + "" + factory2))
                Dist.Add(factory1 + "" + factory2, distance);
            if (!Dist.ContainsKey(factory2 + "" + factory1))
                Dist.Add(factory2 + "" + factory1, distance);
        }
    }
    public static void Fill_Dictionary(Dictionary<int, Factory> allyF, Dictionary<int, Factory> enemyF, Dictionary<int, Factory> neutralF, Dictionary<int, Troop> allyT, Dictionary<int, Troop> enemyT, Dictionary<int, BOMB> allyB, Dictionary<int, BOMB> enemyB, int entityCount, Dictionary<int, List<int>> link0)
    {
        for (int i = 0; i < entityCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int entityId = int.Parse(inputs[0]);
            string entityType = inputs[1];
            int arg1 = int.Parse(inputs[2]);
            int arg2 = int.Parse(inputs[3]);
            int arg3 = int.Parse(inputs[4]);
            int arg4 = int.Parse(inputs[5]);
            int arg5 = int.Parse(inputs[6]);
            if (entityType == "FACTORY")
            {
                if (arg1 == 1)
                    allyF.Add(entityId, new Factory(entityId, arg1, arg2, arg3, link0[entityId]));
                else if (arg1 == -1)
                    enemyF.Add(entityId, new Factory(entityId, arg1, arg2, arg3, link0[entityId]));
                else
                    neutralF.Add(entityId, new Factory(entityId, arg1, arg2, arg3, link0[entityId]));
            }
            else if (entityType == "TROOP")
            {
                if (arg1 == 1)
                    allyT.Add(entityId, new Troop(entityId, arg1, arg2, arg3, arg4, arg5));
                else
                    enemyT.Add(entityId, new Troop(entityId, arg1, arg2, arg3, arg4, arg5));
            }
            else if (entityType == "BOMB")
            {
                if (arg1 == 1)
                    allyB.Add(entityId, new BOMB(entityId, arg1, arg2, arg3, arg4));
                else
                    enemyB.Add(entityId, new BOMB(entityId, arg1, arg2, arg3, arg4));
            }
        }
    }
    public static void Print(List<string> outputs)
    {
        if (outputs.Count != 0)
            Console.WriteLine(outputs.Aggregate((i, j) => i + " ;" + j));
        else
            Console.WriteLine("WAIT");
    }
    public static List<int> BOMB_Check(Dictionary<int, Factory> enemyF,Dictionary<int, BOMB> enemyB, Dictionary<int, Factory> allyF, List<int> F_ToBeBombed)
    {
        
            foreach(BOMB a in enemyB.Values)
            {
                List<int> candidates = new List<int>();
                if (enemyF.ContainsKey(a.F_from))
                    candidates = enemyF[a.F_from].adj;
                else
                    candidates = allyF[a.F_from].adj;
                int maxT = 0;
                int F = 0;
                foreach ( int b in candidates)
                {
                    if(allyF.ContainsKey(b))
                    {
                        int num = allyF[b].numCyborg;
                        if (maxT < num)
                        {
                            maxT = num;
                            F = b;
                        }  
                    }
                }
                F_ToBeBombed.Add(F);
            }
        return F_ToBeBombed;
    }
    public static int Find_nearestF(Factory f, Dictionary<string, int> Dist)
    {
        int nearestD = 25;
        int nearestF = 0;
        foreach (int a in f.adj)
        {
            int distance =Dist[f.ID + ""+a];
            if (nearestD > distance)
            {
                nearestD = distance;
                nearestF = a;
            }
        }
        return nearestF;
    }
    public static string Evacuate(Dictionary<int, Factory> enemyF, Dictionary<int, Factory> allyF, Dictionary<int, Factory> neutralF, Factory f, Dictionary<string, int> Dist)
    {
            //move troops to nearest factory
             int n = Find_nearestF(f, Dist);
             return ("MOVE "+ f.ID+ " "+n+ " "+f.numCyborg);
    }
    public static int AllyT_To_F(int ID,Dictionary<int, Troop> allyT)
    {
        int output = 0;
        foreach (Troop a in allyT.Values)
            if (a.F_to == ID)
                output += a.Num;
        return output;
    }
}
public class Factory
{
    public int ID { get; set; }
    public int owner { get; set; }
    public int numCyborg { get; set; }
    public int Fproduction { get; set; }
    public List<int> adj { get; set; }= new List<int>();
    public Factory(int entityId, int a1,int a2,int a3, List<int> a)
    {
        ID = entityId;
        owner = a1;
        numCyborg = a2;
        Fproduction = a3;
        adj = a;
    }
}
public class Troop
{
    public int ID {get; set;}
    public int owner {get; set;}
    public int F_from {get; set;}
    public int F_to {get; set;}
    public int Num {get; set;}
    public int turnsleft {get; set;}
    public Troop(int entityId, int a1, int a2, int a3, int a4,int a5)
    {
        ID = entityId;
        owner = a1;
        F_from = a2;
        F_to = a3;
        Num = a4;
        turnsleft = a5;
    }
}
public class BOMB
{
    public int ID { get; set; }
    public int owner { get; set; }
    public int F_from { get; set; }
    public int F_to { get; set; }
    public int Distance { get; set; }
    public bool Tsent { get; set; }
    public BOMB(int entityId, int a1, int a2, int a3, int a4)
    {
        ID = entityId;
        owner = a1;
        F_from = a2;
        F_to = a3;
        Distance = a4;
    }
}
public class Sort_adj
{
    public List<int> Aadj { get; set; } = new List<int>();
    public List<int> Eadj { get; set; } = new List<int>();
    public List<int> Nadj { get; set; } = new List<int>();
    public Sort_adj(List<int> adj, Dictionary<int, Factory> allyF,Dictionary<int, Factory> enemyF, Dictionary<int, Factory> neutralF, Factory f)
    {
        foreach (int x in f.adj)
        {
            if (enemyF.ContainsKey(x))
                Eadj.Add(x);
            if (neutralF.ContainsKey(x))
                Nadj.Add(x);
            if (allyF.ContainsKey(x))
                Aadj.Add(x);
        }
    }
}
