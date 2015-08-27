#!/bin/bash

set -o nounset
set -o errexit
set -o pipefail

mkdir -p bin
cc CHMLib/src/test_chmLib.c CHMLib/src/lzx.c CHMLib/src/chm_lib.c -ICHMLib/src -o bin/test_chm
cc CHMLib/src/enum_chmLib.c CHMLib/src/lzx.c CHMLib/src/chm_lib.c -ICHMLib/src -o bin/enum
cc CHMLib/src/enumdir_chmLib.c CHMLib/src/lzx.c CHMLib/src/chm_lib.c -ICHMLib/src -o bin/enumdir
