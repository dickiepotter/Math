namespace RP.Math.Tests
{
    using System;

    using FluentAssertions;

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
            f.Tangent.Magnitude.Should().BeApproximately(1, 1e-6);
            f.Normal.Magnitude.Should().BeApproximately(1, 1e-6);
            f.Binormal.Magnitude.Should().BeApproximately(1, 1e-6);
            f.Tangent.DotProduct(f.Normal).Should().BeApproximately(0, 1e-6);
            f.Tangent.DotProduct(f.Binormal).Should().BeApproximately(0, 1e-6);
            f.Normal.DotProduct(f.Binormal).Should().BeApproximately(0, 1e-6);
            // Right-handed: B = T × N.
            f.Tangent.CrossProduct(f.Normal).Equals(f.Binormal, 1e-6).Should().BeTrue();
        }

        #region Bezier

        [TestMethod]
        public void Bezier_StraightLine_HasExactLengthAndZeroCurvature()
        {
            // Two control points = a straight line with uniform speed.
            var line = new Bezier(new Vector(0, 0, 0), new Vector(6, 8, 0)); // length 10
            line.Length().Should().BeApproximately(10, 1e-9);
            line.LengthBetween(0, 0.5).Should().BeApproximately(5, 1e-9);
            line.Acceleration(0.5).IsZero().Should().BeTrue();
            line.Curvature(0.5).Should().BeApproximately(0, Tol);

            // Uniform speed ⇒ arc-length parameter is linear.
            line.ParameterAtDistance(5).Should().BeApproximately(0.5, 1e-6);
            line.PointAtDistance(5).Equals(new Vector(3, 4, 0), 1e-6).Should().BeTrue();
        }

        [TestMethod]
        public void Bezier_Curved_HasPositiveCurvature_AndPlanarFrame()
        {
            var curve = new Bezier(new Vector(0, 0, 0), new Vector(1, 2, 0), new Vector(2, 0, 0));
            curve.Curvature(0.5).Should().BeGreaterThan(0);

            // The curve lives in the z = 0 plane, so the binormal must be ±Z.
            var f = curve.Frame(0.5);
            AssertOrthonormalRightHanded(f);
            System.Math.Abs(f.Binormal.Z).Should().BeApproximately(1, 1e-6);
        }

        [TestMethod]
        public void Bezier_Acceleration_QuadraticIsConstant()
        {
            // A degree-2 Bézier has a constant second derivative 2·(P0 − 2P1 + P2).
            var q = new Bezier(new Vector(0, 0, 0), new Vector(1, 1, 0), new Vector(2, 0, 0));
            var expected = 2 * (new Vector(0, 0, 0) - 2 * new Vector(1, 1, 0) + new Vector(2, 0, 0));
            q.Acceleration(0.1).Equals(expected, Tol).Should().BeTrue();
            q.Acceleration(0.9).Equals(expected, Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Bezier_Sample_BoundingBox_ClosestPoint()
        {
            var curve = new Bezier(new Vector(0, 0, 0), new Vector(1, 2, 0), new Vector(2, 0, 0));
            curve.Sample(8).Length.Should().Be(9); // count + 1

            var bb = curve.BoundingBox(64);
            bb.Contains(curve.PointAt(0.5)).Should().BeTrue();
            bb.Contains(curve.Start).Should().BeTrue();
            bb.Contains(curve.End).Should().BeTrue();

            // For a query above the arch, the closest point is the interior peak — strictly nearer than
            // either endpoint.
            var query = new Vector(1, 5, 0);
            var near = curve.ClosestPoint(query, out double t);
            t.Should().BeInRange(0, 1);
            near.DistanceSquared(query).Should().BeLessThan(curve.Start.DistanceSquared(query));
            near.DistanceSquared(query).Should().BeLessThan(curve.End.DistanceSquared(query));
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

            h.Length().Should().BeApproximately(12, 1e-9);
            h.Acceleration(0.5).IsZero().Should().BeTrue();
            h.Curvature(0.5).Should().BeApproximately(0, Tol);
            h.PointAt(0.5).Equals(new Vector(0, 0, 6), Tol).Should().BeTrue();
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
            spline.Length(256).Should().BeApproximately(10, 1e-4);
            spline.Curvature(0.5).Should().BeApproximately(0, 1e-6);
        }

        [TestMethod]
        public void CatmullRom_PassesThroughWaypoints_AndReparam()
        {
            var spline = new CatmullRom(new Vector(0, 0, 0), new Vector(4, 0, 0), new Vector(4, 4, 0));
            spline.PointAt(0).Equals(new Vector(0, 0, 0), Tol).Should().BeTrue();
            spline.PointAt(1).Equals(new Vector(4, 4, 0), Tol).Should().BeTrue();

            double len = spline.Length(512);
            // Halfway by distance should be roughly the middle waypoint region; just assert monotone bracket.
            double tMid = spline.ParameterAtDistance(len / 2, 512);
            tMid.Should().BeInRange(0, 1);
            spline.PointAtDistance(0, 512).Equals(spline.Start, 1e-6).Should().BeTrue();
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
