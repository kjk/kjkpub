import os, os.path, sys

def usage_and_exit():
    print "usage: listfiles.py dir"
    sys.exit(1)

def is_cpp_file(file):
    for suffix in (".cpp", ".c", ".cc"):
        if file.endswith(suffix): return True
    return False

def format_files_info(dir,files):
    path = os.path.abspath(dir)
    print '  (r"%s",' % path
    for file in files:
        print '  "%s",' % file
    print '  ),'

def main():
    if len(sys.argv) != 2:
        usage_and_exit()
    dir = sys.argv[1]
    if not os.path.exists(dir):
        print "directory '%s' doesn't exist" % dir
        sys.exit(1)
    files = os.listdir(dir)
    cpp_files = [file for file in files if is_cpp_file(file)]

    format_files_info(dir, cpp_files)

if __name__ == "__main__":
    main()
