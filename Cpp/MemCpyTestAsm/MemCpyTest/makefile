.PHONY: clean

all: MemCpyTest.o
	g++ -o out.exe MemCpyTest.o C:\Program_Files\CppLibs\OpenBLAS-0.2.20\package\lib\libopenblas.a

MemCpyTest.o: MemCpyTest.cpp
	g++ -I C:\Program_Files\CppLibs\Eigen -c -O3 MemCpyTest.cpp

debug: MemCpyTest.cpp
	g++ -g -I C:\Program_Files\CppLibs\Eigen -o out.exe MemCpyTest.cpp

disass: MemCpyTest.cpp
	g++ -S -O3 -I C:\Program_Files\CppLibs\Eigen -o out.s MemCpyTest.cpp

ass: out.s
	g++ out.s

clean:
	rm *.o