package genuid

import (
	"math/rand"
	"sync"
	"time"
)

/*

Fancy ID generator that creates 20-character string identifiers with the following properties:
1. They're based on timestamp so that they sort *after* any existing ids.
2. They contain 72-bits of random data after the timestamp so that IDs won't collide with other clients' IDs.
3. They sort *lexicographically* (so the timestamp is converted to characters that will sort properly).
4. They're monotonically increasing.  Even if you generate more than one in the same timestamp, the
   latter ones will sort after the former ones.  We do this by using the previous random bits
   but "incrementing" them by 1 (only in the case of a timestamp collision).

Read https://www.firebase.com/blog/2015-02-11-firebase-unique-identifiers.html
for more info.

Based on https://gist.github.com/mikelehen/3596a30bd69384624c11
*/

const (
	// Modeled after base64 web-safe chars, but ordered by ASCII.
	pushChars = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz"
)

var (
	// Timestamp of last push, used to prevent local collisions if you push twice in one ms.
	lastPushTimeMs int64
	// We generate 72-bits of randomness which get turned into 12 characters and appended to the
	// timestamp to prevent collisions with other clients.  We store the last characters we
	// generated because in the event of a collision, we'll use those same characters except
	// "incremented" by one.
	lastRandChars [12]int
	mu            sync.Mutex
	rnd           *rand.Rand
)

func init() {
	// have to seed
	rnd = rand.New(rand.NewSource(time.Now().UnixNano()))
	for i := 0; i < 12; i++ {
		lastRandChars[i] = rnd.Intn(64)
	}
}

func New() string {
	var id [8 + 12]byte
	timeMs := time.Now().UnixNano() / 1e6
	mu.Lock()
	if timeMs == lastPushTimeMs {
		// increment lastRandChars
		for i := 0; i < 12; i++ {
			lastRandChars[i]++
			if lastRandChars[i] < 64 {
				break
			}
			// increment the next byte
			lastRandChars[i] = 0
		}
	}
	lastPushTimeMs = timeMs
	// put random as the second part
	for i := 0; i < 12; i++ {
		id[19-i] = pushChars[lastRandChars[i]]
	}
	mu.Unlock()

	// put current time at the beginning
	for i := 7; i >= 0; i-- {
		n := int(timeMs % 64)
		id[i] = pushChars[n]
		timeMs = timeMs / 64
	}
	return string(id[:])
}
