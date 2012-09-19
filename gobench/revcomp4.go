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

// returns either a line starting with '>' and ending
// with '\n' or the whole multi-line DNA strand part that follows
// '>' line (i.e. everything next '>')
func next_fasta_part(buf []byte, pos *int) []byte {
	p := *pos
	start := p
	end := len(buf) - 1
	if p == end {
		return nil
	}
	var until byte
	is_line := buf[p] == '>'
	if is_line {
		until = '\n'
	} else {
		until = '>'
	}
	for p != end && buf[p] != until {
		p += 1
	}
	if until == '>' {
		p -= 1
	}
	*pos = p + 1
	return buf[start:*pos]
}

func fasta_reverse(strand []byte) {
	buf := make([]byte, len(strand), len(strand))
	i := 0
	chars_per_line_left := 60
	for n := len(strand) - 1; n >= 0; n-- {
		c := strand[n]
		if c != '\n' {
			buf[i] = comptbl[c]
			i += 1
			chars_per_line_left -= 1
			if 0 == chars_per_line_left {
				buf[i] = '\n'
				i += 1
				chars_per_line_left = 60
			}
		}
	}
	if i == len(buf) - 1 {
		buf[i] = '\n'
	} else if i != len(buf) {
		panic("unexpected i")
	}
	os.Stdout.Write(buf)
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
		if line[0] == '>' {
			os.Stdout.Write(line)
		} else {
			fasta_reverse(line)
		}
	}
	os.Stdout.WriteString("\n")
	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
