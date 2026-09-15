namespace RP.Math.Noise
{
    /// <summary>
    /// The deterministic integer hash every noise field in <see cref="RP.Math.Noise"/> is built on: given a
    /// lattice coordinate and a seed, it returns a well-scrambled 32-bit value. Change the seed and you get
    /// a completely different — but equally valid — field; keep the seed and the same coordinate always
    /// returns the same value, on any machine, in any order, forever.
    /// </summary>
    /// <remarks>
    /// <para><b>Why not <c>System.Random</c>.</b> A pseudo-random <i>sequence</i> is the wrong tool for a
    /// noise field. A field must be <b>random-access</b>: asking "what is the value at (12, -400, 7)?" has
    /// to be answerable instantly, without generating the values before it, and has to give the same answer
    /// whether it is the first question asked or the millionth. That is a <i>hash</i>, not a generator. It
    /// is what lets a world stream in around a player who walks wherever they please.</para>
    ///
    /// <para><b>The mixing function.</b> Each coordinate is multiplied by a large odd constant (a distinct
    /// prime per axis, so <c>(1, 0)</c> and <c>(0, 1)</c> cannot collide) and the sum is run through a
    /// finalising avalanche — the xor-shift/multiply chain from MurmurHash3. Avalanche is the property that
    /// matters: flipping <i>one</i> input bit must flip about <i>half</i> the output bits. Without it,
    /// neighbouring lattice points produce neighbouring hashes and the noise shows visible grid structure.</para>
    ///
    /// <para><b>Determinism is a feature.</b> Everything built on this — terrain, ore placement, cave
    /// networks, biome layout — is a pure function of (coordinate, seed). Nothing needs saving except the
    /// seed and the player's edits, and two machines given the same seed generate identical worlds. That
    /// property is what makes streaming a shared world across a network affordable.</para>
    ///
    /// <para>All methods are <see langword="static"/>, allocation-free and branch-light, and safe to call
    /// from many threads at once — there is no state to race on.</para>
    /// </remarks>
    public static class NoiseHash
    {
        // Distinct large primes per axis. A different multiplier per axis is what stops coordinate
        // permutations colliding: with one shared multiplier, hash(a, b) and hash(b, a) would land on the
        // same value and the field would be mirror-symmetric about the diagonal.
        private const uint PrimeX = 0x9E3779B1u; // 2^32 / golden ratio — the classic Knuth multiplier
        private const uint PrimeY = 0x85EBCA77u;
        private const uint PrimeZ = 0xC2B2AE3Du;
        private const uint PrimeW = 0x27D4EB2Fu;
        private const uint PrimeSeed = 0x165667B1u;

        /// <summary>
        /// MurmurHash3's finaliser: the avalanche step that turns a merely-different value into a
        /// thoroughly-scrambled one. Every hash here ends with this.
        /// </summary>
        public static uint Avalanche(uint h)
        {
            unchecked
            {
                h ^= h >> 16;
                h *= 0x85EBCA6Bu;
                h ^= h >> 13;
                h *= 0xC2B2AE35u;
                h ^= h >> 16;
                return h;
            }
        }

        /// <summary>Hashes a 1D lattice coordinate with a seed.</summary>
        public static uint Hash(int x, int seed)
        {
            unchecked { return Avalanche(((uint)x * PrimeX) + ((uint)seed * PrimeSeed)); }
        }

        /// <summary>Hashes a 2D lattice coordinate with a seed.</summary>
        public static uint Hash(int x, int y, int seed)
        {
            unchecked { return Avalanche(((uint)x * PrimeX) + ((uint)y * PrimeY) + ((uint)seed * PrimeSeed)); }
        }

        /// <summary>Hashes a 3D lattice coordinate with a seed.</summary>
        public static uint Hash(int x, int y, int z, int seed)
        {
            unchecked
            {
                return Avalanche(((uint)x * PrimeX) + ((uint)y * PrimeY) + ((uint)z * PrimeZ)
                                 + ((uint)seed * PrimeSeed));
            }
        }

        /// <summary>Hashes a 4D lattice coordinate with a seed.</summary>
        public static uint Hash(int x, int y, int z, int w, int seed)
        {
            unchecked
            {
                return Avalanche(((uint)x * PrimeX) + ((uint)y * PrimeY) + ((uint)z * PrimeZ)
                                 + ((uint)w * PrimeW) + ((uint)seed * PrimeSeed));
            }
        }

        /// <summary>
        /// Derives a fresh, independent seed from an existing one and a label. Use this to give each
        /// <i>layer</i> of a world its own field from one master seed: label 1 for the height map, 2 for
        /// caves, 3 for ore, and so on. Reusing one seed across two layers correlates them — mountains that
        /// always sit over caves — which reads as artificial the moment a player notices it.
        /// </summary>
        public static int Derive(int seed, int label)
        {
            unchecked { return (int)Avalanche(((uint)seed * PrimeSeed) + ((uint)label * PrimeX)); }
        }

        /// <summary>Maps a hash to <c>[0, 1)</c>.</summary>
        public static double ToUnit(uint hash) => hash * (1.0 / 4294967296.0);

        /// <summary>Maps a hash to <c>[-1, 1)</c>.</summary>
        public static double ToSigned(uint hash) => (hash * (2.0 / 4294967296.0)) - 1.0;

        /// <summary>
        /// Maps a hash to a uniformly-chosen integer in <c>[0, count)</c>. Uses the high bits (best mixed
        /// after the avalanche) via a widening multiply, so there is no modulo bias.
        /// </summary>
        public static int ToIndex(uint hash, int count) => (int)(((ulong)hash * (ulong)count) >> 32);

        // ---- Gradient selection -------------------------------------------------------------------
        // Gradient noise needs a pseudo-random *direction* at each lattice point. Picking from a small
        // fixed set (rather than normalising a random vector) is both faster and better distributed: these
        // sets are evenly spread, which keeps the noise isotropic — it has no preferred direction, so
        // terrain built from it shows no grain.

        private const double Diag2 = 0.7071067811865476; // 1/sqrt(2)

        // 8 directions on the unit circle: the 4 axes and the 4 diagonals, all unit length.
        private static readonly double[] Grad2 =
        {
            1, 0,   -1, 0,   0, 1,   0, -1,
            Diag2, Diag2,   -Diag2, Diag2,   Diag2, -Diag2,   -Diag2, -Diag2,
        };

        // The 12 edge-midpoints of a cube — Perlin's improved-noise gradient set. All the same length, so
        // the field's amplitude stays uniform in every direction.
        private static readonly double[] Grad3 =
        {
            1, 1, 0,   -1, 1, 0,   1, -1, 0,   -1, -1, 0,
            1, 0, 1,   -1, 0, 1,   1, 0, -1,   -1, 0, -1,
            0, 1, 1,   0, -1, 1,   0, 1, -1,   0, -1, -1,
        };

        // The 32 vectors from the centre of a 4-cube to the midpoints of its cells: one zero component and
        // three +/-1 components, in every arrangement.
        private static readonly double[] Grad4 =
        {
            0, 1, 1, 1,    0, 1, 1, -1,    0, 1, -1, 1,    0, 1, -1, -1,
            0, -1, 1, 1,   0, -1, 1, -1,   0, -1, -1, 1,   0, -1, -1, -1,
            1, 0, 1, 1,    1, 0, 1, -1,    1, 0, -1, 1,    1, 0, -1, -1,
            -1, 0, 1, 1,   -1, 0, 1, -1,   -1, 0, -1, 1,   -1, 0, -1, -1,
            1, 1, 0, 1,    1, 1, 0, -1,    1, -1, 0, 1,    1, -1, 0, -1,
            -1, 1, 0, 1,   -1, 1, 0, -1,   -1, -1, 0, 1,   -1, -1, 0, -1,
            1, 1, 1, 0,    1, 1, -1, 0,    1, -1, 1, 0,    1, -1, -1, 0,
            -1, 1, 1, 0,   -1, 1, -1, 0,   -1, -1, 1, 0,   -1, -1, -1, 0,
        };

        /// <summary>
        /// The dot product of the pseudo-random gradient at lattice point <c>(ix, iy)</c> with the offset
        /// <c>(dx, dy)</c> from that point. This one operation <i>is</i> gradient noise: the contribution is
        /// zero at the lattice point itself and rises in the gradient's direction, so blending neighbours
        /// produces smooth organic slopes rather than a blur of independent point samples.
        /// </summary>
        public static double GradientDot(int ix, int iy, int seed, double dx, double dy)
        {
            int g = ToIndex(Hash(ix, iy, seed), 8) * 2;
            return (Grad2[g] * dx) + (Grad2[g + 1] * dy);
        }

        /// <summary>The 3D gradient dot product. See <see cref="GradientDot(int, int, int, double, double)"/>.</summary>
        public static double GradientDot(int ix, int iy, int iz, int seed, double dx, double dy, double dz)
        {
            int g = ToIndex(Hash(ix, iy, iz, seed), 12) * 3;
            return (Grad3[g] * dx) + (Grad3[g + 1] * dy) + (Grad3[g + 2] * dz);
        }

        /// <summary>The 4D gradient dot product. See <see cref="GradientDot(int, int, int, double, double)"/>.</summary>
        public static double GradientDot(int ix, int iy, int iz, int iw, int seed, double dx, double dy, double dz, double dw)
        {
            int g = ToIndex(Hash(ix, iy, iz, iw, seed), 32) * 4;
            return (Grad4[g] * dx) + (Grad4[g + 1] * dy) + (Grad4[g + 2] * dz) + (Grad4[g + 3] * dw);
        }

        /// <summary>
        /// The gradient vector itself at lattice point <c>(ix, iy)</c>, rather than its dot product with an
        /// offset. Needed wherever the <i>analytic derivative</i> of a gradient-noise field is computed: the
        /// chain rule needs each corner's gradient separately, not the already-collapsed scalar.
        /// </summary>
        public static void Gradient(int ix, int iy, int seed, out double gx, out double gy)
        {
            int g = ToIndex(Hash(ix, iy, seed), 8) * 2;
            gx = Grad2[g];
            gy = Grad2[g + 1];
        }

        /// <summary>The gradient vector at a 3D lattice point. See <see cref="Gradient(int, int, int, out double, out double)"/>.</summary>
        public static void Gradient(int ix, int iy, int iz, int seed, out double gx, out double gy, out double gz)
        {
            int g = ToIndex(Hash(ix, iy, iz, seed), 12) * 3;
            gx = Grad3[g];
            gy = Grad3[g + 1];
            gz = Grad3[g + 2];
        }

        /// <summary>
        /// A pseudo-random feature point inside the unit square of cell <c>(cx, cy)</c>, returned as an
        /// offset in <c>[0, 1)^2</c>. This placement is what cellular (Worley) noise measures distance to.
        /// </summary>
        public static void CellPoint(int cx, int cy, int seed, out double px, out double py)
        {
            uint h = Hash(cx, cy, seed);
            px = ToUnit(h);
            py = ToUnit(Avalanche(h ^ PrimeY));
        }

        /// <summary>A pseudo-random feature point inside the unit cube of cell <c>(cx, cy, cz)</c>.</summary>
        public static void CellPoint(int cx, int cy, int cz, int seed, out double px, out double py, out double pz)
        {
            uint h = Hash(cx, cy, cz, seed);
            px = ToUnit(h);
            uint h2 = Avalanche(h ^ PrimeY);
            py = ToUnit(h2);
            pz = ToUnit(Avalanche(h2 ^ PrimeZ));
        }
    }
}
