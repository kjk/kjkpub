#!/bin/sh

if [ ! -f bin/revcomp-input-big.txt ]
then
	go run fasta.go 25000000 >bin/revcomp-input-big.txt
fi

go build -o bin/revcomp revcomp.go
go build -o bin/revcomp2 revcomp2.go
go build -o bin/revcomp3 revcomp3.go

echo "Small file"

echo "Original version:"
./bin/revcomp <revcomp-input.txt >bin/revcomp-out-1.txt
diff revcomp-output.txt bin/revcomp-out-1.txt

echo "\n\nMy version"
./bin/revcomp2 <revcomp-input.txt >bin/revcomp-out-2.txt
diff revcomp-output.txt bin/revcomp-out-2.txt

echo "\n\nMy version 2"
./bin/revcomp3 <revcomp-input.txt >bin/revcomp-out-3.txt
echo "\n"
diff -q revcomp-output.txt bin/revcomp-out-3.txt
exit

echo "Big file"

echo "Original version:"
./bin/revcomp <bin/revcomp-input-big.txt >/dev/null

echo "\n\nMy version 1"
./bin/revcomp2 <bin/revcomp-input-big-2.txt >/dev/null
echo "\n"

echo "\n\nMy version 2"
./bin/revcomp2 <bin/revcomp-input-big-3.txt >/dev/null
echo "\n"
