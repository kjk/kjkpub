#!/bin/sh

go build -o revcomp revcomp.go
go build -o revcomp2 revcomp2.go

echo "Original version:"
./revcomp <revcomp-input.txt >revcomp-out.txt
diff revcomp-output.txt revcomp-out.txt

echo "\n\nMy version"
./revcomp2 <revcomp-input.txt >revcomp2-out.txt
echo "\n"
diff revcomp-output.txt revcomp-out.txt
