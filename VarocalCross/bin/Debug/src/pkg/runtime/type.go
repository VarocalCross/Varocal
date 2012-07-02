
package runtime
type commonType struct {
	size       uintptr
	hash       uint32
	_          uint8
	align      uint8
	fieldAlign uint8
	kind       uint8
	alg        *uintptr
	string     *string
	*uncommonType
	ptrToThis *interface{}
}
type _method struct {
	name    *string
	pkgPath *string
	mtyp    *interface{}
	typ     *interface{}
	ifn     unsafe.Pointer
	tfn     unsafe.Pointer
}
type uncommonType struct {
	name    *string
	pkgPath *string
	methods []_method
}
type _imethod struct {
	name    *string
	pkgPath *string
	typ     *interface{}
}
type interfaceType struct {
	commonType
	methods []_imethod
}