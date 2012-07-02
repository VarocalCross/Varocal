
package pprof
type Profile struct {
	name  string
	mu    sync.Mutex
	m     map[interface{}][]uintptr
	count func() int
	write func(io.Writer, int) error
}
var profiles struct {
var goroutineProfile
var threadcreateProfile
var heapProfile
func lockProfiles()
func unlockProfiles()
func NewProfile(name string) *Profile
func Lookup(name string) *Profile
func Profiles() []*Profile
type byName []*Profile
func (x byName) Len() int
func (x byName) Swap(i, j int)
func (x byName) Less(i, j int) bool
func (p *Profile) Name() string
func (p *Profile) Count() int
func (p *Profile) Add(value interface{}, skip int)
func (p *Profile) Remove(value interface{})
func (p *Profile) WriteTo(w io.Writer, debug int) error
type stackProfile [][]uintptr
func (x stackProfile) Len() int
func (x stackProfile) Stack(i int) []uintptr
func (x stackProfile) Swap(i, j int)
func (x stackProfile) Less(i, j int) bool
type countProfile interface {
	Len() int
	Stack(i int) []uintptr
}
func printCountProfile(w io.Writer, debug int, name string, p countProfile) error
func printStackRecord(w io.Writer, stk []uintptr, allFrames bool)
type byInUseBytes []runtime.MemProfileRecord
func (x byInUseBytes) Len() int
func (x byInUseBytes) Swap(i, j int)
func (x byInUseBytes) Less(i, j int) bool
func WriteHeapProfile(w io.Writer) error
func countHeap() int
func writeHeap(w io.Writer, debug int) error
func countThreadCreate() int
func writeThreadCreate(w io.Writer, debug int) error
func countGoroutine() int
func writeGoroutine(w io.Writer, debug int) error
func writeGoroutineStacks(w io.Writer) error
func writeRuntimeProfile(w io.Writer, debug int, name string, fetch func([]runtime.StackRecord) (int, bool)) error
type runtimeProfile []runtime.StackRecord
func (p runtimeProfile) Len() int
func (p runtimeProfile) Stack(i int) []uintptr
var cpu struct {
func StartCPUProfile(w io.Writer) error
func profileWriter(w io.Writer)
func StopCPUProfile()