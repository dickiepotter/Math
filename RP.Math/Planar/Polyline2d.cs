namespace RP.Math
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Math = System.Math;

    /// <summary>
    /// An <b>open line through points</b> in order - a path, a pen stroke, a route - with no join from the last
    /// point back to the first (that would make it a <see cref="Polygon2d"/>). Immutable, and a read-only list of
    /// its points.
    /// </summary>
    public sealed class Polyline2d : IReadOnlyList<Vector2d>, IEquatable<Polyline2d>
    {
        private readonly Vector2d[] points;

        /// <exception cref="ArgumentException">Fewer than two points.</exception>
        public Polyline2d(IEnumerable<Vector2d> points)
        {
            this.points = (points ?? throw new ArgumentNullException(nameof(points))).ToArray();
            if (this.points.Length < 2)
            {
                throw new ArgumentException("A line needs at least two points.", nameof(points));
            }
        }

        public Polyline2d(params Vector2d[] points)
            : this((IEnumerable<Vector2d>)points)
        {
        }

        public int Count => this.points.Length;

        public Vector2d this[int index] => this.points[index];

        public Vector2d Start => this.points[0];

        public Vector2d End => this.points[this.points.Length - 1];

        /// <summary>The straight pieces between consecutive points.</summary>
        public IEnumerable<Segment2d> Segments
        {
            get
            {
                for (int i = 1; i < this.points.Length; i++)
                {
                    yield return new Segment2d(this.points[i - 1], this.points[i]);
                }
            }
        }

        /// <summary>The length along the line.</summary>
        public double Length => this.Segments.Sum(s => s.Length);

        public Box2d Bounds => Box2d.FromPoints(this.points);

        /// <summary>
        /// The point a given distance along the line from its start (clamped to the ends): walk the segments,
        /// subtracting each one's length, until the distance left falls within one.
        /// </summary>
        public Vector2d PointAtDistance(double distance)
        {
            if (distance <= 0)
            {
                return this.Start;
            }

            foreach (var segment in this.Segments)
            {
                double length = segment.Length;
                if (distance <= length)
                {
                    return length == 0 ? segment.Start : segment.PointAt(distance / length);
                }

                distance -= length;
            }

            return this.End;
        }

        /// <summary>The point of the line nearest to the given point.</summary>
        public Vector2d ClosestPoint(Vector2d point) =>
            this.Segments.Select(s => s.ClosestPoint(point)).OrderBy(p => Vector2d.DistanceSquared(p, point)).First();

        public double DistanceTo(Vector2d point) => Vector2d.Distance(point, this.ClosestPoint(point));

        /// <summary>The same line, from the other end.</summary>
        public Polyline2d Reversed() => new Polyline2d(Enumerable.Reverse(this.points));

        /// <summary>Points placed evenly along the line, <paramref name="spacing"/> apart, always including both ends.</summary>
        public Polyline2d Resample(double spacing)
        {
            if (spacing <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spacing), "The spacing must be positive.");
            }

            double length = this.Length;
            int steps = Math.Max(1, (int)Math.Ceiling(length / spacing));
            var result = new List<Vector2d>(steps + 1);
            for (int i = 0; i <= steps; i++)
            {
                result.Add(this.PointAtDistance(length * i / steps));
            }

            return new Polyline2d(result);
        }

        /// <summary>The line with fewer points, every point dropped lying within <paramref name="tolerance"/> of what is kept.</summary>
        public Polyline2d Simplify(double tolerance) => new Polyline2d(DouglasPeucker(this.points, tolerance));

        /// <summary>
        /// The <b>Douglas-Peucker</b> simplification of an open line: the indices kept always include both ends.
        /// </summary>
        /// <remarks>
        /// Draw a straight line from the first point to the last. Find the point furthest from it. If that point
        /// is within the tolerance, every point between the ends is - so the straight line will do, and only the
        /// ends are kept. Otherwise keep the furthest point too, and do the same to the line on each side of it.
        /// Each step either finishes a stretch or splits it at a point that must stay, so the result keeps the
        /// shape's important corners and drops the near-straight wobbles.
        /// </remarks>
        public static List<Vector2d> DouglasPeucker(IReadOnlyList<Vector2d> line, double tolerance)
        {
            if (line.Count <= 2)
            {
                return line.ToList();
            }

            var keep = new bool[line.Count];
            keep[0] = true;
            keep[line.Count - 1] = true;
            var stretches = new Stack<(int First, int Last)>();
            stretches.Push((0, line.Count - 1));
            while (stretches.Count > 0)
            {
                var (first, last) = stretches.Pop();
                var chord = new Segment2d(line[first], line[last]);
                int furthest = -1;
                double furthestDistance = tolerance;
                for (int i = first + 1; i < last; i++)
                {
                    double distance = chord.DistanceTo(line[i]);
                    if (distance > furthestDistance)
                    {
                        furthestDistance = distance;
                        furthest = i;
                    }
                }

                if (furthest >= 0)
                {
                    keep[furthest] = true;
                    stretches.Push((first, furthest));
                    stretches.Push((furthest, last));
                }
            }

            var kept = new List<Vector2d>();
            for (int i = 0; i < line.Count; i++)
            {
                if (keep[i])
                {
                    kept.Add(line[i]);
                }
            }

            return kept;
        }

        public IEnumerator<Vector2d> GetEnumerator() => ((IEnumerable<Vector2d>)this.points).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Equals(Polyline2d? other) => other is not null && this.points.SequenceEqual(other.points);

        public override bool Equals(object? obj) => this.Equals(obj as Polyline2d);

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var p in this.points)
            {
                hash = (hash * 31) + p.GetHashCode();
            }

            return hash;
        }

        public override string ToString() => $"Polyline2d({this.points.Length} points, length {this.Length:G4})";
    }
}
