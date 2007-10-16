#!/usr/bin/python

import sys, string, smtplib, time, datetime, os, os.path, subprocess

def exec_cmd(*args):
    try:
        output = subprocess.Popen(args, stdout=subprocess.PIPE).communicate()[0]
    except:
        return None
    return output.strip()

def parse_svn_diff(txt):
    added = 0
    removed = 0
    for l in txt.split():
        #print l
        if 0 == len(l): continue
        if '+' == l[0]: added += 1
        elif '-' == l[0]: removed += 1
    return (added, removed)

def do_svn_diff():
    return exec_cmd("svn", "diff")

def main():
    output = do_svn_diff()
    if None == output:
        print "svn diff failed"
        sys.exit(1)
    (added, removed) = parse_svn_diff(output)
    net = added - removed
    print "Net    : %d\nAdded  : %d\nRemoved: %d\n" % (net, added, removed)

if __name__ == "__main__":
    main()
