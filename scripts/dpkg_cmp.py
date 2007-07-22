# Written by Krzysztof Kowalczyk (http://blog.kowalczyk.info)
# Released to public domain

import sys, os, string

def parse_dpkg_output(file_name):
  d = {} # maps package name to version
  fo = open(file_name, "rb")
  for l in fo:
    lp = l.split()
    if lp[0] != "ii": continue
    if len(lp) < 3: continue
    name = lp[1]
    ver = lp[2]
    d[name] = ver
  fo.close()
  return d

def usage_and_exit():
  print "usage: <dpkg-out-one> <dpkg-out-two>"
  sys.exit(1)

def dodiff(d1,d2):
  different = []
  addone = []
  addtwo = []
  for k in d1.keys():
    if k not in d2:
      addone.append(k)
    else:
      if d1[k] != d2[k]:
        different.append(k)
  for k in d2.keys():
    if k not in d1:
      addtwo.append(k)
  return (different, addone, addtwo)

def main():
  print sys.argv
  if len(sys.argv) != 3:
    usage_and_exit()
  d1 = parse_dpkg_output(sys.argv[1])
  d2 = parse_dpkg_output(sys.argv[2])
  (different, addone, addtwo) = dodiff(d1,d2)
  different.sort()
  addone.sort()
  addtwo.sort()
  if len(different) > 0:
    print "values that are different in one and two"
  for k in different:
    print "  %s (%s vs %s)" % (k, d1[k], d2[k])
  if len(addone) > 0:
    print "packages only available in %s:" % sys.argv[1]
  for k in addone:
    print "  %s" % k
  if len(addtwo) > 0:
    print "packages only available in %s:" % sys.argv[2]
  for k in addtwo:
    print "  %s" % k

if __name__ == "__main__":
  main()
