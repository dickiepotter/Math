using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RP.Math.Tests
{
    /// <summary>
    /// Covers the convention-aware yaw / pitch / roll overloads that rotate about an
    /// <see cref="OrthogonalAxes"/>'s own Up / Right / Forward, with no hard-coded axis assumption.
    /// </summary>
    [TestClass]
    public class ConventionRotationTests
    {
        private const double Tol = 1e-9;

        // ---- Vector.Yaw / Pitch / Roll ----------------------------------------------------------

        [TestMethod]
        public void Yaw_PreservesTheUpComponentAndMagnitude()
        {
            var v = new Vector(1, 2, 3);
            var axes = OrthogonalAxes.OpenGL;

            var r = v.Yaw(new Angle(System.Math.PI / 4), axes);

            // Rotating about Up cannot change how far along Up you are, nor the length.
            Assert.AreEqual(Vector.DotProduct(v, axes.Up), Vector.DotProduct(r, axes.Up), Tol);
            Assert.AreEqual(v.Magnitude, r.Magnitude, Tol);
        }

        [TestMethod]
        public void Pitch_PreservesTheRightComponent()
        {
            var v = new Vector(1, 2, 3);
            var axes = OrthogonalAxes.OpenGL;

            var r = v.Pitch(new Angle(0.6), axes);

            Assert.AreEqual(Vector.DotProduct(v, axes.Right), Vector.DotProduct(r, axes.Right), Tol);
        }

        [TestMethod]
        public void YawThenInverseYaw_ReturnsTheOriginal()
        {
            // Build the inverse from raw radians: Angle's unary - does not numerically negate.
            var v = new Vector(1, 2, 3);
            var axes = OrthogonalAxes.Unreal;

            var roundTrip = v.Yaw(new Angle(0.9), axes).Yaw(new Angle(-0.9), axes);

            Assert.IsTrue(roundTrip.Equals(v, Tol));
        }

        // ---- The whole point: handedness is carried by the convention, not chosen here ----------

        [TestMethod]
        public void Roll_TurnsOppositeWaysInRightAndLeftHandedFrames()
        {
            // OpenGL (right-handed) and Unity (left-handed) share Right and Up but have opposite
            // Forward. A roll therefore banks opposite ways — and rolling -a in one equals rolling +a
            // in the other, because rotation about +axis by a equals rotation about -axis by -a.
            var v = new Vector(1, 0, 0);
            double rad = System.Math.PI / 3;

            var openGl = v.Roll(new Angle(rad), OrthogonalAxes.OpenGL);
            var unity = v.Roll(new Angle(rad), OrthogonalAxes.Unity);

            Assert.IsFalse(openGl.Equals(unity, Tol), "the two frames disagree on which way is forward");
            Assert.IsTrue(openGl.Equals(v.Roll(new Angle(-rad), OrthogonalAxes.Unity), Tol));
        }

        [TestMethod]
        public void Roll_HonoursEachFramesForward_RelatingToLiteralRotateZ()
        {
            // The strength of the convention-aware Roll: it banks about the frame's OWN Forward, where
            // the literal RotateZ always turns about +Z. So the two agree exactly when a frame's forward
            // IS +Z, and correctly turn opposite ways when it is -Z — the convention-aware result is the
            // honest one, surfacing an assumption the fixed-axis primitive silently bakes in.
            var v = new Vector(1, 1, 0);
            double rad = System.Math.PI / 3;

            // DirectX / Unity: Forward = +Z, so Roll matches the literal RotateZ(+rad).
            Assert.IsTrue(v.Roll(new Angle(rad), OrthogonalAxes.DirectX)
                .Equals(v.RotateZ(rad), Tol), "DirectX forward is +Z");

            // OpenGL: Forward = -Z, so Roll(+rad) equals the literal RotateZ(-rad).
            Assert.IsTrue(v.Roll(new Angle(rad), OrthogonalAxes.OpenGL)
                .Equals(v.RotateZ(-rad), Tol), "OpenGL forward is -Z");
        }

        // ---- Quaternion.FromYawPitchRoll(..., axes) and Attitude --------------------------------

        [TestMethod]
        public void AttitudeRotate_MatchesQuaternionFromYawPitchRoll_InTheSameConvention()
        {
            var y = new Angle(0.3);
            var p = new Angle(-0.5);
            var r = new Angle(0.8);
            var axes = OrthogonalAxes.Blender;
            var v = new Vector(1, 2, 3);

            var viaAttitude = new Attitude(y, p, r).Rotate(v, axes);
            var viaQuaternion = Quaternion.FromYawPitchRoll(y, p, r, axes).Rotate(v);

            Assert.IsTrue(viaAttitude.Equals(viaQuaternion, Tol));
        }

        [TestMethod]
        public void FromYawPitchRoll_ComposesAboutTheConventionAxes()
        {
            var y = new Angle(0.4);
            var axes = OrthogonalAxes.OpenGL;
            var v = new Vector(2, 0, 1);

            // Yaw-only should match a single rotation about the convention's Up.
            var viaYpr = Quaternion.FromYawPitchRoll(y, new Angle(0), new Angle(0), axes).Rotate(v);
            var viaAxis = v.Yaw(y, axes);

            Assert.IsTrue(viaYpr.Equals(viaAxis, Tol));
        }

        // ---- LineSegment.Yaw / Pitch / Roll -----------------------------------------------------

        [TestMethod]
        public void LineSegmentYaw_KeepsTheTailFixedAndPreservesLength()
        {
            var seg = new LineSegment(new Vector(1, 1, 1), new Vector(3, 1, 1));

            var yawed = seg.Yaw(new Angle(0.7), OrthogonalAxes.OpenGL);

            Assert.AreEqual(seg.Tail, yawed.Tail);
            Assert.AreEqual(seg.Length, yawed.Length, Tol);
        }

        // ---- Attitude.FromQuaternion(q, axes): the convention-aware read-back ---------------------

        [TestMethod]
        public void AttitudeFromQuaternion_ShouldReproduceTheRotation()
        {
            // The strongest check: the recovered attitude must rotate vectors exactly like the original
            // quaternion, regardless of how the angles happen to be represented. OpenGL is right-handed.
            var q = Quaternion.FromYawPitchRoll(new Angle(0.5), new Angle(0.2), new Angle(-0.9), OrthogonalAxes.OpenGL);
            var att = Attitude.FromQuaternion(q, OrthogonalAxes.OpenGL);
            var v = new Vector(1, 2, 3);

            Assert.IsTrue(att.Rotate(v, OrthogonalAxes.OpenGL).Equals(q.Rotate(v), Tol));
        }

        [TestMethod]
        public void AttitudeFromQuaternion_ShouldInvertToQuaternion_InARightHandedConvention()
        {
            var att = new Attitude(new Angle(0.4), new Angle(-0.3), new Angle(0.8));
            var axes = OrthogonalAxes.Blender; // right-handed

            Assert.IsTrue(Attitude.FromQuaternion(att.ToQuaternion(axes), axes).Equals(att, Tol));
        }

        [TestMethod]
        public void AttitudeFromQuaternion_ShouldInvertToQuaternion_InALeftHandedConvention()
        {
            var att = new Attitude(new Angle(0.4), new Angle(-0.3), new Angle(0.8));
            var axes = OrthogonalAxes.DirectX; // left-handed

            Assert.IsTrue(Attitude.FromQuaternion(att.ToQuaternion(axes), axes).Equals(att, Tol));
        }
    }
}
