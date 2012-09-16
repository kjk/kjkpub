#!/bin/sh
echo "Original version:"
time go run bintree.go 20

echo "My version 3:"
time go run bintree3.go 20

echo "My version 4:"
time go run bintree4.go 20
