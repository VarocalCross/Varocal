
package runtime
var Fadd64
var Fsub64
var Fmul64
var Fdiv64
var F64to32
var F32to64
var Fcmp64
var Fintto64
var F64toint
func entersyscall()
func exitsyscall()
func golockedOSThread() bool
func stackguard() (sp, limit uintptr)
var Entersyscall
var Exitsyscall
var LockedOSThread
var Stackguard