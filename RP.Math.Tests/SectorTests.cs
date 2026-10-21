namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Sector"/> and the positioned <see cref="PlacedSector"/>.</summary>
    [TestClass]
    public class SectorTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static Sector QuarterCircle()
        {
            return new Sector(2, new Angle(90, AngleUnits.DEG));
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var s = QuarterCircle();
            Assert.AreEqual(0.5 * 4 * (Math.PI / 2), s.Area, Tol); // = pi
            Assert.AreEqual(2 * (Math.PI / 2), s.ArcLength, Tol);  // = pi
            Assert.AreEqual(2 * 2 * Math.Sin(Math.PI / 4), s.ChordLength, Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_WithinRadiusAndSweep_Test()
        {
            var s = PlacedSector.InXYPlane(QuarterCircle(), new Vector(0, 0, 0));
            Assert.IsTrue(s.Contains(new Vector(1, 0.5, 0), LooseTol));   // angle ~27deg, r ~1.1
            Assert.IsFalse(s.Contains(new Vector(-1, 0, 0), LooseTol));   // angle 180deg, outside sweep
            Assert.IsFalse(s.Contains(new Vector(1, -0.5, 0), LooseTol)); // negative angle, outside sweep
            Assert.IsFalse(s.Contains(new Vector(3, 0, 0), LooseTol));    // beyond the radius
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_OnArcAndToApex_Test()
        {
            var s = PlacedSector.InXYPlane(QuarterCircle(), new Vector(0, 0, 0));
            Assert.IsTrue(s.ClosestPoint(new Vector(3, 0, 0)).Equals(new Vector(2, 0, 0), LooseTol)); // out along the arc
            Assert.IsTrue(s.ClosestPoint(new Vector(-1, 0, 0)).Equals(new Vector(0, 0, 0), LooseTol)); // nearest is the apex
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_InsideAndOutsideSweep_Test()
        {
            var s = PlacedSector.InXYPlane(QuarterCircle(), new Vector(0, 0, 0));
            Assert.IsTrue(s.TryIntersect(new Ray(new Vector(1, 0.2, 5), new Vector(0, 0, -1)), out Vector p));
            Assert.IsTrue(p.Equals(new Vector(1, 0.2, 0), LooseTol));
            Assert.IsFalse(s.TryIntersect(new Ray(new Vector(-1, 0, 5), new Vector(0, 0, -1)), out _));
        }
    }
}
