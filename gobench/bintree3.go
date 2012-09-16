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

type NodeId int

type Node struct {
	item        int
	left, right NodeId
}

const nodes_per_bucket = 1024 * 1024

var (
	all_nodes    [][]Node = make([][]Node, 0)
	nodes_left   int      = 0
	curr_node_id int      = 0
)

func NodeFromId(id NodeId) *Node {
	n := int(id) - 1
	bucket := n / nodes_per_bucket
	el := n % nodes_per_bucket
	return &all_nodes[bucket][el]
}

func allocNode(item int, left, right NodeId) NodeId {
	if 0 == nodes_left {
		new_nodes := make([]Node, nodes_per_bucket, nodes_per_bucket)
		all_nodes = append(all_nodes, new_nodes)
		nodes_left = nodes_per_bucket
	}
	nodes_left -= 1
	node := NodeFromId(NodeId(curr_node_id + 1))
	node.item = item
	node.left = left
	node.right = right

	nodes_left -= 1
	curr_node_id += 1
	return NodeId(curr_node_id)
}

func (n *Node) Left() *Node {
	return NodeFromId(n.left)
}

func (n *Node) Right() *Node {
	return NodeFromId(n.right)
}

func bottomUpTree(item, depth int) NodeId {
	if depth <= 0 {
		return allocNode(item, 0, 0)
	}
	return allocNode(item, bottomUpTree(2*item-1, depth-1), bottomUpTree(2*item, depth-1))
}

func (n *Node) itemCheck() int {
	if n.left == 0 {
		return n.item
	}
	return n.item + n.Left().itemCheck() - n.Right().itemCheck()
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

	check := NodeFromId(bottomUpTree(0, stretchDepth)).itemCheck()
	fmt.Printf("stretch tree of depth %d\t check: %d\n", stretchDepth, check)

	longLivedTree := NodeFromId(bottomUpTree(0, maxDepth))

	for depth := minDepth; depth <= maxDepth; depth += 2 {
		iterations := 1 << uint(maxDepth-depth+minDepth)
		check = 0

		for i := 1; i <= iterations; i++ {
			check += NodeFromId(bottomUpTree(i, depth)).itemCheck()
			check += NodeFromId(bottomUpTree(-i, depth)).itemCheck()
		}
		fmt.Printf("%d\t trees of depth %d\t check: %d\n", iterations*2, depth, check)
	}
	fmt.Printf("long lived tree of depth %d\t check: %d\n", maxDepth, longLivedTree.itemCheck())
	//fmt.Printf("Total nodes: %d\n", total_nodes)
}
