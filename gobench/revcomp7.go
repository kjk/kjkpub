/* The Computer Language Benchmarks Game
* http://shootout.alioth.debian.org/
*
* contributed by Krzysztof Kowalczyk
* from: http://play.golang.org/p/-qpn-yvVOR (by rgbfirefox)
 */

package main

import (
	"bytes"
	"io/ioutil"
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

// in-place fasta-reverse that skips '\n' (to accomodate the file
// format we're given)
func fasta_reverse(strand []byte) {
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

func main() {
	st := time.Now()
	NCPU := runtime.NumCPU()
	runtime.GOMAXPROCS(NCPU)
	tasks := make(chan int, NCPU)
	for i := 0; i < NCPU; i++ {
		tasks <- 1 // fill pipe of 1, one for each CPU
	}
	build_comptbl()
	data, err := ioutil.ReadAll(os.Stdin)
	if err != nil {
		log.Fatalf("Failed to read os.Stdin")
	}
	pos := 0
	for {
		<-tasks
		line := next_fasta_part(data, &pos)
		if line == nil {
			tasks <- 1
			break
		}
		if line[0] != '>' {
			go func() {
				fasta_reverse(line)
				tasks <- 1
			}()
		} else {
			tasks <- 1
		}
	}
	// Drain the channel.
	for i := 0; i < NCPU; i++ {
		<-tasks // wait for tasks to complete
	}
	os.Stdout.Write(data)
	//os.Stdout.WriteString("\n")
	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String() + "\n")
}
