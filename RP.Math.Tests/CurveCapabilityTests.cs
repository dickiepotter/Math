namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math;

    /// <summary>
    /// Tests for the shared curve additions (arc length, arc-length reparameterisation, Frenet frames,
    /// curvature, sampling, bounds, closest point) across <see cref="Bezier"/>, <see cref="Hermite"/> and
    /// <see cref="CatmullRom"/>. Ground truth comes from curves that are secretly straight lines (where the
    /// length and curvature are known exactly) and from frame orthonormality.
    /// </summary>
    [TestClass]
    public sealed class CurveCapabilityTests
    {
        private const double Tol = 1e-9;

        private static void AssertOrthonormalRightHanded(CurveFrame f)
        {
            Assert.AreEqual(1, f.Tangent.Magnitude, 1e-6);
            Assert.AreEqual(1, f.Normal.Magnitude, 1e-6);
            Assert.AreEqual(1, f.Binormal.Magnitude, 1e-6);
            Assert.AreEqual(0, f.Tangent.DotProduct(f.Normal), 1e-6);
            Assert.AreEqual(0, f.Tangent.DotProduct(f.Binormal), 1e-6);
            Assert.AreEqual(0, f.Normal.DotProduct(f.Binormal), 1e-6);
            // Right-handed: B = T × N.
            Assert.IsTrue(f.Tangent.CrossProduct(f.Normal).Equals(f.Binormal, 1e-6));
        }

        #region Bezier

        [TestMethod]
        public void Bezier_StraightLine_HasExactLengthAndZeroCurvature()
        {
            // Two control points = a straight line with uniform speed.
            var line = new Bezier(new Vector(0, 0, 0), new Vector(6, 8, 0)); // length 10
            Assert.AreEqual(10, line.Length(), 1e-9);
            Assert.AreEqual(5, line.LengthBetween(0, 0.5), 1e-9);
            Assert.IsTrue(line.Acceleration(0.5).IsZero());
            Assert.AreEqual(0, line.Curvature(0.5), Tol);

            // Uniform speed ⇒ arc-length parameter is linear.
            Assert.AreEqual(0.5, line.ParameterAtDistance(5), 1e-6);
            Assert.IsTrue(line.PointAtDistance(5).Equals(new Vector(3, 4, 0), 1e-6));
        }

        [TestMethod]
        public void Bezier_Curved_HasPositiveCurvature_AndPlanarFrame()
        {
            var curve = new Bezier(new Vector(0, 0, 0), new Vector(1, 2, 0), new Vector(2, 0, 0));
            Assert.IsTrue((curve.Curvature(0.5)) > (0));

            // The curve lives in the z = 0 plane, so the binormal must be ±Z.
            var f = curve.Frame(0.5);
            AssertOrthonormalRightHanded(f);
            Assert.AreEqual(1, System.Math.Abs(f.Binormal.Z), 1e-6);
        }

        [TestMethod]
        public void Bezier_Acceleration_QuadraticIsConstant()
        {
            // A degree-2 Bézier has a constant second derivative 2·(P0 − 2P1 + P2).
            var q = new Bezier(new Vector(0, 0, 0), new Vector(1, 1, 0), new Vector(2, 0, 0));
            var expected = 2 * (new Vector(0, 0, 0) - 2 * new Vector(1, 1, 0) + new Vector(2, 0, 0));
            Assert.IsTrue(q.Acceleration(0.1).Equals(expected, Tol));
            Assert.IsTrue(q.Acceleration(0.9).Equals(expected, Tol));
        }

        [TestMethod]
        public void Bezier_Sample_BoundingBox_ClosestPoint()
        {
            var curve = new Bezier(new Vector(0, 0, 0), new Vector(1, 2, 0), new Vector(2, 0, 0));
            Assert.AreEqual(9, curve.Sample(8).Length); // count + 1

            var bb = curve.BoundingBox(64);
            Assert.IsTrue(bb.Contains(curve.PointAt(0.5)));
            Assert.IsTrue(bb.Contains(curve.Start));
            Assert.IsTrue(bb.Contains(curve.End));

            // For a query above the arch, the closest point is the interior peak — strictly nearer than
            // either endpoint.
            var query = new Vector(1, 5, 0);
            var near = curve.ClosestPoint(query, out double t);
            Assert.IsTrue((t) >= (0) && (t) <= (1));
            Assert.IsTrue((near.DistanceSquared(query)) < (curve.Start.DistanceSquared(query)));
            Assert.IsTrue((near.DistanceSquared(query)) < (curve.End.DistanceSquared(query)));
        }

        #endregion

        #region Hermite

        [TestMethod]
        public void Hermite_ChordTangents_IsAStraightLine()
        {
            // A cubic Hermite whose end tangents equal the chord IS the straight line between the endpoints.
            var p0 = new Vector(0, 0, 0);
            var p1 = new Vector(0, 0, 12);
            var d = p1 - p0;
            var h = new Hermite(p0, d, p1, d);

            Assert.AreEqual(12, h.Length(), 1e-9);
            Assert.IsTrue(h.Acceleration(0.5).IsZero());
            Assert.AreEqual(0, h.Curvature(0.5), Tol);
            Assert.IsTrue(h.PointAt(0.5).Equals(new Vector(0, 0, 6), Tol));
        }

        [TestMethod]
        public void Hermite_Frame_IsOrthonormal_OnACurvedSegment()
        {
            var h = new Hermite(new Vector(0, 0, 0), new Vector(1, 1, 0), new Vector(2, 0, 0), new Vector(1, -1, 0));
            AssertOrthonormalRightHanded(h.Frame(0.5));
        }

        #endregion

        #region CatmullRom

        [TestMethod]
        public void CatmullRom_CollinearPoints_LengthIsTheChord()
        {
            var spline = new CatmullRom(new Vector(0, 0, 0), new Vector(0, 0, 5), new Vector(0, 0, 10));
            Assert.AreEqual(10, spline.Length(256), 1e-4);
            Assert.AreEqual(0, spline.Curvature(0.5), 1e-6);
        }

        [TestMethod]
        public void CatmullRom_PassesThroughWaypoints_AndReparam()
        {
            var spline = new CatmullRom(new Vector(0, 0, 0), new Vector(4, 0, 0), new Vector(4, 4, 0));
            Assert.IsTrue(spline.PointAt(0).Equals(new Vector(0, 0, 0), Tol));
            Assert.IsTrue(spline.PointAt(1).Equals(new Vector(4, 4, 0), Tol));

            double len = spline.Length(512);
            // Halfway by distance should be roughly the middle waypoint region; just assert monotone bracket.
            double tMid = spline.ParameterAtDistance(len / 2, 512);
            Assert.IsTrue((tMid) >= (0) && (tMid) <= (1));
            Assert.IsTrue(spline.PointAtDistance(0, 512).Equals(spline.Start, 1e-6));
        }

        [TestMethod]
        public void CatmullRom_Frame_IsOrthonormal()
        {
            var spline = new CatmullRom(new Vector(0, 0, 0), new Vector(1, 1, 0), new Vector(2, 0, 0), new Vector(3, 1, 0));
            AssertOrthonormalRightHanded(spline.Frame(0.4));
        }

        #endregion
    }
}
