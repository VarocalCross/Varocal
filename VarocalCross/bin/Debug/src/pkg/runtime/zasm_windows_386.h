// auto generated by go tool dist

#define	get_tls(r)	MOVL 0x14(FS), r
#define	g(r)	0(r)
#define	m(r)	4(r)
#define gobuf_sp 0
#define gobuf_pc 4
#define gobuf_g 8
#define g_stackguard 0
#define g_stackbase 4
#define g_defer 8
#define g_panic 12
#define g_sched 16
#define g_gcstack 28
#define g_gcsp 32
#define g_gcguard 36
#define g_stack0 40
#define g_entry 44
#define g_alllink 48
#define g_param 52
#define g_status 56
#define g_goid 60
#define g_selgen 64
#define g_waitreason 68
#define g_schedlink 72
#define g_readyonstop 76
#define g_ispanic 77
#define g_m 80
#define g_lockedm 84
#define g_idlem 88
#define g_sig 92
#define g_writenbuf 96
#define g_writebuf 100
#define g_sigcode0 104
#define g_sigcode1 108
#define g_sigpc 112
#define g_gopc 116
#define g_end 120
#define m_g0 0
#define m_morepc 4
#define m_moreargp 8
#define m_morebuf 12
#define m_moreframesize 24
#define m_moreargsize 28
#define m_cret 32
#define m_procid 36
#define m_gsignal 44
#define m_tls 48
#define m_curg 80
#define m_id 84
#define m_mallocing 88
#define m_gcing 92
#define m_locks 96
#define m_nomemprof 100
#define m_waitnextg 104
#define m_dying 108
#define m_profilehz 112
#define m_helpgc 116
#define m_fastrand 120
#define m_ncgocall 124
#define m_havenextg 132
#define m_nextg 136
#define m_alllink 140
#define m_schedlink 144
#define m_machport 148
#define m_mcache 152
#define m_stackalloc 156
#define m_lockedg 160
#define m_idleg 164
#define m_createstack 168
#define m_freglo 296
#define m_freghi 360
#define m_fflag 424
#define m_nextwaitm 428
#define m_waitsema 432
#define m_waitsemacount 436
#define m_waitsemalock 440
#define m_thread 444
#define m_end 448
#define wincall_fn 0
#define wincall_n 4
#define wincall_args 8
#define wincall_r1 12
#define wincall_r2 16
#define wincall_err 20
