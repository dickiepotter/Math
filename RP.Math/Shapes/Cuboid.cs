namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A conceptual (unpositioned) cuboid (rectangular box), described purely by its <see cref="Width"/>,
    /// <see cref="Height"/> and <see cref="Depth"/>.
    /// </summary>
    /// <remarks>
    /// This type holds only the intrinsic shape — the three edge lengths — with no position or
    /// orientation. Its volume, surface area and space diagonal depend solely on those, so cuboids can be
    /// compared by size on their own. To place one in space, pair it with a position; see
    /// <see cref="PlacedCuboid"/>. This solid is named "cuboid" rather than "box" to leave the plain
    /// word for the axis-aligned <i>bounding</i> box, <see cref="BoundingBox"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Cuboid : IEquatable<Cuboid>, IComparable<Cuboid>, IFormattable
    {
        #region Fields

        private readonly double width;
        private readonly double height;
        private readonly double depth;

        #endregion

        #region Constructors

        /// <summary>Construct a cuboid from its width, height and depth.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any size is negative.</exception>
        public Cuboid(double width, double height, double depth)
        {
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width), width, NEGATIVE_SIZE);
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height), height, NEGATIVE_SIZE);
            if (depth < 0) throw new ArgumentOutOfRangeException(nameof(depth), depth, NEGATIVE_SIZE);

            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        #endregion

        #region Factories

        /// <summary>A cube (all edges equal) of the given <paramref name="side"/>.</summary>
        public static Cuboid Cube(double side)
        {
            return new Cuboid(side, side, side);
        }

        #endregion

        #region Accessors

        /// <summary>The width (extent along the first axis).</summary>
        public double Width { get { return this.width; } }

        /// <summary>The height (extent along the second axis).</summary>
        public double Height { get { return this.height; } }

        /// <summary>The depth (extent along the third axis).</summary>
        public double Depth { get { return this.depth; } }

        /// <summary>The three sizes packed as (width, height, depth).</summary>
        public Vector Size { get { return new Vector(this.width, this.height, this.depth); } }

        /// <summary>The enclosed volume, width × height × depth.</summary>
        public double Volume { get { return this.width * this.height * this.depth; } }

        /// <summary>The surface area, 2·(wh + hd + dw).</summary>
        public double SurfaceArea
        {
            get { return 2.0 * ((this.width * this.height) + (this.height * this.depth) + (this.depth * this.width)); }
        }

        /// <summary>The length of the space diagonal, <c>√(w² + h² + d²)</c> (Pythagoras in 3D).</summary>
        public double SpaceDiagonal
        {
            get { return Math.Sqrt((this.width * this.width) + (this.height * this.height) + (this.depth * this.depth)); }
        }

        #endregion

        #region Classification

        /// <summary>Whether all three edges are equal within <paramref name="tolerance"/> (a cube).</summary>
        public bool IsCube(double tolerance)
        {
            return this.width.AlmostEqualsWithAbsTolerance(this.height, tolerance)
                && this.height.AlmostEqualsWithAbsTolerance(this.depth, tolerance);
        }

        #endregion

        #region Comparison (by volume)

        /// <summary>
        /// Compare with another cuboid by <see cref="Volume"/> — the natural "which is bigger" ordering,
        /// independent of proportions. Mirrors how <see cref="Vector"/> compares by magnitude.
        /// </summary>
        public int CompareTo(Cuboid other)
        {
            return this.Volume.CompareTo(other.Volume);
        }

        /// <summary>Whether the left cuboid's volume is less than the right's.</summary>
        public static bool operator <(Cuboid c1, Cuboid c2) { return c1.Volume < c2.Volume; }

        /// <summary>Whether the left cuboid's volume is greater than the right's.</summary>
        public static bool operator >(Cuboid c1, Cuboid c2) { return c1.Volume > c2.Volume; }

        /// <summary>Whether the left cuboid's volume is less than or equal to the right's.</summary>
        public static bool operator <=(Cuboid c1, Cuboid c2) { return c1.Volume <= c2.Volume; }

        /// <summary>Whether the left cuboid's volume is greater than or equal to the right's.</summary>
        public static bool operator >=(Cuboid c1, Cuboid c2) { return c1.Volume >= c2.Volume; }

        #endregion

        #region Operators

        /// <summary>Equality of all three sizes.</summary>
        public static bool operator ==(Cuboid c1, Cuboid c2)
        {
            return c1.Width == c2.Width && c1.Height == c2.Height && c1.Depth == c2.Depth;
        }

        /// <summary>Inequality of any size.</summary>
        public static bool operator !=(Cuboid c1, Cuboid c2)
        {
            return !(c1 == c2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Cuboid c && this.Equals(c);
        }

        /// <summary>Equality with another cuboid (all three sizes).</summary>
        public bool Equals(Cuboid other)
        {
            return this == other;
        }

        /// <summary>Equality with another cuboid within an absolute tolerance.</summary>
        public bool Equals(Cuboid other, double tolerance)
        {
            return this.width.AlmostEqualsWithAbsTolerance(other.Width, tolerance)
                && this.height.AlmostEqualsWithAbsTolerance(other.Height, tolerance)
                && this.depth.AlmostEqualsWithAbsTolerance(other.Depth, tolerance);
        }

        /// <summary>A hash code derived from the three sizes.</summary>
        public override int GetHashCode()
        {
            return this.width.GetHashCode() ^ this.height.GetHashCode() ^ this.depth.GetHashCode();
        }

        /// <summary>Deconstruct into width, height and depth.</summary>
        public void Deconstruct(out double width, out double height, out double depth)
        {
            width = this.width;
            height = this.height;
            depth = this.depth;
        }

        /// <summary>A string of the form <c>w×h×d</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>w×h×d</c> where the sizes use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "{0}×{1}×{2}",
                this.width.ToString(format, formatProvider),
                this.height.ToString(format, formatProvider),
                this.depth.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_SIZE = "A cuboid's sizes cannot be negative.";

        #endregion
    }
}
