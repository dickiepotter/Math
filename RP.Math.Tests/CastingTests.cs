namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Covers the cast operators added across the library: the explicit re-encoding mesh between the
    /// convention-free orientation types (Quaternion / AxisAngle / Rotation / Matrix), the implicit
    /// aggregate-tuple casts on AxisAngle and Pose, and the explicit line-family widenings.
    /// </summary>
    [TestClass]
    public class CastingTests
    {
        private const double Tol = 1e-9;

        private static Quaternion SampleRotation()
        {
            // A representative non-trivial rotation (non-axis-aligned, well away from the gimbal poles).
            return Quaternion.FromAxisAngle(new Vector(1, 2, 3), new Angle(0.7));
        }

        // ---- Rotation-representation explicit mesh ---------------------------------------------

        [TestMethod, TestCategory("Casting")]
        public void RotationCasts_AgreeWithTheirNamedMethods()
        {
            var q = SampleRotation();
            var v = new Vector(1, 2, 3);

            Assert.IsTrue(((AxisAngle)q).Equals(AxisAngle.FromQuaternion(q), Tol));
            Assert.IsTrue(((Rotation)q).Equals(Rotation.FromQuaternion(q), Tol));
            Assert.IsTrue((((Matrix)q) * v).Equals(q.ToMatrix() * v, Tol));
        }

        [TestMethod, TestCategory("Casting")]
        public void RotationCasts_PreserveTheRotation_ThroughEveryType()
        {
            var q = SampleRotation();
            var v = new Vector(2, -1, 0.5);
            var expected = q.Rotate(v);

            // Round-trip out to each representation and back to a quaternion; the action must be identical.
            Assert.IsTrue(((Quaternion)(AxisAngle)q).Rotate(v).Equals(expected, Tol));
            Assert.IsTrue(((Quaternion)(Rotation)q).Rotate(v).Equals(expected, Tol));
            Assert.IsTrue(((Quaternion)(Matrix)q).Rotate(v).Equals(expected, Tol));
        }

        [TestMethod, TestCategory("Casting")]
        public void RotationCasts_ChainAcrossEveryType()
        {
            var q = SampleRotation();
            var v = new Vector(0, 3, -2);
            var expected = q.Rotate(v);

            // q -> AxisAngle -> Rotation -> Matrix -> Quaternion, all via casts.
            var aa = (AxisAngle)q;
            var rot = (Rotation)aa;
            var mat = (Matrix)rot;
            var back = (Quaternion)mat;

            Assert.IsTrue(back.Rotate(v).Equals(expected, Tol));
        }

        // ---- Aggregate tuple casts -------------------------------------------------------------

        [TestMethod, TestCategory("Casting")]
        public void AxisAngle_TupleCast_In_NormalisesTheAxis()
        {
            var rawAxis = new Vector(0, 0, 5); // deliberately non-unit
            var angle = new Angle(1.1);

            var a = (AxisAngle)(rawAxis, angle); // explicit in (normalises the axis)

            Assert.IsTrue(a.Axis.Equals(rawAxis.NormalizeOrDefault(), Tol), "the axis is normalised on construction");
            Assert.AreEqual(1.1, a.Angle.Rad, Tol);
        }

        [TestMethod, TestCategory("Casting")]
        public void AxisAngle_TupleCast_Out_ReadsTheFields()
        {
            var a = new AxisAngle(new Vector(0, 1, 0), new Angle(0.4));

            (Vector Axis, Angle Angle) t = a; // implicit out

            Assert.IsTrue(t.Axis.Equals(a.Axis, Tol));
            Assert.AreEqual(a.Angle.Rad, t.Angle.Rad, Tol);
        }

        [TestMethod, TestCategory("Casting")]
        public void Pose_TupleCast_RoundTrips()
        {
            var pos = new Vector(1, 2, 3);
            var q = SampleRotation();
            var probe = new Vector(0.5, -2, 1);

            var p = (Pose)(pos, q); // explicit in (normalises the orientation)
            Assert.AreEqual(pos, p.Position);
            Assert.IsTrue(p.Rotation.Rotate(probe).Equals(q.Rotate(probe), Tol));

            (Vector Position, Quaternion Orientation) t = p; // implicit out
            Assert.AreEqual(pos, t.Position);
            Assert.IsTrue(t.Orientation.Rotate(probe).Equals(q.Rotate(probe), Tol));
        }

        // ---- Line-family explicit casts --------------------------------------------------------

        [TestMethod, TestCategory("Casting")]
        public void LineSegment_CastsToLineAndRay_MatchTheNamedMethods()
        {
            var seg = new LineSegment(new Vector(1, 1, 1), new Vector(4, 1, 1));

            var line = (Line)seg;
            Assert.AreEqual(seg.ToLine().Point, line.Point);
            Assert.AreEqual(seg.ToLine().Direction, line.Direction);

            var ray = (Ray)seg;
            Assert.AreEqual(seg.ToRay().Origin, ray.Origin);
            Assert.AreEqual(seg.ToRay().Direction, ray.Direction);
        }

        [TestMethod, TestCategory("Casting")]
        public void Line_And_Ray_CastBetweenEachOther()
        {
            var line = new Line(new Vector(2, 0, 0), new Vector(1, 0, 0));
            var ray = new Ray(new Vector(2, 0, 0), new Vector(1, 0, 0));

            var asRay = (Ray)line;
            Assert.AreEqual(line.Point, asRay.Origin);
            Assert.AreEqual(line.Direction, asRay.Direction);

            var asLine = (Line)ray;
            Assert.AreEqual(ray.Origin, asLine.Point);
            Assert.AreEqual(ray.Direction, asLine.Direction);
        }
    }
}
