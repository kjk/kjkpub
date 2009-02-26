import os, os.path, fnmatch, gzip, bz2, re, datetime, sys

try:
    import boto.s3
    from boto.s3.key import Key
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

s3Bucket = "kjklogs"
logsDir = "kjkpub"

g_s3conn = None

def s3connection():
    global g_s3conn
    if g_s3conn is None:
        g_s3conn = boto.s3.connection.S3Connection(awscreds.access, awscreds.secret, True)
    return g_s3conn

def s3PubBucket(): return s3connection().create_bucket(s3Bucket)

def s3UploadPrivate(local_file_name, remote_file_name):
    bucket = s3PubBucket()
    k = Key(bucket)
    k.key = remote_file_name
    k.set_contents_from_filename(local_file_name)

def s3UploadDataPrivate(data, remote_file_name):
    bucket = s3PubBucket()
    k = Key(bucket)
    k.key = remote_file_name
    k.set_contents_from_string(data)

def main():
    print("Nothing to see here just yet")

if __name__ == "__main__":
    main()

