namespace RP.Math
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// An immutable <b>Catmull–Rom spline</b>: a smooth curve that passes <em>through</em> every one of a
    /// sequence of waypoints (unlike a <see cref="Bezier"/>, whose interior control points are not touched).
    /// This makes it the natural choice for routing a camera or object through a set of positions you have
    /// picked by hand.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The trick is that a Catmull–Rom spline is just a chain of <see cref="Hermite"/> segments with the
    /// tangents chosen for you: at each waypoint the curve heads in the direction of the
    /// <i>line through its two neighbours</i>, at half their separation —
    /// <c>tangent(Pᵢ) = (Pᵢ₊₁ − Pᵢ₋₁) / 2</c>. That single rule is what makes the joins between segments
    /// smooth without asking the caller for any tangents. At the two ends, which have only one neighbour,
    /// the missing neighbour is taken to be the endpoint itself.
    /// </para>
    /// <para>
    /// The whole spline is parameterised by a single <c>t</c> from 0 (the first waypoint) to 1 (the last);
    /// internally that is mapped onto the relevant segment and its local parameter. Built on
    /// <see cref="Vector"/> and <see cref="Hermite"/>, following the library's immutable design.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public class CatmullRom : IEquatable<CatmullRom>
    {
        #region Fields

        private readonly Vector[] points;

        #endregion

        #region Constructors

        /// <summary>Construct a Catmull–Rom spline through its ordered waypoints (at least two).</summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if fewer than two waypoints are supplied.</exception>
        public CatmullRom(params Vector[] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Length < 2) throw new ArgumentException(TOO_FEW, nameof(points));
            this.points = (Vector[])points.Clone();
        }

        #endregion

        #region Accessors

        /// <summary>The number of waypoints.</summary>
        public int Count { get { return this.points.Length; } }

        /// <summary>The number of curve segments (one between each adjacent pair of waypoints).</summary>
        public int SegmentCount { get { return this.points.Length - 1; } }

        /// <summary>The waypoint at <paramref name="index"/>.</summary>
        public Vector this[int index] { get { return this.points[index]; } }

        /// <summary>A copy of the waypoints, in order.</summary>
        public Vector[] Points { get { return (Vector[])this.points.Clone(); } }

        /// <summary>The start of the spline — the first waypoint, reached at <c>t = 0</c>.</summary>
        public Vector Start { get { return this.points[0]; } }

        /// <summary>The end of the spline — the last waypoint, reached at <c>t = 1</c>.</summary>
        public Vector End { get { return this.points[this.points.Length - 1]; } }

        #endregion

        #region Segments

        /// <summary>
        /// The <paramref name="index"/>-th segment as a <see cref="Hermite"/> curve (from waypoint
        /// <paramref name="index"/> to <paramref name="index"/>+1), with the Catmull–Rom tangents filled in.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is not a valid segment.</exception>
        public Hermite Segment(int index)
        {
            if (index < 0 || index >= this.SegmentCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, SEGMENT_RANGE);
            }

            Vector p0 = this.points[index];
            Vector p1 = this.points[index + 1];

            return new Hermite(p0, this.TangentAt(index), p1, this.TangentAt(index + 1));
        }

        /// <summary>
        /// The Catmull–Rom tangent at waypoint <paramref name="i"/>: half the vector from the previous
        /// waypoint to the next. At an end, the absent neighbour is taken to be the endpoint itself.
        /// </summary>
        private Vector TangentAt(int i)
        {
            Vector previous = this.points[Math.Max(0, i - 1)];
            Vector next = this.points[Math.Min(this.points.Length - 1, i + 1)];
            return (next - previous) * 0.5;
        }

        #endregion

        #region Evaluation

        /// <summary>
        /// The point on the spline at parameter <paramref name="t"/>, running 0 at the <see cref="Start"/>
        /// to 1 at the <see cref="End"/>. Values outside 0..1 are clamped to the ends.
        /// </summary>
        public Vector PointAt(double t)
        {
            ResolveSegment(t, out int index, out double local);
            return this.Segment(index).PointAt(local);
        }

        /// <summary>
        /// The tangent (velocity) on the spline at parameter <paramref name="t"/>. Note this is the velocity
        /// with respect to the whole-spline parameter, so it is not continuous in magnitude across joins
        /// (segments differ in length); its direction is continuous, which is the point of the spline.
        /// </summary>
        public Vector Tangent(double t)
        {
            ResolveSegment(t, out int index, out double local);
            return this.Segment(index).Tangent(local);
        }

        /// <summary>
        /// Map a whole-spline parameter <paramref name="t"/> (0..1, clamped) onto a segment index and a
        /// local parameter within that segment.
        /// </summary>
        private void ResolveSegment(double t, out int index, out double local)
        {
            if (t <= 0)
            {
                index = 0;
                local = 0;
                return;
            }

            if (t >= 1)
            {
                index = this.SegmentCount - 1;
                local = 1;
                return;
            }

            double scaled = t * this.SegmentCount; // 0 .. SegmentCount
            index = (int)Math.Floor(scaled);
            local = scaled - index;
        }

        /// <summary>
        /// The acceleration (<c>d²P/dt²</c>) on the spline at parameter <paramref name="t"/>, taken on the
        /// active segment. Like <see cref="Tangent(double)"/> it is expressed in the segment's local
        /// parameter, so its magnitude jumps across joins — but the curvature and frame built from it are
        /// invariant to that scaling and so vary smoothly.
        /// </summary>
        public Vector Acceleration(double t)
        {
            ResolveSegment(t, out int index, out double local);
            return this.Segment(index).Acceleration(local);
        }

        #endregion

        #region Frames, length and sampling

        /// <summary>The unit tangent (heading) at parameter <paramref name="t"/>.</summary>
        public Vector TangentDirection(double t) => CurveMath.TangentDirection(this.Tangent, t);

        /// <summary>The curvature κ at parameter <paramref name="t"/> (zero where the spline is straight).</summary>
        public double Curvature(double t) => CurveMath.Curvature(this.Tangent(t), this.Acceleration(t));

        /// <summary>The Frenet frame (position + tangent/normal/binormal) at parameter <paramref name="t"/>.</summary>
        public CurveFrame Frame(double t) => CurveMath.Frame(this.PointAt(t), this.Tangent(t), this.Acceleration(t));

        /// <summary>An approximation of the spline's arc length, summed over <paramref name="segments"/> even chords.</summary>
        public double Length(int segments) => CurveMath.Length(this.PointAt, 0, 1, segments);

        /// <summary>An approximation of the spline's arc length using a sensible default sample count.</summary>
        public double Length() => this.Length(64);

        /// <summary>The arc length between parameters <paramref name="t0"/> and <paramref name="t1"/>.</summary>
        public double LengthBetween(double t0, double t1, int segments = 64) => CurveMath.Length(this.PointAt, t0, t1, segments);

        /// <summary>The parameter at which the arc length from the start first reaches <paramref name="distance"/> (clamped to [0, 1]).</summary>
        public double ParameterAtDistance(double distance, int segments = 64) => CurveMath.ParameterAtDistance(this.PointAt, distance, segments);

        /// <summary>The point a given arc-length <paramref name="distance"/> along the spline from the start.</summary>
        public Vector PointAtDistance(double distance, int segments = 64) => this.PointAt(this.ParameterAtDistance(distance, segments));

        /// <summary>Evenly sample <paramref name="count"/> + 1 points along the spline.</summary>
        public Vector[] Sample(int count) => CurveMath.Sample(this.PointAt, count);

        /// <summary>An axis-aligned bounding box fitted to <paramref name="segments"/> + 1 samples of the spline.</summary>
        public BoundingBox BoundingBox(int segments = 64) => CurveMath.BoundingBox(this.PointAt, segments);

        /// <summary>The point on the spline closest to <paramref name="target"/> (sampled then refined).</summary>
        public Vector ClosestPoint(Vector target, int segments = 64) => CurveMath.ClosestPoint(this.PointAt, target, segments, out _);

        /// <summary>The point on the spline closest to <paramref name="target"/>, also reporting its parameter <paramref name="t"/>.</summary>
        public Vector ClosestPoint(Vector target, out double t, int segments = 64) => CurveMath.ClosestPoint(this.PointAt, target, segments, out t);

        #endregion

        #region Equality and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is CatmullRom c && this.Equals(c);
        }

        /// <summary>Equality with another spline (same waypoints, in order).</summary>
        public bool Equals(CatmullRom? other)
        {
            return other != null && this.points.SequenceEqual(other.points);
        }

        /// <summary>A hash code derived from the waypoints.</summary>
        public override int GetHashCode()
        {
            int hash = 17;
            foreach (Vector p in this.points)
            {
                hash = (hash * 31) ^ p.GetHashCode();
            }

            return hash;
        }

        /// <summary>A string of the form <c>CatmullRom[(p0); (p1); …]</c>.</summary>
        public override string ToString()
        {
            var sb = new StringBuilder("CatmullRom[");
            for (int i = 0; i < this.points.Length; i++)
            {
                if (i > 0) sb.Append("; ");
                sb.Append(this.points[i].ToString());
            }

            return sb.Append(']').ToString();
        }

        #endregion

        #region Messages

        private const string TOO_FEW = "A Catmull–Rom spline needs at least two waypoints.";
        private const string SEGMENT_RANGE = "Segment index must be between zero and SegmentCount − 1.";

        #endregion
    }
}
