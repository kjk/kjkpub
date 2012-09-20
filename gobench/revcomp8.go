/* The Computer Language Benchmarks Game
 * http://shootout.alioth.debian.org/
 *
 * contributed by Krzysztof Kowalczyk
 */

package main

import (
	"bytes"
	"fmt"
	"io"
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
	comptbl['\n'] = '\n'
}

const CHUNK_SIZE = 1024 * 128
const BUF_SIZE = 1024 * 1024 * 243
const (
	START   = 0
	IN_HDR  = 1
	IN_DATA = 2
)

var buf []byte
var start_fasta_hdr int
var start_fasta_data int

func reverse(strand []byte) {
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
}

func print_fasta(start_hdr, start_data, end_data int) {
	reverse(buf[start_data:end_data])
	os.Stdout.Write(buf[start_hdr:end_data])
}

func process_chunk_data(i, end int) int {
	pos := bytes.IndexByte(buf[i:end], '>')
	if -1 == pos {
		i = end
		return IN_DATA
	}
	i = i + pos
	print_fasta(start_fasta_hdr, start_fasta_data, i)
	start_fasta_hdr = i
	return process_chunk_start(i, end)
}

func process_chunk_hdr(i, end int) int {
	for ; i < end; i++ {
		if buf[i] == '\n' {
			start_fasta_data = i + 1
			return process_chunk_data(i+1, end)
		}
	}
	return IN_HDR
}

func process_chunk_start(i, end int) int {
	if buf[i] != '>' {
		panic("Unexpected")
	}
	start_fasta_hdr = i
	return process_chunk_hdr(i+1, end)
}

func main() {
	st := time.Now()
	build_comptbl()
	buf = make([]byte, BUF_SIZE, BUF_SIZE)
	i := 0
	state := START
	for {
		n, err := os.Stdin.Read(buf[i : i+CHUNK_SIZE])
		if n > 0 {
			chunk_start := i
			i += n
			if state == START {
				state = process_chunk_start(chunk_start, i)
			} else if state == IN_HDR {
				state = process_chunk_hdr(chunk_start, i)
			} else {
				state = process_chunk_data(chunk_start, i)
			}
		} else {
			if err == io.EOF {
				if state != IN_DATA {
					panic("unexpected state")
				}
				print_fasta(start_fasta_hdr, start_fasta_data, i)
				break
			}
			log.Fatalf(fmt.Sprintf("Unexpected error: %s\n"), err.Error())
		}
	}

	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
