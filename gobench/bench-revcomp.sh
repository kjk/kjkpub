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
go build -o bin/revcomp5 revcomp5.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp6 revcomp6.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp6c revcomp6c.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp7 revcomp7.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp8 revcomp8.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 

#echo "Small file"

#echo -n "orig: "
#./bin/revcomp <revcomp-input.txt >bin/revcomp-out-1.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff bin/revcomp-out-good.txt bin/revcomp-out-1.txt

#echo -n "rev2: "
#./bin/revcomp2 <revcomp-input.txt >bin/revcomp-out-2.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff bin/revcomp-out-good.txt bin/revcomp-out-2.txt

#echo -n "rev3: "
#./bin/revcomp3 <revcomp-input.txt >bin/revcomp-out-3.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-out-good.txt bin/revcomp-out-3.txt

#echo -n "rev4: "
#./bin/revcomp4 <revcomp-input.txt >bin/revcomp-out-4.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-out-good.txt bin/revcomp-out-4.txt

#echo -n "rev5: "
#./bin/revcomp5 <revcomp-input.txt >bin/revcomp-out-5.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-out-good.txt bin/revcomp-out-5.txt

#echo -n "r  6: "
#./bin/revcomp6 <revcomp-input.txt >bin/revcomp-out-6.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-out-good.txt bin/revcomp-out-6.txt

#echo -n "r 6c: "
#./bin/revcomp6c <revcomp-input.txt >bin/revcomp-out-6c.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-out-good.txt bin/revcomp-out-6c.txt

#echo -n "rev7: "
#./bin/revcomp7 <revcomp-input.txt >bin/revcomp-out-7.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff bin/revcomp-out-good.txt bin/revcomp-out-7.txt

echo -n "rev8: "
./bin/revcomp8 <revcomp-input.txt >bin/revcomp-out-8.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff bin/revcomp-out-good.txt bin/revcomp-out-8.txt
if [ "$?" -ne 0 ]; then echo "bad result"; exit 1; fi 

#exit

echo "Big file"

echo -n "orig: "
time ./bin/revcomp <bin/revcomp-big-input.txt >bin/revcomp-big-out-1.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-1.txt
if [ "$?" -ne 0 ]; then echo "diff failed"; exit 1; fi 

#echo; echo -n "rev2: "
#time ./bin/revcomp2 <bin/revcomp-big-input.txt >bin/revcomp-big-out-2.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-2.txt

#echo; echo -n "rev3: "
#time ./bin/revcomp3 <bin/revcomp-big-input.txt >bin/revcomp-big-out-3.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-3.txt

#echo; echo -n "rev4: "
#time ./bin/revcomp4 <bin/revcomp-big-input.txt >bin/revcomp-big-out-4.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-4.txt

#echo; echo -n "r  6: "
#time ./bin/revcomp6 <bin/revcomp-big-input.txt >bin/revcomp-big-out-6.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-6.txt

#echo; echo -n "r 6c: "
#time ./bin/revcomp6c <bin/revcomp-big-input.txt >bin/revcomp-big-out-6c.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-6c.txt

#echo; echo -n "rev7: "
#time ./bin/revcomp7 <bin/revcomp-big-input.txt >bin/revcomp-big-out-7.txt
#if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-7.txt

echo; echo -n "r  8: "
time ./bin/revcomp8 <bin/revcomp-big-input.txt >bin/revcomp-big-out-8.txt
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-8.txt
if [ "$?" -ne 0 ]; then echo "diff failed"; exit 1; fi 
