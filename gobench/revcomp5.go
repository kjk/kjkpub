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
		c1_lower := l1_lower[i]
		c2_lower := l2_lower[i]
		comptbl[c1_lower] = c2
		comptbl[c2_lower] = c1
	}
}

const (
	PRINT = 0
	FASTA = 1
)

type PrintJob struct {
	Kind int
	Data []byte
}

var buf []byte
var buf_start int
var buf_curr int

const lineSize = 60

func pretty_print(buf []byte) {
	bufsize := len(buf)
	lines := bufsize / lineSize
	for i := 0; i < lines; i++ {
		os.Stdout.Write(buf[:lineSize])
		os.Stdout.WriteString("\n")
		buf = buf[lineSize:]
	}
	if len(buf) != bufsize%lineSize {
		panic("unexpected len(buf)")
	}
	if len(buf) > 0 {
		os.Stdout.Write(buf)
	}
	os.Stdout.WriteString("\n")
}

func reverse_buf(buf []byte) {
	i := 0
	end := len(buf) - 1
	for i < end {
		c := buf[i]
		cend := buf[end]
		buf[i] = cend
		buf[end] = c
		i += 1
		end -= 1
	}
}

func print(job *PrintJob) {
	if job.Kind == PRINT {
		os.Stdout.Write(job.Data)
	} else {
		reverse_buf(job.Data)
		pretty_print(job.Data)
	}
}

func fasta_acc(line []byte) {
	if len(line) > lineSize {
		os.Stderr.WriteString(fmt.Sprintf("len(line) = %d, \n'%s'", len(line), line))
		panic("unexpected len(line)")
	}
	for _, c := range line {
		buf[buf_curr] = comptbl[c]
		buf_curr += 1
	}
}

func fasta_acc_end() {
	print(&PrintJob{Kind: FASTA, Data: buf[buf_start:buf_curr]})
	buf_start = buf_curr
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
