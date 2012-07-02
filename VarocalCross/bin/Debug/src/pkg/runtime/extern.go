
package runtime
func Gosched()
func Goexit()
func Caller(skip int) (pc uintptr, file string, line int, ok bool)
func Callers(skip int, pc []uintptr) int
type Func struct { // Keep in sync with runtime.h:struct Func
	name   string
	typ    string  // go type string
	src    string  // src file name
	pcln   []byte  // pc/ln tab for this func
	entry  uintptr // entry pc
	pc0    uintptr // starting pc, ln for table
	ln0    int32
	frame  int32 // stack frame size
	args   int32 // number of 32-bit in/out args
	locals int32 // number of 32-bit locals
}
func FuncForPC(pc uintptr) *Func
func (f *Func) Name() string
func (f *Func) Entry() uintptr
func (f *Func) FileLine(pc uintptr) (file string, line int)
func funcline_go(*Func, uintptr) (string, int)
func mid() uint32
func SetFinalizer(x, f interface
func getgoroot() string
func GOROOT() string
func Version() string