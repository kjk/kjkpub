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
	comptbl['\n'] = '\n'
}

const CHUNK_SIZE = 1024 * 128
const BUF_SIZE = 1024 * 1024 * 243
const (
	START   = 0
	IN_HDR  = 1
	IN_DATA = 2
)

type ChunkInfo struct {
	i, end int
	last   bool
}

var buf []byte
var start_fasta_hdr int
var start_fasta_data int
var state int = START
var chunker_chan chan ChunkInfo
var done_chan chan bool

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

func process_chunk(i, end int) {
	if state == START {
		state = process_chunk_start(i, end)
	} else if state == IN_HDR {
		state = process_chunk_hdr(i, end)
	} else {
		state = process_chunk_data(i, end)
	}
}

func chunker(chunks chan ChunkInfo, done chan bool) {
	for {
		ci := <-chunks
		if ci.last {
			if state != IN_DATA {
				panic("unexpected state")
			}
			print_fasta(start_fasta_hdr, start_fasta_data, ci.end)
			done <- true
			return
		} else {
			process_chunk(ci.i, ci.end)
		}
	}
}

func main() {
	st := time.Now()
	runtime.GOMAXPROCS(4)
	build_comptbl()
	buf = make([]byte, BUF_SIZE, BUF_SIZE)
	chunker_chan = make(chan ChunkInfo, 128)
	done_chan = make(chan bool)
	go chunker(chunker_chan, done_chan)
	i := 0
	for {
		n, err := os.Stdin.Read(buf[i : i+CHUNK_SIZE])
		if n > 0 {
			var ci ChunkInfo
			ci.i = i
			ci.end = i + n
			ci.last = false
			chunker_chan <- ci
			i += n
		} else {
			if err == io.EOF {
				var ci ChunkInfo
				ci.end = i
				ci.last = true
				chunker_chan <- ci
				break
			}
			log.Fatalf(fmt.Sprintf("Unexpected error: %s\n"), err.Error())
		}
	}
	<-done_chan
	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
