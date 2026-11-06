namespace RP.Math
{
    using System;
    using System.Collections.Generic;

    using Math = System.Math;

    /// <summary>
    /// Measures of a <b>surface made of triangles</b> - the way 3D printers, games and CAD exchange solid shapes
    /// (STL, OBJ, 3MF): its area, the volume it encloses, the centre of that volume, and whether it is a closed
    /// skin at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Volume without slicing anything.</b> Join every triangle to the origin and it becomes a tetrahedron
    /// with volume <c>a · (b × c) / 6</c>. The sign of that triple product follows the triangle's winding seen
    /// from the origin: triangles facing away add volume, triangles facing towards it take it away. For a
    /// closed surface whose triangles all wind anticlockwise seen from outside, everything outside the shape
    /// cancels and exactly the enclosed volume is left - wherever the origin is. It is the 3D cousin of the
    /// shoelace formula (<see cref="Polygon2d.SignedArea"/>), and both are the divergence theorem in disguise.
    /// </para>
    /// <para>
    /// <b>Is it closed?</b> A watertight surface has every edge shared by exactly two triangles, running
    /// opposite ways along it (one triangle's A to B is its neighbour's B to A). An edge used once is the rim of
    /// a hole; used the same way twice, one of the two triangles is wound inside out.
    /// </para>
    /// <para>
    /// <b>Euler's formula.</b> For a closed surface, corners − edges + faces = 2 − 2g, where g is the number of
    /// holes right through it (a sphere 0, a ring 1). It is a quick check on a model, and a famous result.
    /// </para>
    /// </remarks>
    public static class TriangleSurface
    {
        /// <summary>A triangle's unit normal by the right-hand rule (anticlockwise corners face you), or zero if it has no area.</summary>
        public static Vector3d Normal(Vector3d a, Vector3d b, Vector3d c)
        {
            return (b - a).CrossProduct(c - a).NormalizeOrDefault();
        }

        /// <summary>A triangle's area: half the length of the cross product of two of its sides.</summary>
        public static double Area(Vector3d a, Vector3d b, Vector3d c)
        {
            return (b - a).CrossProduct(c - a).Magnitude / 2;
        }

        /// <summary>The total area of the triangles.</summary>
        public static double SurfaceArea(IEnumerable<(Vector3d A, Vector3d B, Vector3d C)> triangles)
        {
            double area = 0;
            foreach (var t in triangles)
            {
                area += Area(t.A, t.B, t.C);
            }

            return area;
        }

        /// <summary>
        /// The volume enclosed, by summing signed tetrahedra to the origin: positive for a closed surface wound
        /// outwards, negative for one inside out. It only means something for a closed surface (see
        /// <see cref="IsClosed"/>).
        /// </summary>
        public static double SignedVolume(IEnumerable<(Vector3d A, Vector3d B, Vector3d C)> triangles)
        {
            double sixTimes = 0;
            foreach (var t in triangles)
            {
                sixTimes += t.A.DotProduct(t.B.CrossProduct(t.C));
            }

            return sixTimes / 6;
        }

        /// <summary>
        /// The centre of the enclosed volume (its centre of mass, if it is solid and even): each tetrahedron's
        /// centre - the average of its four corners, one of them the origin - weighted by its signed volume.
        /// Returns <c>null</c> when there is no volume to weigh.
        /// </summary>
        public static Vector3d? Centroid(IEnumerable<(Vector3d A, Vector3d B, Vector3d C)> triangles)
        {
            double sixTimes = 0;
            Vector3d weighted = Vector3d.Zero;
            foreach (var t in triangles)
            {
                double v = t.A.DotProduct(t.B.CrossProduct(t.C));
                sixTimes += v;
                weighted += (t.A + t.B + t.C) * v;
            }

            if (Math.Abs(sixTimes) < 1e-12)
            {
                return null;
            }

            // each tetrahedron's centre is (a + b + c + origin) / 4
            return weighted / (4 * sixTimes);
        }

        /// <summary>
        /// The edges used other than exactly once each way: each is a hole's rim (used once), an inside-out
        /// triangle (used the same way twice) or a fin where more than two triangles meet. None means the
        /// surface is closed and consistently wound.
        /// </summary>
        public static IReadOnlyList<(int From, int To)> BadEdges(IEnumerable<(int A, int B, int C)> faces)
        {
            var uses = DirectedEdges(faces);
            var bad = new List<(int, int)>();
            foreach (var pair in uses)
            {
                int along = pair.Value;
                uses.TryGetValue((pair.Key.To, pair.Key.From), out int back);
                if (along != 1 || back != 1)
                {
                    bad.Add(pair.Key);
                }
            }

            return bad;
        }

        /// <summary>True when every edge is shared by exactly two triangles running opposite ways along it.</summary>
        public static bool IsClosed(IEnumerable<(int A, int B, int C)> faces)
        {
            return BadEdges(faces).Count == 0;
        }

        /// <summary>
        /// Corners − edges + faces, counting only the corners the faces use. For a closed surface it is
        /// 2 − 2 × <see cref="Genus"/>: 2 for anything shaped like a ball, 0 for a ring.
        /// </summary>
        public static int EulerCharacteristic(IEnumerable<(int A, int B, int C)> faces)
        {
            var corners = new HashSet<int>();
            var edges = new HashSet<(int, int)>();
            int count = 0;
            foreach (var f in faces)
            {
                count++;
                corners.Add(f.A);
                corners.Add(f.B);
                corners.Add(f.C);
                edges.Add(Undirected(f.A, f.B));
                edges.Add(Undirected(f.B, f.C));
                edges.Add(Undirected(f.C, f.A));
            }

            return corners.Count - edges.Count + count;
        }

        /// <summary>
        /// The number of holes right through a closed surface (0 for a ball, 1 for a ring, 2 for a figure
        /// eight), from Euler's formula. Assumes one connected, closed piece.
        /// </summary>
        public static int Genus(IEnumerable<(int A, int B, int C)> faces)
        {
            return (2 - EulerCharacteristic(faces)) / 2;
        }

        private static (int, int) Undirected(int a, int b)
        {
            return a < b ? (a, b) : (b, a);
        }

        private static Dictionary<(int From, int To), int> DirectedEdges(IEnumerable<(int A, int B, int C)> faces)
        {
            var uses = new Dictionary<(int From, int To), int>();
            foreach (var f in faces)
            {
                Count(uses, (f.A, f.B));
                Count(uses, (f.B, f.C));
                Count(uses, (f.C, f.A));
            }

            return uses;
        }

        private static void Count(Dictionary<(int From, int To), int> uses, (int, int) edge)
        {
            uses.TryGetValue(edge, out int n);
            uses[edge] = n + 1;
        }
    }
}
