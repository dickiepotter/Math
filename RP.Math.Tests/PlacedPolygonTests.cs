namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the vertex-defined <see cref="PlacedPolygon"/> type.</summary>
    [TestClass]
    public class PlacedPolygonTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        // A 2x2 square in the XY plane.
        private static PlacedPolygon Square()
        {
            return new PlacedPolygon(
                new Vector(0, 0, 0),
                new Vector(2, 0, 0),
                new Vector(2, 2, 0),
                new Vector(0, 2, 0));
        }

        [TestMethod, TestCategory("Construction")]
        public void TooFewVertices_ShouldThrow_Test()
        {
            Action act = () => new PlacedPolygon(new Vector(0, 0, 0), new Vector(1, 0, 0));
            Assert.ThrowsException<ArgumentException>(act);
        }

        [TestMethod, TestCategory("Measurement")]
        public void Area_Perimeter_Normal_Centroid_Test()
        {
            var s = Square();
            Assert.AreEqual(4, s.Area, Tol);
            Assert.AreEqual(8, s.Perimeter, Tol);
            Assert.IsTrue(s.Normal.Equals(new Vector(0, 0, 1), Tol));
            Assert.IsTrue(s.Centroid.Equals(new Vector(1, 1, 0), LooseTol));
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_Test()
        {
            var s = Square();
            Assert.IsTrue(s.Contains(new Vector(1, 1, 0), Tol));
            Assert.IsFalse(s.Contains(new Vector(3, 1, 0), Tol));
            Assert.IsFalse(s.Contains(new Vector(1, 1, 0.5), Tol)); // off the plane
        }

        [TestMethod, TestCategory("Containment")]
        public void Concave_Convexity_And_Containment_Test()
        {
            // A square with a triangular notch cut into the top edge down to (2, 2).
            var concave = new PlacedPolygon(
                new Vector(0, 0, 0),
                new Vector(4, 0, 0),
                new Vector(4, 4, 0),
                new Vector(2, 2, 0),
                new Vector(0, 4, 0));
            Assert.IsFalse(concave.IsConvex(Tol));
            Assert.IsTrue(concave.Contains(new Vector(2, 1, 0), Tol));  // below the notch
            Assert.IsFalse(concave.Contains(new Vector(2, 3, 0), Tol)); // inside the notch (outside the polygon)
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_OnEdge_Test()
        {
            Assert.IsTrue(Square().ClosestPoint(new Vector(3, 1, 0)).Equals(new Vector(2, 1, 0), Tol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersect_ThroughFace_Test()
        {
            var ray = new Ray(new Vector(1, 1, 5), new Vector(0, 0, -1));
            Assert.IsTrue(Square().TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(1, 1, 0), LooseTol));
        }

        [TestMethod, TestCategory("Transformation")]
        public void Transform_ShouldMoveVertices_Test()
        {
            var moved = Square().Translate(new Vector(0, 0, 5));
            Assert.IsTrue(moved[0].Equals(new Vector(0, 0, 5), Tol));
            Assert.AreEqual(4, moved.Area, Tol);
        }
    }
}
