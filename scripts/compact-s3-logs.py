#!/usr/bin/env python
# Code is Public Domain. Take all the code you want, we'll just write more.
import sys, os, os.path, bz2, stat, shutil

try:
    import boto.s3
    from boto.s3.key import Key
    from boto.exception import S3ResponseError
except:
    print "boto library (http://code.google.com/p/boto/) for aws needs to be installed"
    sys.exit(1)

try:
    import awscreds
except:
    print "awscreds.py file needed with access and secret globals for aws access"
    sys.exit(1)

"""
Script for compacting aws logs for s3 access.
s3 can be configured to store logs for s3 access. Logs are also stored in s3
as files. Unfortunately, the number of log files is huge: apparently amazon
generates 216 small log files per day.

This script combines all log files for one day into one file, compresses them
with bzip2, re-upload such combined log file back to s3 and deletes original
log files.

How it works, roughly:
* download all logs for one day from s3 locally
* combine them into one file & gzip
* upload back to s3
* delete the original files
* repeat until there are no more logs to process
"""

s3BucketName = "kjklogs"
logsDir = "kjkpub/"

BASE_DIR = os.path.expanduser("~/rsynced-data/s3logs")
UNCOMPRESSED_LOGS_DIR = os.path.join(BASE_DIR, "uncompressed")
COMPRESSED_LOGS_DIR = os.path.join(BASE_DIR, "compressed")

def compressed_file_path_local(day):
    return os.path.join(COMPRESSED_LOGS_DIR, day + ".bz2")

def compressed_file_name_s3(day):
    return logsDir + "compressed-access-logs-" + day + ".bz2"

def uncompressed_file_path(s3name):
    # skip kjkpub/ at the beginning
    name = s3name[len(logsDir):]
    dirname = year_month_from_name(name)
    return os.path.join(UNCOMPRESSED_LOGS_DIR, dirname, name)

def ensure_dir_exists(path):
    if os.path.exists(path):
        if os.path.isdir(path): return
        raise Exception, "Path %s already exists but is not a directory"
    os.makedirs(path)

def get_file_size(filename):
    st = os.stat(filename)
    return st[stat.ST_SIZE]

def read_file(file_path):
    fo = open(file_path, "rb")
    data = fo.read()
    fo.close()
    return data

g_s3conn = None
def s3connection():
    global g_s3conn
    if g_s3conn is None:
        g_s3conn = boto.s3.connection.S3Connection(awscreds.access, awscreds.secret, True)
    return g_s3conn

def s3Bucket(): return s3connection().get_bucket(s3BucketName)

def s3UploadPrivate(local_file_name, remote_file_name):
    print("Uploading %s as %s" % (local_file_name, remote_file_name))
    bucket = s3Bucket()
    k = Key(bucket)
    k.key = remote_file_name
    k.set_contents_from_filename(local_file_name)

# given a name in format "access_log-YYYY-MM-DD-$time-$hash" return YYYY-MM part
def year_month_from_name(name):
    parts = name.split("-")
    year = parts[1]
    assert 4 == len(year)
    month = parts[2]
    assert 2 == len(month)
    return year + "-" + month

# given a name in format "access_log-YYYY-MM-DD-$time-$hash" return DD part
def year_month_day_from_name(name):
    parts = name.split("-")
    ymd = "-".join(parts[1:4])
    assert 10 == len(ymd)
    return ymd

def gen_files_for_day(keys):
    curr_day = None
    curr = []
    for key in keys:
        day = year_month_day_from_name(key.name)
        if day == curr_day:
            curr.append(key)
        else:
            if len(curr) > 0:
                yield curr
            curr = [key]
            curr_day = day
    if len(curr) > 0:
        yield curr

def compress_files(compressed_file_path, file_paths):
    global g_uncompressed_size, g_compressed_size
    print("Creating new compressed file %s from %d files" % (compressed_file_path, len(file_paths)))
    if os.path.exists(compressed_file_path):
        os.remove(compressed_file_path)
    fo_out = bz2.BZ2File(compressed_file_path, "wb")
    for file_path in file_paths:
        g_uncompressed_size += get_file_size(file_path)
        data = read_file(file_path)
        fo_out.write(data)
    fo_out.close()
    g_compressed_size += get_file_size(compressed_file_path)

