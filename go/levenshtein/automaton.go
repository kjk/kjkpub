package levenshtein

// based on https://raw.githubusercontent.com/julesjacobs/levenshtein/master/levenshtein.py
// http://julesjacobs.github.io/2015/06/17/disqus-levenshtein-simple-and-fast.html

type Automaton struct {
	Text     string
	MaxEdits int
}

type State struct {
	whichHalf int
	arr       []int
}

func (s *State) getHalfs() ([]int, []int) {
	n := len(s.arr) / 2
	currStart := 0
	nextStart := n
	if s.whichHalf%2 == 1 {
		currStart = n
		nextStart = 0
	}
	curr := s.arr[currStart : currStart+n]
	next := s.arr[nextStart : nextStart+n]
	return curr, next
}

func min3(n1, n2, n3 int) int {
	if n1 < n2 {
		if n1 < n3 {
			return n1
		}
		return n3
	}
	// n2 <= n1
	if n2 < n3 {
		return n2
	}
	return n3
}

func min2(n1, n2 int) int {
	if n1 < n2 {
		return n1
	}
	return n2
}

func minArr(a []int) int {
	n := len(a)
	if n == 0 {
		return 0
	}
	min := a[0]
	for i := 0; i < n; i++ {
		if a[i] < min {
			min = a[i]
		}
	}
	return min
}

func NewAutomaton(s string, maxEdits int) *Automaton {
	return &Automaton{
		Text:     s,
		MaxEdits: maxEdits,
	}
}

func (a *Automaton) Start() *State {
	n := len(a.Text) + 1
	arr := make([]int, n*2, n*2)
	for i := 0; i < n; i++ {
		arr[i] = i
	}
	return &State{
		whichHalf: 0,
		arr:       arr,
	}
}

func (a *Automaton) Step(stateFull *State, c byte) {
	state, newState := stateFull.getHalfs()
	n := len(state)
	newState[0] = state[0] + 1
	for i := 0; i < n-1; i++ {
		cost := 0
		if a.Text[i] != c {
			cost = 1
		}
		n1 := newState[i] + 1
		n2 := state[i] + cost
		n3 := state[i+1] + 1
		newState[i+1] = min3(n1, n2, n3)
	}
	max := a.MaxEdits + 1
	for i := 0; i < len(newState); i++ {
		newState[i] = min2(newState[i], max)
	}
	stateFull.whichHalf++
}

func (a *Automaton) IsMatch(stateFull *State) bool {
	state, _ := stateFull.getHalfs()
	last := state[len(state)-1]
	return last <= a.MaxEdits
}

func (a *Automaton) CanMatch(stateFull *State) bool {
	state, _ := stateFull.getHalfs()
	min := minArr(state)
	return min <= a.MaxEdits
}
