#!/usr/bin/env python
import sys
import os
import os.path
import stat
import shutil

BASE_DIR = os.path.expanduser("~/rsynced-data/s3logs")
UNCOMPRESSED_LOGS_DIR = os.path.join(BASE_DIR, "uncompressed")

def get_file_size(filename):
    st = os.stat(filename)
    return st[stat.ST_SIZE]

def ensure_dir_exists(path):
    if os.path.exists(path):
        if os.path.isdir(path): return
        raise Exception, "Path %s already exists but is not a directory"
    os.makedirs(path)

# given a name in format "access_log-YYYY-MM-DD-$time-$hash" return YYYY-MM part
def year_month_from_name(f):
    parts = f.split("-")
    year = parts[1]
    assert 4 == len(year)
    month = parts[2]
    assert 2 == len(month)
    return year + "-" + month

def main():
    files = os.listdir(UNCOMPRESSED_LOGS_DIR)
    for f in files:
        path = os.path.join(UNCOMPRESSED_LOGS_DIR, f)
        if not os.path.isfile(path): continue
        if 0 == get_file_size(path):
            print("Removing empty file '%s'" % path)
            os.remove(path)
        dirname = year_month_from_name(f)
        dirpath = os.path.join(UNCOMPRESSED_LOGS_DIR, dirname)
        ensure_dir_exists(dirpath)
        newpath = os.path.join(dirpath, f)
        print("Moving: \n %s to:\n %s" % (path, newpath))
        os.rename(path, newpath)

if __name__ == "__main__":
    main()
