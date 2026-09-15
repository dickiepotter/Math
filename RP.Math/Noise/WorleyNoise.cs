namespace RP.Math.Noise
{
    /// <summary>
    /// How distance to a feature point is measured by <see cref="WorleyNoise"/>. The metric changes the
    /// <i>shape</i> of the cells, not merely their size, so it is a genuine artistic control.
    /// </summary>
    public enum DistanceMetric
    {
        /// <summary>Straight-line distance. Cells are rounded and organic — cobble, cracked mud, scales.</summary>
        Euclidean,

        /// <summary>Squared straight-line distance: the same ordering of cells, without the square root.
        /// Cheaper, and the contrast is higher because near distances are compressed. Use when only the
        /// cell <i>identity</i> matters, or when you want harder-edged cells for free.</summary>
        EuclideanSquared,

        /// <summary>Sum of absolute differences per axis. Cells become diamonds — an angular, crystalline
        /// look that suits mineral veins and fractured rock.</summary>
        Manhattan,

        /// <summary>The largest single-axis difference. Cells become squares, giving a blocky, tiled,
        /// deliberately artificial character — useful for man-made floors and circuitry.</summary>
        Chebyshev,
    }

    /// <summary>
    /// Everything one sample of <see cref="WorleyNoise"/> learned, so a caller can ask several questions
    /// for the price of one lookup.
    /// </summary>
    /// <remarks>
    /// The three fields answer very different questions. <see cref="F1"/> — distance to the nearest feature
    /// point — gives rounded blobs. <c>F2 - F1</c> gives the <b>ridges between cells</b>: it is near zero
    /// exactly where two cells are equidistant, which is the cell boundary, and that is how cracks, dry
    /// riverbeds, cobbled paths and cracked ice are drawn. And <see cref="CellHash"/> gives every cell a
    /// stable identity, which is the cleanest way to partition a world into <i>regions</i> — biomes,
    /// territories, districts — that have irregular organic borders rather than a visible grid.
    /// </remarks>
    public readonly struct CellularSample
    {
        /// <summary>Distance to the nearest feature point, in the chosen metric.</summary>
        public readonly double F1;

        /// <summary>Distance to the second-nearest feature point.</summary>
        public readonly double F2;

        /// <summary>A stable hash identifying the cell that owns the nearest feature point.</summary>
        public readonly uint CellHash;

        /// <summary>The integer coordinate of the cell that owns the nearest feature point.</summary>
        public readonly int CellX;

        /// <summary>The integer coordinate of the cell that owns the nearest feature point.</summary>
        public readonly int CellY;

        /// <summary>The integer coordinate of the cell that owns the nearest feature point (0 in 2D).</summary>
        public readonly int CellZ;

        internal CellularSample(double f1, double f2, uint cellHash, int cellX, int cellY, int cellZ)
        {
            this.F1 = f1;
            this.F2 = f2;
            this.CellHash = cellHash;
            this.CellX = cellX;
            this.CellY = cellY;
            this.CellZ = cellZ;
        }

        /// <summary>
        /// <c>F2 - F1</c>: near zero on cell boundaries, rising toward cell centres. The canonical way to
        /// draw the <i>edges</i> of the cellular pattern rather than its interiors.
        /// </summary>
        public double Edge => this.F2 - this.F1;
    }

    /// <summary>
    /// Worley (cellular / Voronoi) noise in two and three dimensions. Where Perlin and simplex noise give
    /// you smooth hills, this gives you <b>cells</b>: scatter a feature point into every lattice cell, then
    /// report how far the sample is from the nearest ones.
    /// </summary>
    /// <remarks>
    /// <para><b>Why a world wants it.</b> It produces the patterns gradient noise cannot: cobblestone,
    /// cracked earth, basalt columns, crystal clusters, cell-shaped leaf litter — and, most valuable of all,
    /// irregular <b>region partitioning</b>. Asking "which cell owns this point?" at a coarse scale assigns
    /// every position in an infinite world to a region with natural, wandering borders, in constant time,
    /// with no map to store. That is exactly the shape of a biome layout.</para>
    ///
    /// <para><b>The search window.</b> The nearest feature point is not necessarily in the sample's own
    /// cell — a point near one edge may be closer to a neighbour's. Searching the 3x3 (or 3x3x3) block of
    /// cells around the sample is sufficient for the standard "one point per cell, uniformly placed" scheme
    /// used here, which is why the placement is constrained that way.</para>
    ///
    /// <para><b>Range.</b> <see cref="Sample(double, double)"/> implements <see cref="INoise2"/> by
    /// returning <c>F1</c> remapped to roughly <c>[-1, 1]</c> so it composes with the other fields in
    /// <see cref="FractalNoise2"/>. For real work call <see cref="SampleCellular(double, double)"/> and use
    /// the parts you need.</para>
    /// </remarks>
    public sealed class WorleyNoise : INoise2, INoise3
    {
        /// <summary>Creates a cellular field.</summary>
        /// <param name="seed">Selects which variant of the field this is.</param>
        /// <param name="metric">How distance is measured — see <see cref="DistanceMetric"/>.</param>
        /// <param name="jitter">How far feature points may stray from their cell centres, in <c>[0, 1]</c>.
        /// At 1 they fill the cell and the pattern is fully organic; at 0 they sit dead centre and the
        /// pattern collapses to a regular grid. Values around 0.8 keep cells irregular while avoiding the
        /// occasional near-degenerate sliver that full jitter produces.</param>
        public WorleyNoise(int seed = 0, DistanceMetric metric = DistanceMetric.Euclidean, double jitter = 1.0)
        {
            this.Seed = seed;
            this.Metric = metric;
            this.Jitter = jitter < 0.0 ? 0.0 : (jitter > 1.0 ? 1.0 : jitter);
        }

        /// <summary>The seed selecting which variant of the field this is.</summary>
        public int Seed { get; }

        /// <summary>How distance to feature points is measured.</summary>
        public DistanceMetric Metric { get; }

        /// <summary>How far feature points stray from their cell centres, in <c>[0, 1]</c>.</summary>
        public double Jitter { get; }

        /// <summary>
        /// The full 2D cellular result: both nearest distances, and the identity of the owning cell.
        /// </summary>
        public CellularSample SampleCellular(double x, double y)
        {
            int cx = PerlinNoise.FastFloor(x), cy = PerlinNoise.FastFloor(y);
            double f1 = double.MaxValue, f2 = double.MaxValue;
            uint bestHash = 0;
            int bestX = cx, bestY = cy;
            double offset = (1.0 - this.Jitter) * 0.5; // pull points toward the centre as jitter falls

            for (int oy = -1; oy <= 1; oy++)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    int nx = cx + ox, ny = cy + oy;
                    NoiseHash.CellPoint(nx, ny, this.Seed, out double px, out double py);
                    double fx = nx + offset + (px * this.Jitter);
                    double fy = ny + offset + (py * this.Jitter);
                    double d = this.Distance(fx - x, fy - y, 0.0, false);

                    if (d < f1)
                    {
                        f2 = f1;
                        f1 = d;
                        bestHash = NoiseHash.Hash(nx, ny, this.Seed);
                        bestX = nx;
                        bestY = ny;
                    }
                    else if (d < f2)
                    {
                        f2 = d;
                    }
                }
            }

            return new CellularSample(f1, f2, bestHash, bestX, bestY, 0);
        }

        /// <summary>The full 3D cellular result. See <see cref="SampleCellular(double, double)"/>.</summary>
        public CellularSample SampleCellular(double x, double y, double z)
        {
            int cx = PerlinNoise.FastFloor(x), cy = PerlinNoise.FastFloor(y), cz = PerlinNoise.FastFloor(z);
            double f1 = double.MaxValue, f2 = double.MaxValue;
            uint bestHash = 0;
            int bestX = cx, bestY = cy, bestZ = cz;
            double offset = (1.0 - this.Jitter) * 0.5;

            for (int oz = -1; oz <= 1; oz++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        int nx = cx + ox, ny = cy + oy, nz = cz + oz;
                        NoiseHash.CellPoint(nx, ny, nz, this.Seed, out double px, out double py, out double pz);
                        double fx = nx + offset + (px * this.Jitter);
                        double fy = ny + offset + (py * this.Jitter);
                        double fz = nz + offset + (pz * this.Jitter);
                        double d = this.Distance(fx - x, fy - y, fz - z, true);

                        if (d < f1)
                        {
                            f2 = f1;
                            f1 = d;
                            bestHash = NoiseHash.Hash(nx, ny, nz, this.Seed);
                            bestX = nx;
                            bestY = ny;
                            bestZ = nz;
                        }
                        else if (d < f2)
                        {
                            f2 = d;
                        }
                    }
                }
            }

            return new CellularSample(f1, f2, bestHash, bestX, bestY, bestZ);
        }

        /// <inheritdoc />
        /// <remarks>Returns <c>F1</c> remapped from roughly <c>[0, 1]</c> to <c>[-1, 1]</c> so the field
        /// composes with the coherent ones in a fractal stack.</remarks>
        public double Sample(double x, double y) => (this.SampleCellular(x, y).F1 * 2.0) - 1.0;

        /// <inheritdoc />
        /// <remarks>Returns <c>F1</c> remapped to roughly <c>[-1, 1]</c>.</remarks>
        public double Sample(double x, double y, double z) => (this.SampleCellular(x, y, z).F1 * 2.0) - 1.0;

        private double Distance(double dx, double dy, double dz, bool threeD)
        {
            switch (this.Metric)
            {
                case DistanceMetric.EuclideanSquared:
                    return (dx * dx) + (dy * dy) + (threeD ? dz * dz : 0.0);

                case DistanceMetric.Manhattan:
                    return System.Math.Abs(dx) + System.Math.Abs(dy) + (threeD ? System.Math.Abs(dz) : 0.0);

                case DistanceMetric.Chebyshev:
                {
                    double m = System.Math.Abs(dx);
                    double ay = System.Math.Abs(dy);
                    if (ay > m) m = ay;
                    if (threeD)
                    {
                        double az = System.Math.Abs(dz);
                        if (az > m) m = az;
                    }

                    return m;
                }

                default:
                    return System.Math.Sqrt((dx * dx) + (dy * dy) + (threeD ? dz * dz : 0.0));
            }
        }
    }
}
