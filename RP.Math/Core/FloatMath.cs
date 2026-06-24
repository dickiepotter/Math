namespace RP.Math
{
    /// <summary>
    /// Small internal helpers used by the float vector family so it compiles on every target. The
    /// <c>netstandard2.0</c> target predates <c>System.Math.Clamp</c> and <c>System.HashCode</c>, so we
    /// supply equivalents here rather than litter the vectors with <c>#if</c> blocks.
    /// </summary>
    internal static class FloatMath
    {
        /// <summary>Clamps <paramref name="v"/> into <c>[min, max]</c>.</summary>
        public static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);

        // A standard FNV-ish combine: stable within a run and well-spread, which is all GetHashCode needs.
        public static int CombineHash(float a, float b)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + a.GetHashCode();
                h = h * 31 + b.GetHashCode();
                return h;
            }
        }

        public static int CombineHash(float a, float b, float c)
        {
            unchecked
            {
                int h = CombineHash(a, b);
                return h * 31 + c.GetHashCode();
            }
        }

        public static int CombineHash(float a, float b, float c, float d)
        {
            unchecked
            {
                int h = CombineHash(a, b, c);
                return h * 31 + d.GetHashCode();
            }
        }
    }
}
