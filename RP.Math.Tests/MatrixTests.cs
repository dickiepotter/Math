namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Matrix"/> type.
    /// NOTE: RP.Math is in an early state; some of these document the intended behaviour and may fail.
    /// </summary>
    [TestClass]
    public class MatrixTests
    {
        [TestMethod, TestCategory("Matrix")]
        public void Identity_ShouldHaveOnesOnTheDiagonal_Test()
        {
            var m = Matrix.Identity;
            for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
            {
                Assert.AreEqual(i == j ? 1d : 0d, m[i, j], "identity element [{0},{1}]", i, j);
            }
        }

        [TestMethod, TestCategory("Matrix")]
        public void Identity_TimesIdentity_ShouldBeIdentity_Test()
        {
            var product = Matrix.Identity * Matrix.Identity;
            Assert.IsTrue(product.IsIdentity);
        }

        [TestMethod, TestCategory("Matrix")]
        public void IdentityTimesVector_ShouldReturnSameVector_Test()
        {
            var v = new Vector(2, 3, 4);
            var result = Matrix.Identity * v;
            Assert.AreEqual(2, result.X);
            Assert.AreEqual(3, result.Y);
            Assert.AreEqual(4, result.Z);
        }

        [TestMethod, TestCategory("Matrix")]
        public void Transpose_OfIdentity_ShouldBeIdentity_Test()
        {
            Assert.IsTrue(Matrix.Identity.Transpose().IsIdentity);
        }

        [TestMethod, TestCategory("Matrix")]
        public void Transpose_ShouldSwapRowsAndColumns_Test()
        {
            var m = new Matrix(new double[,]
            {
                { 1, 2, 3, 4 },
                { 5, 6, 7, 8 },
                { 9, 10, 11, 12 },
                { 13, 14, 15, 16 },
            });

            var t = m.Transpose();

            for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
            {
                Assert.AreEqual(m[j, i], t[i, j]);
            }
        }

        [TestMethod, TestCategory("Matrix")]
        public void TranslationMatrix_ShouldTranslateAPoint_Test()
        {
            var translate = Matrix.TranslationMatrix(10, 20, 30);
            var moved = translate * new Vector(1, 2, 3);
            Assert.AreEqual(11, moved.X);
            Assert.AreEqual(22, moved.Y);
            Assert.AreEqual(33, moved.Z);
        }

        [TestMethod, TestCategory("Matrix")]
        public void Determinant_OfIdentity_ShouldBeOne_Test()
        {
            Assert.AreEqual(1d, Matrix.Identity.Determinant, 1e-9);
        }
    }
}
