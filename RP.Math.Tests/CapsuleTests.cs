namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Capsule"/> and the positioned <see cref="PlacedCapsule"/>.</summary>
    [TestClass]
    public class CapsuleTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var c = new Capsule(1, 4);
            Assert.AreEqual(6, c.TotalHeight);
            Assert.AreEqual((Math.PI * 1 * 4) + ((4.0 / 3.0) * Math.PI), c.Volume, Tol);
            Assert.AreEqual((2 * Math.PI * 1 * 4) + (4 * Math.PI), c.SurfaceArea, Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_BodyAndCaps_Test()
        {
            var c = PlacedCapsule.AlongZ(new Capsule(1, 4), new Vector(0, 0, 0)); // segment z in [-2, 2]
            Assert.IsTrue(c.Contains(new Vector(1, 0, 0), LooseTol));   // cylinder side
            Assert.IsTrue(c.Contains(new Vector(0, 0, 3), LooseTol));   // tip of the top cap
            Assert.IsFalse(c.Contains(new Vector(0, 0, 3.1), LooseTol));
            Assert.IsFalse(c.Contains(new Vector(1.1, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_Test()
        {
            var c = PlacedCapsule.AlongZ(new Capsule(1, 4), new Vector(0, 0, 0));
            Assert.IsTrue(c.ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(1, 0, 0), LooseTol));
            Assert.IsTrue(c.ClosestPoint(new Vector(0, 0, 5)).Equals(new Vector(0, 0, 3), LooseTol)); // out along the cap
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_SideAndCap_Test()
        {
            var c = PlacedCapsule.AlongZ(new Capsule(1, 4), new Vector(0, 0, 0));
            Assert.IsTrue(c.TryIntersect(new Ray(new Vector(5, 0, 0), new Vector(-1, 0, 0)), out Vector side));
            Assert.IsTrue(side.Equals(new Vector(1, 0, 0), LooseTol));

            Assert.IsTrue(c.TryIntersect(new Ray(new Vector(0, 0, 5), new Vector(0, 0, -1)), out Vector cap));
            Assert.IsTrue(cap.Equals(new Vector(0, 0, 3), LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectLine_AcrossBody_Test()
        {
            var c = PlacedCapsule.AlongZ(new Capsule(1, 4), new Vector(0, 0, 0));
            Assert.IsTrue(c.TryIntersect(new Line(new Vector(0, 0, 0), new Vector(1, 0, 0)), out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(-1, 0, 0), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(1, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void FromSegment_ShouldOrientAlongSegment_Test()
        {
            var c = PlacedCapsule.FromSegment(new Vector(0, 0, 0), new Vector(4, 0, 0), 1);
            Assert.IsTrue(c.Axis.Equals(new Vector(1, 0, 0), LooseTol));
            Assert.IsTrue(c.SegmentStart.Equals(new Vector(0, 0, 0), LooseTol));
            Assert.IsTrue(c.SegmentEnd.Equals(new Vector(4, 0, 0), LooseTol));
        }
    }
}
