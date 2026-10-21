namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the conceptual (unpositioned) <see cref="Rectangle"/> type.</summary>
    [TestClass]
    public class RectangleTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_StoresSizes_Test()
        {
            var r = new Rectangle(3, 4);
            Assert.AreEqual(3, r.Width);
            Assert.AreEqual(4, r.Height);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeSize_ShouldThrow_Test()
        {
            Action act = () => new Rectangle(-1, 2);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldMatchFormulae_Test()
        {
            var r = new Rectangle(3, 4);
            Assert.AreEqual(12, r.Area);
            Assert.AreEqual(14, r.Perimeter);
            Assert.AreEqual(5, r.DiagonalLength, Tol);
            Assert.AreEqual(0.75, r.AspectRatio, Tol);
        }

        [TestMethod, TestCategory("Classification")]
        public void IsSquare_ShouldDetectEqualSides_Test()
        {
            Assert.IsTrue(Rectangle.Square(5).IsSquare(Tol));
            Assert.IsFalse(new Rectangle(3, 4).IsSquare(Tol));
        }

        [TestMethod, TestCategory("Comparison")]
        public void Comparison_ShouldOrderByArea_Test()
        {
            var small = new Rectangle(2, 2); // area 4
            var large = new Rectangle(3, 4); // area 12
            Assert.IsTrue((large > small));
            Assert.IsTrue((small <= large));
            Assert.IsTrue((large.CompareTo(small)) > (0));
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareSizes_Test()
        {
            Assert.IsTrue((new Rectangle(3, 4) == new Rectangle(3, 4)));
            Assert.IsTrue((new Rectangle(3, 4) != new Rectangle(4, 3)));
            Assert.IsTrue(new Rectangle(3, 4).Equals(new Rectangle(3.0000001, 4), 1e-3));
        }

        [TestMethod, TestCategory("Deconstruction")]
        public void Deconstruct_ShouldYieldSizes_Test()
        {
            var (w, h) = new Rectangle(3, 4);
            Assert.AreEqual(3, w);
            Assert.AreEqual(4, h);
        }
    }
}
