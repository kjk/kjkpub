#!/usr/bin/env python
"""Parses the output of objdump -t -C (i.e. dump of symbols with demangled C++ names. Invocation:
   parse_objdump.py -show-dups objdump-output binary
or
   parse_objdump.py -show-diffs objdump-output-one objdump-output-two

TODO: oops, turns out symbol addresses are not offsets into a file and I don't know
      how to map them. That kind of breaks my idea of calculating hashes of symbol data.
"""

import os, sys, re

def usage_and_exit():
  print "usage: parse_objdump.py -show-dups objdump-output binary"
  print "or:    parse_objdump.py -show-diffs objdump-output-one objdump-output-two"    
  sys.exit(1)

class Entry:
  def __init__(self, addr, len, name):
    self.addr = addr
    self.len = len
    self.name = name
    self.sha = None

def parse_objdump(file_name):
  results = []
  fo = open(file_name, "r")
  for line in fo:
    parts = line.split(None, 5)
    if len(parts) != 6: continue
    if ".ctors" == parts[2]: continue
    if "*UND*" == parts[2]: continue
    if "00000000" == parts[0]:
      #print "bad line: %s" % line.strip()
      continue
    addr = parts[0]
    try:
      func_len = int(parts[4], 16)
    except:
      print "bad line: %s" % line.strip()
      continue
    if 0 == func_len: continue
    func_name = parts[5].strip()
    results.append(Entry(addr, func_len, func_name))
    #print line.strip()
  fo.close()
  return results

def calc_sha_of_symbols(objdump_info, binary_file_name):
  objdump_info.sort(lambda x,y: cmp(x.addr, y.addr))
  for n in range(10):
    o = objdump_info[n]
    print "%s, %d, %s" % (o.addr, o.len, o.name)
  #fo = open(binary_file_name, "rb")
  #fo.close()

def dump_duplicates(objdump_info):
  objdump_info.sort(lambda x,y: cmp(x.sha, y.sha))

def show_dups(objdump_file_name, binary_file_name):
  objdump_info = parse_objdump(objdump_file_name)
  calc_sha_of_symbols(objdump_info, binary_file_name)

def show_diffs(objdump_file_name_1, objdump_file_name_2):
  objdump_info_1 = parse_objdump(objdump_file_name_1)
  objdump_info_2 = parse_objdump(objdump_file_name_2)

def main():
  if len(sys.argv) != 4: usage_and_exit()
  if sys.argv[1] in ["-show-dups", "--show-dups", "-showdups", "--showdups"]:
    show_dups(sys.argv[2], sys.argv[3])
  elif sys.argv[1] in ["-show-diffs", "--show-diffs", "-showdiffs", "--showdiffs"]:
    show_diffs(sys.argv[2], sys.argv[3])
  else: usage_and_exit()

if __name__=="__main__":
    main()



