#!/bin/bash

go build -o bin/revcomp revcomp.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp8 revcomp8.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
go build -o bin/revcomp8c revcomp8c.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 

echo -n "orig: "

for i in {1..10}
do
	time ./bin/revcomp <bin/revcomp-big-input.txt >bin/revcomp-big-out-1.txt
	if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
	diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-1.txt
	if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
done

#echo -n "r  8: "
#for i in {1..10}
#do
#	time ./bin/revcomp8 <bin/revcomp-big-input.txt >bin/revcomp-big-out-8.txt
#	if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
#	diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-8.txt
#	if [ "$?" -ne 0 ]; then echo "diff failed"; exit 1; fi 
#done

echo -n "r 8c: "

for i in {1..10}
do
	time ./bin/revcomp8c <bin/revcomp-big-input.txt >bin/revcomp-big-out-8c.txt
	if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 
	diff -q bin/revcomp-big-out-good.txt bin/revcomp-big-out-8c.txt
	if [ "$?" -ne 0 ]; then echo "diff failed"; exit 1; fi 
done
