namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Plane"/> type.
    /// </summary>
    [TestClass]
    public class PlaneTests
    {
        private const double Tol = 1e-9;

        #region Construction and factories

        [TestMethod, TestCategory("Construction")]
        public void Construction_FromCoefficients_ShouldExposeThem_Test()
        {
            var p = new Plane(1, 2, 3, 4);
            Assert.AreEqual(1, p.A);
            Assert.AreEqual(2, p.B);
            Assert.AreEqual(3, p.C);
            Assert.AreEqual(4, p.D);
            Assert.AreEqual(new Vector(1, 2, 3), p.Normal);
        }

        [TestMethod, TestCategory("Construction")]
        public void FromPointNormal_ShouldHaveUnitNormalAndContainThePoint_Test()
        {
            var point = new Vector(0, 0, 5);
            var p = Plane.FromPointNormal(point, new Vector(0, 0, 10));

            Assert.AreEqual(1, p.Normal.Magnitude, Tol);
            Assert.IsTrue(p.Contains(point, Tol));
        }

        [TestMethod, TestCategory("Construction")]
        public void FromThreePoints_ShouldContainAllThree_Test()
        {
            var a = new Vector(0, 0, 0);
            var b = new Vector(1, 0, 0);
            var c = new Vector(0, 1, 0);
            var p = Plane.FromThreePoints(a, b, c);

            Assert.IsTrue(p.Contains(a, Tol));
            Assert.IsTrue(p.Contains(b, Tol));
            Assert.IsTrue(p.Contains(c, Tol));
            // XY plane: normal should be ±Z.
            Assert.IsTrue(p.UnitNormal.Equals(new Vector(0, 0, 1), Tol));
        }

        [TestMethod, TestCategory("Construction")]
        public void FromThreePoints_Collinear_ShouldThrow_Test()
        {
            Action act = () => Plane.FromThreePoints(new Vector(0, 0, 0), new Vector(1, 0, 0), new Vector(2, 0, 0));
            Assert.ThrowsException<ArgumentException>(act);
        }

        #endregion

        #region Distance, side, closest point

        [TestMethod, TestCategory("Distance")]
        public void SignedDistanceTo_ShouldBePositiveOnTheNormalSide_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            Assert.AreEqual(5, p.SignedDistanceTo(new Vector(0, 0, 5)), Tol);
            Assert.AreEqual(-5, p.SignedDistanceTo(new Vector(0, 0, -5)), Tol);
        }

        [TestMethod, TestCategory("Distance")]
        public void DistanceTo_ShouldBeUnsigned_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            Assert.AreEqual(7, p.DistanceTo(new Vector(3, 4, -7)), Tol);
        }

        [TestMethod, TestCategory("Distance")]
        public void DistanceTo_ShouldIgnoreRawNormalScale_Test()
        {
            // Same geometric plane z = 0 but with a non-unit normal.
            var p = new Plane(0, 0, 10, 0);
            Assert.AreEqual(4, p.DistanceTo(new Vector(0, 0, 4)), Tol);
        }

        [TestMethod, TestCategory("Side")]
        public void SideOf_ShouldClassifyPoints_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            Assert.AreEqual(1, p.SideOf(new Vector(0, 0, 1), Tol));
            Assert.AreEqual(-1, p.SideOf(new Vector(0, 0, -1), Tol));
            Assert.AreEqual(0, p.SideOf(new Vector(5, 5, 0), Tol));
        }

        [TestMethod, TestCategory("ClosestPoint")]
        public void ClosestPoint_ShouldDropOntoThePlane_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            var closest = p.ClosestPoint(new Vector(3, 4, 9));
            Assert.IsTrue(closest.Equals(new Vector(3, 4, 0), Tol));
            Assert.IsTrue(p.Contains(closest, Tol));
        }

        [TestMethod, TestCategory("Reflect")]
        public void Reflect_ShouldMirrorThroughThePlane_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            Assert.IsTrue(p.Reflect(new Vector(2, 3, 5)).Equals(new Vector(2, 3, -5), Tol));
        }

        #endregion

        #region Line intersection

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersectLine_ShouldFindCrossingPoint_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            var ok = p.TryIntersectLine(new Vector(1, 2, 5), new Vector(0, 0, -1), out var hit);
            Assert.IsTrue(ok);
            Assert.IsTrue(hit.Equals(new Vector(1, 2, 0), Tol));
        }

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersectLine_ParallelLine_ShouldReturnFalse_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            var ok = p.TryIntersectLine(new Vector(0, 0, 5), new Vector(1, 0, 0), out _);
            Assert.IsFalse(ok);
        }

        [TestMethod, TestCategory("Intersection")]
        public void IntersectLineParameter_ShouldGiveTAlongTheLine_Test()
        {
            var p = Plane.FromPointNormal(new Vector(0, 0, 0), new Vector(0, 0, 1));
            // From z=5 heading -z, the plane is reached at t = 5.
            Assert.AreEqual(5, p.IntersectLineParameter(new Vector(0, 0, 5), new Vector(0, 0, -1)), Tol);
        }

        #endregion

        #region Predicates and equality

        [TestMethod, TestCategory("Predicate")]
        public void IsParallelTo_ShouldDetectParallelPlanes_Test()
        {
            var p = new Plane(0, 0, 1, 0);
            var q = new Plane(0, 0, 1, -5);    // z = 5, parallel
            Assert.IsTrue(p.IsParallelTo(q, Tol));
        }

        [TestMethod, TestCategory("Predicate")]
        public void IsDegenerate_ShouldDetectZeroNormal_Test()
        {
            Assert.IsTrue(new Plane(0, 0, 0, 1).IsDegenerate());
            Assert.IsFalse(new Plane(0, 0, 1, 0).IsDegenerate());
        }

        [TestMethod, TestCategory("Equality")]
        public void GeometricEquality_ShouldIgnoreScaleAndDirection_Test()
        {
            var p = new Plane(0, 0, 1, 0);        // z = 0
            var scaled = new Plane(0, 0, 5, 0);   // same plane, scaled normal
            var flipped = new Plane(0, 0, -1, 0); // same plane, opposite normal

            Assert.IsTrue(p.Equals(scaled, Tol));
            Assert.IsTrue(p.Equals(flipped, Tol));
        }

        [TestMethod, TestCategory("Equality")]
        public void GeometricEquality_ShouldRejectDifferentPlanes_Test()
        {
            var p = new Plane(0, 0, 1, 0);    // z = 0
            var q = new Plane(0, 0, 1, -5);   // z = 5
            Assert.IsFalse(p.Equals(q, Tol));
        }

        [TestMethod, TestCategory("Operator")]
        public void Negation_ShouldFlipCoefficientsButKeepTheGeometricPlane_Test()
        {
            var p = new Plane(0, 0, 1, -5);
            var n = -p;
            Assert.AreEqual(new Plane(0, 0, -1, 5), n);
            Assert.IsTrue(p.Equals(n, Tol));
        }

        #endregion
    }
}
