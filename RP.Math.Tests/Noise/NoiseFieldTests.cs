namespace RP.Math.Tests.Noise
{
    using System;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RP.Math.Noise;

    /// <summary>
    /// The properties every noise field must hold, checked by sampling rather than by inspection.
    /// </summary>
    /// <remarks>
    /// <para>Noise is unusually hard to test: there is no "right answer" to compare against, because the
    /// output is supposed to look arbitrary. What can be pinned down is its <i>behaviour</i> — and those
    /// properties are precisely the ones whose violation produces the classic bugs:</para>
    /// <list type="bullet">
    ///   <item><description><b>Determinism</b> — or a streamed world disagrees with itself at chunk
    ///   boundaries, and two players see different terrain.</description></item>
    ///   <item><description><b>Range</b> — or every threshold tuned against the field (sea level, ore
    ///   cutoff, cave density) silently means something different.</description></item>
    ///   <item><description><b>Continuity</b> — or the terrain has cliffs where nothing asked for one.</description></item>
    ///   <item><description><b>Symmetry about the origin</b> — the <c>FastFloor</c> trap: truncation
    ///   instead of flooring mirrors the entire world at x = 0, which nobody notices until they walk
    ///   west.</description></item>
    /// </list>
    /// </remarks>
    [TestClass]
    public sealed class NoiseFieldTests
    {
        private const int Seed = 20260915;

        // A deliberately awkward sampling walk: irrational-ish steps so samples never land on lattice
        // points, and a span crossing the origin so negative coordinates are covered.
        private static void ForEachSample2(Action<double, double> body, int count = 20000)
        {
            for (int i = 0; i < count; i++)
            {
                double x = ((i * 0.37219) % 400.0) - 200.0;
                double y = ((i * 0.61803) % 400.0) - 200.0;
                body(x, y);
            }
        }

        private static void ForEachSample3(Action<double, double, double> body, int count = 20000)
        {
            for (int i = 0; i < count; i++)
            {
                double x = ((i * 0.37219) % 200.0) - 100.0;
                double y = ((i * 0.61803) % 200.0) - 100.0;
                double z = ((i * 0.13717) % 200.0) - 100.0;
                body(x, y, z);
            }
        }

        // ---- Determinism ---------------------------------------------------------------------------

        [TestMethod]
        public void AllFields_SameSeedAndCoordinate_ReturnTheSameValue()
        {
            var a = new PerlinNoise(Seed);
            var b = new PerlinNoise(Seed);
            var sa = new SimplexNoise(Seed);
            var sb = new SimplexNoise(Seed);
            var va = new ValueNoise(Seed);
            var vb = new ValueNoise(Seed);
            var wa = new WorleyNoise(Seed);
            var wb = new WorleyNoise(Seed);

            ForEachSample2(
                (x, y) =>
                {
                    a.Sample(x, y).Should().Be(b.Sample(x, y));
                    sa.Sample(x, y).Should().Be(sb.Sample(x, y));
                    va.Sample(x, y).Should().Be(vb.Sample(x, y));
                    wa.Sample(x, y).Should().Be(wb.Sample(x, y));
                },
                count: 500);
        }

        [TestMethod]
        public void AllFields_DifferentSeeds_ProduceDifferentFields()
        {
            var a = new PerlinNoise(1);
            var b = new PerlinNoise(2);

            int differences = 0;
            ForEachSample2((x, y) => { if (a.Sample(x, y) != b.Sample(x, y)) differences++; }, count: 500);

            // Not "every sample differs" — two independent fields will coincide occasionally by chance.
            differences.Should().BeGreaterThan(450);
        }

        [TestMethod]
        public void Derive_ProducesIndependentSeeds()
        {
            int a = NoiseHash.Derive(Seed, 1);
            int b = NoiseHash.Derive(Seed, 2);

            a.Should().NotBe(b);
            a.Should().NotBe(Seed);
            NoiseHash.Derive(Seed, 1).Should().Be(a, "derivation must itself be deterministic");
        }

        // ---- Range ---------------------------------------------------------------------------------

        [TestMethod]
        public void Perlin2D_StaysWithinTheDocumentedRange()
        {
            var n = new PerlinNoise(Seed);
            ForEachSample2((x, y) => n.Sample(x, y).Should().BeInRange(-1.0, 1.0));
        }

        [TestMethod]
        public void Perlin3D_StaysWithinTheDocumentedRange()
        {
            var n = new PerlinNoise(Seed);
            ForEachSample3((x, y, z) => n.Sample(x, y, z).Should().BeInRange(-1.0, 1.0));
        }

        [TestMethod]
        public void Perlin4D_StaysWithinTheDocumentedRange()
        {
            var n = new PerlinNoise(Seed);
            ForEachSample3((x, y, z) => n.Sample(x, y, z, x - y).Should().BeInRange(-1.0, 1.0));
        }

        [TestMethod]
        public void Simplex2D_StaysWithinTheDocumentedRange()
        {
            // This is the test that pins SimplexNoise.Scale2. If that normalisation constant ever drifts
            // out of step with the gradient table in NoiseHash, this fails rather than quietly rescaling
            // every threshold in every consuming world.
            var n = new SimplexNoise(Seed);
            ForEachSample2((x, y) => n.Sample(x, y).Should().BeInRange(-1.0, 1.0));
        }

        [TestMethod]
        public void Simplex3D_StaysWithinTheDocumentedRange()
        {
            var n = new SimplexNoise(Seed);
            ForEachSample3((x, y, z) => n.Sample(x, y, z).Should().BeInRange(-1.0, 1.0));
        }

        [TestMethod]
        public void Simplex4D_StaysWithinTheDocumentedRange()
        {
            var n = new SimplexNoise(Seed);
            ForEachSample3((x, y, z) => n.Sample(x, y, z, (x * 0.5) - z).Should().BeInRange(-1.0, 1.0));
        }

        [TestMethod]
        public void ValueNoise_StaysWithinTheDocumentedRange()
        {
            var n = new ValueNoise(Seed);
            ForEachSample2((x, y) => n.Sample(x, y).Should().BeInRange(-1.0, 1.0));
            ForEachSample3((x, y, z) => n.Sample(x, y, z).Should().BeInRange(-1.0, 1.0));
        }

        [TestMethod]
        public void CoherentFields_ActuallyUseTheirRange()
        {
            // A field clamped flat near zero would pass the range test above while being useless. Assert it
            // reaches out toward the ends too.
            var n = new SimplexNoise(Seed);
            double min = double.MaxValue, max = double.MinValue;
            ForEachSample2(
                (x, y) =>
                {
                    double v = n.Sample(x, y);
                    if (v < min) min = v;
                    if (v > max) max = v;
                });

            min.Should().BeLessThan(-0.7);
            max.Should().BeGreaterThan(0.7);
        }

        [TestMethod]
        public void CoherentFields_HaveNoDcBias()
        {
            // A field whose mean drifts from zero shifts every threshold tuned against it. Perlin and
            // simplex are both symmetric by construction; this catches a broken gradient table.
            var n = new PerlinNoise(Seed);
            double sum = 0;
            int count = 0;
            ForEachSample2((x, y) => { sum += n.Sample(x, y); count++; });

            (sum / count).Should().BeApproximately(0.0, 0.05);
        }

        // ---- Continuity and the negative-coordinate trap -------------------------------------------

        [TestMethod]
        public void CoherentFields_AreContinuous()
        {
            // Coherent noise is Lipschitz-bounded: a small step in the input cannot produce a large jump in
            // the output. A broken floor or a mis-indexed gradient shows up here as a cliff.
            var n = new PerlinNoise(Seed);
            const double Step = 1e-4;

            ForEachSample2(
                (x, y) =>
                {
                    double here = n.Sample(x, y);
                    System.Math.Abs(n.Sample(x + Step, y) - here).Should().BeLessThan(0.01);
                    System.Math.Abs(n.Sample(x, y + Step) - here).Should().BeLessThan(0.01);
                },
                count: 2000);
        }

        [TestMethod]
        public void Fields_AreContinuousAcrossTheOrigin()
        {
            // The FastFloor trap in one test: truncation toward zero makes the cell containing -0.5 read as
            // cell 0, which duplicates the field either side of the axis and leaves a seam exactly on it.
            var n = new PerlinNoise(Seed);

            for (double t = -0.01; t < 0.01; t += 0.0005)
            {
                double a = n.Sample(t, 0.3);
                double b = n.Sample(t + 0.0005, 0.3);
                System.Math.Abs(b - a).Should().BeLessThan(0.05);
            }
        }

        [TestMethod]
        public void NegativeCoordinates_AreNotAMirrorOfPositiveOnes()
        {
            var n = new PerlinNoise(Seed);
            const int Samples = 500;
            int mirrored = 0;

            for (int i = 1; i <= Samples; i++)
            {
                // A step that never lands on an integer: at a lattice point the field is zero on *both*
                // sides by construction, which would read as a mirror while proving nothing.
                double x = i * 0.3719;
                if (System.Math.Abs(n.Sample(x, 1.5) - n.Sample(-x, 1.5)) < 1e-12) mirrored++;
            }

            // Not "never" — with eight gradient directions per corner and four corners, a cell whose
            // gradients happen to be the mirror image of its opposite number turns up about once every
            // four thousand cells, and every sample inside that one cell then matches. Truncation instead
            // of flooring would mirror *everything*, so a small tolerance separates the two cases cleanly
            // while leaving the test insensitive to which seed it runs under.
            mirrored.Should().BeLessThan(Samples / 20);
        }

        // ---- Perlin-specific structure --------------------------------------------------------------

        [TestMethod]
        public void Perlin_IsZeroAtEveryLatticePoint()
        {
            // The defining property of gradient noise: each corner contributes gradient . offset, and the
            // offset from a corner to itself is zero. If this fails, the gradient indexing is wrong.
            var n = new PerlinNoise(Seed);

            for (int x = -8; x <= 8; x++)
            {
                for (int y = -8; y <= 8; y++)
                {
                    n.Sample(x, y).Should().BeApproximately(0.0, 1e-12);
                    n.Sample(x, y, 3).Should().BeApproximately(0.0, 1e-12);
                }
            }
        }

        [TestMethod]
        public void PerlinTiled_RepeatsExactly()
        {
            var n = new PerlinNoise(Seed);
            const int Period = 8;

            for (double x = 0.0; x < Period; x += 0.31)
            {
                for (double y = 0.0; y < Period; y += 0.29)
                {
                    double here = n.SampleTiled(x, y, Period, Period);
                    n.SampleTiled(x + Period, y, Period, Period).Should().BeApproximately(here, 1e-12);
                    n.SampleTiled(x, y + Period, Period, Period).Should().BeApproximately(here, 1e-12);
                    n.SampleTiled(x + (3 * Period), y - (2 * Period), Period, Period).Should().BeApproximately(here, 1e-12);
                }
            }
        }

        [TestMethod]
        public void PerlinTiled_RejectsNonPositivePeriods()
        {
            var n = new PerlinNoise(Seed);
            Action act = () => n.SampleTiled(1, 1, 0, 4);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void PerlinDerivative_MatchesAFiniteDifference()
        {
            var n = new PerlinNoise(Seed);
            const double H = 1e-6;

            for (int i = 0; i < 200; i++)
            {
                double x = (i * 0.413) - 40.0;
                double y = (i * 0.277) - 25.0;

                double value = n.SampleWithDerivative(x, y, out double dx, out double dy);
                value.Should().BeApproximately(n.Sample(x, y), 1e-12);

                double fdX = (n.Sample(x + H, y) - n.Sample(x - H, y)) / (2 * H);
                double fdY = (n.Sample(x, y + H) - n.Sample(x, y - H)) / (2 * H);

                dx.Should().BeApproximately(fdX, 1e-4);
                dy.Should().BeApproximately(fdY, 1e-4);
            }
        }

        [TestMethod]
        public void Fade_HasZeroSlopeAtBothEnds()
        {
            PerlinNoise.Fade(0.0).Should().Be(0.0);
            PerlinNoise.Fade(1.0).Should().Be(1.0);
            PerlinNoise.Fade(0.5).Should().BeApproximately(0.5, 1e-12);
            PerlinNoise.FadeDerivative(0.0).Should().BeApproximately(0.0, 1e-12);
            PerlinNoise.FadeDerivative(1.0).Should().BeApproximately(0.0, 1e-12);
        }

        // ---- Simplex-specific -----------------------------------------------------------------------

        [TestMethod]
        public void SimplexSeamless_WrapsInBothAxes()
        {
            var n = new SimplexNoise(Seed);

            for (double u = 0.0; u < 1.0; u += 0.07)
            {
                for (double v = 0.0; v < 1.0; v += 0.11)
                {
                    double here = n.SampleSeamless(u, v, detail: 4.0);
                    n.SampleSeamless(u + 1.0, v, detail: 4.0).Should().BeApproximately(here, 1e-9);
                    n.SampleSeamless(u, v + 1.0, detail: 4.0).Should().BeApproximately(here, 1e-9);
                }
            }
        }

        // ---- Cellular -------------------------------------------------------------------------------

        [TestMethod]
        public void Worley_F1IsNeverGreaterThanF2()
        {
            var w = new WorleyNoise(Seed);
            ForEachSample2((x, y) =>
            {
                CellularSample s = w.SampleCellular(x, y);
                s.F1.Should().BeLessThanOrEqualTo(s.F2);
                s.Edge.Should().BeGreaterThanOrEqualTo(0.0);
            });
        }

        [TestMethod]
        public void Worley_CellIdentityIsStableWithinACell()
        {
            // The property biome partitioning depends on: every point nearest the same feature point must
            // report the same cell, so a region has one identity rather than flickering along its interior.
            var w = new WorleyNoise(Seed, jitter: 0.0); // no jitter: cells are exactly the unit grid
            CellularSample a = w.SampleCellular(4.2, 7.3);
            CellularSample b = w.SampleCellular(4.8, 7.6);

            b.CellHash.Should().Be(a.CellHash);
            b.CellX.Should().Be(a.CellX);
            b.CellY.Should().Be(a.CellY);
        }

        [TestMethod]
        public void Worley_ReportsTheRunnerUpCellDistinctlyFromTheWinner()
        {
            // Needed to blend between regions: a consumer partitioning a world needs to know not only which
            // region a point is in but which one is next door, or every property that differs between them
            // changes discontinuously at the border.
            var w = new WorleyNoise(Seed);
            int distinct = 0, samples = 0;

            ForEachSample2(
                (x, y) =>
                {
                    CellularSample s = w.SampleCellular(x, y);
                    samples++;
                    if (s.SecondCellHash != s.CellHash) distinct++;
                },
                count: 2000);

            distinct.Should().Be(samples, "the runner-up is by definition a different cell from the winner");
        }

        [TestMethod]
        public void Worley_Certainty_IsZeroOnABorderAndOneDeepInside()
        {
            var w = new WorleyNoise(Seed, jitter: 0.0); // no jitter: cells are exactly the unit grid

            // The centre of a cell is as far from the border as it gets.
            w.SampleCellular(4.5, 7.5).Certainty(0.3).Should().BeApproximately(1.0, 1e-9);

            // Exactly halfway between two feature points, F1 and F2 are equal, so certainty is zero.
            w.SampleCellular(5.0, 7.5).Certainty(0.3).Should().BeApproximately(0.0, 1e-9);

            // And it is monotone in between, which is what makes it usable as a blend weight.
            double previous = -1;
            for (double t = 0.0; t <= 0.5; t += 0.01)
            {
                double certainty = w.SampleCellular(4.5 + t, 7.5).Certainty(0.3);
                certainty.Should().BeInRange(0.0, 1.0);
                if (previous >= 0) certainty.Should().BeLessThanOrEqualTo(previous + 1e-12);
                previous = certainty;
            }
        }

        [TestMethod]
        public void Worley_ZeroJitter_PutsFeaturePointsAtCellCentres()
        {
            var w = new WorleyNoise(Seed, jitter: 0.0);
            w.SampleCellular(0.5, 0.5).F1.Should().BeApproximately(0.0, 1e-12);
            w.SampleCellular(7.5, -3.5).F1.Should().BeApproximately(0.0, 1e-12);
        }

        [TestMethod]
        public void Worley_EveryMetricProducesNonNegativeDistances()
        {
            foreach (DistanceMetric metric in Enum.GetValues(typeof(DistanceMetric)))
            {
                var w = new WorleyNoise(Seed, metric);
                ForEachSample2(
                    (x, y) =>
                    {
                        CellularSample s = w.SampleCellular(x, y);
                        s.F1.Should().BeGreaterThanOrEqualTo(0.0);
                        s.F1.Should().BeLessThanOrEqualTo(s.F2);
                    },
                    count: 1000);
            }
        }

        [TestMethod]
        public void Worley3D_FindsTheNearestOfTwentySevenCells()
        {
            var w = new WorleyNoise(Seed);
            ForEachSample3(
                (x, y, z) =>
                {
                    CellularSample s = w.SampleCellular(x, y, z);
                    s.F1.Should().BeLessThanOrEqualTo(s.F2);
                    // With one point per unit cell, the nearest is never further than the cell diagonal.
                    s.F1.Should().BeLessThan(2.0);
                },
                count: 2000);
        }

        // ---- Fractal stacking -----------------------------------------------------------------------

        [TestMethod]
        public void Fractal_OneOctave_IsTheSourceField()
        {
            var source = new SimplexNoise(Seed);
            var stack = new FractalNoise2(source, octaves: 1, frequency: 1.0);

            ForEachSample2((x, y) => stack.Sample(x, y).Should().BeApproximately(source.Sample(x, y), 1e-12), count: 500);
        }

        [TestMethod]
        public void Fractal_StaysInRangeForEveryModeAndOctaveCount()
        {
            var source = new SimplexNoise(Seed);

            foreach (FractalMode mode in Enum.GetValues(typeof(FractalMode)))
            {
                for (int octaves = 1; octaves <= 8; octaves++)
                {
                    var stack = new FractalNoise2(source, octaves, frequency: 0.05, mode: mode);
                    ForEachSample2(
                        (x, y) => stack.Sample(x, y).Should().BeInRange(-1.0, 1.0, "mode {0}, {1} octaves", mode, octaves),
                        count: 800);
                }
            }
        }

        [TestMethod]
        public void Fractal_MoreOctaves_AddDetailWithoutChangingScale()
        {
            // The normalisation guarantee: adding octaves must not inflate the field, or every threshold
            // tuned against a 4-octave world silently breaks in a 6-octave one.
            var source = new SimplexNoise(Seed);
            var four = new FractalNoise2(source, octaves: 4, frequency: 0.02);
            var eight = new FractalNoise2(source, octaves: 8, frequency: 0.02);

            double sum4 = 0, sum8 = 0;
            int n = 0;
            ForEachSample2(
                (x, y) =>
                {
                    sum4 += System.Math.Abs(four.Sample(x, y));
                    sum8 += System.Math.Abs(eight.Sample(x, y));
                    n++;
                },
                count: 3000);

            // Mean magnitudes should be in the same ballpark, not a factor apart.
            (sum8 / n).Should().BeApproximately(sum4 / n, 0.12);
        }

        [TestMethod]
        public void Fractal_RejectsZeroOctaves()
        {
            Action act = () => new FractalNoise2(new SimplexNoise(Seed), octaves: 0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Fractal_RejectsANullSource()
        {
            Action act = () => new FractalNoise3(null!, octaves: 2);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void RidgedMultifractal_IsNonNegativeAndBounded()
        {
            var stack = new FractalNoise2(new SimplexNoise(Seed), octaves: 5, frequency: 0.03, mode: FractalMode.Ridged);
            ForEachSample2((x, y) => stack.SampleRidgedMultifractal(x, y).Should().BeInRange(0.0, 1.5), count: 3000);
        }

        [TestMethod]
        public void SquashedSampling_FlattensFeaturesVertically()
        {
            // Sampling Y at a higher frequency makes the field vary faster vertically, so a fixed vertical
            // step crosses more feature boundaries: the structures are flatter. Measured as total variation.
            var stack = new FractalNoise3(new SimplexNoise(Seed), octaves: 3, frequency: 0.02);

            double flatVariation = 0, roundVariation = 0;
            for (int i = 1; i < 2000; i++)
            {
                double y = i * 0.5;
                flatVariation += System.Math.Abs(stack.SampleSquashed(10, y, 20, 4.0) - stack.SampleSquashed(10, y - 0.5, 20, 4.0));
                roundVariation += System.Math.Abs(stack.SampleSquashed(10, y, 20, 1.0) - stack.SampleSquashed(10, y - 0.5, 20, 1.0));
            }

            flatVariation.Should().BeGreaterThan(roundVariation);
        }

        // ---- Domain warping -------------------------------------------------------------------------

        [TestMethod]
        public void DomainWarp_ZeroAmplitude_LeavesTheFieldUntouched()
        {
            var source = new SimplexNoise(Seed);
            var warp = DomainWarp2.Simplex(source, Seed, amplitude: 0.0, frequency: 0.1);

            ForEachSample2((x, y) => warp.Sample(x, y).Should().BeApproximately(source.Sample(x, y), 1e-12), count: 500);
        }

        [TestMethod]
        public void DomainWarp_DisplacesNoFurtherThanAmplitudeTimesIterations()
        {
            const double Amplitude = 12.0;
            const int Iterations = 2;
            var warp = DomainWarp2.Simplex(new SimplexNoise(Seed), Seed, Amplitude, frequency: 0.05, iterations: Iterations);

            ForEachSample2(
                (x, y) =>
                {
                    warp.Displace(x, y, out double wx, out double wy);
                    System.Math.Abs(wx - x).Should().BeLessThanOrEqualTo(Amplitude * Iterations);
                    System.Math.Abs(wy - y).Should().BeLessThanOrEqualTo(Amplitude * Iterations);
                },
                count: 1000);
        }

        [TestMethod]
        public void DomainWarp_IsStillDeterministicAndInRange()
        {
            var warp = DomainWarp3.Simplex(new FractalNoise3(new SimplexNoise(Seed), 4, 0.01), Seed, 20.0, 0.008);
            ForEachSample3(
                (x, y, z) =>
                {
                    double v = warp.Sample(x, y, z);
                    v.Should().BeInRange(-1.0, 1.0);
                    v.Should().Be(warp.Sample(x, y, z));
                },
                count: 1000);
        }

        // ---- Hashing --------------------------------------------------------------------------------

        [TestMethod]
        public void Hash_DoesNotCollideOnCoordinateSwaps()
        {
            // The symmetric-multiplier bug: with one prime per axis, hash(3,7) would equal hash(7,3) and
            // the whole field would be mirrored about the diagonal.
            NoiseHash.Hash(3, 7, Seed).Should().NotBe(NoiseHash.Hash(7, 3, Seed));
            NoiseHash.Hash(1, 0, 0, Seed).Should().NotBe(NoiseHash.Hash(0, 1, 0, Seed));
            NoiseHash.Hash(0, 0, 1, Seed).Should().NotBe(NoiseHash.Hash(0, 1, 0, Seed));
        }

        [TestMethod]
        public void Hash_SpreadsAcrossTheWholeUnitInterval()
        {
            // A crude uniformity check: with 16 buckets and 64k samples, a well-mixed hash fills each to
            // within a few percent. A weak hash clumps.
            var buckets = new int[16];
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    buckets[NoiseHash.ToIndex(NoiseHash.Hash(x, y, Seed), 16)]++;
                }
            }

            const int Expected = 256 * 256 / 16;
            foreach (int count in buckets)
            {
                count.Should().BeInRange((int)(Expected * 0.9), (int)(Expected * 1.1));
            }
        }

        [TestMethod]
        public void ToIndex_StaysInBounds()
        {
            for (uint h = 0; h < 100000; h += 97)
            {
                NoiseHash.ToIndex(h, 12).Should().BeInRange(0, 11);
            }

            NoiseHash.ToIndex(uint.MaxValue, 8).Should().Be(7);
            NoiseHash.ToIndex(0, 8).Should().Be(0);
        }

        [TestMethod]
        public void ToUnitAndToSigned_CoverTheirRanges()
        {
            NoiseHash.ToUnit(0).Should().Be(0.0);
            NoiseHash.ToUnit(uint.MaxValue).Should().BeLessThan(1.0).And.BeGreaterThan(0.999);
            NoiseHash.ToSigned(0).Should().Be(-1.0);
            NoiseHash.ToSigned(uint.MaxValue).Should().BeLessThan(1.0).And.BeGreaterThan(0.999);
        }
    }
}
