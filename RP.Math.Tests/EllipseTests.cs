namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Ellipse"/> and the positioned <see cref="PlacedEllipse"/>.</summary>
    [TestClass]
    public class EllipseTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var e = new Ellipse(3, 2);
            Assert.AreEqual(Math.PI * 6, e.Area, Tol);
            Assert.AreEqual(3, e.SemiMajor);
            Assert.AreEqual(2, e.SemiMinor);
            Assert.AreEqual(Math.Sqrt(5), e.FocalDistance, Tol);
            Assert.AreEqual(Math.Sqrt(5) / 3.0, e.Eccentricity, Tol);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Circle_Case_Test()
        {
            var e = new Ellipse(2, 2);
            Assert.IsTrue(e.IsCircle(Tol));
            Assert.AreEqual(0, e.Eccentricity, Tol);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void NegativeAxis_ShouldThrow_Test()
        {
            Action act = () => new Ellipse(-1, 2);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Comparison_ByArea_Test()
        {
            Assert.IsTrue((new Ellipse(3, 2) > new Ellipse(1, 1)));
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_OnAndBeyondBoundary_Test()
        {
            var e = PlacedEllipse.InXYPlane(new Ellipse(3, 2), new Vector(0, 0, 0));
            Assert.IsTrue(e.Contains(new Vector(3, 0, 0), LooseTol));
            Assert.IsTrue(e.Contains(new Vector(0, 2, 0), LooseTol));
            Assert.IsFalse(e.Contains(new Vector(3.1, 0, 0), Tol));
            Assert.IsFalse(e.Contains(new Vector(0, 0, 0.5), Tol)); // off the plane
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_OnAxes_Test()
        {
            var e = PlacedEllipse.InXYPlane(new Ellipse(3, 2), new Vector(0, 0, 0));
            Assert.IsTrue(e.ClosestPoint(new Vector(10, 0, 0)).Equals(new Vector(3, 0, 0), LooseTol));
            Assert.IsTrue(e.ClosestPoint(new Vector(0, 10, 0)).Equals(new Vector(0, 2, 0), LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestPoint_GenericExterior_ShouldLandOnBoundary_Test()
        {
            var e = PlacedEllipse.InXYPlane(new Ellipse(3, 2), new Vector(0, 0, 0));
            Vector p = e.ClosestPoint(new Vector(10, 10, 0));
            // The returned point must satisfy the ellipse equation (x/3)^2 + (y/2)^2 = 1.
            double onBoundary = ((p.X * p.X) / 9.0) + ((p.Y * p.Y) / 4.0);
            Assert.AreEqual(1, onBoundary, LooseTol);
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersect_ThroughDisc_Test()
        {
            var e = PlacedEllipse.InXYPlane(new Ellipse(3, 2), new Vector(0, 0, 0));
            var ray = new Ray(new Vector(1, 0.5, 5), new Vector(0, 0, -1));
            Assert.IsTrue(e.TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(1, 0.5, 0), LooseTol));
        }
    }
}
