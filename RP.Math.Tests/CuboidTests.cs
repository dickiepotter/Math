namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the conceptual (unpositioned) <see cref="Cuboid"/> type.</summary>
    [TestClass]
    public class CuboidTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_StoresSizes_Test()
        {
            var c = new Cuboid(2, 3, 4);
            Assert.AreEqual(2, c.Width);
            Assert.AreEqual(3, c.Height);
            Assert.AreEqual(4, c.Depth);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeSize_ShouldThrow_Test()
        {
            Action act = () => new Cuboid(2, -3, 4);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldMatchFormulae_Test()
        {
            var c = new Cuboid(2, 3, 4);
            Assert.AreEqual(24, c.Volume);
            Assert.AreEqual(52, c.SurfaceArea);                       // 2(6 + 12 + 8)
            Assert.AreEqual(System.Math.Sqrt(29), c.SpaceDiagonal, Tol);
            Assert.AreEqual(new Vector(2, 3, 4), c.Size);
        }

        [TestMethod, TestCategory("Classification")]
        public void IsCube_ShouldDetectEqualEdges_Test()
        {
            Assert.IsTrue(Cuboid.Cube(3).IsCube(Tol));
            Assert.IsFalse(new Cuboid(2, 3, 4).IsCube(Tol));
        }

        [TestMethod, TestCategory("Comparison")]
        public void Comparison_ShouldOrderByVolume_Test()
        {
            var small = Cuboid.Cube(2); // 8
            var large = new Cuboid(2, 3, 4); // 24
            Assert.IsTrue((large > small));
            Assert.IsTrue((large.CompareTo(small)) > (0));
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareSizes_Test()
        {
            Assert.IsTrue((new Cuboid(2, 3, 4) == new Cuboid(2, 3, 4)));
            Assert.IsTrue((new Cuboid(2, 3, 4) != new Cuboid(2, 3, 5)));
        }

        [TestMethod, TestCategory("Deconstruction")]
        public void Deconstruct_ShouldYieldSizes_Test()
        {
            var (w, h, d) = new Cuboid(2, 3, 4);
            Assert.AreEqual(2, w);
            Assert.AreEqual(3, h);
            Assert.AreEqual(4, d);
        }
    }
}
