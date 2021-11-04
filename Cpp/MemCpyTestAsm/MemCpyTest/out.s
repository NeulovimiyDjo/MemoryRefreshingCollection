	.file	"MemCpyTest.cpp"
	.text
	.p2align 4,,15
	.def	__tcf_0;	.scl	3;	.type	32;	.endef
	.seh_proc	__tcf_0
__tcf_0:
	.seh_endprologue
	leaq	_ZStL8__ioinit(%rip), %rcx
	jmp	_ZNSt8ios_base4InitD1Ev
	.seh_endproc
	.p2align 4,,15
	.globl	_Z10initializePf
	.def	_Z10initializePf;	.scl	2;	.type	32;	.endef
	.seh_proc	_Z10initializePf
_Z10initializePf:
	.seh_endprologue
	movq	%rcx, %rax
	shrq	$2, %rax
	negq	%rax
	andl	$3, %eax
	je	.L8
	movss	.LC0(%rip), %xmm0
	cmpl	$1, %eax
	movss	%xmm0, (%rcx)
	je	.L9
	cmpl	$2, %eax
	movss	%xmm0, 4(%rcx)
	je	.L10
	movss	%xmm0, 8(%rcx)
	movl	$239997, %r11d
	movl	$3, %r9d
.L4:
	movl	$240000, %r10d
	movaps	.LC1(%rip), %xmm0
	subl	%eax, %r10d
	movl	%eax, %eax
	leaq	(%rcx,%rax,4), %rdx
	movl	%r10d, %r8d
	xorl	%eax, %eax
	shrl	$2, %r8d
	.p2align 4,,10
.L6:
	addl	$1, %eax
	movaps	%xmm0, (%rdx)
	addq	$16, %rdx
	cmpl	%eax, %r8d
	ja	.L6
	movl	%r10d, %r8d
	movl	%r11d, %edx
	andl	$-4, %r8d
	leal	(%r8,%r9), %eax
	subl	%r8d, %edx
	cmpl	%r8d, %r10d
	je	.L3
	movss	.LC0(%rip), %xmm0
	movslq	%eax, %r8
	cmpl	$1, %edx
	movss	%xmm0, (%rcx,%r8,4)
	leal	1(%rax), %r8d
	je	.L3
	movslq	%r8d, %r8
	addl	$2, %eax
	cmpl	$2, %edx
	movss	%xmm0, (%rcx,%r8,4)
	je	.L3
	cltq
	movss	%xmm0, (%rcx,%rax,4)
.L3:
	ret
.L9:
	movl	$239999, %r11d
	movl	$1, %r9d
	jmp	.L4
.L8:
	movl	$240000, %r11d
	xorl	%r9d, %r9d
	jmp	.L4
.L10:
	movl	$239998, %r11d
	movl	$2, %r9d
	jmp	.L4
	.seh_endproc
	.section .rdata,"dr"
	.align 8
.LC2:
	.ascii "C:\\Program_Files\\CppLibs\\Eigen/Eigen/src/Core/DenseCoeffsBase.h\0"
.LC3:
	.ascii "index >= 0 && index < size()\0"
	.text
	.p2align 4,,15
	.globl	_Z13aliasing_testRSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE
	.def	_Z13aliasing_testRSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE;	.scl	2;	.type	32;	.endef
	.seh_proc	_Z13aliasing_testRSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE
_Z13aliasing_testRSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE:
	pushq	%r15
	.seh_pushreg	%r15
	pushq	%r14
	.seh_pushreg	%r14
	pushq	%r13
	.seh_pushreg	%r13
	pushq	%r12
	.seh_pushreg	%r12
	pushq	%rbp
	.seh_pushreg	%rbp
	pushq	%rdi
	.seh_pushreg	%rdi
	pushq	%rsi
	.seh_pushreg	%rsi
	pushq	%rbx
	.seh_pushreg	%rbx
	subq	$56, %rsp
	.seh_stackalloc	56
	.seh_endprologue
	movl	$7500, %esi
	movl	$240000, %r12d
	movq	%rcx, %rbp
	movq	%rdx, %rdi
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movq	%rax, %rbx
	.p2align 4,,10
.L27:
	cmpq	$0, 8(%rdi)
	jle	.L44
.L19:
	movq	(%rdi), %r8
	movq	0(%rbp), %r9
	leaq	16(%r8), %rax
	cmpq	%rax, %r9
	jnb	.L32
	leaq	16(%r9), %rax
	cmpq	%rax, %r8
	jb	.L28
.L32:
	movq	%r8, %rax
	shrq	$2, %rax
	negq	%rax
	andl	$3, %eax
	je	.L29
	movss	(%r8), %xmm0
	cmpl	$1, %eax
	movss	%xmm0, (%r9)
	je	.L30
	movss	4(%r8), %xmm0
	cmpl	$2, %eax
	movss	%xmm0, 4(%r9)
	je	.L31
	movss	8(%r8), %xmm0
	movl	$239997, %r13d
	movl	$3, %r11d
	movss	%xmm0, 8(%r9)
.L22:
	movl	%r12d, %r14d
	movl	%eax, %ecx
	xorl	%edx, %edx
	subl	%eax, %r14d
	salq	$2, %rcx
	xorl	%eax, %eax
	leaq	(%r8,%rcx), %r15
	movl	%r14d, %r10d
	addq	%r9, %rcx
	shrl	$2, %r10d
	.p2align 4,,10
