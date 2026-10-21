namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math;

    /// <summary>
    /// Tests for the completionist additions to the bounding volumes: transform, interpolation, the empty
    /// accumulator, cross-volume containment/intersection and the frustum full-containment tests.
    /// </summary>
    [TestClass]
    public sealed class BoundsParityTests
    {
        private const double Tol = 1e-9;

        #region BoundingBox

        [TestMethod]
        public void Box_Translate_MovesBothCorners()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(2, 2, 2));
            var moved = box.Translate(new Vector(10, 0, -5));
            Assert.IsTrue(moved.Min.Equals(new Vector(10, 0, -5), Tol));
            Assert.IsTrue(moved.Max.Equals(new Vector(12, 2, -3), Tol));
        }

        [TestMethod]
        public void Box_Transform_ByTranslationMatrix_MatchesTranslate()
        {
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            var m = Matrix.TranslationMatrix(new Vector(5, 6, 7));
            var t = box.Transform(m);
            Assert.IsTrue(t.Center.Equals(new Vector(5, 6, 7), 1e-9));
        }

        [TestMethod]
        public void Box_Transform_ByRotation_RefitsLarger()
        {
            // A 45° rotation about Z grows the AABB of a unit cube (corners no longer axis-aligned).
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            var rot = Matrix.RotationMatrixAboutZAxis(new Angle(System.Math.PI / 4));
            var t = box.Transform(rot);
            Assert.IsTrue((t.Size.X) > (2.0 - 1e-9)); // grew from 2 toward 2√2
            Assert.IsTrue(t.Center.Equals(Vector.Origin, 1e-9));
        }

        [TestMethod]
        public void Box_Lerp_BlendsCorners()
        {
            var a = new BoundingBox(Vector.Origin, new Vector(2, 2, 2));
            var b = new BoundingBox(new Vector(10, 10, 10), new Vector(20, 20, 20));
            var mid = BoundingBox.Lerp(a, b, 0.5);
            Assert.IsTrue(mid.Min.Equals(new Vector(5, 5, 5), Tol));
            Assert.IsTrue(mid.Max.Equals(new Vector(11, 11, 11), Tol));
        }

        [TestMethod]
        public void Box_SphereIntersectAndContain()
        {
            var box = new BoundingBox(Vector.Origin, new Vector(10, 10, 10));
            Assert.IsTrue(box.Intersects(new BoundingSphere(new Vector(5, 5, 5), 1)));
            Assert.IsFalse(box.Intersects(new BoundingSphere(new Vector(20, 20, 20), 1)));
            Assert.IsTrue(box.Contains(new BoundingSphere(new Vector(5, 5, 5), 2)));
            Assert.IsFalse(box.Contains(new BoundingSphere(new Vector(5, 5, 5), 6))); // pokes out
        }

        [TestMethod]
        public void Box_Intersection_OverlapAndDisjoint()
        {
            var a = new BoundingBox(Vector.Origin, new Vector(4, 4, 4));
            var b = new BoundingBox(new Vector(2, 2, 2), new Vector(8, 8, 8));
            var overlap = a.Intersection(b);
            Assert.IsFalse(overlap.IsEmpty);
            Assert.IsTrue(overlap.Min.Equals(new Vector(2, 2, 2), Tol));
            Assert.IsTrue(overlap.Max.Equals(new Vector(4, 4, 4), Tol));

            var disjoint = a.Intersection(new BoundingBox(new Vector(100, 100, 100), new Vector(101, 101, 101)));
            Assert.IsTrue(disjoint.IsEmpty);
        }

        [TestMethod]
        public void Box_Empty_IsTheIdentityForMerge()
        {
            Assert.IsTrue(BoundingBox.Empty.IsEmpty);

            // Accumulating from Empty yields exactly the merged geometry.
            var acc = BoundingBox.Empty
                .Merge(new Vector(1, 2, 3))
                .Merge(new Vector(-1, 5, 0));
            Assert.IsTrue(acc.Min.Equals(new Vector(-1, 2, 0), Tol));
            Assert.IsTrue(acc.Max.Equals(new Vector(1, 5, 3), Tol));
        }

        #endregion

        #region BoundingSphere

        [TestMethod]
        public void Sphere_FromBox_ReachesTheCorners()
        {
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            var s = BoundingSphere.FromBox(box);
            Assert.IsTrue(s.Center.Equals(Vector.Origin, Tol));
            Assert.AreEqual(System.Math.Sqrt(3), s.Radius, 1e-9); // half-diagonal
            // The corners sit exactly on the sphere (a knife-edge that double rounding can push either side),
            // so containment is asserted with a rounding margin.
            Assert.IsTrue(s.Expand(1e-9).Contains(box));
        }

        [TestMethod]
        public void Sphere_Transform_Pose_KeepsRadius_Matrix_ScalesRadius()
        {
            var s = new BoundingSphere(new Vector(1, 0, 0), 2);

            var posed = s.Transform(Pose.At(new Vector(10, 0, 0)));
            Assert.IsTrue(posed.Center.Equals(new Vector(11, 0, 0), 1e-9));
            Assert.AreEqual(2, posed.Radius, Tol); // rigid: radius unchanged

            var scaled = s.Transform(Matrix.ScalingMatrix(3, 3, 3));
            Assert.AreEqual(6, scaled.Radius, 1e-9); // uniform scale ×3
        }

        [TestMethod]
        public void Sphere_Transform_NonUniformScale_TakesLargestAxis()
        {
            var s = new BoundingSphere(Vector.Origin, 1);
            var scaled = s.Transform(Matrix.ScalingMatrix(2, 5, 3));
            Assert.AreEqual(5, scaled.Radius, 1e-9); // enclosing: largest factor
        }

        [TestMethod]
        public void Sphere_Lerp_BlendsCentreAndRadius()
        {
            var a = new BoundingSphere(Vector.Origin, 1);
            var b = new BoundingSphere(new Vector(10, 0, 0), 3);
            var mid = BoundingSphere.Lerp(a, b, 0.5);
            Assert.IsTrue(mid.Center.Equals(new Vector(5, 0, 0), Tol));
            Assert.AreEqual(2, mid.Radius, Tol);
        }

        #endregion

        #region Frustum

        [TestMethod]
        public void Frustum_ContainsVsIntersects()
        {
            // A symmetric perspective frustum looking down -Z.
            var proj = Matrix.PerspectiveFieldOfView(new Angle(System.Math.PI / 2), 1.0, 1.0, 100.0);
            var view = Matrix.LookAt(new Vector(0, 0, 0), new Vector(0, 0, -1), new Vector(0, 1, 0));
            var frustum = Frustum.FromViewProjection(proj * view);

            var inside = new BoundingSphere(new Vector(0, 0, -10), 1);
            Assert.IsTrue(frustum.Contains(inside));
            Assert.IsTrue(frustum.Intersects(inside));

            // A sphere straddling the near plane intersects but is not fully contained.
            var straddling = new BoundingSphere(new Vector(0, 0, -1), 2);
            Assert.IsTrue(frustum.Intersects(straddling));
            Assert.IsFalse(frustum.Contains(straddling));

            // Well behind the camera: neither.
            var behind = new BoundingSphere(new Vector(0, 0, 50), 1);
            Assert.IsFalse(frustum.Intersects(behind));

            Assert.AreEqual(6, frustum.AllPlanes.Count);
        }

        #endregion
    }
}
