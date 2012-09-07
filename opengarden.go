package main

// To run on mac, if you have brew installed:
// brew install go
// go run opengarden.go
//
// if you don't have brew, install Go manually: http://golang.org/doc/install
// and then run "go run opengarden.go"
//
// Notes: there are 2 solutions, both far from perfect
//
// Random solution looks for answer generating random answers and trying them
// (although, as implemented, it's only pseudo-random, as I don't initialize the
// random seed).
//
// Exhaustive search looks at all possibilities.
//
// I'm sure there's a more clever way that generates potential solutions 
// exhaustively and backtracts

import (
	"fmt"
	"math/rand"
)

var population = [...]int{18897109, 12828837, 9461105, 6371773, 5965343, 5946800,
	5582170, 5564635, 5268860, 4552402, 4335391, 4296250, 4224851, 4192887, 3439809,
	3279833, 3095313, 2812896, 2783243, 2710489, 2543482, 2356285, 2226009, 2149127,
	2142508, 2134411}

const desired = 100000000
const maxBits = len(population)

// a solution can be represented as an int where if n-th bit is set, the n-th
// value in population array belongs to the solution.
// So all solutions are in the range [0...maxSolution]
var maxSolution int = (1 << uint(maxBits)) - 1

func isBitSet(n int, bit int) bool {
	return n&(1<<uint(bit)) != 0
}

func printSolution(solution int) {
	fmt.Printf("Solution: 0x%xd\n", solution)
	total := 0
	for i := 0; i < maxBits; i++ {
		if isBitSet(solution, i) {
			n := population[i]
			total += n
			fmt.Printf("  pos: %02d, population: %d \n", i, n)
		}
	}
	fmt.Printf("\nTotal: %d\n", total)
}

func printIfValid(n int) bool {
	remaining := desired
	for bit := 0; bit < maxBits; bit++ {
		if isBitSet(n, bit) {
			remaining -= population[bit]
			if remaining < 0 {
				return false
			}
		}
	}
	if 0 == remaining {
		printSolution(n)
		return true
	}
	return false
}

func random() {
	for {
		solution := rand.Intn(maxSolution + 1)
		if valid := printIfValid(solution); valid {
			return
		}
	}
}

func getMeAllOfThem() {
	for i := maxSolution; i > 0; i-- {
		printIfValid(i)
	}
}

func main() {
	fmt.Printf("Running random search:\n")
	random()
	fmt.Printf("Running exhaustive, linear search:\n")
	getMeAllOfThem()
}
