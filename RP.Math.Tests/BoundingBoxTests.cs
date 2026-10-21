namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the axis-aligned <see cref="BoundingBox"/> bounding volume.</summary>
    [TestClass]
    public class BoundingBoxTests
    {
        private const double Tolerance = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_SortsCornersPerAxis_Test()
        {
            var box = new BoundingBox(new Vector(4, 1, 6), new Vector(0, 5, 2));
            Assert.IsTrue(box.Min.Equals(new Vector(0, 1, 2), Tolerance));
            Assert.IsTrue(box.Max.Equals(new Vector(4, 5, 6), Tolerance));
        }

        [TestMethod, TestCategory("Factories")]
        public void FromCenterExtents_BuildsTheExpectedCorners_Test()
        {
            BoundingBox box = BoundingBox.FromCenterExtents(new Vector(1, 1, 1), new Vector(2, 3, 4));
            Assert.IsTrue(box.Min.Equals(new Vector(-1, -2, -3), Tolerance));
            Assert.IsTrue(box.Max.Equals(new Vector(3, 4, 5), Tolerance));
        }

        [TestMethod, TestCategory("Factories")]
        public void FromPoints_IsTheCornerwiseMinAndMax_Test()
        {
            BoundingBox box = BoundingBox.FromPoints(new Vector(1, 5, -2), new Vector(-3, 2, 4), new Vector(0, 0, 0));
            Assert.IsTrue(box.Min.Equals(new Vector(-3, 0, -2), Tolerance));
            Assert.IsTrue(box.Max.Equals(new Vector(1, 5, 4), Tolerance));
        }

        [TestMethod, TestCategory("Measurement")]
        public void VolumeAndSurfaceArea_AreCorrect_Test()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(2, 3, 4));
            Assert.AreEqual(24, box.Volume, Tolerance);
            Assert.AreEqual(2 * ((2 * 3) + (3 * 4) + (4 * 2)), box.SurfaceArea, Tolerance);
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_DistinguishesInsideFromOutside_Test()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(10, 10, 10));
            Assert.IsTrue(box.Contains(new Vector(5, 5, 5)));
            Assert.IsTrue(box.Contains(new Vector(5, 5, 5), 0));
            Assert.IsFalse(box.Contains(new Vector(11, 5, 5)));
            Assert.IsTrue(box.Contains(new Vector(10, 10, 10))); // on the surface counts
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_OtherBox_Test()
        {
            var outer = new BoundingBox(new Vector(0, 0, 0), new Vector(10, 10, 10));
            Assert.IsTrue(outer.Contains(new BoundingBox(new Vector(2, 2, 2), new Vector(8, 8, 8))));
            Assert.IsFalse(outer.Contains(new BoundingBox(new Vector(2, 2, 2), new Vector(12, 8, 8))));
        }

        [TestMethod, TestCategory("Intersection")]
        public void Intersects_OverlappingAndDisjoint_Test()
        {
            var a = new BoundingBox(new Vector(0, 0, 0), new Vector(5, 5, 5));
            Assert.IsTrue(a.Intersects(new BoundingBox(new Vector(4, 4, 4), new Vector(9, 9, 9))));
            Assert.IsFalse(a.Intersects(new BoundingBox(new Vector(6, 6, 6), new Vector(9, 9, 9))));
            Assert.IsTrue(a.Intersects(new BoundingBox(new Vector(5, 0, 0), new Vector(9, 5, 5)))); // touching faces
        }

        [TestMethod, TestCategory("Queries")]
        public void ClosestPoint_ClampsToTheBox_Test()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(4, 4, 4));
            Assert.IsTrue(box.ClosestPoint(new Vector(10, 2, -3)).Equals(new Vector(4, 2, 0), Tolerance));
            Assert.IsTrue(box.ClosestPoint(new Vector(1, 2, 3)).Equals(new Vector(1, 2, 3), Tolerance)); // inside
            Assert.AreEqual(0, box.DistanceTo(new Vector(1, 2, 3)), Tolerance);
        }

        [TestMethod, TestCategory("Combination")]
        public void Merge_GrowsToIncludePointAndBox_Test()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(1, 1, 1));
            Assert.IsTrue(box.Merge(new Vector(3, -2, 0)).Contains(new Vector(3, -2, 0)));

            BoundingBox merged = box.Merge(new BoundingBox(new Vector(5, 5, 5), new Vector(6, 6, 6)));
            Assert.IsTrue(merged.Contains(box));
            Assert.IsTrue(merged.Max.Equals(new Vector(6, 6, 6), Tolerance));
        }

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersect_Ray_HitsAndMisses_Test()
        {
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));

            var hitRay = new Ray(new Vector(-5, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(box.TryIntersect(hitRay, out Vector point));
            Assert.IsTrue(point.Equals(new Vector(-1, 0, 0), 1e-6));

            var missRay = new Ray(new Vector(-5, 5, 0), new Vector(1, 0, 0));
            Assert.IsFalse(box.TryIntersect(missRay, out _));

            var behindRay = new Ray(new Vector(5, 0, 0), new Vector(1, 0, 0));
            Assert.IsFalse(box.TryIntersect(behindRay, out _));
        }

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersect_Line_ReturnsBothCrossings_Test()
        {
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));

            Assert.IsTrue(box.TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(-1, 0, 0), 1e-6));
            Assert.IsTrue(far.Equals(new Vector(1, 0, 0), 1e-6));
        }
    }
}
