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
using System.Collections.Generic;
using System.IO;
using ScUtil;

namespace Git
{
    class GitDiff
    {
        public static List<FileNameAndRev> ParseDiffOut(string diffTxt)
        {
            var al = new List<FileNameAndRev>();
            StringReader    reader = new StringReader(diffTxt);
            string          l;

            while (true)
            {
                l = reader.ReadLine();
                if (null == l)
                    break;
                // parse lines like:
                // :100755 100755 6ca2624... 0000000... M  src/WordNetConverter/Program.cs
                string[] parts = l.Split();

                /* Console.WriteLine("line: '{0}'", l);
                int n = 0;
                foreach (var p in parts)
                {
                    Console.WriteLine("part {0}: {1}", n, p);
                    n += 1;
                } */
                var cfi = new FileNameAndRev();
                cfi.Revision = parts[2];
                cfi.FileName = parts[5];
                al.Add(cfi);
            }
            return al;
        }
    }
}