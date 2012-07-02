
package debug
type T int
func (t *T) ptrmethod() []byte
func (t T) method() []byte
func TestStack(t *testing.T)
func check(t *testing.T, line, has string)