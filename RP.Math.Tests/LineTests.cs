namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the infinite <see cref="Line"/> type.</summary>
    [TestClass]
    public class LineTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_ShouldNormalizeDirection_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(0, 0, 5));
            Assert.IsTrue(line.Direction.Equals(new Vector(0, 0, 1), Tol));
        }

        [TestMethod, TestCategory("Construction")]
        public void ThroughPoints_ShouldRunBetweenThem_Test()
        {
            var line = Line.ThroughPoints(new Vector(0, 0, 0), new Vector(2, 2, 0));
            Assert.IsTrue(line.Contains(new Vector(1, 1, 0), Tol));
        }

        [TestMethod, TestCategory("Points")]
        public void PointAt_ShouldTravelBothWays_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(line.PointAt(3).Equals(new Vector(3, 0, 0), Tol));
            Assert.IsTrue(line.PointAt(-3).Equals(new Vector(-3, 0, 0), Tol)); // a line is infinite both ways
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPointTo_And_DistanceTo_Test()
        {
            var line = new Line(new Vector(0, 0, 0), new Vector(1, 1, 0));
            Assert.IsTrue(line.ClosestPointTo(new Vector(0, 2, 0)).Equals(new Vector(1, 1, 0), Tol));
            Assert.AreEqual(System.Math.Sqrt(2), line.DistanceTo(new Vector(0, 2, 0)), Tol);
        }

        [TestMethod, TestCategory("Relationship")]
        public void Parallel_Lines_ShouldReportDistance_Test()
        {
            var a = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var b = new Line(new Vector(0, 5, 0), new Vector(1, 0, 0));
            Assert.IsTrue(a.IsParallelTo(b, Tol));
            Assert.AreEqual(5, a.DistanceTo(b), Tol);
        }

        [TestMethod, TestCategory("Relationship")]
        public void Skew_Lines_ShouldMeasureGap_Test()
        {
            var a = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var b = new Line(new Vector(0, 0, 1), new Vector(0, 1, 0)); // passes 1 above, perpendicular
            Assert.AreEqual(1, a.DistanceTo(b), Tol);
        }

        [TestMethod, TestCategory("Relationship")]
        public void TryIntersect_CrossingLines_ShouldReturnPoint_Test()
        {
            var a = new Line(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var b = new Line(new Vector(1, -1, 0), new Vector(0, 1, 0));
            Assert.IsTrue(a.TryIntersect(b, Tol, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(1, 0, 0), Tol));
        }

        [TestMethod, TestCategory("Conversion")]
        public void ToRay_ShouldShareOriginAndDirection_Test()
        {
            var line = new Line(new Vector(1, 2, 3), new Vector(0, 0, 1));
            var ray = line.ToRay();
            Assert.IsTrue(ray.Origin.Equals(new Vector(1, 2, 3), Tol));
            Assert.IsTrue(ray.Direction.Equals(new Vector(0, 0, 1), Tol));
        }
    }
}
