namespace RP.Math
{
    using System;
    using System.Collections.Generic;

    using Math = System.Math;

    /// <summary>
    /// An <b>axis-aligned rectangle</b> in the plane, held as its two opposite corners: the corner with the
    /// smallest x and y (<see cref="Min"/>) and the one with the largest (<see cref="Max"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Axis-aligned" means its sides run along the x and y axes, never at an angle. That makes the common
    /// questions very cheap - is a point inside? do two boxes overlap? - because each is answered by comparing
    /// x with x and y with y separately. It is the 2D cousin of <see cref="BoundingBox"/>, and is most often used
    /// the same way: as a quick, conservative stand-in for a more complicated shape. If two shapes' boxes do not
    /// overlap, the shapes cannot either, so the expensive test can be skipped.
    /// </para>
    /// <para>
    /// The <see cref="Empty"/> box has its minimum above its maximum, so that growing it to include any point
    /// gives exactly that point: a handy starting value when measuring a set of points.
    /// </para>
    /// </remarks>
    public readonly struct Box2d : IEquatable<Box2d>
    {
        /// <summary>A box that contains nothing: <see cref="Include(Vector2d)"/> on it gives a box round just that point.</summary>
        public static readonly Box2d Empty = new Box2d(
            new Vector2d(double.PositiveInfinity, double.PositiveInfinity),
            new Vector2d(double.NegativeInfinity, double.NegativeInfinity),
            sorted: true);

        /// <summary>A box from two opposite corners, given either way round.</summary>
        public Box2d(Vector2d cornerA, Vector2d cornerB)
            : this(
                new Vector2d(Math.Min(cornerA.X, cornerB.X), Math.Min(cornerA.Y, cornerB.Y)),
                new Vector2d(Math.Max(cornerA.X, cornerB.X), Math.Max(cornerA.Y, cornerB.Y)),
                sorted: true)
        {
        }

        // takes the corners exactly as given: how Empty keeps its minimum above its maximum
        private Box2d(Vector2d min, Vector2d max, bool sorted)
        {
            this.Min = min;
            this.Max = max;
        }

        /// <summary>A box from its lower-left corner and its size.</summary>
        public static Box2d FromCornerAndSize(double x, double y, double width, double height) =>
            new Box2d(new Vector2d(x, y), new Vector2d(x + width, y + height));

        /// <summary>The smallest box containing every point (<see cref="Empty"/> for none).</summary>
        public static Box2d FromPoints(IEnumerable<Vector2d> points)
        {
            var box = Empty;
            foreach (var point in points)
            {
                box = box.Include(point);
            }

            return box;
        }

        /// <summary>The corner with the smallest x and y.</summary>
        public Vector2d Min { get; }

        /// <summary>The corner with the largest x and y.</summary>
        public Vector2d Max { get; }

        /// <summary>True for the empty box (no points at all).</summary>
        public bool IsEmpty => this.Min.X > this.Max.X || this.Min.Y > this.Max.Y;

        public double Width => this.IsEmpty ? 0 : this.Max.X - this.Min.X;

        public double Height => this.IsEmpty ? 0 : this.Max.Y - this.Min.Y;

        public double Area => this.Width * this.Height;

        /// <summary>The point halfway between the corners.</summary>
        public Vector2d Centre => (this.Min + this.Max) / 2;

        /// <summary>The length of the diagonal: the furthest apart two points in the box can be.</summary>
        public double Diagonal => this.IsEmpty ? 0 : Vector2d.Distance(this.Min, this.Max);

        /// <summary>True when the point is inside the box or on its edge.</summary>
        public bool Contains(Vector2d point) =>
            point.X >= this.Min.X && point.X <= this.Max.X && point.Y >= this.Min.Y && point.Y <= this.Max.Y;

        /// <summary>True when the other box lies wholly inside this one.</summary>
        public bool Contains(Box2d other) =>
            !other.IsEmpty && this.Contains(other.Min) && this.Contains(other.Max);

        /// <summary>
        /// True when the boxes overlap (touching counts). Two boxes overlap exactly when their x ranges overlap
        /// <i>and</i> their y ranges overlap - the "separating axis" idea in its simplest form.
        /// </summary>
        public bool Intersects(Box2d other) =>
            !this.IsEmpty && !other.IsEmpty
            && this.Min.X <= other.Max.X && other.Min.X <= this.Max.X
            && this.Min.Y <= other.Max.Y && other.Min.Y <= this.Max.Y;

        /// <summary>The overlap of two boxes, or <see cref="Empty"/> when they do not meet.</summary>
        public Box2d Intersection(Box2d other)
        {
            if (!this.Intersects(other))
            {
                return Empty;
            }

            return new Box2d(
                new Vector2d(Math.Max(this.Min.X, other.Min.X), Math.Max(this.Min.Y, other.Min.Y)),
                new Vector2d(Math.Min(this.Max.X, other.Max.X), Math.Min(this.Max.Y, other.Max.Y)));
        }

        /// <summary>The smallest box containing both.</summary>
        public Box2d Union(Box2d other)
        {
            if (this.IsEmpty)
            {
                return other;
            }

            if (other.IsEmpty)
            {
                return this;
            }

            return new Box2d(
                new Vector2d(Math.Min(this.Min.X, other.Min.X), Math.Min(this.Min.Y, other.Min.Y)),
                new Vector2d(Math.Max(this.Max.X, other.Max.X), Math.Max(this.Max.Y, other.Max.Y)));
        }

        /// <summary>The smallest box containing this one and the point.</summary>
        public Box2d Include(Vector2d point)
        {
            if (this.IsEmpty)
            {
                return new Box2d(point, point);
            }

            return new Box2d(
                new Vector2d(Math.Min(this.Min.X, point.X), Math.Min(this.Min.Y, point.Y)),
                new Vector2d(Math.Max(this.Max.X, point.X), Math.Max(this.Max.Y, point.Y)));
        }

        /// <summary>The box grown by a margin on every side (a negative margin shrinks it).</summary>
        public Box2d Inflate(double margin) =>
            this.IsEmpty ? this : new Box2d(this.Min - new Vector2d(margin, margin), this.Max + new Vector2d(margin, margin));

        /// <summary>The point of the box nearest to the given point (the point itself when it is inside).</summary>
        public Vector2d ClosestPoint(Vector2d point) =>
            new Vector2d(Clamp(point.X, this.Min.X, this.Max.X), Clamp(point.Y, this.Min.Y, this.Max.Y));

        /// <summary>The four corners, anticlockwise from <see cref="Min"/>.</summary>
        public Vector2d[] Corners() =>
        [
            this.Min,
            new Vector2d(this.Max.X, this.Min.Y),
            this.Max,
            new Vector2d(this.Min.X, this.Max.Y),
        ];

        private static double Clamp(double value, double low, double high) => value < low ? low : value > high ? high : value;

        public bool Equals(Box2d other) => this.Min.Equals(other.Min) && this.Max.Equals(other.Max);

        public override bool Equals(object? obj) => obj is Box2d other && this.Equals(other);

        public override int GetHashCode() => (this.Min.GetHashCode() * 397) ^ this.Max.GetHashCode();

        public static bool operator ==(Box2d a, Box2d b) => a.Equals(b);

        public static bool operator !=(Box2d a, Box2d b) => !a.Equals(b);

        public override string ToString() => this.IsEmpty ? "Box2d(empty)" : $"Box2d({this.Min} to {this.Max})";
    }
}
