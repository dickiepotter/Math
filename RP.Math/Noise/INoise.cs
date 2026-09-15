namespace RP.Math.Noise
{
    /// <summary>
    /// A scalar noise field over the plane: give it a point, it gives you a value. Implemented by every
    /// 2D field here (<see cref="PerlinNoise"/>, <see cref="SimplexNoise"/>, <see cref="ValueNoise"/>,
    /// <see cref="WorleyNoise"/>) so that layering machinery such as <see cref="FractalNoise2"/> can stack
    /// any of them without knowing which it holds.
    /// </summary>
    /// <remarks>
    /// <para><b>The contract.</b> A field must be <i>pure</i> (same input, same output, always), <i>total</i>
    /// (defined for every finite coordinate, positive or negative, near the origin or a million units out),
    /// and <i>thread-safe for reading</i>. Those three properties are what let a caller sample the same
    /// field from eight worker threads generating eight different chunks and get a seamless result.</para>
    /// <para><b>Range.</b> The coherent fields return approximately <c>[-1, 1]</c>; cellular fields return
    /// distances from <c>0</c> upward. Each implementation documents its own range — do not assume.</para>
    /// </remarks>
    public interface INoise2
    {
        /// <summary>Samples the field at <c>(x, y)</c>.</summary>
        double Sample(double x, double y);
    }

    /// <summary>
    /// A scalar noise field over space. The 3D counterpart of <see cref="INoise2"/>; this is the one
    /// volumetric worlds use, since a cave or an ore vein is a shape in three dimensions, not a height.
    /// </summary>
    public interface INoise3
    {
        /// <summary>Samples the field at <c>(x, y, z)</c>.</summary>
        double Sample(double x, double y, double z);
    }

    /// <summary>
    /// A scalar noise field over four dimensions. Two uses justify it: <b>animated</b> 3D noise, where the
    /// fourth axis is time and the field visibly flows rather than cross-fading; and <b>seamlessly tiling</b>
    /// 2D noise, made by tracing a torus through 4D space so the result wraps with no visible join.
    /// </summary>
    public interface INoise4
    {
        /// <summary>Samples the field at <c>(x, y, z, w)</c>.</summary>
        double Sample(double x, double y, double z, double w);
    }
}
