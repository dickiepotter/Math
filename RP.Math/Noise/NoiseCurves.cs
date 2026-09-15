namespace RP.Math.Noise
{
    using System;

    /// <summary>
    /// Shaping curves: the small remapping functions applied <i>after</i> a noise field is sampled, to turn
    /// its raw statistical distribution into the one the world actually wants.
    /// </summary>
    /// <remarks>
    /// <para><b>Why sampling is only half the job.</b> Coherent noise returns a bell-shaped distribution —
    /// most samples cluster near the middle, extremes are rare. Left alone, a height map built from it gives
    /// a world that is almost entirely gently-rolling mid-altitude ground, with mountains and trenches too
    /// scarce to find. That is statistically correct and dramatically useless. These curves redistribute the
    /// values: pushing them toward the extremes for a world of plateaux and cliffs, toward the middle for
    /// plains, or into discrete bands for terraces.</para>
    ///
    /// <para><b>Shape the curve, not the amplitude.</b> The instinct on seeing flat terrain is to multiply
    /// the noise by a larger number. That does not work — it scales the rare extremes past the world's
    /// ceiling where they clip into flat-topped mesas, while the common mid-range, which is what the player
    /// actually walks on, stays just as flat. Changing the distribution is what changes the landscape.</para>
    ///
    /// <para>Every function here is pure, allocation-free, and defined for the whole real line (inputs
    /// outside the nominal domain are clamped rather than left to produce nonsense).</para>
    /// </remarks>
    public static class NoiseCurves
    {
        /// <summary>Clamps <paramref name="v"/> into <c>[min, max]</c>. Present because the
        /// <c>netstandard2.0</c> target predates <c>System.Math.Clamp</c>.</summary>
        public static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);

        /// <summary>Clamps <paramref name="v"/> into <c>[0, 1]</c>.</summary>
        public static double Saturate(double v) => v < 0.0 ? 0.0 : (v > 1.0 ? 1.0 : v);

        /// <summary>Maps <c>[-1, 1]</c> onto <c>[0, 1]</c> — the conversion between the coherent fields'
        /// signed range and the unit range most thresholds are expressed in.</summary>
        public static double ToUnit(double signed) => (signed + 1.0) * 0.5;

        /// <summary>Maps <c>[0, 1]</c> onto <c>[-1, 1]</c>.</summary>
        public static double ToSigned(double unit) => (unit * 2.0) - 1.0;

        /// <summary>
        /// Rescales <paramref name="v"/> from one interval to another, without clamping.
        /// </summary>
        /// <param name="v">The value to rescale.</param>
        /// <param name="fromMin">Start of the source interval.</param>
        /// <param name="fromMax">End of the source interval.</param>
        /// <param name="toMin">Start of the destination interval.</param>
        /// <param name="toMax">End of the destination interval.</param>
        public static double Remap(double v, double fromMin, double fromMax, double toMin, double toMax)
        {
            double span = fromMax - fromMin;
            if (span == 0.0) return toMin;
            return toMin + ((v - fromMin) / span * (toMax - toMin));
        }

        /// <summary>
        /// Hermite smoothstep: 0 below <paramref name="edge0"/>, 1 above <paramref name="edge1"/>, and a
        /// smooth S-curve between, with zero slope at both ends.
        /// </summary>
        /// <remarks>The everyday tool for blending one thing into another — a biome into its neighbour, fog
        /// into clear air, one material into the next — without the visible crease a linear ramp leaves at
        /// each end.</remarks>
        public static double SmoothStep(double edge0, double edge1, double v)
        {
            if (edge0 == edge1) return v < edge0 ? 0.0 : 1.0;
            double t = Saturate((v - edge0) / (edge1 - edge0));
            return t * t * (3.0 - (2.0 * t));
        }

        /// <summary>
        /// Perlin's quintic smootherstep: like <see cref="SmoothStep"/>, but with zero <i>second</i>
        /// derivative at each end too. Use it when the blend drives something whose curvature is visible —
        /// a surface normal, a camera path — where plain smoothstep leaves a faint crease.
        /// </summary>
        public static double SmootherStep(double edge0, double edge1, double v)
        {
            if (edge0 == edge1) return v < edge0 ? 0.0 : 1.0;
            double t = Saturate((v - edge0) / (edge1 - edge0));
            return t * t * t * ((t * ((t * 6.0) - 15.0)) + 10.0);
        }

        /// <summary>
        /// Schlick's bias: pushes values in <c>[0, 1]</c> toward 0 or toward 1 without moving the endpoints.
        /// </summary>
        /// <param name="v">A value in <c>[0, 1]</c>.</param>
        /// <param name="bias">0.5 leaves the value untouched; below 0.5 darkens (pushes toward 0); above
        /// 0.5 lightens (pushes toward 1).</param>
        /// <remarks>The dial for "how much of the world is ocean?" — bias a height field down and the sea
        /// rises across the whole map without any coastline losing its shape.</remarks>
        public static double Bias(double v, double bias)
        {
            v = Saturate(v);
            bias = Clamp(bias, 0.0001, 0.9999);
            return v / ((((1.0 / bias) - 2.0) * (1.0 - v)) + 1.0);
        }

        /// <summary>
        /// Schlick's gain: increases or decreases contrast about the midpoint, leaving 0, 0.5 and 1 fixed.
        /// </summary>
        /// <param name="v">A value in <c>[0, 1]</c>.</param>
        /// <param name="gain">0.5 leaves the value untouched; above 0.5 pushes values away from the middle
        /// (more cliffs and flats, fewer slopes); below 0.5 pulls them toward it (gentler, rolling).</param>
        public static double Gain(double v, double gain)
        {
            v = Saturate(v);
            return v < 0.5
                ? Bias(v * 2.0, gain) * 0.5
                : 1.0 - (Bias(2.0 - (v * 2.0), gain) * 0.5);
        }

        /// <summary>
        /// Quantises a value into <paramref name="steps"/> flat bands separated by ramps — terracing.
        /// </summary>
        /// <param name="v">A value in <c>[0, 1]</c>.</param>
        /// <param name="steps">How many bands (at least 1).</param>
        /// <param name="sharpness">How abrupt the step between bands is, in <c>[0, 1]</c>. At 0 the curve is
        /// unchanged; at 1 it is a hard staircase with vertical risers.</param>
        /// <remarks>Terraced height fields give rice-paddy hillsides, sedimentary cliff bands and the stepped
        /// mesas of arid country — structure that no amount of extra octaves produces, because the effect is
        /// not about detail but about a discontinuous <i>distribution</i>.</remarks>
        public static double Terrace(double v, int steps, double sharpness = 1.0)
        {
            if (steps < 1) throw new ArgumentOutOfRangeException(nameof(steps), "At least one step is required.");

            v = Saturate(v);
            sharpness = Saturate(sharpness);

            double scaled = v * steps;
            double band = System.Math.Floor(scaled);
            double within = scaled - band;

            // Blend between the raw ramp and a sharpened one, so `sharpness` is a continuous dial rather
            // than a switch between two looks.
            double sharpened = within * within * (3.0 - (2.0 * within)); // smoothstep the riser
            double risen = (within * (1.0 - sharpness)) + (sharpened * sharpness);
            return Saturate((band + risen) / steps);
        }

        /// <summary>
        /// Raises a value to a power while keeping it in <c>[0, 1]</c> — the simplest contrast control.
        /// Exponents above 1 push toward 0 (rarer highs: isolated peaks); below 1 push toward 1.
        /// </summary>
        public static double Power(double v, double exponent) => System.Math.Pow(Saturate(v), exponent);

        /// <summary>
        /// A <b>continentalness</b> curve: flattens the middle of the range into broad plains while leaving
        /// the extremes free to rise and fall.
        /// </summary>
        /// <remarks>
        /// <para>This is the shape that makes a world feel like a world rather than a crumpled sheet. Left
        /// raw, noise spends most of its time in the middle of its range, so terrain slopes constantly and
        /// there is nowhere flat to build, farm or fight. Compressing the middle band buys large tracts of
        /// walkable ground, and because the tails are untouched, the mountains and the ocean trenches keep
        /// their full height — you gain plains without losing drama.</para>
        /// </remarks>
        /// <param name="signed">A value in <c>[-1, 1]</c>.</param>
        /// <param name="flatness">How hard the middle band is compressed, in <c>[0, 1)</c>. At 0 the curve
        /// is the identity; approaching 1 gives a world of plateaux separated by cliffs.</param>
        /// <param name="plainsWidth">How much of the range counts as "middle", in <c>(0, 1)</c>. At 0.5,
        /// the central half of the noise range becomes the plains.</param>
        public static double Continentalness(double signed, double flatness = 0.5, double plainsWidth = 0.5)
        {
            signed = Clamp(signed, -1.0, 1.0);
            flatness = Clamp(flatness, 0.0, 0.99);
            double knee = Clamp(plainsWidth, 0.01, 0.99);

            // Odd symmetry: shape the magnitude, then restore the sign. Sea level (0) must stay put, or
            // every coastline in the world moves when this dial is touched.
            double sign = signed < 0.0 ? -1.0 : 1.0;
            double magnitude = System.Math.Abs(signed);

            // Everything below the knee is squeezed into `low`, leaving [low, 1] for the tails — so the
            // peaks keep their full height while the plains lose their slope.
            double low = knee * (1.0 - flatness);

            double shaped = magnitude <= knee
                ? low * (magnitude / knee)
                : low + ((1.0 - low) * ((magnitude - knee) / (1.0 - knee)));

            return sign * shaped;
        }

        /// <summary>
        /// Folds a value into a <b>ridge</b>: near 1 where the input crossed zero, falling away on both
        /// sides. The per-sample form of what <see cref="FractalMode.Ridged"/> does per octave, for use when
        /// a field has already been built and only its final shape wants sharpening.
        /// </summary>
        /// <param name="signed">A value in <c>[-1, 1]</c>.</param>
        /// <param name="sharpness">Exponent on the fold; 1 gives a smooth crest, higher values a knife edge.</param>
        public static double Ridge(double signed, double sharpness = 2.0)
        {
            double r = 1.0 - System.Math.Abs(Clamp(signed, -1.0, 1.0));
            return System.Math.Pow(r, sharpness);
        }

        /// <summary>
        /// A smooth <i>maximum</i> of two values: like <c>Math.Max</c>, but the join is rounded over a width
        /// of <paramref name="smoothing"/> instead of forming a crease.
        /// </summary>
        /// <remarks>
        /// The tool for combining terrain features without leaving seams. Overlay a mountain field on a
        /// plains field with a hard max and the boundary is a visible fold line running across the
        /// landscape; blend them with this and the mountains rise out of the plain the way real ones do.
        /// The same operation underlies constructive solid geometry in signed-distance fields.
        /// </remarks>
        public static double SmoothMax(double a, double b, double smoothing)
        {
            if (smoothing <= 0.0) return a > b ? a : b;
            double h = Saturate(0.5 + ((a - b) / (2.0 * smoothing)));
            return (b * (1.0 - h)) + (a * h) + (smoothing * h * (1.0 - h));
        }

        /// <summary>A smooth <i>minimum</i> of two values. See <see cref="SmoothMax"/>.</summary>
        public static double SmoothMin(double a, double b, double smoothing)
        {
            if (smoothing <= 0.0) return a < b ? a : b;
            double h = Saturate(0.5 - ((a - b) / (2.0 * smoothing)));
            return (b * (1.0 - h)) + (a * h) - (smoothing * h * (1.0 - h));
        }

        /// <summary>
        /// Evaluates a piecewise-linear curve defined by control points, interpolating between them.
        /// </summary>
        /// <remarks>
        /// The escape hatch for shapes no closed-form curve expresses: an author-tuned mapping from noise
        /// value to terrain height, or from altitude to temperature, given as a handful of points and read
        /// off directly. <paramref name="xs"/> must be sorted ascending; inputs beyond either end clamp to
        /// the nearest control value rather than extrapolating into nonsense.
        /// </remarks>
        /// <param name="xs">Control-point inputs, sorted ascending.</param>
        /// <param name="ys">Control-point outputs, the same length as <paramref name="xs"/>.</param>
        /// <param name="v">The value to look up.</param>
        public static double Spline(double[] xs, double[] ys, double v)
        {
            if (xs == null) throw new ArgumentNullException(nameof(xs));
            if (ys == null) throw new ArgumentNullException(nameof(ys));
            if (xs.Length == 0) throw new ArgumentException("At least one control point is required.", nameof(xs));
            if (xs.Length != ys.Length) throw new ArgumentException("Control-point arrays must be the same length.", nameof(ys));

            if (v <= xs[0]) return ys[0];
            int last = xs.Length - 1;
            if (v >= xs[last]) return ys[last];

            // Binary search for the bracketing interval: control-point sets are usually short, but this is
            // sampled per voxel, and a linear scan at that rate is measurable.
            int lo = 0, hi = last;
            while (hi - lo > 1)
            {
                int mid = (lo + hi) >> 1;
                if (xs[mid] <= v) { lo = mid; } else { hi = mid; }
            }

            double span = xs[hi] - xs[lo];
            if (span == 0.0) return ys[lo];
            double t = (v - xs[lo]) / span;
            return ys[lo] + ((ys[hi] - ys[lo]) * t);
        }
    }
}
