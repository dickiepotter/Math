namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the vertex-defined <see cref="PlacedTetrahedron"/> type.</summary>
    [TestClass]
    public class PlacedTetrahedronTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        // The corner tetrahedron at the origin with unit legs along the axes.
        private static PlacedTetrahedron Unit()
        {
            return new PlacedTetrahedron(
                new Vector(0, 0, 0),
                new Vector(1, 0, 0),
                new Vector(0, 1, 0),
                new Vector(0, 0, 1));
        }

        [TestMethod, TestCategory("Measurement")]
        public void Volume_Centroid_SurfaceArea_Test()
        {
            var t = Unit();
            Assert.AreEqual(1.0 / 6.0, t.Volume, Tol);
            Assert.IsTrue(t.Centroid.Equals(new Vector(0.25, 0.25, 0.25), Tol));
            Assert.AreEqual(1.5 + (0.5 * Math.Sqrt(3)), t.SurfaceArea, LooseTol); // three legs + the slanted face
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_Test()
        {
            var t = Unit();
            Assert.IsTrue(t.Contains(t.Centroid, Tol));
            Assert.IsTrue(t.Contains(new Vector(0.1, 0.1, 0.1), Tol));
            Assert.IsFalse(t.Contains(new Vector(1, 1, 1), Tol));
            Assert.IsFalse(t.Contains(new Vector(-0.1, 0, 0), Tol));
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_ToVertexRegion_Test()
        {
            Assert.IsTrue(Unit().ClosestPoint(new Vector(-1, 0, 0)).Equals(new Vector(0, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersect_ThroughInterior_Test()
        {
            var t = Unit();
            var ray = new Ray(new Vector(0.25, 0.25, 5), new Vector(0, 0, -1));
            Assert.IsTrue(t.TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(0.25, 0.25, 0.5), LooseTol)); // enters through the slanted face x+y+z=1

            Assert.IsTrue(t.TryIntersect(new Line(new Vector(0.25, 0.25, 0), new Vector(0, 0, 1)), out Vector near, out Vector far));
            Assert.IsTrue(near.Equals(new Vector(0.25, 0.25, 0), LooseTol));
            Assert.IsTrue(far.Equals(new Vector(0.25, 0.25, 0.5), LooseTol));
        }

        [TestMethod, TestCategory("Transformation")]
        public void Transform_ShouldPreserveVolume_Test()
        {
            var moved = Unit().Transform(Pose.At(new Vector(10, 10, 10)));
            Assert.AreEqual(1.0 / 6.0, moved.Volume, LooseTol);
            Assert.IsTrue(moved.Centroid.Equals(new Vector(10.25, 10.25, 10.25), LooseTol));
        }
    }
}
