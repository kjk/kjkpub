#!/bin/bash

set -o nounset
set -o errexit
set -o pipefail

clang-format -style="{BasedOnStyle: Mozilla, IndentWidth: 4, ColumnLimit: 100}" -i *.c
