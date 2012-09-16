#!/bin/sh
#rm -f out.txt
go run revcomp2.go <revcomp-input.txt >out.txt
diff revcomp-output.txt out.txt
