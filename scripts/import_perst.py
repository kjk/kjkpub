#!/usr/bin/env python

import zipfile
import os
import os.path
import shutil
import subprocess
import sys

def run_cmd(*args):
  cmd = " ".join(args)
  try:
    cmdproc = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    res = cmdproc.communicate()
    stdout = res[0]
    stderr = res[1]
  except:
    print "Failed to execute!"
    return (1, "Failed to execute '%s'" % cmd, "")
  errcode = cmdproc.returncode
  return (errcode, stdout, stderr)

def version_from_filename(f):
  p = f.split("-")
  if len(p) != 2: return None
  p = p[1].split(".")
  if len(p) != 2: return None
  return int(p[0])

def verify_dir_doesnt_exist(path):
  if os.path.exists(path):
    print("Dir %s already exists and shouldn't" % path)
    sys.exit(1)

def ensure_dir_for_file(f):
  dir = os.path.dirname(f)
  if 0 == len(dir): return
  if not os.path.exists(dir):
    os.makedirs(dir)

def zip_extract(zippath, dst_dir, prev, commit_msg):
  os.chdir(dst_dir)
  zf = zipfile.ZipFile(zippath)
  files = zf.namelist()
  changed = []
  added = []
  removed = {}
  for el in prev:
    removed[el] = True
  for f in files:
    name = f
    if f.endswith("/"): continue
    if f.endswith("~"): continue
    if f.startswith("perst/"):
      f = f[len("perst/"):]
      if f in ["lib/perst.jar"]: continue
      if f.startswith("doc/"): continue
      if f.startswith("doc15/"): continue
      if f.endswith(".tgs"): continue
      if f.endswith(".txvpck"): continue
      if f.endswith(".config"): continue
    elif f.startswith("Perst.NET/"):
      f = f[len("Perst.NET/"):]
      if f.startswith("doc/"): continue
      if "Copy of " in f: continue
      if "/obj" in f: continue
      if "/bin/" in f: continue
      if f.endswith("/.xml"): continue
      if f.endswith(".tgs"): continue
      if f.endswith(".user"): continue
      if f.endswith(".config"): continue
      if f.endswith(".csdproj"): continue
      if f.endswith(".txvpck"): continue
      if f.endswith("App.ico"): continue
      if f in ["Guess/Guess.sln", "Guess/Guess.xml", "Perst.NET.xml", "Guess/Guess.sln", "Perst.NET.xml", "test.xml", "testconcur.dbs"]: continue
    if f in prev:
      changed.append(f)
      del removed[f]
    else:
      added.append(f)

    print("Extracting %s as %s" % (name, f))
    data = zf.read(name)
    ensure_dir_for_file(f)
    fo = open(f, "wb")
    fo.write(data)
    fo.close()

  zf.close()
  if len(changed) > 0:
    print(" Changed: %d" % len(changed))
    #print("  " + ", ".join(changed))
  if len(removed) > 0:
    print(" Removed: %d" % len(removed))
    print("  " + ", ".join(removed.keys()))
    for f in removed:
      run_cmd("git", "rm", f)
  if len(added) > 0:
    print(" Added:   %d" % len(added))
    print("  " + ", ".join(added))
    for f in added:
      run_cmd("git", "add", f)
  run_cmd("git", "commit", "-am", commit_msg)
  return changed + added

def main():
  perst_dir = os.path.realpath(os.path.join("..", "..", "perst"))
  dst_dir = os.path.realpath(os.path.join("..", "..", "nachodb"))
  dst_java_dir = os.path.join(dst_dir, "java")
  dst_csharp_dir = os.path.join(dst_dir, "csharp")

  if os.path.exists(dst_dir):
    shutil.rmtree(dst_dir)
  os.makedirs(dst_dir)
  os.chdir(dst_dir)
  run_cmd("git", "init")
  os.makedirs(dst_java_dir)
  os.makedirs(dst_csharp_dir)
  files = os.listdir(perst_dir)
  versions = []
  for f in files:
    ver = version_from_filename(f)
    if None != ver and ver not in versions:
      versions.append(ver)
  versions.sort()
  java_prev = []
  net_prev = []
  for ver in versions:
    javapath = os.path.join(perst_dir, "perst-%d.zip" % ver)
    netpath = os.path.join(perst_dir, "perstnet-%d.zip" % ver)
    if os.path.exists(javapath):
      print("*** java ver %d ***:" % ver)
      java_prev = zip_extract(javapath, dst_java_dir, java_prev, "importing java ver %d" % ver)
    if os.path.exists(netpath):
      print("*** csharp ver %d ***:" % ver)
      net_prev = zip_extract(netpath, dst_csharp_dir, net_prev, "importing c# ver %d" % ver)

if __name__ == "__main__":
  main()
