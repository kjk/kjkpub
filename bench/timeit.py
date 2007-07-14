import time
from subprocess import *

# Timing script
# Note: this is not the best way to measure time since it also
# measures a startup time of the interpreter. I try to compensate
# by making the runs long enough to make it not matter

TIS_EXE = r"c:\kjk\src\tiscript\obj-rel-win\tiscript.exe"
LUA_EXE = "lua"

print "lua: "
start = time.clock()
output = Popen([LUA_EXE, "nbody.lua"], stdout=PIPE).communicate()[0]
end = time.clock()
lua_time = end - start
print output

print "tiscript: "
start = time.clock()
output = Popen([TIS_EXE, "nbody.tis"], stdout=PIPE).communicate()[0]
end = time.clock()
tis_time = end - start
print output

print "lua time: " + str(lua_time)
print "tis time: " + str(tis_time) + "  ration: " + str(tis_time / lua_time)
