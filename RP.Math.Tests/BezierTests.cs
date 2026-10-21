namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="Bezier"/> curve.</summary>
    [TestClass]
    public class BezierTests
    {
        private const double Tolerance = 1e-9;
        private const double LooseTolerance = 1e-6;

        [TestMethod, TestCategory("Construction")]
        public void Construction_WithFewerThanTwoPoints_Throws_Test()
        {
            Action act = () => new Bezier(new Vector(0, 0, 0));
            Assert.ThrowsException<ArgumentException>(act);
        }

        [TestMethod, TestCategory("Accessors")]
        public void Degree_IsOneLessThanControlPointCount_Test()
        {
            Assert.AreEqual(3, Bezier.Cubic(Vector.Zero, Vector.XAxis, Vector.YAxis, Vector.ZAxis).Degree);
            Assert.AreEqual(2, Bezier.Quadratic(Vector.Zero, Vector.XAxis, Vector.YAxis).Degree);
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_AtEnds_ReturnsStartAndEnd_Test()
        {
            var b = Bezier.Cubic(new Vector(0, 0, 0), new Vector(1, 2, 0), new Vector(3, 2, 0), new Vector(4, 0, 0));
            Assert.IsTrue(b.PointAt(0).Equals(b.Start, Tolerance));
            Assert.IsTrue(b.PointAt(1).Equals(b.End, Tolerance));
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_OnLinearBezier_IsTheMidpoint_Test()
        {
            var b = new Bezier(new Vector(0, 0, 0), new Vector(4, 0, 0));
            Assert.IsTrue(b.PointAt(0.5).Equals(new Vector(2, 0, 0), Tolerance));
        }

        [TestMethod, TestCategory("Evaluation")]
        public void PointAt_OnSymmetricQuadratic_HitsTheExpectedHeight_Test()
        {
            // The midpoint of a quadratic is the average of the midpoint of the chord and the control point.
            var b = Bezier.Quadratic(new Vector(0, 0, 0), new Vector(2, 4, 0), new Vector(4, 0, 0));
            Assert.IsTrue(b.PointAt(0.5).Equals(new Vector(2, 2, 0), Tolerance));
        }

        [TestMethod, TestCategory("Evaluation")]
        public void Tangent_OnLinearBezier_PointsAlongTheChord_Test()
        {
            var b = new Bezier(new Vector(0, 0, 0), new Vector(4, 0, 0));
            // Derivative of a linear Bézier is constant: (P1 - P0) once (degree 1).
            Assert.IsTrue(b.Tangent(0.3).Equals(new Vector(4, 0, 0), Tolerance));
        }

        [TestMethod, TestCategory("Length")]
        public void Length_OfLinearBezier_IsTheChordLength_Test()
        {
            var b = new Bezier(new Vector(0, 0, 0), new Vector(3, 4, 0));
            Assert.AreEqual(5, b.Length(16), LooseTolerance);
        }

        [TestMethod, TestCategory("Length")]
        public void Length_WithZeroSegments_Throws_Test()
        {
            Action act = () => new Bezier(Vector.Zero, Vector.XAxis).Length(0);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }
    }
}
