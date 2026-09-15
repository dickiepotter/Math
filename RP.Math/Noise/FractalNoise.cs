namespace RP.Math.Noise
{
    using System;

    /// <summary>
    /// How the octaves of a <see cref="FractalNoise2"/> or <see cref="FractalNoise3"/> stack are combined.
    /// Each mode is a one-line change that produces a completely different kind of landscape.
    /// </summary>
    public enum FractalMode
    {
        /// <summary>
        /// Fractional Brownian motion: octaves summed as they come. The default, and the one that looks
        /// like ordinary rolling terrain, cloud and rock. Smooth everywhere, with detail at every scale.
        /// </summary>
        Brownian,

        /// <summary>
        /// Each octave is folded about zero (<c>|n|</c>) before summing, turning every zero-crossing into a
        /// crease. Reads as billowing cloud, spume, and puffy organic mass — bulges rather than slopes.
        /// </summary>
        Billow,

        /// <summary>
        /// Each octave is inverted after folding (<c>1 - |n|</c>), so the creases become <b>sharp ridges</b>
        /// instead of valleys. This is how mountain ranges with knife-edge crests and eroded gullies are
        /// made, and it is the single biggest visual upgrade over plain Brownian terrain.
        /// </summary>
        Ridged,

        /// <summary>
        /// Folded like <see cref="Billow"/> but left unsigned in <c>[0, 1]</c>. The classic turbulence term:
        /// fed into a sine or used to distort a coordinate, it produces marble, wood grain and flame.
        /// </summary>
        Turbulence,
    }

    /// <summary>
    /// Stacks many octaves of a 2D noise field into one — the step that turns a single smooth wave into
    /// something that reads as a real landscape.
    /// </summary>
    /// <remarks>
    /// <para><b>Why one octave is never enough.</b> A single sample of Perlin noise gives smooth, featureless
    /// swells: correct, and boring, because nature is not smooth at one scale. Real terrain has mountains,
    /// and hills on the mountains, and boulders on the hills, and grit on the boulders — the same statistical
    /// structure repeating at every magnification. That property is called <i>self-similarity</i>, and the
    /// way to synthesise it is to add the same field to itself several times, each copy at a higher frequency
    /// and a lower amplitude.</para>
    ///
    /// <list type="bullet">
    ///   <item><description><b>Octaves</b> — how many copies. Each roughly doubles the cost, and each adds
    ///   detail at half the previous scale. Past the point where one octave is smaller than a single voxel
    ///   (or a single pixel), further octaves cost time and change nothing — a cheap and common waste.</description></item>
    ///   <item><description><b>Lacunarity</b> — the frequency multiplier between octaves, conventionally 2.
    ///   Exactly 2 can align features across octaves and produce faint repetition; something irrational-ish
    ///   like 2.01 or 1.97 breaks that up for free.</description></item>
    ///   <item><description><b>Gain</b> — the amplitude multiplier between octaves, conventionally 0.5.
    ///   This is the roughness dial: below 0.5 gives smooth, weathered ground; above 0.5 gives jagged,
    ///   noisy, young rock. It is the parameter to expose to a level designer.</description></item>
    /// </list>
    ///
    /// <para><b>Normalisation.</b> The octave amplitudes are summed and divided out, so the result stays in
    /// the source field's range regardless of octave count. Without this, adding an octave would quietly
    /// change the scale of the whole world and every threshold tuned against it.</para>
    ///
    /// <para>The decorator takes any <see cref="INoise2"/>, so it stacks Perlin, simplex, value or cellular
    /// noise — or another fractal stack, which is a legitimate and occasionally wonderful thing to do.</para>
    /// </remarks>
    public sealed class FractalNoise2 : INoise2
    {
        private readonly INoise2 source;
        private readonly double normaliser;

        /// <summary>Builds a fractal stack over <paramref name="source"/>.</summary>
        /// <param name="source">The field to stack. Must not be null.</param>
        /// <param name="octaves">How many copies to sum (at least 1).</param>
        /// <param name="frequency">The first octave's frequency — how many features per world unit.
        /// Think of it as the reciprocal of feature size: 1/200 gives features about 200 units across.</param>
        /// <param name="lacunarity">Frequency multiplier per octave.</param>
        /// <param name="gain">Amplitude multiplier per octave.</param>
        /// <param name="mode">How the octaves are folded before summing.</param>
        public FractalNoise2(
            INoise2 source,
            int octaves = 4,
            double frequency = 1.0,
            double lacunarity = 2.0,
            double gain = 0.5,
            FractalMode mode = FractalMode.Brownian)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (octaves < 1) throw new ArgumentOutOfRangeException(nameof(octaves), "At least one octave is required.");

            this.source = source;
            this.Octaves = octaves;
            this.Frequency = frequency;
            this.Lacunarity = lacunarity;
            this.Gain = gain;
            this.Mode = mode;
            this.normaliser = ComputeNormaliser(octaves, gain);
        }

        /// <summary>How many copies of the source field are summed.</summary>
        public int Octaves { get; }

        /// <summary>The first octave's frequency.</summary>
        public double Frequency { get; }

        /// <summary>The frequency multiplier applied between octaves.</summary>
        public double Lacunarity { get; }

        /// <summary>The amplitude multiplier applied between octaves — the roughness dial.</summary>
        public double Gain { get; }

        /// <summary>How octaves are folded before summing.</summary>
        public FractalMode Mode { get; }

        /// <summary>
        /// The reciprocal of the summed octave amplitudes — a geometric series, so it has a closed form and
        /// need not be accumulated per sample.
        /// </summary>
        internal static double ComputeNormaliser(int octaves, double gain)
        {
            double total = 0.0, amplitude = 1.0;
            for (int i = 0; i < octaves; i++)
            {
                total += amplitude;
                amplitude *= gain;
            }

            return total > 0.0 ? 1.0 / total : 1.0;
        }

        /// <summary>Applies the fold for a mode to one octave's raw value.</summary>
        internal static double Fold(double n, FractalMode mode)
        {
            switch (mode)
            {
                // |n| maps [-1,1] to [0,1]; rescale to [-1,1] so the sum keeps a zero mean and the stack
                // does not drift upward with every octave added.
                case FractalMode.Billow:
                    return (2.0 * System.Math.Abs(n)) - 1.0;

                // 1 - |n| puts the peak where the source crossed zero. Squaring sharpens the crest into a
                // true ridge rather than a rounded hump; the rescale again restores a zero mean.
                case FractalMode.Ridged:
                {
                    double r = 1.0 - System.Math.Abs(n);
                    return (2.0 * r * r) - 1.0;
                }

                case FractalMode.Turbulence:
                    return System.Math.Abs(n);

                default:
                    return n;
            }
        }

        /// <inheritdoc />
        public double Sample(double x, double y)
        {
            double sum = 0.0, amplitude = 1.0, frequency = this.Frequency;

            for (int i = 0; i < this.Octaves; i++)
            {
                sum += Fold(this.source.Sample(x * frequency, y * frequency), this.Mode) * amplitude;
                frequency *= this.Lacunarity;
                amplitude *= this.Gain;
            }

            return sum * this.normaliser;
        }

        /// <summary>
        /// A <b>ridged multifractal</b> sample: like <see cref="FractalMode.Ridged"/>, but each octave is
        /// additionally damped by how strong the previous one was.
        /// </summary>
        /// <remarks>
        /// <para>This is the difference between mountains that look drawn and mountains that look eroded.
        /// In a plain stack, fine detail is applied at full strength everywhere — including down in the
        /// valleys, where in reality sediment has filled and smoothed everything. Feeding the previous
        /// octave's value forward as a weight suppresses detail in low areas and concentrates it on the
        /// high ridges, which is exactly what weathering does: peaks stay sharp, valleys silt up.</para>
        /// <para>The result is not normalised to a tight range the way <see cref="Sample(double, double)"/>
        /// is — multifractals are inhomogeneous by construction, which is the point — so expect roughly
        /// <c>[0, 1]</c> and shape it with <see cref="NoiseCurves"/> to taste.</para>
        /// </remarks>
        /// <param name="x">Sample coordinate.</param>
        /// <param name="y">Sample coordinate.</param>
        /// <param name="ridgeOffset">Where the fold is centred. Near 1 gives tall isolated peaks; lower
        /// values give broader massifs.</param>
        /// <param name="weightFalloff">How strongly the previous octave damps the next. 0 disables the
        /// erosion term entirely; around 1 is a good starting point.</param>
        public double SampleRidgedMultifractal(double x, double y, double ridgeOffset = 1.0, double weightFalloff = 1.0)
        {
            double sum = 0.0, amplitude = 1.0, frequency = this.Frequency, weight = 1.0;

            for (int i = 0; i < this.Octaves; i++)
            {
                double signal = ridgeOffset - System.Math.Abs(this.source.Sample(x * frequency, y * frequency));
                signal *= signal;           // sharpen the crest
                signal *= weight;           // damp where the previous octave was weak (the valleys)

                sum += signal * amplitude;

                // The next octave's weight is this octave's strength, clamped so the stack cannot run away.
                weight = signal * weightFalloff;
                if (weight > 1.0) weight = 1.0;
                else if (weight < 0.0) weight = 0.0;

                frequency *= this.Lacunarity;
                amplitude *= this.Gain;
            }

            return sum * this.normaliser;
        }
    }

    /// <summary>
    /// Stacks many octaves of a 3D noise field into one. The volumetric counterpart of
    /// <see cref="FractalNoise2"/>, and the field that carves caves, overhangs and floating rock — anything
    /// whose shape is genuinely three-dimensional rather than a height above a plane.
    /// </summary>
    /// <remarks>See <see cref="FractalNoise2"/> for what octaves, lacunarity and gain do; the parameters
    /// mean exactly the same here, and cost one dimension more per sample.</remarks>
    public sealed class FractalNoise3 : INoise3
    {
        private readonly INoise3 source;
        private readonly double normaliser;

        /// <summary>Builds a fractal stack over <paramref name="source"/>.</summary>
        public FractalNoise3(
            INoise3 source,
            int octaves = 4,
            double frequency = 1.0,
            double lacunarity = 2.0,
            double gain = 0.5,
            FractalMode mode = FractalMode.Brownian)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (octaves < 1) throw new ArgumentOutOfRangeException(nameof(octaves), "At least one octave is required.");

            this.source = source;
            this.Octaves = octaves;
            this.Frequency = frequency;
            this.Lacunarity = lacunarity;
            this.Gain = gain;
            this.Mode = mode;
            this.normaliser = FractalNoise2.ComputeNormaliser(octaves, gain);
        }

        /// <summary>How many copies of the source field are summed.</summary>
        public int Octaves { get; }

        /// <summary>The first octave's frequency.</summary>
        public double Frequency { get; }

        /// <summary>The frequency multiplier applied between octaves.</summary>
        public double Lacunarity { get; }

        /// <summary>The amplitude multiplier applied between octaves.</summary>
        public double Gain { get; }

        /// <summary>How octaves are folded before summing.</summary>
        public FractalMode Mode { get; }

        /// <inheritdoc />
        public double Sample(double x, double y, double z)
        {
            double sum = 0.0, amplitude = 1.0, frequency = this.Frequency;

            for (int i = 0; i < this.Octaves; i++)
            {
                double n = this.source.Sample(x * frequency, y * frequency, z * frequency);
                sum += FractalNoise2.Fold(n, this.Mode) * amplitude;
                frequency *= this.Lacunarity;
                amplitude *= this.Gain;
            }

            return sum * this.normaliser;
        }

        /// <summary>
        /// Samples the stack with a <b>vertical squash</b>: the Y axis is sampled at a different frequency
        /// from X and Z.
        /// </summary>
        /// <remarks>
        /// Not a convenience — a correction. Isotropic 3D noise makes caves as tall as they are wide, which
        /// reads as a field of spherical bubbles, nothing like a real cave system. Squashing the vertical
        /// axis (sampling Y at a higher frequency, so it varies faster) flattens those spheres into
        /// wandering horizontal passages that a player can actually walk along. The same trick stretches
        /// ore veins into seams and strata into layers.
        /// </remarks>
        /// <param name="verticalScale">Multiplier on the Y frequency. Above 1 flattens features into
        /// horizontal sheets; below 1 stretches them into vertical shafts and chimneys.</param>
        public double SampleSquashed(double x, double y, double z, double verticalScale)
        {
            double sum = 0.0, amplitude = 1.0, frequency = this.Frequency;

            for (int i = 0; i < this.Octaves; i++)
            {
                double n = this.source.Sample(x * frequency, y * frequency * verticalScale, z * frequency);
                sum += FractalNoise2.Fold(n, this.Mode) * amplitude;
                frequency *= this.Lacunarity;
                amplitude *= this.Gain;
            }

            return sum * this.normaliser;
        }
    }
}
