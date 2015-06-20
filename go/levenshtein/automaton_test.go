package levenshtein

import "testing"

func matches(a *Automaton, s string) bool {
	state := a.Start()
	for i := 0; i < len(s); i++ {
		state = a.Step(state, s[i])
		if !a.CanMatch(state) {
			return false
		}
	}
	return a.IsMatch(state)
}

func assertMatches(t *testing.T, a *Automaton, s string) {
	if !matches(a, s) {
		t.Errorf("'%s' doesn't match '%s'", s, a.Text)
	}
}

func assertNotMatches(t *testing.T, a *Automaton, s string) {
	if matches(a, s) {
		t.Errorf("'%s' matches '%s' (and shouldn't)", s, a.Text)
	}
}

func assertAllMatching(t *testing.T, term string, maxEdits int, toCheck ...string) {
	a := NewAutomaton(term, maxEdits)
	for _, s := range toCheck {
		assertMatches(t, a, s)
	}
}

func assertAllNotMatching(t *testing.T, term string, maxEdits int, toCheck ...string) {
	a := NewAutomaton(term, maxEdits)
	for _, s := range toCheck {
		assertNotMatches(t, a, s)
	}
}

func Test(t *testing.T) {
	assertAllMatching(t, "banana", 1, "banana", "canana", "banata", "banano")
	assertAllNotMatching(t, "banana", 1, "carana", "rotary", "glow")

	assertAllMatching(t, "banana", 2, "banana", "canana", "banata", "banano", "banato")
	assertAllNotMatching(t, "banana", 2, "corana", "rotary", "glow")
}
