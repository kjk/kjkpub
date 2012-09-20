/* The Computer Language Benchmarks Game
 * http://shootout.alioth.debian.org/
 *
 * contributed by Krzysztof Kowalczyk
 */

package main

import (
	"bufio"
	"bytes"
	"fmt"
	"io"
	"log"
	"os"
	"runtime"
	"time"
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

// in-place fasta-reverse that skips '\n' (to accomodate the file
// format we're given)
func fasta_reverse_and_print(strand []byte) {
	i := 0
	end := len(strand) - 1
	for i < end {
		c := strand[i]
		if c == '\n' {
			i += 1
			c = strand[i]
		}
		cend := strand[end]
		if cend == '\n' {
			end -= 1
			cend = strand[end]
		}
		strand[i] = comptbl[cend]
		strand[end] = comptbl[c]
		i += 1
		end -= 1
	}
	os.Stdout.Write(strand)
}

func main() {
	st := time.Now()
	ncpu := 3
	runtime.GOMAXPROCS(ncpu)
	tasks := make(chan int, ncpu)
	for i := 0; i < ncpu; i++ {
		tasks <- 1 // fill pipe of 1, one for each CPU
	}
	build_comptbl()
	in := bufio.NewReaderSize(os.Stdin, 1024*1024*243)
	for {
		<-tasks
		line, err := in.ReadSlice('\n')
		if err != nil || line[0] != '>' {
			panic("unexpected 1")
		}
		os.Stdout.Write(line)
		line, err = in.ReadSlice('>')
		if err != nil {
			if err == io.EOF {
				go func() {
					fasta_reverse_and_print(line)
					tasks <- 1
				}()
				break
			}
			log.Fatalf(fmt.Sprintf("Err: %s\n"), err.Error())
		}
		fasta_reverse_and_print(line[:len(line)-1])
		in.UnreadByte()
		tasks <- 1
	}
	for i := 0; i < ncpu; i++ {
		<-tasks
	}

	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
