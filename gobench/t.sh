go build -o bin/revcomp8c revcomp8c.go
if [ "$?" -ne 0 ]; then echo "command failed"; exit 1; fi 

./bin/revcomp8c <revcomp-input.txt
