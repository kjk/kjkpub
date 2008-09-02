/*
 * Author: Krzysztof Kowalczyk (http://blog.kowalczyk.info)
 * 
 * This program is in public domain. Take all the code you like; we'll just write more.
 * 
 * Purpose:
 *   Implements git handling
 * 
 **/
using System;
using System.Collections;
using System.IO;

namespace Git
{
    class GitDiff
    {
        public static ArrayList ParseDiffOut(string diffTxt)
        {
            var al = new ArrayList();
            StringReader    reader = new StringReader(diffTxt);
            string          l;

            while (true)
            {
                l = reader.ReadLine();
                if (null == l)
                    break;
                // parse lines like:
                // :100755 100755 6ca2624... 0000000... M  src/WordNetConverter/Program.cs
                Console.WriteLine("line: '{0}'", l);
                string[] parts = l.Split();
                foreach (var p in parts)
                {
                    Console.WriteLine("part: {0}", p);
                }
            }
            return al;
        }
    }
}