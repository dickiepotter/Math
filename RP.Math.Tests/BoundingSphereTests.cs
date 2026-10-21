namespace RP.Math.Tests
{
    using System;


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
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_DistinguishesInsideFromOutside_Test()
        {
            var s = new BoundingSphere(Vector.Zero, 5);
            Assert.IsTrue(s.Contains(new Vector(3, 0, 0)));
            Assert.IsTrue(s.Contains(new Vector(5, 0, 0))); // on the surface
            Assert.IsFalse(s.Contains(new Vector(6, 0, 0)));
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_OtherSphere_Test()
        {
            var outer = new BoundingSphere(Vector.Zero, 10);
            Assert.IsTrue(outer.Contains(new BoundingSphere(new Vector(2, 0, 0), 3)));
            Assert.IsFalse(outer.Contains(new BoundingSphere(new Vector(8, 0, 0), 3)));
        }

        [TestMethod, TestCategory("Intersection")]
        public void Intersects_OtherSphere_Test()
        {
            var a = new BoundingSphere(Vector.Zero, 2);
            Assert.IsTrue(a.Intersects(new BoundingSphere(new Vector(3, 0, 0), 2)));
            Assert.IsFalse(a.Intersects(new BoundingSphere(new Vector(5, 0, 0), 2)));
            Assert.IsTrue(a.Intersects(new BoundingSphere(new Vector(4, 0, 0), 2))); // touching
        }

        [TestMethod, TestCategory("Intersection")]
        public void Intersects_BoundingBox_Test()
        {
            var s = new BoundingSphere(new Vector(0, 0, 0), 2);
            Assert.IsTrue(s.Intersects(new BoundingBox(new Vector(1, 1, 1), new Vector(5, 5, 5))));
            Assert.IsFalse(s.Intersects(new BoundingBox(new Vector(3, 3, 3), new Vector(5, 5, 5))));
        }

        [TestMethod, TestCategory("Queries")]
        public void ClosestPoint_AndSignedDistance_Test()
        {
            var s = new BoundingSphere(Vector.Zero, 5);
            Assert.IsTrue(s.ClosestPoint(new Vector(10, 0, 0)).Equals(new Vector(5, 0, 0), LooseTolerance));
            Assert.IsTrue(s.ClosestPoint(new Vector(1, 0, 0)).Equals(new Vector(1, 0, 0), Tolerance)); // inside
            Assert.AreEqual(3, s.SignedDistanceTo(new Vector(8, 0, 0)), LooseTolerance);
            Assert.AreEqual(-4, s.SignedDistanceTo(new Vector(1, 0, 0)), LooseTolerance);
        }

        [TestMethod, TestCategory("Combination")]
        public void Merge_GrowsToIncludeAPoint_AndKeepsTheOriginal_Test()
        {
            var s = new BoundingSphere(Vector.Zero, 1);
            BoundingSphere grown = s.Merge(new Vector(5, 0, 0));

            Assert.IsTrue(grown.Contains(new Vector(5, 0, 0), LooseTolerance));
            Assert.IsTrue(grown.Contains(new Vector(-1, 0, 0), LooseTolerance)); // original far side still inside
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
                Assert.IsTrue(s.Contains(p, LooseTolerance));
            }
        }

        [TestMethod, TestCategory("Factories")]
        public void FromPoints_WithNoPoints_Throws_Test()
        {
            Action act = () => BoundingSphere.FromPoints();
            Assert.ThrowsException<ArgumentException>(act);
        }
    }
}
