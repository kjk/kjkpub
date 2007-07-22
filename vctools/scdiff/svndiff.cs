/*
 * Author: Krzysztof Kowalczyk (http://blog.kowalczyk.info)
 * 
 * This program is in public domain. Take all the code you like; we'll just write more.
 * 
 * Purpose:
 *   Implements subversion handling
 * 
 **/
using System;
using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Svn
{
    class svndiff
    {
        public static string quoteFileName(string fileName)
        {
            if (-1 != fileName.IndexOf(' '))
            {
                return "\"" + fileName + "\"";
            }
            else
            {
                return fileName;
            }
        }

        public static string SvnCat(string fileName, string rev)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "svn";
            process.StartInfo.Arguments = String.Format("cat -r {0} {1}", rev, quoteFileName(fileName));
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
            try 
            {
                process.Start();
            } 
            catch(Win32Exception) 
            {
                // subversion not installed
                Console.WriteLine("Couldn't execute 'svn cat', is subversion installed and available in command line?");
                return null;
            }
            string output = process.StandardOutput.ReadToEnd();
            return output;
        }

        public static Stream GetSvnRevisionStream(string fileName, string rev)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "svn";
            process.StartInfo.Arguments = String.Format("cat -r {0} {1}", rev, quoteFileName(fileName));
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
            try 
            {
                process.Start();
            } 
            catch(Win32Exception) 
            {
                // subversion not installed
                Console.WriteLine("Couldn't execute 'svn cat', is subversion installed and available in command line?");
                return null;
            }
            return process.StandardOutput.BaseStream;
        }

        static string indexTxt  = "Index: ";
        static string sepTxt    = "===================================================================";
        static string binaryTxt = "Cannot display: file marked as a binary type.";

        enum ParseState 
        {
            EXPECT_INDEX = 0,
            AFTER_INDEX,
            SKIP_DIFF,
            AFTER_SEP,
            AFTER_REV
        };

        // Given an output of 'svn diff' command, return an array of strings, 2 strings for each
        // file that contains a diff, first string is a file name, second is a revision against which
        // the local copy diff is made
        public static ArrayList ExtractSvnDiffInfo(string diffTxt)
        {
            StringReader reader = new StringReader(diffTxt);
            ArrayList    res = new ArrayList();
            ParseState   state = ParseState.EXPECT_INDEX;
            string       txt;
            string       fileName = null;

            while (true)
            {
                txt = reader.ReadLine();
                if (null==txt)
                    break;

                switch(state)
                {
                    case ParseState.EXPECT_INDEX:
                        if (! txt.StartsWith(indexTxt))
                        {
                            // shouldn't happen
                            return res;
                        }
                        fileName = txt.Substring(indexTxt.Length);
                        state = ParseState.AFTER_INDEX;
                        break;
                    case ParseState.AFTER_INDEX:
                        if (! txt.StartsWith(sepTxt))
                        {
                            // assumption: we might get indexTxt in the body of the diff
                            // so we allow it to not be followed by sepTxt. If it's not
                            // then we'll go into diff skipping mode
                            // this should be very rare, thought!
                            Debug.Assert(false, "Did we really get this in the diff?");
                            state = ParseState.SKIP_DIFF;
                            break;
                        }
                        state = ParseState.AFTER_SEP;
                        break;
                    case ParseState.AFTER_SEP:
                        if (txt.StartsWith(binaryTxt))
                        {
                            state = ParseState.SKIP_DIFF;
                            break;
                        }
                        Debug.Assert( txt.StartsWith("---") );
                        Debug.Assert( -1 != txt.IndexOf(fileName) );
                        Match match = Regex.Match(txt, @"\(revision[^\d]+(\d+)\)");
                        string rev = match.Groups[1].Value;
                        res.Add(fileName);
                        res.Add(rev);
                        state = ParseState.AFTER_REV;
                        break;
                    case ParseState.AFTER_REV:
                        Debug.Assert( txt.StartsWith("+++") );
                        Debug.Assert( -1 != txt.IndexOf(fileName) );
                        state = ParseState.SKIP_DIFF;
                        break;
                    case ParseState.SKIP_DIFF:
                        if (txt.StartsWith(indexTxt))
                        {
                            state = ParseState.AFTER_INDEX;
                            fileName = txt.Substring(indexTxt.Length);
                        }
                        break;
                    default:
                        Debug.Assert(false, String.Format("Unkown state {0}",state));
                        break;
                }
            }
            return res;
        }

        public static ArrayList ExtractSvnDiffFromFile(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            string svnDiffTxt = reader.ReadToEnd();
            reader.Close();
            return ExtractSvnDiffInfo(svnDiffTxt);
        }

        public static void TestSvnDiffExtract()
        {
            ArrayList res;
            string filePath = @"c:\kjk\src\mine\sctools\svndiff\tests";
            res = ExtractSvnDiffFromFile(System.IO.Path.Combine(filePath,"svnDiffRes1.txt"));
            Debug.Assert(res.Count==2);
            Debug.Assert((string)res[0]=="verifyRedirects.py");
            Debug.Assert((string)res[1]=="295");

            res = ExtractSvnDiffFromFile(System.IO.Path.Combine(filePath,"svnDiffRes2.txt"));
            Debug.Assert(res.Count==4);
            Debug.Assert((string)res[0]=="converter.py");
            Debug.Assert((string)res[1]=="295");
            Debug.Assert((string)res[2]=="verifyRedirects.py");
            Debug.Assert((string)res[3]=="295");

            res = ExtractSvnDiffFromFile(System.IO.Path.Combine(filePath,"svnDiffBin.txt"));
            Debug.Assert(res.Count==0);
        }
    }
}
