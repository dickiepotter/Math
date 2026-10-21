namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Rotation"/> (Euler X/Y/Z) and <see cref="Attitude"/> (yaw/pitch/roll) types.
    /// </summary>
    [TestClass]
    public class RotationTests
    {
        private const double Tol = 1e-6;

        private static Angle Deg(double d) => new Angle(d, AngleUnits.DEG);

        #region Rotation construction and accessors

        [TestMethod, TestCategory("Construction")]
        public void Construction_ShouldExposeComponentAngles_Test()
        {
            var r = new Rotation(Deg(10), Deg(20), Deg(30));
            Assert.AreEqual(10, r.X.Deg, Tol);
            Assert.AreEqual(20, r.Y.Deg, Tol);
            Assert.AreEqual(30, r.Z.Deg, Tol);
        }

        [TestMethod, TestCategory("Construction")]
        public void Zero_ShouldHaveAllZeroAngles_Test()
        {
            Assert.AreEqual(0, Rotation.Zero.X.Rad);
            Assert.AreEqual(0, Rotation.Zero.Y.Rad);
            Assert.AreEqual(0, Rotation.Zero.Z.Rad);
        }

        #endregion

        #region Rotation application

        [TestMethod, TestCategory("Rotation")]
        public void AboutZ_NinetyDegrees_ShouldTakeXToY_Test()
        {
            var r = Rotation.AboutZ(Deg(90));
            Assert.IsTrue(r.Rotate(new Vector(1, 0, 0)).Equals(new Vector(0, 1, 0), Tol));
        }

        [TestMethod, TestCategory("Rotation")]
        public void AboutX_NinetyDegrees_ShouldTakeYToZ_Test()
        {
            var r = Rotation.AboutX(Deg(90));
            Assert.IsTrue(r.Rotate(new Vector(0, 1, 0)).Equals(new Vector(0, 0, 1), Tol));
        }

        [TestMethod, TestCategory("Rotation")]
        public void Rotate_ShouldPreserveMagnitude_Test()
        {
            var r = new Rotation(Deg(15), Deg(40), Deg(75));
            var v = new Vector(3, -2, 5);
            Assert.AreEqual(v.Magnitude, r.Rotate(v).Magnitude, Tol);
        }

        #endregion

        #region Quaternion / matrix conversion

        [TestMethod, TestCategory("Conversion")]
        public void ToQuaternion_ShouldBeUnit_Test()
        {
            Assert.IsTrue(new Rotation(Deg(15), Deg(40), Deg(75)).ToQuaternion().IsUnit(Tol));
        }

        [TestMethod, TestCategory("Conversion")]
        public void FromQuaternion_ShouldRoundTrip_ForNonSingularAngles_Test()
        {
            var original = new Rotation(Deg(15), Deg(40), Deg(75));
            var roundTripped = Rotation.FromQuaternion(original.ToQuaternion());
            Assert.IsTrue(roundTripped.Equals(original, Tol));
        }

        [TestMethod, TestCategory("Conversion")]
        public void ToMatrix_TimesVector_ShouldMatchRotate_Test()
        {
            var r = new Rotation(Deg(20), Deg(35), Deg(50));
            var v = new Vector(1, 2, 3);
            Assert.IsTrue((r.ToMatrix() * v).Equals(r.Rotate(v), Tol));
        }

        [TestMethod, TestCategory("Conversion")]
        public void Inverse_ThenRotate_ShouldUndoRotation_Test()
        {
            var r = new Rotation(Deg(20), Deg(35), Deg(50));
            var v = new Vector(1, 2, 3);
            Assert.IsTrue(r.Inverse().Rotate(r.Rotate(v)).Equals(v, Tol));
        }

        #endregion

        #region Equality, formatting, deconstruction

        [TestMethod, TestCategory("Equality")]
        public void Equality_WithTolerance_ShouldIgnoreTinyDifferences_Test()
        {
            Assert.IsTrue(new Rotation(Deg(10), Deg(20), Deg(30))
                .Equals(new Rotation(Deg(10), Deg(20), Deg(30.0000001)), Tol));
        }

        [TestMethod, TestCategory("Conversion")]
        public void Deconstruct_ShouldYieldComponentAngles_Test()
        {
            var (x, y, z) = new Rotation(Deg(10), Deg(20), Deg(30));
            Assert.AreEqual(10, x.Deg, Tol);
            Assert.AreEqual(20, y.Deg, Tol);
            Assert.AreEqual(30, z.Deg, Tol);
        }

        #endregion
    }
}
