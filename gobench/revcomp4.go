/* The Computer Language Benchmarks Game
 * http://shootout.alioth.debian.org/
 *
 * contributed by Krzysztof Kowalczyk
 */

package main

import (
	"bytes"
	"io/ioutil"
	"log"
	"os"
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
	build_comptbl()
	buf, err := ioutil.ReadAll(os.Stdin)
	if err != nil {
		log.Fatalf("Failed to read os.Stdin")
	}

	for len(buf) != 0 {
		end := bytes.IndexByte(buf, '\n')
		os.Stdout.Write(buf[:end])
		buf = buf[end:]
		end = bytes.IndexByte(buf, '>')
		if end == -1 {
			end = len(buf)
		}
		fasta_reverse_and_print(buf[:end])
		buf = buf[end:]
	}
	//os.Stdout.WriteString("\n")
	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
