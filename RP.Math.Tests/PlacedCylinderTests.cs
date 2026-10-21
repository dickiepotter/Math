namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the positioned <see cref="PlacedCylinder"/> type.</summary>
    [TestClass]
    public class PlacedCylinderTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static PlacedCylinder AlongZ()
        {
            return PlacedCylinder.AlongZ(new Cylinder(2, 4), new Vector(0, 0, 0)); // radius 2, half-height 2
        }

        [TestMethod, TestCategory("Placement")]
        public void AlongZ_ShouldExposeAxisAndCaps_Test()
        {
            var c = AlongZ();
            Assert.IsTrue(c.Axis.Equals(new Vector(0, 0, 1), Tol));
            Assert.IsTrue(c.BaseCenter.Equals(new Vector(0, 0, -2), Tol));
            Assert.IsTrue(c.TopCenter.Equals(new Vector(0, 0, 2), Tol));
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_ShouldRespectRadiusAndHeight_Test()
        {
            var c = AlongZ();
            Assert.IsTrue(c.Contains(new Vector(1, 0, 0), Tol));
            Assert.IsTrue(c.Contains(new Vector(0, 0, 2), Tol));   // on a cap
            Assert.IsFalse(c.Contains(new Vector(0, 0, 2.5), Tol)); // beyond a cap
            Assert.IsFalse(c.Contains(new Vector(2.5, 0, 0), Tol)); // beyond the radius
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_ShouldClampRadiusAndHeight_Test()
        {
            Assert.IsTrue(AlongZ().ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(2, 0, 0), Tol));
            Assert.IsTrue(AlongZ().ClosestPoint(new Vector(0, 0, 5)).Equals(new Vector(0, 0, 2), Tol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_HitsCurvedSide_Test()
        {
            var ray = new Ray(new Vector(5, 0, 0), new Vector(-1, 0, 0));
            Assert.IsTrue(AlongZ().TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(2, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_HitsCap_Test()
        {
            var ray = new Ray(new Vector(0, 0, 5), new Vector(0, 0, -1));
            Assert.IsTrue(AlongZ().TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(0, 0, 2), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectLine_AcrossSide_ShouldGiveBoth_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(AlongZ().TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(-2, 0, 0), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(2, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_FromInside_ShouldHitExit_Test()
        {
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(AlongZ().TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(2, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Factory")]
        public void FromEndPoints_ShouldOrientAlongAxis_Test()
        {
            var c = PlacedCylinder.FromEndPoints(new Vector(0, 0, 0), new Vector(4, 0, 0), 2);
            Assert.IsTrue(c.Axis.Equals(new Vector(1, 0, 0), LooseTol));
            Assert.IsTrue(c.BaseCenter.Equals(new Vector(0, 0, 0), LooseTol));
            Assert.IsTrue(c.TopCenter.Equals(new Vector(4, 0, 0), LooseTol));
            Assert.AreEqual(4, c.Height, LooseTol);
        }
    }
}
