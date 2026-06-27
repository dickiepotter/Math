namespace RP.Math
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <b>bounding sphere</b>: a centre and a radius used as a cheap stand-in for the thing it encloses.
    /// Like an axis-aligned <see cref="BoundingBox"/>, it exists for <i>quick rejection</i> — the overlap test
    /// between two spheres is a single distance comparison — but it is rotation-independent, so it does not
    /// need re-fitting when the enclosed object turns.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the bounding-volume partner of <see cref="BoundingBox"/>. It is distinct from
    /// <see cref="PlacedSphere"/>: that is a geometric sphere with full surface/volume maths and exact
    /// line/ray hits, whereas this is a lightweight enclosing volume focused on containment and overlap.
    /// Treated as a filled ball: <see cref="Contains(Vector)"/> means "on or within".
    /// </para>
    /// <para>Built on <see cref="Vector"/>, following the library's immutable design.</para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct BoundingSphere : IEquatable<BoundingSphere>, IFormattable
    {
        #region Fields

        private readonly Vector center;
        private readonly double radius;

        #endregion

        #region Constructors

        /// <summary>Construct a bounding sphere from a centre and a radius.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="radius"/> is negative.</exception>
        public BoundingSphere(Vector center, double radius)
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, NEGATIVE_RADIUS);
            this.center = center;
            this.radius = radius;
        }

        #endregion

        #region Factories

        /// <summary>A bounding sphere that contains all of <paramref name="points"/>.</summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if no points are supplied.</exception>
        public static BoundingSphere FromPoints(params Vector[] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            return FromPoints((IEnumerable<Vector>)points);
        }

        /// <summary>
        /// A bounding sphere that contains all of <paramref name="points"/>, fitted by <b>Ritter's
        /// algorithm</b>: take a rough sphere spanning the two points found farthest apart in a quick scan,
        /// then sweep the points once more, each time the current sphere falls short growing it by just
        /// enough to swallow the stray point. The result is a small (though not provably the smallest)
        /// enclosing sphere — cheap to build and good enough for rejection.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if no points are supplied.</exception>
        public static BoundingSphere FromPoints(IEnumerable<Vector> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));

            var list = new List<Vector>(points);
            if (list.Count == 0) throw new ArgumentException(NO_POINTS, nameof(points));
            if (list.Count == 1) return new BoundingSphere(list[0], 0);

            // 1. From an arbitrary point, find the farthest point y; from y, find the farthest point z.
            //    y and z are a good guess at the longest diameter of the cloud.
            Vector y = Farthest(list, list[0]);
            Vector z = Farthest(list, y);

            // 2. Start with the sphere that just spans y..z.
            Vector c = (y + z) * 0.5;
            double r = y.Distance(z) * 0.5;

            // 3. Grow to include any point still outside, moving the centre as little as possible.
            foreach (Vector p in list)
            {
                double d = c.Distance(p);
                if (d > r)
                {
                    double newRadius = (r + d) * 0.5;
                    // Shift the centre toward p by the amount the radius grew.
                    c = c + ((newRadius - r) / d * (p - c));
                    r = newRadius;
                }
            }

            return new BoundingSphere(c, r);
        }

        private static Vector Farthest(List<Vector> points, Vector from)
        {
            Vector best = from;
            double bestSq = -1;
            foreach (Vector p in points)
            {
                double sq = from.DistanceSquared(p);
                if (sq > bestSq)
                {
                    bestSq = sq;
                    best = p;
                }
            }

            return best;
        }

        #endregion

        #region Accessors

        /// <summary>The centre of the sphere.</summary>
        public Vector Center { get { return this.center; } }

        /// <summary>The radius of the sphere.</summary>
        public double Radius { get { return this.radius; } }

        /// <summary>The diameter of the sphere.</summary>
        public double Diameter { get { return 2.0 * this.radius; } }

        /// <summary>The enclosed volume.</summary>
        public double Volume { get { return 4.0 / 3.0 * Math.PI * this.radius * this.radius * this.radius; } }

        /// <summary>The surface area.</summary>
        public double SurfaceArea { get { return 4.0 * Math.PI * this.radius * this.radius; } }

        #endregion

        #region Containment and queries

        /// <summary>Whether <paramref name="point"/> lies on or within the sphere.</summary>
        public bool Contains(Vector point)
        {
            return this.center.DistanceSquared(point) <= this.radius * this.radius;
        }

        /// <summary>Whether <paramref name="point"/> lies on or within the sphere, with the radius grown by <paramref name="tolerance"/>.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            double r = this.radius + tolerance;
            return this.center.DistanceSquared(point) <= r * r;
        }

        /// <summary>Whether <paramref name="other"/> lies entirely on or within this sphere.</summary>
        public bool Contains(BoundingSphere other)
        {
            // The far side of `other` must still be inside this one.
            return this.center.Distance(other.center) + other.radius <= this.radius;
        }

        /// <summary>Whether this sphere overlaps <paramref name="other"/> (touching counts as intersecting).</summary>
        public bool Intersects(BoundingSphere other)
        {
            double sum = this.radius + other.radius;
            return this.center.DistanceSquared(other.center) <= sum * sum;
        }

        /// <summary>Whether this sphere overlaps the axis-aligned <paramref name="box"/> (touching counts as intersecting).</summary>
        public bool Intersects(BoundingBox box)
        {
            // The sphere meets the box exactly when the box's closest point is within the radius.
            return box.DistanceTo(this.center) <= this.radius;
        }

        /// <summary>The point on or within the sphere closest to <paramref name="point"/> (the point itself if inside).</summary>
        public Vector ClosestPoint(Vector point)
        {
            Vector offset = point - this.center;
            double distance = offset.Magnitude;
            if (distance <= this.radius || distance == 0)
            {
                return point;
            }

            return this.center + (offset / distance * this.radius);
        }

        /// <summary>The distance from <paramref name="point"/> to the surface (zero on it, negative inside).</summary>
        public double SignedDistanceTo(Vector point)
        {
            return this.center.Distance(point) - this.radius;
        }

        #endregion

        #region Combination

        /// <summary>The smallest sphere containing both this one and <paramref name="point"/>.</summary>
        public BoundingSphere Merge(Vector point)
        {
            double d = this.center.Distance(point);
            if (d <= this.radius) return this; // already inside

            double newRadius = (this.radius + d) * 0.5;
            Vector newCenter = this.center + ((newRadius - this.radius) / d * (point - this.center));
            return new BoundingSphere(newCenter, newRadius);
        }

        /// <summary>The smallest sphere containing both this one and <paramref name="other"/>.</summary>
        public BoundingSphere Merge(BoundingSphere other)
        {
            Vector between = other.center - this.center;
            double distance = between.Magnitude;

            // One already swallows the other: return the larger.
            if (distance + other.radius <= this.radius) return this;
            if (distance + this.radius <= other.radius) return other;

            double newRadius = (distance + this.radius + other.radius) * 0.5;
            Vector newCenter = distance == 0
                ? this.center
                : this.center + ((newRadius - this.radius) / distance * between);
            return new BoundingSphere(newCenter, newRadius);
        }

        /// <summary>A copy with the radius grown by <paramref name="margin"/> (shrunk if negative, never below zero).</summary>
        public BoundingSphere Expand(double margin)
        {
            return new BoundingSphere(this.center, Math.Max(0, this.radius + margin));
        }

        #endregion

        #region Operators

        /// <summary>Equality of centre and radius.</summary>
        public static bool operator ==(BoundingSphere s1, BoundingSphere s2)
        {
            return s1.center == s2.center && s1.radius == s2.radius;
        }

        /// <summary>Inequality of centre or radius.</summary>
        public static bool operator !=(BoundingSphere s1, BoundingSphere s2)
        {
            return !(s1 == s2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is BoundingSphere s && this.Equals(s);
        }

        /// <summary>Equality with another bounding sphere (centre and radius).</summary>
        public bool Equals(BoundingSphere other)
        {
            return this == other;
        }

        /// <summary>Equality with another bounding sphere within an absolute tolerance.</summary>
        public bool Equals(BoundingSphere other, double tolerance)
        {
            return this.center.Equals(other.center, tolerance)
                && Math.Abs(this.radius - other.radius) <= tolerance;
        }

        /// <summary>A hash code derived from the centre and radius.</summary>
        public override int GetHashCode()
        {
            return this.center.GetHashCode() ^ this.radius.GetHashCode();
        }

        /// <summary>Deconstruct into the centre and radius.</summary>
        public void Deconstruct(out Vector center, out double radius)
        {
            center = this.center;
            radius = this.radius;
        }

        /// <summary>A string of the form <c>BoundingSphere[centre, r=radius]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>BoundingSphere[centre, r=radius]</c>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "BoundingSphere[{0}, r={1}]",
                this.center.ToString(),
                this.radius.ToString(format, formatProvider));
        }

        #endregion

        #region Factories from other volumes

        /// <summary>The smallest sphere that encloses the axis-aligned <paramref name="box"/> (centred on the box, reaching its corners).</summary>
        public static BoundingSphere FromBox(BoundingBox box)
        {
            return new BoundingSphere(box.Center, box.Extents.Magnitude);
        }

        #endregion

        #region Transform, translate and interpolate

        /// <summary>A copy moved by <paramref name="offset"/> (the radius is unchanged).</summary>
        public BoundingSphere Translate(Vector offset)
        {
            return new BoundingSphere(this.center + offset, this.radius);
        }

        /// <summary>
        /// This sphere placed by <paramref name="pose"/>. A pose is a rigid motion (rotation + translation),
        /// so only the centre moves — the radius is preserved exactly.
        /// </summary>
        public BoundingSphere Transform(Pose pose)
        {
            return new BoundingSphere(pose.Apply(this.center), this.radius);
        }

        /// <summary>
        /// This sphere transformed by <paramref name="matrix"/>. The centre is transformed as a point and the
        /// radius is scaled by the largest of the matrix's three axis scale factors, so a non-uniform scale
        /// yields the enclosing sphere (conservative, never too small).
        /// </summary>
        public BoundingSphere Transform(Matrix matrix)
        {
            Vector o = matrix * Vector.Origin;
            double sx = (matrix * Vector.XAxis - o).Magnitude;
            double sy = (matrix * Vector.YAxis - o).Magnitude;
            double sz = (matrix * Vector.ZAxis - o).Magnitude;
            double scale = Math.Max(sx, Math.Max(sy, sz));

            return new BoundingSphere(matrix * this.center, this.radius * scale);
        }

        /// <summary>Linearly interpolate between two spheres — centre and radius blended (<paramref name="t"/> in [0, 1]).</summary>
        public static BoundingSphere Lerp(BoundingSphere a, BoundingSphere b, double t)
        {
            return new BoundingSphere(
                a.center + (b.center - a.center) * t,
                a.radius + (b.radius - a.radius) * t);
        }

        #endregion

        #region Box containment

        /// <summary>Whether the axis-aligned <paramref name="box"/> lies entirely on or within this sphere.</summary>
        public bool Contains(BoundingBox box)
        {
            // The whole box is inside iff its farthest corner from the centre is within the radius.
            Vector c = this.center;
            Vector min = box.Min;
            Vector max = box.Max;
            var farthest = new Vector(
                Math.Abs(min.X - c.X) > Math.Abs(max.X - c.X) ? min.X : max.X,
                Math.Abs(min.Y - c.Y) > Math.Abs(max.Y - c.Y) ? min.Y : max.Y,
                Math.Abs(min.Z - c.Z) > Math.Abs(max.Z - c.Z) ? min.Z : max.Z);

            return c.DistanceSquared(farthest) <= this.radius * this.radius;
        }

        #endregion

        #region Messages

        private const string NO_POINTS = "At least one point is required to build a bounding sphere.";
        private const string NEGATIVE_RADIUS = "A bounding sphere radius cannot be negative.";

        #endregion
    }
}