.L24:
	movaps	(%r15,%rax), %xmm0
	addl	$1, %edx
	movlps	%xmm0, (%rcx,%rax)
	movhps	%xmm0, 8(%rcx,%rax)
	addq	$16, %rax
	cmpl	%r10d, %edx
	jb	.L24
	movl	%r14d, %eax
	andl	$-4, %eax
	addl	%eax, %r11d
	subl	%eax, %r13d
	cmpl	%eax, %r14d
	je	.L26
	movslq	%r11d, %rax
	cmpl	$1, %r13d
	movss	(%r8,%rax,4), %xmm0
	movss	%xmm0, (%r9,%rax,4)
	leal	1(%r11), %eax
	je	.L26
	cltq
	addl	$2, %r11d
	cmpl	$2, %r13d
	movss	(%r8,%rax,4), %xmm0
	movss	%xmm0, (%r9,%rax,4)
	je	.L26
	movslq	%r11d, %rax
	movss	(%r8,%rax,4), %xmm0
	movss	%xmm0, (%r9,%rax,4)
.L26:
	subl	$1, %esi
	jne	.L27
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movabsq	$4835703278458516699, %rdx
	subq	%rbx, %rax
	movq	%rax, %rcx
	imulq	%rdx
	sarq	$63, %rcx
	sarq	$18, %rdx
	subq	%rcx, %rdx
	movq	.refptr._ZSt4cout(%rip), %rcx
	call	_ZNSo9_M_insertIxEERSoT_
	leaq	47(%rsp), %rdx
	movl	$1, %r8d
	movb	$10, 47(%rsp)
	movq	%rax, %rcx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	nop
	addq	$56, %rsp
	popq	%rbx
	popq	%rsi
	popq	%rdi
	popq	%rbp
	popq	%r12
	popq	%r13
	popq	%r14
	popq	%r15
	ret
	.p2align 4,,10
.L44:
	leaq	.LC2(%rip), %rdx
	movl	$408, %r8d
	leaq	.LC3(%rip), %rcx
	call	_assert
	jmp	.L19
.L28:
	xorl	%eax, %eax
	.p2align 4,,10
.L20:
	movss	(%r8,%rax), %xmm0
	movss	%xmm0, (%r9,%rax)
	addq	$4, %rax
	cmpq	$960000, %rax
	jne	.L20
	jmp	.L26
.L30:
	movl	$239999, %r13d
	movl	$1, %r11d
	jmp	.L22
.L29:
	movl	$240000, %r13d
	xorl	%r11d, %r11d
	jmp	.L22
.L31:
	movl	$239998, %r13d
	movl	$2, %r11d
	jmp	.L22
	.seh_endproc
	.p2align 4,,15
	.globl	_Z21aliasing_test_movableSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE
	.def	_Z21aliasing_test_movableSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE;	.scl	2;	.type	32;	.endef
	.seh_proc	_Z21aliasing_test_movableSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE
_Z21aliasing_test_movableSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE:
	pushq	%r15
	.seh_pushreg	%r15
	pushq	%r14
	.seh_pushreg	%r14
	pushq	%r13
	.seh_pushreg	%r13
	pushq	%r12
	.seh_pushreg	%r12
	pushq	%rbp
	.seh_pushreg	%rbp
	pushq	%rdi
	.seh_pushreg	%rdi
	pushq	%rsi
	.seh_pushreg	%rsi
	pushq	%rbx
	.seh_pushreg	%rbx
	subq	$56, %rsp
	.seh_stackalloc	56
	.seh_endprologue
	leaq	.LC2(%rip), %rdi
	movl	$7500, %ebp
	leaq	.LC3(%rip), %rsi
	movq	%rcx, %r12
	movq	%rdx, %r15
	movq	%r8, %r14
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movq	%rax, %r13
	.p2align 4,,10
.L51:
	movq	8(%r14), %rax
	testq	%rax, %rax
	jle	.L59
.L46:
	xorl	%ebx, %ebx
	movq	(%r14), %rdx
	movq	(%r15), %rcx
	.p2align 4,,10
.L50:
	cmpq	%rax, %rbx
	jge	.L60
	movaps	(%rdx,%rbx,4), %xmm0
	movaps	%xmm0, (%rcx,%rbx,4)
	addq	$4, %rbx
	cmpq	$240000, %rbx
	jne	.L50
.L48:
	subl	$1, %ebp
	jne	.L51
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movabsq	$4835703278458516699, %rdx
	subq	%r13, %rax
	movq	%rax, %rcx
	imulq	%rdx
	sarq	$63, %rcx
	sarq	$18, %rdx
	subq	%rcx, %rdx
	movq	.refptr._ZSt4cout(%rip), %rcx
	call	_ZNSo9_M_insertIxEERSoT_
	leaq	47(%rsp), %rdx
	movl	$1, %r8d
	movb	$10, 47(%rsp)
	movq	%rax, %rcx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	(%r15), %rax
	movq	$0, (%r15)
	movq	%rax, (%r12)
	movq	8(%r15), %rax
	movq	$0, 8(%r15)
	movq	%rax, 8(%r12)
	movq	16(%r15), %rax
	movq	$0, 16(%r15)
	movq	%rax, 16(%r12)
	movq	%r12, %rax
	addq	$56, %rsp
	popq	%rbx
	popq	%rsi
	popq	%rdi
	popq	%rbp
	popq	%r12
	popq	%r13
	popq	%r14
	popq	%r15
	ret
