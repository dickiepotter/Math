namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the conceptual (unpositioned) <see cref="Triangle"/> type.</summary>
    [TestClass]
    public class TriangleTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_StoresSides_Test()
        {
            var t = new Triangle(3, 4, 5);
            Assert.AreEqual(3, t.SideA);
            Assert.AreEqual(4, t.SideB);
            Assert.AreEqual(5, t.SideC);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NonPositiveSide_ShouldThrow_Test()
        {
            Action act = () => new Triangle(0, 1, 1);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_BreaksTriangleInequality_ShouldThrow_Test()
        {
            Action act = () => new Triangle(1, 1, 5);
            Assert.ThrowsException<ArgumentException>(act);
        }

        [TestMethod, TestCategory("Factory")]
        public void RightAngled_DerivesHypotenuse_Test()
        {
            var t = Triangle.RightAngled(3, 4);
            Assert.AreEqual(5, t.SideC, Tol);
        }

        [TestMethod, TestCategory("Measurement")]
        public void Perimeter_And_Area_ShouldMatchFormulae_Test()
        {
            var t = new Triangle(3, 4, 5);
            Assert.AreEqual(12, t.Perimeter);
            Assert.AreEqual(6, t.Semiperimeter);
            Assert.AreEqual(6, t.Area, Tol); // Heron: sqrt(6*3*2*1)
        }

        [TestMethod, TestCategory("Measurement")]
        public void Equilateral_Area_ShouldMatchFormula_Test()
        {
            var t = Triangle.Equilateral(2);
            Assert.AreEqual(System.Math.Sqrt(3), t.Area, Tol); // (sqrt3/4)*side^2
        }

        [TestMethod, TestCategory("Angles")]
        public void Angles_OfRightTriangle_ShouldSumAndIncludeRightAngle_Test()
        {
            var t = new Triangle(3, 4, 5);
            Assert.AreEqual(90, t.AngleC.Deg, 1e-6);            // opposite the longest side
            Assert.AreEqual(180, (t.AngleA.Deg + t.AngleB.Deg + t.AngleC.Deg), 1e-6);
        }

        [TestMethod, TestCategory("Classification")]
        public void Classification_ShouldIdentifyKinds_Test()
        {
            Assert.IsTrue(Triangle.Equilateral(2).IsEquilateral(Tol));
            Assert.IsTrue(new Triangle(2, 2, 3).IsIsosceles(Tol));
            Assert.IsTrue(new Triangle(3, 4, 5).IsScalene(Tol));
            Assert.IsTrue(new Triangle(3, 4, 5).IsRightAngled(1e-6));
            Assert.IsFalse(Triangle.Equilateral(2).IsRightAngled(1e-6));
        }

        [TestMethod, TestCategory("Comparison")]
        public void Comparison_ShouldOrderByArea_Test()
        {
            var small = Triangle.Equilateral(2);   // area ~1.73
            var large = new Triangle(3, 4, 5);      // area 6
            Assert.IsTrue((large > small));
            Assert.IsTrue((small < large));
            Assert.IsTrue((large.CompareTo(small)) > (0));
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareSides_Test()
        {
            Assert.IsTrue(new Triangle(3, 4, 5).Equals(new Triangle(3, 4, 5)));
            Assert.IsTrue((new Triangle(3, 4, 5) == new Triangle(3, 4, 5)));
            Assert.IsTrue((new Triangle(3, 4, 5) != new Triangle(4, 4, 5)));
        }

        [TestMethod, TestCategory("Deconstruction")]
        public void Deconstruct_ShouldYieldSides_Test()
        {
            var (a, b, c) = new Triangle(3, 4, 5);
            Assert.AreEqual(3, a);
            Assert.AreEqual(4, b);
            Assert.AreEqual(5, c);
        }
    }
}
