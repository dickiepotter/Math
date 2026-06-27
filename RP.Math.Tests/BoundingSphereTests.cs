namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="BoundingSphere"/> bounding volume.</summary>
    [TestClass]
    public class BoundingSphereTests
    {
        private const double Tolerance = 1e-9;
        private const double LooseTolerance = 1e-6;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_WithNegativeRadius_Throws_Test()
        {
            Action act = () => new BoundingSphere(Vector.Zero, -1);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_DistinguishesInsideFromOutside_Test()
        {
            var s = new BoundingSphere(Vector.Zero, 5);
            s.Contains(new Vector(3, 0, 0)).Should().BeTrue();
            s.Contains(new Vector(5, 0, 0)).Should().BeTrue(); // on the surface
            s.Contains(new Vector(6, 0, 0)).Should().BeFalse();
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_OtherSphere_Test()
        {
            var outer = new BoundingSphere(Vector.Zero, 10);
            outer.Contains(new BoundingSphere(new Vector(2, 0, 0), 3)).Should().BeTrue();
            outer.Contains(new BoundingSphere(new Vector(8, 0, 0), 3)).Should().BeFalse();
        }

        [TestMethod, TestCategory("Intersection")]
        public void Intersects_OtherSphere_Test()
        {
            var a = new BoundingSphere(Vector.Zero, 2);
            a.Intersects(new BoundingSphere(new Vector(3, 0, 0), 2)).Should().BeTrue();
            a.Intersects(new BoundingSphere(new Vector(5, 0, 0), 2)).Should().BeFalse();
            a.Intersects(new BoundingSphere(new Vector(4, 0, 0), 2)).Should().BeTrue(); // touching
        }

        [TestMethod, TestCategory("Intersection")]
        public void Intersects_BoundingBox_Test()
        {
            var s = new BoundingSphere(new Vector(0, 0, 0), 2);
            s.Intersects(new BoundingBox(new Vector(1, 1, 1), new Vector(5, 5, 5))).Should().BeTrue();
            s.Intersects(new BoundingBox(new Vector(3, 3, 3), new Vector(5, 5, 5))).Should().BeFalse();
        }

        [TestMethod, TestCategory("Queries")]
        public void ClosestPoint_AndSignedDistance_Test()
        {
            var s = new BoundingSphere(Vector.Zero, 5);
            s.ClosestPoint(new Vector(10, 0, 0)).Equals(new Vector(5, 0, 0), LooseTolerance).Should().BeTrue();
            s.ClosestPoint(new Vector(1, 0, 0)).Equals(new Vector(1, 0, 0), Tolerance).Should().BeTrue(); // inside
            s.SignedDistanceTo(new Vector(8, 0, 0)).Should().BeApproximately(3, LooseTolerance);
            s.SignedDistanceTo(new Vector(1, 0, 0)).Should().BeApproximately(-4, LooseTolerance);
        }

        [TestMethod, TestCategory("Combination")]
        public void Merge_GrowsToIncludeAPoint_AndKeepsTheOriginal_Test()
        {
            var s = new BoundingSphere(Vector.Zero, 1);
            BoundingSphere grown = s.Merge(new Vector(5, 0, 0));

            grown.Contains(new Vector(5, 0, 0), LooseTolerance).Should().BeTrue();
            grown.Contains(new Vector(-1, 0, 0), LooseTolerance).Should().BeTrue(); // original far side still inside
        }

        [TestMethod, TestCategory("Factories")]
        public void FromPoints_EnclosesEveryPoint_Test()
        {
            var points = new[]
            {
                new Vector(0, 0, 0),
                new Vector(10, 0, 0),
                new Vector(0, 8, 0),
                new Vector(3, 3, 6),
                new Vector(-4, 2, 1),
            };

            BoundingSphere s = BoundingSphere.FromPoints(points);

            foreach (Vector p in points)
            {
                s.Contains(p, LooseTolerance).Should().BeTrue();
            }
        }

        [TestMethod, TestCategory("Factories")]
        public void FromPoints_WithNoPoints_Throws_Test()
        {
            Action act = () => BoundingSphere.FromPoints();
            act.Should().Throw<ArgumentException>();
        }
    }
}
