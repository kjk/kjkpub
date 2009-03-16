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

def uncompressed_logs_dir():
    return os.path.join(BASE_DIR, "uncompressed")

def compressed_logs_dir():
    return os.path.join(BASE_DIR, "compressed")

def compressed_file_name_local(day):
    return os.path.join(compressed_logs_dir(), day + ".bz2")

def compressed_file_name_s3(day):
    return logsDir + "compressed-access-logs-" + day + ".bz2"

def ensure_dir_exists(path):
    if os.path.exists(path):
        if os.path.isdir(path): return
        raise Exception, "Path %s already exists but is not a directory"
    os.makedirs(path)

def get_file_size(filename):
    st = os.stat(filename)
    return st[stat.ST_SIZE]

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

def day_from_name(name):
    tmp = "access_log-"
    s = name.find(tmp)
    day_name_start = s+len(tmp)
    day_name_end = day_name_start + len("2009-00-00")
    day = name[day_name_start:day_name_end]
    return day

def gen_files_for_day(keys):
    curr_day = None
    curr = []
    for key in keys:
        day = day_from_name(key.name)
        if day == curr_day:
            curr.append(key)
        else:
            if len(curr) > 0:
                yield curr
            curr = [key]
            curr_day = day
    if len(curr) > 0:
        yield curr

def file_name_from_s3_name(s3name):
    # skip kjkpub/ at the beginning
    name = s3name[len(logsDir):]
    return os.path.join(uncompressed_logs_dir(), name)

def new_compressed(file_name, files):
    global g_uncompressed_size, g_compressed_size
    print("Saving to new compressed file %s" % file_name)
    fo_out = bz2.BZ2File(file_name, "wb")
    for f in files:
        g_uncompressed_size += get_file_size(f)
        data = file(f, "rb").read()
        fo_out.write(data)
    fo_out.close()
    g_compressed_size += get_file_size(file_name)

def read_bzip2(file_name):
    fo = bz2.BZ2File(file_name, "rb")
    data = fo.read()
    fo.close()
    return data

def append_to_compressed(file_name, files):
    global g_uncompressed_size, g_compressed_size
    print("Appending to compressed file %s" % file_name)
    assert os.path.exists(file_name)
    g_compressed_size -= get_file_size(file_name)
    #shutil.copy(file_name, file_name + ".orig.bz2")
    file_name_tmp = file_name + ".tmp"
    fo_out = bz2.BZ2File(file_name_tmp, "wb")
    data = read_bzip2(file_name)
    fo_out.write(data)
    for f in files:
        g_uncompressed_size += get_file_size(f)
        data = file(f, "rb").read()
        fo_out.write(data)
    fo_out.close()
    shutil.move(file_name_tmp, file_name)
    g_compressed_size += get_file_size(file_name)

def concat_and_compress_files(day, files):
    file_name = compressed_file_name_local(day)
    if os.path.exists(file_name):
        append_to_compressed(file_name, files)
    else:
        new_compressed(file_name, files)

def delete_keys_from_s3(keys):
    for key in keys:
        print("Deleting %s" % key.name)
        key.delete()

def file_downloaded(file_name, size):
    if not os.path.exists(file_name): return False
    file_size = get_file_size(file_name)
    return size == file_size

g_total_deleted = 0
g_total_deleted_size = 0
g_uncompressed_size = 0
g_compressed_size = 0

# some files have screwy permissions and the only allowed operation on them
# is deletion so this function tries to download a file and if it can't due
# to screwy permissions, it deletes it. Sometimes s3 listing claims a file exists
# if it doesn't, in which case we can fix it by deleting it (no, really)
# Returns True if had to delete a file
def dl_or_delete_forbidden(key, file_name):
    global g_total_deleted, g_total_deleted_size
    try:
        key.get_contents_to_filename(file_name)
    except S3ResponseError, err:
        if err.status in [403, 404]:
            print("*** Deleting %s of size %d" % (key.name, key.size))
            key.delete()
            g_total_deleted += 1
            g_total_deleted_size += key.size
            return True
        raise
    return False

def process_day(day_keys):
    day = day_from_name(day_keys[0].name)
    print("Processing %s, files: %d" % (day, len(day_keys)))
    files = []
    for key in day_keys:
        file_name = file_name_from_s3_name(key.name)
        if file_downloaded(file_name, key.size):
            print("'%s' already downloaded as '%s'" % (key.name, file_name))            
        else:
            print("downloading '%s' to '%s'" % (key.name, file_name))
        deleted = dl_or_delete_forbidden(key, file_name)
        if not deleted:
            files.append(file_name)
    concat_and_compress_files(day, files)
    file_name = compressed_file_name_local(day)
    s3name = compressed_file_name_s3(day)
    s3UploadPrivate(file_name, s3name)
    delete_keys_from_s3(day_keys)

def tests():
    s3name = logsDir + "access_log-2008-09-21-23-45-40-B7CE947BBC3F87B2"
    assert day_from_name(s3name) == "2008-09-21"
    expected_dir = os.path.join(uncompressed_logs_dir(), "access_log-2008-09-21-23-45-40-B7CE947BBC3F87B2")
    assert file_name_from_s3_name(s3name) == expected_dir

def compress_s3_logs():
    ensure_dir_exists(compressed_logs_dir())
    ensure_dir_exists(uncompressed_logs_dir())

    b = s3Bucket()
    limit = 999
    all_keys = b.list(logsDir + "access_log-")
    for day_keys in gen_files_for_day(all_keys):
        process_day(day_keys)
        limit -= 1
        if limit <= 0:
            break
    print("Had to delete %d files of total size %d bytes" % (g_total_deleted, g_total_deleted_size))
    saved = g_uncompressed_size - g_compressed_size
    saved_percent = float(100) * float(saved) / float(g_uncompressed_size)
    print("Compressed size: %d, uncompressed size: %d, saving: %d which is %.2f %%" % (g_compressed_size, g_uncompressed_size, saved, saved_percent))

def main():
    tests()
    compress_s3_logs()

if __name__ == "__main__":
    main()
