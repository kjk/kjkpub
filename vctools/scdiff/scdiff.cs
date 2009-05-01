/*
 * Author: Krzysztof Kowalczyk (http://blog.kowalczyk.info)
 * 
 * This program is in public domain. Take all the code you like; we'll just write more.
 * 
 * Purpose:
 *   This programs does a gui diff (using an external diff program) of local changes
 *   against subversion or cvs or git repository.
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

/*
TODO:
 * add exception handling e.g. for a case where temp directory is in use by other process. currently we "crash"
   we should display a nice error message
 * use built-in diff viewer instead of windiff/etc.
 **/
using Cvs;
using Svn;
using Git;

namespace Scdiff
{
    class Scdiff
    {
        private string  tempDir;
        private string  diffProgram_;
        private string  cvsProgram_;
        private string  cvsOptions_;
        private string  diffOptions_;
        private bool    fOld_;
        private Process process = null;
        private string  tempDirBefore;
        private string  tempDirAfter;

        [STAThread]
        static void Main(string[] args)
        {
            string diffProgram = "windiff";
            string cvsProgram = "cvs -z3";
            string diffOptions = "-u -N";
            bool fOld=false;
            bool fSkipNext = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (fSkipNext)
                {
                    fSkipNext = false;
                    continue;
                }

                if (args[i]=="-h" || args[i]=="-help" || args[i]=="--help" || args[i]=="/?" || args[i]=="/h" || args[i] == "-v" || args[i] == "--version")
                {
                    Usage();
                    return;
                }

                if (args[i]=="-diff")
                {
                    if (args.Length<i+1)
                    {
                        Usage();
                        return;
                    }
                    diffProgram = args[i+1];
                    Console.WriteLine("Using diff program {0}", diffProgram);
                }

                if (args[i]=="-cvs")
                {
                    if (args.Length<i+1)
                    {
                        Usage();
                        return;
                    }
                    cvsProgram = args[i+1].Trim('\"');
                }

                if (args[i]=="-cvsargs")
                {
                    if (args.Length<i+1)
                    {
                        Usage();
                        return;
                    }
                    diffOptions = args[i+1].Trim('\"');
                }

                if (args[i]=="-old")
                    fOld=true;
            }

            var sc = new Scdiff();
            sc.fOld_ = fOld;
            sc.diffProgram_ = diffProgram;
            sc.diffOptions_ = diffOptions;

            string[] cvsCommands = cvsProgram.Split(" ".ToCharArray(), 2);
            sc.cvsProgram_ = cvsCommands[0].Trim();
            if (2 == cvsCommands.Length)
                sc.cvsOptions_ = cvsCommands[1].Trim();
            sc.run();
        }

        public Scdiff()
        {
        }

        public void run()
        {
            tempDir = System.Environment.GetEnvironmentVariable("TEMP");
            if (null==tempDir)
            {
                Console.WriteLine("Couldn't obtain temporary directory (result of 'echo %TEMP%'). Can't continue.");
                return;
            }
            Console.WriteLine("Using temporary directory: {0}", tempDir);
            // tempDirBefore is where the original files from repository are,
            // tempDirAfter is where we put our (modified) working copy
            tempDirBefore = System.IO.Path.Combine(tempDir, "sc_original");
            tempDirAfter  = System.IO.Path.Combine(tempDir, "sc_altered");

            if (Directory.Exists(".svn"))
            {
                DoSvn();
                return;
            }

            if (Directory.Exists("CVS"))
            {
                DoCvs();
                return;
            }

            if (IsGitDirectory())
            {
                DoGit();
                return;
            }
            Console.WriteLine("Doesn't look like svn, cvs or git repository.");
        }

        enum Action
        {
            Unknown,
            Added,
            Deleted,
            Modified
        };

        bool IsGitDirectory()
        {
            process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = "rev-parse --is-inside-work-tree";
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
            try
            {
                process.Start();
            }
            catch (Win32Exception)
            {
                // git not installed
                Console.WriteLine("Couldn't execute '{0} {1}', is git installed and available in command line?", process.StartInfo.FileName, process.StartInfo.Arguments);
                return false;
            }
            string s = process.StandardOutput.ReadToEnd();
            s = s.Trim();
            if (s == "true")
                return true;
            return false;
        }

