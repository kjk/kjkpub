/* The Computer Language Benchmarks Game
 * http://shootout.alioth.debian.org/
 *
 * contributed by Krzysztof Kowalczyk
 */

package main

import (
	"bufio"
	"bytes"
	"os"
	"runtime"
	"time"
	"fmt"
)

var comptbl = [256]uint8{}

func build_comptbl() {
	l1 := []byte("UACBDKRWSN")
	l2 := []byte("ATGVHMYWSN")
	l1_lower := bytes.ToLower(l1)
	l2_lower := bytes.ToLower(l2)
	for i, c1 := range l1 {
		c2 := l2[i]
		comptbl[c1] = c2
		comptbl[c2] = c1
		comptbl[l1_lower[i]] = c2
		comptbl[l2_lower[i]] = c1
	}
}

type struct 
func read_next_block(io.Reader) bool {

}

func main() {
	st := time.Now()
	runtime.GOMAXPROCS(4)
	build_comptbl()

	buf = make([]byte, 1<<20)

	in := bufio.NewReaderSize(os.Stdin, 1<<18)

	s, err := in.ReadSlice('\n')

	for err == nil {
		print(&PrintJob{Kind: PRINT, Data: s})
		os.Stderr.Write(s)
		for {
			s, err := in.ReadSlice('\n')
			if err != nil || len(s) == 0 || s[0] == '>' {
				fasta_acc_end()
				break
			}
			os.Stderr.Write(s)
			fasta_acc(s[:len(s)-1])
		}
	}

	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
