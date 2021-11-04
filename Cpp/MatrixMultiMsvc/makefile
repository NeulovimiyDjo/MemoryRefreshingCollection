.PHONY: all clean

all: prog mydll

clean: 
	del *.exp *.lib *.obj

prog: prog.cpp
	cl /EHsc /O2 prog.cpp

mydll: mydll.cpp
	cl /LD /EHsc /O2 mydll.cpp