#!/bin/bash

set -o nounset
set -o errexit
set -o pipefail

mkdir -p out

# when using clang:
#CPPFLAGS="-std=c++17 -stdlib=libc++ -Weverything -Wno-unneeded-internal-declaration"

# when using gcc
CPPFLAGS="-std=c++17 -Wall"

# for both:
CPPFLAGS="${CPPFLAGS} "

t00() { c++ ${CPPFLAGS} test00.cc -o out/t00 && ./out/t00; }
t01() { c++ ${CPPFLAGS} test01.cc -o out/t01 && ./out/t01; }

t01
