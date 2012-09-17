#!/bin/sh

go build -o revcomp revcomp.go
go build -o revcomp2 revcomp2.go

echo "Original version:"
./revcomp <revcomp-input.txt >/dev/null

echo "\n\nMy version"
./revcomp2 <revcomp-input.txt >/dev/null

