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
    public static Dictionary<string, string> seqTable = new Dictionary<string, string>();

    public static string GetSequenceFirst(string str1, string str2)
    {
        if (str1.Length != str2.Length)
        {            
            int minLength = Math.Min(str1.Length, str2.Length);
            int maxLength = Math.Max(str1.Length, str2.Length);

            int diff = maxLength - minLength;
            string longerStr = str1.Length > str2.Length ? str1 : str2;
            string shorterStr = str1.Length < str2.Length ? str1 : str2;

            if (longerStr.IndexOf(shorterStr) >= 0)
            {
                return longerStr;
            }

            var diffStr = longerStr.Substring(0, diff);
            var diffStr2 = longerStr.Substring(minLength, diff);

            if (str1.Length > str2.Length)
            {
                var ret1 = diffStr + GetSequence(str1, str2, minLength);
                var ret2 = GetSequence(str2, str1, minLength) + diffStr2;
                return ret1.Length > ret2.Length ? ret2 : ret1;
            }
            else
            {
                var ret1 = GetSequence(str1, str2, minLength) + diffStr2;
                var ret2 = diffStr + GetSequence(str2, str1, minLength);
                return ret1.Length > ret2.Length ? ret2 : ret1;
            }
            
        }
        else
        {
            var ret1 = GetSequence(str1, str2, str1.Length);
            var ret2 = GetSequence(str2, str1, str1.Length);

            return ret1.Length > ret2.Length ? ret2 : ret1;
        }
    }

    public static string GetSequence(string str1, string str2, int step)
    {
        string a = GetSubstring(str1, step, true);
        string b = GetSubstring(str2, step, false);

        //Console.Error.WriteLine(a + " " + b);

        if (seqTable.ContainsKey(a + b))
        {
            return seqTable[a + b];
        }

        if (a + b == b + a)
        {
            seqTable[a + b] = b;
            return b;
        }

        if (a == b)
        {
            seqTable[a + b] = a;
            return a;
        }
        else
        {
            var c = a.Length > 0 ? a[0] + "" : "";
            var d = b.Length > 0 ? b[b.Length - 1] + "" : "";
            var min = c + GetSequence(str1, str2, step - 1) + d;
            //Console.Error.WriteLine("min: " + min);

            seqTable[a + b] = min;
            return min;
        }
    }

    public static string GetSubstring(string str, int step, bool fromBack)
    {
        if (fromBack)
        {
            return str.Substring(Math.Max(0, str.Length - step));
        }
        else
        {
            return str.Substring(0, step);
        }
    }

    static void Main(string[] args)
    {
        string lastSeq = "lastseq";
        int N = int.Parse(Console.ReadLine());

        HashSet<string> sequenceList = new HashSet<string>();

        for (int i = 0; i < N; i++)
        {
            string subseq = Console.ReadLine();
            sequenceList.Add(subseq);

            //Console.Error.WriteLine("seq: " + subseq);
        }

        int max = 0;
        string current = sequenceList.OrderByDescending(s=>s.Length).FirstOrDefault();
        sequenceList.Remove(current);
        while (sequenceList.Count > 0)
        {
            int min = 99999;
            string minSeq = current;
            string finalSeq = current;

            foreach (var seq in sequenceList)
            {
                var minStr = GetSequenceFirst(current, seq);

                seqTable[current + seq] = minStr;
                
                if (minStr.Length <= min)
                {
                    min = minStr.Length;
                    minSeq = seq;
                    finalSeq = minStr;

                    Console.Error.WriteLine("current: " + finalSeq);
                }
            }

            sequenceList.Remove(minSeq);
            current = finalSeq;
            
        }

        Console.WriteLine(current.Length);
    }
}