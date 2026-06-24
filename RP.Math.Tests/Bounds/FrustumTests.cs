namespace RP.Math.Tests.Bounds
{
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RP.Math;

    /// <summary>
    /// Tests for <see cref="Frustum"/>. The frustum is built from a real view-projection: a camera at
    /// (0,0,5) looking down −Z, 60° vertical FOV, 1:1 aspect, near 1 / far 100. Points near the origin
    /// are inside; points well behind, beside, or beyond the far plane are outside.
    /// </summary>
    [TestClass]
    public sealed class FrustumTests
    {
        private static Frustum MakeFrustum()
        {
            Matrix view = Matrix.LookAt(new Vector(0, 0, 5), Vector.Origin, Vector.YAxis);
            Matrix proj = Matrix.PerspectiveFieldOfView(new Angle(60, AngleUnits.DEG), 1.0, 1.0, 100.0);
            return Frustum.FromViewProjection(proj * view);
        }

        [TestMethod]
        public void Contains_PointAtOrigin_IsInside()
        {
            MakeFrustum().Contains(Vector.Origin).Should().BeTrue();
        }

        [TestMethod]
        public void Contains_PointBehindCamera_IsOutside()
        {
            // The camera sits at z = 5 looking toward −z; z = 20 is well behind it.
            MakeFrustum().Contains(new Vector(0, 0, 20)).Should().BeFalse();
        }

        [TestMethod]
        public void Contains_PointBeyondFarPlane_IsOutside()
        {
            // Far plane is 100 units ahead of the camera (z = 5), i.e. around z = −95.
            MakeFrustum().Contains(new Vector(0, 0, -200)).Should().BeFalse();
        }

        [TestMethod]
        public void Contains_PointFarToTheSide_IsOutside()
        {
            MakeFrustum().Contains(new Vector(100, 0, 0)).Should().BeFalse();
        }

        [TestMethod]
        public void Intersects_SphereAtOrigin_IsVisible()
        {
            MakeFrustum().Intersects(new BoundingSphere(Vector.Origin, 1.0)).Should().BeTrue();
        }

        [TestMethod]
        public void Intersects_SphereFarToTheSide_IsCulled()
        {
            MakeFrustum().Intersects(new BoundingSphere(new Vector(100, 0, 0), 1.0)).Should().BeFalse();
        }

        [TestMethod]
        public void Intersects_SphereJustOutsideButWithinRadius_IsStillVisible()
        {
            // A sphere whose centre is a little outside the side, but whose radius reaches back in.
            var frustum = MakeFrustum();
            var bigSphere = new BoundingSphere(new Vector(3, 0, 0), 5.0);
            frustum.Intersects(bigSphere).Should().BeTrue();
        }

        [TestMethod]
        public void Intersects_BoxAtOrigin_IsVisible()
        {
            var box = Box.FromMinMax(new Vector(-1, -1, -1), new Vector(1, 1, 1));
            MakeFrustum().Intersects(box).Should().BeTrue();
        }

        [TestMethod]
        public void Intersects_BoxFarToTheSide_IsCulled()
        {
            var box = Box.FromMinMax(new Vector(90, -1, -1), new Vector(92, 1, 1));
            MakeFrustum().Intersects(box).Should().BeFalse();
        }
    }
}
