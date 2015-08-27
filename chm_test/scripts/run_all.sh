#!/bin/bash

set -o nounset
set -o errexit
set -o pipefail

for file in ~/Downloads/_chm_fuzz/*
do
  ./bin/test_chm "$file"
done
