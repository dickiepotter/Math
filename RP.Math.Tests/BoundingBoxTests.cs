namespace RP.Math.Tests
{
    using FluentAssertions;

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
            box.Min.Equals(new Vector(0, 1, 2), Tolerance).Should().BeTrue();
            box.Max.Equals(new Vector(4, 5, 6), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Factories")]
        public void FromCenterExtents_BuildsTheExpectedCorners_Test()
        {
            BoundingBox box = BoundingBox.FromCenterExtents(new Vector(1, 1, 1), new Vector(2, 3, 4));
            box.Min.Equals(new Vector(-1, -2, -3), Tolerance).Should().BeTrue();
            box.Max.Equals(new Vector(3, 4, 5), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Factories")]
        public void FromPoints_IsTheCornerwiseMinAndMax_Test()
        {
            BoundingBox box = BoundingBox.FromPoints(new Vector(1, 5, -2), new Vector(-3, 2, 4), new Vector(0, 0, 0));
            box.Min.Equals(new Vector(-3, 0, -2), Tolerance).Should().BeTrue();
            box.Max.Equals(new Vector(1, 5, 4), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Measurement")]
        public void VolumeAndSurfaceArea_AreCorrect_Test()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(2, 3, 4));
            box.Volume.Should().BeApproximately(24, Tolerance);
            box.SurfaceArea.Should().BeApproximately(2 * ((2 * 3) + (3 * 4) + (4 * 2)), Tolerance);
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_DistinguishesInsideFromOutside_Test()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(10, 10, 10));
            box.Contains(new Vector(5, 5, 5)).Should().BeTrue();
            box.Contains(new Vector(5, 5, 5), 0).Should().BeTrue();
            box.Contains(new Vector(11, 5, 5)).Should().BeFalse();
            box.Contains(new Vector(10, 10, 10)).Should().BeTrue(); // on the surface counts
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_OtherBox_Test()
        {
            var outer = new BoundingBox(new Vector(0, 0, 0), new Vector(10, 10, 10));
            outer.Contains(new BoundingBox(new Vector(2, 2, 2), new Vector(8, 8, 8))).Should().BeTrue();
            outer.Contains(new BoundingBox(new Vector(2, 2, 2), new Vector(12, 8, 8))).Should().BeFalse();
        }

        [TestMethod, TestCategory("Intersection")]
        public void Intersects_OverlappingAndDisjoint_Test()
        {
            var a = new BoundingBox(new Vector(0, 0, 0), new Vector(5, 5, 5));
            a.Intersects(new BoundingBox(new Vector(4, 4, 4), new Vector(9, 9, 9))).Should().BeTrue();
            a.Intersects(new BoundingBox(new Vector(6, 6, 6), new Vector(9, 9, 9))).Should().BeFalse();
            a.Intersects(new BoundingBox(new Vector(5, 0, 0), new Vector(9, 5, 5))).Should().BeTrue(); // touching faces
        }

        [TestMethod, TestCategory("Queries")]
        public void ClosestPoint_ClampsToTheBox_Test()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(4, 4, 4));
            box.ClosestPoint(new Vector(10, 2, -3)).Equals(new Vector(4, 2, 0), Tolerance).Should().BeTrue();
            box.ClosestPoint(new Vector(1, 2, 3)).Equals(new Vector(1, 2, 3), Tolerance).Should().BeTrue(); // inside
            box.DistanceTo(new Vector(1, 2, 3)).Should().BeApproximately(0, Tolerance);
        }

        [TestMethod, TestCategory("Combination")]
        public void Merge_GrowsToIncludePointAndBox_Test()
        {
            var box = new BoundingBox(new Vector(0, 0, 0), new Vector(1, 1, 1));
            box.Merge(new Vector(3, -2, 0)).Contains(new Vector(3, -2, 0)).Should().BeTrue();

            BoundingBox merged = box.Merge(new BoundingBox(new Vector(5, 5, 5), new Vector(6, 6, 6)));
            merged.Contains(box).Should().BeTrue();
            merged.Max.Equals(new Vector(6, 6, 6), Tolerance).Should().BeTrue();
        }

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersect_Ray_HitsAndMisses_Test()
        {
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));

            var hitRay = new Ray(new Vector(-5, 0, 0), new Vector(1, 0, 0));
            box.TryIntersect(hitRay, out Vector point).Should().BeTrue();
            point.Equals(new Vector(-1, 0, 0), 1e-6).Should().BeTrue();

            var missRay = new Ray(new Vector(-5, 5, 0), new Vector(1, 0, 0));
            box.TryIntersect(missRay, out _).Should().BeFalse();

            var behindRay = new Ray(new Vector(5, 0, 0), new Vector(1, 0, 0));
            box.TryIntersect(behindRay, out _).Should().BeFalse();
        }

        [TestMethod, TestCategory("Intersection")]
        public void TryIntersect_Line_ReturnsBothCrossings_Test()
        {
            var box = new BoundingBox(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));

            box.TryIntersect(line, out Vector near, out Vector far).Should().BeTrue();
            near.Equals(new Vector(-1, 0, 0), 1e-6).Should().BeTrue();
            far.Equals(new Vector(1, 0, 0), 1e-6).Should().BeTrue();
        }
    }
}
