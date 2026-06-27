namespace RP.Math.Sample.SpaceFlight
{
    using Xna = Microsoft.Xna.Framework;

    // RP.Math and MonoGame both define Vector/Matrix/Quaternion; alias the RP.Math ones so the
    // two worlds never get confused at a call site.
    using RVector = RP.Math.Vector3d;
    using RMatrix = RP.Math.Matrix;

    /// <summary>
    /// Bridges RP.Math values into the MonoGame/XNA types the renderer needs.
    /// </summary>
    /// <remarks>
    /// The only subtlety is the matrix layout. RP.Math stores <b>column-vector</b> matrices
    /// (a point is transformed as <c>M · v</c>, translation lives in the last <i>column</i>) in
    /// row-major order. XNA/MonoGame uses <b>row-vector</b> matrices (<c>v · M</c>, translation in
    /// the last <i>row</i>). The two are transposes of one another, so <see cref="ToXna(RMatrix)"/>
    /// transposes on the way across. Because (P·V·M·v)ᵀ = vᵀ·Mᵀ·Vᵀ·Pᵀ, feeding the transposed
    /// world/view/projection into MonoGame reproduces exactly the RP.Math transform.
    /// </remarks>
    internal static class Interop
    {
        /// <summary>RP.Math <see cref="RVector"/> → XNA <see cref="Xna.Vector3"/>.</summary>
        public static Xna.Vector3 ToXna(in RVector v) => new((float)v.X, (float)v.Y, (float)v.Z);

        /// <summary>RP.Math column-vector matrix → XNA row-vector matrix (transposed).</summary>
        public static Xna.Matrix ToXna(RMatrix m) => new(
            // Each XNA row is a column of the RP.Math matrix.
            (float)m[0, 0], (float)m[1, 0], (float)m[2, 0], (float)m[3, 0],
            (float)m[0, 1], (float)m[1, 1], (float)m[2, 1], (float)m[3, 1],
            (float)m[0, 2], (float)m[1, 2], (float)m[2, 2], (float)m[3, 2],
            (float)m[0, 3], (float)m[1, 3], (float)m[2, 3], (float)m[3, 3]);

        /// <summary>
        /// Build a Direct3D-ready perspective projection using RP.Math's own
        /// <see cref="RMatrix.PerspectiveFieldOfView"/> builder.
        /// </summary>
        /// <remarks>
        /// RP.Math projections are OpenGL-style: they map the near/far planes to clip-space z of
        /// −1 / +1. Direct3D (and therefore MonoGame's depth buffer) expects 0 / +1. We fix this by
        /// composing the OpenGL projection with a tiny remap matrix R that does
        /// <c>z' = ½z + ½w</c> — i.e. (z+1)/2 after the perspective divide — which exercises
        /// RP.Math's matrix multiply at the same time. The result is then transposed by
        /// <see cref="ToXna(RMatrix)"/> when handed to the effect.
        /// </remarks>
        public static RMatrix PerspectiveD3D(Angle verticalFov, double aspect, double near, double far)
        {
            RMatrix openGl = RMatrix.PerspectiveFieldOfView(verticalFov, aspect, near, far);

            RMatrix glToD3DZ = new(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 0.5, 0.5,
                0, 0, 0, 1);

            // Column-vector composition: R applied after the projection.
            return glToD3DZ * openGl;
        }
    }
}
