import time
from subprocess import *

# Timing script
# Note: this is not the best way to measure time since it also
# measures a startup time of the interpreter. I try to compensate
# by making the runs long enough to make it not matter

TIS_EXE = r"c:\kjk\src\tiscript\obj-rel-win\tiscript.exe"
LUA_EXE = "lua"

def time_lua():
  print "lua: "
  start = time.clock()
  output = Popen([LUA_EXE, "nbody.lua"], stdout=PIPE).communicate()[0]
  end = time.clock()
  print output
  return end - start

def time_tiscript2():
  print "tiscript2: "
  start = time.clock()
  output = Popen([TIS_EXE, "nbody2.tis"], stdout=PIPE).communicate()[0]
  end = time.clock()
  print output
  return end - start

def time_tiscript():
  print "tiscript: "
  start = time.clock()
  output = Popen([TIS_EXE, "nbody.tis"], stdout=PIPE).communicate()[0]
  end = time.clock()
  print output
  return end - start

do_tis2 = False

if do_tis2:
  tis_time2 = time_tiscript2()
lua_time = time_lua()
tis_time = time_tiscript()

print "lua time : " + str(lua_time)
print "tis time : " + str(tis_time) + "  ration: " + str(tis_time / lua_time)
if do_tis2:
  print "tis time2: " + str(tis_time2) + "  ration: " + str(tis_time2 / lua_time)
