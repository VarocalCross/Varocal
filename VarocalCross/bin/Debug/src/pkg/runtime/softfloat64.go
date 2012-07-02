
package runtime
func funpack64(f uint64) (sign, mant uint64, exp int, inf, nan bool)
func funpack32(f uint32) (sign, mant uint32, exp int, inf, nan bool)
func fpack64(sign, mant uint64, exp int, trunc uint64) uint64
func fpack32(sign, mant uint32, exp int, trunc uint32) uint32
func fadd64(f, g uint64) uint64
func fsub64(f, g uint64) uint64
func fneg64(f uint64) uint64
func fmul64(f, g uint64) uint64
func fdiv64(f, g uint64) uint64
func f64to32(f uint64) uint32
func f32to64(f uint32) uint64
func fcmp64(f, g uint64) (cmp int, isnan bool)
func f64toint(f uint64) (val int64, ok bool)
func fintto64(val int64) (f uint64)
func mullu(u, v uint64) (lo, hi uint64)
func divlu(u1, u0, v uint64) (q, r uint64)
func fadd64c(f, g uint64, ret *uint64)
func fsub64c(f, g uint64, ret *uint64)
func fmul64c(f, g uint64, ret *uint64)
func fdiv64c(f, g uint64, ret *uint64)
func fneg64c(f uint64, ret *uint64)
func f32to64c(f uint32, ret *uint64)
func f64to32c(f uint64, ret *uint32)
func fcmp64c(f, g uint64, ret *int, retnan *bool)
func fintto64c(val int64, ret *uint64)
func f64tointc(f uint64, ret *int64, retok *bool)