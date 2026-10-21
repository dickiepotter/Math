namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for <see cref="Plane.TryIntersect(Plane, out Line)"/>.</summary>
    [TestClass]
    public class PlanePlaneIntersectionTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        /// <summary>Assert that an infinite line lies in a plane (two of its points are on the plane).</summary>
        private static void LineShouldLieIn(Plane plane, Line line)
        {
            Assert.IsTrue(plane.Contains(line.Point, LooseTol));
            Assert.IsTrue(plane.Contains(line.PointAt(5), LooseTol));
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_XYWithXZ_GivesTheXAxis_Test()
        {
            Assert.IsTrue(Plane.XY.TryIntersect(Plane.XZ, out Line line));

            // The line of intersection lies in both planes...
            LineShouldLieIn(Plane.XY, line);
            LineShouldLieIn(Plane.XZ, line);

            // ...and runs along the X axis (parallel to it, either sense).
            Assert.AreEqual(0, line.Direction.CrossProduct(Vector.XAxis).Magnitude, LooseTol);
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_OffsetAxisPlanes_GivesTheExpectedLine_Test()
        {
            var x2 = Plane.FromPointNormal(new Vector(2, 0, 0), new Vector(1, 0, 0)); // x = 2
            var y3 = Plane.FromPointNormal(new Vector(0, 3, 0), new Vector(0, 1, 0)); // y = 3

            Assert.IsTrue(x2.TryIntersect(y3, out Line line));

            LineShouldLieIn(x2, line);
            LineShouldLieIn(y3, line);

            // The line is { x = 2, y = 3, z free }: parallel to Z and passing through (2, 3, 0).
            Assert.AreEqual(0, line.Direction.CrossProduct(Vector.ZAxis).Magnitude, LooseTol);
            Assert.IsTrue(line.Contains(new Vector(2, 3, 0), LooseTol));
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_ObliquePlanes_LineLiesInBoth_Test()
        {
            var p1 = Plane.FromPointNormal(new Vector(1, 0, 0), new Vector(1, 1, 0));
            var p2 = Plane.FromPointNormal(new Vector(0, 0, 2), new Vector(0, 1, 2));

            Assert.IsTrue(p1.TryIntersect(p2, out Line line));

            LineShouldLieIn(p1, line);
            LineShouldLieIn(p2, line);

            // Direction is perpendicular to both normals.
            Assert.AreEqual(0, line.Direction.DotProduct(p1.Normal), LooseTol);
            Assert.AreEqual(0, line.Direction.DotProduct(p2.Normal), LooseTol);
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_ParallelPlanes_ReturnsFalse_Test()
        {
            var z5 = Plane.FromPointNormal(new Vector(0, 0, 5), new Vector(0, 0, 1));
            Assert.IsFalse(Plane.XY.TryIntersect(z5, out _));
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_CoincidentPlanes_ReturnsFalse_Test()
        {
            // Same surface, differently scaled coefficients — still parallel, so no single line.
            Assert.IsFalse(Plane.XY.TryIntersect(new Plane(0, 0, 5, 0), out _));
        }

        [TestMethod, TestCategory("PlaneIntersect")]
        public void TryIntersect_OppositelyOrientedSamePlane_ReturnsFalse_Test()
        {
            Assert.IsFalse(Plane.XY.TryIntersect(-Plane.XY, out _));
        }
    }
}
