#!/bin/sh

echo "sysctl -w fs.file-max=128000:"
sysctl -w fs.file-max=128000

echo "cat /proc/sys/fs/file-max:"
cat /proc/sys/fs/file-max
echo "ulimit -Hn:"
ulimit -Hn
echo "ulimit -Sn:"
ulimit -Sn

l=`cat /proc/sys/fs/file-max`
echo "ulimit -Hn ${l}"
ulimit -Hn ${l}
echo "ulimit -Sn ${l}"
ulimit -Sn ${l}

echo "ulimit -Hn:"
ulimit -Hn
echo "ulimit -Sn:"
ulimit -Sn

go run main.go
