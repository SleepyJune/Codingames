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
class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());

        int sum = 0;

        int xMax = 0;
        int xMin = int.MaxValue;

        var yValues = new int[N];

        for (int i = 0; i < N; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]);
            int Y = int.Parse(inputs[1]);

            xMin = Math.Min(xMin, X);
            xMax = Math.Max(xMax, X);

            yValues[i] = Y;

            sum += Y;
        }

        Array.Sort(yValues);

        var medium = yValues[N / 2];

        long sumY = 0;
        foreach (var y in yValues)
        {
            sumY += Math.Abs(medium - y);

            //Console.WriteLine( Math.Abs(medium - y));
        }

        Console.WriteLine(sumY + xMax - xMin);
    }
}