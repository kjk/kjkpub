#!/bin/bash

set -o nounset
set -o errexit
set -o pipefail

mkdir -p out
c++ -std=c++11 -stdlib=libc++ -Weverything -Wno-unneeded-internal-declaration test00.cc -o out/t
./out/t