.L59:
	movl	$408, %r8d
	movq	%rdi, %rdx
	movq	%rsi, %rcx
	call	_assert
	movq	8(%r14), %rax
	jmp	.L46
	.p2align 4,,10
.L60:
	movl	$408, %r8d
	movq	%rdi, %rdx
	movq	%rsi, %rcx
	call	_assert
	movq	(%r14), %rax
	movss	(%rax,%rbx,4), %xmm0
	movq	(%r15), %rax
	movss	%xmm0, (%rax,%rbx,4)
	addq	$1, %rbx
	cmpq	$240000, %rbx
	je	.L48
	movq	8(%r14), %rax
	jmp	.L50
	.seh_endproc
	.p2align 4,,15
	.globl	_Z7funclolRN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEESt10unique_ptrIfSt14default_deleteIfEE
	.def	_Z7funclolRN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEESt10unique_ptrIfSt14default_deleteIfEE;	.scl	2;	.type	32;	.endef
	.seh_proc	_Z7funclolRN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEESt10unique_ptrIfSt14default_deleteIfEE
_Z7funclolRN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEESt10unique_ptrIfSt14default_deleteIfEE:
	pushq	%rdi
	.seh_pushreg	%rdi
	pushq	%rsi
	.seh_pushreg	%rsi
	pushq	%rbx
	.seh_pushreg	%rbx
	subq	$48, %rsp
	.seh_stackalloc	48
	.seh_endprologue
	movq	(%rdx), %rsi
	movq	$0, (%rdx)
	movq	%rcx, %rbx
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movq	%rax, %rdi
	movl	$7500, %eax
	.p2align 4,,10
.L62:
	subl	$1, %eax
	movq	%rsi, (%rbx)
	movq	$240000, 8(%rbx)
	jne	.L62
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movabsq	$4835703278458516699, %rdx
	subq	%rdi, %rax
	movq	%rax, %rcx
	imulq	%rdx
	sarq	$63, %rcx
	sarq	$18, %rdx
	subq	%rcx, %rdx
	movq	.refptr._ZSt4cout(%rip), %rcx
	call	_ZNSo9_M_insertIxEERSoT_
	leaq	47(%rsp), %rdx
	movl	$1, %r8d
	movb	$10, 47(%rsp)
	movq	%rax, %rcx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	nop
	addq	$48, %rsp
	popq	%rbx
	popq	%rsi
	popq	%rdi
	ret
	.seh_endproc
	.def	__gxx_personality_sj0;	.scl	2;	.type	32;	.endef
	.def	_Unwind_SjLj_Register;	.scl	2;	.type	32;	.endef
	.def	_Unwind_SjLj_Unregister;	.scl	2;	.type	32;	.endef
	.p2align 4,,15
	.globl	_Z11test_memcpyv
	.def	_Z11test_memcpyv;	.scl	2;	.type	32;	.endef
	.seh_proc	_Z11test_memcpyv
_Z11test_memcpyv:
	pushq	%rbp
	.seh_pushreg	%rbp
	pushq	%rbx
	.seh_pushreg	%rbx
	subq	$344, %rsp
	.seh_stackalloc	344
	.seh_endprologue
	leaq	__gxx_personality_sj0(%rip), %rax
	movq	%rax, 176(%rsp)
	leaq	.LLSDA8497(%rip), %rax
	movq	%rax, 184(%rsp)
	leaq	336(%rsp), %rax
	movq	%rax, 192(%rsp)
	leaq	.L113(%rip), %rax
	movq	%rax, 200(%rsp)
	leaq	128(%rsp), %rax
	movq	%rax, %rcx
	movq	%rax, 232(%rsp)
	movq	%rsp, 208(%rsp)
	call	_Unwind_SjLj_Register
	movl	$960000, %ecx
	movl	$-1, 136(%rsp)
	call	_Znay
	movl	$960000, %ecx
	movl	$3, 136(%rsp)
	movq	%rax, 48(%rsp)
	call	_Znay
	movq	%rax, 104(%rsp)
	movq	48(%rsp), %rax
	movq	%rax, %rdx
	shrq	$2, %rdx
	negq	%rdx
	andl	$3, %edx
	je	.L104
	movss	.LC0(%rip), %xmm0
	cmpl	$1, %edx
	movss	%xmm0, (%rax)
	je	.L105
	cmpl	$2, %edx
	movss	%xmm0, 4(%rax)
	je	.L106
	movss	%xmm0, 8(%rax)
	movl	$239997, %ecx
	movl	$3, %eax
.L65:
	movq	48(%rsp), %rbx
	movl	$240000, %r9d
	subl	%edx, %r9d
	movaps	.LC1(%rip), %xmm0
	movl	%edx, %edx
	movl	%r9d, %r10d
	shrl	$2, %r10d
	leaq	(%rbx,%rdx,4), %r8
	xorl	%edx, %edx
	.p2align 4,,10
