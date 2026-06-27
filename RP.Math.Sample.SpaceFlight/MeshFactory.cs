namespace RP.Math.Sample.SpaceFlight
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    using RVector = RP.Math.Vector3d;

    /// <summary>
    /// Builds the scene geometry procedurally (no content pipeline). Each builder emits a triangle list
    /// in the <i>same local frame the matching RP.Math shape uses</i> (every solid runs along local +Z),
    /// so the rendered mesh and the collision body line up exactly when both are placed by the same
    /// <see cref="Pose"/>.
    /// </summary>
    /// <remarks>
    /// Flat shading is baked into the vertex colours using RP.Math vector maths: a face normal is the
    /// <see cref="RVector.CrossProduct(RVector)"/> of two edges, and its brightness is the
    /// <see cref="RVector.DotProduct(RVector)"/> with a fixed light direction. The library does the 3D
    /// maths; MonoGame just draws the coloured triangles.
    /// </remarks>
    internal static class MeshFactory
    {
        // A fixed "sun" direction, normalised with RP.Math.
        private static readonly RVector Light = new RVector(0.35, 1.0, 0.55).Normalize();

        private static Color Shade(Color baseColor, RVector faceNormal)
        {
            double d = faceNormal.NormalizeOrDefault().DotProduct(Light);
            double s = 0.40 + 0.60 * Math.Max(0.0, d); // soft ambient + directional
            return new Color((int)(baseColor.R * s), (int)(baseColor.G * s), (int)(baseColor.B * s));
        }

        private static void Tri(List<VertexPositionColor> v, RVector a, RVector b, RVector c, Color col)
        {
            // Face normal via the cross product of two edges — an RP.Math operation.
            RVector n = (b - a).CrossProduct(c - a);
            Color sc = Shade(col, n);
            v.Add(new VertexPositionColor(Interop.ToXna(a), sc));
            v.Add(new VertexPositionColor(Interop.ToXna(b), sc));
            v.Add(new VertexPositionColor(Interop.ToXna(c), sc));
        }

        private static void Quad(List<VertexPositionColor> v, RVector a, RVector b, RVector c, RVector d, Color col)
        {
            Tri(v, a, b, c, col);
            Tri(v, a, c, d, col);
        }

        // ----- Solids matching the RP.Math placed shapes (all centred, axis = local +Z) ---------------

        public static VertexPositionColor[] Cuboid(double w, double h, double d, Color col)
        {
            double x = w / 2, y = h / 2, z = d / 2;
            var v = new List<VertexPositionColor>();
            RVector p000 = new(-x, -y, -z), p100 = new(x, -y, -z), p110 = new(x, y, -z), p010 = new(-x, y, -z);
            RVector p001 = new(-x, -y, z), p101 = new(x, -y, z), p111 = new(x, y, z), p011 = new(-x, y, z);
            Quad(v, p000, p010, p110, p100, col); // -Z
            Quad(v, p101, p111, p011, p001, col); // +Z
            Quad(v, p000, p001, p011, p010, col); // -X
            Quad(v, p100, p110, p111, p101, col); // +X
            Quad(v, p000, p100, p101, p001, col); // -Y
            Quad(v, p010, p011, p111, p110, col); // +Y
            return v.ToArray();
        }

        public static VertexPositionColor[] Ellipsoid(double ax, double ay, double az, Color col, int slices = 20, int stacks = 14)
        {
            var v = new List<VertexPositionColor>();
            RVector P(double i, double j)
            {
                double theta = Math.PI * j / stacks;       // 0..pi  (latitude)
                double phi = 2 * Math.PI * i / slices;      // 0..2pi (longitude)
                return new RVector(
                    ax * Math.Sin(theta) * Math.Cos(phi),
                    ay * Math.Cos(theta),
                    az * Math.Sin(theta) * Math.Sin(phi));
            }
            for (int j = 0; j < stacks; j++)
                for (int i = 0; i < slices; i++)
                    Quad(v, P(i, j), P(i, j + 1), P(i + 1, j + 1), P(i + 1, j), col);
            return v.ToArray();
        }

        public static VertexPositionColor[] Sphere(double r, Color col, int slices = 20, int stacks = 14)
            => Ellipsoid(r, r, r, col, slices, stacks);

        public static VertexPositionColor[] Cylinder(double r, double h, Color col, int slices = 24)
        {
            var v = new List<VertexPositionColor>();
            double z0 = -h / 2, z1 = h / 2;
            for (int i = 0; i < slices; i++)
            {
                double a = 2 * Math.PI * i / slices, b = 2 * Math.PI * (i + 1) / slices;
                RVector ca = new(r * Math.Cos(a), r * Math.Sin(a), 0), cb = new(r * Math.Cos(b), r * Math.Sin(b), 0);
                RVector la0 = ca + new RVector(0, 0, z0), la1 = ca + new RVector(0, 0, z1);
                RVector lb0 = cb + new RVector(0, 0, z0), lb1 = cb + new RVector(0, 0, z1);
                Quad(v, la0, la1, lb1, lb0, col);                                   // curved side
                Tri(v, new RVector(0, 0, z1), la1, lb1, col);                       // +Z cap
                Tri(v, new RVector(0, 0, z0), lb0, la0, col);                       // -Z cap
            }
            return v.ToArray();
        }

        public static VertexPositionColor[] Cone(double baseR, double h, Color col, int slices = 24)
        {
            // Base disc on z = 0, apex at z = h (matches PlacedCone's local frame).
            var v = new List<VertexPositionColor>();
            RVector apex = new(0, 0, h);
            for (int i = 0; i < slices; i++)
            {
                double a = 2 * Math.PI * i / slices, b = 2 * Math.PI * (i + 1) / slices;
                RVector pa = new(baseR * Math.Cos(a), baseR * Math.Sin(a), 0);
                RVector pb = new(baseR * Math.Cos(b), baseR * Math.Sin(b), 0);
                Tri(v, pa, pb, apex, col);             // side
                Tri(v, new RVector(0, 0, 0), pb, pa, col); // base
            }
            return v.ToArray();
        }

        public static VertexPositionColor[] Capsule(double r, double cylH, Color col, int slices = 20, int capStacks = 6)
        {
            var v = new List<VertexPositionColor>();
            double half = cylH / 2;

            // Cylindrical middle.
            for (int i = 0; i < slices; i++)
            {
                double a = 2 * Math.PI * i / slices, b = 2 * Math.PI * (i + 1) / slices;
                RVector ca = new(r * Math.Cos(a), r * Math.Sin(a), 0), cb = new(r * Math.Cos(b), r * Math.Sin(b), 0);
                Quad(v, ca + new RVector(0, 0, -half), ca + new RVector(0, 0, half),
                        cb + new RVector(0, 0, half), cb + new RVector(0, 0, -half), col);
            }

            // Two hemispherical caps, centred at z = ±half.
            RVector Hemi(double i, double j, int sign)
            {
                double theta = (Math.PI / 2) * j / capStacks;     // 0..pi/2
                double phi = 2 * Math.PI * i / slices;
                double rr = r * Math.Sin(theta);
                return new RVector(rr * Math.Cos(phi), rr * Math.Sin(phi), sign * (half + r * Math.Cos(theta)));
            }
            foreach (int sign in new[] { 1, -1 })
                for (int j = 0; j < capStacks; j++)
                    for (int i = 0; i < slices; i++)
                        Quad(v, Hemi(i, j, sign), Hemi(i + 1, j, sign), Hemi(i + 1, j + 1, sign), Hemi(i, j + 1, sign), col);
            return v.ToArray();
        }

        public static VertexPositionColor[] Torus(double major, double minor, Color col, int majSeg = 32, int minSeg = 16)
        {
            var v = new List<VertexPositionColor>();
            RVector P(double i, double j)
            {
                double theta = 2 * Math.PI * i / majSeg;   // around the ring (in XY plane)
                double phi = 2 * Math.PI * j / minSeg;      // around the tube
                double rr = major + minor * Math.Cos(phi);
                return new RVector(rr * Math.Cos(theta), rr * Math.Sin(theta), minor * Math.Sin(phi));
            }
            for (int i = 0; i < majSeg; i++)
                for (int j = 0; j < minSeg; j++)
                    Quad(v, P(i, j), P(i + 1, j), P(i + 1, j + 1), P(i, j + 1), col);
            return v.ToArray();
        }

        /// <summary>A little dart that points along its local −Z (the convention's forward), with two fins.</summary>
        public static VertexPositionColor[] PlayerDart(Color body, Color fin)
        {
            var v = new List<VertexPositionColor>();
            RVector nose = new(0, 0, -1.4);
            RVector tlu = new(-0.45, 0.30, 0.6), tru = new(0.45, 0.30, 0.6);
            RVector tld = new(-0.45, -0.30, 0.6), trd = new(0.45, -0.30, 0.6);
            Tri(v, nose, tlu, tru, body);   // top
            Tri(v, nose, trd, tld, body);   // bottom
            Tri(v, nose, tld, tlu, body);   // left
            Tri(v, nose, tru, trd, body);   // right
            Quad(v, tlu, tld, trd, tru, body); // tail
            // Vertical fin.
            Tri(v, new RVector(0, 0, 0.5), new RVector(0, 0.7, 0.7), new RVector(0, 0, 0.7), fin);
            // Horizontal fins.
            Tri(v, new RVector(0, 0, 0.5), new RVector(0.8, 0, 0.7), new RVector(0, 0, 0.7), fin);
            Tri(v, new RVector(0, 0, 0.5), new RVector(-0.8, 0, 0.7), new RVector(0, 0, 0.7), fin);
            return v.ToArray();
        }

        // ----- Reference overlays (line lists) --------------------------------------------------------

        /// <summary>A grid on the y = 0 plane plus coloured world axes, as a line list.</summary>
        public static VertexPositionColor[] GridAndAxes(double half, double step)
        {
            var v = new List<VertexPositionColor>();
            Color grid = new(40, 60, 80);
            for (double c = -half; c <= half + 1e-6; c += step)
            {
                v.Add(new VertexPositionColor(new Vector3((float)c, 0, (float)-half), grid));
                v.Add(new VertexPositionColor(new Vector3((float)c, 0, (float)half), grid));
                v.Add(new VertexPositionColor(new Vector3((float)-half, 0, (float)c), grid));
                v.Add(new VertexPositionColor(new Vector3((float)half, 0, (float)c), grid));
            }
            void Axis(Vector3 dir, Color col)
            {
                v.Add(new VertexPositionColor(Vector3.Zero, col));
                v.Add(new VertexPositionColor(dir * (float)half, col));
            }
            Axis(Vector3.UnitX, new Color(200, 80, 80));
            Axis(Vector3.UnitY, new Color(80, 200, 80));
            Axis(Vector3.UnitZ, new Color(80, 120, 220));
            return v.ToArray();
        }

        /// <summary>The 12 edges of a min/max box as a line list, for drawing the arena / bounds.</summary>
        public static VertexPositionColor[] BoxWire(RVector min, RVector max, Color col)
        {
            Vector3 lo = Interop.ToXna(min), hi = Interop.ToXna(max);
            var c = new[]
            {
                new Vector3(lo.X, lo.Y, lo.Z), new Vector3(hi.X, lo.Y, lo.Z), new Vector3(hi.X, lo.Y, hi.Z), new Vector3(lo.X, lo.Y, hi.Z),
                new Vector3(lo.X, hi.Y, lo.Z), new Vector3(hi.X, hi.Y, lo.Z), new Vector3(hi.X, hi.Y, hi.Z), new Vector3(lo.X, hi.Y, hi.Z),
            };
            int[,] e = { { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 }, { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 }, { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 } };
            var v = new List<VertexPositionColor>();
            for (int i = 0; i < e.GetLength(0); i++)
            {
                v.Add(new VertexPositionColor(c[e[i, 0]], col));
                v.Add(new VertexPositionColor(c[e[i, 1]], col));
            }
            return v.ToArray();
        }
    }
}
