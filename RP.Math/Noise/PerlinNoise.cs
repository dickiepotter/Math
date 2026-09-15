namespace RP.Math.Noise
{
    using System;

    /// <summary>
    /// Perlin gradient noise in two, three and four dimensions — the workhorse coherent-noise field.
    /// "Coherent" means nearby inputs give nearby outputs: unlike white noise it is smooth, so it reads as
    /// hills, clouds and marble rather than static.
    /// </summary>
    /// <remarks>
    /// <para><b>How it works, in one paragraph.</b> Space is divided by an integer lattice. Every lattice
    /// corner is assigned a pseudo-random unit <i>direction</i> (not a value) by <see cref="NoiseHash"/>.
    /// To sample a point, take the offset from each surrounding corner to the point and dot it with that
    /// corner's gradient — giving a value that is exactly zero at the corner and rises along the gradient.
    /// Then blend those corner contributions together, weighting by how close the point is to each, using a
    /// smoothing curve. The result is a field that is zero-crossing on the lattice and smoothly undulating
    /// between, with continuous first and second derivatives.</para>
    ///
    /// <para><b>The fade curve.</b> The blend weight is Perlin's improved quintic
    /// <c>6t^5 - 15t^4 + 10t^3</c>, not plain linear or cubic interpolation. That matters: the quintic has
    /// zero <i>first and second</i> derivatives at <c>t = 0</c> and <c>t = 1</c>, so the field's curvature is
    /// continuous across lattice boundaries. The original 1985 cubic had a discontinuous second derivative,
    /// which shows up as faint creases along the grid once you light the surface — the classic
    /// "why does my terrain have square artefacts under a low sun?" bug.</para>
    ///
    /// <para><b>Range.</b> <c>[-1, 1]</c>, and genuinely so: each dimension carries its own normalisation
    /// constant below, because raw Perlin noise does <i>not</i> share one bound across dimensions — it peaks
    /// at <c>sqrt(2)/2</c> in 2D but overshoots <c>1.22</c> in 4D. Normalising per dimension is what lets a
    /// threshold such as "sea level at 0.1" mean the same thing whichever field feeds it. Note that the
    /// distribution is still bell-shaped, not uniform: most samples cluster near zero, so if you need the
    /// extremes to be common, reach for a shaping curve in <see cref="NoiseCurves"/> rather than multiplying
    /// up, which only clips.</para>
    ///
    /// <para><b>Perlin or simplex?</b> <see cref="SimplexNoise"/> is faster in 3D and 4D and has no
    /// directional bias along the axes; Perlin is marginally cheaper in 2D and has a subtle square-ish
    /// character that some terrain actually wants. Both are here; profile before caring.</para>
    ///
    /// <para>Instances are immutable and safe to share across threads.</para>
    /// </remarks>
    public sealed class PerlinNoise : INoise2, INoise3, INoise4
    {
        // Per-dimension normalisation. Raw Perlin noise has a different bound in every dimension, so one
        // shared constant would either clip the low-dimensional fields or let the high-dimensional ones
        // escape [-1, 1]. These divide out the true maxima, which were found by hill-climbing the raw field
        // to convergence rather than by sampling and hoping: 2D lands exactly on sqrt(2)/2 = 0.70710678,
        // 3D on 1.0005040, 4D on 1.2209320. The 0.9995 factor is head-room, so that a coordinate nobody
        // tried cannot nudge a sample past the bound. RP.Math.Tests asserts the range, so a drift here
        // fails the build rather than silently rescaling every world built on it.
        private const double Normalise2 = 0.9995 / 0.7071067811865476;
        private const double Normalise3 = 0.9995 / 1.0005040;
        private const double Normalise4 = 0.9995 / 1.2209320;

        /// <summary>Creates a Perlin field. The same seed always yields the same field.</summary>
        public PerlinNoise(int seed = 0)
        {
            this.Seed = seed;
        }

        /// <summary>The seed selecting which of the field's infinitely many variants this is.</summary>
        public int Seed { get; }

        /// <summary>
        /// Perlin's quintic fade, <c>6t^5 - 15t^4 + 10t^3</c>: an S-curve from 0 to 1 whose first and second
        /// derivatives both vanish at each end. Exposed because it is useful well beyond noise — any blend
        /// that must not crease at its joins wants this rather than smoothstep.
        /// </summary>
        public static double Fade(double t) => t * t * t * ((t * ((t * 6.0) - 15.0)) + 10.0);

        /// <summary>The derivative of <see cref="Fade"/>, <c>30t^4 - 60t^3 + 30t^2</c>.</summary>
        public static double FadeDerivative(double t) => 30.0 * t * t * ((t * (t - 2.0)) + 1.0);

        /// <summary>Linear interpolation, the blend that the faded weight drives.</summary>
        private static double Lerp(double a, double b, double t) => a + ((b - a) * t);

        /// <summary>
        /// Floor as an <see cref="int"/>. <c>(int)</c> truncates toward zero, which is wrong for negative
        /// coordinates: <c>(int)(-0.5)</c> is 0, but the lattice cell containing -0.5 is -1. Getting this
        /// wrong mirrors the entire field about the origin — a bug that hides until someone walks west.
        /// </summary>
        internal static int FastFloor(double v)
        {
            int i = (int)v;
            return v < i ? i - 1 : i;
        }

        /// <inheritdoc />
        public double Sample(double x, double y)
        {
            int x0 = FastFloor(x), y0 = FastFloor(y);
            double fx = x - x0, fy = y - y0;
            double u = Fade(fx), v = Fade(fy);

            // The four corners of the cell, each contributing gradient . offset.
            double n00 = NoiseHash.GradientDot(x0, y0, this.Seed, fx, fy);
            double n10 = NoiseHash.GradientDot(x0 + 1, y0, this.Seed, fx - 1.0, fy);
            double n01 = NoiseHash.GradientDot(x0, y0 + 1, this.Seed, fx, fy - 1.0);
            double n11 = NoiseHash.GradientDot(x0 + 1, y0 + 1, this.Seed, fx - 1.0, fy - 1.0);

            // Bilinear blend, but with faded weights — that is what makes it smooth rather than faceted.
            return Normalise2 * Lerp(Lerp(n00, n10, u), Lerp(n01, n11, u), v);
        }

        /// <inheritdoc />
        public double Sample(double x, double y, double z)
        {
            int x0 = FastFloor(x), y0 = FastFloor(y), z0 = FastFloor(z);
            double fx = x - x0, fy = y - y0, fz = z - z0;
            double u = Fade(fx), v = Fade(fy), w = Fade(fz);

            int x1 = x0 + 1, y1 = y0 + 1, z1 = z0 + 1;
            double gx = fx - 1.0, gy = fy - 1.0, gz = fz - 1.0;

            double n000 = NoiseHash.GradientDot(x0, y0, z0, this.Seed, fx, fy, fz);
            double n100 = NoiseHash.GradientDot(x1, y0, z0, this.Seed, gx, fy, fz);
            double n010 = NoiseHash.GradientDot(x0, y1, z0, this.Seed, fx, gy, fz);
            double n110 = NoiseHash.GradientDot(x1, y1, z0, this.Seed, gx, gy, fz);
            double n001 = NoiseHash.GradientDot(x0, y0, z1, this.Seed, fx, fy, gz);
            double n101 = NoiseHash.GradientDot(x1, y0, z1, this.Seed, gx, fy, gz);
            double n011 = NoiseHash.GradientDot(x0, y1, z1, this.Seed, fx, gy, gz);
            double n111 = NoiseHash.GradientDot(x1, y1, z1, this.Seed, gx, gy, gz);

            double a = Lerp(Lerp(n000, n100, u), Lerp(n010, n110, u), v);
            double b = Lerp(Lerp(n001, n101, u), Lerp(n011, n111, u), v);
            return Normalise3 * Lerp(a, b, w);
        }

        /// <inheritdoc />
        public double Sample(double x, double y, double z, double w)
        {
            int x0 = FastFloor(x), y0 = FastFloor(y), z0 = FastFloor(z), w0 = FastFloor(w);
            double fx = x - x0, fy = y - y0, fz = z - z0, fw = w - w0;
            double ux = Fade(fx), uy = Fade(fy), uz = Fade(fz), uw = Fade(fw);

            // 16 corners in 4D. Written as a loop rather than unrolled: the unrolled form is sixteen
            // near-identical lines in which a single transposed index is invisible.
            double acc0 = 0, acc1 = 0;
            for (int corner = 0; corner < 16; corner++)
            {
                int cx = corner & 1, cy = (corner >> 1) & 1, cz = (corner >> 2) & 1, cw = (corner >> 3) & 1;
                double contribution = NoiseHash.GradientDot(
                    x0 + cx, y0 + cy, z0 + cz, w0 + cw, this.Seed,
                    fx - cx, fy - cy, fz - cz, fw - cw);

                // Weight by the faded distance along each axis; the product is the 4D blend weight.
                double weight = (cx == 1 ? ux : 1.0 - ux)
                              * (cy == 1 ? uy : 1.0 - uy)
                              * (cz == 1 ? uz : 1.0 - uz);

                if (cw == 1) { acc1 += contribution * weight; } else { acc0 += contribution * weight; }
            }

            return Normalise4 * Lerp(acc0, acc1, uw);
        }

        /// <summary>
        /// A <b>seamlessly tiling</b> 2D sample: the field repeats exactly every <paramref name="periodX"/>
        /// by <paramref name="periodY"/> units, with no visible join.
        /// </summary>
        /// <remarks>
        /// Achieved by wrapping the lattice coordinates rather than the input — the corner at
        /// <c>periodX</c> is literally the same corner, with the same gradient, as the one at <c>0</c>, so
        /// the two edges do not merely look similar, they are identical. Periods must be positive integers;
        /// a non-integer period cannot tile, because the lattice would not line up.
        /// </remarks>
        public double SampleTiled(double x, double y, int periodX, int periodY)
        {
            if (periodX <= 0) throw new ArgumentOutOfRangeException(nameof(periodX), "Tiling period must be positive.");
            if (periodY <= 0) throw new ArgumentOutOfRangeException(nameof(periodY), "Tiling period must be positive.");

            int x0 = FastFloor(x), y0 = FastFloor(y);
            double fx = x - x0, fy = y - y0;
            double u = Fade(fx), v = Fade(fy);

            int wx0 = Wrap(x0, periodX), wx1 = Wrap(x0 + 1, periodX);
            int wy0 = Wrap(y0, periodY), wy1 = Wrap(y0 + 1, periodY);

            double n00 = NoiseHash.GradientDot(wx0, wy0, this.Seed, fx, fy);
            double n10 = NoiseHash.GradientDot(wx1, wy0, this.Seed, fx - 1.0, fy);
            double n01 = NoiseHash.GradientDot(wx0, wy1, this.Seed, fx, fy - 1.0);
            double n11 = NoiseHash.GradientDot(wx1, wy1, this.Seed, fx - 1.0, fy - 1.0);

            return Normalise2 * Lerp(Lerp(n00, n10, u), Lerp(n01, n11, u), v);
        }

        /// <summary>A seamlessly tiling 3D sample. See <see cref="SampleTiled(double, double, int, int)"/>.</summary>
        public double SampleTiled(double x, double y, double z, int periodX, int periodY, int periodZ)
        {
            if (periodX <= 0) throw new ArgumentOutOfRangeException(nameof(periodX), "Tiling period must be positive.");
            if (periodY <= 0) throw new ArgumentOutOfRangeException(nameof(periodY), "Tiling period must be positive.");
            if (periodZ <= 0) throw new ArgumentOutOfRangeException(nameof(periodZ), "Tiling period must be positive.");

            int x0 = FastFloor(x), y0 = FastFloor(y), z0 = FastFloor(z);
            double fx = x - x0, fy = y - y0, fz = z - z0;
            double u = Fade(fx), v = Fade(fy), w = Fade(fz);

            int wx0 = Wrap(x0, periodX), wx1 = Wrap(x0 + 1, periodX);
            int wy0 = Wrap(y0, periodY), wy1 = Wrap(y0 + 1, periodY);
            int wz0 = Wrap(z0, periodZ), wz1 = Wrap(z0 + 1, periodZ);
            double gx = fx - 1.0, gy = fy - 1.0, gz = fz - 1.0;

            double n000 = NoiseHash.GradientDot(wx0, wy0, wz0, this.Seed, fx, fy, fz);
            double n100 = NoiseHash.GradientDot(wx1, wy0, wz0, this.Seed, gx, fy, fz);
            double n010 = NoiseHash.GradientDot(wx0, wy1, wz0, this.Seed, fx, gy, fz);
            double n110 = NoiseHash.GradientDot(wx1, wy1, wz0, this.Seed, gx, gy, fz);
            double n001 = NoiseHash.GradientDot(wx0, wy0, wz1, this.Seed, fx, fy, gz);
            double n101 = NoiseHash.GradientDot(wx1, wy0, wz1, this.Seed, gx, fy, gz);
            double n011 = NoiseHash.GradientDot(wx0, wy1, wz1, this.Seed, fx, gy, gz);
            double n111 = NoiseHash.GradientDot(wx1, wy1, wz1, this.Seed, gx, gy, gz);

            double a = Lerp(Lerp(n000, n100, u), Lerp(n010, n110, u), v);
            double b = Lerp(Lerp(n001, n101, u), Lerp(n011, n111, u), v);
            return Normalise3 * Lerp(a, b, w);
        }

        /// <summary>
        /// Samples the field <i>and</i> its analytic gradient — the exact slope, not a finite difference.
        /// </summary>
        /// <remarks>
        /// <para>Worth having for three reasons. <b>Normals for free</b>: a height field's surface normal is
        /// derived directly from the gradient, with no need to sample neighbours (three samples become one).
        /// <b>Erosion</b>: the cheapest convincing erosion term damps a terrain octave by how steep the
        /// terrain already is, which requires the slope. <b>Correctness</b>: finite differences are only as
        /// good as the step you pick, and a step too small drowns in floating-point cancellation.</para>
        /// <para><b>The product rule, in full.</b> Each corner term <c>g . d</c> depends on <c>x</c> in
        /// <i>two</i> ways, and both must be differentiated. The offset <c>d</c> moves with the sample point,
        /// contributing the corner's own gradient component; and the blend weight <c>u = Fade(fx)</c> moves
        /// too, contributing the difference between corners times <c>Fade'</c>. Dropping the first term is an
        /// easy mistake, and a quiet one: the derivative still looks plausible and still points roughly
        /// uphill, but it is wrong everywhere, and wrong by 100% at any lattice point (where every corner
        /// value is zero, so the surviving term vanishes). The test suite compares against a central finite
        /// difference precisely to catch that.</para>
        /// </remarks>
        public double SampleWithDerivative(double x, double y, out double dx, out double dy)
        {
            int x0 = FastFloor(x), y0 = FastFloor(y);
            double fx = x - x0, fy = y - y0;
            double u = Fade(fx), v = Fade(fy);
            double du = FadeDerivative(fx), dv = FadeDerivative(fy);

            int x1 = x0 + 1, y1 = y0 + 1;
            double ox = fx - 1.0, oy = fy - 1.0;

            NoiseHash.Gradient(x0, y0, this.Seed, out double g00x, out double g00y);
            NoiseHash.Gradient(x1, y0, this.Seed, out double g10x, out double g10y);
            NoiseHash.Gradient(x0, y1, this.Seed, out double g01x, out double g01y);
            NoiseHash.Gradient(x1, y1, this.Seed, out double g11x, out double g11y);

            double n00 = (g00x * fx) + (g00y * fy);
            double n10 = (g10x * ox) + (g10y * fy);
            double n01 = (g01x * fx) + (g01y * oy);
            double n11 = (g11x * ox) + (g11y * oy);

            double bottom = Lerp(n00, n10, u);
            double top = Lerp(n01, n11, u);
            double value = Lerp(bottom, top, v);

            // Term one: the corner gradients themselves, blended exactly as the values are.
            double gradX = Lerp(Lerp(g00x, g10x, u), Lerp(g01x, g11x, u), v);
            double gradY = Lerp(Lerp(g00y, g10y, u), Lerp(g01y, g11y, u), v);

            // Term two: the moving blend weights.
            dx = Normalise2 * (gradX + (du * (1.0 - v) * (n10 - n00)) + (du * v * (n11 - n01)));
            dy = Normalise2 * (gradY + (dv * (top - bottom)));
            return Normalise2 * value;
        }

        /// <summary>Euclidean modulo: always returns <c>[0, period)</c>, unlike <c>%</c> on negatives.</summary>
        private static int Wrap(int value, int period)
        {
            int m = value % period;
            return m < 0 ? m + period : m;
        }
    }
}
