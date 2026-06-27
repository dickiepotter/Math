namespace RP.Math
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// An <b>axis-aligned bounding box</b> (AABB): the smallest box, with faces parallel to the x, y and z
    /// planes, that is described by two opposite corners — a minimum and a maximum. Because its faces never
    /// tilt, the containment and overlap tests are just per-axis number comparisons, which is what makes an
    /// AABB the standard tool for <i>quick rejection</i>: a cheap "could these two things possibly touch?"
    /// test done before any exact (and expensive) geometry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the bounding-box partner of <see cref="BoundingSphere"/> — a different idea from the
    /// oriented solid <see cref="Cuboid"/>, which can sit at any angle. An AABB is always
    /// axis-aligned and lives directly in world space (it carries its own corners), so it has no separate
    /// conceptual/placed split.
    /// </para>
    /// <para>
    /// Treated as a filled region: <see cref="Contains(Vector)"/> means "on or within". Built on
    /// <see cref="Vector"/>, following the library's immutable design.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct BoundingBox : IEquatable<BoundingBox>, IFormattable
    {
        #region Fields

        private readonly Vector min;
        private readonly Vector max;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a box from two opposite corners. The corners are sorted per axis, so the smaller value
        /// on each axis becomes <see cref="Min"/> and the larger <see cref="Max"/> — you need not pass them
        /// in any particular order.
        /// </summary>
        public BoundingBox(Vector cornerA, Vector cornerB)
        {
            this.min = cornerA.ComponentMin(cornerB);
            this.max = cornerA.ComponentMax(cornerB);
        }

        #endregion

        #region Factories

        /// <summary>A box from its minimum and maximum corners (sorted per axis, as the constructor does).</summary>
        public static BoundingBox FromMinMax(Vector min, Vector max)
        {
            return new BoundingBox(min, max);
        }

        /// <summary>
        /// A box from its <paramref name="center"/> and half-size <paramref name="extents"/> (the distance
        /// from the centre to a face along each axis). Negative extents are treated as their absolute value.
        /// </summary>
        public static BoundingBox FromCenterExtents(Vector center, Vector extents)
        {
            Vector e = extents.AbsComponents();
            return new BoundingBox(center - e, center + e);
        }

        /// <summary>
        /// The smallest box that contains all of <paramref name="points"/> — the corner-wise minimum and
        /// maximum over them.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if no points are supplied.</exception>
        public static BoundingBox FromPoints(params Vector[] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            return FromPoints((IEnumerable<Vector>)points);
        }

        /// <summary>The smallest box that contains all of <paramref name="points"/>.</summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if no points are supplied.</exception>
        public static BoundingBox FromPoints(IEnumerable<Vector> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));

            bool any = false;
            Vector lo = Vector.Zero;
            Vector hi = Vector.Zero;
            foreach (Vector p in points)
            {
                if (!any)
                {
                    lo = hi = p;
                    any = true;
                }
                else
                {
                    lo = lo.ComponentMin(p);
                    hi = hi.ComponentMax(p);
                }
            }

            if (!any) throw new ArgumentException(NO_POINTS, nameof(points));
            return new BoundingBox(lo, hi);
        }

        #endregion

        #region Accessors

        /// <summary>The corner with the smallest coordinate on every axis.</summary>
        public Vector Min { get { return this.min; } }

        /// <summary>The corner with the largest coordinate on every axis.</summary>
        public Vector Max { get { return this.max; } }

        /// <summary>The centre of the box.</summary>
        public Vector Center { get { return (this.min + this.max) * 0.5; } }

        /// <summary>The full size along each axis (<see cref="Max"/> − <see cref="Min"/>).</summary>
        public Vector Size { get { return this.max - this.min; } }

        /// <summary>The half-size along each axis (the distance from the centre to a face).</summary>
        public Vector Extents { get { return (this.max - this.min) * 0.5; } }

        /// <summary>The enclosed volume (zero if the box is flat on any axis).</summary>
        public double Volume
        {
            get
            {
                Vector s = this.Size;
                return s.X * s.Y * s.Z;
            }
        }

        /// <summary>The total surface area of the six faces.</summary>
        public double SurfaceArea
        {
            get
            {
                Vector s = this.Size;
                return 2.0 * ((s.X * s.Y) + (s.Y * s.Z) + (s.Z * s.X));
            }
        }

        /// <summary>The eight corners of the box.</summary>
        public Vector[] Corners
        {
            get
            {
                return new[]
                {
                    new Vector(this.min.X, this.min.Y, this.min.Z),
                    new Vector(this.max.X, this.min.Y, this.min.Z),
                    new Vector(this.min.X, this.max.Y, this.min.Z),
                    new Vector(this.max.X, this.max.Y, this.min.Z),
                    new Vector(this.min.X, this.min.Y, this.max.Z),
                    new Vector(this.max.X, this.min.Y, this.max.Z),
                    new Vector(this.min.X, this.max.Y, this.max.Z),
                    new Vector(this.max.X, this.max.Y, this.max.Z),
                };
            }
        }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the box.</summary>
        public bool Contains(Vector point)
        {
            return point.X >= this.min.X && point.X <= this.max.X
                && point.Y >= this.min.Y && point.Y <= this.max.Y
                && point.Z >= this.min.Z && point.Z <= this.max.Z;
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the box, with the box grown by <paramref name="tolerance"/> on every side.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            return point.X >= this.min.X - tolerance && point.X <= this.max.X + tolerance
                && point.Y >= this.min.Y - tolerance && point.Y <= this.max.Y + tolerance
                && point.Z >= this.min.Z - tolerance && point.Z <= this.max.Z + tolerance;
        }

        /// <summary>Whether <paramref name="other"/> lies entirely on or within this box.</summary>
        public bool Contains(BoundingBox other)
        {
            return this.Contains(other.min) && this.Contains(other.max);
        }

        /// <summary>Whether this box overlaps <paramref name="other"/> (touching faces count as intersecting).</summary>
        public bool Intersects(BoundingBox other)
        {
            return this.min.X <= other.max.X && this.max.X >= other.min.X
                && this.min.Y <= other.max.Y && this.max.Y >= other.min.Y
                && this.min.Z <= other.max.Z && this.max.Z >= other.min.Z;
        }

        /// <summary>The point on or within the box closest to <paramref name="point"/> (the point itself if inside).</summary>
        public Vector ClosestPoint(Vector point)
        {
            return point.Clamp(this.min, this.max);
        }

        /// <summary>The distance from <paramref name="point"/> to the box (zero if inside or on it).</summary>
        public double DistanceTo(Vector point)
        {
            return (point - this.ClosestPoint(point)).Magnitude;
        }

        #endregion

        #region Combination

        /// <summary>The smallest box containing both this one and <paramref name="point"/>.</summary>
        public BoundingBox Merge(Vector point)
        {
            return new BoundingBox(this.min.ComponentMin(point), this.max.ComponentMax(point));
        }

        /// <summary>The smallest box containing both this one and <paramref name="other"/> (their union).</summary>
        public BoundingBox Merge(BoundingBox other)
        {
            return new BoundingBox(this.min.ComponentMin(other.min), this.max.ComponentMax(other.max));
        }

        /// <summary>A copy grown by <paramref name="margin"/> on every side (shrunk if negative).</summary>
        public BoundingBox Expand(double margin)
        {
            Vector m = new Vector(margin, margin, margin);
            return new BoundingBox(this.min - m, this.max + m);
        }

        #endregion

        #region Intersection with a line or ray

        /// <summary>
        /// Intersect the box with an infinite <see cref="Line"/> by the <b>slab method</b>: an AABB is the
        /// overlap of three pairs of parallel planes ("slabs"), one pair per axis. For each axis we find the
        /// span of the line parameter that lies between that axis's two planes, and intersect the three
        /// spans; the line hits the box exactly when a non-empty span survives. Returns true with the entry
        /// (<paramref name="near"/>) and exit (<paramref name="far"/>) points; false when the line misses.
        /// </summary>
        public bool TryIntersect(Line line, out Vector near, out Vector far)
        {
            return this.Slab(line.Point, line.Direction, double.NegativeInfinity, out double tNear, out double tFar)
                ? Hit(line.PointAt(tNear), line.PointAt(tFar), out near, out far)
                : Miss(line.Point, out near, out far);
        }

        /// <summary>
        /// Intersect the box with a <see cref="Ray"/> (the slab method, restricted to the forward half).
        /// Returns true with the first surface point at or ahead of the origin (the origin if it starts
        /// inside); false when missed or behind.
        /// </summary>
        public bool TryIntersect(Ray ray, out Vector point)
        {
            if (this.Slab(ray.Origin, ray.Direction, 0.0, out double tNear, out double tFar))
            {
                point = ray.PointAt(tNear);
                return true;
            }

            point = ray.Origin;
            return false;
        }

        /// <summary>
        /// The shared slab test. Returns the parameter span [<paramref name="tNear"/>, <paramref name="tFar"/>]
        /// for which <c>origin + t·direction</c> is inside all three axis slabs, clamped so <c>tNear</c> is
        /// at least <paramref name="tMinLimit"/> (negative-infinity for a line, zero for a ray). False when
        /// the span is empty.
        /// </summary>
        private bool Slab(Vector origin, Vector direction, double tMinLimit, out double tNear, out double tFar)
        {
            double tMin = tMinLimit;
            double tMax = double.PositiveInfinity;

            for (int axis = 0; axis < 3; axis++)
            {
                double o = origin[axis];
                double d = direction[axis];
                double lo = this.min[axis];
                double hi = this.max[axis];

                if (Math.Abs(d) < 1e-15)
                {
                    // Parallel to this slab: a hit is only possible if the origin already lies between its planes.
                    if (o < lo || o > hi)
                    {
                        tNear = tFar = 0;
                        return false;
                    }
                }
                else
                {
                    double t1 = (lo - o) / d;
                    double t2 = (hi - o) / d;
                    if (t1 > t2) { double swap = t1; t1 = t2; t2 = swap; }

                    if (t1 > tMin) tMin = t1;
                    if (t2 < tMax) tMax = t2;

                    if (tMin > tMax)
                    {
                        tNear = tFar = 0;
                        return false;
                    }
                }
            }

            tNear = tMin;
            tFar = tMax;
            return true;
        }

        private static bool Hit(Vector n, Vector f, out Vector near, out Vector far)
        {
            near = n;
            far = f;
            return true;
        }

        private static bool Miss(Vector fallback, out Vector near, out Vector far)
        {
            near = far = fallback;
            return false;
        }

        #endregion

        #region Operators

        /// <summary>Equality of both corners.</summary>
        public static bool operator ==(BoundingBox b1, BoundingBox b2)
        {
            return b1.min == b2.min && b1.max == b2.max;
        }

        /// <summary>Inequality of either corner.</summary>
        public static bool operator !=(BoundingBox b1, BoundingBox b2)
        {
            return !(b1 == b2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is BoundingBox b && this.Equals(b);
        }

        /// <summary>Equality with another box (both corners).</summary>
        public bool Equals(BoundingBox other)
        {
            return this == other;
        }

        /// <summary>Equality with another box within an absolute tolerance on every corner component.</summary>
        public bool Equals(BoundingBox other, double tolerance)
        {
            return this.min.Equals(other.min, tolerance) && this.max.Equals(other.max, tolerance);
        }

        /// <summary>A hash code derived from the two corners.</summary>
        public override int GetHashCode()
        {
            return this.min.GetHashCode() ^ this.max.GetHashCode();
        }

        /// <summary>Deconstruct into the minimum and maximum corners.</summary>
        public void Deconstruct(out Vector min, out Vector max)
        {
            min = this.min;
            max = this.max;
        }

        /// <summary>A string of the form <c>BoundingBox[min → max]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>BoundingBox[min → max]</c> where components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "BoundingBox[{0} → {1}]",
                this.min.ToString(),
                this.max.ToString());
        }

        #endregion

        #region Transform, translate and interpolate

        /// <summary>
        /// The smallest axis-aligned box that contains this box after it is moved by <paramref name="offset"/>.
        /// (A pure translation keeps the box axis-aligned, so this is exact.)
        /// </summary>
        public BoundingBox Translate(Vector offset)
        {
            return new BoundingBox(this.min + offset, this.max + offset);
        }

        /// <summary>
        /// The smallest axis-aligned box that contains this box after every corner is transformed by
        /// <paramref name="matrix"/>. A rotation tilts the box, so the result is re-fitted around the eight
        /// transformed corners and is generally larger than the original (the standard cost of staying
        /// axis-aligned).
        /// </summary>
        public BoundingBox Transform(Matrix matrix)
        {
            Vector[] corners = this.Corners;
            Vector lo = matrix * corners[0];
            Vector hi = lo;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector p = matrix * corners[i];
                lo = lo.ComponentMin(p);
                hi = hi.ComponentMax(p);
            }

            return new BoundingBox(lo, hi);
        }

        /// <summary>
        /// The smallest axis-aligned box that contains this box after every corner is placed by
        /// <paramref name="pose"/> (re-fitted around the transformed corners, as <see cref="Transform(Matrix)"/>).
        /// </summary>
        public BoundingBox Transform(Pose pose)
        {
            Vector[] corners = this.Corners;
            Vector lo = pose.Apply(corners[0]);
            Vector hi = lo;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector p = pose.Apply(corners[i]);
                lo = lo.ComponentMin(p);
                hi = hi.ComponentMax(p);
            }

            return new BoundingBox(lo, hi);
        }

        /// <summary>Linearly interpolate between two boxes, corner by corner (<paramref name="t"/> in [0, 1]).</summary>
        public static BoundingBox Lerp(BoundingBox a, BoundingBox b, double t)
        {
            return new BoundingBox(
                a.min + (b.min - a.min) * t,
                a.max + (b.max - a.max) * t);
        }

        #endregion

        #region Sphere queries and box intersection

        /// <summary>Whether this box overlaps the <paramref name="sphere"/> (touching counts as intersecting).</summary>
        public bool Intersects(BoundingSphere sphere)
        {
            // Mirrors BoundingSphere.Intersects(BoundingBox): they meet when the box's closest point is within the radius.
            return this.DistanceTo(sphere.Center) <= sphere.Radius;
        }

        /// <summary>Whether the <paramref name="sphere"/> lies entirely on or within this box.</summary>
        public bool Contains(BoundingSphere sphere)
        {
            Vector c = sphere.Center;
            double r = sphere.Radius;
            return c.X - r >= this.min.X && c.X + r <= this.max.X
                && c.Y - r >= this.min.Y && c.Y + r <= this.max.Y
                && c.Z - r >= this.min.Z && c.Z + r <= this.max.Z;
        }

        /// <summary>
        /// The overlap of this box and <paramref name="other"/>, or an empty box when they are disjoint.
        /// Use <see cref="IsEmpty"/> to detect the disjoint case.
        /// </summary>
        public BoundingBox Intersection(BoundingBox other)
        {
            Vector lo = this.min.ComponentMax(other.min);
            Vector hi = this.max.ComponentMin(other.max);

            if (lo.X > hi.X || lo.Y > hi.Y || lo.Z > hi.Z)
            {
                return Empty;
            }

            return new BoundingBox(lo, hi, unsorted: true);
        }

        /// <summary>The smallest box containing both this one and <paramref name="other"/> — an alias of <see cref="Merge(BoundingBox)"/>.</summary>
        public BoundingBox Union(BoundingBox other) => this.Merge(other);

        #endregion

        #region Empty accumulator

        /// <summary>
        /// Direct (unsorted) corner constructor for the inverted <see cref="Empty"/> sentinel and for
        /// intersection results that are already known to be ordered.
        /// </summary>
        private BoundingBox(Vector min, Vector max, bool unsorted)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// The "inverted" empty box (min at +∞, max at −∞). It contains nothing, but merging any point or box
        /// into it yields exactly that point or box — so it is the correct starting value when accumulating a
        /// bounding box over a sequence with <see cref="Merge(Vector)"/>.
        /// </summary>
        public static readonly BoundingBox Empty = new BoundingBox(
            new Vector(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity),
            new Vector(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity),
            unsorted: true);

        /// <summary>Whether the box is degenerate/inverted on any axis (its <see cref="Max"/> is below its <see cref="Min"/>).</summary>
        public bool IsEmpty
        {
            get { return this.max.X < this.min.X || this.max.Y < this.min.Y || this.max.Z < this.min.Z; }
        }

        #endregion

        #region Messages

        private const string NO_POINTS = "At least one point is required to build a bounding box.";

        #endregion
    }
}
