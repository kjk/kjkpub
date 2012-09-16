/* The Computer Language Benchmarks Game
 * http://shootout.alioth.debian.org/
 *
 * contributed by The Go Authors.
 * based on C program by Kevin Carson
 * flag.Arg hack by Isaac Gouy
 * ~4x speed improved by Krzysztof Kowalczyk (http://blog.kowalczyk.info)
 * by writing custom allocator for nodes 
 */

package main

import (
	"flag"
	"fmt"
	"strconv"
)

var n = 0

type Node struct {
	item        int
	left, right *Node
}

const nodes_per_bucket = 1024 * 1024

var (
	all_nodes    [][]Node = make([][]Node, 0)
	curr_nodes   []Node
	nodes_left   int      = 0
	curr_node    int      = 0
)

func allocNode(item int, left, right *Node) *Node {
	if 0 == nodes_left {
		curr_nodes = make([]Node, nodes_per_bucket, nodes_per_bucket)
		all_nodes = append(all_nodes, curr_nodes)
		nodes_left = nodes_per_bucket
		curr_node = 0
	}
	if curr_node >= len(curr_nodes) {
		fmt.Printf("curr_node: %d, len(curr_nodes): %d, cap(curr_nodes): %d, len(all_nodes): %d\n", curr_node, len(curr_nodes), cap(curr_nodes), len(all_nodes))
	}
	node := &curr_nodes[curr_node]
	node.item = item
	node.left = left
	node.right = right

	nodes_left -= 1
	curr_node += 1
	return node
}

func bottomUpTree(item, depth int) *Node {
	if depth <= 0 {
		return allocNode(item, nil, nil)
	}
	return allocNode(item, bottomUpTree(2*item-1, depth-1), bottomUpTree(2*item, depth-1))
}

func (n *Node) itemCheck() int {
	if n.left == nil {
		return n.item
	}
	return n.item + n.left.itemCheck() - n.right.itemCheck()
}

const minDepth = 4

func main() {
	flag.Parse()
	if flag.NArg() > 0 {
		n, _ = strconv.Atoi(flag.Arg(0))
	}

	maxDepth := n
	if minDepth+2 > n {
		maxDepth = minDepth + 2
	}
	stretchDepth := maxDepth + 1

	check := bottomUpTree(0, stretchDepth).itemCheck()
	fmt.Printf("stretch tree of depth %d\t check: %d\n", stretchDepth, check)

	longLivedTree := bottomUpTree(0, maxDepth)

	for depth := minDepth; depth <= maxDepth; depth += 2 {
		iterations := 1 << uint(maxDepth-depth+minDepth)
		check = 0

		for i := 1; i <= iterations; i++ {
			check += bottomUpTree(i, depth).itemCheck()
			check += bottomUpTree(-i, depth).itemCheck()
		}
		fmt.Printf("%d\t trees of depth %d\t check: %d\n", iterations*2, depth, check)
	}
	fmt.Printf("long lived tree of depth %d\t check: %d\n", maxDepth, longLivedTree.itemCheck())
	//fmt.Printf("Total nodes: %d\n", total_nodes)
}
