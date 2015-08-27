#!/bin/bash

set -o nounset
set -o errexit
set -o pipefail

mkdir -p bin
#CC=clang
CC=/Applications/Xcode-beta.app//Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang
#OPTS="-g -O2 -fsanitize=address,integer,undefined,dataflow"
OPTS="-g -O -Wall -fsanitize=address"
#OPTS="-g -O2 -fsanitize=undefined"
#OPTS="-g -O2 -faddress-sanitizer"
#$CC $OPTS CHMLib/src/test_chmLib.c CHMLib/src/lzx.c CHMLib/src/chm_lib.c -ICHMLib/src -o bin/test_chm
#$CC $OPTS CHMLib/src/enum_chmLib.c CHMLib/src/lzx.c CHMLib/src/chm_lib.c -ICHMLib/src -o bin/enum
#$CC $OPTS CHMLib/src/enumdir_chmLib.c CHMLib/src/lzx.c CHMLib/src/chm_lib.c -ICHMLib/src -o bin/enumdir

$CC $OPTS test_chm.c CHMLib/src/lzx.c CHMLib/src/chm_lib.c -ICHMLib/src -o bin/test_chm