.L67:
	addl	$1, %edx
	movaps	%xmm0, (%r8)
	addq	$16, %r8
	cmpl	%r10d, %edx
	jb	.L67
	movl	%r9d, %edx
	andl	$-4, %edx
	addl	%edx, %eax
	subl	%edx, %ecx
	cmpl	%edx, %r9d
	je	.L68
	movq	48(%rsp), %rbx
	movslq	%eax, %rdx
	cmpl	$1, %ecx
	movss	.LC0(%rip), %xmm0
	movss	%xmm0, (%rbx,%rdx,4)
	leal	1(%rax), %edx
	je	.L68
	movslq	%edx, %rdx
	addl	$2, %eax
	cmpl	$2, %ecx
	movss	%xmm0, (%rbx,%rdx,4)
	je	.L68
	cltq
	movss	%xmm0, (%rbx,%rax,4)
.L68:
	movl	$3, 136(%rsp)
	movl	$960000, %ecx
	call	_Znwy
	movq	%rax, %rbx
	movq	%rax, 40(%rsp)
	leaq	960000(%rax), %rax
	movq	%rax, 112(%rsp)
	movq	%rbx, %rax
	shrq	$2, %rax
	negq	%rax
	movq	%rax, 64(%rsp)
	andl	$3, %eax
	movq	%rax, %r8
	je	.L107
	movss	.LC4(%rip), %xmm0
	cmpq	$1, %r8
	movl	$239999, %eax
	leaq	4(%rbx), %rcx
	movss	%xmm0, (%rbx)
	je	.L69
	cmpq	$2, %r8
	movss	%xmm0, 4(%rbx)
	movl	$239998, %eax
	leaq	8(%rbx), %rcx
	je	.L69
	movss	%xmm0, 8(%rbx)
	leaq	12(%rbx), %rcx
	movl	$239997, %eax
.L69:
	movaps	.LC5(%rip), %xmm0
	leaq	(%rbx,%r8,4), %r9
	movl	$240000, %edx
	subq	%r8, %rdx
	xorl	%r8d, %r8d
	movq	%rdx, %r10
	shrq	$2, %r10
.L71:
	addq	$1, %r8
	movaps	%xmm0, (%r9)
	addq	$16, %r9
	cmpq	%r10, %r8
	jb	.L71
	movq	%rdx, %r8
	andq	$-4, %r8
	leaq	(%rcx,%r8,4), %rcx
	subq	%r8, %rax
	cmpq	%r8, %rdx
	je	.L72
	movss	.LC4(%rip), %xmm0
	cmpq	$1, %rax
	movss	%xmm0, (%rcx)
	je	.L72
	cmpq	$2, %rax
	movss	%xmm0, 4(%rcx)
	je	.L72
	movss	%xmm0, 8(%rcx)
.L72:
	movq	$0, 256(%rsp)
	movq	$0, 264(%rsp)
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movl	$7500, 56(%rsp)
	movq	%rax, 72(%rsp)
	.p2align 4,,10
