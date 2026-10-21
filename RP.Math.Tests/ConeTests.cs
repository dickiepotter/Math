namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Cone"/> and the positioned <see cref="PlacedCone"/>.</summary>
    [TestClass]
    public class ConeTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var c = new Cone(3, 4);
            Assert.AreEqual(5, c.SlantHeight, Tol);
            Assert.AreEqual(Math.PI * 9, c.BaseArea, Tol);
            Assert.AreEqual(Math.PI * 3 * 5, c.LateralArea, Tol);
            Assert.AreEqual((1.0 / 3.0) * Math.PI * 9 * 4, c.Volume, Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_TaperedProfile_Test()
        {
            var c = PlacedCone.Upright(new Cone(2, 4), new Vector(0, 0, 0));
            Assert.IsTrue(c.Contains(new Vector(0, 0, 0), LooseTol));   // base centre
            Assert.IsTrue(c.Contains(new Vector(0, 0, 4), LooseTol));   // apex
            Assert.IsTrue(c.Contains(new Vector(1, 0, 2), LooseTol));   // on the slant (allowed radius is 1 at z=2)
            Assert.IsFalse(c.Contains(new Vector(1.5, 0, 2), LooseTol));
            Assert.IsFalse(c.Contains(new Vector(0, 0, 5), LooseTol));  // above the apex
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_Test()
        {
            var c = PlacedCone.Upright(new Cone(2, 4), new Vector(0, 0, 0));
            Assert.IsTrue(c.ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(2, 0, 0), LooseTol));  // base rim
            Assert.IsTrue(c.ClosestPoint(new Vector(0, 0, 10)).Equals(new Vector(0, 0, 4), LooseTol)); // apex
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_SideAndAxis_Test()
        {
            var c = PlacedCone.Upright(new Cone(2, 4), new Vector(0, 0, 0));
            Assert.IsTrue(c.TryIntersect(new Ray(new Vector(5, 0, 2), new Vector(-1, 0, 0)), out Vector p));
            Assert.IsTrue(p.Equals(new Vector(1, 0, 2), LooseTol)); // allowed radius is 1 at z=2

            Assert.IsTrue(c.TryIntersect(new Ray(new Vector(0, 0, 10), new Vector(0, 0, -1)), out Vector apex));
            Assert.IsTrue(apex.Equals(new Vector(0, 0, 4), LooseTol)); // hits the apex first
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectLine_AcrossSide_Test()
        {
            var c = PlacedCone.Upright(new Cone(2, 4), new Vector(0, 0, 0));
            Assert.IsTrue(c.TryIntersect(new Line(new Vector(0, 0, 2), new Vector(1, 0, 0)), out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(-1, 0, 2), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(1, 0, 2), LooseTol));
        }
    }
}
