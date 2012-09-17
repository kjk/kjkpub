/* The Computer Language Benchmarks Game
 * http://shootout.alioth.debian.org/
 *
 * contributed by Krzysztof Kowalczyk
 */

package main

import (
	"io/ioutil"
	"log"
	"os"
	"time"
)

var comptbl = [256]uint8{
	'A': 'T', 'a': 'T',
	'C': 'G', 'c': 'G',
	'G': 'C', 'g': 'C',
	'T': 'A', 't': 'A',
	'U': 'A', 'u': 'A',
	'M': 'K', 'm': 'K',
	'R': 'Y', 'r': 'Y',
	'W': 'W', 'w': 'W',
	'S': 'S', 's': 'S',
	'Y': 'R', 'y': 'R',
	'K': 'M', 'k': 'M',
	'V': 'B', 'v': 'B',
	'H': 'D', 'h': 'D',
	'D': 'H', 'd': 'H',
	'B': 'V', 'b': 'V',
	'N': 'N', 'n': 'N',
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
		if line[0] != '>' {
			fasta_reverse(line)
		}
		os.Stdout.Write(line)
	}
	os.Stdout.WriteString("\n")
	dur := time.Now().Sub(st)
	os.Stderr.WriteString(dur.String())
}
