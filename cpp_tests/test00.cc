#include <stdio.h>

#include <string>
#include <vector>

using namespace std;

#if 0
using std::string;
using std::vector;
#endif

#if 0
static void foo(const string& s) {
	const char *s2 = s.c_str();
	printf("foo: %s\n", s2);
}

static void bar(const string& s) {
	const char *s2 = s.c_str();
	printf("bar: %s\n", s2);
}

static void ex1() {
	string str("meow");
	const char *ptr = "purr";
	foo(str + ", " + ptr);
	bar(string(ptr) + ", " + str);
}
#endif

static const char s1[] = "fo\0" \
	"ba\0";

static const char s2[] =
	"fo\0"
	"ba\0";

static const char s3[] = "foo";
static const char s4[] = "foo\0";

static void ex2() {
	vector<string> v;
	string str("meow");
	v.push_back(str);
	v.push_back(str + "purr");
	v.emplace_back("kitty");
	v.emplace_back();
	printf("ex2: vec count: %lu\n", v.size());
}

static void testSize() {
	printf("size1: %lu, size2: %lu, size3: %lu, size4: %lu\n", sizeof(s1), sizeof(s2), sizeof(s3), sizeof(s4));
}

int main(int, char **) {
	ex2();
	testSize();
}
