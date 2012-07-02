// auto generated by go tool dist
// goos=darwin goarch=amd64

#include "runtime.h"
#include "defs_GOOS_GOARCH.h"
#include "os_GOOS.h"

#line 3211 "C:\Go\src\pkg\runtime\sigqueue.goc"
static struct { 
Note; 
uint32 mask[ ( NSIG+31 ) /32]; 
uint32 wanted[ ( NSIG+31 ) /32]; 
uint32 kick; 
bool inuse; 
} sig; 
#line 3220 "C:\Go\src\pkg\runtime\sigqueue.goc"
bool 
runtime·sigsend ( int32 s ) 
{ 
uint32 bit , mask; 
#line 3225 "C:\Go\src\pkg\runtime\sigqueue.goc"
if ( !sig.inuse || s < 0 || s >= 32*nelem ( sig.wanted ) || ! ( sig.wanted[s/32]& ( 1U<< ( s&31 ) ) ) ) 
return false; 
bit = 1 << ( s&31 ) ; 
for ( ;; ) { 
mask = sig.mask[s/32]; 
if ( mask & bit ) 
break; 
if ( runtime·cas ( &sig.mask[s/32] , mask , mask|bit ) ) { 
#line 3235 "C:\Go\src\pkg\runtime\sigqueue.goc"
if ( runtime·cas ( &sig.kick , 1 , 0 ) ) 
runtime·notewakeup ( &sig ) ; 
break; 
} 
} 
return true; 
} 
void
runtime·signal_recv(uint32 m)
{
#line 3245 "C:\Go\src\pkg\runtime\sigqueue.goc"

	static uint32 recv[nelem(sig.mask)];
	int32 i, more;
	
	for(;;) {
		// Serve from local copy if there are bits left.
		for(i=0; i<NSIG; i++) {
			if(recv[i/32]&(1U<<(i&31))) {
				recv[i/32] ^= 1U<<(i&31);
				m = i;
				goto done;
			}
		}

		// Get a new local copy.
		// Ask for a kick if more signals come in
		// during or after our check (before the sleep).
		if(sig.kick == 0) {
			runtime·noteclear(&sig);
			runtime·cas(&sig.kick, 0, 1);
		}

		more = 0;
		for(i=0; i<nelem(sig.mask); i++) {
			for(;;) {
				m = sig.mask[i];
				if(runtime·cas(&sig.mask[i], m, 0))
					break;
			}
			recv[i] = m;
			if(m != 0)
				more = 1;
		}
		if(more)
			continue;

		// Sleep waiting for more.
		runtime·entersyscall();
		runtime·notesleep(&sig);
		runtime·exitsyscall();
	}

done:;
	// goc requires that we fall off the end of functions
	// that return values instead of using our own return
	// statements.
	FLUSH(&m);
}
void
runtime·signal_enable(uint32 s)
{
#line 3294 "C:\Go\src\pkg\runtime\sigqueue.goc"

	int32 i;

	if(!sig.inuse) {
		// The first call to signal_enable is for us
		// to use for initialization.  It does not pass
		// signal information in m.
		sig.inuse = true;	// enable reception of signals; cannot disable
		runtime·noteclear(&sig);
		return;
	}
	
	if(~s == 0) {
		// Special case: want everything.
		for(i=0; i<nelem(sig.wanted); i++)
			sig.wanted[i] = ~(uint32)0;
		runtime·sigenable(s);
		return;
	}

	if(s >= nelem(sig.wanted)*32)
		return;
	sig.wanted[s/32] |= 1U<<(s&31);
	runtime·sigenable(s);
}
