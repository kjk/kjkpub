#!/bin/bash

set -o nounset
set -o errexit
set -o pipefail

for file in ~/Downloads/_chm_fuzz/*
#for file in ~/Downloads/_chm/*
do
  echo "$file"
  ./bin/test_chm "$file"
done
