namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the positioned <see cref="PlacedTriangle"/> type.</summary>
    [TestClass]
    public class PlacedTriangleTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        // A 3-4-5 right triangle laid flat in the XY plane.
        private static PlacedTriangle Flat()
        {
            return PlacedTriangle.FromVertices(new Vector(0, 0, 0), new Vector(4, 0, 0), new Vector(0, 3, 0));
        }

        [TestMethod, TestCategory("Factory")]
        public void FromVertices_ShouldRoundTripTheCorners_Test()
        {
            var t = Flat();
            Assert.IsTrue(t.A.Equals(new Vector(0, 0, 0), LooseTol));
            Assert.IsTrue(t.B.Equals(new Vector(4, 0, 0), LooseTol));
            Assert.IsTrue(t.C.Equals(new Vector(0, 3, 0), LooseTol));
        }

        [TestMethod, TestCategory("Factory")]
        public void FromVertices_TiltedTriangle_ShouldRoundTripTheCorners_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(0, 1, 0);
            var c = new Vector(0, 0, 1);
            var t = PlacedTriangle.FromVertices(a, b, c);
            Assert.IsTrue(t.A.Equals(a, LooseTol));
            Assert.IsTrue(t.B.Equals(b, LooseTol));
            Assert.IsTrue(t.C.Equals(c, LooseTol));
        }

        [TestMethod, TestCategory("Placement")]
        public void Centroid_And_Normal_Test()
        {
            var t = Flat();
            Assert.IsTrue(t.Centroid.Equals(new Vector(4.0 / 3.0, 1, 0), LooseTol));
            Assert.IsTrue(t.Normal.Equals(new Vector(0, 0, 1), LooseTol));
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldDelegateToShape_Test()
        {
            var t = Flat();
            Assert.AreEqual(6, t.Area, LooseTol);
            Assert.AreEqual(5, t.SideA, LooseTol); // opposite A, |B-C|
            Assert.AreEqual(3, t.SideB, LooseTol); // opposite B, |C-A|
            Assert.AreEqual(4, t.SideC, LooseTol); // opposite C, |A-B|
        }

        [TestMethod, TestCategory("Containment")]
        public void Contains_ShouldRespectEdgesAndPlane_Test()
        {
            var t = Flat();
            Assert.IsTrue(t.Contains(t.Centroid, LooseTol));
            Assert.IsTrue(t.Contains(new Vector(1, 0.5, 0), LooseTol));
            Assert.IsFalse(t.Contains(new Vector(10, 10, 0), LooseTol));
            Assert.IsFalse(t.Contains(new Vector(1, 0.5, 0.5), LooseTol)); // off the plane
        }

        [TestMethod, TestCategory("Query")]
        public void ClosestPoint_OutsidePoint_ShouldClampToCorner_Test()
        {
            var t = Flat();
            Assert.IsTrue(t.ClosestPoint(new Vector(10, 0, 0)).Equals(new Vector(4, 0, 0), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_ThroughFace_ShouldHit_Test()
        {
            var t = Flat();
            var ray = new Ray(new Vector(1, 0.5, 5), new Vector(0, 0, -1));
            Assert.IsTrue(t.TryIntersect(ray, out Vector p));
            Assert.IsTrue(p.Equals(new Vector(1, 0.5, 0), LooseTol));
        }

        [TestMethod, TestCategory("Intersect")]
        public void TryIntersectRay_OutsideFace_ShouldMiss_Test()
        {
            var t = Flat();
            var ray = new Ray(new Vector(10, 10, 5), new Vector(0, 0, -1));
            Assert.IsFalse(t.TryIntersect(ray, out _));
        }

        [TestMethod, TestCategory("Construction")]
        public void ConstructFromShapeAndPose_ShouldPlaceCentroid_Test()
        {
            var t = new PlacedTriangle(Triangle.Equilateral(2), Pose.At(new Vector(5, 5, 5)));
            Assert.IsTrue(t.Centroid.Equals(new Vector(5, 5, 5), Tol));
            Assert.AreEqual(Triangle.Equilateral(2).Area, t.Area, Tol);
        }

        [TestMethod, TestCategory("Modification")]
        public void Translate_ShouldMoveCorners_Test()
        {
            var t = Flat().Translate(new Vector(0, 0, 10));
            Assert.IsTrue(t.A.Equals(new Vector(0, 0, 10), LooseTol));
        }
    }
}
