namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the positioned <see cref="PlacedCuboid"/> type.</summary>
    [TestClass]
    public class PlacedCuboidTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static PlacedCuboid Unit()
        {
            return PlacedCuboid.AxisAligned(new Cuboid(2, 2, 2), new Vector(0, 0, 0)); // half-extents of 1
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldDelegateToShape_Test()
        {
            var c = Unit();
            Assert.AreEqual(8, c.Volume);
            Assert.AreEqual(24, c.SurfaceArea);
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_ShouldRespectExtents_Test()
        {
            var c = Unit();
            Assert.IsTrue(c.Contains(new Vector(0.5, 0.5, 0.5), Tol));
            Assert.IsFalse(c.Contains(new Vector(1.5, 0, 0), Tol));
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_ShouldClampPerAxis_Test()
        {
            Assert.IsTrue(Unit().ClosestPoint(new Vector(5, 0, 0)).Equals(new Vector(1, 0, 0), Tol));
            Assert.IsTrue(Unit().ClosestPoint(new Vector(5, 5, 5)).Equals(new Vector(1, 1, 1), Tol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_FromOutside_ShouldHitNearFace_Test()
        {
            var ray = new Ray(new Vector(5, 0, 0), new Vector(-1, 0, 0));
            Assert.IsTrue(Unit().TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(1, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_FromInside_ShouldHitExit_Test()
        {
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(Unit().TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(1, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectLine_ShouldGiveBothFaces_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(Unit().TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(-1, 0, 0), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(1, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_Missing_ShouldBeFalse_Test()
        {
            var ray = new Ray(new Vector(5, 5, 5), new Vector(1, 1, 1));
            Assert.IsFalse(Unit().TryIntersect(ray, out _));
        }

        [TestMethod, TestCategory("Placement")]
        public void Rotated_Containment_ShouldRoundTripThroughPose_Test()
        {
            var pose = Pose.FromAxisAngle(new Vector(3, 0, 0), new Vector(0, 1, 0), new Angle(45, AngleUnits.DEG));
            var c = new PlacedCuboid(new Cuboid(2, 2, 2), pose);

            Assert.IsTrue(c.Contains(pose.Apply(new Vector(0.9, 0.9, 0.9)), LooseTol));
            Assert.IsFalse(c.Contains(pose.Apply(new Vector(1.1, 0, 0)), LooseTol));
            Assert.IsTrue(c.Center.Equals(new Vector(3, 0, 0), Tol));
        }

        [TestMethod, TestCategory("Corners")]
        public void Corners_ShouldReturnEight_Test()
        {
            Assert.AreEqual(8, Unit().Corners().Length);
        }
    }
}
