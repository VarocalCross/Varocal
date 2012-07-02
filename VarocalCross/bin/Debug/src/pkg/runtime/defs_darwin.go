
package runtime
type MachBody C.mach_msg_body_t
type MachHeader C.mach_msg_header_t
type MachNDR C.NDR_record_t
type MachPort C.mach_msg_port_descriptor_t
type StackT C.struct_sigaltstack
type Sighandler C.union___sigaction_u
type Sigaction C.struct___sigaction // used in syscalls
type Sigval C.union_sigval
type Siginfo C.siginfo_t
type Timeval C.struct_timeval
type Itimerval C.struct_itimerval
type FPControl C.struct_fp_control
type FPStatus C.struct_fp_status
type RegMMST C.struct_mmst_reg
type RegXMM C.struct_xmm_reg
type Regs64 C.struct_x86_thread_state64
type FloatState64 C.struct_x86_float_state64
type ExceptionState64 C.struct_x86_exception_state64
type Mcontext64 C.struct_mcontext64
type Regs32 C.struct_i386_thread_state
type FloatState32 C.struct_i386_float_state
type ExceptionState32 C.struct_i386_exception_state
type Mcontext32 C.struct_mcontext32
type Ucontext C.struct_ucontext