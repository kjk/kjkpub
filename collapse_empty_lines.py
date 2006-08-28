# Written by Krzysztof Kowalczyk (http://blog.kowalczyk.info)
# This code is in public domain
#
# Purpose: for each file given as an argument, collapse multiple
#          empty lines into one and write back to the same file
#
# Notes: tried to do it with clever regexp but proved beyond
#        my abilities so I have a not-so-clever but working
#        state machine approach
#
import sys, string

def is_empty_line(txt):
  return 0 == len(txt.strip())

def usage_and_exit():
  print "Usage: collapse_empty_lines.py file1 file2 ..."
  sys.exit(1)

def write_to_file(file_path, data):
  fo = open(file_path, "wb")
  fo.write(data)
  fo.close()

def process_file(file_path):
  collapsed = []
  changed = False
  prev_line_was_empty = False
  fo = open(file_path, "rb")
  for l in fo:
    if is_empty_line(l):
      if prev_line_was_empty:
        changed = True
      else:
        prev_line_was_empty = True
        collapsed.append(l)
    else:
      prev_line_was_empty = False
      collapsed.append(l)
  fo.close()
  if changed:
    collapsed_txt = string.join(collapsed, "")
    #file_out_path = file_path + ".new"
    file_out_path = file_path
    print "writing to %s" % file_out_path
    write_to_file(file_out_path, collapsed_txt)

def main():
  if len(sys.argv) < 2:
    usage_and_exit()
  for file_path in sys.argv[1:]:
    process_file(file_path)

if __name__ == "__main__":
  main()
