namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the <see cref="Chord"/> type (a line segment that knows its circle's radius).</summary>
    [TestClass]
    public class ChordTests
    {
        private const double Tol = 1e-9;

        // A chord of length 6 across a circle of radius 5.
        private static Chord SixAcrossFive()
        {
            return new Chord(new Vector(-3, 0, 0), new Vector(3, 0, 0), 5);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeRadius_ShouldThrow_Test()
        {
            Action act = () => new Chord(new Vector(0, 0, 0), new Vector(1, 0, 0), -1);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Measurement")]
        public void Length_And_Diameter_Test()
        {
            var c = SixAcrossFive();
            Assert.AreEqual(6, c.Length, Tol);
            Assert.AreEqual(10, c.Diameter);
        }

        [TestMethod, TestCategory("Geometry")]
        public void DistanceFromCentre_And_Sagitta_Test()
        {
            var c = SixAcrossFive();
            Assert.AreEqual(4, c.DistanceFromCentre, Tol); // sqrt(25 - 9)
            Assert.AreEqual(1, c.Sagitta, Tol);            // 5 - 4
        }

        [TestMethod, TestCategory("Geometry")]
        public void CentralAngle_And_ArcLength_Test()
        {
            var c = SixAcrossFive();
            double expectedAngle = 2 * Math.Asin(3.0 / 5.0); // 2·asin((c/2)/r)
            Assert.AreEqual(expectedAngle, c.CentralAngle.Rad, Tol);
            Assert.AreEqual(5 * expectedAngle, c.ArcLength, Tol);
        }

        [TestMethod, TestCategory("Geometry")]
        public void Diameter_Chord_ShouldSubtendStraightAngle_Test()
        {
            var c = new Chord(new Vector(-5, 0, 0), new Vector(5, 0, 0), 5); // a full diameter
            Assert.AreEqual(Math.PI, c.CentralAngle.Rad, Tol);
            Assert.AreEqual(5, c.Sagitta, Tol); // the arc bulges a full radius
        }
    }
}
