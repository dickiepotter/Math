namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>
    /// Tests for the cross-type interop: a <see cref="Vector"/> driven by the orientation/transform types,
    /// the <see cref="Matrix"/>↔<see cref="Quaternion"/> round trip, the <see cref="AxisAngle"/> bridges,
    /// and the line-family conversions.
    /// </summary>
    [TestClass]
    public class InteropTests
    {
        private const double Tol = 1e-9;
        private const double LooseTol = 1e-6;

        private static readonly Vector V = new Vector(1, 2, 3);

        #region Vector rotated by orientation types

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_RotateByQuaternion_ShouldMatchQuaternionRotate_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(0, 0, 1), new Angle(Math.PI / 2));
            Assert.IsTrue(V.Rotate(q).Equals(q.Rotate(V), LooseTol));
        }

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_RotateByAxisAngle_ShouldMatch_Test()
        {
            var aa = new AxisAngle(new Vector(0, 1, 0), new Angle(Math.PI / 3));
            Assert.IsTrue(V.Rotate(aa).Equals(aa.Rotate(V), LooseTol));
        }

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_RotateByRotationAndAttitude_ShouldMatch_Test()
        {
            var r = Rotation.AboutZ(new Angle(Math.PI / 2));
            Assert.IsTrue(V.Rotate(r).Equals(r.Rotate(V), LooseTol));

            var a = Attitude.FromYaw(new Angle(Math.PI / 4));
            Assert.IsTrue(V.Rotate(a, OrthogonalAxes.DirectX).Equals(a.Rotate(V, OrthogonalAxes.DirectX), LooseTol));
        }

        #endregion

        #region Vector transformed by matrix and pose

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_TransformByMatrix_ShouldEqualMatrixTimesVector_Test()
        {
            var m = Rotation.AboutX(new Angle(Math.PI / 2)).ToMatrix();
            Assert.IsTrue(V.Transform(m).Equals(m * V, LooseTol));
        }

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_TransformByPose_ShouldEqualPoseApply_Test()
        {
            var pose = new Pose(new Vector(10, 0, 0), Rotation.AboutZ(new Angle(Math.PI / 2)));
            Assert.IsTrue(V.Transform(pose).Equals(pose.Apply(V), LooseTol));
        }

        #endregion

        #region Vector angle as an Angle

        [TestMethod, TestCategory("VectorInterop")]
        public void Vector_AngleTo_ShouldMatchTheDoubleForm_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(0, 1, 0);
            Assert.AreEqual(a.Angle(b), a.AngleTo(b).Rad, Tol);
            Assert.AreEqual(Math.PI / 2, a.AngleTo(b).Rad, Tol);
        }

        #endregion

        #region Matrix <-> Quaternion round trip

        [TestMethod, TestCategory("MatrixInterop")]
        public void Matrix_ToQuaternion_ShouldInvertQuaternionToMatrix_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(1, 2, 3), new Angle(1.1));
            var back = q.ToMatrix().ToQuaternion();
            Assert.AreEqual(0, back.AngleTo(q).Rad, LooseTol);
        }

        [TestMethod, TestCategory("MatrixInterop")]
        public void Matrix_ToQuaternion_NearHalfTurn_ShouldStayStable_Test()
        {
            // A ~180° rotation drives the trace negative, exercising the pivot branches.
            var q = Quaternion.FromAxisAngle(new Vector(0, 1, 0), new Angle(Math.PI - 1e-3));
            var back = q.ToMatrix().ToQuaternion();
            Assert.AreEqual(0, back.AngleTo(q).Rad, LooseTol);
        }

        [TestMethod, TestCategory("MatrixInterop")]
        public void Matrix_ToQuaternion_OfIdentity_ShouldBeIdentity_Test()
        {
            Assert.IsTrue(Matrix.Identity.ToQuaternion().Equals(Quaternion.Identity, LooseTol));
        }

        #endregion

        #region AxisAngle bridges

        [TestMethod, TestCategory("Interop")]
        public void Quaternion_FromAxisAngleType_ShouldMatchVectorAngleForm_Test()
        {
            var aa = new AxisAngle(new Vector(0, 0, 1), new Angle(0.7));
            Assert.IsTrue(Quaternion.FromAxisAngle(aa).Equals(Quaternion.FromAxisAngle(aa.Axis, aa.Angle), Tol));
        }

        [TestMethod, TestCategory("Interop")]
        public void Pose_FromAxisAngle_ShouldRotateLikeTheAxisAngle_Test()
        {
            var aa = new AxisAngle(new Vector(0, 1, 0), new Angle(Math.PI / 2));
            var pose = new Pose(new Vector(0, 0, 0), aa);
            Assert.IsTrue(pose.Apply(V).Equals(aa.Rotate(V), LooseTol));
        }

        #endregion

        #region Line family conversions

        [TestMethod, TestCategory("LineFamily")]
        public void Line_ToRay_ShouldShareOriginAndDirection_Test()
        {
            var line = new Line(new Vector(1, 2, 3), new Vector(0, 0, 2));
            var ray = line.ToRay();
            Assert.IsTrue(ray.Origin.Equals(new Vector(1, 2, 3), Tol));
            Assert.IsTrue(ray.Direction.Equals(new Vector(0, 0, 1), Tol));
        }

        [TestMethod, TestCategory("LineFamily")]
        public void Ray_ToLine_ShouldShareOriginAndDirection_Test()
        {
            var ray = new Ray(new Vector(1, 2, 3), new Vector(0, 5, 0));
            var line = ray.ToLine();
            Assert.IsTrue(line.Point.Equals(new Vector(1, 2, 3), Tol));
            Assert.IsTrue(line.Direction.Equals(new Vector(0, 1, 0), Tol));
        }

        [TestMethod, TestCategory("LineFamily")]
        public void Segment_ToRay_ShouldStartAtTailTowardsHead_Test()
        {
            var seg = new LineSegment(new Vector(0, 0, 0), new Vector(0, 0, 4));
            var ray = seg.ToRay();
            Assert.IsTrue(ray.Origin.Equals(new Vector(0, 0, 0), Tol));
            Assert.IsTrue(ray.Direction.Equals(new Vector(0, 0, 1), Tol));
        }

        #endregion
    }
}
