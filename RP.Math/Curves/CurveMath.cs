namespace RP.Math
{
    using System;

    using Math = System.Math;

    /// <summary>
    /// Shared parametric-curve maths, written against a curve's <c>PointAt</c>/<c>Tangent</c>/<c>Acceleration</c>
    /// delegates so <see cref="Bezier"/>, <see cref="Hermite"/> and <see cref="CatmullRom"/> all gain the same
    /// arc-length, reparameterisation, frame, curvature and sampling behaviour from one implementation.
    /// </summary>
    /// <remarks>
    /// A parametric curve has no general closed-form arc length, so the length-based routines work by sampling
    /// the curve at evenly spaced parameters and summing straight chords — more samples give a closer (and
    /// always slightly short) estimate. The differential quantities (tangent direction, curvature, Frenet
    /// frame) are exact, built from the curve's own analytic derivatives.
    /// </remarks>
    internal static class CurveMath
    {
        /// <summary>The arc length between parameters <paramref name="t0"/> and <paramref name="t1"/>, by chord summation.</summary>
        public static double Length(Func<double, Vector> pointAt, double t0, double t1, int segments)
        {
            if (segments < 1) throw new ArgumentOutOfRangeException(nameof(segments), segments, SEGMENTS_POSITIVE);

            double total = 0;
            Vector previous = pointAt(t0);
            for (int i = 1; i <= segments; i++)
            {
                Vector current = pointAt(t0 + (t1 - t0) * ((double)i / segments));
                total += previous.Distance(current);
                previous = current;
            }

            return total;
        }

        /// <summary>
        /// The parameter <c>t</c> in [0, 1] at which the arc length measured from the start first reaches
        /// <paramref name="distance"/>. Built from a cumulative-length table over <paramref name="segments"/>
        /// samples with linear interpolation inside the bracketing sample; clamps to [0, 1].
        /// </summary>
        public static double ParameterAtDistance(Func<double, Vector> pointAt, double distance, int segments)
        {
            if (segments < 1) throw new ArgumentOutOfRangeException(nameof(segments), segments, SEGMENTS_POSITIVE);
            if (distance <= 0) return 0;

            double travelled = 0;
            Vector previous = pointAt(0);
            for (int i = 1; i <= segments; i++)
            {
                double t = (double)i / segments;
                Vector current = pointAt(t);
                double step = previous.Distance(current);

                if (travelled + step >= distance)
                {
                    // Linearly interpolate the parameter across this chord.
                    double tPrev = (double)(i - 1) / segments;
                    double fraction = step == 0 ? 0 : (distance - travelled) / step;
                    return tPrev + (t - tPrev) * fraction;
                }

                travelled += step;
                previous = current;
            }

            return 1; // distance beyond the curve's length
        }

        /// <summary>The unit tangent at <paramref name="t"/> (the curve's heading), or zero where the speed is zero.</summary>
        public static Vector TangentDirection(Func<double, Vector> tangent, double t)
        {
            return tangent(t).NormalizeOrDefault();
        }

        /// <summary>
        /// The signed-magnitude curvature κ = |v × a| / |v|³ at the given velocity <paramref name="v"/> and
        /// acceleration <paramref name="a"/> (zero where the curve is straight or stationary).
        /// </summary>
        public static double Curvature(Vector v, Vector a)
        {
            double speed = v.Magnitude;
            if (speed < 1e-12) return 0;
            return v.CrossProduct(a).Magnitude / (speed * speed * speed);
        }

        /// <summary>
        /// The Frenet frame from a velocity <paramref name="v"/> and acceleration <paramref name="a"/> at the
        /// given <paramref name="position"/>. Where the curve is straight (acceleration parallel to velocity,
        /// so the cross product vanishes) an arbitrary perpendicular normal is chosen so the frame is always
        /// well-defined and orthonormal.
        /// </summary>
        public static CurveFrame Frame(Vector position, Vector v, Vector a)
        {
            Vector t = v.NormalizeOrDefault();
            if (t.IsZero())
            {
                // No heading at all: fall back to the world axes so the frame is still orthonormal.
                return new CurveFrame(position, Vector.XAxis, Vector.YAxis, Vector.ZAxis);
            }

            Vector b = t.CrossProduct(a);
            if (b.IsZero(1e-12))
            {
                // Straight (or stationary acceleration): pick any normal perpendicular to the tangent.
                Vector reference = Math.Abs(t.DotProduct(Vector.YAxis)) > 0.99 ? Vector.XAxis : Vector.YAxis;
                Vector n0 = reference.Rejection(t).NormalizeOrDefault();
                Vector b0 = t.CrossProduct(n0).NormalizeOrDefault();
                return new CurveFrame(position, t, n0, b0);
            }

            b = b.NormalizeOrDefault();
            Vector n = b.CrossProduct(t).NormalizeOrDefault();
            return new CurveFrame(position, t, n, b);
        }

        /// <summary>Evenly sample <paramref name="count"/> + 1 points across [0, 1] (so the first and last points are the curve's ends).</summary>
        public static Vector[] Sample(Func<double, Vector> pointAt, int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), count, SEGMENTS_POSITIVE);

            var points = new Vector[count + 1];
            for (int i = 0; i <= count; i++)
            {
                points[i] = pointAt((double)i / count);
            }

            return points;
        }

        /// <summary>An axis-aligned bounding box around the curve, fitted to <paramref name="segments"/> + 1 samples (a close, slightly tight estimate).</summary>
        public static BoundingBox BoundingBox(Func<double, Vector> pointAt, int segments)
        {
            return RP.Math.BoundingBox.FromPoints(Sample(pointAt, segments));
        }

        /// <summary>
        /// The point on the curve closest to <paramref name="target"/>, found by sampling
        /// <paramref name="segments"/> + 1 points and then refining around the best one with a few bisection
        /// steps. <paramref name="t"/> receives the parameter of the returned point.
        /// </summary>
        public static Vector ClosestPoint(Func<double, Vector> pointAt, Vector target, int segments, out double t)
        {
            if (segments < 1) throw new ArgumentOutOfRangeException(nameof(segments), segments, SEGMENTS_POSITIVE);

            double bestT = 0;
            double bestSq = double.PositiveInfinity;
            for (int i = 0; i <= segments; i++)
            {
                double ti = (double)i / segments;
                double sq = pointAt(ti).DistanceSquared(target);
                if (sq < bestSq)
                {
                    bestSq = sq;
                    bestT = ti;
                }
            }

            // Refine within the neighbouring samples by repeated three-point bracketing.
            double span = 1.0 / segments;
            double lo = Math.Max(0, bestT - span);
            double hi = Math.Min(1, bestT + span);
            for (int iter = 0; iter < 24 && hi - lo > 1e-9; iter++)
            {
                double m1 = lo + (hi - lo) / 3;
                double m2 = hi - (hi - lo) / 3;
                if (pointAt(m1).DistanceSquared(target) < pointAt(m2).DistanceSquared(target))
                {
                    hi = m2;
                }
                else
                {
                    lo = m1;
                }
            }

            t = (lo + hi) / 2;
            return pointAt(t);
        }

        private const string SEGMENTS_POSITIVE = "The number of segments must be at least one.";
    }
}
