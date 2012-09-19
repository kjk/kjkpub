#!/bin/bash

mkdir -p bin
if [ ! -f bin/revcomp-big-input.txt ]
then
	go run fasta.go 25000000 >bin/revcomp-big-input.txt
fi

if [ ! -f bin/revcomp-out-good.txt ]
then
	mcs -out:bin/revcomp.exe revcomp.cs
	mono bin/revcomp.exe <revcomp-input.txt >bin/revcomp-out-good.txt
fi

if [ ! -f bin/revcomp-big-out-good.txt ]
then
	mcs -out:bin/revcomp.exe revcomp.cs
	mono bin/revcomp.exe <bin/revcomp-big-input.txt >bin/revcomp-big-out-good.txt
fi

go build -o bin/revcomp revcomp.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp2 revcomp2.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp3 revcomp3.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp4 revcomp4.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 

echo "Small file"

echo -n "orig: "
./bin/revcomp <revcomp-input.txt >bin/revcomp-out-1.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff bin/revcomp-out-good.txt bin/revcomp-out-1.txt

echo -n "my 1: "
./bin/revcomp2 <revcomp-input.txt >bin/revcomp-out-2.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff bin/revcomp-out-good.txt bin/revcomp-out-2.txt

echo -n "my 2: "
./bin/revcomp3 <revcomp-input.txt >bin/revcomp-out-3.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff -q bin/revcomp-out-good.txt bin/revcomp-out-3.txt

echo -n "my 3: "
./bin/revcomp4 <revcomp-input.txt >bin/revcomp-out-4.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff -q bin/revcomp-out-good.txt bin/revcomp-out-4.txt

# exit

echo "Big file"

echo -n "orig: "
./bin/revcomp <bin/revcomp-big-input.txt >bin/revcomp-big-out-1.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-1.txt

echo -n "my 1: "
./bin/revcomp2 <bin/revcomp-big-input.txt >bin/revcomp-big-out-2.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-2.txt

#echo -n "my 2: "
#./bin/revcomp3 <bin/revcomp-big-input.txt >bin/revcomp-big-out-3.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -w -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-3.txt

echo -n "my 3: "
./bin/revcomp4 <bin/revcomp-big-input.txt >bin/revcomp-big-out-4.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-4.txt
