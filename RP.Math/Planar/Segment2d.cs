namespace RP.Math
{
    using System;

    using Math = System.Math;

    /// <summary>How two segments meet.</summary>
    public enum SegmentIntersectionKind
    {
        /// <summary>They do not touch.</summary>
        None,

        /// <summary>They cross or touch at a single point.</summary>
        Point,

        /// <summary>They lie along the same line and share a stretch of it.</summary>
        Overlap,
    }

    /// <summary>
    /// A <b>straight line segment</b> in the plane: the piece of line between two end points. The edges of every
    /// polygon in this library are segments.
    /// </summary>
    /// <remarks>
    /// Most of what a segment can do comes from writing its points as <c>Start + t * (End - Start)</c>, where
    /// <c>t</c> runs from 0 at the start to 1 at the end. Finding the closest point, or where two segments
    /// cross, comes down to finding the right <c>t</c> and checking it lies between 0 and 1.
    /// </remarks>
    public readonly struct Segment2d : IEquatable<Segment2d>
    {
        public Segment2d(Vector2d start, Vector2d end)
        {
            this.Start = start;
            this.End = end;
        }

        public Vector2d Start { get; }

        public Vector2d End { get; }

        /// <summary>The vector from start to end (not normalised: its length is the segment's).</summary>
        public Vector2d Direction => this.End - this.Start;

        public double Length => Vector2d.Distance(this.Start, this.End);

        public Vector2d Midpoint => (this.Start + this.End) / 2;

        public Box2d Bounds => new Box2d(this.Start, this.End);

        /// <summary>The point a fraction <paramref name="t"/> of the way along (0 the start, 1 the end).</summary>
        public Vector2d PointAt(double t) => this.Start + (this.Direction * t);

        /// <summary>
        /// The fraction along the segment of the point nearest to <paramref name="point"/>, between 0 and 1.
        /// </summary>
        /// <remarks>
        /// Project the point onto the segment's line with a dot product, which gives how far along the line it
        /// falls as a fraction of the segment's length; then clamp that to the segment's ends.
        /// </remarks>
        public double ClosestFraction(Vector2d point)
        {
            var d = this.Direction;
            double lengthSquared = Vector2d.Dot(d, d);
            if (lengthSquared == 0)
            {
                return 0; // a segment of no length: its one point is the closest
            }

            double t = Vector2d.Dot(point - this.Start, d) / lengthSquared;
            return t < 0 ? 0 : t > 1 ? 1 : t;
        }

        /// <summary>The point of the segment nearest to <paramref name="point"/>.</summary>
        public Vector2d ClosestPoint(Vector2d point) => this.PointAt(this.ClosestFraction(point));

        /// <summary>How far the point is from the segment.</summary>
        public double DistanceTo(Vector2d point) => Vector2d.Distance(point, this.ClosestPoint(point));

        /// <summary>
        /// Which side of the segment's line a point is on: positive to the left (looking from start to end),
        /// negative to the right, zero on the line. The value is twice the area of the triangle the three points
        /// make - the 2D cross product - which is why it is also the basis of <see cref="Polygon2d.SignedArea"/>.
        /// </summary>
        public double SideOf(Vector2d point) => Vector2d.Cross(this.Direction, point - this.Start);

        /// <summary>
        /// Where this segment meets another.
        /// </summary>
        /// <param name="other">The other segment.</param>
        /// <param name="point">The meeting point (for an overlap, the start of the shared stretch).</param>
        /// <param name="tolerance">How close to parallel counts as parallel.</param>
        /// <remarks>
        /// Write both segments as <c>P + t r</c> and <c>Q + u s</c> and solve <c>P + t r = Q + u s</c>. Taking the
        /// 2D cross product of both sides with <c>s</c>, and then with <c>r</c>, removes one unknown each time:
        /// <c>t = (Q - P) x s / (r x s)</c> and <c>u = (Q - P) x r / (r x s)</c>. They meet when both <c>t</c> and
        /// <c>u</c> lie between 0 and 1. When <c>r x s</c> is zero the segments are parallel: they meet only if
        /// they lie on the same line and their ranges along it overlap.
        /// </remarks>
        public SegmentIntersectionKind Intersect(Segment2d other, out Vector2d point, double tolerance = 1e-12)
        {
            var r = this.Direction;
            var s = other.Direction;
            var qp = other.Start - this.Start;
            double rxs = Vector2d.Cross(r, s);
            double qpxr = Vector2d.Cross(qp, r);
            point = default;

            if (Math.Abs(rxs) <= tolerance)
            {
                if (Math.Abs(qpxr) > tolerance)
                {
                    return SegmentIntersectionKind.None; // parallel, on different lines
                }

                // on the same line: compare where the other's ends fall along this one
                double rr = Vector2d.Dot(r, r);
                if (rr == 0)
                {
                    bool touches = other.DistanceTo(this.Start) <= Math.Sqrt(tolerance);
                    point = this.Start;
                    return touches ? SegmentIntersectionKind.Point : SegmentIntersectionKind.None;
                }

                double t0 = Vector2d.Dot(qp, r) / rr;
                double t1 = t0 + (Vector2d.Dot(s, r) / rr);
                double low = Math.Max(0, Math.Min(t0, t1));
                double high = Math.Min(1, Math.Max(t0, t1));
                if (low > high)
                {
                    return SegmentIntersectionKind.None;
                }

                point = this.PointAt(low);
                return low == high ? SegmentIntersectionKind.Point : SegmentIntersectionKind.Overlap;
            }

            double t = Vector2d.Cross(qp, s) / rxs;
            double u = qpxr / rxs;
            if (t < 0 || t > 1 || u < 0 || u > 1)
            {
                return SegmentIntersectionKind.None;
            }

            point = this.PointAt(t);
            return SegmentIntersectionKind.Point;
        }

        /// <summary>True when the segments touch or cross.</summary>
        public bool Intersects(Segment2d other) => this.Intersect(other, out _) != SegmentIntersectionKind.None;

        /// <summary>The same segment, running the other way.</summary>
        public Segment2d Reversed() => new Segment2d(this.End, this.Start);

        public bool Equals(Segment2d other) => this.Start.Equals(other.Start) && this.End.Equals(other.End);

        public override bool Equals(object? obj) => obj is Segment2d other && this.Equals(other);

        public override int GetHashCode() => (this.Start.GetHashCode() * 397) ^ this.End.GetHashCode();

        public override string ToString() => $"Segment2d({this.Start} to {this.End})";
    }
}