        void DoGit()
        {
            if (fOld_)
                goto JustDiff;
            process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = "diff --raw";
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            try
            {
                process.Start();
            }
            catch (Win32Exception)
            {
                // git not installed
                Console.WriteLine("Couldn't execute '{0} {1}', is git installed and available in command line?", process.StartInfo.FileName, process.StartInfo.Arguments);
                return;
            }

            string s = process.StandardOutput.ReadToEnd();
            var fileList = GitDiff.ParseDiffOut(s);
            if (0 == fileList.Count)
            {
                Console.WriteLine("There are no diffs!");
                return;
            }
            PrepareTempDirs();

            for (int i = 0; i < fileList.Count; i++)
            {
                var fi = fileList[i];
                var rev = fi.Revision;
                var fileNameIn = fi.FileName;
                /* strip trailing "..." from the end since git show
                 * interprets them as range */
                rev = rev.Trim(".".ToCharArray());
                Console.WriteLine("Will show {0}", rev);
                string fileNameOut = fileNameIn.Replace(@"/", @"\");
                string pathOutAfter = System.IO.Path.Combine(tempDirAfter, fileNameOut);
                EnsureDirForFilePathExists(pathOutAfter);

                // copy working copy to tempDirAfter
                if (File.Exists(fileNameOut))
                {
                    File.Copy(fileNameOut, pathOutAfter);
                }

                // copy revision to tempDirBefore
                string pathOutBefore = System.IO.Path.Combine(tempDirBefore, fileNameOut);
                EnsureDirForFilePathExists(pathOutBefore);

                FileStream streamOut = File.OpenWrite(pathOutBefore);

                Stream streamIn = GitDiff.GetRevisionStream(rev);
                int bufSize = 2048;
                byte[] buf = new byte[bufSize];
                int read;
                while (true)
                {
                    read = streamIn.Read(buf, 0, bufSize);
                    if (0 == read)
                        break; // end of file
                    streamOut.Write(buf, 0, read);
                }
                streamOut.Close();
            }

JustDiff:
            process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.FileName = diffProgram_;
            process.StartInfo.Arguments = String.Format("{0} {1}", tempDirBefore, tempDirAfter);
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            try
            {
                process.Start();
            }
            catch (Win32Exception)
            {
                Console.WriteLine("Couldn't execute '{0}'", diffProgram_);
            }

        }

        void EnsureDirForFilePathExists(string filePath)
        {
            string dirName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
        }

        void DoCvs()
        {
            if (fOld_)
                goto JustDiff;

            process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = cvsProgram_;
            process.StartInfo.Arguments = cvsOptions_ + " diff " + diffOptions_;
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            try 
            {
                process.Start();
            } 
            catch(Win32Exception) 
            {
                // cvs not installed
                Console.WriteLine("Couldn't execute '{0} {1}', is cvs installed and available in command line?", process.StartInfo.FileName, process.StartInfo.Arguments);
                return;
            }
            string output = process.StandardOutput.ReadToEnd();
            var fileList = CvsDiff.ParseDiffOutput(output);
            if (0==fileList.Count)
            {
                Console.WriteLine("There are no diffs!");
                return;
            }

            PrepareTempDirs();

            for (int i = 0; i < fileList.Count; i++)
            {
                string fileNameIn = fileList[i].FileName;
                string rev = fileList[i].Revision;

                // change Unix path into Windows path. Hope this is correct
                string fileNameOut = fileNameIn.Replace(@"/", @"\");

                string pathOutAfter = System.IO.Path.Combine(tempDirAfter,fileNameOut);
                EnsureDirForFilePathExists(pathOutAfter);

                Action a = Action.Unknown;

                if( Directory.Exists( fileNameOut ) )
                {
                  FileSystem.CopyDirectory( fileNameOut, pathOutAfter );
                  a = Action.Modified;
                }
                else
                {
                    // copy working copy to tempDirAfter
                    if (File.Exists(fileNameOut))
                    {
                        if (0 == rev.Length)
                            a = Action.Added;
                        else
                            a = Action.Modified;
                        File.Copy(fileNameOut, pathOutAfter);
                    }
                    else
                    {
                       a = Action.Deleted;
                    }
               }
                // copy revision to tempDirBefore

                switch(a)
                {
                    case Action.Added:
                        Console.Write("+ ");
                        break;
                    case Action.Deleted:
                        Console.Write("- ");
                        break;
                    case Action.Modified:
                        Console.Write("* ");
                        break;
                }

                Console.WriteLine(fileNameIn);

                if (a != Action.Added)
                {
                    string pathOutBefore = System.IO.Path.Combine(tempDirBefore,fileNameIn);
                    EnsureDirForFilePathExists(pathOutBefore);
                    FileStream streamOut = File.OpenWrite(pathOutBefore);

                    var diff = new CvsDiff(cvsProgram_, cvsOptions_);
                    Stream streamIn = diff.GetRevisionStream(fileNameIn, rev);
                    int bufSize = 2048;
                    byte [] buf = new byte[bufSize];
                    int read;
                    while (true)
                    {
                        read = streamIn.Read(buf,0,bufSize);
                        if (0==read)
                            break; // end of file
                        streamOut.Write(buf,0,read);
                    }
                    streamOut.Close();
                }
            }
JustDiff:
            process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.FileName = diffProgram_;
            process.StartInfo.Arguments = String.Format("{0} {1}", tempDirBefore, tempDirAfter);
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            try 
            {
                process.Start();
            } 
            catch(Win32Exception) 
            {
                Console.WriteLine("Couldn't execute '{0}'", diffProgram_);
            }
        }

        void PrepareTempDirs()
        {
            if (Directory.Exists(tempDirBefore))
                Directory.Delete(tempDirBefore, true);
            Directory.CreateDirectory(tempDirBefore);

            if (Directory.Exists(tempDirAfter))
                Directory.Delete(tempDirAfter, true);
            Directory.CreateDirectory(tempDirAfter);
        }

        void DoSvn()
        {
            if (fOld_)
                goto JustDiff;

            process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "svn";
            process.StartInfo.Arguments = "diff";
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            try 
            {
                process.Start();
            } 
            catch (Win32Exception) 
            {
                // subversion not installed
                Console.WriteLine("Couldn't execute '{0} {1}', is subversion installed and available in command line?", process.StartInfo.FileName, process.StartInfo.Arguments);
                return;
            }
            string output = process.StandardOutput.ReadToEnd();
            var fileList = SvnDiff.ParseDiffOutput(output);
            if (0==fileList.Count)
            {
                Console.WriteLine("There are no diffs!");
                return;
            }

            PrepareTempDirs();

            for (int i = 0; i < fileList.Count; i++)
            {
                string fileNameIn = fileList[i].FileName;
                string rev = fileList[i].Revision;

                // change Unix path into Windows path. Hope this is correct
                string fileNameOut = fileNameIn.Replace(@"/", @"\");
                string pathOutAfter = System.IO.Path.Combine(tempDirAfter,fileNameOut);
                EnsureDirForFilePathExists(pathOutAfter);

                // copy working copy to tempDirAfter
                if (File.Exists(fileNameOut))
                {
                    try 
                    {
                        File.Copy(fileNameOut, pathOutAfter);
                    } 
                    catch (FileNotFoundException)
                    {
                        // shouldn't happen, but this is multi-user system
                    }
                }
                else
                {
                    // this must be the case of deleting a file in svn
                    // we don't have a good solution for that, so just ignore this case
                    continue;
                }

                // revision "0" means we're adding a new file, so there's nothing to get
                // from the repository
                if (rev!="0")
                {
                    // copy revision to tempDirBefore
                    string pathOutBefore = System.IO.Path.Combine(tempDirBefore,fileNameOut);
                    EnsureDirForFilePathExists(pathOutBefore);
                    FileStream streamOut = File.OpenWrite(pathOutBefore);

                    Stream streamIn = SvnDiff.GetRevisionStream(fileNameIn, rev);
                    int bufSize = 2048;
                    byte [] buf = new byte[bufSize];
                    int read;
                    while (true)
                    {
                        read = streamIn.Read(buf,0,bufSize);
                        if (0==read)
                            break; // end of file
                        streamOut.Write(buf,0,read);
                    }
                    streamOut.Close();
                }
            }

JustDiff:
            process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.FileName = diffProgram_;
            process.StartInfo.Arguments = String.Format("{0} {1}", tempDirBefore, tempDirAfter);
            Console.WriteLine("Executing {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            try
            {
                process.Start();
            }
            catch(Win32Exception) 
            {
                Console.WriteLine("Couldn't execute '{0}'", diffProgram_);
            }
        }

        public static void Usage()
        {
            Console.WriteLine("scdiff v0.5 usage: scdiff [-h] [-old] [-cvs cvsCommand] [-cvsargs cvsOptions] [-diff diffProgram]");
        }
    }

    /// <summary>
    /// Copied from http://www.codeproject.com/csharp/copydirectoriesrecursive.asp
    /// </summary>
    public class FileSystem
    {
        // Copy directory structure recursively
        public static void CopyDirectory(string src, string dst)
        {
            String[] files;

            if (dst[dst.Length-1] != Path.DirectorySeparatorChar)
                dst += Path.DirectorySeparatorChar;

            if (!Directory.Exists(dst))
                Directory.CreateDirectory(dst);

            files = Directory.GetFileSystemEntries(src);

            foreach (string Element in files)
            {
                if (Directory.Exists(Element))
                {
                    // Sub directories
                    CopyDirectory(Element, dst+Path.GetFileName(Element));
                }
                else
                {
                    // Files in directory
                    File.Copy(Element, dst+Path.GetFileName(Element), true);
                }
            }
        }
    }
}
