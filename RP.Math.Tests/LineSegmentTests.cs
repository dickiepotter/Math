namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the finite <see cref="LineSegment"/> type.</summary>
    [TestClass]
    public class LineSegmentTests
    {
        private const double Tol = 1e-9;

        private static LineSegment ThreeFourFive()
        {
            return new LineSegment(new Vector(0, 0, 0), new Vector(3, 4, 0));
        }

        [TestMethod, TestCategory("Measurement")]
        public void Length_Midpoint_Direction_Test()
        {
            var s = ThreeFourFive();
            Assert.AreEqual(5, s.Length, Tol);
            Assert.IsTrue(s.Midpoint.Equals(new Vector(1.5, 2, 0), Tol));
            Assert.IsTrue(s.Direction.Equals(new Vector(0.6, 0.8, 0), Tol));
        }

        [TestMethod, TestCategory("Points")]
        public void PointAt_ShouldClampToEnds_Test()
        {
            var s = ThreeFourFive();
            Assert.IsTrue(s.PointAt(0.25).Equals(new Vector(0.75, 1, 0), Tol));
            Assert.IsTrue(s.PointAt(-1).Equals(new Vector(0, 0, 0), Tol)); // clamped to the tail
            Assert.IsTrue(s.PointAt(2).Equals(new Vector(3, 4, 0), Tol));   // clamped to the head
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPointTo_ShouldClampToSegment_Test()
        {
            var s = ThreeFourFive();
            Assert.IsTrue(s.ClosestPointTo(new Vector(10, 10, 0)).Equals(new Vector(3, 4, 0), Tol)); // beyond the head
            Assert.IsTrue(s.ClosestPointTo(new Vector(-5, -5, 0)).Equals(new Vector(0, 0, 0), Tol)); // before the tail
        }

        [TestMethod, TestCategory("Conversion")]
        public void ToLine_And_ToRay_Test()
        {
            var s = ThreeFourFive();
            Assert.IsTrue(s.ToLine().Contains(new Vector(6, 8, 0), Tol)); // the unbounded line keeps going
            Assert.IsTrue(s.ToRay().Origin.Equals(new Vector(0, 0, 0), Tol));
        }

        [TestMethod, TestCategory("Producing")]
        public void Reversed_And_Translate_Test()
        {
            var s = ThreeFourFive();
            var r = s.Reversed();
            Assert.IsTrue(r.Tail.Equals(new Vector(3, 4, 0), Tol));
            Assert.IsTrue(r.Head.Equals(new Vector(0, 0, 0), Tol));

            var moved = s.Translate(new Vector(1, 1, 1));
            Assert.IsTrue(moved.Tail.Equals(new Vector(1, 1, 1), Tol));
            Assert.IsTrue(moved.Head.Equals(new Vector(4, 5, 1), Tol));
        }

        [TestMethod, TestCategory("Producing")]
        public void RotateZ_ShouldPivotAboutTailAndPreserveLength_Test()
        {
            var s = new LineSegment(new Vector(0, 0, 0), new Vector(1, 0, 0));
            var rotated = s.RotateZ(new Angle(90, AngleUnits.DEG));
            Assert.IsTrue(rotated.Tail.Equals(new Vector(0, 0, 0), Tol)); // tail is the pivot
            Assert.AreEqual(1, rotated.Length, Tol);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_ShouldBlendEnds_Test()
        {
            var s = ThreeFourFive();
            Assert.IsTrue(s.Interpolate(0.5).Equals(new Vector(1.5, 2, 0), Tol));
        }
    }
}
