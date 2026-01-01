// MoveForwardSemantics.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <iostream>
#include <type_traits>
#include <typeinfo>
#include <typeindex>
#include <cstdlib>
#include <string>
#include <cstddef>
#include <stdexcept>
#include <cstring>
#include <ostream>
#include <chrono>
#include <cmath>
#include <algorithm>

//----------------MAGIC----------------------

#ifndef _MSC_VER
#  if __cplusplus < 201103
#    define CONSTEXPR11_TN
#    define CONSTEXPR14_TN
#    define NOEXCEPT_TN
#  elif __cplusplus < 201402
#    define CONSTEXPR11_TN constexpr
#    define CONSTEXPR14_TN
#    define NOEXCEPT_TN noexcept
#  else
#    define CONSTEXPR11_TN constexpr
#    define CONSTEXPR14_TN constexpr
#    define NOEXCEPT_TN noexcept
#  endif
#else  // _MSC_VER
#  if _MSC_VER < 1900
#    define CONSTEXPR11_TN
#    define CONSTEXPR14_TN
#    define NOEXCEPT_TN
#  elif _MSC_VER < 2000
#    define CONSTEXPR11_TN constexpr
#    define CONSTEXPR14_TN
#    define NOEXCEPT_TN noexcept
#  else
#    define CONSTEXPR11_TN constexpr
#    define CONSTEXPR14_TN constexpr
#    define NOEXCEPT_TN noexcept
#  endif
#endif  // _MSC_VER

class static_string
{
	const char* const p_;
	const std::size_t sz_;

public:
	typedef const char* const_iterator;

	template <std::size_t N>
	CONSTEXPR11_TN static_string(const char(&a)[N]) NOEXCEPT_TN
		: p_(a)
		, sz_(N - 1)
	{}

	CONSTEXPR11_TN static_string(const char* p, std::size_t N) NOEXCEPT_TN
		: p_(p)
		, sz_(N)
	{}

	CONSTEXPR11_TN const char* data() const NOEXCEPT_TN { return p_; }
	CONSTEXPR11_TN std::size_t size() const NOEXCEPT_TN { return sz_; }

	CONSTEXPR11_TN const_iterator begin() const NOEXCEPT_TN { return p_; }
	CONSTEXPR11_TN const_iterator end()   const NOEXCEPT_TN { return p_ + sz_; }

	CONSTEXPR11_TN char operator[](std::size_t n) const
	{
		return n < sz_ ? p_[n] : throw std::out_of_range("static_string");
	}
};

inline
std::ostream&
operator<<(std::ostream& os, static_string const& s)
{
	return os.write(s.data(), s.size());
}

template <class T>
CONSTEXPR14_TN
static_string
type_name()
{
#ifdef __clang__
	static_string p = __PRETTY_FUNCTION__;
	return static_string(p.data() + 31, p.size() - 31 - 1);
#elif defined(__GNUC__)
	static_string p = __PRETTY_FUNCTION__;
#  if __cplusplus < 201402
	return static_string(p.data() + 36, p.size() - 36 - 1);
#  else
	return static_string(p.data() + 46, p.size() - 46 - 1);
#  endif
#elif defined(_MSC_VER)
	static_string p = __FUNCSIG__;
	return static_string(p.data() + 38, p.size() - 38 - 7);
#endif
}

//----------------MAGIC----------------------


using namespace std;

struct IntHolder
{
	int value;
	IntHolder():value() {}
	IntHolder(int v) :value(v) {}
};

template<class Member>
struct MainClass
{
	Member val;
	MainClass():val() {}
	MainClass(Member v) :val(v) {}
	operator int() { return val.value; }
};


void Show(int& arg) { cout << "lvalue\n"; }
void Show(int&& arg) { cout << "rvalue\n"; }

void Show(MainClass<IntHolder>& arg) { cout << "c-lvalue\n"; }
void Show(MainClass<IntHolder>&& arg) { cout << "c-rvalue\n"; }


template<class Container, class Value>
struct helper
{
	typedef Value&& result;
};
template<class Container, class Value>
struct helper<Container&, Value>
{
	typedef Value& result;
};

template<class Container, class Value>
typename helper<Container, Value>::result move_if_rr(Container& arg)
{
	//cout << is_reference_v<Container> << "--   " << is_lvalue_reference_v<Container><<"--   " << is_rvalue_reference_v<Container> << "--   ";
	//cout << type_name<decltype(static_cast<typename helper<Container, Value>::result>(arg.val.value))>() << "  ";
	return static_cast<typename helper<Container, Value>::result>(arg.val.value);
};

