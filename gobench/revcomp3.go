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
	_ "fmt"
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
		c1_lower := l1_lower[i]
		c2_lower := l2_lower[i]
		comptbl[c1_lower] = c2
		comptbl[c2_lower] = c1
	}
}

// in-place fasta-reverse that skips '\n' (to accomodate the file
// format we're given)
func fasta_reverse(strand []byte) []byte {
	i := 0
	end := len(strand) - 1
	for i < end {
		c := strand[i]
		cend := strand[end]
		strand[i] = comptbl[cend]
		strand[end] = comptbl[c]
		i += 1
		end -= 1
	}
	return strand
}

func pretty_print_buf(buf []byte) {
	for len(buf) > 60 {
		os.Stdout.Write(buf[:60])
		os.Stdout.WriteString("\n")
		buf = buf[60:]
	}
	os.Stdout.Write(buf)
	os.Stdout.WriteString("\n")
}

// returns either a line starting with '>' and ending
// with '\n' or the whole multi-line DNA strand part that follows
// '>' line (i.e. everything next '>')
func next_fasta_part(buf []byte) []byte {
	if buf[0] != '>' {
		panic("expected '>' here!")
	}

	for i, c := range buf {
		if c == '\n' {
			os.Stdout.Write(buf[:i+1])
			buf = buf[i:]
			break
		}			
	}

	var line []byte
	w := 0
	for i, c := range buf  {
		if c == '>' {
			line = buf[:w]
			buf = buf[i:]
			break
		}
		if c != '\n' {
			buf[w] = c
			w += 1
		}
	}
	if line == nil {
		fasta_reverse(buf[:w])
		pretty_print_buf(buf[:w])
		return nil
	}
	fasta_reverse(line)
	pretty_print_buf(line)
	return buf
}

func main() {
	st := time.Now()
	build_comptbl()
	next, err := ioutil.ReadAll(os.Stdin)
	if err != nil {
		log.Fatalf("Failed to read os.Stdin")
	}

	for next != nil {
		next = next_fasta_part(next)
	}
	//os.Stdout.WriteString("\n")
	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
