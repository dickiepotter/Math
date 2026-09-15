namespace RP.Math.Noise
{
    /// <summary>
    /// Simplex noise in two, three and four dimensions — Ken Perlin's 2001 replacement for his own 1985
    /// gradient noise. Same job as <see cref="PerlinNoise"/>, better behaved in higher dimensions.
    /// </summary>
    /// <remarks>
    /// <para><b>The idea.</b> Classic Perlin noise blends the corners of a hypercube: 4 in 2D, 8 in 3D,
    /// 16 in 4D — the cost doubles with every dimension. Simplex noise instead divides space into
    /// <i>simplices</i>, the simplest shape that can fill a space: a triangle in 2D, a tetrahedron in 3D.
    /// A simplex has only <c>n+1</c> corners, so the cost grows linearly rather than exponentially — 3 in
    /// 2D, 4 in 3D, 5 in 4D. In 4D that is 5 corners against 16.</para>
    ///
    /// <para><b>Skewing.</b> You cannot lay triangles on a square grid directly, so the input is
    /// <i>skewed</i> into a space where the simplex grid becomes an axis-aligned one that is trivial to
    /// index; the corners found there are then unskewed back. That is all the <c>F</c> and <c>G</c>
    /// constants below do.</para>
    ///
    /// <para><b>Radial falloff, not interpolation.</b> The other structural difference: rather than
    /// interpolating between corners, each corner contributes a bump that falls smoothly to exactly zero at
    /// a fixed radius, and the contributions are summed. Because each bump dies before reaching the next
    /// cell, there is nothing to blend and no lattice-aligned crease to leak through — which is why simplex
    /// noise has no visible directional bias, while Perlin noise retains a faint square character.</para>
    ///
    /// <para><b>Range.</b> Approximately <c>[-1, 1]</c>, set by the scale constants below. Those constants
    /// are the normalisation that maps the raw summed bumps onto that interval; they are specific to the
    /// gradient tables in <see cref="NoiseHash"/> and must be re-measured if those tables ever change.
    /// <c>RP.Math.Tests</c> asserts the range directly, so a silent drift here fails the build.</para>
    ///
    /// <para>Instances are immutable and safe to share across threads.</para>
    /// </remarks>
    public sealed class SimplexNoise : INoise2, INoise3, INoise4
    {
        // --- 2D skew constants ---
        // F2 maps the square grid onto the triangular one; G2 maps back. F2 = (sqrt(3) - 1) / 2.
        private const double F2 = 0.3660254037844386;
        private const double G2 = 0.21132486540518713; // (3 - sqrt(3)) / 6

        // --- 3D skew constants --- F3 = 1/3, G3 = 1/6.
        private const double F3 = 1.0 / 3.0;
        private const double G3 = 1.0 / 6.0;

        // --- 4D skew constants --- F4 = (sqrt(5) - 1) / 4, G4 = (5 - sqrt(5)) / 20.
        private const double F4 = 0.30901699437494745;
        private const double G4 = 0.13819660112501051;

        // Normalisation: the raw sum of corner bumps is tiny, and these map it onto [-1, 1]. The constants
        // are 1/max, with the maxima found by hill-climbing the un-normalised field to convergence (raw
        // maxima 1.008020e-2, 1.300716e-2, 1.592922e-2), times 0.9995 of head-room. They are specific to
        // the gradient tables in NoiseHash *and* to the support radius below; change either and these must
        // be re-measured. RP.Math.Tests asserts the range, so a drift fails the build.
        private const double Scale2 = 99.1547;
        private const double Scale3 = 76.8424;
        private const double Scale4 = 62.7463;

        /// <summary>Creates a simplex field. The same seed always yields the same field.</summary>
        public SimplexNoise(int seed = 0)
        {
            this.Seed = seed;
        }

        /// <summary>The seed selecting which variant of the field this is.</summary>
        public int Seed { get; }

        /// <inheritdoc />
        public double Sample(double x, double y)
        {
            // Skew the input into the triangular lattice and find which cell we landed in.
            double skew = (x + y) * F2;
            int i = PerlinNoise.FastFloor(x + skew);
            int j = PerlinNoise.FastFloor(y + skew);

            // Unskew the cell origin back to input space and take the offset from it.
            double unskew = (i + j) * G2;
            double x0 = x - (i - unskew);
            double y0 = y - (j - unskew);

            // A square cell is two triangles; which one are we in? Upper or lower, decided by x0 > y0.
            int i1 = x0 > y0 ? 1 : 0;
            int j1 = x0 > y0 ? 0 : 1;

            // Offsets to the other two corners of this triangle, unskewed.
            double x1 = x0 - i1 + G2;
            double y1 = y0 - j1 + G2;
            double x2 = x0 - 1.0 + (2.0 * G2);
            double y2 = y0 - 1.0 + (2.0 * G2);

            double total = Corner2(i, j, x0, y0)
                         + Corner2(i + i1, j + j1, x1, y1)
                         + Corner2(i + 1, j + 1, x2, y2);

            return Scale2 * total;
        }

        /// <summary>
        /// One corner's contribution in 2D: a radial bump of squared radius 0.5 around the corner,
        /// modulated by the corner's gradient. The fourth power gives the bump a smooth, flat-topped
        /// falloff that reaches zero with zero slope — no crease where neighbouring bumps meet.
        /// </summary>
        /// <remarks>
        /// <para><b>Why the radius is 0.5 and not 0.6.</b> Most published ports of this algorithm use 0.6
        /// for the 3D and 4D cases. That value is too large: it lets a corner's bump extend past the
        /// boundary of the region in which that corner is actually one of the summed neighbours, so the
        /// contribution is truncated the moment the sample crosses into the next simplex — and the field
        /// <i>jumps</i>. It is a small jump, which is exactly why it survives in so much code; it shows up
        /// as faint speckled seams once the field drives a surface normal. Measured here, 0.6 produced
        /// slopes around 250 where the field's genuine maximum slope is about 6. At 0.5 every bump reaches
        /// zero before it is dropped, and the field is continuous.</para>
        /// </remarks>
        private double Corner2(int i, int j, double dx, double dy)
        {
            double t = 0.5 - (dx * dx) - (dy * dy);
            if (t < 0.0) return 0.0; // outside this corner's radius: contributes nothing at all
            t *= t;
            return t * t * NoiseHash.GradientDot(i, j, this.Seed, dx, dy);
        }

        /// <inheritdoc />
        public double Sample(double x, double y, double z)
        {
            double skew = (x + y + z) * F3;
            int i = PerlinNoise.FastFloor(x + skew);
            int j = PerlinNoise.FastFloor(y + skew);
            int k = PerlinNoise.FastFloor(z + skew);

            double unskew = (i + j + k) * G3;
            double x0 = x - (i - unskew);
            double y0 = y - (j - unskew);
            double z0 = z - (k - unskew);

            // A cube is six tetrahedra. Which one holds this point is decided by the ordering of x0, y0, z0
            // — the six permutations map one-to-one onto the six tetrahedra.
            int i1, j1, k1, i2, j2, k2;
            if (x0 >= y0)
            {
                if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }        // x > y > z
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }   // x > z > y
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }                 // z > x > y
            }
            else
            {
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }         // z > y > x
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }    // y > z > x
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }                 // y > x > z
            }

            double x1 = x0 - i1 + G3, y1 = y0 - j1 + G3, z1 = z0 - k1 + G3;
            double x2 = x0 - i2 + (2.0 * G3), y2 = y0 - j2 + (2.0 * G3), z2 = z0 - k2 + (2.0 * G3);
            double x3 = x0 - 1.0 + (3.0 * G3), y3 = y0 - 1.0 + (3.0 * G3), z3 = z0 - 1.0 + (3.0 * G3);

            double total = Corner3(i, j, k, x0, y0, z0)
                         + Corner3(i + i1, j + j1, k + k1, x1, y1, z1)
                         + Corner3(i + i2, j + j2, k + k2, x2, y2, z2)
                         + Corner3(i + 1, j + 1, k + 1, x3, y3, z3);

            return Scale3 * total;
        }

        /// <summary>One corner's contribution in 3D, at the same squared radius 0.5 as 2D, and for the same
        /// continuity reason — see <see cref="Corner2"/>.</summary>
        private double Corner3(int i, int j, int k, double dx, double dy, double dz)
        {
            double t = 0.5 - (dx * dx) - (dy * dy) - (dz * dz);
            if (t < 0.0) return 0.0;
            t *= t;
            return t * t * NoiseHash.GradientDot(i, j, k, this.Seed, dx, dy, dz);
        }

        /// <inheritdoc />
        public double Sample(double x, double y, double z, double w)
        {
            double skew = (x + y + z + w) * F4;
            int i = PerlinNoise.FastFloor(x + skew);
            int j = PerlinNoise.FastFloor(y + skew);
            int k = PerlinNoise.FastFloor(z + skew);
            int l = PerlinNoise.FastFloor(w + skew);

            double unskew = (i + j + k + l) * G4;
            double x0 = x - (i - unskew);
            double y0 = y - (j - unskew);
            double z0 = z - (k - unskew);
            double w0 = w - (l - unskew);

            // In 4D a hypercube is 24 simplices, and the point's simplex is determined by the full ordering
            // of the four offsets. Rather than a 24-case branch or the traditional lookup table, count how
            // many coordinates each one exceeds: that rank (0..3) *is* the position in the sorted order, so
            // corner c is entered once the rank is at least 4-c. This is both shorter and self-evidently
            // correct, where the lookup table is neither.
            int rx = (x0 > y0 ? 1 : 0) + (x0 > z0 ? 1 : 0) + (x0 > w0 ? 1 : 0);
            int ry = (y0 >= x0 ? 1 : 0) + (y0 > z0 ? 1 : 0) + (y0 > w0 ? 1 : 0);
            int rz = (z0 >= x0 ? 1 : 0) + (z0 >= y0 ? 1 : 0) + (z0 > w0 ? 1 : 0);
            int rw = (w0 >= x0 ? 1 : 0) + (w0 >= y0 ? 1 : 0) + (w0 >= z0 ? 1 : 0);

            double total = Corner4(i, j, k, l, x0, y0, z0, w0);
            for (int c = 1; c <= 3; c++)
            {
                int threshold = 4 - c;
                int dx = rx >= threshold ? 1 : 0;
                int dy = ry >= threshold ? 1 : 0;
                int dz = rz >= threshold ? 1 : 0;
                int dw = rw >= threshold ? 1 : 0;
                double g = c * G4;
                total += Corner4(
                    i + dx, j + dy, k + dz, l + dw,
                    x0 - dx + g, y0 - dy + g, z0 - dz + g, w0 - dw + g);
            }

            double g4 = 4.0 * G4;
            total += Corner4(i + 1, j + 1, k + 1, l + 1, x0 - 1.0 + g4, y0 - 1.0 + g4, z0 - 1.0 + g4, w0 - 1.0 + g4);

            return Scale4 * total;
        }

        /// <summary>One corner's contribution in 4D, at squared radius 0.5 — see <see cref="Corner2"/>.</summary>
        private double Corner4(int i, int j, int k, int l, double dx, double dy, double dz, double dw)
        {
            double t = 0.5 - (dx * dx) - (dy * dy) - (dz * dz) - (dw * dw);
            if (t < 0.0) return 0.0;
            t *= t;
            return t * t * NoiseHash.GradientDot(i, j, k, l, this.Seed, dx, dy, dz, dw);
        }

        /// <summary>
        /// A seamlessly tiling 2D field, produced by walking a <b>torus</b> through the 4D field rather than
        /// a plane through the 2D one.
        /// </summary>
        /// <remarks>
        /// <para>The trick is worth understanding. A square that tiles in both axes is topologically a
        /// torus: walk off the right edge and you arrive at the left. A torus embeds naturally in four
        /// dimensions as a pair of circles — <c>(cos u, sin u, cos v, sin v)</c>. Sample 4D noise along that
        /// surface and the result is <i>necessarily</i> seamless, because the path literally closes on
        /// itself; there is no join to hide. This is how seamless textures are made, and it works for any
        /// 4D field, not just this one.</para>
        /// <para><paramref name="u"/> and <paramref name="v"/> are positions within the tile in
        /// <c>[0, 1)</c>; values outside simply wrap. <paramref name="detail"/> scales how many features
        /// fit across the tile.</para>
        /// </remarks>
        public double SampleSeamless(double u, double v, double detail = 1.0)
        {
            const double TwoPi = 6.283185307179586;
            double au = u * TwoPi, av = v * TwoPi;
            double r = detail / TwoPi;
            return this.Sample(r * System.Math.Cos(au), r * System.Math.Sin(au), r * System.Math.Cos(av), r * System.Math.Sin(av));
        }
    }
}
