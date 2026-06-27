// Backward-compatibility alias for this sample assembly.
//
// The library renamed its double-precision 3-component vector `Vector` -> `Vector3d` (see RP.Math's own
// GlobalAliases.cs). That short-name `global using` lives inside the RP.Math project, so it is not visible
// to a referencing assembly like this one. This file recreates it here so the sample's existing files can
// keep using the unqualified name `Vector`. `Vector` and `Vector3d` are the exact same type.
global using Vector = RP.Math.Vector3d;
