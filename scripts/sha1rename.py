#!/usr/bin/python
import sys, sha, os, os.path, shutil

def sha1sum(filename):
    print "Calculating sha1 for '%s'" % filename
    sha1sum = sha.new()
    fo = open(filename, "rb")
    chunksize = 16*1024
    while True:
        data = fo.read(chunksize)
        if 0 == len(data):
            break
        sha1sum.update(data)
    fo.close()
    return sha1sum.hexdigest()

def usage():
    print "Usage: %s $src-dir $dest-dir" % sys.argv[0]
    sys.exit(1)

def ensure_dir_exists(dirname):
    if not os.path.isdir(dirname):
        print "'%s' directory doesn't exist" % dirname
        usage()

def sha1file(srcpath, destdir):
    sha1 = sha1sum(srcpath)
    (root, ext) = os.path.splitext(srcpath)
    destpath = os.path.join(destdir, sha1 + ext)
    print "Copyting %s to %s" % (srcpath, destpath)
    shutil.copy(srcpath, destpath)

def main():
    if len(sys.argv) != 3:
        usage()
    srcdir = os.path.normpath(sys.argv[1])
    ensure_dir_exists(srcdir)
    destdir = os.path.normpath(sys.argv[2])
    ensure_dir_exists(destdir)
    if srcdir == destdir:
        print "Source and destination directory are the same!"
        usage()
    for f in os.listdir(srcdir):
        srcpath = os.path.join(srcdir, f)
        sha1file(srcpath, destdir)

if __name__ == "__main__": main()