namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the cross-type intersection maths between the placed solid/planar shapes
    /// (<see cref="PlacedSphere"/>, <see cref="PlacedCircle"/>) and the line family.
    /// </summary>
    [TestClass]
    public class ShapeIntersectionTests
    {
        private const double LooseTol = 1e-6;

        #region Sphere

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectRay_PointingAtCentre_ShouldHitNearSide_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            var ray = new Ray(new Vector(10, 0, 0), new Vector(-1, 0, 0));
            Assert.IsTrue(s.TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(2, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectRay_PointingAway_ShouldMiss_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            var ray = new Ray(new Vector(10, 0, 0), new Vector(1, 0, 0));
            Assert.IsFalse(s.TryIntersect(ray, out _));
        }

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectRay_FromInside_ShouldHitTheExit_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(s.TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(2, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectLine_ShouldGiveBothCrossings_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(s.TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(-2, 0, 0), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(2, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("SphereIntersect")]
        public void Sphere_TryIntersectLine_Missing_ShouldBeFalse_Test()
        {
            var s = PlacedSphere.At(new Sphere(2), new Vector(0, 0, 0));
            var line = new Line(new Vector(0, 5, 0), new Vector(1, 0, 0)); // passes 5 above, misses r = 2
            Assert.IsFalse(s.TryIntersect(line, out _, out _));
        }

        #endregion

        #region Circle

        [TestMethod, TestCategory("CircleIntersect")]
        public void Circle_TryIntersectLine_ThroughDisc_ShouldHit_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            var line = new Line(new Vector(1, 1, 5), new Vector(0, 0, -1)); // straight down through the disc
            Assert.IsTrue(c.TryIntersect(line, out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(1, 1, 0), LooseTol));
            Assert.IsTrue(far.Equals(near, LooseTol)); // a flat disc has a single crossing

        }

        [TestMethod, TestCategory("CircleIntersect")]
        public void Circle_TryIntersectLine_CrossingPlaneOutsideRadius_ShouldMiss_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            var line = new Line(new Vector(20, 0, 5), new Vector(0, 0, -1)); // hits the plane at x = 20, outside r = 5
            Assert.IsFalse(c.TryIntersect(line, out _, out _));
        }

        [TestMethod, TestCategory("CircleIntersect")]
        public void Circle_TryIntersectRay_PointingAway_ShouldMiss_Test()
        {
            var c = PlacedCircle.InXYPlane(new Circle(5), new Vector(0, 0, 0));
            var ray = new Ray(new Vector(0, 0, 5), new Vector(0, 0, 1)); // away from the plane
            Assert.IsFalse(c.TryIntersect(ray, out _));
        }

        #endregion
    }
}
