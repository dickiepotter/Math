namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for <see cref="Matrix.Inverse()"/> and <see cref="Matrix.IsInvertible"/>.</summary>
    [TestClass]
    public class MatrixInverseTests
    {
        private const double Tolerance = 1e-9;

        [TestMethod, TestCategory("Inverse")]
        public void Inverse_TimesOriginal_IsIdentity_Test()
        {
            // A composite transform: rotate, scale, translate — clearly invertible.
            Matrix m =
                Matrix.TranslationMatrix(new Vector(3, -4, 5))
                * Matrix.ScalingMatrix(2, 0.5, 3)
                * Matrix.RotationMatrixAboutZAxis(new Angle(System.Math.PI / 5));

            Assert.IsTrue((m * m.Inverse()).Equals(Matrix.Identity, Tolerance));
            Assert.IsTrue((m.Inverse() * m).Equals(Matrix.Identity, Tolerance));
        }

        [TestMethod, TestCategory("Inverse")]
        public void Inverse_OfScaling_IsReciprocalScaling_Test()
        {
            Matrix inv = Matrix.ScalingMatrix(2, 4, 5).Inverse();
            Assert.IsTrue(inv.Equals(Matrix.ScalingMatrix(0.5, 0.25, 0.2), Tolerance));
        }

        [TestMethod, TestCategory("Inverse")]
        public void Inverse_OfTranslation_NegatesTheOffset_Test()
        {
            Matrix inv = Matrix.TranslationMatrix(new Vector(1, 2, 3)).Inverse();
            Assert.IsTrue(inv.Equals(Matrix.TranslationMatrix(new Vector(-1, -2, -3)), Tolerance));
        }

        [TestMethod, TestCategory("Inverse")]
        public void Inverse_UndoesATransformAppliedToAVector_Test()
        {
            Matrix m = Matrix.TranslationMatrix(new Vector(10, 0, 0)) * Matrix.ScalingMatrix(2, 2, 2);
            var v = new Vector(1, 2, 3);
            Vector roundTrip = m.Inverse() * (m * v);
            Assert.IsTrue(roundTrip.Equals(v, Tolerance));
        }

        [TestMethod, TestCategory("Inverse")]
        public void IsInvertible_IsTrueForIdentity_FalseForSingular_Test()
        {
            Assert.IsTrue(Matrix.Identity.IsInvertible);
            Assert.IsFalse(Matrix.ScalingMatrix(1, 0, 1).IsInvertible); // collapses the y axis
        }

        [TestMethod, TestCategory("Inverse")]
        public void Inverse_OfSingularMatrix_Throws_Test()
        {
            Action act = () => Matrix.ScalingMatrix(1, 0, 1).Inverse();
            Assert.ThrowsException<InvalidOperationException>(act);
        }
    }
}
