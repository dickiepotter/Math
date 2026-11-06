namespace RP.Math
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Math = System.Math;

    /// <summary>
    /// <b>Mending a triangle surface</b> so it becomes the closed, consistently wound skin that
    /// <see cref="TriangleSurface"/> can measure: the faults model files commonly arrive with, fixed one
    /// algorithm at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A surface here is a list of corner positions and a list of faces, each face three indices into the
    /// corners, wound anticlockwise seen from outside. Every method changes the face list in place (and, for
    /// <see cref="FillHoles"/>, adds corners) and returns how much it did, so they can be run one after another:
    /// </para>
    /// <list type="number">
    /// <item><see cref="Weld"/> - corners that should be one but sit a hair apart are made one, so neighbouring
    /// faces really share their edges. A grid of cells the size of the tolerance means each corner is compared
    /// only with the few in the cells round it, not with every other corner.</item>
    /// <item><see cref="RemoveDegenerate"/> and <see cref="RemoveDuplicates"/> - faces with no area, or the same
    /// face twice, are dropped.</item>
    /// <item><see cref="OrientConsistently"/> - a breadth-first walk across shared edges turns each face to run
    /// the opposite way to its neighbour along the edge they share; then each separate piece that encloses a
    /// negative volume is inside out, and is turned round whole.</item>
    /// <item><see cref="FillHoles"/> - the edges used by only one face form loops round each hole; walking a
    /// loop the opposite way to its faces and fanning triangles from its centre closes it.</item>
    /// </list>
    /// </remarks>
    public static class SurfaceRepair
    {
        /// <summary>
        /// Makes corners closer than <paramref name="tolerance"/> into one and renumbers the faces to match.
        /// Returns, for each corner kept, the index it had before, so the caller can rebuild its corner list
        /// (<c>newCorners[i] = oldCorners[kept[i]]</c>). The number welded away is the old count less the new.
        /// </summary>
        public static int[] Weld(IReadOnlyList<Vector3d> corners, List<(int A, int B, int C)> faces, double tolerance)
        {
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            if (faces == null) throw new ArgumentNullException(nameof(faces));

            double cell = Math.Max(tolerance, 1e-12);
            var newIndex = new int[corners.Count];
            var kept = new List<int>();
            var cells = new Dictionary<(long, long, long), List<int>>();
            for (int i = 0; i < corners.Count; i++)
            {
                var p = corners[i];
                var key = ((long)Math.Floor(p.X / cell), (long)Math.Floor(p.Y / cell), (long)Math.Floor(p.Z / cell));
                int found = FindNear(corners, kept, cells, key, p, tolerance);
                if (found < 0)
                {
                    found = kept.Count;
                    kept.Add(i);
                    if (!cells.TryGetValue(key, out var bucket))
                    {
                        bucket = new List<int>();
                        cells[key] = bucket;
                    }

                    bucket.Add(found);
                }

                newIndex[i] = found;
            }

            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                faces[f] = (newIndex[face.A], newIndex[face.B], newIndex[face.C]);
            }

            return kept.ToArray();
        }

        /// <summary>Drops faces that use a corner twice or have (next to) no area. Returns how many went.</summary>
        public static int RemoveDegenerate(IReadOnlyList<Vector3d> corners, List<(int A, int B, int C)> faces, double smallestArea = 1e-12)
        {
            return faces.RemoveAll(f => f.A == f.B || f.B == f.C || f.A == f.C
                || TriangleSurface.Area(corners[f.A], corners[f.B], corners[f.C]) < smallestArea);
        }

        /// <summary>Drops the second of two faces on the same three corners, whichever way round. Returns how many went.</summary>
        public static int RemoveDuplicates(List<(int A, int B, int C)> faces)
        {
            var seen = new HashSet<(int, int, int)>();
            return faces.RemoveAll(f => !seen.Add(Sorted(f)));
        }

        /// <summary>
        /// Winds every face the same way as its neighbours, then turns any piece that encloses a negative volume
        /// (it is inside out) the right way round. Returns how many faces end up turned (a face turned twice,
        /// once to match its neighbours and again with its whole piece, is back as it was and not counted).
        /// </summary>
        public static int OrientConsistently(IReadOnlyList<Vector3d> corners, List<(int A, int B, int C)> faces)
        {
            var original = faces.ToArray();

            // which faces use each edge, whichever way
            var byEdge = new Dictionary<(int, int), List<int>>();
            for (int f = 0; f < faces.Count; f++)
            {
                foreach (var (a, b) in Edges(faces[f]))
                {
                    var key = Undirected(a, b);
                    if (!byEdge.TryGetValue(key, out var list))
                    {
                        list = new List<int>();
                        byEdge[key] = list;
                    }

                    list.Add(f);
                }
            }

            var reached = new bool[faces.Count];
            var piece = new List<int>();
            var queue = new Queue<int>();
            for (int start = 0; start < faces.Count; start++)
            {
                if (reached[start])
                {
                    continue;
                }

                // spread out from one face across shared edges, setting each neighbour to match
                piece.Clear();
                reached[start] = true;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    int f = queue.Dequeue();
                    piece.Add(f);
                    foreach (var (a, b) in Edges(faces[f]))
                    {
                        foreach (int g in byEdge[Undirected(a, b)])
                        {
                            if (reached[g])
                            {
                                continue;
                            }

                            // a neighbour must run along the shared edge from b to a; running a to b, it is turned
                            if (Edges(faces[g]).Contains((a, b)))
                            {
                                faces[g] = Reversed(faces[g]);
                            }

                            reached[g] = true;
                            queue.Enqueue(g);
                        }
                    }
                }

                // the piece is consistent now, but maybe consistently inside out
                double sixTimesVolume = 0;
                foreach (int f in piece)
                {
                    var face = faces[f];
                    sixTimesVolume += corners[face.A].DotProduct(corners[face.B].CrossProduct(corners[face.C]));
                }

                if (sixTimesVolume < 0)
                {
                    foreach (int f in piece)
                    {
                        faces[f] = Reversed(faces[f]);
                    }
                }
            }

            int turned = 0;
            for (int f = 0; f < faces.Count; f++)
            {
                if (!faces[f].Equals(original[f]))
                {
                    turned++;
                }
            }

            return turned;
        }

        /// <summary>
        /// The holes: each a loop of corners round edges that only one face uses, in the order a face closing the
        /// hole would run (the opposite way to the faces beside it). Loops that cross themselves at a shared
        /// corner are left out, as there is no single way to read them.
        /// </summary>
        public static IReadOnlyList<IReadOnlyList<int>> HoleLoops(IEnumerable<(int A, int B, int C)> faces)
        {
            var directed = new HashSet<(int, int)>();
            foreach (var f in faces)
            {
                foreach (var e in Edges(f))
                {
                    directed.Add(e);
                }
            }

            // an edge with no partner running the other way is on a hole's rim; the hole runs back along it
            var next = new Dictionary<int, int>();
            foreach (var (a, b) in directed)
            {
                if (!directed.Contains((b, a)) && !next.ContainsKey(b))
                {
                    next[b] = a;
                }
            }

            var loops = new List<IReadOnlyList<int>>();
            var visited = new HashSet<int>();
            foreach (int start in next.Keys.ToList())
            {
                if (visited.Contains(start))
                {
                    continue;
                }

                var loop = new List<int>();
                int at = start;
                while (!visited.Contains(at) && next.TryGetValue(at, out int to) && loop.Count <= next.Count)
                {
                    visited.Add(at);
                    loop.Add(at);
                    at = to;
                }

                if (at == start && loop.Count >= 3)
                {
                    loops.Add(loop);
                }
            }

            return loops;
        }

        /// <summary>
        /// Closes the holes of up to <paramref name="largestHole"/> edges: a triangle for a three-edged hole, a fan
        /// from a new corner at the loop's centre for a bigger one. Bigger holes are left, since filling them
        /// guesses at a shape. Returns how many were closed and how many left open.
        /// </summary>
        public static (int Filled, int LeftOpen) FillHoles(IList<Vector3d> corners, List<(int A, int B, int C)> faces, int largestHole = 64)
        {
            int filled = 0, open = 0;
            foreach (var loop in HoleLoops(faces))
            {
                if (loop.Count > largestHole)
                {
                    open++;
                    continue;
                }

                if (loop.Count == 3)
                {
                    faces.Add((loop[0], loop[1], loop[2]));
                }
                else
                {
                    var centre = Vector3d.Zero;
                    foreach (int i in loop)
                    {
                        centre += corners[i];
                    }

                    int c = corners.Count;
                    corners.Add(centre / loop.Count);
                    for (int i = 0; i < loop.Count; i++)
                    {
                        faces.Add((loop[i], loop[(i + 1) % loop.Count], c));
                    }
                }

                filled++;
            }

            return (filled, open);
        }

        /// <summary>
        /// Renumbers the faces to use only the corners they need, in the order first used. Returns, for each corner
        /// kept, the index it had before (<c>newCorners[i] = oldCorners[kept[i]]</c>).
        /// </summary>
        public static int[] RemoveUnusedCorners(int cornerCount, List<(int A, int B, int C)> faces)
        {
            var newIndex = new int[cornerCount];
            for (int i = 0; i < newIndex.Length; i++)
            {
                newIndex[i] = -1;
            }

            var kept = new List<int>();
            int Map(int i)
            {
                if (newIndex[i] < 0)
                {
                    newIndex[i] = kept.Count;
                    kept.Add(i);
                }

                return newIndex[i];
            }

            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                faces[f] = (Map(face.A), Map(face.B), Map(face.C));
            }

            return kept.ToArray();
        }

        /// <summary>Edges used by one face only: the rims of holes.</summary>
        public static int OpenEdgeCount(IEnumerable<(int A, int B, int C)> faces)
        {
            return EdgeUse(faces).Count(e => e.Value.Count == 1);
        }

        /// <summary>Edges whose two faces both run along them the same way: neighbours wound opposite ways.</summary>
        public static int MisorientedEdgeCount(IEnumerable<(int A, int B, int C)> faces)
        {
            return EdgeUse(faces).Count(e => e.Value.Count == 2 && e.Value[0] == e.Value[1]);
        }

        private static int FindNear(IReadOnlyList<Vector3d> corners, List<int> kept, Dictionary<(long, long, long), List<int>> cells, (long, long, long) key, Vector3d p, double tolerance)
        {
            for (long dx = -1; dx <= 1; dx++)
            {
                for (long dy = -1; dy <= 1; dy++)
                {
                    for (long dz = -1; dz <= 1; dz++)
                    {
                        if (cells.TryGetValue((key.Item1 + dx, key.Item2 + dy, key.Item3 + dz), out var list))
                        {
                            foreach (int k in list)
                            {
                                if ((corners[kept[k]] - p).MagnitudeSquared <= tolerance * tolerance)
                                {
                                    return k;
                                }
                            }
                        }
                    }
                }
            }

            return -1;
        }

        // for each undirected edge, the ways it is used (true: from the lower index to the higher)
        private static Dictionary<(int, int), List<bool>> EdgeUse(IEnumerable<(int A, int B, int C)> faces)
        {
            var use = new Dictionary<(int, int), List<bool>>();
            foreach (var f in faces)
            {
                foreach (var (a, b) in Edges(f))
                {
                    var key = Undirected(a, b);
                    if (!use.TryGetValue(key, out var list))
                    {
                        list = new List<bool>();
                        use[key] = list;
                    }

                    list.Add(a < b);
                }
            }

            return use;
        }

        private static (int A, int B)[] Edges((int A, int B, int C) f)
        {
            return new[] { (f.A, f.B), (f.B, f.C), (f.C, f.A) };
        }

        private static (int, int) Undirected(int a, int b)
        {
            return a < b ? (a, b) : (b, a);
        }

        private static (int A, int B, int C) Reversed((int A, int B, int C) f)
        {
            return (f.A, f.C, f.B);
        }

        private static (int, int, int) Sorted((int A, int B, int C) f)
        {
            int lo = Math.Min(f.A, Math.Min(f.B, f.C));
            int hi = Math.Max(f.A, Math.Max(f.B, f.C));
            return (lo, f.A + f.B + f.C - lo - hi, hi);
        }
    }
}
