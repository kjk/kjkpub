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

// returns either a line starting with '>' and ending
// with '\n' or the whole multi-line DNA strand part that follows
// '>' line (i.e. everything next '>')
func next_fasta_part(buf []byte, pos *int) []byte {
	p := *pos
	start := p
	end := len(buf) - 1
	if p >= end {
		return nil
	}

	if buf[p] == '>' {
		for p != end && buf[p] != '\n' {
			p += 1
		}
		*pos = p + 1
		return buf[start:*pos]
	}

	rest := buf[p:]
	i := 0
	for _, c := range rest  {
		if c == '>' {
			break
		}
		if c != '\n' {
			rest[i] = c
			i += 1
		}
	}
	*pos = p + i + 1
	return buf[start:*pos-1]
}

func main() {
	st := time.Now()
	build_comptbl()
	data, err := ioutil.ReadAll(os.Stdin)
	if err != nil {
		log.Fatalf("Failed to read os.Stdin")
	}
	pos := 0
	for {
		line := next_fasta_part(data, &pos)
		if nil == line {
			break
		}
		os.Stdout.Write(line)
	}
	os.Stdout.WriteString("\n")
	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String())
}
