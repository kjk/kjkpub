package genuid

import (
	"fmt"
	"testing"
)

const (
	// for manual debugging
	showIds = true
)

func Test(t *testing.T) {
	id1 := New()
	if len(id1) != 20 {
		t.Fatalf("len(id1) != 20 (=%d)", len(id1))
	}
	id2 := New()
	if len(id2) != 20 {
		t.Fatalf("len(id1) != 20 (=%d)", len(id2))
	}
	if id1 == id2 {
		t.Fatalf("generated same ids (id1: '%s', id2: '%s')", id1, id2)
	}
	if showIds {
		fmt.Printf("%s\n", id1)
		fmt.Printf("%s\n", id2)
	}
}

func TestMany(t *testing.T) {
	ids := make(map[string]bool)
	for i := 0; i < 100000; i++ {
		id := New()
		if _, exists := ids[id]; exists {
			t.Fatalf("generated duplicate id '%s'", id)
		}
		ids[id] = true
	}
}
