// Backward-compatibility alias for the whole RP.Math assembly.
//
// The double-precision 3-component vector was renamed `Vector` -> `Vector3d` so it reads as one member of
// a named family (float Vector2/3/4 and double Vector2d/3d/4d). This assembly-wide `global using` keeps
// the original short name `Vector` valid in every existing file (and the test suite) without touching
// them: `Vector` and `Vector3d` are the exact same type. New code may use whichever name reads better —
// `Vector3d` where the precision matters next to a float `Vector3`, `Vector` where it is unambiguous.
global using Vector = RP.Math.Vector3d;
