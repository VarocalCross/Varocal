
package runtime
var _ unsafe.Pointer
type lock struct {
	key	uint
	// (union)	waitm	*m
}
type note struct {
	key	uint
	// (union)	waitm	*m
}
type _string struct {
	str	*uint8
	len	int
}
type iface struct {
	tab	*itab
	data	unsafe.Pointer
}
type eface struct {
	_type	*_type
	data	unsafe.Pointer
}
type _complex64 struct {
	real	float32
	imag	float32
}
type _complex128 struct {
	real	float64
	imag	float64
}
type slice struct {
	array	*uint8
	len	uint
	cap	uint
}
type gobuf struct {
	sp	*uint8
	pc	*uint8
	g	*g
}
type g struct {
	stackguard	*uint8
	stackbase	*uint8
	_defer	*_defer
	_panic	*_panic
	sched	gobuf
	gcstack	*uint8
	gcsp	*uint8
	gcguard	*uint8
	stack0	*uint8
	entry	*uint8
	alllink	*g
	param	unsafe.Pointer
	status	int16
	goid	int
	selgen	uint
	waitreason	*int8
	schedlink	*g
	readyonstop	uint8
	ispanic	uint8
	m	*m
	lockedm	*m
	idlem	*m
	sig	int
	writenbuf	int
	writebuf	*uint8
	sigcode0	uint
	sigcode1	uint
	sigpc	uint
	gopc	uint
	end	[0]uint
}
type m struct {
	g0	*g
	morepc	func()
	moreargp	unsafe.Pointer
	morebuf	gobuf
	moreframesize	uint
	moreargsize	uint
	cret	uint
	procid	uint64
	gsignal	*g
	tls	[8]uint
	curg	*g
	id	int
	mallocing	int
	gcing	int
	locks	int
	nomemprof	int
	waitnextg	int
	dying	int
	profilehz	int
	helpgc	int
	fastrand	uint
	ncgocall	uint64
	havenextg	note
	nextg	*g
	alllink	*m
	schedlink	*m
	machport	uint
	mcache	*mcache
	stackalloc	*fixalloc
	lockedg	*g
	idleg	*g
	createstack	[32]uint
	freglo	[16]uint
	freghi	[16]uint
	fflag	uint
	nextwaitm	*m
	waitsema	uint
	waitsemacount	uint
	waitsemalock	uint
	end	[0]uint
}
type stktop struct {
	stackguard	*uint8
	stackbase	*uint8
	gobuf	gobuf
	argsize	uint
	argp	*uint8
	free	uint
	_panic	uint8
}
type sigtab struct {
	flags	int
	name	*int8
}
type _func struct {
	name	string
	_type	string
	src	string
	pcln	[]byte
	entry	uint
	pc0	uint
	ln0	int
	frame	int
	args	int
	locals	int
}
type wincall struct {
	fn	func(unsafe.Pointer)
	n	uint
	args	unsafe.Pointer
	r1	uint
	r2	uint
	err	uint
}
type timers struct {
	lock
	timerproc	*g
	sleeping	uint8
	rescheduling	uint8
	waitnote	note
	t	**timer
	len	int
	cap	int
}
type timer struct {
	i	int
	when	int64
	period	int64
	f	func(int64, eface)
	arg	eface
}
type alg struct {
	hash	func(*uint, uint, unsafe.Pointer)
	equal	func(*uint8, uint, unsafe.Pointer, unsafe.Pointer)
	print	func(uint, unsafe.Pointer)
	copy	func(uint, unsafe.Pointer, unsafe.Pointer)
}
var algarray	[22]alg
type _defer struct {
	siz	int
	nofree	uint8
	argp	*uint8
	pc	*uint8
	fn	*uint8
	link	*_defer
	args	[8]uint8
}
type _panic struct {
	arg	eface
	stackbase	*uint8
	link	*_panic
	recovered	uint8
}
var emptystring	string
var allg	*g
var lastg	*g
var allm	*m
var gomaxprocs	int
var singleproc	uint8
var panicking	uint
var gcwaiting	int
var goos	*int8
var ncpu	int
var iscgo	uint8
var worldsema	uint
type machbody struct {
	msgh_descriptor_count	uint
}
type machheader struct {
	msgh_bits	uint
	msgh_size	uint
	msgh_remote_port	uint
	msgh_local_port	uint
	msgh_reserved	uint
	msgh_id	int
}
type machndr struct {
	mig_vers	uint8
	if_vers	uint8
	reserved1	uint8
	mig_encoding	uint8
	int_rep	uint8
	char_rep	uint8
	float_rep	uint8
	reserved2	uint8
}
type machport struct {
	name	uint
	pad1	uint
	pad2	uint16
	disposition	uint8
	_type	uint8
}
type stackt struct {
	ss_sp	*uint8
	ss_size	uint
	ss_flags	int
}
type sigaction struct {
	__sigaction_u	[4]uint8
	sa_tramp	unsafe.Pointer
	sa_mask	uint
	sa_flags	int
}
type siginfo struct {
	si_signo	int
	si_errno	int
	si_code	int
	si_pid	int
	si_uid	uint
	si_status	int
	si_addr	*uint8
	si_value	[4]uint8
	si_band	int
	__pad	[7]uint
}
type timeval struct {
	tv_sec	int
	tv_usec	int
}
type itimerval struct {
	it_interval	timeval
	it_value	timeval
}
type fpcontrol struct {
	pad_cgo_0	[2]uint8
}
type fpstatus struct {
	pad_cgo_0	[2]uint8
}
type regmmst struct {
	mmst_reg	[10]int8
	mmst_rsrv	[6]int8
}
type regxmm struct {
	xmm_reg	[16]int8
}
type regs64 struct {
	rax	uint64
	rbx	uint64
	rcx	uint64
	rdx	uint64
	rdi	uint64
	rsi	uint64
	rbp	uint64
	rsp	uint64
	r8	uint64
	r9	uint64
	r10	uint64
	r11	uint64
	r12	uint64
	r13	uint64
	r14	uint64
	r15	uint64
	rip	uint64
	rflags	uint64
	cs	uint64
	fs	uint64
	gs	uint64
}
type floatstate64 struct {
	fpu_reserved	[2]int
	fpu_fcw	fpcontrol
	fpu_fsw	fpstatus
	fpu_ftw	uint8
	fpu_rsrv1	uint8
	fpu_fop	uint16
	fpu_ip	uint
	fpu_cs	uint16
	fpu_rsrv2	uint16
	fpu_dp	uint
	fpu_ds	uint16
	fpu_rsrv3	uint16
	fpu_mxcsr	uint
	fpu_mxcsrmask	uint
	fpu_stmm0	regmmst
	fpu_stmm1	regmmst
	fpu_stmm2	regmmst
	fpu_stmm3	regmmst
	fpu_stmm4	regmmst
	fpu_stmm5	regmmst
	fpu_stmm6	regmmst
	fpu_stmm7	regmmst
	fpu_xmm0	regxmm
	fpu_xmm1	regxmm
	fpu_xmm2	regxmm
	fpu_xmm3	regxmm
	fpu_xmm4	regxmm
	fpu_xmm5	regxmm
	fpu_xmm6	regxmm
	fpu_xmm7	regxmm
	fpu_xmm8	regxmm
	fpu_xmm9	regxmm
	fpu_xmm10	regxmm
	fpu_xmm11	regxmm
	fpu_xmm12	regxmm
	fpu_xmm13	regxmm
	fpu_xmm14	regxmm
	fpu_xmm15	regxmm
	fpu_rsrv4	[96]int8
	fpu_reserved1	int
}
type exceptionstate64 struct {
	trapno	uint16
	cpu	uint16
	err	uint
	faultvaddr	uint64
}
type mcontext64 struct {
	es	exceptionstate64
	ss	regs64
	fs	floatstate64
}
type regs32 struct {
	eax	uint
	ebx	uint
	ecx	uint
	edx	uint
	edi	uint
	esi	uint
	ebp	uint
	esp	uint
	ss	uint
	eflags	uint
	eip	uint
	cs	uint
	ds	uint
	es	uint
	fs	uint
	gs	uint
}
type floatstate32 struct {
	fpu_reserved	[2]int
	fpu_fcw	fpcontrol
	fpu_fsw	fpstatus
	fpu_ftw	uint8
	fpu_rsrv1	uint8
	fpu_fop	uint16
	fpu_ip	uint
	fpu_cs	uint16
	fpu_rsrv2	uint16
	fpu_dp	uint
	fpu_ds	uint16
	fpu_rsrv3	uint16
	fpu_mxcsr	uint
	fpu_mxcsrmask	uint
	fpu_stmm0	regmmst
	fpu_stmm1	regmmst
	fpu_stmm2	regmmst
	fpu_stmm3	regmmst
	fpu_stmm4	regmmst
	fpu_stmm5	regmmst
	fpu_stmm6	regmmst
	fpu_stmm7	regmmst
	fpu_xmm0	regxmm
	fpu_xmm1	regxmm
	fpu_xmm2	regxmm
	fpu_xmm3	regxmm
	fpu_xmm4	regxmm
	fpu_xmm5	regxmm
	fpu_xmm6	regxmm
	fpu_xmm7	regxmm
	fpu_rsrv4	[224]int8
	fpu_reserved1	int
}
type exceptionstate32 struct {
	trapno	uint16
	cpu	uint16
	err	uint
	faultvaddr	uint
}
type mcontext32 struct {
	es	exceptionstate32
	ss	regs32
	fs	floatstate32
}
type ucontext struct {
	uc_onstack	int
	uc_sigmask	uint
	uc_stack	stackt
	uc_link	*ucontext
	uc_mcsize	uint
	uc_mcontext	*mcontext32
}
type mlink struct {
	next	*mlink
}
type fixalloc struct {
	size	uint
	alloc	func(uint) unsafe.Pointer
	first	func(unsafe.Pointer, *uint8)
	arg	unsafe.Pointer
	list	*mlink
	chunk	*uint8
	nchunk	uint
	inuse	uint
	sys	uint
}
type _1_ struct {
	size	uint
	nmalloc	uint64
	nfree	uint64
}
type mstats struct {
	alloc	uint64
	total_alloc	uint64
	sys	uint64
	nlookup	uint64
	nmalloc	uint64
	nfree	uint64
	heap_alloc	uint64
	heap_sys	uint64
	heap_idle	uint64
	heap_inuse	uint64
	heap_released	uint64
	heap_objects	uint64
	stacks_inuse	uint64
	stacks_sys	uint64
	mspan_inuse	uint64
	mspan_sys	uint64
	mcache_inuse	uint64
	mcache_sys	uint64
	buckhash_sys	uint64
	next_gc	uint64
	last_gc	uint64
	pause_total_ns	uint64
	pause_ns	[256]uint64
	numgc	uint
	enablegc	uint8
	debuggc	uint8
	by_size	[61]_1_
}
var memstats	mstats
var class_to_size	[61]int
var class_to_allocnpages	[61]int
var class_to_transfercount	[61]int
type mcachelist struct {
	list	*mlink
	nlist	uint
	nlistmin	uint
}
type _2_ struct {
	nmalloc	int64
	nfree	int64
}
type mcache struct {
	list	[61]mcachelist
	size	uint64
	local_cachealloc	int64
	local_objects	int64
	local_alloc	int64
	local_total_alloc	int64
	local_nmalloc	int64
	local_nfree	int64
	local_nlookup	int64
	next_sample	int
	local_by_size	[61]_2_
}
type mspan struct {
	next	*mspan
	prev	*mspan
	allnext	*mspan
	start	uint
	npages	uint
	freelist	*mlink
	ref	uint
	sizeclass	uint
	state	uint
	unusedsince	int64
	npreleased	uint
	limit	*uint8
}
type mcentral struct {
	lock
	sizeclass	int
	nonempty	mspan
	empty	mspan
	nfree	int
}
type _3_ struct {
	mcentral
	// (union)	pad	[64]uint8
}
type mheap struct {
	lock
	free	[256]mspan
	large	mspan
	allspans	*mspan
	_map	[1048576]*mspan
	bitmap	*uint8
	bitmap_mapped	uint
	arena_start	*uint8
	arena_used	*uint8
	arena_end	*uint8
	central	[61]_3_
	spanalloc	fixalloc
	cachealloc	fixalloc
}
var checking	int
var m0	m
var g0	g
var debug	int
type sched struct {
	lock
	gfree	*g
	goidgen	int
	ghead	*g
	gtail	*g
	gwait	int
	gcount	int
	grunning	int
	mhead	*m
	mwait	int
	mcount	int
	atomic	uint
	profilehz	int
	init	uint8
	lockmain	uint8
	stopped	note
}
var mwakeup	*m
var scvg	*g
var libcgo_thread_start	func(unsafe.Pointer)
type cgothreadstart struct {
	m	*m
	g	*g
	fn	func()
}
type _4_ struct {
	lock
	fn	func(*uint, int)
	hz	int
	pcbuf	[100]uint
}
var prof	_4_
var libcgo_setenv	func(**uint8)
type commontype struct {
	size	uint
	hash	uint
	_unused	uint8
	align	uint8
	fieldalign	uint8
	kind	uint8
	alg	*alg
	_string	*string
	x	*uncommontype
	ptrto	*_type
}
type method struct {
	name	*string
	pkgpath	*string
	mtyp	*_type
	typ	*_type
	ifn	func()
	tfn	func()
}
type uncommontype struct {
	name	*string
	pkgpath	*string
	mhdr	[]byte
	m	[0]method
}
type _type struct {
	_type	unsafe.Pointer
	ptr	unsafe.Pointer
	commontype
}
type imethod struct {
	name	*string
	pkgpath	*string
	_type	*_type
}
type interfacetype struct {
	_type
	mhdr	[]byte
	m	[0]imethod
}
type maptype struct {
	_type
	key	*_type
	elem	*_type
}
type chantype struct {
	_type
	elem	*_type
	dir	uint
}
type slicetype struct {
	_type
	elem	*_type
}
type functype struct {
	_type
	dotdotdot	uint8
	in	[]byte
	out	[]byte
}
type itab struct {
	inter	*interfacetype
	_type	*_type
	link	*itab
	bad	int
	unused	int
	fun	[0]func()
}
var hash	[1009]*itab
var ifacelock	lock
type hash_iter_sub struct {
	e	*hash_entry
	start	*hash_entry
	last	*hash_entry
}
type hash_iter struct {
	data	*uint8
	elemsize	int
	changes	int
	i	int
	cycled	uint8
	last_hash	uint
	cycle	uint
	h	*hmap
	t	*maptype
	subtable_state	[4]hash_iter_sub
}
type hmap struct {
	count	uint
	datasize	uint8
	max_power	uint8
	indirectval	uint8
	valoff	uint8
	changes	int
	hash0	uint
	st	*hash_subtable
}
type hash_entry struct {
	hash	uint
	data	[1]uint8
}
type hash_subtable struct {
	power	uint8
	used	uint8
	datasize	uint8
	max_probes	uint8
	limit_bytes	int16
	last	*hash_entry
	entry	[1]hash_entry
}
type sudog struct {
	g	*g
	selgen	uint
	link	*sudog
	elem	*uint8
}
type waitq struct {
	first	*sudog
	last	*sudog
}
type hchan struct {
	qcount	uint
	dataqsiz	uint
	elemsize	uint16
	closed	uint8
	elemalign	uint8
	elemalg	*alg
	sendx	uint
	recvx	uint
	recvq	waitq
	sendq	waitq
	lock
}
type scase struct {
	sg	sudog
	_chan	*hchan
	pc	*uint8
	kind	uint16
	so	uint16
	receivedp	*uint8
}
type _select struct {
	tcase	uint16
	ncase	uint16
	pollorder	*uint16
	lockorder	**hchan
	scase	[1]scase
}