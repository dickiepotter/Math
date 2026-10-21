namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Annulus"/> and the positioned <see cref="PlacedAnnulus"/>.</summary>
    [TestClass]
    public class AnnulusTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var a = new Annulus(2, 5);
            Assert.AreEqual(3, a.Width);
            Assert.AreEqual(3.5, a.MeanRadius);
            Assert.AreEqual(Math.PI * (25 - 4), a.Area, Tol);
            Assert.AreEqual(2 * Math.PI * 7, a.Perimeter, Tol);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void InnerExceedingOuter_ShouldThrow_Test()
        {
            Action act = () => new Annulus(5, 2);
            Assert.ThrowsException<ArgumentException>(act);
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_RingHoleAndRim_Test()
        {
            var a = PlacedAnnulus.InXYPlane(new Annulus(2, 5), new Vector(0, 0, 0));
            Assert.IsTrue(a.Contains(new Vector(3, 0, 0), Tol));
            Assert.IsFalse(a.Contains(new Vector(1, 0, 0), Tol)); // in the hole
            Assert.IsFalse(a.Contains(new Vector(6, 0, 0), Tol)); // beyond the rim
            Assert.IsTrue(a.Contains(new Vector(2, 0, 0), Tol));  // on the inner edge
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_PushesToNearestEdge_Test()
        {
            var a = PlacedAnnulus.InXYPlane(new Annulus(2, 5), new Vector(0, 0, 0));
            Assert.IsTrue(a.ClosestPoint(new Vector(1, 0, 0)).Equals(new Vector(2, 0, 0), Tol));  // out to inner edge
            Assert.IsTrue(a.ClosestPoint(new Vector(10, 0, 0)).Equals(new Vector(5, 0, 0), Tol)); // in to outer edge
            Assert.IsTrue(a.ClosestPoint(new Vector(3, 0, 0)).Equals(new Vector(3, 0, 0), Tol));  // already over the ring
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_OnRingAndThroughHole_Test()
        {
            var a = PlacedAnnulus.InXYPlane(new Annulus(2, 5), new Vector(0, 0, 0));
            Assert.IsTrue(a.TryIntersect(new Ray(new Vector(3, 0, 5), new Vector(0, 0, -1)), out Vector p));
            Assert.IsTrue(p.Equals(new Vector(3, 0, 0), 1e-6));
            Assert.IsFalse(a.TryIntersect(new Ray(new Vector(1, 0, 5), new Vector(0, 0, -1)), out _)); // through the hole
        }
    }
}
