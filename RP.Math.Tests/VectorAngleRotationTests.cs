namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the convention-aware yaw / pitch / roll overloads on <see cref="Vector"/>. Under the
    /// DirectX convention (Up = +Y, Right = +X, Forward = +Z) they coincide with the explicit
    /// <see cref="Vector.RotateY(Angle)"/> / <see cref="Vector.RotateX(Angle)"/> / <see cref="Vector.RotateZ(Angle)"/>
    /// rotations, which pins their geometry without assuming a fixed axis mapping.
    /// </summary>
    [TestClass]
    public class VectorAngleRotationTests
    {
        private const double Tol = 1e-12;

        private static readonly Vector Sample = new Vector(2, 3, 5);
        private static readonly Angle Quarter = new Angle(90, AngleUnits.DEG);
        private static readonly OrthogonalAxes DirectX = OrthogonalAxes.DirectX;

        #region Convention overloads match the explicit axis rotations

        [TestMethod, TestCategory("Rotation")]
        public void Yaw_UnderDirectX_ShouldMatchRotateY_Test()
        {
            Assert.IsTrue(Sample.Yaw(Quarter, DirectX).Equals(Sample.RotateY(Quarter), Tol));
            Assert.IsTrue(Vector.Yaw(Sample, Quarter, DirectX).Equals(Vector.RotateY(Sample, Quarter), Tol));
        }

        [TestMethod, TestCategory("Rotation")]
        public void Pitch_UnderDirectX_ShouldMatchRotateX_Test()
        {
            Assert.IsTrue(Sample.Pitch(Quarter, DirectX).Equals(Sample.RotateX(Quarter), Tol));
            Assert.IsTrue(Vector.Pitch(Sample, Quarter, DirectX).Equals(Vector.RotateX(Sample, Quarter), Tol));
        }

        [TestMethod, TestCategory("Rotation")]
        public void Roll_UnderDirectX_ShouldMatchRotateZ_Test()
        {
            Assert.IsTrue(Sample.Roll(Quarter, DirectX).Equals(Sample.RotateZ(Quarter), Tol));
            Assert.IsTrue(Vector.Roll(Sample, Quarter, DirectX).Equals(Vector.RotateZ(Sample, Quarter), Tol));
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateXYZ_AngleOverloads_ShouldEqualRadianForms_Test()
        {
            Assert.IsTrue(Sample.RotateX(Quarter).Equals(Sample.RotateX(Quarter.Rad), Tol));
            Assert.IsTrue(Sample.RotateY(Quarter).Equals(Sample.RotateY(Quarter.Rad), Tol));
            Assert.IsTrue(Sample.RotateZ(Quarter).Equals(Sample.RotateZ(Quarter.Rad), Tol));

            Assert.IsTrue(Vector.RotateX(Sample, Quarter).Equals(Vector.RotateX(Sample, Quarter.Rad), Tol));
            Assert.IsTrue(Vector.RotateY(Sample, Quarter).Equals(Vector.RotateY(Sample, Quarter.Rad), Tol));
            Assert.IsTrue(Vector.RotateZ(Sample, Quarter).Equals(Vector.RotateZ(Sample, Quarter.Rad), Tol));
        }

        #endregion

        #region Behaviour

        [TestMethod, TestCategory("Rotation")]
        public void Roll_NinetyDegrees_UnderDirectX_ShouldTakeXAxisToYAxis_Test()
        {
            Assert.IsTrue(new Vector(1, 0, 0).Roll(new Angle(90, AngleUnits.DEG), DirectX)
                .Equals(new Vector(0, 1, 0), 1e-9));
        }

        [TestMethod, TestCategory("Rotation")]
        public void Yaw_FullTurn_ShouldReturnToStart_Test()
        {
            Assert.IsTrue(Sample.Yaw(new Angle(360, AngleUnits.DEG), DirectX).Equals(Sample, 1e-9));
        }

        #endregion
    }
}
