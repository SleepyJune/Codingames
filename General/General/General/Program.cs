using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int seats = int.Parse(inputs[0]); //seats
        int runs = int.Parse(inputs[1]); //runs
        int groups = int.Parse(inputs[2]); //groups

        Ride firstRide = null;
        Ride prev = null;

        float startTime = Timer.TickCount;

        Console.Error.WriteLine("seats: " + seats + " runs: " + runs);

        //Console.Error.Write("Groups");
        for (int i = 0; i < groups; i++)
        {
            int pi = int.Parse(Console.ReadLine());

            if (firstRide == null)
            {
                firstRide = new Ride(pi);
                prev = firstRide;
            }
            else
            {
                Ride newRide = new Ride(pi);
                prev.nextRide = newRide;
                prev = newRide;

            }
            
            //Console.Error.Write(pi + " ,");
        }
        //Console.Error.WriteLine("");
        
        prev.nextRide = firstRide;

        Ride currentRide = firstRide;

        Dictionary<Ride, RideMemory> rideMemories = new Dictionary<Ride, RideMemory>();

        double total = 0;
        for (int i = 0; i < runs; i++)
        {
            if (rideMemories.ContainsKey(currentRide))
            {
                var mem = rideMemories[currentRide];

                total += mem.people;
                currentRide = mem.nextRide;
            }
            else
            {
                int numSeats = 0;

                //Console.Error.Write("Rides: ");

                Ride firstRideKey = currentRide;
                Ride firstAppearance = null;

                while (currentRide != null
                    && currentRide != firstAppearance
                    && numSeats + currentRide.people <= seats)
                {
                    if (firstAppearance == null)
                    {
                        firstAppearance = currentRide;
                    }

                    numSeats += currentRide.people;
                    total += currentRide.people;
                    currentRide = currentRide.nextRide;

                    //Console.Error.Write(currentRide.people + ", ");
                }
                //Console.Error.WriteLine(" Total: " + numSeats);

                RideMemory newMem = new RideMemory(numSeats, currentRide);
                rideMemories.Add(firstRideKey, newMem);
            }
        }

        Console.WriteLine(total);

        var finalTime = Timer.TickCount - startTime;

        Console.Error.WriteLine("Time: " + finalTime);
    }

    public class Ride
    {
        public Ride nextRide;

        public int people;

        public Ride(int people)
        {
            this.people = people;
        }
    }

    public class RideMemory
    {
        public Ride nextRide;

        public int people;

        public RideMemory(int people, Ride nextRide)
        {
            this.people = people;
            this.nextRide = nextRide;
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
}