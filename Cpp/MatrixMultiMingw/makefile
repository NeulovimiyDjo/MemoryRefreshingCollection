.PHONY: all clean

all: prog mydll

clean: 
	rm *.o

#g++ -o prog.exe prog.o -I C:\Program_Files\CppLibs\OpenBLAS-0.2.20\package\include -L C:\Program_Files\CppLibs\OpenBLAS-0.2.20\package\lib -lopenblas -lpthread -lgfortran
prog: prog.o
	g++ -o prog.exe prog.o C:\Program_Files\CppLibs\OpenBLAS-0.2.20\package\lib\libopenblas.a

prog.o: prog.cpp
	g++ -I C:\Program_Files\CppLibs\Eigen -c -O3 prog.cpp

mydll: mydll.o
	g++ -shared -o mydll.dll mydll.o

mydll.o: mydll.cpp
	g++ -c -O3 mydll.cpp