#!/bin/sh

echo "ulimit -Hn:"
ulimit -Hn
echo "ulimit -Sn:"
ulimit -Sn

go run main.go
