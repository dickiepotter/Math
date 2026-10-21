namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the consistency surface shared across placed shapes: <c>DistanceTo</c> on every shape, and
    /// the unified <c>TryIntersect(Line, out near, out far)</c> signature (flat shapes report a single
    /// crossing as <c>near == far</c>).
    /// </summary>
    [TestClass]
    public class PlacedShapeConsistencyTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        #region DistanceTo

        [TestMethod, TestCategory("DistanceTo")]
        public void DistanceTo_OnSolid_IsZeroInside_AndGapOutside_Test()
        {
            // Unit cuboid: half-extents of 1 at the origin.
            var c = PlacedCuboid.AxisAligned(new Cuboid(2, 2, 2), new Vector(0, 0, 0));
            Assert.AreEqual(0, c.DistanceTo(new Vector(0, 0, 0)), Tol);   // inside
            Assert.AreEqual(2, c.DistanceTo(new Vector(3, 0, 0)), Tol);   // 2 beyond the +x face
        }

        [TestMethod, TestCategory("DistanceTo")]
        public void DistanceTo_OnFlatShape_IsPerpendicularGap_AndZeroOnRegion_Test()
        {
            var disc = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            Assert.AreEqual(0, disc.DistanceTo(new Vector(1, 1, 0)), Tol);    // on the disc
            Assert.AreEqual(4, disc.DistanceTo(new Vector(0, 0, 4)), Tol);    // straight above the centre
            Assert.AreEqual(5, disc.DistanceTo(new Vector(10, 0, 0)), Tol);   // 5 past the rim, in-plane
        }

        [TestMethod, TestCategory("DistanceTo")]
        public void DistanceTo_OnSphere_IsZeroInside_AndAgreesWithSignedOutside_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            Assert.AreEqual(0, s.DistanceTo(new Vector(1, 0, 0)), Tol);   // inside → clamped to zero
            Assert.AreEqual(3, s.DistanceTo(new Vector(5, 0, 0)), Tol);   // 3 outside the surface
        }

        #endregion

        #region Unified Line intersection (flat shapes report near == far)

        [TestMethod, TestCategory("LineIntersect")]
        public void Triangle_TryIntersectLine_ReportsSingleCrossingAsNearEqualsFar_Test()
        {
            var t = PlacedTriangle.FromVertices(new Vector(0, 0, 0), new Vector(4, 0, 0), new Vector(0, 4, 0));
            var line = new Line(new Vector(1, 1, 5), new Vector(0, 0, -1));

            Assert.IsTrue(t.TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(1, 1, 0), LooseTol));
            Assert.IsTrue(far.Equals(near, LooseTol));
        }

        [TestMethod, TestCategory("LineIntersect")]
        public void Polygon_TryIntersectLine_ReportsSingleCrossingAsNearEqualsFar_Test()
        {
            var square = new PlacedPolygon(
                new Vector(2, 2, 0), new Vector(-2, 2, 0), new Vector(-2, -2, 0), new Vector(2, -2, 0));
            var line = new Line(new Vector(1, 1, 5), new Vector(0, 0, -1));

            Assert.IsTrue(square.TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(1, 1, 0), LooseTol));
            Assert.IsTrue(far.Equals(near, LooseTol));
        }

        [TestMethod, TestCategory("LineIntersect")]
        public void Annulus_TryIntersectLine_HitsRingAndMissesHole_Test()
        {
            var a = PlacedAnnulus.InXYPlane(new Annulus(2, 5), new Vector(0, 0, 0));

            Assert.IsTrue(a.TryIntersect(new Line(new Vector(3, 0, 5), new Vector(0, 0, -1)), out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(3, 0, 0), LooseTol));
            Assert.IsTrue(far.Equals(near, LooseTol));

            // Straight down through the central hole: crosses the plane but misses the filled ring.
            Assert.IsFalse(a.TryIntersect(new Line(new Vector(0, 0, 5), new Vector(0, 0, -1)), out _, out _));
        }

        #endregion
    }
}
