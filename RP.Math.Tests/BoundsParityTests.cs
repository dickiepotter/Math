namespace RP.Math.Tests
{
    using FluentAssertions;

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
            moved.Min.Equals(new Vector(10, 0, -5), Tol).Should().BeTrue();
            moved.Max.Equals(new Vector(12, 2, -3), Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Box_Transform_ByTranslationMatrix_MatchesTranslate()
        {
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            var m = Matrix.TranslationMatrix(new Vector(5, 6, 7));
            var t = box.Transform(m);
            t.Center.Equals(new Vector(5, 6, 7), 1e-9).Should().BeTrue();
        }

        [TestMethod]
        public void Box_Transform_ByRotation_RefitsLarger()
        {
            // A 45° rotation about Z grows the AABB of a unit cube (corners no longer axis-aligned).
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            var rot = Matrix.RotationMatrixAboutZAxis(new Angle(System.Math.PI / 4));
            var t = box.Transform(rot);
            t.Size.X.Should().BeGreaterThan(2.0 - 1e-9); // grew from 2 toward 2√2
            t.Center.Equals(Vector.Origin, 1e-9).Should().BeTrue();
        }

        [TestMethod]
        public void Box_Lerp_BlendsCorners()
        {
            var a = new BoundingBox(Vector.Origin, new Vector(2, 2, 2));
            var b = new BoundingBox(new Vector(10, 10, 10), new Vector(20, 20, 20));
            var mid = BoundingBox.Lerp(a, b, 0.5);
            mid.Min.Equals(new Vector(5, 5, 5), Tol).Should().BeTrue();
            mid.Max.Equals(new Vector(11, 11, 11), Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Box_SphereIntersectAndContain()
        {
            var box = new BoundingBox(Vector.Origin, new Vector(10, 10, 10));
            box.Intersects(new BoundingSphere(new Vector(5, 5, 5), 1)).Should().BeTrue();
            box.Intersects(new BoundingSphere(new Vector(20, 20, 20), 1)).Should().BeFalse();
            box.Contains(new BoundingSphere(new Vector(5, 5, 5), 2)).Should().BeTrue();
            box.Contains(new BoundingSphere(new Vector(5, 5, 5), 6)).Should().BeFalse(); // pokes out
        }

        [TestMethod]
        public void Box_Intersection_OverlapAndDisjoint()
        {
            var a = new BoundingBox(Vector.Origin, new Vector(4, 4, 4));
            var b = new BoundingBox(new Vector(2, 2, 2), new Vector(8, 8, 8));
            var overlap = a.Intersection(b);
            overlap.IsEmpty.Should().BeFalse();
            overlap.Min.Equals(new Vector(2, 2, 2), Tol).Should().BeTrue();
            overlap.Max.Equals(new Vector(4, 4, 4), Tol).Should().BeTrue();

            var disjoint = a.Intersection(new BoundingBox(new Vector(100, 100, 100), new Vector(101, 101, 101)));
            disjoint.IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void Box_Empty_IsTheIdentityForMerge()
        {
            BoundingBox.Empty.IsEmpty.Should().BeTrue();

            // Accumulating from Empty yields exactly the merged geometry.
            var acc = BoundingBox.Empty
                .Merge(new Vector(1, 2, 3))
                .Merge(new Vector(-1, 5, 0));
            acc.Min.Equals(new Vector(-1, 2, 0), Tol).Should().BeTrue();
            acc.Max.Equals(new Vector(1, 5, 3), Tol).Should().BeTrue();
        }

        #endregion

        #region BoundingSphere

        [TestMethod]
        public void Sphere_FromBox_ReachesTheCorners()
        {
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            var s = BoundingSphere.FromBox(box);
            s.Center.Equals(Vector.Origin, Tol).Should().BeTrue();
            s.Radius.Should().BeApproximately(System.Math.Sqrt(3), 1e-9); // half-diagonal
            // The corners sit exactly on the sphere (a knife-edge that double rounding can push either side),
            // so containment is asserted with a rounding margin.
            s.Expand(1e-9).Contains(box).Should().BeTrue();
        }

        [TestMethod]
        public void Sphere_Transform_Pose_KeepsRadius_Matrix_ScalesRadius()
        {
            var s = new BoundingSphere(new Vector(1, 0, 0), 2);

            var posed = s.Transform(Pose.At(new Vector(10, 0, 0)));
            posed.Center.Equals(new Vector(11, 0, 0), 1e-9).Should().BeTrue();
            posed.Radius.Should().BeApproximately(2, Tol); // rigid: radius unchanged

            var scaled = s.Transform(Matrix.ScalingMatrix(3, 3, 3));
            scaled.Radius.Should().BeApproximately(6, 1e-9); // uniform scale ×3
        }

        [TestMethod]
        public void Sphere_Transform_NonUniformScale_TakesLargestAxis()
        {
            var s = new BoundingSphere(Vector.Origin, 1);
            var scaled = s.Transform(Matrix.ScalingMatrix(2, 5, 3));
            scaled.Radius.Should().BeApproximately(5, 1e-9); // enclosing: largest factor
        }

        [TestMethod]
        public void Sphere_Lerp_BlendsCentreAndRadius()
        {
            var a = new BoundingSphere(Vector.Origin, 1);
            var b = new BoundingSphere(new Vector(10, 0, 0), 3);
            var mid = BoundingSphere.Lerp(a, b, 0.5);
            mid.Center.Equals(new Vector(5, 0, 0), Tol).Should().BeTrue();
            mid.Radius.Should().BeApproximately(2, Tol);
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
            frustum.Contains(inside).Should().BeTrue();
            frustum.Intersects(inside).Should().BeTrue();

            // A sphere straddling the near plane intersects but is not fully contained.
            var straddling = new BoundingSphere(new Vector(0, 0, -1), 2);
            frustum.Intersects(straddling).Should().BeTrue();
            frustum.Contains(straddling).Should().BeFalse();

            // Well behind the camera: neither.
            var behind = new BoundingSphere(new Vector(0, 0, 50), 1);
            frustum.Intersects(behind).Should().BeFalse();

            frustum.AllPlanes.Count.Should().Be(6);
        }

        #endregion
    }
}
