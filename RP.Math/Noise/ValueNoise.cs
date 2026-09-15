namespace RP.Math.Noise
{
    /// <summary>
    /// Value noise in two, three and four dimensions: the simplest coherent field. A pseudo-random
    /// <i>value</i> is placed at every lattice point and the space between is interpolated smoothly.
    /// </summary>
    /// <remarks>
    /// <para><b>Value noise versus gradient noise.</b> <see cref="PerlinNoise"/> stores a random direction
    /// at each lattice point and produces a field that is <i>zero</i> there, peaking between; value noise
    /// stores a random height and produces a field whose extremes land <i>on</i> the lattice. That is its
    /// weakness and its use. The weakness: the extremes are grid-aligned, so a single octave betrays its
    /// square lattice as soon as you look for it. The use: it is roughly twice as fast (one hash and a
    /// blend, no dot products), and once several octaves are stacked and warped the grid character
    /// disappears into the detail.</para>
    ///
    /// <para><b>When to reach for it.</b> High-frequency detail octaves, where speed matters and nobody can
    /// see the lattice through the low-frequency shape; scalar per-cell variation such as ore richness or
    /// tree size; and any place a cheap smooth random field is wanted. For the shape of a landscape, use
    /// gradient or simplex noise instead.</para>
    ///
    /// <para><b>Range.</b> <c>[-1, 1]</c> exactly — unlike gradient noise, the bound is tight and the value
    /// really does reach both ends, at the lattice points where a hash happens to land there.</para>
    /// </remarks>
    public sealed class ValueNoise : INoise2, INoise3, INoise4
    {
        /// <summary>Creates a value-noise field. The same seed always yields the same field.</summary>
        public ValueNoise(int seed = 0)
        {
            this.Seed = seed;
        }

        /// <summary>The seed selecting which variant of the field this is.</summary>
        public int Seed { get; }

        private static double Lerp(double a, double b, double t) => a + ((b - a) * t);

        /// <inheritdoc />
        public double Sample(double x, double y)
        {
            int x0 = PerlinNoise.FastFloor(x), y0 = PerlinNoise.FastFloor(y);
            double u = PerlinNoise.Fade(x - x0), v = PerlinNoise.Fade(y - y0);

            double v00 = NoiseHash.ToSigned(NoiseHash.Hash(x0, y0, this.Seed));
            double v10 = NoiseHash.ToSigned(NoiseHash.Hash(x0 + 1, y0, this.Seed));
            double v01 = NoiseHash.ToSigned(NoiseHash.Hash(x0, y0 + 1, this.Seed));
            double v11 = NoiseHash.ToSigned(NoiseHash.Hash(x0 + 1, y0 + 1, this.Seed));

            return Lerp(Lerp(v00, v10, u), Lerp(v01, v11, u), v);
        }

        /// <inheritdoc />
        public double Sample(double x, double y, double z)
        {
            int x0 = PerlinNoise.FastFloor(x), y0 = PerlinNoise.FastFloor(y), z0 = PerlinNoise.FastFloor(z);
            double u = PerlinNoise.Fade(x - x0), v = PerlinNoise.Fade(y - y0), w = PerlinNoise.Fade(z - z0);
            int x1 = x0 + 1, y1 = y0 + 1, z1 = z0 + 1;

            double v000 = NoiseHash.ToSigned(NoiseHash.Hash(x0, y0, z0, this.Seed));
            double v100 = NoiseHash.ToSigned(NoiseHash.Hash(x1, y0, z0, this.Seed));
            double v010 = NoiseHash.ToSigned(NoiseHash.Hash(x0, y1, z0, this.Seed));
            double v110 = NoiseHash.ToSigned(NoiseHash.Hash(x1, y1, z0, this.Seed));
            double v001 = NoiseHash.ToSigned(NoiseHash.Hash(x0, y0, z1, this.Seed));
            double v101 = NoiseHash.ToSigned(NoiseHash.Hash(x1, y0, z1, this.Seed));
            double v011 = NoiseHash.ToSigned(NoiseHash.Hash(x0, y1, z1, this.Seed));
            double v111 = NoiseHash.ToSigned(NoiseHash.Hash(x1, y1, z1, this.Seed));

            double a = Lerp(Lerp(v000, v100, u), Lerp(v010, v110, u), v);
            double b = Lerp(Lerp(v001, v101, u), Lerp(v011, v111, u), v);
            return Lerp(a, b, w);
        }

        /// <inheritdoc />
        public double Sample(double x, double y, double z, double w)
        {
            int x0 = PerlinNoise.FastFloor(x), y0 = PerlinNoise.FastFloor(y);
            int z0 = PerlinNoise.FastFloor(z), w0 = PerlinNoise.FastFloor(w);
            double ux = PerlinNoise.Fade(x - x0), uy = PerlinNoise.Fade(y - y0);
            double uz = PerlinNoise.Fade(z - z0), uw = PerlinNoise.Fade(w - w0);

            double acc0 = 0, acc1 = 0;
            for (int corner = 0; corner < 16; corner++)
            {
                int cx = corner & 1, cy = (corner >> 1) & 1, cz = (corner >> 2) & 1, cw = (corner >> 3) & 1;
                double value = NoiseHash.ToSigned(NoiseHash.Hash(x0 + cx, y0 + cy, z0 + cz, w0 + cw, this.Seed));
                double weight = (cx == 1 ? ux : 1.0 - ux)
                              * (cy == 1 ? uy : 1.0 - uy)
                              * (cz == 1 ? uz : 1.0 - uz);
                if (cw == 1) { acc1 += value * weight; } else { acc0 += value * weight; }
            }

            return Lerp(acc0, acc1, uw);
        }

        /// <summary>
        /// The raw value at a single lattice point, with no interpolation — a stable pseudo-random number in
        /// <c>[-1, 1]</c> attached to an integer coordinate.
        /// </summary>
        /// <remarks>
        /// Reach for this whenever something is decided <i>per discrete cell</i> rather than continuously:
        /// whether this chunk holds a rare structure, how rich this particular ore blob is, which variant a
        /// given tree takes. Sampling the interpolated field at integer coordinates would work, but this
        /// says what is meant and skips the blend.
        /// </remarks>
        public double At(int x, int y) => NoiseHash.ToSigned(NoiseHash.Hash(x, y, this.Seed));

        /// <summary>The raw value at a 3D lattice point. See <see cref="At(int, int)"/>.</summary>
        public double At(int x, int y, int z) => NoiseHash.ToSigned(NoiseHash.Hash(x, y, z, this.Seed));

        /// <summary>The raw value at a 2D lattice point, mapped to <c>[0, 1)</c> — the convenient form for
        /// probability tests such as "does a tree grow here?".</summary>
        public double UnitAt(int x, int y) => NoiseHash.ToUnit(NoiseHash.Hash(x, y, this.Seed));

        /// <summary>The raw value at a 3D lattice point, mapped to <c>[0, 1)</c>.</summary>
        public double UnitAt(int x, int y, int z) => NoiseHash.ToUnit(NoiseHash.Hash(x, y, z, this.Seed));
    }
}
