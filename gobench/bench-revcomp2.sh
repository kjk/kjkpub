#!/bin/sh
if [ ! -f revcomp-input-big.txt]
then
	go run fasta.go 25000000 >revcomp-input-big.txt
fi

go build -o revcomp revcomp.go
go build -o revcomp2 revcomp2.go

echo "Original version:"
./revcomp <revcomp-input-big.txt >/dev/null

echo "\n\nMy version"
./revcomp2 <revcomp-input-big.txt >/dev/null
echo "\n"

#diff revcomp-out-big.txt revcomp2-out-big.txt
