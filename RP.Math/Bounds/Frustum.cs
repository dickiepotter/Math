namespace RP.Math
{
    /// <summary>
    /// A view frustum: the six-sided pyramid-with-the-tip-cut-off that a perspective camera can actually
    /// see. Represented as its six bounding <see cref="Plane"/>s (left, right, bottom, top, near, far),
    /// each with its normal pointing <i>inward</i>. The headline use is <b>culling</b> — cheaply rejecting
    /// objects that fall outside the frustum so the renderer never touches them.
    /// </summary>
    /// <remarks>
    /// <para><b>Where the planes come from.</b> A world position becomes a clip position via
    /// <c>clip = M · world</c> (M is the view-projection matrix). After the perspective divide a point is
    /// visible when, on every axis, <c>-w ≤ component ≤ w</c>. Re-arranging each of those six inequalities
    /// into <c>(row ± otherRow) · world ≥ 0</c> hands you a plane directly from rows of M — the classic
    /// Gribb–Hartmann extraction. Because RP.Math uses the column-vector convention (<c>v' = M·v</c>) with
    /// <c>m[row, col]</c> storage and OpenGL-style clip depth <c>[-w, w]</c>, the near plane is
    /// <c>row3 + row2</c> (a <c>[0, w]</c> convention would use <c>row2</c> alone).</para>
    /// <para>Build the frustum from a view-projection that does <b>not</b> include any renderer-specific
    /// clip correction (Y-flip / depth remap) — culling happens in world space and is independent of how
    /// the image is finally presented.</para>
    /// </remarks>
    public readonly struct Frustum
    {
        /// <summary>The six bounding planes, normals pointing inward (a point is inside when it is on the
        /// positive side of all six).</summary>
        public Plane Left { get; }
        public Plane Right { get; }
        public Plane Bottom { get; }
        public Plane Top { get; }
        public Plane Near { get; }
        public Plane Far { get; }

        private Frustum(Plane left, Plane right, Plane bottom, Plane top, Plane near, Plane far)
        {
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
            Near = near;
            Far = far;
        }

        /// <summary>
        /// Extracts the six frustum planes from a world→clip <paramref name="viewProjection"/> matrix.
        /// </summary>
        public static Frustum FromViewProjection(Matrix viewProjection)
        {
            Matrix m = viewProjection;

            // Each plane is (rowA ± rowB) of the matrix, then normalised. Inward-facing.
            Plane MakePlane(double a, double b, double c, double d) => new Plane(a, b, c, d).Normalize();

            var left = MakePlane(
                m[3, 0] + m[0, 0], m[3, 1] + m[0, 1], m[3, 2] + m[0, 2], m[3, 3] + m[0, 3]);
            var right = MakePlane(
                m[3, 0] - m[0, 0], m[3, 1] - m[0, 1], m[3, 2] - m[0, 2], m[3, 3] - m[0, 3]);
            var bottom = MakePlane(
                m[3, 0] + m[1, 0], m[3, 1] + m[1, 1], m[3, 2] + m[1, 2], m[3, 3] + m[1, 3]);
            var top = MakePlane(
                m[3, 0] - m[1, 0], m[3, 1] - m[1, 1], m[3, 2] - m[1, 2], m[3, 3] - m[1, 3]);
            var near = MakePlane(
                m[3, 0] + m[2, 0], m[3, 1] + m[2, 1], m[3, 2] + m[2, 2], m[3, 3] + m[2, 3]);
            var far = MakePlane(
                m[3, 0] - m[2, 0], m[3, 1] - m[2, 1], m[3, 2] - m[2, 2], m[3, 3] - m[2, 3]);

            return new Frustum(left, right, bottom, top, near, far);
        }

        private Plane[] Planes => new[] { Left, Right, Bottom, Top, Near, Far };

        /// <summary>True if the point lies inside (or on) all six planes.</summary>
        public bool Contains(Vector point)
        {
            foreach (Plane plane in Planes)
            {
                if (plane.SignedDistanceTo(point) < 0) return false;
            }

            return true;
        }

        /// <summary>
        /// True if any part of the sphere is inside the frustum. Conservative and exact for spheres: a
        /// sphere is fully outside only if its centre is farther than its radius behind some plane.
        /// </summary>
        public bool Intersects(BoundingSphere sphere)
        {
            foreach (Plane plane in Planes)
            {
                if (plane.SignedDistanceTo(sphere.Center) < -sphere.Radius) return false;
            }

            return true;
        }

        /// <summary>
        /// True if the axis-aligned box might be visible. Uses the "positive vertex" test: for each plane,
        /// the box corner farthest along the plane normal is checked; if even that corner is outside, the
        /// whole box is. Conservative (rare false positives at the corners), never a false negative.
        /// </summary>
        public bool Intersects(Box box)
        {
            Vector min = box.Min;
            Vector max = box.Max;

            foreach (Plane plane in Planes)
            {
                Vector n = plane.Normal;
                // The vertex of the box farthest in the direction of the (inward) normal.
                var positive = new Vector(
                    n.X >= 0 ? max.X : min.X,
                    n.Y >= 0 ? max.Y : min.Y,
                    n.Z >= 0 ? max.Z : min.Z);

                if (plane.SignedDistanceTo(positive) < 0) return false;
            }

            return true;
        }
    }
}
