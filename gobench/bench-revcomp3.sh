#!/bin/sh

if [ ! -f revcomp-input-big.txt]
then
	go run fasta.go 25000000 >revcomp-input-big.txt
fi

go build -o revcomp revcomp.go
go build -o revcomp2 revcomp2.go
go build -o revcomp3 revcomp3.go

echo "Original version:"
./revcomp <revcomp-input.txt >revcomp-out.txt
diff revcomp-output.txt revcomp-out.txt

echo "\n\nMy version 1"
./revcomp2 <revcomp-input.txt >revcomp2-out.txt
echo "\n"
diff revcomp-output.txt revcomp2-out.txt

echo "\n\nMy version 2"
./revcomp3 <revcomp-input.txt >revcomp3-out.txt
echo "\n"
diff -q revcomp-output.txt revcomp3-out.txt
