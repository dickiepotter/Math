namespace RP.Math
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Math = System.Math;

    /// <summary>Which way round a polygon's corners run.</summary>
    public enum Winding
    {
        /// <summary>Anticlockwise (counter-clockwise), with y up: the mathematical positive direction.</summary>
        Anticlockwise,

        /// <summary>Clockwise, with y up.</summary>
        Clockwise,

        /// <summary>No area at all (the corners lie on a line), so no direction.</summary>
        Degenerate,
    }

    /// <summary>
    /// A <b>polygon in the plane</b>: a closed outline through an ordered list of corners, the last joined back
    /// to the first. It is immutable - every operation returns a new polygon - and it is also a read-only list
    /// of its corners, so it can be passed wherever a list of points is wanted.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ideas this type demonstrates, each in its own method:
    /// </para>
    /// <list type="bullet">
    /// <item><b>Shoelace formula</b> for the area, whose sign also tells the winding (<see cref="SignedArea"/>).</item>
    /// <item><b>Winding number</b> for whether a point is inside, which copes with any shape (<see cref="WindingNumber"/>).</item>
    /// <item><b>Ear clipping</b> to cut a polygon into triangles (<see cref="Triangulate"/>).</item>
    /// <item><b>Andrew's monotone chain</b> for the convex hull of a set of points (<see cref="ConvexHull"/>).</item>
    /// <item><b>Douglas-Peucker</b> to simplify an outline to fewer corners within a tolerance (<see cref="Simplify"/>).</item>
    /// <item><b>Sutherland-Hodgman</b> to clip a polygon to a convex window (<see cref="ClipTo"/>).</item>
    /// </list>
    /// <para>
    /// Union, difference, intersection and offsetting of arbitrary polygons are a much harder problem (robust
    /// implementations run to thousands of lines); they are described by <see cref="IPolygonClipper"/> so that a
    /// specialist library can supply them.
    /// </para>
    /// </remarks>
    public sealed class Polygon2d : IReadOnlyList<Vector2d>, IEquatable<Polygon2d>
    {
        private readonly Vector2d[] corners;

        /// <summary>A polygon through the corners, in order. Repeating the first corner at the end is allowed and ignored.</summary>
        /// <exception cref="ArgumentException">Fewer than three distinct corners.</exception>
        public Polygon2d(IEnumerable<Vector2d> corners)
        {
            if (corners == null)
            {
                throw new ArgumentNullException(nameof(corners));
            }

            var list = corners.ToList();
            if (list.Count > 1 && list[0] == list[list.Count - 1])
            {
                list.RemoveAt(list.Count - 1); // a closed list written out with the first point again
            }

            if (list.Count < 3)
            {
                throw new ArgumentException("A polygon needs at least three corners.", nameof(corners));
            }

            this.corners = list.ToArray();
        }

        /// <summary>A polygon through the corners, in order.</summary>
        public Polygon2d(params Vector2d[] corners)
            : this((IEnumerable<Vector2d>)corners)
        {
        }

        /// <summary>An axis-aligned rectangle, anticlockwise from its lower-left corner.</summary>
        public static Polygon2d Rectangle(double x, double y, double width, double height) =>
            new Polygon2d(new Vector2d(x, y), new Vector2d(x + width, y), new Vector2d(x + width, y + height), new Vector2d(x, y + height));

        /// <summary>A regular polygon (all sides and angles equal), anticlockwise, with its first corner straight right of the centre.</summary>
        public static Polygon2d Regular(Vector2d centre, double radius, int sides)
        {
            if (sides < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(sides), "A polygon needs at least three sides.");
            }

            var points = new Vector2d[sides];
            for (int i = 0; i < sides; i++)
            {
                double angle = 2 * Math.PI * i / sides;
                points[i] = centre + new Vector2d(radius * Math.Cos(angle), radius * Math.Sin(angle));
            }

            return new Polygon2d(points);
        }

        /// <summary>How many corners.</summary>
        public int Count => this.corners.Length;

        /// <summary>A corner. Indices wrap round, so <c>[-1]</c> is the last corner and <c>[Count]</c> the first.</summary>
        public Vector2d this[int index] => this.corners[((index % this.corners.Length) + this.corners.Length) % this.corners.Length];

        /// <summary>The edges, each from a corner to the next, the last back to the first.</summary>
        public IEnumerable<Segment2d> Edges
        {
            get
            {
                for (int i = 0; i < this.corners.Length; i++)
                {
                    yield return new Segment2d(this[i], this[i + 1]);
                }
            }
        }

        // ------------------------------------------------------------------------------------------- measuring

        /// <summary>
        /// The area, positive when the corners run anticlockwise and negative when clockwise.
        /// </summary>
        /// <remarks>
        /// The <b>shoelace formula</b>: add up <c>x[i] * y[i+1] - x[i+1] * y[i]</c> round the polygon and halve it.
        /// Each term is twice the signed area of the triangle from the origin to one edge; triangles for edges
        /// going one way round add, those going back subtract, and what is left is exactly the polygon's area.
        /// The name comes from the criss-cross pattern of the products when the coordinates are written in two
        /// columns. It works for any simple polygon, convex or not.
        /// </remarks>
        public double SignedArea => SignedAreaOf(this.corners);

        /// <summary>The area enclosed, whichever way the corners run.</summary>
        public double Area => Math.Abs(this.SignedArea);

        /// <summary>The shoelace formula for any list of corners: positive anticlockwise.</summary>
        public static double SignedAreaOf(IReadOnlyList<Vector2d> corners)
        {
            double twiceArea = 0;
            for (int i = 0; i < corners.Count; i++)
            {
                var a = corners[i];
                var b = corners[(i + 1) % corners.Count];
                twiceArea += (a.X * b.Y) - (b.X * a.Y);
            }

            return twiceArea / 2;
        }

        /// <summary>Which way the corners run.</summary>
        public Winding Winding
        {
            get
            {
                double area = this.SignedArea;
                return area > 0 ? Winding.Anticlockwise : area < 0 ? Winding.Clockwise : Winding.Degenerate;
            }
        }

        /// <summary>The length of the outline.</summary>
        public double Perimeter => this.Edges.Sum(e => e.Length);

        /// <summary>The smallest axis-aligned box containing the polygon.</summary>
        public Box2d Bounds => Box2d.FromPoints(this.corners);

        /// <summary>
        /// The centre of area: where a cut-out of the shape would balance on a pin.
        /// </summary>
        /// <remarks>
        /// Each shoelace term is a triangle from the origin to an edge; its centre is a third of the way along
        /// the sum of the edge's two corners. Averaging those centres, each weighted by its (signed) triangle
        /// area, gives the centroid of the whole shape. This differs from the plain average of the corners, which
        /// is pulled towards wherever corners crowd together.
        /// </remarks>
        public Vector2d Centroid
        {
            get
            {
                double twiceArea = 0, cx = 0, cy = 0;
                for (int i = 0; i < this.corners.Length; i++)
                {
                    var a = this[i];
                    var b = this[i + 1];
                    double cross = (a.X * b.Y) - (b.X * a.Y);
                    twiceArea += cross;
                    cx += (a.X + b.X) * cross;
                    cy += (a.Y + b.Y) * cross;
                }

                if (twiceArea == 0)
                {
                    // no area: fall back to the average of the corners
                    return new Vector2d(this.corners.Average(c => c.X), this.corners.Average(c => c.Y));
                }

                return new Vector2d(cx / (3 * twiceArea), cy / (3 * twiceArea));
            }
        }

        /// <summary>
        /// True when every interior angle is less than 180 degrees: walking round it, you always turn the same way.
        /// </summary>
        /// <remarks>
        /// At each corner, the cross product of the edge arriving and the edge leaving says which way you turn.
        /// A convex polygon turns the same way at every corner (straight-on corners do not count either way).
        /// </remarks>
        public bool IsConvex
        {
            get
            {
                int turn = 0;
                for (int i = 0; i < this.corners.Length; i++)
                {
                    double cross = Vector2d.Cross(this[i] - this[i - 1], this[i + 1] - this[i]);
                    int sign = cross > 0 ? 1 : cross < 0 ? -1 : 0;
                    if (sign == 0)
                    {
                        continue;
                    }

                    if (turn != 0 && sign != turn)
                    {
                        return false;
                    }

                    turn = sign;
                }

                return turn != 0;
            }
        }

        /// <summary>
        /// True when no two edges cross or touch except neighbours at their shared corner: an outline that does
        /// not loop over itself. Checks every pair of edges, so it is slow for very large polygons.
        /// </summary>
        public bool IsSimple
        {
            get
            {
                var edges = this.Edges.ToArray();
                for (int i = 0; i < edges.Length; i++)
                {
                    for (int j = i + 1; j < edges.Length; j++)
                    {
                        bool neighbours = j == i + 1 || (i == 0 && j == edges.Length - 1);
                        var kind = edges[i].Intersect(edges[j], out _);
                        if (kind == SegmentIntersectionKind.Overlap || (kind == SegmentIntersectionKind.Point && !neighbours))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        // ------------------------------------------------------------------------------------------- points

        /// <summary>
        /// How many times the outline winds round the point: 0 outside, +1 inside an anticlockwise polygon, -1
        /// inside a clockwise one, and more for an outline that loops round more than once.
        /// </summary>
        /// <remarks>
        /// Imagine a ray from the point heading right. Every edge that crosses it going up adds one; every edge
        /// crossing it going down takes one away. Unlike just counting crossings (even = outside, odd = inside),
        /// this gives the right answer for outlines that overlap themselves, and its sign says which way round
        /// the outline goes. The "is it left of the edge" test is the same cross product as
        /// <see cref="Segment2d.SideOf"/>.
        /// </remarks>
        public int WindingNumber(Vector2d point)
        {
            int winding = 0;
            for (int i = 0; i < this.corners.Length; i++)
            {
                var a = this[i];
                var b = this[i + 1];
                double side = Vector2d.Cross(b - a, point - a);
                if (a.Y <= point.Y)
                {
                    if (b.Y > point.Y && side > 0)
                    {
                        winding++; // an upward crossing with the point on its left
                    }
                }
                else if (b.Y <= point.Y && side < 0)
                {
                    winding--; // a downward crossing with the point on its right
                }
            }

            return winding;
        }

        /// <summary>True when the point is inside the polygon (a point exactly on the outline may go either way; see <see cref="IsOnBoundary"/>).</summary>
        public bool Contains(Vector2d point) => this.WindingNumber(point) != 0;

        /// <summary>True when the point lies on the outline, within a tolerance.</summary>
        public bool IsOnBoundary(Vector2d point, double tolerance = 1e-9) => this.DistanceToBoundary(point) <= tolerance;

        /// <summary>The distance from the point to the nearest part of the outline.</summary>
        public double DistanceToBoundary(Vector2d point) => this.Edges.Min(e => e.DistanceTo(point));

        /// <summary>The point of the outline nearest to the given point.</summary>
        public Vector2d ClosestPointOnBoundary(Vector2d point)
        {
            Vector2d best = this[0];
            double bestDistance = double.MaxValue;
            foreach (var edge in this.Edges)
            {
                var candidate = edge.ClosestPoint(point);
                double distance = Vector2d.Distance(candidate, point);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        // ------------------------------------------------------------------------------------------- reshaping

        /// <summary>The same outline with its corners in the opposite order (its winding reversed).</summary>
        public Polygon2d Reversed() => new Polygon2d(Enumerable.Reverse(this.corners));

        /// <summary>The same outline running the given way round.</summary>
        public Polygon2d WithWinding(Winding winding) =>
            winding == Winding.Degenerate || this.Winding == winding || this.Winding == Winding.Degenerate ? this : this.Reversed();

        /// <summary>Every corner moved by an offset.</summary>
        public Polygon2d Translated(Vector2d by) => new Polygon2d(this.corners.Select(c => c + by));

        /// <summary>Every corner turned about a centre, anticlockwise by the angle in radians.</summary>
        public Polygon2d Rotated(double radians, Vector2d centre) => new Polygon2d(this.corners.Select(c => centre + (c - centre).Rotate(radians)));

        /// <summary>Every corner scaled away from (or towards) a centre.</summary>
        public Polygon2d Scaled(double factor, Vector2d centre) => new Polygon2d(this.corners.Select(c => centre + ((c - centre) * factor)));

        /// <summary>Every corner put through a function: for any transform of your own.</summary>
        public Polygon2d Transformed(Func<Vector2d, Vector2d> transform) => new Polygon2d(this.corners.Select(transform));

        /// <summary>
        /// The outline with fewer corners: every corner removed lies within <paramref name="tolerance"/> of the
        /// simplified outline, so the shape barely changes.
        /// </summary>
        /// <remarks>
        /// The <b>Douglas-Peucker</b> algorithm, applied to the closed outline: keep the two corners furthest
        /// apart, then for each stretch between kept corners find the corner furthest from the straight line
        /// across it. If even that one is within the tolerance, the whole stretch can be replaced by the line;
        /// otherwise keep it and repeat on the two halves. See <see cref="Polyline2d.Simplify"/> for the open form.
        /// </remarks>
        public Polygon2d Simplify(double tolerance)
        {
            // split the loop at its two furthest-apart corners, simplify each half as an open line, rejoin
            int first = 0, second = 0;
            double furthest = -1;
            for (int i = 0; i < this.corners.Length; i++)
            {
                double d = Vector2d.DistanceSquared(this.corners[0], this.corners[i]);
                if (d > furthest)
                {
                    furthest = d;
                    second = i;
                }
            }

            var there = new List<Vector2d>();
            for (int i = first; i <= second; i++)
            {
                there.Add(this.corners[i]);
            }

            var back = new List<Vector2d>();
            for (int i = second; i <= this.corners.Length; i++)
            {
                back.Add(this[i]);
            }

            var keptThere = Polyline2d.DouglasPeucker(there, tolerance);
            var keptBack = Polyline2d.DouglasPeucker(back, tolerance);
            var result = keptThere.Take(keptThere.Count - 1).Concat(keptBack.Take(keptBack.Count - 1)).ToList();
            return result.Count >= 3 ? new Polygon2d(result) : this;
        }

        /// <summary>
        /// The part of this polygon inside a convex window - a viewport, a tile, a clip rectangle.
        /// Null when nothing of it is inside.
        /// </summary>
        /// <remarks>
        /// The <b>Sutherland-Hodgman</b> algorithm. Take the window's edges one at a time; each edge's line cuts
        /// the plane in two, and the polygon is trimmed to the inside half: walk round its corners, keeping those
        /// inside and adding a new corner wherever the outline crosses the line. After every edge of the window,
        /// what is left is inside them all - inside the window. It only works when the window is convex.
        /// </remarks>
        /// <exception cref="ArgumentException">The window is not convex.</exception>
        public Polygon2d? ClipTo(Polygon2d convexWindow)
        {
            if (!convexWindow.IsConvex)
            {
                throw new ArgumentException("Sutherland-Hodgman clipping needs a convex window.", nameof(convexWindow));
            }

            var window = convexWindow.WithWinding(Winding.Anticlockwise);
            var output = new List<Vector2d>(this.corners);
            foreach (var clipEdge in window.Edges)
            {
                var input = output;
                output = new List<Vector2d>();
                for (int i = 0; i < input.Count; i++)
                {
                    var current = input[i];
                    var previous = input[(i + input.Count - 1) % input.Count];
                    bool currentInside = clipEdge.SideOf(current) >= 0; // left of an anticlockwise edge is inside
                    bool previousInside = clipEdge.SideOf(previous) >= 0;
                    if (currentInside != previousInside)
                    {
                        output.Add(LineCrossing(previous, current, clipEdge));
                    }

                    if (currentInside)
                    {
                        output.Add(current);
                    }
                }

                if (output.Count == 0)
                {
                    return null;
                }
            }

            return output.Count >= 3 && Math.Abs(SignedAreaOf(output)) > 0 ? new Polygon2d(output) : null;
        }

        // where the segment from a to b crosses the infinite line through the clip edge
        private static Vector2d LineCrossing(Vector2d a, Vector2d b, Segment2d line)
        {
            double sideA = line.SideOf(a), sideB = line.SideOf(b);
            double t = sideA / (sideA - sideB);
            return a + ((b - a) * t);
        }

        // ------------------------------------------------------------------------------------------- triangles and hulls

        /// <summary>
        /// The polygon cut into triangles, as triples of corner indices, each anticlockwise. A simple polygon
        /// with n corners always gives n - 2 triangles.
        /// </summary>
        /// <remarks>
        /// <b>Ear clipping</b>. An "ear" is a corner where the polygon turns inward-convex and whose triangle with
        /// its two neighbours has no other corner inside it: that triangle can be cut off and the polygon is still
        /// simple, one corner smaller. Every simple polygon with more than three corners has at least two ears
        /// (the "two ears theorem"), so repeating this always finishes. It is quadratic in the number of corners,
        /// which is fine for the outlines people draw.
        /// </remarks>
        public IReadOnlyList<(int A, int B, int C)> Triangulate()
        {
            var triangles = new List<(int, int, int)>();
            bool anticlockwise = this.SignedArea >= 0;
            var remaining = Enumerable.Range(0, this.corners.Length).ToList();
            if (!anticlockwise)
            {
                remaining.Reverse(); // work anticlockwise, so "convex corner" means a left turn
            }

            int guard = remaining.Count * remaining.Count;
            while (remaining.Count > 3 && guard-- > 0)
            {
                bool clipped = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int prev = remaining[(i + remaining.Count - 1) % remaining.Count];
                    int curr = remaining[i];
                    int next = remaining[(i + 1) % remaining.Count];
                    if (!this.IsEar(prev, curr, next, remaining))
                    {
                        continue;
                    }

                    triangles.Add((prev, curr, next));
                    remaining.RemoveAt(i);
                    clipped = true;
                    break;
                }

                if (!clipped)
                {
                    break; // not a simple polygon: no ear to cut
                }
            }

            if (remaining.Count == 3)
            {
                triangles.Add((remaining[0], remaining[1], remaining[2]));
            }

            return triangles;
        }

        private bool IsEar(int prev, int curr, int next, List<int> remaining)
        {
            Vector2d a = this.corners[prev], b = this.corners[curr], c = this.corners[next];
            if (Vector2d.Cross(b - a, c - b) <= 0)
            {
                return false; // a reflex (or straight) corner cannot be cut off
            }

            foreach (int other in remaining)
            {
                if (other != prev && other != curr && other != next && InTriangle(this.corners[other], a, b, c))
                {
                    return false;
                }
            }

            return true;
        }

        // inside or on the edge of the anticlockwise triangle a, b, c: left of (or on) all three edges
        private static bool InTriangle(Vector2d p, Vector2d a, Vector2d b, Vector2d c) =>
            Vector2d.Cross(b - a, p - a) >= 0 && Vector2d.Cross(c - b, p - b) >= 0 && Vector2d.Cross(a - c, p - c) >= 0;

        /// <summary>
        /// The convex hull of a set of points: the smallest convex polygon containing them all, like a rubber band
        /// snapped round pins. Anticlockwise; null when the points are all on one line.
        /// </summary>
        /// <remarks>
        /// <b>Andrew's monotone chain</b>. Sort the points left to right. Walk along them building the lower edge
        /// of the hull: whenever the last two points kept and the new one make a clockwise turn, the middle one
        /// cannot be on the hull, so drop it. Walk back right to left the same way for the upper edge, and join the
        /// two. Sorting dominates, so it runs in n log n.
        /// </remarks>
        public static Polygon2d? ConvexHull(IEnumerable<Vector2d> points)
        {
            var sorted = points.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            if (sorted.Count < 3)
            {
                return null;
            }

            var hull = new List<Vector2d>();
            // lower hull, left to right
            foreach (var p in sorted)
            {
                while (hull.Count >= 2 && Vector2d.Cross(hull[hull.Count - 1] - hull[hull.Count - 2], p - hull[hull.Count - 1]) <= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                }

                hull.Add(p);
            }

            // upper hull, right to left
            int lowerCount = hull.Count;
            for (int i = sorted.Count - 2; i >= 0; i--)
            {
                var p = sorted[i];
                while (hull.Count > lowerCount && Vector2d.Cross(hull[hull.Count - 1] - hull[hull.Count - 2], p - hull[hull.Count - 1]) <= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                }

                hull.Add(p);
            }

            hull.RemoveAt(hull.Count - 1); // the start, reached again
            return hull.Count >= 3 ? new Polygon2d(hull) : null;
        }

        // ------------------------------------------------------------------------------------------- list and equality

        public IEnumerator<Vector2d> GetEnumerator() => ((IEnumerable<Vector2d>)this.corners).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>The corners as a new array.</summary>
        public Vector2d[] ToArray() => (Vector2d[])this.corners.Clone();

        /// <summary>Equal when the corners are the same, in the same order, from the same starting corner.</summary>
        public bool Equals(Polygon2d? other) => other is not null && this.corners.SequenceEqual(other.corners);

        public override bool Equals(object? obj) => this.Equals(obj as Polygon2d);

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var c in this.corners)
            {
                hash = (hash * 31) + c.GetHashCode();
            }

            return hash;
        }

        public override string ToString() => $"Polygon2d({this.corners.Length} corners, area {this.Area:G4})";
    }
}