# given day in YYYY-MM-DD format, get all uncompressed file names for that day
def uncompressed_files_for_day(ymd):
    desired_file_prefix = "access_log-" + ymd
    parts = ymd.split("-")
    ym = parts[0] + "-" + parts[1]
    assert 7 == len(ym)
    d = os.path.join(UNCOMPRESSED_LOGS_DIR, ym)
    all_files = os.listdir(d)
    files_for_day = []
    for f in all_files:
        if f.startswith(desired_file_prefix):
            file_path = os.path.join(d, f)
            files_for_day.append(file_path)
    return files_for_day

def compress_files_for_one_day(ymd):
    compressed_file_path = compressed_file_path_local(ymd)
    file_paths = uncompressed_files_for_day(ymd)
    compress_files(compressed_file_path, file_paths)

def delete_keys_from_s3(keys):
    for key in keys:
        print("Deleting %s" % key.name)
        key.delete()

def file_downloaded(file_name, expected_size):
    if not os.path.exists(file_name): return False
    file_size = get_file_size(file_name)
    return expected_size == file_size

g_total_deleted = 0
g_total_deleted_size = 0
g_uncompressed_size = 0
g_compressed_size = 0

# some files have screwy permissions and the only allowed operation on them
# is deletion so this function tries to download a file and if it can't due
# to screwy permissions, it deletes it. Sometimes s3 listing claims a file exists
# if it doesn't, in which case we can fix it by deleting it (no, really)
# Returns True if had to delete a file
def dl_or_delete_forbidden(key, file_path):
    global g_total_deleted, g_total_deleted_size
    try:
        key.get_contents_to_filename(file_path)
    except S3ResponseError, err:
        if err.status in [403, 404]:
            print("*** Deleting s3 file %s of size %d" % (key.name, key.size))
            os.remove(file_path)
            key.delete()
            g_total_deleted += 1
            g_total_deleted_size += key.size
            return True
        raise
    return False

def process_day(day_keys):
    ymd = year_month_day_from_name(day_keys[0].name)
    print("Processing %s, files: %d" % (ymd, len(day_keys)))
    for key in day_keys:
        file_name = uncompressed_file_path(key.name)
        file_size = key.size
        if file_downloaded(file_name, file_size):
            print("'%s' already downloaded as '%s'" % (key.name, file_name))            
        else:
            print("downloading '%s' to '%s'" % (key.name, file_name))
        dl_or_delete_forbidden(key, file_name)
    compress_files_for_one_day(ymd)
    compressed_file_path = compressed_file_path_local(ymd)
    s3name = compressed_file_name_s3(ymd)
    print("Uploading '%s' as '%s'" % (compressed_file_path, s3name))
    s3UploadPrivate(compressed_file_path, s3name)
    delete_keys_from_s3(day_keys)

def tests():
    s3name = logsDir + "access_log-2008-09-21-23-45-40-B7CE947BBC3F87B2"
    assert year_month_day_from_name(s3name) == "2008-09-21"
    expected_path = os.path.join(UNCOMPRESSED_LOGS_DIR, "2008-09", "access_log-2008-09-21-23-45-40-B7CE947BBC3F87B2")
    real_path = uncompressed_file_path(s3name)
    assert real_path == expected_path

def compress_s3_logs():
    ensure_dir_exists(COMPRESSED_LOGS_DIR)
    ensure_dir_exists(UNCOMPRESSED_LOGS_DIR)

    b = s3Bucket()
    limit = 9999
    all_keys = b.list(logsDir + "access_log-")
    for day_keys in gen_files_for_day(all_keys):
        process_day(day_keys)
        limit -= 1
        if limit <= 0:
            break
    if g_total_deleted > 0:
        print("Had to delete %d files of total size %d bytes" % (g_total_deleted, g_total_deleted_size))

def t():
    ensure_dir_exists(COMPRESSED_LOGS_DIR)
    ensure_dir_exists(UNCOMPRESSED_LOGS_DIR)

    b = s3Bucket()
    limit = 1
    all_keys = b.list(logsDir + "access_log-")
    for k in all_keys:
        print("%6d, %s" % (k.size, k.name))

def compress_days():
    for d in range(30):
        ymd = "2009-06-%02d" % (d + 1)
        compress_files_for_one_day(ymd)
    for d in range(16):
        ymd = "2009-07-%02d" % (d + 1)
        compress_files_for_one_day(ymd)

def main():
    tests()
    compress_s3_logs()

if __name__ == "__main__":
    main()
