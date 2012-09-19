/* The Computer Language Benchmarks Game
 * http://shootout.alioth.debian.org/
 *
 * contributed by Krzysztof Kowalczyk
 */

package main

import (
	"bytes"
	_ "fmt"
	"io/ioutil"
	"log"
	"os"
	"time"
)

var bigbuf []byte

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
	comptbl['\n'] = '\n'
}

func print_reverse2(strand []byte) {
	buf := make([]byte, len(strand), len(strand))
	i := 0
	chars_per_line_left := 60
	for n := len(strand) - 1; n >= 0; n-- {
		c := strand[n]
		if c != '\n' {
			buf[i] = c
			i += 1
			chars_per_line_left -= 1
			if 0 == chars_per_line_left {
				buf[i] = '\n'
				i += 1
				chars_per_line_left = 60
			}
		}
	}
	if i == len(buf)-1 {
		buf[i] = '\n'
	} else if i != len(buf) {
		panic("unexpected i")
	}
	os.Stdout.Write(buf)
}

// a version where we don't compact '\n'
func next_fasta_strand2(buf []byte) []byte {
	j := 0
	for i, c := range buf {
		if c == '>' {
			print_reverse2(buf[:j])
			return buf[i:]
		}
		buf[j] = comptbl[c]
		j += 1
	}
	print_reverse2(buf[:j])
	return nil
}

// a version where we don't compact '\n'
func next_fasta_strand3(buf []byte) []byte {
	j := 0
	buflen := len(buf)
	for i := 0; i < buflen; i++ {
		c := buf[i]
		if c == '>' {
			print_reverse2(buf[:j])
			return buf[i:]
		}
		buf[j] = comptbl[c]
		j += 1
	}
	print_reverse2(buf[:j])
	return nil
}

func print_reverse(strand []byte) {
	bufsize := len(strand) + (len(strand) / 60) + 10
	buf := bigbuf[:bufsize]
	i := 0
	chars_per_line_left := 60
	for n := len(strand) - 1; n >= 0; n-- {
		buf[i] = strand[n]
		i += 1
		chars_per_line_left -= 1
		if 0 == chars_per_line_left {
			buf[i] = '\n'
			i += 1
			chars_per_line_left = 60
		}
	}
	if chars_per_line_left != 60 {
		buf[i] = '\n'
		i += 1
	}
	os.Stdout.Write(buf[:i])
}

// a version where we compact '\n' while we go
func next_fasta_strand(buf []byte) []byte {
	j := 0
	for i, c := range buf {
		if c == '>' {
			print_reverse(buf[:j])
			return buf[i:]
		}
		if c != '\n' {
			buf[j] = comptbl[c]
			j += 1
		}
	}
	print_reverse(buf[:j])
	return nil
}

func next_fasta_part(buf []byte) []byte {
	if buf[0] != '>' {
		panic("unexpected buf[0]")
	}
	for i, c := range buf {
		if c == '\n' {
			os.Stdout.Write(buf[:i+1])
			return next_fasta_strand3(buf[i+1:])
		}
	}
	panic("unexpected to be here")

}

func main() {
	st := time.Now()
	build_comptbl()
	data, err := ioutil.ReadAll(os.Stdin)
	bigbuf = make([]byte, len(data), len(data))
	if err != nil {
		log.Fatalf("Failed to read os.Stdin")
	}
	for data != nil {
		data = next_fasta_part(data)
	}
	//os.Stdout.WriteString("\n")
	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
