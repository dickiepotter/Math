namespace RP.Math
{
    using System;

    /// <summary>
    /// An immutable <b>cubic Hermite</b> curve segment: the smooth curve between two endpoints that not
    /// only passes through them but also leaves and arrives along two given <em>tangents</em> (velocities).
    /// Where a <see cref="Bezier"/> is shaped by pulling on off-curve control points, a Hermite segment is
    /// shaped by stating the endpoints and the direction-and-speed the curve has at each — which is exactly
    /// the information needed to join segments together smoothly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Maths: the point at parameter <c>t</c> (0 at the start, 1 at the end) is a blend of the two
    /// endpoints and two tangents, weighted by the four cubic <em>Hermite basis functions</em>:
    /// </para>
    /// <code>
    /// h00(t) =  2t³ − 3t² + 1     (weight on the start point)
    /// h10(t) =   t³ − 2t² + t     (weight on the start tangent)
    /// h01(t) = −2t³ + 3t²         (weight on the end point)
    /// h11(t) =   t³ −  t²         (weight on the end tangent)
    /// P(t)   = h00·P0 + h10·M0 + h01·P1 + h11·M1
    /// </code>
    /// <para>
    /// You can check the ends by hand: at <c>t = 0</c> only <c>h00</c> is 1 so <c>P(0) = P0</c>, and at
    /// <c>t = 1</c> only <c>h01</c> is 1 so <c>P(1) = P1</c>; differentiating shows the start and end
    /// velocities are exactly <c>M0</c> and <c>M1</c>. This segment is the building block of
    /// <see cref="CatmullRom"/>, which simply computes the tangents for you.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct Hermite : IEquatable<Hermite>
    {
        #region Fields

        private readonly Vector startPoint;
        private readonly Vector startTangent;
        private readonly Vector endPoint;
        private readonly Vector endTangent;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a cubic Hermite segment from a start point and tangent and an end point and tangent.
        /// </summary>
        /// <param name="startPoint">The point reached at <c>t = 0</c>.</param>
        /// <param name="startTangent">The velocity (direction and speed) the curve leaves the start with.</param>
        /// <param name="endPoint">The point reached at <c>t = 1</c>.</param>
        /// <param name="endTangent">The velocity the curve arrives at the end with.</param>
        public Hermite(Vector startPoint, Vector startTangent, Vector endPoint, Vector endTangent)
        {
            this.startPoint = startPoint;
            this.startTangent = startTangent;
            this.endPoint = endPoint;
            this.endTangent = endTangent;
        }

        #endregion

        #region Accessors

        /// <summary>The point reached at <c>t = 0</c>.</summary>
        public Vector StartPoint { get { return this.startPoint; } }

        /// <summary>The velocity the curve leaves the start with.</summary>
        public Vector StartTangent { get { return this.startTangent; } }

        /// <summary>The point reached at <c>t = 1</c>.</summary>
        public Vector EndPoint { get { return this.endPoint; } }

        /// <summary>The velocity the curve arrives at the end with.</summary>
        public Vector EndTangent { get { return this.endTangent; } }

        #endregion

        #region Evaluation

        /// <summary>The point on the segment at parameter <paramref name="t"/> (0 at the start, 1 at the end).</summary>
        public Vector PointAt(double t)
        {
            double t2 = t * t;
            double t3 = t2 * t;

            double h00 = (2 * t3) - (3 * t2) + 1;
            double h10 = t3 - (2 * t2) + t;
            double h01 = (-2 * t3) + (3 * t2);
            double h11 = t3 - t2;

            return (h00 * this.startPoint)
                 + (h10 * this.startTangent)
                 + (h01 * this.endPoint)
                 + (h11 * this.endTangent);
        }

        /// <summary>
        /// The tangent (velocity, <c>dP/dt</c>) at parameter <paramref name="t"/>, found by differentiating
        /// the four basis functions. At <c>t = 0</c> it is <see cref="StartTangent"/>; at <c>t = 1</c>,
        /// <see cref="EndTangent"/>.
        /// </summary>
        public Vector Tangent(double t)
        {
            double t2 = t * t;

            double h00 = (6 * t2) - (6 * t);
            double h10 = (3 * t2) - (4 * t) + 1;
            double h01 = (-6 * t2) + (6 * t);
            double h11 = (3 * t2) - (2 * t);

            return (h00 * this.startPoint)
                 + (h10 * this.startTangent)
                 + (h01 * this.endPoint)
                 + (h11 * this.endTangent);
        }

        /// <summary>
        /// The acceleration (<c>d²P/dt²</c>) at parameter <paramref name="t"/>, the exact second derivative of
        /// the cubic basis: <c>(12t−6)P0 + (6t−4)M0 + (−12t+6)P1 + (6t−2)M1</c>.
        /// </summary>
        public Vector Acceleration(double t)
        {
            double h00 = (12 * t) - 6;
            double h10 = (6 * t) - 4;
            double h01 = (-12 * t) + 6;
            double h11 = (6 * t) - 2;

            return (h00 * this.startPoint)
                 + (h10 * this.startTangent)
                 + (h01 * this.endPoint)
                 + (h11 * this.endTangent);
        }

        #endregion

        #region Frames, length and sampling

        /// <summary>The unit tangent (heading) at parameter <paramref name="t"/>.</summary>
        public Vector TangentDirection(double t) => CurveMath.TangentDirection(this.Tangent, t);

        /// <summary>The curvature κ at parameter <paramref name="t"/> (zero where the segment is straight).</summary>
        public double Curvature(double t) => CurveMath.Curvature(this.Tangent(t), this.Acceleration(t));

        /// <summary>The Frenet frame (position + tangent/normal/binormal) at parameter <paramref name="t"/>.</summary>
        public CurveFrame Frame(double t) => CurveMath.Frame(this.PointAt(t), this.Tangent(t), this.Acceleration(t));

        /// <summary>An approximation of the segment's arc length, summed over <paramref name="segments"/> even chords.</summary>
        public double Length(int segments) => CurveMath.Length(this.PointAt, 0, 1, segments);

        /// <summary>An approximation of the segment's arc length using a sensible default sample count.</summary>
        public double Length() => this.Length(64);

        /// <summary>The arc length between parameters <paramref name="t0"/> and <paramref name="t1"/>.</summary>
        public double LengthBetween(double t0, double t1, int segments = 64) => CurveMath.Length(this.PointAt, t0, t1, segments);

        /// <summary>The parameter at which the arc length from the start first reaches <paramref name="distance"/> (clamped to [0, 1]).</summary>
        public double ParameterAtDistance(double distance, int segments = 64) => CurveMath.ParameterAtDistance(this.PointAt, distance, segments);

        /// <summary>The point a given arc-length <paramref name="distance"/> along the segment from the start.</summary>
        public Vector PointAtDistance(double distance, int segments = 64) => this.PointAt(this.ParameterAtDistance(distance, segments));

        /// <summary>Evenly sample <paramref name="count"/> + 1 points along the segment.</summary>
        public Vector[] Sample(int count) => CurveMath.Sample(this.PointAt, count);

        /// <summary>An axis-aligned bounding box fitted to <paramref name="segments"/> + 1 samples of the segment.</summary>
        public BoundingBox BoundingBox(int segments = 64) => CurveMath.BoundingBox(this.PointAt, segments);

        /// <summary>The point on the segment closest to <paramref name="target"/> (sampled then refined).</summary>
        public Vector ClosestPoint(Vector target, int segments = 64) => CurveMath.ClosestPoint(this.PointAt, target, segments, out _);

        /// <summary>The point on the segment closest to <paramref name="target"/>, also reporting its parameter <paramref name="t"/>.</summary>
        public Vector ClosestPoint(Vector target, out double t, int segments = 64) => CurveMath.ClosestPoint(this.PointAt, target, segments, out t);

        #endregion

        #region Operators

        /// <summary>Component-wise equality of endpoints and tangents.</summary>
        public static bool operator ==(Hermite h1, Hermite h2)
        {
            return h1.startPoint == h2.startPoint
                && h1.startTangent == h2.startTangent
                && h1.endPoint == h2.endPoint
                && h1.endTangent == h2.endTangent;
        }

        /// <summary>Component-wise inequality.</summary>
        public static bool operator !=(Hermite h1, Hermite h2)
        {
            return !(h1 == h2);
        }

        #endregion

        #region Equality and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is Hermite h && this.Equals(h);
        }

        /// <summary>Equality with another Hermite segment.</summary>
        public bool Equals(Hermite other)
        {
            return this == other;
        }

        /// <summary>A hash code derived from the endpoints and tangents.</summary>
        public override int GetHashCode()
        {
            return this.startPoint.GetHashCode()
                ^ this.startTangent.GetHashCode()
                ^ this.endPoint.GetHashCode()
                ^ this.endTangent.GetHashCode();
        }

        /// <summary>A string of the form <c>Hermite[P0→P1, tangents M0, M1]</c>.</summary>
        public override string ToString()
        {
            return string.Format(
                "Hermite[{0}→{1}, tangents {2}, {3}]",
                this.startPoint,
                this.endPoint,
                this.startTangent,
                this.endTangent);
        }

        #endregion
    }
}
