/*
 * Author: Krzysztof Kowalczyk (http://blog.kowalczyk.info)
 * 
 * This program is in public domain. Take all the code you like; we'll just write more.
 * 
 * Purpose:
 *   Implements the cvs handling.
 * 
 **/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using ScUtil;

// TODO: test how it handles adding new files
namespace Cvs
{
    class cvsdiff
    {
        string cvsProgram_ = null;
        string cvsOptions_ = null;

        public cvsdiff(string cvsProgram, string cvsOptions)
        {
            cvsProgram_ = cvsProgram;
            cvsOptions_ = cvsOptions;
        }
        // Return a given revision ref of a file fileName as a string
        // Using "cvs -z3 update -p -r $rev $file" command
        public string CvsCat(string fileName, string rev)
        {
            
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = cvsProgram_;
            process.StartInfo.Arguments = cvsOptions_ + String.Format(" update -p -r {0} {1}", rev, fileName);
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
            try 
            {
                process.Start();
            } 
            catch(Win32Exception) 
            {
                Console.WriteLine("Couldn't execute '{0} {1}', is cvs installed and available in command line?", process.StartInfo.FileName, process.StartInfo.Arguments);
                return null;
            }
            string output = process.StandardOutput.ReadToEnd();
            return output;
        }

        public Stream GetCvsRevisionStream(string fileName, string rev)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = cvsProgram_;
            string revision = string.Empty;
            if (rev.Length > 0)
                revision = "-r " + rev;
            process.StartInfo.Arguments = cvsOptions_ + String.Format(" update -p {0} {1}", revision, fileName);
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
            try 
            {
                process.Start();
            } 
            catch(Win32Exception) 
            {
                Console.WriteLine("Couldn't execute '{0} {1}', is cvs installed and available in command line?", process.StartInfo.FileName, process.StartInfo.Arguments);
                return null;
            }
            return process.StandardOutput.BaseStream;
        }

        static string indexTxt = "Index: ";
        static string sepTxt = "===================================================================";

        enum ParseState 
        {
            EXPECT_INDEX = 0,
            AFTER_INDEX,
            SKIP_DIFF,
            AFTER_SEP,
            AFTER_REV
        };

        // Given an output of 'cvs diff -u' command, return an array of strings, 2 strings for each
        // file that contains a diff, first string is a file name, second is a revision against which
        // the local copy diff is made
        public static List<FileNameAndRev> ExtractCvsDiffInfo(string diffTxt)
        {
            StringReader    reader = new StringReader(diffTxt);
            var             res = new List<FileNameAndRev>();
            ParseState      state = ParseState.EXPECT_INDEX;
            string          txt;
            string          fileName = null;

            while (true)
            {
                txt = reader.ReadLine();
                if (null==txt)
                    break;

                switch(state)
                {
                    case ParseState.EXPECT_INDEX:
                        // that's how non-cvs files are marked
                        if ( txt.StartsWith("? ") )
                        {
                            // file unknown to CVS
                            break;
                        }

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
                        Debug.Assert( txt.StartsWith("RCS file:") );
                        Debug.Assert( -1 != txt.IndexOf(fileName) );
                        state = ParseState.AFTER_REV;
                        break;
                    case ParseState.AFTER_REV:
                        Debug.Assert( txt.StartsWith("retrieving revision") );
                        Match match = Regex.Match(txt, @"retrieving revision[^\d]+([\d\.]+)");
                        string rev = match.Groups[1].Value;
                        var fi = new FileNameAndRev();
                        fi.FileName = fileName;
                        fi.Revision = rev;
                        res.Add(fi);
                        // TODO: should also make sure that this is followed by:
                        // diff -u -r...
                        // --- FileName ...
                        // +++ FileName ...
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

        public static void TestCvsDiffExtract()
        {
            string filePath = @"c:\kjk\src\mine\sctools\cvsdiff";
            string fileName = "cvsDiffRes1.txt";
            string fullPath = System.IO.Path.Combine(filePath,fileName);
            StreamReader reader = new StreamReader(fullPath);
            string diffTxt = reader.ReadToEnd();
            reader.Close();
            var res = ExtractCvsDiffInfo(diffTxt);
            Debug.Assert(res.Count==1);
            Debug.Assert(res[0].FileName == "Src/Merge.rc");
            Debug.Assert(res[0].Revision == "1.198");
        }
    }
}
