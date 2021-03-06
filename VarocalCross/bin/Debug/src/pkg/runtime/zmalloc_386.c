// auto generated by go tool dist
// goos=darwin goarch=386

#include "runtime.h"
#include "arch_GOARCH.h"
#include "stack.h"
#include "malloc.h"
#include "defs_GOOS_GOARCH.h"
#include "type.h"
#pragma dataflag 16 /* mark mheap as 'no pointers', hiding from garbage collector */

#line 2062 "C:\Go\src\pkg\runtime\malloc.goc"
MHeap runtime·mheap; 
#line 2064 "C:\Go\src\pkg\runtime\malloc.goc"
extern MStats mstats; 
#line 2066 "C:\Go\src\pkg\runtime\malloc.goc"
extern volatile int32 runtime·MemProfileRate; 
#line 2071 "C:\Go\src\pkg\runtime\malloc.goc"
void* 
runtime·mallocgc ( uintptr size , uint32 flag , int32 dogc , int32 zeroed ) 
{ 
int32 sizeclass , rate; 
MCache *c; 
uintptr npages; 
MSpan *s; 
void *v; 
#line 2080 "C:\Go\src\pkg\runtime\malloc.goc"
if ( runtime·gcwaiting && g != m->g0 && m->locks == 0 ) 
runtime·gosched ( ) ; 
if ( m->mallocing ) 
runtime·throw ( "malloc/free - deadlock" ) ; 
m->mallocing = 1; 
if ( size == 0 ) 
size = 1; 
#line 2088 "C:\Go\src\pkg\runtime\malloc.goc"
c = m->mcache; 
c->local_nmalloc++; 
if ( size <= MaxSmallSize ) { 
#line 2092 "C:\Go\src\pkg\runtime\malloc.goc"
sizeclass = runtime·SizeToClass ( size ) ; 
size = runtime·class_to_size[sizeclass]; 
v = runtime·MCache_Alloc ( c , sizeclass , size , zeroed ) ; 
if ( v == nil ) 
runtime·throw ( "out of memory" ) ; 
c->local_alloc += size; 
c->local_total_alloc += size; 
c->local_by_size[sizeclass].nmalloc++; 
} else { 
#line 2104 "C:\Go\src\pkg\runtime\malloc.goc"
npages = size >> PageShift; 
if ( ( size & PageMask ) != 0 ) 
npages++; 
s = runtime·MHeap_Alloc ( &runtime·mheap , npages , 0 , 1 ) ; 
if ( s == nil ) 
runtime·throw ( "out of memory" ) ; 
size = npages<<PageShift; 
c->local_alloc += size; 
c->local_total_alloc += size; 
v = ( void* ) ( s->start << PageShift ) ; 
#line 2116 "C:\Go\src\pkg\runtime\malloc.goc"
runtime·markspan ( v , 0 , 0 , true ) ; 
} 
if ( ! ( flag & FlagNoGC ) ) 
runtime·markallocated ( v , size , ( flag&FlagNoPointers ) != 0 ) ; 
#line 2121 "C:\Go\src\pkg\runtime\malloc.goc"
m->mallocing = 0; 
#line 2123 "C:\Go\src\pkg\runtime\malloc.goc"
if ( ! ( flag & FlagNoProfiling ) && ( rate = runtime·MemProfileRate ) > 0 ) { 
if ( size >= rate ) 
goto profile; 
if ( m->mcache->next_sample > size ) 
m->mcache->next_sample -= size; 
else { 
#line 2131 "C:\Go\src\pkg\runtime\malloc.goc"
if ( rate > 0x3fffffff ) 
rate = 0x3fffffff; 
m->mcache->next_sample = runtime·fastrand1 ( ) % ( 2*rate ) ; 
profile: 
runtime·setblockspecial ( v , true ) ; 
runtime·MProf_Malloc ( v , size ) ; 
} 
} 
#line 2140 "C:\Go\src\pkg\runtime\malloc.goc"
if ( dogc && mstats.heap_alloc >= mstats.next_gc ) 
runtime·gc ( 0 ) ; 
return v; 
} 
#line 2145 "C:\Go\src\pkg\runtime\malloc.goc"
void* 
runtime·malloc ( uintptr size ) 
{ 
return runtime·mallocgc ( size , 0 , 0 , 1 ) ; 
} 
#line 2152 "C:\Go\src\pkg\runtime\malloc.goc"
void 
runtime·free ( void *v ) 
{ 
int32 sizeclass; 
MSpan *s; 
MCache *c; 
uint32 prof; 
uintptr size; 
#line 2161 "C:\Go\src\pkg\runtime\malloc.goc"
if ( v == nil ) 
return; 
#line 2167 "C:\Go\src\pkg\runtime\malloc.goc"
if ( m->mallocing ) 
runtime·throw ( "malloc/free - deadlock" ) ; 
m->mallocing = 1; 
#line 2171 "C:\Go\src\pkg\runtime\malloc.goc"
if ( !runtime·mlookup ( v , nil , nil , &s ) ) { 
runtime·printf ( "free %p: not an allocated block\n" , v ) ; 
runtime·throw ( "free runtime·mlookup" ) ; 
} 
prof = runtime·blockspecial ( v ) ; 
#line 2178 "C:\Go\src\pkg\runtime\malloc.goc"
sizeclass = s->sizeclass; 
c = m->mcache; 
if ( sizeclass == 0 ) { 
#line 2182 "C:\Go\src\pkg\runtime\malloc.goc"
size = s->npages<<PageShift; 
* ( uintptr* ) ( s->start<<PageShift ) = 1; 
#line 2186 "C:\Go\src\pkg\runtime\malloc.goc"
runtime·markfreed ( v , size ) ; 
runtime·unmarkspan ( v , 1<<PageShift ) ; 
runtime·MHeap_Free ( &runtime·mheap , s , 1 ) ; 
} else { 
#line 2191 "C:\Go\src\pkg\runtime\malloc.goc"
size = runtime·class_to_size[sizeclass]; 
if ( size > sizeof ( uintptr ) ) 
( ( uintptr* ) v ) [1] = 1; 
#line 2197 "C:\Go\src\pkg\runtime\malloc.goc"
runtime·markfreed ( v , size ) ; 
c->local_by_size[sizeclass].nfree++; 
runtime·MCache_Free ( c , v , sizeclass , size ) ; 
} 
c->local_alloc -= size; 
if ( prof ) 
runtime·MProf_Free ( v , size ) ; 
m->mallocing = 0; 
} 
#line 2207 "C:\Go\src\pkg\runtime\malloc.goc"
int32 
runtime·mlookup ( void *v , byte **base , uintptr *size , MSpan **sp ) 
{ 
uintptr n , i; 
byte *p; 
MSpan *s; 
#line 2214 "C:\Go\src\pkg\runtime\malloc.goc"
m->mcache->local_nlookup++; 
s = runtime·MHeap_LookupMaybe ( &runtime·mheap , v ) ; 
if ( sp ) 
*sp = s; 
if ( s == nil ) { 
runtime·checkfreed ( v , 1 ) ; 
if ( base ) 
*base = nil; 
if ( size ) 
*size = 0; 
return 0; 
} 
#line 2227 "C:\Go\src\pkg\runtime\malloc.goc"
p = ( byte* ) ( ( uintptr ) s->start<<PageShift ) ; 
if ( s->sizeclass == 0 ) { 
#line 2230 "C:\Go\src\pkg\runtime\malloc.goc"
if ( base ) 
*base = p; 
if ( size ) 
*size = s->npages<<PageShift; 
return 1; 
} 
#line 2237 "C:\Go\src\pkg\runtime\malloc.goc"
if ( ( byte* ) v >= ( byte* ) s->limit ) { 
#line 2239 "C:\Go\src\pkg\runtime\malloc.goc"
return 0; 
} 
#line 2242 "C:\Go\src\pkg\runtime\malloc.goc"
n = runtime·class_to_size[s->sizeclass]; 
if ( base ) { 
i = ( ( byte* ) v - p ) /n; 
*base = p + i*n; 
} 
if ( size ) 
*size = n; 
#line 2250 "C:\Go\src\pkg\runtime\malloc.goc"
return 1; 
} 
#line 2253 "C:\Go\src\pkg\runtime\malloc.goc"
MCache* 
runtime·allocmcache ( void ) 
{ 
int32 rate; 
MCache *c; 
#line 2259 "C:\Go\src\pkg\runtime\malloc.goc"
runtime·lock ( &runtime·mheap ) ; 
c = runtime·FixAlloc_Alloc ( &runtime·mheap.cachealloc ) ; 
mstats.mcache_inuse = runtime·mheap.cachealloc.inuse; 
mstats.mcache_sys = runtime·mheap.cachealloc.sys; 
runtime·unlock ( &runtime·mheap ) ; 
#line 2266 "C:\Go\src\pkg\runtime\malloc.goc"
rate = runtime·MemProfileRate; 
if ( rate > 0x3fffffff ) 
rate = 0x3fffffff; 
if ( rate != 0 ) 
c->next_sample = runtime·fastrand1 ( ) % ( 2*rate ) ; 
#line 2272 "C:\Go\src\pkg\runtime\malloc.goc"
return c; 
} 
#line 2275 "C:\Go\src\pkg\runtime\malloc.goc"
void 
runtime·purgecachedstats ( M* m ) 
{ 
MCache *c; 
#line 2281 "C:\Go\src\pkg\runtime\malloc.goc"
c = m->mcache; 
mstats.heap_alloc += c->local_cachealloc; 
c->local_cachealloc = 0; 
mstats.heap_objects += c->local_objects; 
c->local_objects = 0; 
mstats.nmalloc += c->local_nmalloc; 
c->local_nmalloc = 0; 
mstats.nfree += c->local_nfree; 
c->local_nfree = 0; 
mstats.nlookup += c->local_nlookup; 
c->local_nlookup = 0; 
mstats.alloc += c->local_alloc; 
c->local_alloc= 0; 
mstats.total_alloc += c->local_total_alloc; 
c->local_total_alloc= 0; 
} 
#line 2298 "C:\Go\src\pkg\runtime\malloc.goc"
uintptr runtime·sizeof_C_MStats = sizeof ( MStats ) ; 
#line 2300 "C:\Go\src\pkg\runtime\malloc.goc"
#define MaxArena32 ( 2U<<30 ) 
#line 2302 "C:\Go\src\pkg\runtime\malloc.goc"
void 
runtime·mallocinit ( void ) 
{ 
byte *p; 
uintptr arena_size , bitmap_size; 
extern byte end[]; 
byte *want; 
uintptr limit; 
#line 2311 "C:\Go\src\pkg\runtime\malloc.goc"
p = nil; 
arena_size = 0; 
bitmap_size = 0; 
#line 2316 "C:\Go\src\pkg\runtime\malloc.goc"
USED ( p ) ; 
USED ( arena_size ) ; 
USED ( bitmap_size ) ; 
#line 2320 "C:\Go\src\pkg\runtime\malloc.goc"
runtime·InitSizes ( ) ; 
#line 2322 "C:\Go\src\pkg\runtime\malloc.goc"
limit = runtime·memlimit ( ) ; 
#line 2327 "C:\Go\src\pkg\runtime\malloc.goc"
if ( sizeof ( void* ) == 8 && ( limit == 0 || limit > ( 1<<30 ) ) ) { 
#line 2352 "C:\Go\src\pkg\runtime\malloc.goc"
arena_size = 16LL<<30; 
bitmap_size = arena_size / ( sizeof ( void* ) *8/4 ) ; 
p = runtime·SysReserve ( ( void* ) ( 0x00f8ULL<<32 ) , bitmap_size + arena_size ) ; 
} 
if ( p == nil ) { 
#line 2374 "C:\Go\src\pkg\runtime\malloc.goc"
bitmap_size = MaxArena32 / ( sizeof ( void* ) *8/4 ) ; 
arena_size = 512<<20; 
if ( limit > 0 && arena_size+bitmap_size > limit ) { 
bitmap_size = ( limit / 9 ) & ~ ( ( 1<<PageShift ) - 1 ) ; 
arena_size = bitmap_size * 8; 
} 
#line 2390 "C:\Go\src\pkg\runtime\malloc.goc"
want = ( byte* ) ( ( ( uintptr ) end + ( 1<<18 ) + ( 1<<20 ) - 1 ) &~ ( ( 1<<20 ) -1 ) ) ; 
p = runtime·SysReserve ( want , bitmap_size + arena_size ) ; 
if ( p == nil ) 
runtime·throw ( "runtime: cannot reserve arena virtual address space" ) ; 
if ( ( uintptr ) p & ( ( ( uintptr ) 1<<PageShift ) -1 ) ) 
runtime·printf ( "runtime: SysReserve returned unaligned address %p; asked for %p" , p , bitmap_size+arena_size ) ; 
} 
if ( ( uintptr ) p & ( ( ( uintptr ) 1<<PageShift ) -1 ) ) 
runtime·throw ( "runtime: SysReserve returned unaligned address" ) ; 
#line 2400 "C:\Go\src\pkg\runtime\malloc.goc"
runtime·mheap.bitmap = p; 
runtime·mheap.arena_start = p + bitmap_size; 
runtime·mheap.arena_used = runtime·mheap.arena_start; 
runtime·mheap.arena_end = runtime·mheap.arena_start + arena_size; 
#line 2406 "C:\Go\src\pkg\runtime\malloc.goc"
runtime·MHeap_Init ( &runtime·mheap , runtime·SysAlloc ) ; 
m->mcache = runtime·allocmcache ( ) ; 
#line 2410 "C:\Go\src\pkg\runtime\malloc.goc"
runtime·free ( runtime·malloc ( 1 ) ) ; 
} 
#line 2413 "C:\Go\src\pkg\runtime\malloc.goc"
void* 
runtime·MHeap_SysAlloc ( MHeap *h , uintptr n ) 
{ 
byte *p; 
#line 2418 "C:\Go\src\pkg\runtime\malloc.goc"
if ( n > h->arena_end - h->arena_used ) { 
#line 2421 "C:\Go\src\pkg\runtime\malloc.goc"
byte *new_end; 
uintptr needed; 
#line 2424 "C:\Go\src\pkg\runtime\malloc.goc"
needed = ( uintptr ) h->arena_used + n - ( uintptr ) h->arena_end; 
#line 2426 "C:\Go\src\pkg\runtime\malloc.goc"
needed = ( needed + ( 256<<20 ) - 1 ) & ~ ( ( 256<<20 ) -1 ) ; 
new_end = h->arena_end + needed; 
if ( new_end <= h->arena_start + MaxArena32 ) { 
p = runtime·SysReserve ( h->arena_end , new_end - h->arena_end ) ; 
if ( p == h->arena_end ) 
h->arena_end = new_end; 
} 
} 
if ( n <= h->arena_end - h->arena_used ) { 
#line 2436 "C:\Go\src\pkg\runtime\malloc.goc"
p = h->arena_used; 
runtime·SysMap ( p , n ) ; 
h->arena_used += n; 
runtime·MHeap_MapBits ( h ) ; 
return p; 
} 
#line 2444 "C:\Go\src\pkg\runtime\malloc.goc"
if ( sizeof ( void* ) == 8 && ( uintptr ) h->bitmap >= 0xffffffffU ) 
return nil; 
#line 2450 "C:\Go\src\pkg\runtime\malloc.goc"
p = runtime·SysAlloc ( n ) ; 
if ( p == nil ) 
return nil; 
#line 2454 "C:\Go\src\pkg\runtime\malloc.goc"
if ( p < h->arena_start || p+n - h->arena_start >= MaxArena32 ) { 
runtime·printf ( "runtime: memory allocated by OS (%p) not in usable range [%p,%p)\n" , 
p , h->arena_start , h->arena_start+MaxArena32 ) ; 
runtime·SysFree ( p , n ) ; 
return nil; 
} 
#line 2461 "C:\Go\src\pkg\runtime\malloc.goc"
if ( p+n > h->arena_used ) { 
h->arena_used = p+n; 
if ( h->arena_used > h->arena_end ) 
h->arena_end = h->arena_used; 
runtime·MHeap_MapBits ( h ) ; 
} 
#line 2468 "C:\Go\src\pkg\runtime\malloc.goc"
return p; 
} 
#line 2473 "C:\Go\src\pkg\runtime\malloc.goc"
void* 
runtime·mal ( uintptr n ) 
{ 
return runtime·mallocgc ( n , 0 , 1 , 1 ) ; 
} 
void
runtime·new(Type* typ, uint8* ret)
{
#line 2479 "C:\Go\src\pkg\runtime\malloc.goc"

	uint32 flag = typ->kind&KindNoPointers ? FlagNoPointers : 0;
	ret = runtime·mallocgc(typ->size, flag, 1, 1);
	FLUSH(&ret);
	FLUSH(&ret);
}

