using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class StopInfo : IEquatable<StopInfo>
{
    public string id;
    public string name;
    public string desc;
    public double lat;
    public double lng;
    public int zone;
    public string url;
    public int type;
    public string mother;

    public Dictionary<StopInfo, double> routes = new Dictionary<StopInfo, double>();

    public double gScore = 9999;
    public double fScore = 9999;

    public StopInfo parent = null;

    public StopInfo(string str)
    {
        var fields = str.Split(',');

        id = fields[0];
        name = fields[1].Substring(1,fields[1].Length-2);//fields[1].Replace("\"","");
        lat = Convert.ToDouble(fields[3]);
        lng = Convert.ToDouble(fields[4]);
        //type = Int32.Parse(fields[7]);
    }

    public double Distance(StopInfo b)
    {
        var a = this;

        double x = (b.lng - a.lng) * Math.Cos(((b.lat + a.lat)/2.0) * Math.PI / 180.0);
        double y = b.lat - a.lat;

        return Math.Sqrt(x * x + y * y) * 6371;
    }

    public override bool Equals(object obj)
    {
        if (obj is StopInfo)
        {
            return Equals((StopInfo)this);
        }

        return false;
    }

    public bool Equals(StopInfo obj)
    {
        return obj.id == this.id;
    }

    public override int GetHashCode()
    {
        return this.id.GetHashCode();
    }
}

class Pathfinding
{
    public static void GetShortestPath(StopInfo start, StopInfo end, Dictionary<string, StopInfo> stopList)
    {
        HashSet<StopInfo> closedSet = new HashSet<StopInfo>();
        HashSet<StopInfo> openSet = new HashSet<StopInfo>();

        openSet.Add(start);
        
        /*foreach (var stop in stopList.Values)
        {
            stop.gScore = 9999;
            stop.fScore = 9999;
            stop.parent = null;
            //prev.Add(stop, null);
        }*/

        start.gScore = 0;
        start.fScore = start.Distance(end);

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(n => n.fScore).FirstOrDefault();

            if (current == end)
            {
                PrintPath(current);
                return;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var pair in current.routes)
            {
                var neighbour = pair.Key;
                var distance = pair.Value;

                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                var alternativeDistance = current.gScore + distance;
                if (!openSet.Contains(neighbour))
                {
                    openSet.Add(neighbour);
                }
                else if (alternativeDistance >= neighbour.gScore)
                {
                    continue;
                }

                neighbour.parent = current;
                neighbour.gScore = alternativeDistance;
                neighbour.fScore = alternativeDistance + neighbour.Distance(end);

            }
        }

        Console.WriteLine("IMPOSSIBLE");
    }

    public static void PrintPath(StopInfo current)
    {
        List<string> pathNames = new List<string>();

        while (current != null)
        {
            pathNames.Add(current.name);
            current = current.parent;
        }

        pathNames.Reverse();
        pathNames.ForEach(n => Console.WriteLine(n));
    }
}

class Solution
{
    static void Main(string[] args)
    {
        Dictionary<string, StopInfo> stopList = new Dictionary<string, StopInfo>();

        string startPoint = Console.ReadLine();
        string endPoint = Console.ReadLine();
        
        int N = int.Parse(Console.ReadLine());

        for (int i = 0; i < N; i++)
        {
            string stopName = Console.ReadLine();
            StopInfo newStop = new StopInfo(stopName);
            stopList.Add(newStop.id, newStop);
        }
        int M = int.Parse(Console.ReadLine());
        for (int i = 0; i < M; i++)
        {
            string route = Console.ReadLine();
            var path = route.Split(' ');

            var start = stopList[path[0]];
            var end = stopList[path[1]];

            var dist = start.Distance(end);

            start.routes.Add(end,dist);        
        }

        var a = stopList[startPoint];
        var b = stopList[endPoint];

        Pathfinding.GetShortestPath(a, b, stopList);        
    }
}