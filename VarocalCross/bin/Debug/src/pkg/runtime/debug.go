
package runtime
func Breakpoint()
func LockOSThread()
func UnlockOSThread()
func GOMAXPROCS(n int) int
func NumCPU() int
func NumCgoCall() int64
func NumGoroutine() int
var MemProfileRate int
type MemProfileRecord struct {
	AllocBytes, FreeBytes     int64       // number of bytes allocated, freed
	AllocObjects, FreeObjects int64       // number of objects allocated, freed
	Stack0                    [32]uintptr // stack trace for this record; ends at first 0 entry
}
func (r *MemProfileRecord) InUseBytes() int64
func (r *MemProfileRecord) InUseObjects() int64
func (r *MemProfileRecord) Stack() []uintptr
func MemProfile(p []MemProfileRecord, inuseZero bool) (n int, ok bool)
type StackRecord struct {
	Stack0 [32]uintptr // stack trace for this record; ends at first 0 entry
}
func (r *StackRecord) Stack() []uintptr
func ThreadCreateProfile(p []StackRecord) (n int, ok bool)
func GoroutineProfile(p []StackRecord) (n int, ok bool)
func CPUProfile() []byte
func SetCPUProfileRate(hz int)
func Stack(buf []byte, all bool) int