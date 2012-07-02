
package runtime
type Error interface {
	error
	// RuntimeError is a no-op function but
	// serves to distinguish types that are runtime
	// errors from ordinary errors: a type is a
	// runtime error if it has a RuntimeError method.
	RuntimeError()
}
type TypeAssertionError struct {
	interfaceString string
	concreteString  string
	assertedString  string
	missingMethod   string // one method needed by Interface, missing from Concrete
}
func (*TypeAssertionError) RuntimeError()
func (e *TypeAssertionError) Error() string
func newTypeAssertionError(ps1, ps2, ps3 *string, pmeth *string, ret *interface
var s1, s2, s3, meth string
type errorString string
func (e errorString) RuntimeError()
func (e errorString) Error() string
func newErrorString(s string, ret *interface
type stringer interface {
	String() string
}
func typestring(interface
func printany(i interface
func panicwrap(pkg, typ, meth string)