.L73:
	movq	48(%rsp), %rdx
	movl	$960000, %r8d
	movq	40(%rsp), %rcx
	call	memcpy
	subl	$1, 56(%rsp)
	jne	.L73
	call	_ZNSt6chrono3_V212system_clock3nowEv
	subq	72(%rsp), %rax
	movabsq	$4835703278458516699, %rdx
	movl	$1, 136(%rsp)
	movq	%rax, %rcx
	imulq	%rdx
	sarq	$63, %rcx
	sarq	$18, %rdx
	subq	%rcx, %rdx
	movq	.refptr._ZSt4cout(%rip), %rcx
	call	_ZNSo9_M_insertIxEERSoT_
	leaq	252(%rsp), %rdx
	movl	$1, %r8d
	movq	%rax, %rcx
	movb	$10, 252(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movl	$7500, 56(%rsp)
	movq	%rax, 72(%rsp)
	movq	48(%rsp), %xmm8
	movq	40(%rsp), %xmm9
	.p2align 4,,10
.L74:
	movq	%xmm8, %rdx
	movq	%xmm9, %rcx
	movl	$960000, %r8d
	call	memcpy
	subl	$1, 56(%rsp)
	jne	.L74
	call	_ZNSt6chrono3_V212system_clock3nowEv
	subq	72(%rsp), %rax
	movabsq	$4835703278458516699, %rdx
	movl	$1, 136(%rsp)
	movq	%rax, %rcx
	imulq	%rdx
	sarq	$63, %rcx
	sarq	$18, %rdx
	subq	%rcx, %rdx
	movq	.refptr._ZSt4cout(%rip), %rcx
	call	_ZNSo9_M_insertIxEERSoT_
	leaq	253(%rsp), %rdx
	movl	$1, %r8d
	movq	%rax, %rcx
	movb	$10, 253(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movq	48(%rsp), %rdx
	movq	%rax, 56(%rsp)
	movl	$7500, %eax
	.p2align 4,,10
.L75:
	subl	$1, %eax
	movq	%rdx, 256(%rsp)
	movq	$240000, 264(%rsp)
	jne	.L75
	call	_ZNSt6chrono3_V212system_clock3nowEv
	subq	56(%rsp), %rax
	movabsq	$4835703278458516699, %rdx
	movl	$6, 136(%rsp)
	movq	%rax, %rcx
	imulq	%rdx
	sarq	$63, %rcx
	sarq	$18, %rdx
	subq	%rcx, %rdx
	movq	.refptr._ZSt4cout(%rip), %rcx
	call	_ZNSo9_M_insertIxEERSoT_
	leaq	254(%rsp), %rdx
	movl	$1, %r8d
	movq	%rax, %rcx
	movb	$10, 254(%rsp)
	movl	$5, 136(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movl	64(%rsp), %ecx
	movl	$7500, 56(%rsp)
	movq	%rax, 120(%rsp)
	movl	$240000, %eax
	andl	$3, %ecx
	subl	%ecx, %eax
	movl	%ecx, 84(%rsp)
	movl	%eax, %ebx
	movl	%eax, 80(%rsp)
	shrl	$2, %eax
	movl	%eax, 72(%rsp)
	movl	%ecx, %eax
	movq	40(%rsp), %rcx
	andl	$-4, %ebx
	salq	$2, %rax
	movl	%ebx, 64(%rsp)
	movq	%rax, 88(%rsp)
	addq	%rax, %rcx
	movq	%rcx, 96(%rsp)
	.p2align 4,,10
.L92:
	cmpq	$0, 264(%rsp)
	jle	.L81
.L84:
	movq	256(%rsp), %r8
	movq	40(%rsp), %rbx
	leaq	16(%r8), %rax
	cmpq	%rax, %rbx
	jnb	.L82
	leaq	16(%rbx), %rax
	cmpq	%rax, %r8
	jb	.L172
.L82:
	movl	84(%rsp), %eax
	testl	%eax, %eax
	je	.L110
	movq	40(%rsp), %rbx
	cmpl	$1, %eax
	movss	(%r8), %xmm0
	movss	%xmm0, (%rbx)
	je	.L111
	movss	4(%r8), %xmm0
	cmpl	$2, %eax
	movss	%xmm0, 4(%rbx)
	je	.L112
	movss	8(%r8), %xmm0
	movl	$239997, %r10d
	movl	$3, %r9d
	movss	%xmm0, 8(%rbx)
.L88:
	movq	88(%rsp), %rax
	xorl	%edx, %edx
	movl	72(%rsp), %r11d
	movq	96(%rsp), %rbx
	leaq	(%r8,%rax), %rcx
	xorl	%eax, %eax
	.p2align 4,,10
.L91:
	pxor	%xmm0, %xmm0
	movlps	(%rcx,%rax), %xmm0
	addl	$1, %edx
	movhps	8(%rcx,%rax), %xmm0
	movaps	%xmm0, (%rbx,%rax)
	addq	$16, %rax
	cmpl	%edx, %r11d
	ja	.L91
	movl	64(%rsp), %ebx
	movl	%r10d, %edx
	leal	(%r9,%rbx), %eax
	subl	%ebx, %edx
	cmpl	80(%rsp), %ebx
	je	.L87
	movq	40(%rsp), %rbx
	movslq	%eax, %rcx
	cmpl	$1, %edx
	movss	(%r8,%rcx,4), %xmm0
	movss	%xmm0, (%rbx,%rcx,4)
	leal	1(%rax), %ecx
	je	.L87
	movslq	%ecx, %rcx
	addl	$2, %eax
	cmpl	$2, %edx
	movss	(%r8,%rcx,4), %xmm0
	movss	%xmm0, (%rbx,%rcx,4)
	je	.L87
	cltq
	movss	(%r8,%rax,4), %xmm0
	movss	%xmm0, (%rbx,%rax,4)
.L87:
	subl	$1, 56(%rsp)
	jne	.L92
	call	_ZNSt6chrono3_V212system_clock3nowEv
	subq	120(%rsp), %rax
	movabsq	$4835703278458516699, %rdx
	movl	$8, 136(%rsp)
	movq	%rax, %rcx
	imulq	%rdx
	sarq	$63, %rcx
	sarq	$18, %rdx
	subq	%rcx, %rdx
	movq	.refptr._ZSt4cout(%rip), %rcx
	call	_ZNSo9_M_insertIxEERSoT_
	leaq	255(%rsp), %rdx
	movl	$1, %r8d
	movq	%rax, %rcx
	movb	$10, 255(%rsp)
	movl	$4, 136(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	112(%rsp), %rax
	leaq	272(%rsp), %rcx
	movl	$2, 136(%rsp)
	movq	40(%rsp), %xmm0
	leaq	304(%rsp), %rdx
	leaq	256(%rsp), %r8
	movq	%rax, %xmm1
	movq	%rax, 320(%rsp)
	punpcklqdq	%xmm1, %xmm0
	movaps	%xmm0, 304(%rsp)
	call	_Z21aliasing_test_movableSt6vectorIfSaIfEERN5Eigen6MatrixIfLin1ELi1ELi0ELin1ELi1EEE
	movq	304(%rsp), %rcx
	testq	%rcx, %rcx
	je	.L97
	call	_ZdlPv
.L97:
	movq	104(%rsp), %rcx
	movl	$4, %edx
	call	_ZdlPvy
	movq	272(%rsp), %rcx
	testq	%rcx, %rcx
	je	.L98
	call	_ZdlPv
.L98:
	movq	256(%rsp), %rcx
	call	free
	movq	232(%rsp), %rcx
	call	_Unwind_SjLj_Unregister
	nop
	addq	$344, %rsp
	popq	%rbx
	popq	%rbp
	ret
	.p2align 4,,10
.L81:
	leaq	.LC2(%rip), %rdx
	movl	$408, %r8d
	movl	$7, 136(%rsp)
	leaq	.LC3(%rip), %rcx
	call	_assert
	jmp	.L84
.L110:
	movl	$240000, %r10d
	xorl	%r9d, %r9d
	jmp	.L88
.L111:
	movl	$239999, %r10d
	movl	$1, %r9d
	jmp	.L88
.L172:
	xorl	%eax, %eax
	movq	%rbx, %rdx
	.p2align 4,,10
.L83:
	movss	(%r8,%rax), %xmm0
	movss	%xmm0, (%rdx,%rax)
	addq	$4, %rax
	cmpq	$960000, %rax
	jne	.L83
	jmp	.L87
.L112:
	movl	$239998, %r10d
	movl	$2, %r9d
	jmp	.L88
.L107:
	movq	40(%rsp), %rcx
	movl	$240000, %eax
	movq	%rcx, %rbx
	jmp	.L69
.L106:
	movl	$239998, %ecx
	movl	$2, %eax
	jmp	.L65
.L105:
	movl	$239999, %ecx
	movl	$1, %eax
	jmp	.L65
.L104:
	movl	$240000, %ecx
	xorl	%eax, %eax
	jmp	.L65
.L113:
	movq	144(%rsp), %rax
	cmpl	$7, 136(%rsp)
	movq	%rax, 56(%rsp)
	ja	.L122
	movl	136(%rsp), %eax
	leaq	.L123(%rip), %rdx
	movslq	(%rdx,%rax,4), %rax
	addq	%rdx, %rax
	jmp	*%rax
	.section .rdata,"dr"
	.align 4
.L123:
	.long	.L78-.L123
	.long	.L115-.L123
	.long	.L103-.L123
	.long	.L171-.L123
	.long	.L171-.L123
	.long	.L171-.L123
	.long	.L171-.L123
	.long	.L171-.L123
	.text
.L122:
	ud2
.L115:
	movq	304(%rsp), %rcx
	testq	%rcx, %rcx
	je	.L100
	call	_ZdlPv
.L100:
	movq	$0, 40(%rsp)
.L171:
	movq	$0, 48(%rsp)
.L78:
	movq	256(%rsp), %rcx
	call	free
	movq	40(%rsp), %rax
	testq	%rax, %rax
	je	.L101
	movq	%rax, %rcx
	call	_ZdlPv
.L101:
	cmpq	$0, 48(%rsp)
	je	.L102
.L103:
	movq	48(%rsp), %rcx
	movl	$4, %edx
	call	_ZdlPvy
.L102:
	movl	$-1, 136(%rsp)
	movq	56(%rsp), %rcx
	call	_Unwind_SjLj_Resume
	nop
	.seh_handler	__gxx_personality_sj0, @unwind, @except
	.seh_handlerdata
.LLSDA8497:
	.byte	0xff
	.byte	0xff
	.byte	0x1
	.uleb128 .LLSDACSE8497-.LLSDACSB8497
.LLSDACSB8497:
	.uleb128 0
	.uleb128 0
	.uleb128 0x1
	.uleb128 0
	.uleb128 0x2
	.uleb128 0
	.uleb128 0x3
	.uleb128 0
	.uleb128 0x4
	.uleb128 0
	.uleb128 0x5
	.uleb128 0
	.uleb128 0x6
	.uleb128 0
	.uleb128 0x7
	.uleb128 0
.LLSDACSE8497:
	.text
	.seh_endproc
	.section .rdata,"dr"
.LC6:
	.ascii "Int Constructor\12\0"
.LC7:
	.ascii "Destructor\12\0"
	.text
	.p2align 4,,15
	.globl	_Z8test_newv
	.def	_Z8test_newv;	.scl	2;	.type	32;	.endef
	.seh_proc	_Z8test_newv
_Z8test_newv:
	pushq	%rbp
	.seh_pushreg	%rbp
	subq	$208, %rsp
	.seh_stackalloc	208
	.seh_endprologue
	leaq	__gxx_personality_sj0(%rip), %rax
	movq	%rax, 128(%rsp)
	leaq	.LLSDA8512(%rip), %rax
	movq	%rax, 136(%rsp)
	leaq	208(%rsp), %rax
	movq	%rax, 144(%rsp)
	leaq	.L189(%rip), %rax
	movq	%rax, 152(%rsp)
	leaq	80(%rsp), %rax
	movq	%rax, %rcx
	movq	%rax, 184(%rsp)
	movq	%rsp, 160(%rsp)
	call	_Unwind_SjLj_Register
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movl	$-1, 88(%rsp)
	movabsq	$8000000000, %rcx
	movq	%rax, 40(%rsp)
	call	_Znay
	xorl	%edx, %edx
	movabsq	$8000000000, %r8
	movq	%rax, %rcx
	movq	%rax, 32(%rsp)
	call	memset
	call	_ZNSt6chrono3_V212system_clock3nowEv
	movabsq	$4835703278458516699, %rdx
	movq	%rax, %rcx
	subq	40(%rsp), %rcx
	movq	%rcx, %rax
	sarq	$63, %rcx
	imulq	%rdx
	sarq	$18, %rdx
	subq	%rcx, %rdx
	movq	.refptr._ZSt4cout(%rip), %rcx
	call	_ZNSo9_M_insertIxEERSoT_
	leaq	194(%rsp), %rdx
	movl	$1, %r8d
	movb	$10, 194(%rsp)
	movq	%rax, %rcx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	32(%rsp), %rax
	movq	.refptr._ZSt4cout(%rip), %rcx
	movsd	24(%rax), %xmm1
	call	_ZNSo9_M_insertIdEERSoT_
	leaq	195(%rsp), %rdx
	movl	$1, %r8d
	movb	$10, 195(%rsp)
	movq	%rax, %rcx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	32(%rsp), %rcx
	call	_ZdaPv
	movl	$12, %ecx
	call	_Znay
	movl	$0, 32(%rsp)
	movq	%rax, 72(%rsp)
	movq	%rax, 40(%rsp)
.L175:
	cmpq	$0, 40(%rsp)
	je	.L174
	movq	.refptr._ZSt4cout(%rip), %rcx
	movl	$16, %r8d
	movl	$-1, 88(%rsp)
	leaq	.LC6(%rip), %rdx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	40(%rsp), %rax
	movl	32(%rsp), %ecx
	movl	%ecx, (%rax)
.L174:
	addl	$1, 32(%rsp)
	movl	32(%rsp), %eax
	addq	$4, 40(%rsp)
	cmpl	$3, %eax
	jne	.L175
	movq	.refptr._ZSt4cout(%rip), %rax
	movl	$3, 32(%rsp)
	movq	%rax, 40(%rsp)
.L176:
	movq	40(%rsp), %rcx
	movl	$11, %r8d
	movl	$0, 88(%rsp)
	leaq	.LC7(%rip), %rdx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	subl	$1, 32(%rsp)
	jne	.L176
	movq	72(%rsp), %rcx
	call	_ZdaPv
	movl	$4, %ecx
	movl	$5, 88(%rsp)
	call	_Znwy
	movq	.refptr._ZSt4cout(%rip), %rcx
	movl	$16, %r8d
	movq	%rax, 48(%rsp)
	leaq	.LC6(%rip), %rdx
	movl	$2, 88(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	48(%rsp), %rax
	movl	$4, %ecx
	movl	$1, 196(%rsp)
	movl	$1, (%rax)
	movl	$6, 88(%rsp)
	call	_Znwy
	movq	.refptr._ZSt4cout(%rip), %rcx
	movl	$16, %r8d
	movq	%rax, 56(%rsp)
	leaq	.LC6(%rip), %rdx
	movl	$3, 88(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	56(%rsp), %rax
	movl	$4, %ecx
	movl	$1, 200(%rsp)
	movl	$1, (%rax)
	movl	$1, 88(%rsp)
	call	_Znwy
	movq	.refptr._ZSt4cout(%rip), %rcx
	movl	$16, %r8d
	movq	%rax, 64(%rsp)
	leaq	.LC6(%rip), %rdx
	movl	$4, 88(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	64(%rsp), %rax
	movl	$11, %r8d
	movq	.refptr._ZSt4cout(%rip), %rcx
	leaq	.LC7(%rip), %rdx
	movl	$1, (%rax)
	movl	$0, 88(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	.refptr._ZSt4cout(%rip), %rcx
	movl	$11, %r8d
	leaq	.LC7(%rip), %rdx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	.refptr._ZSt4cout(%rip), %rcx
	movl	$11, %r8d
	leaq	.LC7(%rip), %rdx
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	movq	184(%rsp), %rcx
	call	_Unwind_SjLj_Unregister
	nop
	addq	$208, %rsp
	popq	%rbp
	ret
.L189:
	movq	96(%rsp), %rax
	cmpl	$5, 88(%rsp)
	movq	%rax, 40(%rsp)
	ja	.L196
	movl	88(%rsp), %eax
	leaq	.L197(%rip), %rdx
	movslq	(%rdx,%rax,4), %rax
	addq	%rdx, %rax
	jmp	*%rax
	.section .rdata,"dr"
	.align 4
.L197:
	.long	.L209-.L197
	.long	.L191-.L197
	.long	.L192-.L197
	.long	.L193-.L197
	.long	.L207-.L197
	.long	.L208-.L197
	.text
.L196:
	ud2
.L193:
	movq	64(%rsp), %rcx
	movl	$4, %edx
	call	_ZdlPvy
.L209:
	xorl	%eax, %eax
.L179:
	leaq	196(%rsp), %rcx
	movl	$2, %edx
	subq	%rax, %rdx
	movq	%rcx, 48(%rsp)
	leaq	(%rcx,%rdx,4), %rax
	movq	%rax, 32(%rsp)
.L188:
	movq	32(%rsp), %rax
	movq	48(%rsp), %rdx
	cmpq	%rdx, %rax
	je	.L187
	movq	.refptr._ZSt4cout(%rip), %rcx
	subq	$4, %rax
	movl	$11, %r8d
	movl	$0, 88(%rsp)
	leaq	.LC7(%rip), %rdx
	movq	%rax, 32(%rsp)
	call	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x
	jmp	.L188
.L191:
	movq	48(%rsp), %rcx
	movl	$4, %edx
	call	_ZdlPvy
.L207:
	movl	$2, %eax
	jmp	.L179
.L192:
	movq	56(%rsp), %rcx
	movl	$4, %edx
	call	_ZdlPvy
.L208:
	movl	$1, %eax
	jmp	.L179
.L187:
	movq	40(%rsp), %rcx
	movl	$-1, 88(%rsp)
	call	_Unwind_SjLj_Resume
	nop
	.seh_handler	__gxx_personality_sj0, @unwind, @except
	.seh_handlerdata
.LLSDA8512:
	.byte	0xff
	.byte	0xff
	.byte	0x1
	.uleb128 .LLSDACSE8512-.LLSDACSB8512
.LLSDACSB8512:
	.uleb128 0
	.uleb128 0
	.uleb128 0x1
	.uleb128 0
	.uleb128 0x2
	.uleb128 0
	.uleb128 0x3
	.uleb128 0
	.uleb128 0x4
	.uleb128 0
	.uleb128 0x5
	.uleb128 0
.LLSDACSE8512:
	.text
	.seh_endproc
	.def	__main;	.scl	2;	.type	32;	.endef
	.section	.text.startup,"x"
	.p2align 4,,15
	.globl	main
	.def	main;	.scl	2;	.type	32;	.endef
	.seh_proc	main
main:
	subq	$40, %rsp
	.seh_stackalloc	40
	.seh_endprologue
	call	__main
	call	_Z11test_memcpyv
	movq	.refptr._ZSt3cin(%rip), %rcx
	call	_ZNSi3getEv
	xorl	%eax, %eax
	addq	$40, %rsp
	ret
	.seh_endproc
	.p2align 4,,15
	.def	_GLOBAL__sub_I__Z10initializePf;	.scl	3;	.type	32;	.endef
	.seh_proc	_GLOBAL__sub_I__Z10initializePf
_GLOBAL__sub_I__Z10initializePf:
	subq	$40, %rsp
	.seh_stackalloc	40
	.seh_endprologue
	leaq	_ZStL8__ioinit(%rip), %rcx
	call	_ZNSt8ios_base4InitC1Ev
	leaq	__tcf_0(%rip), %rcx
	addq	$40, %rsp
	jmp	atexit
	.seh_endproc
	.section	.ctors,"w"
	.align 8
	.quad	_GLOBAL__sub_I__Z10initializePf
.lcomm _ZStL8__ioinit,1,1
	.section .rdata,"dr"
	.align 4
.LC0:
	.long	1065353216
	.align 16
.LC1:
	.long	1065353216
	.long	1065353216
	.long	1065353216
	.long	1065353216
	.align 4
.LC4:
	.long	1073741824
	.align 16
.LC5:
	.long	1073741824
	.long	1073741824
	.long	1073741824
	.long	1073741824
	.ident	"GCC: (x86_64-posix-sjlj-rev0, Built by MinGW-W64 project) 7.3.0"
	.def	_ZNSt8ios_base4InitD1Ev;	.scl	2;	.type	32;	.endef
	.def	_ZNSt6chrono3_V212system_clock3nowEv;	.scl	2;	.type	32;	.endef
	.def	_ZNSo9_M_insertIxEERSoT_;	.scl	2;	.type	32;	.endef
	.def	_ZSt16__ostream_insertIcSt11char_traitsIcEERSt13basic_ostreamIT_T0_ES6_PKS3_x;	.scl	2;	.type	32;	.endef
	.def	_assert;	.scl	2;	.type	32;	.endef
	.def	_Znay;	.scl	2;	.type	32;	.endef
	.def	_Znwy;	.scl	2;	.type	32;	.endef
	.def	memcpy;	.scl	2;	.type	32;	.endef
	.def	_ZdlPv;	.scl	2;	.type	32;	.endef
	.def	_ZdlPvy;	.scl	2;	.type	32;	.endef
	.def	free;	.scl	2;	.type	32;	.endef
	.def	_Unwind_SjLj_Resume;	.scl	2;	.type	32;	.endef
	.def	memset;	.scl	2;	.type	32;	.endef
	.def	_ZNSo9_M_insertIdEERSoT_;	.scl	2;	.type	32;	.endef
	.def	_ZdaPv;	.scl	2;	.type	32;	.endef
	.def	_ZNSi3getEv;	.scl	2;	.type	32;	.endef
	.def	_ZNSt8ios_base4InitC1Ev;	.scl	2;	.type	32;	.endef
	.def	atexit;	.scl	2;	.type	32;	.endef
	.section	.rdata$.refptr._ZSt3cin, "dr"
	.globl	.refptr._ZSt3cin
	.linkonce	discard
.refptr._ZSt3cin:
	.quad	_ZSt3cin
	.section	.rdata$.refptr._ZSt4cout, "dr"
	.globl	.refptr._ZSt4cout
	.linkonce	discard
.refptr._ZSt4cout:
	.quad	_ZSt4cout
