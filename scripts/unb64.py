import base64, sys

def usage_and_exit():
  print "unb64.py filename";
  sys.exit(1)

def main():
  if len(sys.argv) != 2: usage_and_exit()
  filename = sys.argv[1]
  fo = open(filename, "rb")
  encoded = fo.read()
  fo.close()
  decoded = base64.b64decode(encoded)
  print decoded

if __name__ == "__main__":
  main()
