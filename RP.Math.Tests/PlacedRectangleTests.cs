namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the positioned <see cref="PlacedRectangle"/> type.</summary>
    [TestClass]
    public class PlacedRectangleTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static PlacedRectangle Flat()
        {
            return PlacedRectangle.InXYPlane(new Rectangle(4, 6), new Vector(0, 0, 0));
        }

        [TestMethod, TestCategory("Placement")]
        public void InXYPlane_ShouldExposeAxesAndNormal_Test()
        {
            var r = Flat();
            Assert.AreEqual(new Vector(0, 0, 0), r.Center);
            Assert.IsTrue(r.AxisU.Equals(new Vector(1, 0, 0), Tol));
            Assert.IsTrue(r.AxisV.Equals(new Vector(0, 1, 0), Tol));
            Assert.IsTrue(r.Normal.Equals(new Vector(0, 0, 1), Tol));
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldDelegateToShape_Test()
        {
            var r = Flat();
            Assert.AreEqual(24, r.Area);
            Assert.AreEqual(20, r.Perimeter);
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_ShouldRespectExtentsAndPlane_Test()
        {
            var r = Flat();
            Assert.IsTrue(r.Contains(new Vector(1, 2, 0), Tol));
            Assert.IsFalse(r.Contains(new Vector(3, 0, 0), Tol)); // half-width is 2
            Assert.IsFalse(r.Contains(new Vector(0, 0, 0.5), Tol)); // off the plane
            Assert.IsTrue(r.Contains(new Vector(0, 0, 0.5), 1));    // within tolerance of the plane
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_ShouldClampToExtents_Test()
        {
            var r = Flat();
            Assert.IsTrue(r.ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(2, 0, 0), Tol));
            Assert.IsTrue(r.ClosestPoint(new Vector(0, 10, 7)).Equals(new Vector(0, 3, 0), Tol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectLine_ThroughFace_ShouldHit_Test()
        {
            var r = Flat();
            var line = new Line(new Vector(1, 1, 5), new Vector(0, 0, -1));
            Assert.IsTrue(r.TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(1, 1, 0), LooseTol));
            Assert.IsTrue(far.Equals(near, LooseTol)); // a flat rectangle has a single crossing

        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectLine_OutsideExtent_ShouldMiss_Test()
        {
            var r = Flat();
            var line = new Line(new Vector(3, 0, 5), new Vector(0, 0, -1)); // hits plane at x = 3 > half-width 2
            Assert.IsFalse(r.TryIntersect(line, out _, out _));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_PointingAway_ShouldMiss_Test()
        {
            var r = Flat();
            var ray = new Ray(new Vector(1, 1, 5), new Vector(0, 0, 1)); // away from the plane
            Assert.IsFalse(r.TryIntersect(ray, out _));
        }

        [TestMethod, TestCategory("Placement")]
        public void Rotated_Containment_ShouldRoundTripThroughPose_Test()
        {
            var pose = Pose.FromAxisAngle(new Vector(1, 2, 3), new Vector(1, 0, 0), new Angle(90, AngleUnits.DEG));
            var r = new PlacedRectangle(new Rectangle(4, 6), pose);

            // A point known to be inside in local coordinates must be contained once placed by the pose.
            Assert.IsTrue(r.Contains(pose.Apply(new Vector(1, 2, 0)), LooseTol));
            Assert.IsFalse(r.Contains(pose.Apply(new Vector(3, 0, 0)), LooseTol)); // beyond half-width
            Assert.IsTrue(r.Center.Equals(new Vector(1, 2, 3), Tol));
        }

        [TestMethod, TestCategory("Modification")]
        public void Translate_And_Scale_Test()
        {
            var r = Flat().Translate(new Vector(0, 0, 5));
            Assert.IsTrue(r.Center.Equals(new Vector(0, 0, 5), Tol));

            var bigger = Flat().Scale(2);
            Assert.AreEqual(24 * 4, bigger.Area); // both sides doubled
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareShapeAndPose_Test()
        {
            Assert.IsTrue((Flat() == Flat()));
            Assert.IsTrue((Flat() != Flat().Translate(new Vector(1, 0, 0))));
        }
    }
}
