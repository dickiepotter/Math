namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual <see cref="Sphere"/> and the positioned <see cref="PlacedSphere"/>.</summary>
    [TestClass]
    public class SphereTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        [TestMethod, TestCategory("Conceptual")]
        public void Measurements_Test()
        {
            var s = new Sphere(2);
            Assert.AreEqual((4.0 / 3.0) * Math.PI * 8, s.Volume, Tol);
            Assert.AreEqual(4 * Math.PI * 4, s.SurfaceArea, Tol);
            Assert.AreEqual(4, s.Diameter);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Unit_And_NegativeRadius_Test()
        {
            Assert.AreEqual(1, Sphere.Unit.Radius);
            Action act = () => new Sphere(-1);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Conceptual")]
        public void Comparison_ByVolume_Test()
        {
            Assert.IsTrue((new Sphere(3) > new Sphere(2)));
        }

        [TestMethod, TestCategory("Placed")]
        public void Contains_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            Assert.IsTrue(s.Contains(new Vector(1, 0, 0)));
            Assert.IsTrue(s.Contains(new Vector(2, 0, 0)));   // on the surface
            Assert.IsFalse(s.Contains(new Vector(3, 0, 0)));
        }

        [TestMethod, TestCategory("Placed")]
        public void ClosestSurfacePoint_And_SignedDistance_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            Assert.IsTrue(s.ClosestSurfacePoint(new Vector(10, 0, 0)).Equals(new Vector(2, 0, 0), LooseTol));
            Assert.AreEqual(3, s.SignedDistanceTo(new Vector(5, 0, 0)), Tol);
            Assert.AreEqual(-1, s.SignedDistanceTo(new Vector(1, 0, 0)), Tol); // inside
        }

        [TestMethod, TestCategory("Placed")]
        public void Intersects_OtherSphere_Test()
        {
            var a = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            var near = PlacedSphere.At(new Sphere(2), new Vector(3, 0, 0));
            var far = PlacedSphere.At(new Sphere(2), new Vector(10, 0, 0));
            Assert.IsTrue(a.Intersects(near));
            Assert.IsFalse(a.Intersects(far));
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectRay_FromOutsideAndInside_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            Assert.IsTrue(s.TryIntersect(new Ray(new Vector(10, 0, 0), new Vector(-1, 0, 0)), out Vector p));
            Assert.IsTrue(p.Equals(new Vector(2, 0, 0), LooseTol));

            Assert.IsTrue(s.TryIntersect(new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0)), out Vector exit));
            Assert.IsTrue(exit.Equals(new Vector(2, 0, 0), LooseTol));

            Assert.IsFalse(s.TryIntersect(new Ray(new Vector(10, 0, 0), new Vector(1, 0, 0)), out _));
        }

        [TestMethod, TestCategory("Placed")]
        public void TryIntersectLine_BothCrossings_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            Assert.IsTrue(s.TryIntersect(new Line(new Vector(0, 0, 0), new Vector(1, 0, 0)), out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(-2, 0, 0), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(2, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Placed")]
        public void Translate_MovesCentre_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0)).Translate(new Vector(1, 2, 3));
            Assert.IsTrue(s.Center.Equals(new Vector(1, 2, 3), Tol));
        }
    }
}
