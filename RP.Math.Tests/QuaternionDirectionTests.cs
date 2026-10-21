namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for <see cref="Quaternion.FromToRotation"/> and <see cref="Quaternion.LookRotation(Vector, Vector, OrthogonalAxes)"/>.
    /// </summary>
    [TestClass]
    public class QuaternionDirectionTests
    {
        private const double Tolerance = 1e-9;
        private const double LooseTolerance = 1e-6;

        #region FromToRotation

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_TurnsFromOntoTo_Test()
        {
            Quaternion q = Quaternion.FromToRotation(Vector.XAxis, Vector.YAxis);
            Assert.IsTrue(q.Rotate(Vector.XAxis).Equals(Vector.YAxis, LooseTolerance));
        }

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_OfAlignedVectors_IsIdentity_Test()
        {
            Quaternion q = Quaternion.FromToRotation(new Vector(2, 0, 0), new Vector(5, 0, 0));
            Assert.IsTrue(q.IsIdentity(Tolerance));
        }

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_OfOppositeVectors_IsAHalfTurn_Test()
        {
            Quaternion q = Quaternion.FromToRotation(Vector.XAxis, new Vector(-1, 0, 0));
            Assert.IsTrue(q.Rotate(Vector.XAxis).Equals(new Vector(-1, 0, 0), LooseTolerance));
        }

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_NormalizesInputs_Test()
        {
            // Non-unit inputs should behave like their directions.
            Quaternion q = Quaternion.FromToRotation(new Vector(0, 3, 0), new Vector(0, 0, 7));
            Assert.IsTrue(q.Rotate(Vector.YAxis).Equals(Vector.ZAxis, LooseTolerance));
        }

        [TestMethod, TestCategory("FromToRotation")]
        public void FromToRotation_OfZeroVector_IsIdentity_Test()
        {
            Assert.IsTrue(Quaternion.FromToRotation(Vector.Zero, Vector.XAxis).IsIdentity(Tolerance));
        }

        #endregion

        #region LookRotation

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_AlongTheConventionForward_IsIdentity_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.OpenGL;
            Quaternion q = Quaternion.LookRotation(axes.Forward, axes.Up, axes);
            Assert.IsTrue(q.IsIdentity(LooseTolerance));
        }

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_MapsTheConventionForwardOntoTheTarget_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.OpenGL;
            var target = new Vector(1, 2, 3).NormalizeOrDefault();

            Quaternion q = Quaternion.LookRotation(target, axes.Up, axes);

            Assert.IsTrue(q.Rotate(axes.Forward).Equals(target, LooseTolerance));
        }

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_KeepsUpPerpendicularToForwardAndOnTheUpSide_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.OpenGL;
            var forward = new Vector(1, 0, 1);
            var up = new Vector(0, 1, 0);

            Quaternion q = Quaternion.LookRotation(forward, up, axes);
            Vector resultUp = q.Rotate(axes.Up);

            // The resulting up is perpendicular to the forward direction...
            Assert.AreEqual(0, resultUp.DotProduct(forward.NormalizeOrDefault()), LooseTolerance);
            // ...and points to the same side as the requested up (positive component along it).
            Assert.IsTrue((resultUp.DotProduct(up)) > (0));
        }

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_WithUpParallelToForward_StillFacesTheTarget_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.OpenGL;
            var forward = new Vector(0, 1, 0);

            // up parallel to forward: the roll is unspecified, but it must still look along forward.
            Quaternion q = Quaternion.LookRotation(forward, new Vector(0, 2, 0), axes);

            Assert.IsTrue(q.Rotate(axes.Forward).Equals(forward.NormalizeOrDefault(), LooseTolerance));
        }

        [TestMethod, TestCategory("LookRotation")]
        public void LookRotation_SingleArgument_UsesTheConventionUp_Test()
        {
            OrthogonalAxes axes = OrthogonalAxes.DirectX;
            var forward = new Vector(1, 1, 1);

            Quaternion two = Quaternion.LookRotation(forward, axes.Up, axes);
            Quaternion one = Quaternion.LookRotation(forward, axes);

            Assert.IsTrue(one.Equals(two, LooseTolerance));
        }

        #endregion
    }
}
