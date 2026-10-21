namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the intersection and closest-approach maths that ties the line family
    /// (<see cref="Line"/>, <see cref="Ray"/>, <see cref="LineSegment"/>) to <see cref="Plane"/> and to
    /// itself.
    /// </summary>
    [TestClass]
    public class PlaneLineIntersectionTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        #region Plane and Line

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectLine_Crossing_ShouldReturnThePoint_Test()
        {
            var plane = Plane.XY; // z = 0
            var line = new Line(new Vector(2, 3, 5), new Vector(0, 0, -1)); // straight down onto z = 0

            Assert.IsTrue(plane.TryIntersect(line, out Vector hit));
            Assert.IsTrue(hit.Equals(new Vector(2, 3, 0), LooseTol));
        }

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectLine_Parallel_ShouldReturnFalse_Test()
        {
            var plane = Plane.XY;
            var line = new Line(new Vector(0, 0, 4), new Vector(1, 0, 0)); // runs parallel to z = 0

            Assert.IsFalse(plane.TryIntersect(line, out _));
        }

        [TestMethod, TestCategory("Line")]
        public void Line_TryIntersectPlane_ShouldAgreeWithThePlane_Test()
        {
            var plane = Plane.XY;
            var line = new Line(new Vector(2, 3, 5), new Vector(0, 0, -1));

            Assert.IsTrue(line.TryIntersect(plane, out Vector hit));
            Assert.IsTrue(hit.Equals(new Vector(2, 3, 0), LooseTol));
        }

        #endregion

        #region Plane and Ray

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectRay_PointingAtThePlane_ShouldReturnThePoint_Test()
        {
            var plane = Plane.XY;
            var ray = new Ray(new Vector(0, 0, 5), new Vector(0, 0, -1)); // aimed at z = 0

            Assert.IsTrue(plane.TryIntersect(ray, out Vector hit));
            Assert.IsTrue(hit.Equals(new Vector(0, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectRay_PointingAway_ShouldReturnFalse_Test()
        {
            var plane = Plane.XY;
            var ray = new Ray(new Vector(0, 0, 5), new Vector(0, 0, 1)); // crossing is behind the origin

            Assert.IsFalse(plane.TryIntersect(ray, out _));
        }

        #endregion

        #region Plane and LineSegment

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectSegment_Crossing_ShouldReturnThePoint_Test()
        {
            var plane = Plane.XY;
            var segment = new LineSegment(new Vector(0, 0, 2), new Vector(0, 0, -2)); // crosses z = 0 at the middle

            Assert.IsTrue(plane.TryIntersect(segment, out Vector hit));
            Assert.IsTrue(hit.Equals(new Vector(0, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Plane")]
        public void Plane_TryIntersectSegment_NotReaching_ShouldReturnFalse_Test()
        {
            var plane = Plane.XY;
            var segment = new LineSegment(new Vector(0, 0, 2), new Vector(0, 0, 1)); // both ends above z = 0

            Assert.IsFalse(plane.TryIntersect(segment, out _));
        }

        #endregion

        #region Line and Line

        [TestMethod, TestCategory("LineLine")]
        public void Line_TryIntersectLine_Crossing_ShouldReturnThePoint_Test()
        {
            var x = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0)); // the X axis
            var y = new Line(new Vector(0, 0, 0), new Vector(0, 1, 0)); // the Y axis

            Assert.IsTrue(x.TryIntersect(y, Tol, out Vector hit));
            Assert.IsTrue(hit.Equals(new Vector(0, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("LineLine")]
        public void Line_TryIntersectLine_Skew_ShouldReturnFalse_Test()
        {
            var x = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));      // along X at z = 0
            var skew = new Line(new Vector(0, 0, 1), new Vector(0, 1, 0));   // along Y at z = 1 (never meets)

            Assert.IsFalse(x.TryIntersect(skew, Tol, out _));
        }

        [TestMethod, TestCategory("LineLine")]
        public void Line_DistanceTo_SkewLines_ShouldBeTheGap_Test()
        {
            var x = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));    // along X at z = 0
            var skew = new Line(new Vector(0, 0, 1), new Vector(0, 1, 0)); // along Y at z = 1

            Assert.AreEqual(1, x.DistanceTo(skew), LooseTol);
        }

        [TestMethod, TestCategory("LineLine")]
        public void Line_DistanceTo_ParallelLines_ShouldBeThePerpendicularGap_Test()
        {
            var a = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var b = new Line(new Vector(0, 3, 0), new Vector(1, 0, 0)); // parallel, 3 apart in Y

            Assert.AreEqual(3, a.DistanceTo(b), LooseTol);
        }

        [TestMethod, TestCategory("LineLine")]
        public void Line_ClosestPointTo_OtherLine_ShouldBeTheFootOfTheCommonPerpendicular_Test()
        {
            var x = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));      // along X at z = 0
            var skew = new Line(new Vector(2, 0, 1), new Vector(0, 1, 0));   // along Y through x = 2 at z = 1

            // Nearest point on the X axis is directly under the crossing at x = 2.
            Assert.IsTrue(x.ClosestPointTo(skew).Equals(new Vector(2, 0, 0), LooseTol));
        }

        #endregion
    }
}
