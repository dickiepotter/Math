namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Ellipsoid"/> and the positioned <see cref="PlacedEllipsoid"/>.</summary>
    [TestClass]
    public class EllipsoidTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Volume_Test()
        {
            var e = new Ellipsoid(3, 2, 1);
            Assert.AreEqual((4.0 / 3.0) * Math.PI * 6, e.Volume, Tol);
            Assert.IsFalse(e.IsSphere(Tol));
            Assert.IsTrue(new Ellipsoid(2, 2, 2).IsSphere(Tol));
        }

        [TestMethod, TestCategory("Conceptual")]
        public void NegativeAxis_ShouldThrow_Test()
        {
            Action act = () => new Ellipsoid(1, -1, 1);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_Test()
        {
            var e = PlacedEllipsoid.AxisAligned(new Ellipsoid(3, 2, 1), new Vector(0, 0, 0));
            Assert.IsTrue(e.Contains(new Vector(3, 0, 0), LooseTol));
            Assert.IsTrue(e.Contains(new Vector(0, 0, 1), LooseTol));
            Assert.IsFalse(e.Contains(new Vector(3.1, 0, 0), Tol));
            Assert.IsTrue(e.Contains(new Vector(1, 1, 0.5), Tol));
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_OnAxisAndGeneric_Test()
        {
            var e = PlacedEllipsoid.AxisAligned(new Ellipsoid(3, 2, 1), new Vector(0, 0, 0));
            Assert.IsTrue(e.ClosestPoint(new Vector(10, 0, 0)).Equals(new Vector(3, 0, 0), LooseTol));
            Assert.IsTrue(e.ClosestPoint(new Vector(0, 0, 10)).Equals(new Vector(0, 0, 1), LooseTol));

            Vector p = e.ClosestPoint(new Vector(10, 10, 10));
            double onSurface = ((p.X * p.X) / 9.0) + ((p.Y * p.Y) / 4.0) + (p.Z * p.Z);
            Assert.AreEqual(1, onSurface, LooseTol);
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_Test()
        {
            var e = PlacedEllipsoid.AxisAligned(new Ellipsoid(3, 2, 1), new Vector(0, 0, 0));
            Assert.IsTrue(e.TryIntersect(new Ray(new Vector(10, 0, 0), new Vector(-1, 0, 0)), out Vector p));
            Assert.IsTrue(p.Equals(new Vector(3, 0, 0), LooseTol));

            Assert.IsTrue(e.TryIntersect(new Line(new Vector(0, 0, 0), new Vector(0, 0, 1)), out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(0, 0, -1), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(0, 0, 1), LooseTol));
        }
    }
}
