#include <string>
#include <vector>
#include <iostream>

// testing code from "Practical C++17" talk at
// https://www.youtube.com/watch?v=nnY4e4faNp0&index=8&list=WL

using namespace std;

struct sb {
    int fa;
    string fb;
};

void structuredBindings() {
    auto v = sb { 3, "foo" };
    auto [fa, fb] = v;
    cout << "a: " << fa << " b: " << fb << "\n";
}

void ifInitExpr(int a) {
    if (auto b = a; b == 3) {
        cout << "a is 3\n";
    } else {
        cout << "a is not 3\n";
    }
}

// before c++17 emplace_back() returned a reference, in C++ returns a reference
// same for emplace_front()
void emplaceBack() {
    vector<int> v;
    auto& vref = v.emplace_back(3);
    vref = 4;
    cout << "v[0] = " << v[0] << "\n";
}

namespace nested1 {
    namespace foo1 {
        void bar() { cout << "this is nestd1::foo1::bar\n"; };
    }
}

// a more compact form
namespace nested2::foo2 {
    void bar() { cout << "this is nestd2::foo2::bar\n"; };
}

void nestedNamespace() {
    nested1::foo1::bar();
    nested2::foo2::bar();
}

template<typename First, typename Second>
struct Pair {
    Pair(First t_first, Second t_second)
        : first(move(t_first)), second(move(t_second))
        {}
    First first;
    Second second;
};

void classTemplateTypeDeduction() {
    Pair p{1, 2.3}; // template parameters not needed
    cout << "p: " << &p << "\n";
}

void classTemplateTypeDeduction2() {
    vector v{1, 2, 3};
    cout << "v[0] = " << v[0] << "\n";
}

int const a = 3; // or int constexpr

void ifConstExpr() {
    if constexpr( a == 3) {
        cout << "a is 3\n";
    } else {
        cout << "a is not 3\n";
    }
}

// nexcept is part of the type system which means functions can be defined
// as noexcept

int main(int argc, char **argv) {
    //structuredBindings();
    //ifInitExpr(3);
    //emplaceBack();
    //nestedNamespace();
    //classTemplateTypeDeduction();
    classTemplateTypeDeduction2();
    //ifConstExpr();
    return 0;
}
