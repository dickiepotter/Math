namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Torus"/> and the positioned <see cref="PlacedTorus"/>.</summary>
    [TestClass]
    public class TorusTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var t = new Torus(3, 1);
            Assert.AreEqual(4, t.OuterRadius);
            Assert.AreEqual(2, t.InnerRadius);
            Assert.AreEqual(2 * Math.PI * Math.PI * 3 * 1 * 1, t.Volume, Tol);   // 2π²Rr² (r² written out)
            Assert.AreEqual(4 * Math.PI * Math.PI * 3 * 1, t.SurfaceArea, Tol);  // 4π²Rr
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_TubeEdgesAndHole_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            Assert.IsTrue(t.Contains(new Vector(3, 0, 0), LooseTol));   // on the central circle
            Assert.IsTrue(t.Contains(new Vector(4, 0, 0), LooseTol));   // outer edge
            Assert.IsTrue(t.Contains(new Vector(2, 0, 0), LooseTol));   // inner edge
            Assert.IsTrue(t.Contains(new Vector(3, 0, 1), LooseTol));   // top of the tube
            Assert.IsFalse(t.Contains(new Vector(0, 0, 0), LooseTol));  // the central hole
            Assert.IsFalse(t.Contains(new Vector(4.1, 0, 0), Tol));
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            Assert.IsTrue(t.ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(4, 0, 0), LooseTol));
            Assert.IsTrue(t.ClosestPoint(new Vector(0, 0, 0)).Equals(new Vector(2, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectRay_HitsNearestTubeWall_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            Assert.IsTrue(t.TryIntersect(new Ray(new Vector(10, 0, 0), new Vector(-1, 0, 0)), out Vector p));
            Assert.IsTrue(p.Equals(new Vector(4, 0, 0), LooseTol)); // nearest of the four crossings
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectLine_FourCrossings_GivesOuterMost_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            Assert.IsTrue(t.TryIntersect(new Line(new Vector(0, 0, 0), new Vector(1, 0, 0)), out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(-4, 0, 0), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(4, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectRay_DownTheHole_ShouldMiss_Test()
        {
            var t = PlacedTorus.InXYPlane(new Torus(3, 1), new Vector(0, 0, 0));
            Assert.IsFalse(t.TryIntersect(new Ray(new Vector(0, 0, 10), new Vector(0, 0, -1)), out _));
        }
    }
}