template<class Container, class Value>
typename helper<Container, Value>::result move_if_rr(Container&& arg)
{
	//cout << is_reference_v<Container> << "--   " << is_lvalue_reference_v<Container> << "--   " << is_rvalue_reference_v<Container> << "--   ";
	//cout << type_name<decltype(static_cast<typename helper<Container, Value>::result>(arg.val.value))>() << "  ";
	return static_cast<typename helper<Container, Value>::result>(arg.val.value);
};

template<class type, class ret>
ret& move_if_rr2(type& arg)
{
	return arg.val.value;
}

template<class type, class ret>
ret&& move_if_rr2(type&& arg)
{
	return move(arg.val.value);
}

/*
template <typename type>
void Foo(type& arg)
{
	cout << "\n--&--"<<type_name<type>()<<"--&--\n";

	cout << "\nShow(arg): ";
	Show(arg);

	using rval_type = MainClass<IntHolder>;
	cout << "\nShow(RVAL): ";
	Show(rval_type(IntHolder(8)));
}
*/
template <typename type>
void Foo(type&& arg)
{
	cout << "\n--&&--" << type_name<type>() << "--&&--\n";

	cout << "\nShow(move_if_rr(arg)): ";
	Show(forward<type>(arg));

	cout << "\nShow(move_if_rr(arg)): ";
	Show(move_if_rr<type, int>(arg));

	cout << "\nmove_if_rr2(forward(arg)): ";
	Show(move_if_rr2<type, int>(forward<type>(arg)));

	cout << "\nShow(static_cast(arg)): ";
	Show(static_cast<type>(arg));

	using rval_type = MainClass<IntHolder>;
	cout << "\nShow(forward(RVAL)): ";
	Show(forward<rval_type>(rval_type(IntHolder(8))));

	cout << "\nShow(move_if_rr(RVAL)): ";
	Show(move_if_rr<rval_type, int>(rval_type(IntHolder(8))));

	cout << "\nmove_if_rr2(forward(RVAL)): ";
	Show(move_if_rr2<rval_type, int>(forward<rval_type>(rval_type(IntHolder(8)))));

	cout << "\nShow(static_cast(RVAL)): ";
	Show(static_cast<rval_type>(rval_type(IntHolder(8))));
}

void test_forwarding()
{
	MainClass<IntHolder> holder(IntHolder(8));

	cout << "\nFoo(IntHolder(8)): ";
	Foo(MainClass<IntHolder>(IntHolder(8)));

	cout << "\nFoo(holder): ";
	Foo(holder);
}

#define N 50'000'00

struct MoveClass
{
	int size;
	double* arr;
	MoveClass(int sz) :size(sz) {
		cout << "Construnctor Started\n";
		arr = new double[size];
		for (int i = 0; i < size; i++)
		{
			arr[i] = pow(i, 0.29)/7457.4;
		}
		cout << "Construnctor Ended\n";
	}
	~MoveClass() { cout << "~Destructor size = "<< size <<'\n'; if (size > 0) delete arr; }

	MoveClass(MoveClass& a) = delete;
	MoveClass& operator=(MoveClass& a) = delete;
	MoveClass(MoveClass&& a) { cout << "Move Constructor\n"; arr = a.arr; size = a.size; a.size = 0; }
	MoveClass& operator=(MoveClass&& a) { cout << "Move Operator\n"; swap(this->size,a.size); swap(this->arr, a.arr); return *this; }

	operator int() { cout << "operator int\n"; return size; }

	void print(int start, int end) {
		cout << "print()\n";
		for (int i = 0; i < size; i++)
		{
			if (i>start && i<end)
				cout << arr[i] << " ";
		}
		cout << "\n";
	}
};

template<class Out, class In>
using gfunc = Out (*)(In);

typedef void (* func)();

MoveClass create(int size)
{
	MoveClass mc(size);
	return mc;
}

template<class Out, class In>
Out time_shit(gfunc<Out, In> f, In in)
{
	auto start = chrono::high_resolution_clock::now();
	Out out = f(in);
	auto end = chrono::high_resolution_clock::now();
	cout << chrono::duration_cast<chrono::milliseconds>(end - start).count() << "\n\n\n";
	return out;
}

void test_moves()
{
	auto mc = time_shit<MoveClass, int>(create, N);
	cout << mc << '\n';
	mc.print(N - 100, N);
	MoveClass mc2(N / 2);
	mc = move(mc2);
}

int main() 
{
	test_forwarding();
		
	//test_moves();


	cin.get();
	return 0;
}
