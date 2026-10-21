namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Circle"/> and the positioned <see cref="PlacedCircle"/>.</summary>
    [TestClass]
    public class CircleTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var c = new Circle(3);
            Assert.AreEqual(Math.PI * 9, c.Area, Tol);
            Assert.AreEqual(2 * Math.PI * 3, c.Circumference, Tol);
            Assert.AreEqual(2 * Math.PI * 3, c.Perimeter, Tol);
            Assert.AreEqual(6, c.Diameter);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void NegativeRadius_ShouldThrow_Test()
        {
            Action act = () => new Circle(-1);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Comparison_ByArea_Test()
        {
            Assert.IsTrue((new Circle(3) > new Circle(2)));
            Assert.IsTrue((new Circle(3).CompareTo(new Circle(2))) > (0));
        }

        [TestMethod, TestCategory("Placed")]
        public void Placement_ExposesCentreAndNormal_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(3), new Vector(1, 2, 0));
            Assert.AreEqual(new Vector(1, 2, 0), c.Center);
            Assert.AreEqual(new Vector(1, 2, 0), c.Centroid);
            Assert.IsTrue(c.Normal.Equals(new Vector(0, 0, 1), Tol));
            Assert.AreEqual(Math.PI * 9, c.Area, Tol);
        }

        [TestMethod, TestCategory("Placed")]
        public void FromCenterNormal_ShouldOrientPlane_Test()
        {
            var c = PlacedCircle.FromCenterNormal(new Circle(4), new Vector(1, 2, 3), new Vector(0, 1, 0));
            Assert.IsTrue(c.Normal.Equals(new Vector(0, 1, 0), LooseTol));
            Assert.IsTrue(c.Plane.Contains(c.Center, LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_OnDisc_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            Assert.IsTrue(c.Contains(new Vector(3, 4, 0), Tol));  // on the rim
            Assert.IsTrue(c.Contains(new Vector(1, 1, 0), Tol));
            Assert.IsFalse(c.Contains(new Vector(6, 0, 0), Tol)); // beyond the radius
            Assert.IsFalse(c.Contains(new Vector(1, 1, 2), Tol)); // off the plane
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_OutsidePoint_ShouldClampToRim_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            Assert.IsTrue(c.ClosestPoint(new Vector(20, 0, 7)).Equals(new Vector(5, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_ThroughDisc_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            var line = new Line(new Vector(1, 1, 5), new Vector(0, 0, -1));
            Assert.IsTrue(c.TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(1, 1, 0), LooseTol));
            Assert.IsTrue(far.Equals(near, LooseTol)); // a flat disc has a single crossing

        }
    }
}
