#!/usr/bin/env python
# Written by Krzysztof Kowalczyk (http://blog.kowalczyk.info)
# Code is Public Domain. Take all the code you want, we'll just write more.

import sys, os, os.path, subprocess, string

def usage():
  print("Usage: %s [git-dir]" % __file__)
  sys.exit(1)

def run_cmd(*args):
  cmd = " ".join(args)
  try:
    cmdproc = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    res = cmdproc.communicate()
    stdout = res[0]
    stderr = res[1]
  except:
    print "Failed to execute!"
    return (1, "Failed to execute '%s'" % cmd, "")
  errcode = cmdproc.returncode
  return (errcode, stdout, stderr)

# parses "git log" output and returns dict mapping date (in format YYYY-MM-DD)
# to an array of names who did checkins that day (there will be duplicate names,
# if the same person did more than one checkin)
def parse_git_log(stdout):
  lines = string.split(stdout, "\n")
  date_to_name = {}
  author = None
  for l in lines:
    if author:
      # previous line was "Author"
      if not l.startswith("Date:"):
        print("parse_git_log: expected Date: line and got '%s'" % l)
      else:
        date = l.split()[1]
        if date in date_to_name:
            date_to_name[date].append(author)
        else:
            date_to_name[date] = [author]
      author = None
    if l.startswith("Author:"):
      author = l.split(": ")[1]
  return date_to_name

def dump_stats_for_name(name, stats):
  total_checkins = sum(stats.values())
  no_days = len(stats)
  days = stats.keys()
  days.sort()
  print("Stats for: %s" % name)
  for day in days:
    print("%s: %d" % (day, stats[day]))
  print("Total checkins: %d" % total_checkins)
  print("Total days    : %d" % no_days)

def git_log_stats(date_to_name):
  stats_for_name = {}
  for (date, names) in date_to_name.items():
    for name in names:
      if name not in stats_for_name:
        stats_for_name[name] = {}
      stats = stats_for_name[name]
      if date not in stats:
        stats[date] = 0
      stats[date] = stats[date] + 1
  for name in stats_for_name:
    dump_stats_for_name(name, stats_for_name[name])

def main():
  if len(sys.argv) > 2: usage()
  if len(sys.argv) == 2:
    os.chdir(sys.argv[1])
  if not os.path.exists(".git"):
    print("Directory .git doesn't exist!")
    usage()
  cmd = "git log --date=iso"
  (err, stdout, stderr) = run_cmd("git", "log", "--date=iso")
  if err != 0:
    print("Failed to run '%s', returned error %d" % (cmd, err))
    sys.exit(1)
  stats = parse_git_log(stdout)
  git_log_stats(stats)

if __name__ == "__main__":
  main()
