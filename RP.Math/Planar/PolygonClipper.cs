namespace RP.Math
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Math = System.Math;

    /// <summary>
    /// <b>Boolean operations and offsetting</b> for regions in the plane, written to be read: union, intersection,
    /// difference and exclusive-or of any outlines (holes, overlaps, outlines that cross themselves), growing and
    /// shrinking a region by a distance, and clipping lines to a region.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The method is an <b>overlay</b>, in four steps, each one simple on its own:
    /// </para>
    /// <list type="number">
    /// <item><b>Split.</b> Put every edge of both inputs together and cut each one wherever another edge crosses
    /// or touches it. Afterwards no two pieces cross: they meet only at their ends.</item>
    /// <item><b>Classify.</b> Each piece has the same thing on its left all the way along, and the same on its
    /// right. Fire a ray from the piece's midpoint and count how many times each input's outlines wind round
    /// the point beside it (the <i>winding number</i>, as <see cref="Polygon2d.WindingNumber"/>). That says whether
    /// each side is inside the subject and inside the clip, and so - by the operation's truth table - inside
    /// the result.</item>
    /// <item><b>Keep.</b> A piece is part of the answer's outline exactly when the result is inside on one side and
    /// outside on the other. Turn it so the inside is on its left.</item>
    /// <item><b>Link.</b> Join the kept pieces end to end into closed outlines. With the inside always on the
    /// left, outer outlines come out anticlockwise and holes clockwise, as <see cref="Region2d"/> expects.</item>
    /// </list>
    /// <para>
    /// Floating-point arithmetic makes "the same point" slippery: an intersection worked out from one edge can
    /// differ in the last digit from the same point worked out from the other. So every point is <b>snapped</b> to
    /// a fine grid (<see cref="Precision"/>), and two points on the same grid square are the same point. That is
    /// the one approximation; with it, the topology - what joins what - is exact.
    /// </para>
    /// <para>
    /// Offsetting builds on the overlay: move each edge sideways by the distance, fill the gap at each corner (a
    /// mitre, an arc or a square end), and the raw outline may now loop over itself where the shape was too thin.
    /// A union with the <see cref="FillRule.Positive"/> rule keeps exactly the parts that wind the right way,
    /// which removes those loops.
    /// </para>
    /// </remarks>
    public sealed class PolygonClipper : IPolygonClipper
    {
        /// <summary>A clipper working to a millionth of a unit.</summary>
        public PolygonClipper()
            : this(1e-6)
        {
        }

        /// <summary>A clipper working to the given grid size.</summary>
        public PolygonClipper(double precision)
        {
            if (!(precision > 0))
            {
                throw new ArgumentOutOfRangeException(nameof(precision), "The precision must be positive.");
            }

            this.Precision = precision;
        }

        /// <summary>The snapping grid: points closer than this are treated as one.</summary>
        public double Precision { get; }

        /// <summary>A mitred corner sharper than this many times the offset distance is cut off square.</summary>
        public double MiterLimit { get; set; } = 2;

        /// <summary>How far a rounded corner's straight pieces may stray from the true arc, as a fraction of the distance.</summary>
        public double ArcTolerance { get; set; } = 0.005;

        // ------------------------------------------------------------------------------------------- the operations

        private enum Operation
        {
            Union,
            Intersection,
            Difference,
            Xor,
        }

        public Region2d Union(IEnumerable<IReadOnlyList<Vector2d>> outlines, FillRule fillRule = FillRule.NonZero) =>
            this.Overlay(outlines, Enumerable.Empty<IReadOnlyList<Vector2d>>(), Operation.Union, fillRule);

        public Region2d Intersection(IEnumerable<IReadOnlyList<Vector2d>> subject, IEnumerable<IReadOnlyList<Vector2d>> clip) =>
            this.Overlay(subject, clip, Operation.Intersection, FillRule.NonZero);

        public Region2d Difference(IEnumerable<IReadOnlyList<Vector2d>> subject, IEnumerable<IReadOnlyList<Vector2d>> clip) =>
            this.Overlay(subject, clip, Operation.Difference, FillRule.NonZero);

        public Region2d Xor(IEnumerable<IReadOnlyList<Vector2d>> subject, IEnumerable<IReadOnlyList<Vector2d>> clip) =>
            this.Overlay(subject, clip, Operation.Xor, FillRule.NonZero);

        // ------------------------------------------------------------------------------------------- the overlay

        // one piece of an input edge, after splitting; Owner 0 is the subject, 1 the clip
        private struct Piece
        {
            public Vector2d A;
            public Vector2d B;
            public int Owner;
            public int Group;
        }

        private Region2d Overlay(IEnumerable<IReadOnlyList<Vector2d>> subject, IEnumerable<IReadOnlyList<Vector2d>> clip, Operation operation, FillRule fillRule)
        {
            // step 1: gather the edges and split them where they meet
            var edges = new List<(Vector2d A, Vector2d B, int Owner)>();
            this.AddEdges(edges, subject, 0);
            this.AddEdges(edges, clip, 1);
            if (edges.Count == 0)
            {
                return Region2d.Empty;
            }

            var pieces = this.Split(edges);

            // pieces lying on top of each other (a shared edge) are one "group": classified once, kept at most once
            var groups = new Dictionary<(long, long, long, long), int>();
            var groupFirst = new List<int>();
            for (int i = 0; i < pieces.Count; i++)
            {
                var key = this.UndirectedKey(pieces[i].A, pieces[i].B);
                if (!groups.TryGetValue(key, out int g))
                {
                    g = groupFirst.Count;
                    groups[key] = g;
                    groupFirst.Add(i);
                }

                var piece = pieces[i];
                piece.Group = g;
                pieces[i] = piece;
            }

            // step 2 and 3: classify each group's two sides and keep the boundary pieces
            var bands = new RayIndex(pieces);
            var kept = new List<(Vector2d From, Vector2d To)>();
            var members = pieces.Select((p, i) => (p.Group, i)).GroupBy(x => x.Group).ToDictionary(x => x.Key, x => x.Select(y => y.i).ToList());
            for (int g = 0; g < groupFirst.Count; g++)
            {
                var first = pieces[groupFirst[g]];
                // a fixed direction for the group, so "left" means the same for all its pieces
                Vector2d p = first.A, q = first.B;
                if (Compare(p, q) > 0)
                {
                    var swap = p;
                    p = q;
                    q = swap;
                }

                var mid = (p + q) / 2;
                bool horizontal = Math.Abs(q.Y - p.Y) < Math.Abs(q.X - p.X) * 1e-12 || q.Y == p.Y;
                var near = bands.Cast(pieces, mid, horizontal, g);
                var far = near;
                foreach (int i in members[g])
                {
                    // the group's own pieces are crossed by the ray from the point on their other side
                    int sign = horizontal ? (pieces[i].B.X < pieces[i].A.X ? 1 : -1) : (pieces[i].B.Y > pieces[i].A.Y ? 1 : -1);
                    if (pieces[i].Owner == 0)
                    {
                        far.Subject += sign;
                    }
                    else
                    {
                        far.Clip += sign;
                    }
                }

                // "near" is the side the ray leaves from without crossing the group: east of an upright piece,
                // north of a level one; work out which of those is the left of p -> q
                bool nearIsLeft = horizontal ? q.X > p.X : q.Y < p.Y;
                var left = nearIsLeft ? near : far;
                var right = nearIsLeft ? far : near;
                bool insideLeft = Result(operation, left, fillRule);
                bool insideRight = Result(operation, right, fillRule);
                if (insideLeft != insideRight)
                {
                    kept.Add(insideLeft ? (p, q) : (q, p));
                }
            }

            // step 4: link the kept pieces into closed outlines
            return new Region2d(this.Link(kept));
        }

        private static bool Result(Operation operation, Winding2 w, FillRule fillRule)
        {
            bool s = Region2d.IsInside(w.Subject, fillRule);
            bool c = Region2d.IsInside(w.Clip, FillRule.NonZero);
            switch (operation)
            {
                case Operation.Intersection:
                    return s && c;
                case Operation.Difference:
                    return s && !c;
                case Operation.Xor:
                    return s != c;
                default:
                    return s || c;
            }
        }

        private void AddEdges(List<(Vector2d, Vector2d, int)> edges, IEnumerable<IReadOnlyList<Vector2d>> outlines, int owner)
        {
            foreach (var outline in outlines)
            {
                for (int i = 0; i < outline.Count; i++)
                {
                    var a = this.Snap(outline[i]);
                    var b = this.Snap(outline[(i + 1) % outline.Count]);
                    if (this.Key(a) != this.Key(b))
                    {
                        edges.Add((a, b, owner));
                    }
                }
            }
        }

        /// <summary>
        /// Cuts every edge wherever another crosses it, touches it, or runs along it. Edges are sorted left to right
        /// so each is only compared with those whose x ranges overlap it - a "sweep" that skips most pairs.
        /// </summary>
        private List<Piece> Split(List<(Vector2d A, Vector2d B, int Owner)> edges)
        {
            var order = Enumerable.Range(0, edges.Count).OrderBy(i => Math.Min(edges[i].A.X, edges[i].B.X)).ToArray();
            var cuts = new List<Vector2d>[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                cuts[i] = new List<Vector2d>();
            }

            double slack = this.Precision * 2;
            for (int oi = 0; oi < order.Length; oi++)
            {
                int i = order[oi];
                var e = edges[i];
                double maxX = Math.Max(e.A.X, e.B.X) + slack;
                double minY = Math.Min(e.A.Y, e.B.Y) - slack, maxY = Math.Max(e.A.Y, e.B.Y) + slack;
                for (int oj = oi + 1; oj < order.Length; oj++)
                {
                    int j = order[oj];
                    var f = edges[j];
                    if (Math.Min(f.A.X, f.B.X) > maxX)
                    {
                        break; // sorted by left end: nothing further along can reach back
                    }

                    if (Math.Max(f.A.Y, f.B.Y) < minY || Math.Min(f.A.Y, f.B.Y) > maxY)
                    {
                        continue;
                    }

                    this.Meet(e.A, e.B, f.A, f.B, cuts[i], cuts[j]);
                }
            }

            var pieces = new List<Piece>();
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                var d = e.B - e.A;
                double length2 = Vector2d.Dot(d, d);
                var points = cuts[i]
                    .Select(this.Snap)
                    .Where(c => this.Key(c) != this.Key(e.A) && this.Key(c) != this.Key(e.B))
                    .Select(c => (Point: c, T: Vector2d.Dot(c - e.A, d) / length2))
                    .Where(c => c.T > 0 && c.T < 1)
                    .OrderBy(c => c.T)
                    .Select(c => c.Point)
                    .ToList();
                var previous = e.A;
                foreach (var point in points.Concat(new[] { e.B }))
                {
                    if (this.Key(point) != this.Key(previous))
                    {
                        pieces.Add(new Piece { A = previous, B = point, Owner = e.Owner });
                        previous = point;
                    }
                }
            }

            return pieces;
        }

        // where two edges meet: a crossing point, or ends of one lying on the other (touching, or overlapping)
        private void Meet(Vector2d a, Vector2d b, Vector2d c, Vector2d d, List<Vector2d> onFirst, List<Vector2d> onSecond)
        {
            if (OnSegment(c, a, b, this.Precision))
            {
                onFirst.Add(c);
            }

            if (OnSegment(d, a, b, this.Precision))
            {
                onFirst.Add(d);
            }

            if (OnSegment(a, c, d, this.Precision))
            {
                onSecond.Add(a);
            }

            if (OnSegment(b, c, d, this.Precision))
            {
                onSecond.Add(b);
            }

            // a proper crossing, solved with 2D cross products as in Segment2d.Intersect
            var r = b - a;
            var s = d - c;
            double rxs = Vector2d.Cross(r, s);
            if (Math.Abs(rxs) < 1e-18)
            {
                return; // parallel: any meeting was an end on the other, found above
            }

            var qp = c - a;
            double t = Vector2d.Cross(qp, s) / rxs;
            double u = Vector2d.Cross(qp, r) / rxs;
            if (t > 0 && t < 1 && u > 0 && u < 1)
            {
                var point = a + (r * t);
                onFirst.Add(point);
                onSecond.Add(point);
            }
        }

        private static bool OnSegment(Vector2d p, Vector2d a, Vector2d b, double tolerance) =>
            new Segment2d(a, b).DistanceTo(p) <= tolerance;

        /// <summary>
        /// Joins directed pieces end to end into closed outlines. Where several pieces leave one point (two shapes
        /// touching at a corner), take the one that turns furthest left: with the inside always on the left, that
        /// keeps to the same face, so each outline goes round one shape rather than looping into its neighbour.
        /// </summary>
        private List<Polygon2d> Link(List<(Vector2d From, Vector2d To)> kept)
        {
            var leaving = new Dictionary<(long, long), List<int>>();
            for (int i = 0; i < kept.Count; i++)
            {
                var key = this.Key(kept[i].From);
                if (!leaving.TryGetValue(key, out var list))
                {
                    leaving[key] = list = new List<int>();
                }

                list.Add(i);
            }

            var used = new bool[kept.Count];
            var outlines = new List<Polygon2d>();
            for (int start = 0; start < kept.Count; start++)
            {
                if (used[start])
                {
                    continue;
                }

                var ring = new List<Vector2d>();
                var startKey = this.Key(kept[start].From);
                int current = start;
                bool closed = false;
                while (true)
                {
                    used[current] = true;
                    ring.Add(kept[current].From);
                    var at = kept[current].To;
                    if (this.Key(at) == startKey)
                    {
                        closed = true;
                        break;
                    }

                    if (!leaving.TryGetValue(this.Key(at), out var options))
                    {
                        break;
                    }

                    var incoming = kept[current].To - kept[current].From;
                    int next = -1;
                    double best = double.MinValue;
                    foreach (int option in options)
                    {
                        if (used[option])
                        {
                            continue;
                        }

                        var outgoing = kept[option].To - kept[option].From;
                        double turn = Math.Atan2(Vector2d.Cross(incoming, outgoing), Vector2d.Dot(incoming, outgoing));
                        if (turn > best)
                        {
                            best = turn;
                            next = option;
                        }
                    }

                    if (next < 0)
                    {
                        break;
                    }

                    current = next;
                }

                if (closed)
                {
                    var tidy = this.DropStraightCorners(ring);
                    if (tidy.Count >= 3 && Math.Abs(Polygon2d.SignedAreaOf(tidy)) > this.Precision * this.Precision)
                    {
                        outlines.Add(new Polygon2d(tidy));
                    }
                }
            }

            return outlines;
        }

        // corners where the outline runs straight on add nothing: splitting left many of them
        private List<Vector2d> DropStraightCorners(List<Vector2d> ring)
        {
            var result = new List<Vector2d>(ring);
            bool removed = true;
            while (removed && result.Count > 3)
            {
                removed = false;
                for (int i = 0; i < result.Count && result.Count > 3; i++)
                {
                    var a = result[(i + result.Count - 1) % result.Count];
                    var b = result[i];
                    var c = result[(i + 1) % result.Count];
                    if (new Segment2d(a, c).DistanceTo(b) <= this.Precision)
                    {
                        result.RemoveAt(i);
                        removed = true;
                        i--;
                    }
                }
            }

            return result;
        }

        // ------------------------------------------------------------------------------------------- snapping

        private Vector2d Snap(Vector2d p) => new Vector2d(Math.Round(p.X / this.Precision) * this.Precision, Math.Round(p.Y / this.Precision) * this.Precision);

        private (long, long) Key(Vector2d p) => ((long)Math.Round(p.X / this.Precision), (long)Math.Round(p.Y / this.Precision));

        private (long, long, long, long) UndirectedKey(Vector2d a, Vector2d b)
        {
            var ka = this.Key(a);
            var kb = this.Key(b);
            return Compare(a, b) <= 0 ? (ka.Item1, ka.Item2, kb.Item1, kb.Item2) : (kb.Item1, kb.Item2, ka.Item1, ka.Item2);
        }

        private static int Compare(Vector2d a, Vector2d b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y);

        // ------------------------------------------------------------------------------------------- winding numbers

        private struct Winding2
        {
            public int Subject;
            public int Clip;
        }

        /// <summary>
        /// Casting rays quickly. The pieces are filed into horizontal bands (for rays heading east) and vertical
        /// bands (for rays heading north), so a ray only looks at the pieces in its own band.
        /// </summary>
        private sealed class RayIndex
        {
            private readonly List<int>[] rows;
            private readonly List<int>[] columns;
            private readonly double minX, minY, rowHeight, columnWidth;

            public RayIndex(List<Piece> pieces)
            {
                this.minX = pieces.Min(p => Math.Min(p.A.X, p.B.X));
                this.minY = pieces.Min(p => Math.Min(p.A.Y, p.B.Y));
                double maxX = pieces.Max(p => Math.Max(p.A.X, p.B.X));
                double maxY = pieces.Max(p => Math.Max(p.A.Y, p.B.Y));
                int bands = Math.Max(1, (int)Math.Sqrt(pieces.Count));
                this.rowHeight = Math.Max((maxY - this.minY) / bands, 1e-12);
                this.columnWidth = Math.Max((maxX - this.minX) / bands, 1e-12);
                this.rows = Enumerable.Range(0, bands).Select(_ => new List<int>()).ToArray();
                this.columns = Enumerable.Range(0, bands).Select(_ => new List<int>()).ToArray();
                for (int i = 0; i < pieces.Count; i++)
                {
                    var p = pieces[i];
                    for (int r = this.Row(Math.Min(p.A.Y, p.B.Y)); r <= this.Row(Math.Max(p.A.Y, p.B.Y)); r++)
                    {
                        this.rows[r].Add(i);
                    }

                    for (int c = this.Column(Math.Min(p.A.X, p.B.X)); c <= this.Column(Math.Max(p.A.X, p.B.X)); c++)
                    {
                        this.columns[c].Add(i);
                    }
                }
            }

            private int Row(double y) => Clamp((int)((y - this.minY) / this.rowHeight), this.rows.Length);

            private int Column(double x) => Clamp((int)((x - this.minX) / this.columnWidth), this.columns.Length);

            private static int Clamp(int value, int count) => value < 0 ? 0 : value >= count ? count - 1 : value;

            /// <summary>
            /// The winding numbers at a point, from a ray heading east (or north, for a level piece), leaving out the
            /// pieces of the group the point sits on. The half-open rule - a piece counts when the ray's line passes
            /// its lower end but not its upper one - makes a ray through a corner count it exactly once.
            /// </summary>
            public Winding2 Cast(List<Piece> pieces, Vector2d from, bool north, int skipGroup)
            {
                var w = default(Winding2);
                var band = north ? this.columns[this.Column(from.X)] : this.rows[this.Row(from.Y)];
                foreach (int i in band)
                {
                    var p = pieces[i];
                    if (p.Group == skipGroup)
                    {
                        continue;
                    }

                    int sign = 0;
                    if (!north)
                    {
                        // heading east: an upward piece to the right counts +1, a downward one -1
                        if (p.A.Y <= from.Y && p.B.Y > from.Y)
                        {
                            sign = 1;
                        }
                        else if (p.B.Y <= from.Y && p.A.Y > from.Y)
                        {
                            sign = -1;
                        }

                        if (sign == 0)
                        {
                            continue;
                        }

                        double x = p.A.X + ((from.Y - p.A.Y) / (p.B.Y - p.A.Y) * (p.B.X - p.A.X));
                        if (x <= from.X)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // heading north: a piece running west (right to left) above counts +1, east -1
                        if (p.B.X <= from.X && p.A.X > from.X)
                        {
                            sign = 1;
                        }
                        else if (p.A.X <= from.X && p.B.X > from.X)
                        {
                            sign = -1;
                        }

                        if (sign == 0)
                        {
                            continue;
                        }

                        double y = p.A.Y + ((from.X - p.A.X) / (p.B.X - p.A.X) * (p.B.Y - p.A.Y));
                        if (y <= from.Y)
                        {
                            continue;
                        }
                    }

                    if (p.Owner == 0)
                    {
                        w.Subject += sign;
                    }
                    else
                    {
                        w.Clip += sign;
                    }
                }

                return w;
            }
        }

        // ------------------------------------------------------------------------------------------- offsetting

        /// <summary>
        /// The region grown outwards by <paramref name="delta"/> (shrunk when negative). Each edge moves sideways
        /// by the distance - outwards is to the right of an anticlockwise outline - and the gap left at each
        /// convex corner is filled with a mitre, an arc or a square end. A union with the
        /// <see cref="FillRule.Positive"/> rule then removes the inside-out loops a thin part leaves when shrunk.
        /// </summary>
        public Region2d Offset(IEnumerable<IReadOnlyList<Vector2d>> outlines, double delta, JoinStyle join = JoinStyle.Miter)
        {
            var list = outlines.Where(o => o.Count >= 3).ToList();
            if (delta == 0 || list.Count == 0)
            {
                return this.Union(list);
            }

            var raw = new List<IReadOnlyList<Vector2d>>();
            foreach (var outline in list)
            {
                var path = this.OffsetOutline(outline, delta, join);
                if (path.Count >= 3)
                {
                    raw.Add(path);
                }
            }

            return this.Union(raw, FillRule.Positive);
        }

        private List<Vector2d> OffsetOutline(IReadOnlyList<Vector2d> outline, double delta, JoinStyle join)
        {
            // drop repeated corners first: an edge of no length has no direction
            var corners = new List<Vector2d>();
            foreach (var p in outline)
            {
                if (corners.Count == 0 || Vector2d.Distance(corners[corners.Count - 1], p) > this.Precision)
                {
                    corners.Add(p);
                }
            }

            if (corners.Count > 1 && Vector2d.Distance(corners[0], corners[corners.Count - 1]) <= this.Precision)
            {
                corners.RemoveAt(corners.Count - 1);
            }

            int n = corners.Count;
            var path = new List<Vector2d>();
            if (n < 3)
            {
                return path;
            }

            // each edge's unit direction and its right-hand normal (outwards for an anticlockwise outline)
            var direction = new Vector2d[n];
            var normal = new Vector2d[n];
            for (int i = 0; i < n; i++)
            {
                direction[i] = (corners[(i + 1) % n] - corners[i]).Normalize();
                normal[i] = new Vector2d(direction[i].Y, -direction[i].X);
            }

            for (int i = 0; i < n; i++)
            {
                int before = (i + n - 1) % n;
                var p = corners[i];
                var u0 = normal[before] * delta; // where the edge arriving at this corner has moved to
                var u1 = normal[i] * delta;      // and the edge leaving it
                double turn = Vector2d.Cross(direction[before], direction[i]);
                if (turn * delta <= 0)
                {
                    // the moved edges overlap here (a concave corner, as seen from the side moving out): go in to the
                    // corner and out again; the loop this makes is removed by the union afterwards
                    path.Add(p + u0);
                    path.Add(p);
                    path.Add(p + u1);
                    continue;
                }

                // the moved edges have pulled apart: fill the gap
                switch (join)
                {
                    case JoinStyle.Round:
                        this.AddArc(path, p, u0, u1, Math.Abs(delta));
                        break;
                    case JoinStyle.Square:
                        path.Add(p + u0 + (direction[before] * Math.Abs(delta)));
                        path.Add(p + u1 - (direction[i] * Math.Abs(delta)));
                        break;
                    default:
                        double cosine = Vector2d.Dot(normal[before], normal[i]);
                        double length = Math.Sqrt(2 / (1 + cosine)); // the mitre's reach, in multiples of the distance
                        if (cosine > -0.999 && length <= this.MiterLimit)
                        {
                            path.Add(p + ((normal[before] + normal[i]) * (delta / (1 + cosine))));
                        }
                        else
                        {
                            path.Add(p + u0);
                            path.Add(p + u1);
                        }

                        break;
                }
            }

            return path;
        }

        // an arc round p from p + u0 to p + u1, the short way, in steps close enough to the true curve
        private void AddArc(List<Vector2d> path, Vector2d p, Vector2d u0, Vector2d u1, double radius)
        {
            double sweep = Math.Atan2(Vector2d.Cross(u0, u1), Vector2d.Dot(u0, u1));
            double tolerance = Math.Max(this.Precision * 4, radius * this.ArcTolerance);
            double step = 2 * Math.Acos(Math.Max(-1, 1 - (tolerance / radius)));
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / Math.Max(step, 1e-3)));
            for (int k = 0; k <= steps; k++)
            {
                path.Add(p + u0.Rotate(sweep * k / steps));
            }
        }

        // ------------------------------------------------------------------------------------------- lines

        /// <summary>
        /// The pieces of open lines inside a region: each line is cut wherever it crosses the region's outline,
        /// and a piece is kept when its midpoint is inside.
        /// </summary>
        public IReadOnlyList<Polyline2d> ClipLines(IEnumerable<IReadOnlyList<Vector2d>> lines, IEnumerable<IReadOnlyList<Vector2d>> region)
        {
            var boundary = new List<Piece>();
            foreach (var outline in region)
            {
                for (int i = 0; i < outline.Count; i++)
                {
                    var a = outline[i];
                    var b = outline[(i + 1) % outline.Count];
                    if (a != b)
                    {
                        boundary.Add(new Piece { A = a, B = b, Owner = 0, Group = -1 });
                    }
                }
            }

            var result = new List<Polyline2d>();
            if (boundary.Count == 0)
            {
                return result;
            }

            var index = new RayIndex(boundary);
            var byLeft = Enumerable.Range(0, boundary.Count).OrderBy(i => Math.Min(boundary[i].A.X, boundary[i].B.X)).ToArray();
            var lefts = byLeft.Select(i => Math.Min(boundary[i].A.X, boundary[i].B.X)).ToArray();
            double reach = boundary.Max(p => Math.Abs(p.B.X - p.A.X));
            foreach (var line in lines)
            {
                var current = new List<Vector2d>();
                for (int s = 0; s + 1 < line.Count; s++)
                {
                    var a = line[s];
                    var b = line[s + 1];
                    var d = b - a;
                    var ts = new List<double> { 0, 1 };
                    double lo = Math.Min(a.X, b.X), hi = Math.Max(a.X, b.X);

                    // only the boundary pieces whose x range can overlap this segment
                    int k = Array.BinarySearch(lefts, lo - reach);
                    if (k < 0)
                    {
                        k = ~k;
                    }

                    for (; k < byLeft.Length && lefts[k] <= hi; k++)
                    {
                        var e = boundary[byLeft[k]];
                        var r = e.B - e.A;
                        double rxs = Vector2d.Cross(d, r);
                        if (Math.Abs(rxs) < 1e-18)
                        {
                            continue;
                        }

                        var qp = e.A - a;
                        double t = Vector2d.Cross(qp, r) / rxs;
                        double u = Vector2d.Cross(qp, d) / rxs;
                        if (t > 0 && t < 1 && u >= 0 && u <= 1)
                        {
                            ts.Add(t);
                        }
                    }

                    ts.Sort();
                    for (int i = 0; i + 1 < ts.Count; i++)
                    {
                        if (ts[i + 1] - ts[i] < 1e-12)
                        {
                            continue;
                        }

                        var from = a + (d * ts[i]);
                        var to = a + (d * ts[i + 1]);
                        var w = index.Cast(boundary, (from + to) / 2, false, -2);
                        if (w.Subject != 0)
                        {
                            if (current.Count == 0)
                            {
                                current.Add(from);
                            }

                            current.Add(to);
                        }
                        else if (current.Count > 0)
                        {
                            result.Add(new Polyline2d(current));
                            current = new List<Vector2d>();
                        }
                    }
                }

                if (current.Count >= 2)
                {
                    result.Add(new Polyline2d(current));
                }
            }

            return result;
        }
    }
}
