
package runtime
type Sigaltstack C.struct_sigaltstack
type Sigset C.sigset_t
type Siginfo C.siginfo_t
type Sigval C.union_sigval
type StackT C.stack_t
type Timespec C.struct_timespec
type Timeval C.struct_timeval
type Itimerval C.struct_itimerval
type sfxsave64 struct{}
type usavefpu struct{}
type Sigcontext C.struct_sigcontext