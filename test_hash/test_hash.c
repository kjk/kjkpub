#include <windows.h>
#include <stdio.h>

typedef int int32;
typedef unsigned int uint32;
typedef unsigned short uint16_t;
typedef unsigned int uint32_t;
typedef unsigned char uint8_t;

extern const char *all_strings[];

/* TODO: count collisions */

typedef struct ms_timer {
#ifdef _WIN32
    LARGE_INTEGER   start;
    LARGE_INTEGER   end;
#else
    struct timeval  start;
    struct timeval  end;
#endif
} ms_timer;

/* milli-second timer */
#ifdef _WIN32
void ms_timer_start(ms_timer *timer)
{
    if (!timer)
        return;
    QueryPerformanceCounter(&timer->start);
}
void ms_timer_stop(ms_timer *timer)
{
    if (!timer)
        return;
    QueryPerformanceCounter(&timer->end);
}

double ms_timer_time_in_ms(ms_timer *timer)
{
    LARGE_INTEGER   freq;
    double          time_in_secs;
    QueryPerformanceFrequency(&freq);
    time_in_secs = (double)(timer->end.QuadPart-timer->start.QuadPart)/(double)freq.QuadPart;
    return time_in_secs * 1000.0;
}
#else
void ms_timer_start(ms_timer *timer)
{
    if (!timer)
        return;
    gettimeofday(&timer->start, NULL);
}

void ms_timer_stop(ms_timer *timer)
{
    if (!timer)
        return;
    gettimeofday(&timer->end, NULL);
}

double ms_timer_time_in_ms(ms_timer *timer)
{
    double timeInMs;
    time_t seconds;
    int    usecs;

    assert(timer);
    if (!timer)
        return 0.0;
    /* TODO: this logic needs to be verified */
    seconds = timer->end.tv_sec - timer->start.tv_sec;
    usecs = timer->end.tv_usec - timer->start.tv_usec;
    if (usecs < 0) {
        --seconds;
        usecs += 1000000;
    }
    timeInMs = (double)seconds*(double)1000.0 + (double)usecs/(double)1000.0;
    return timeInMs;
}
#endif

int32 syx_string_hash (const char* string)
{
  int32 ret;
  for (ret=0, string = string + 1; *string != '\0'; string++)
    ret += *string + *(string - 1);
  return ret;
}

int32 bernstein_hash2 (const char* s)
{
  unsigned char c;
  uint32 h = 5381;
  for (c = *s; c != '\0'; c = *++s)
    h = h * 33 + c;
  return (int32)(h & 0xffffffff);
}

#undef get16bits
#if (defined(__GNUC__) && defined(__i386__)) || defined(__WATCOMC__) \
  || defined(_MSC_VER) || defined (__BORLANDC__) || defined (__TURBOC__)
#define get16bits(d) (*((const uint16_t *) (d)))
#endif

#if !defined (get16bits)
#define get16bits(d) ((((uint32_t)(((const uint8_t *)(d))[1])) << 8)\
                       +(uint32_t)(((const uint8_t *)(d))[0]) )
#endif

int32 SuperFastHash2 (const char * data, int len) 
{
  uint32_t hash = len, tmp;
  int rem;

    if (len <= 0 || data == NULL) return 0;

    rem = len & 3;
    len >>= 2;

    /* Main loop */
    for (;len > 0; len--) {
        hash  += get16bits (data);
        tmp    = (get16bits (data+2) << 11) ^ hash;
        hash   = (hash << 16) ^ tmp;
        data  += 2*sizeof (uint16_t);
        hash  += hash >> 11;
    }

    /* Handle end cases */
    switch (rem) {
        case 3: hash += get16bits (data);
                hash ^= hash << 16;
                hash ^= data[sizeof (uint16_t)] << 18;
                hash += hash >> 11;
                break;
        case 2: hash += get16bits (data);
                hash ^= hash << 11;
                hash += hash >> 17;
                break;
        case 1: hash += *data;
                hash ^= hash << 10;
                hash += hash >> 1;
    }

    /* Force "avalanching" of final 127 bits */
    hash ^= hash << 3;
    hash += hash >> 5;
    hash ^= hash << 4;
    hash += hash >> 17;
    hash ^= hash << 25;
    hash += hash >> 6;

    return (int32)hash;
}

#define SuperFastHash(str) SuperFastHash2(str, strlen(str))

#define ITERS 1000

int32 hash1f()
{
  int i;
  int32 hash;
  for (i=0; i<ITERS; i++) {
    const char **txt = all_strings;
    while (*txt) {
      hash = syx_string_hash(*txt);
      ++txt;
    }
  }
  return hash;
}

int32 hash2f()
{
  int i;
  int32 hash;
  for (i=0; i<ITERS; i++) {
    const char **txt = all_strings;
    while (*txt) {
      hash = bernstein_hash2(*txt);
      ++txt;
    }
  }
  return hash;
}

int32 hash3f()
{
  int i;
  int32 hash;
  for (i=0; i<ITERS; i++) {
    const char **txt = all_strings;
    while (*txt) {
      hash = SuperFastHash(*txt);
      ++txt;
    }
  }
  return hash;
}

int main(int argc, char **argv)
{
  int32 total = 0;
  ms_timer hash1, hash2, hash3;

  ms_timer_start(&hash1);
  total += hash1f();
  ms_timer_stop(&hash1);

  ms_timer_start(&hash2);
  total += hash2f();
  ms_timer_stop(&hash2);

  ms_timer_start(&hash3);
  total += hash3f();
  ms_timer_stop(&hash3);

  printf("total: %d\n", total);
  printf("hash1: %.4f\n", ms_timer_time_in_ms(&hash1));
  printf("hash2: %.4f\n", ms_timer_time_in_ms(&hash2));
  printf("hash3: %.4f\n", ms_timer_time_in_ms(&hash3));
}
