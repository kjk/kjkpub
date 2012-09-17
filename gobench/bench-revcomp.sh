#!/bin/sh
echo "Original version:"
time go run revcomp.go <revcomp-input.txt >/dev/null

echo "\n\nMy version"
time go run revcomp2.go <revcomp-input.txt >/dev/null

