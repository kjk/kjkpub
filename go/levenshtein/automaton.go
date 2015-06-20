package levenshtein

// based on https://raw.githubusercontent.com/julesjacobs/levenshtein/master/levenshtein.py
// http://julesjacobs.github.io/2015/06/17/disqus-levenshtein-simple-and-fast.html

type Automaton struct {
	Text     string
	MaxEdits int
}

type State []int

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

func (a *Automaton) Start() State {
	n := len(a.Text) + 1
	res := make([]int, n, n)
	for i := 0; i < n; i++ {
		res[i] = i
	}
	return res
}

func (a *Automaton) Step(state State, c byte) State {
	n := len(state)
	newState := make([]int, n, n)
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
	return newState
}

func (a *Automaton) IsMatch(state State) bool {
	last := state[len(state)-1]
	return last <= a.MaxEdits
}

func (a *Automaton) CanMatch(state State) bool {
	min := minArr(state)
	return min <= a.MaxEdits
}
