namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="Ray"/> type.</summary>
    [TestClass]
    public class RayTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_ShouldNormalizeDirection_Test()
        {
            var ray = new Ray(new Vector(0, 0, 0), new Vector(9, 0, 0));
            Assert.IsTrue(ray.Direction.Equals(new Vector(1, 0, 0), Tol));
        }

        [TestMethod, TestCategory("Points")]
        public void PointAt_ShouldClampBehindOrigin_Test()
        {
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(ray.PointAt(5).Equals(new Vector(5, 0, 0), Tol));
            Assert.IsTrue(ray.PointAt(-3).Equals(new Vector(0, 0, 0), Tol)); // clamped to the origin
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPointTo_BehindOrigin_ShouldReturnOrigin_Test()
        {
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(ray.ClosestPointTo(new Vector(-5, 2, 0)).Equals(new Vector(0, 0, 0), Tol));
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPointTo_AheadOfOrigin_ShouldProject_Test()
        {
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.IsTrue(ray.ClosestPointTo(new Vector(3, 4, 0)).Equals(new Vector(3, 0, 0), Tol));
            Assert.AreEqual(4, ray.DistanceTo(new Vector(3, 4, 0)), Tol);
        }

        [TestMethod, TestCategory("Conversion")]
        public void ToLine_ShouldExtendBothWays_Test()
        {
            var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var line = ray.ToLine();
            Assert.IsTrue(line.PointAt(-5).Equals(new Vector(-5, 0, 0), Tol));
        }
    }
}