#line 2485 "C:\Go\src\pkg\runtime\malloc.goc"
void* 
runtime·stackalloc ( uint32 n ) 
{ 
#line 2491 "C:\Go\src\pkg\runtime\malloc.goc"
if ( g != m->g0 ) 
runtime·throw ( "stackalloc not on scheduler stack" ) ; 
#line 2501 "C:\Go\src\pkg\runtime\malloc.goc"
if ( m->mallocing || m->gcing || n == FixedStack ) { 
if ( n != FixedStack ) { 
runtime·printf ( "stackalloc: in malloc, size=%d want %d" , FixedStack , n ) ; 
runtime·throw ( "stackalloc" ) ; 
} 
return runtime·FixAlloc_Alloc ( m->stackalloc ) ; 
} 
return runtime·mallocgc ( n , FlagNoProfiling|FlagNoGC , 0 , 0 ) ; 
} 
#line 2511 "C:\Go\src\pkg\runtime\malloc.goc"
void 
runtime·stackfree ( void *v , uintptr n ) 
{ 
if ( m->mallocing || m->gcing || n == FixedStack ) { 
runtime·FixAlloc_Free ( m->stackalloc , v ) ; 
return; 
} 
runtime·free ( v ) ; 
} 
void
runtime·GC()
{
#line 2521 "C:\Go\src\pkg\runtime\malloc.goc"

	runtime·gc(1);
}
void
runtime·SetFinalizer(Eface obj, Eface finalizer)
{
#line 2525 "C:\Go\src\pkg\runtime\malloc.goc"

	byte *base;
	uintptr size;
	FuncType *ft;
	int32 i, nret;
	Type *t;

	if(obj.type == nil) {
		runtime·printf("runtime.SetFinalizer: first argument is nil interface\n");
		goto throw;
	}
	if(obj.type->kind != KindPtr) {
		runtime·printf("runtime.SetFinalizer: first argument is %S, not pointer\n", *obj.type->string);
		goto throw;
	}
	if(!runtime·mlookup(obj.data, &base, &size, nil) || obj.data != base) {
		runtime·printf("runtime.SetFinalizer: pointer not at beginning of allocated block\n");
		goto throw;
	}
	nret = 0;
	if(finalizer.type != nil) {
		if(finalizer.type->kind != KindFunc)
			goto badfunc;
		ft = (FuncType*)finalizer.type;
		if(ft->dotdotdot || ft->in.len != 1 || *(Type**)ft->in.array != obj.type)
			goto badfunc;

		// compute size needed for return parameters
		for(i=0; i<ft->out.len; i++) {
			t = ((Type**)ft->out.array)[i];
			nret = (nret + t->align - 1) & ~(t->align - 1);
			nret += t->size;
		}
		nret = (nret + sizeof(void*)-1) & ~(sizeof(void*)-1);
	}
	
	if(!runtime·addfinalizer(obj.data, finalizer.data, nret)) {
		runtime·printf("runtime.SetFinalizer: finalizer already set\n");
		goto throw;
	}
	return;

badfunc:
	runtime·printf("runtime.SetFinalizer: second argument is %S, not func(%S)\n", *finalizer.type->string, *obj.type->string);
throw:
	runtime·throw("runtime.SetFinalizer");
}
