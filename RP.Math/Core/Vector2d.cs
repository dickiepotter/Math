namespace RP.Math
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    using Exceptions;

    using Math = System.Math;

    /// <summary>
    /// A double-precision vector with two components (x, y) — the planar sibling of <see cref="Vector3d"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This completes the double-precision vector family (<see cref="Vector2d"/>, <see cref="Vector3d"/>,
    /// <c>Vector4d</c>) alongside the single-precision family (<see cref="Vector2"/>, <see cref="Vector3"/>,
    /// <see cref="Vector4"/>). It mirrors the completionist surface of <see cref="Vector3d"/> — operators,
    /// products, normalisation, interpolation, projection/rejection/reflection, component-wise maths,
    /// rounding, tolerance-aware equality/comparison and the same numeric edge-case handling — restricted to
    /// the operations that make sense in the plane.
    /// </para>
    /// <para>
    /// In two dimensions the cross product is the scalar <c>z</c> of the 3-D cross (the "perp-dot"); it is
    /// the signed area of the parallelogram spanned by the two vectors, positive when the turn from the first
    /// to the second is counter-clockwise. Rotation is a single in-plane angle rather than a choice of axis.
    /// </para>
    /// <para>
    /// As with the rest of RP.Math this is an immutable value type: every operation returns a new vector.
    /// Method names follow <see cref="Vector3d"/> (<see cref="DotProduct(Vector2d)"/>,
    /// <see cref="Interpolate(Vector2d, double)"/>, …); the terser single-precision names
    /// (<see cref="Dot(Vector2d)"/>, <see cref="Lerp(Vector2d, Vector2d, double)"/>, <see cref="Length"/>, …)
    /// are provided as aliases so one convention reads across the whole library.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [ImmutableObject(true), Serializable]
    public struct Vector2d
        : IComparable, IComparable<Vector2d>, IEquatable<Vector2d>, IFormattable
    {
        #region Class Variables

        /// <summary>The X component of the vector.</summary>
        private readonly double x;

        /// <summary>The Y component of the vector.</summary>
        private readonly double y;

        #endregion

        #region Constructors

        /// <summary>Construct a vector from its two components.</summary>
        public Vector2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>Construct a vector from a two-element array.</summary>
        /// <exception cref="ArgumentException">Thrown if the array does not contain exactly two components.</exception>
        public Vector2d(double[] xy)
        {
            if (xy.Length == 2)
            {
                this.x = xy[0];
                this.y = xy[1];
            }
            else
            {
                throw new ArgumentException(TWO_COMPONENTS);
            }
        }

        /// <summary>Construct a copy of another vector.</summary>
        public Vector2d(Vector2d v1)
        {
            this.x = v1.X;
            this.y = v1.Y;
        }

        /// <summary>Construct a double vector from the single-precision <see cref="Vector2"/>.</summary>
        public Vector2d(Vector2 v1)
        {
            this.x = v1.X;
            this.y = v1.Y;
        }

        #endregion

        #region Accessors & Mutators

        /// <summary>Get the x component of the vector.</summary>
        public double X { get { return this.x; } }

        /// <summary>Get the y component of the vector.</summary>
        public double Y { get { return this.y; } }

        /// <summary>Gets the magnitude (aka. length or absolute value) of the vector.</summary>
        public double Magnitude { get { return Math.Sqrt(this.SumComponentSqrs()); } }

        /// <summary>
        /// The square of the vector's magnitude (an alias of <see cref="SumComponentSqrs()"/>), avoiding a
        /// square root where only relative magnitudes are needed.
        /// </summary>
        public double MagnitudeSquared { get { return this.SumComponentSqrs(); } }

        /// <summary>The vector's length — an alias of <see cref="Magnitude"/> matching the single-precision family.</summary>
        public double Length { get { return this.Magnitude; } }

        /// <summary>The vector's squared length — an alias of <see cref="MagnitudeSquared"/>.</summary>
        public double LengthSquared { get { return this.MagnitudeSquared; } }

        /// <summary>Gets the vector as an array.</summary>
        [XmlIgnore]
        public double[] Array { get { return new[] { this.x, this.y }; } }

        /// <summary>An index accessor mapping [0] -&gt; X and [1] -&gt; Y.</summary>
        /// <exception cref="ArgumentException">Thrown if the index is not 0 or 1.</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return this.X;
                    case 1: return this.Y;
                    default: throw new ArgumentException(TWO_COMPONENTS, "index");
                }
            }
        }

        #endregion

        #region Operators

        /// <summary>Addition of two vectors.</summary>
        public static Vector2d operator +(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.X + v2.X, v1.Y + v2.Y);
        }

        /// <summary>Subtraction of two vectors.</summary>
        public static Vector2d operator -(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.X - v2.X, v1.Y - v2.Y);
        }

        /// <summary>Scalar multiplication (vector on the left).</summary>
        public static Vector2d operator *(Vector2d v1, double s2)
        {
            return new Vector2d(v1.X * s2, v1.Y * s2);
        }

        /// <summary>Scalar multiplication (scalar on the left).</summary>
        public static Vector2d operator *(double s1, Vector2d v2)
        {
            return v2 * s1;
        }

        /// <summary>Component-wise (Hadamard) product — handy for non-uniform scaling.</summary>
        public static Vector2d operator *(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.X * v2.X, v1.Y * v2.Y);
        }

        /// <summary>Scalar division.</summary>
        public static Vector2d operator /(Vector2d v1, double s2)
        {
            return new Vector2d(v1.X / s2, v1.Y / s2);
        }

        /// <summary>Negation — reverses direction.</summary>
        public static Vector2d operator -(Vector2d v1)
        {
            return new Vector2d(-v1.X, -v1.Y);
        }

        /// <summary>Unary plus — returns the vector unchanged.</summary>
        public static Vector2d operator +(Vector2d v1)
        {
            return new Vector2d(+v1.X, +v1.Y);
        }

        /// <summary>
        /// Less-than by magnitude. Comparing two vectors has no geometric meaning; this compares magnitudes
        /// for convenience (see <see cref="CompareTo(Vector2d)"/>).
        /// </summary>
        public static bool operator <(Vector2d v1, Vector2d v2)
        {
            return v1.SumComponentSqrs() < v2.SumComponentSqrs();
        }

        /// <summary>Greater-than by magnitude.</summary>
        public static bool operator >(Vector2d v1, Vector2d v2)
        {
            return v1.SumComponentSqrs() > v2.SumComponentSqrs();
        }

        /// <summary>Less-than-or-equal by magnitude.</summary>
        public static bool operator <=(Vector2d v1, Vector2d v2)
        {
            return v1.SumComponentSqrs() <= v2.SumComponentSqrs();
        }

        /// <summary>Greater-than-or-equal by magnitude.</summary>
        public static bool operator >=(Vector2d v1, Vector2d v2)
        {
            return v1.SumComponentSqrs() >= v2.SumComponentSqrs();
        }

        /// <summary>Exact component equality.</summary>
        public static bool operator ==(Vector2d v1, Vector2d v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        /// <summary>Exact component inequality.</summary>
        public static bool operator !=(Vector2d v1, Vector2d v2)
        {
            return !(v1 == v2);
        }

        #endregion

        #region Magnitude operation

        /// <summary>Scale a vector to the given magnitude, preserving direction.</summary>
        public static Vector2d Scale(Vector2d vector, double magnitude)
        {
            if (magnitude < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(magnitude), magnitude, NEGATIVE_MAGNITUDE);
            }

            if (vector == Origin)
            {
                throw new ArgumentException(ORIGIN_VECTOR_MAGNITUDE, nameof(vector));
            }

            return vector * (magnitude / vector.Magnitude);
        }

        /// <summary>Scale this vector to the given magnitude, preserving direction.</summary>
        public Vector2d Scale(double magnitude)
        {
            return Scale(this, magnitude);
        }

        #endregion

        #region Product Operations

        /// <summary>The dot product: <c>|a||b|cosθ</c>. Zero when perpendicular; its sign tells you which side.</summary>
        public static double DotProduct(Vector2d v1, Vector2d v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        /// <summary>The dot product of this vector with another.</summary>
        public double DotProduct(Vector2d other)
        {
            return DotProduct(this, other);
        }

        /// <summary>The dot product — terse alias of <see cref="DotProduct(Vector2d, Vector2d)"/>.</summary>
        public static double Dot(Vector2d v1, Vector2d v2) => DotProduct(v1, v2);

        /// <summary>The dot product — terse alias of <see cref="DotProduct(Vector2d)"/>.</summary>
        public double Dot(Vector2d other) => DotProduct(this, other);

        /// <summary>
        /// The 2-D cross product (the "perp-dot"): the scalar <c>x1·y2 − y1·x2</c>. It equals the z component
        /// of the 3-D cross of the two vectors in the xy-plane, i.e. the signed area of the parallelogram they
        /// span — positive when the turn from <paramref name="v1"/> to <paramref name="v2"/> is
        /// counter-clockwise, zero when they are parallel.
        /// </summary>
        public static double CrossProduct(Vector2d v1, Vector2d v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        /// <summary>The 2-D cross product (perp-dot) of this vector with another.</summary>
        public double CrossProduct(Vector2d other)
        {
            return CrossProduct(this, other);
        }

        /// <summary>The 2-D cross product — terse alias of <see cref="CrossProduct(Vector2d, Vector2d)"/>.</summary>
        public static double Cross(Vector2d v1, Vector2d v2) => CrossProduct(v1, v2);

        /// <summary>The 2-D cross product — terse alias of <see cref="CrossProduct(Vector2d)"/>.</summary>
        public double Cross(Vector2d other) => CrossProduct(this, other);

        /// <summary>
        /// The vector rotated a quarter-turn counter-clockwise: <c>(−y, x)</c>. Its dot with the original is
        /// zero, so it is the canonical perpendicular in the plane.
        /// </summary>
        public Vector2d Perpendicular()
        {
            return new Vector2d(-this.y, this.x);
        }

        /// <summary>The vector rotated a quarter-turn clockwise: <c>(y, −x)</c>.</summary>
        public Vector2d PerpendicularCW()
        {
            return new Vector2d(this.y, -this.x);
        }

        #endregion

        #region Normalize Operations

        /// <summary>
        /// Get the normalized unit vector with a magnitude of one.
        /// </summary>
        /// <exception cref="NormalizeVectorException">
        /// Thrown when the vector has a magnitude of zero, NaN, or an un-normalizable infinite magnitude.
        /// </exception>
        public static Vector2d Normalize(Vector2d v1)
        {
            // Special Cases
            if (double.IsInfinity(v1.Magnitude))
            {
                v1 = NormalizeSpecialCasesOrOriginal(v1);

                if (v1.IsNaN())
                {
                    throw new NormalizeVectorException(NORMALIZE_Inf);
                }
            }

            if (v1.Magnitude == 0)
            {
                throw new NormalizeVectorException(NORMALIZE_0);
            }

            if (v1.IsNaN())
            {
                throw new NormalizeVectorException(NORMALIZE_NaN);
            }

            return NormalizeOrNaN(v1);
        }

        /// <summary>
        /// Get the normalized unit vector with a magnitude of one, returning the origin for a zero magnitude
        /// and (NaN, NaN) for a NaN magnitude rather than throwing.
        /// </summary>
        public static Vector2d NormalizeOrDefault(Vector2d v1)
        {
            v1 = NormalizeSpecialCasesOrOriginal(v1);

            if (v1.Magnitude == 0)
            {
                return Origin;
            }

            if (v1.IsNaN())
            {
                return NaN;
            }

            return NormalizeOrNaN(v1);
        }

        /// <summary>Get the normalized unit vector with a magnitude of one.</summary>
        /// <exception cref="NormalizeVectorException">
        /// Thrown when the vector has a magnitude of zero, NaN, or an un-normalizable infinite magnitude.
        /// </exception>
        public Vector2d Normalize()
        {
            return Normalize(this);
        }

        /// <summary>Get the normalized unit vector, falling back to the origin / NaN as <see cref="NormalizeOrDefault(Vector2d)"/>.</summary>
        public Vector2d NormalizeOrDefault()
        {
            return NormalizeOrDefault(this);
        }

        private static Vector2d NormalizeOrNaN(Vector2d v1)
        {
            double inverse = 1 / v1.Magnitude;
            return new Vector2d(v1.X * inverse, v1.Y * inverse);
        }

        private static Vector2d NormalizeSpecialCasesOrOriginal(Vector2d v1)
        {
            if (double.IsInfinity(v1.Magnitude))
            {
                var x = v1.X == 0 ? 0 : double.IsPositiveInfinity(v1.X) ? 1 : double.IsNegativeInfinity(v1.X) ? -1 : double.NaN;
                var y = v1.Y == 0 ? 0 : double.IsPositiveInfinity(v1.Y) ? 1 : double.IsNegativeInfinity(v1.Y) ? -1 : double.NaN;

                return new Vector2d(x, y);
            }

            return v1;
        }

        #endregion

        #region Interpolation Operations

        /// <summary>
        /// Take an interpolated value from between two vectors, or an extrapolated value if allowed.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the control is not between 0 and 1 and extrapolation is not allowed.
        /// </exception>
        public static Vector2d Interpolate(Vector2d v1, Vector2d v2, double control, bool allowExtrapolation)
        {
            if (!allowExtrapolation && (control > 1 || control < 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(control),
                    control,
                    INTERPOLATION_RANGE + "\n" + ARGUMENT_VALUE + control);
            }

            return new Vector2d(
                v1.X * (1 - control) + v2.X * control,
                v1.Y * (1 - control) + v2.Y * control);
        }

        /// <summary>Take an interpolated value from between two vectors (control in [0, 1]).</summary>
        public static Vector2d Interpolate(Vector2d v1, Vector2d v2, double control)
        {
            return Interpolate(v1, v2, control, false);
        }

        /// <summary>Take an interpolated value between this vector and another (control in [0, 1]).</summary>
        public Vector2d Interpolate(Vector2d other, double control)
        {
            return Interpolate(this, other, control);
        }

        /// <summary>Take an interpolated, or extrapolated, value between this vector and another.</summary>
        public Vector2d Interpolate(Vector2d other, double control, bool allowExtrapolation)
        {
            return Interpolate(this, other, control, allowExtrapolation);
        }

        /// <summary>Linear interpolation — terse alias of <see cref="Interpolate(Vector2d, Vector2d, double, bool)"/> allowing extrapolation.</summary>
        public static Vector2d Lerp(Vector2d v1, Vector2d v2, double control) => Interpolate(v1, v2, control, true);

        /// <summary>Linear interpolation — terse alias allowing extrapolation.</summary>
        public Vector2d Lerp(Vector2d other, double control) => Interpolate(this, other, control, true);

        /// <summary>
        /// Spherically interpolate between two vectors: the direction follows the shortest arc while the
        /// magnitude is blended linearly. Falls back to linear interpolation when the vectors are
        /// (anti)parallel or either is zero.
        /// </summary>
        public static Vector2d Slerp(Vector2d v1, Vector2d v2, double control)
        {
            var n1 = v1.NormalizeOrDefault();
            var n2 = v2.NormalizeOrDefault();

            double dot = n1.DotProduct(n2);
            if (dot > 1) { dot = 1; }
            else if (dot < -1) { dot = -1; }

            double theta = Math.Acos(dot);
            double sinTheta = Math.Sin(theta);

            if (sinTheta < 1e-9)
            {
                return Interpolate(v1, v2, control, true);
            }

            double a = Math.Sin((1 - control) * theta) / sinTheta;
            double b = Math.Sin(control * theta) / sinTheta;
            return (a * v1) + (b * v2);
        }

        /// <summary>Spherically interpolate between this vector and another.</summary>
        public Vector2d Slerp(Vector2d other, double control) => Slerp(this, other, control);

        #endregion

        #region Distance Operations

        /// <summary>Find the distance between two vectors (Pythagoras).</summary>
        public static double Distance(Vector2d v1, Vector2d v2)
        {
            return Math.Sqrt(
                (v1.X - v2.X) * (v1.X - v2.X) +
                (v1.Y - v2.Y) * (v1.Y - v2.Y));
        }

        /// <summary>Find the distance between this vector and another.</summary>
        public double Distance(Vector2d other)
        {
            return Distance(this, other);
        }

        /// <summary>The squared distance between two vectors — cheaper when only comparing distances.</summary>
        public static double DistanceSquared(Vector2d v1, Vector2d v2)
        {
            return (v1.X - v2.X) * (v1.X - v2.X) + (v1.Y - v2.Y) * (v1.Y - v2.Y);
        }

        /// <summary>The squared distance between this vector and another.</summary>
        public double DistanceSquared(Vector2d other) => DistanceSquared(this, other);

        #endregion

        #region Angle Operations

        /// <summary>
        /// The unsigned angle between two vectors, in radians, in [0, π]. Uses the numerically stable
        /// <c>atan2(|cross|, dot)</c> form so it never returns NaN for (anti)parallel inputs.
        /// </summary>
        public static double Angle(Vector2d v1, Vector2d v2)
        {
            if (v1 == v2)
            {
                return 0;
            }

            var u1 = NormalizeOrDefault(v1);
            var u2 = NormalizeOrDefault(v2);

            return Math.Atan2(Math.Abs(CrossProduct(u1, u2)), u1.DotProduct(u2));
        }

        /// <summary>The unsigned angle between this vector and another, in radians, in [0, π].</summary>
        public double Angle(Vector2d other)
        {
            return Angle(this, other);
        }

        /// <summary>
        /// The signed angle, in radians, in (−π, π], measured from <paramref name="v1"/> to
        /// <paramref name="v2"/>. Positive is counter-clockwise. Uses <c>atan2(cross, dot)</c>.
        /// </summary>
        public static double SignedAngle(Vector2d v1, Vector2d v2)
        {
            var u1 = NormalizeOrDefault(v1);
            var u2 = NormalizeOrDefault(v2);

            return Math.Atan2(CrossProduct(u1, u2), u1.DotProduct(u2));
        }

        /// <summary>The signed angle from this vector to another, in radians, in (−π, π] (positive counter-clockwise).</summary>
        public double SignedAngle(Vector2d other) => SignedAngle(this, other);

        /// <summary>The unsigned angle between this vector and another as an <see cref="RP.Math.Angle"/>.</summary>
        public Angle AngleTo(Vector2d other) => new Angle(Angle(this, other));

        /// <summary>The unsigned angle between two vectors as an <see cref="RP.Math.Angle"/>.</summary>
        public static Angle AngleBetween(Vector2d v1, Vector2d v2) => new Angle(Angle(v1, v2));

        #endregion

        #region Rotation Operations

        /// <summary>
        /// Rotate a vector in the plane by <paramref name="radians"/> counter-clockwise (a positive angle
        /// turns +X toward +Y).
        /// </summary>
        public static Vector2d Rotate(Vector2d v1, double radians)
        {
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            return new Vector2d(
                v1.X * cos - v1.Y * sin,
                v1.X * sin + v1.Y * cos);
        }

        /// <summary>Rotate this vector in the plane by <paramref name="radians"/> counter-clockwise.</summary>
        public Vector2d Rotate(double radians)
        {
            return Rotate(this, radians);
        }

        /// <summary>Rotate this vector in the plane by an <see cref="RP.Math.Angle"/> counter-clockwise.</summary>
        public Vector2d Rotate(Angle angle)
        {
            return Rotate(this, angle.Rad);
        }

        #endregion

        #region Projection, Rejection and Reflection Operations

        /// <summary>The vector resolute of <paramref name="v1"/> in the direction of <paramref name="v2"/> (its "shadow").</summary>
        public static Vector2d Projection(Vector2d v1, Vector2d v2)
        {
            return v2 * (v1.DotProduct(v2) / Math.Pow(v2.Magnitude, 2));
        }

        /// <summary>The vector resolute of this vector in the given <paramref name="direction"/>.</summary>
        public Vector2d Projection(Vector2d direction)
        {
            return Projection(this, direction);
        }

        /// <summary>Vector projection — terse alias of <see cref="Projection(Vector2d, Vector2d)"/>.</summary>
        public static Vector2d Project(Vector2d v1, Vector2d v2) => Projection(v1, v2);

        /// <summary>Vector projection — terse alias of <see cref="Projection(Vector2d)"/>.</summary>
        public Vector2d Project(Vector2d direction) => Projection(this, direction);

        /// <summary>The component of <paramref name="v1"/> perpendicular to <paramref name="v2"/> (so projection + rejection = original).</summary>
        public static Vector2d Rejection(Vector2d v1, Vector2d v2)
        {
            return v1 - v1.Projection(v2);
        }

        /// <summary>The component of this vector perpendicular to the given <paramref name="direction"/>.</summary>
        public Vector2d Rejection(Vector2d direction)
        {
            return Rejection(this, direction);
        }

        /// <summary>Vector rejection — terse alias of <see cref="Rejection(Vector2d, Vector2d)"/>.</summary>
        public static Vector2d Reject(Vector2d v1, Vector2d v2) => Rejection(v1, v2);

        /// <summary>Vector rejection — terse alias of <see cref="Rejection(Vector2d)"/>.</summary>
        public Vector2d Reject(Vector2d direction) => Rejection(this, direction);

        /// <summary>
        /// Reflect <paramref name="v1"/> <i>about</i> the line through <paramref name="v2"/> (mirroring the
        /// vector across that direction), preserving <paramref name="v1"/>'s magnitude.
        /// </summary>
        public static Vector2d Reflection(Vector2d v1, Vector2d v2)
        {
            // If v2 is at a right angle to v1, the mirror image is the reverse vector.
            if (Math.Abs(Math.Abs(v1.Angle(v2)) - Math.PI / 2) < double.Epsilon)
            {
                return -v1;
            }

            Vector2d retval = 2 * v1.Projection(v2) - v1;
            return retval.Scale(v1.Magnitude);
        }

        /// <summary>Reflect this vector about the line through <paramref name="reflector"/>.</summary>
        public Vector2d Reflection(Vector2d reflector)
        {
            return Reflection(this, reflector);
        }

        /// <summary>
        /// Reflect a vector off a surface with the given <paramref name="normal"/> (the classic "bounce":
        /// angle of incidence equals angle of reflection). The normal should be unit length.
        /// </summary>
        public static Vector2d Reflect(Vector2d vector, Vector2d normal)
        {
            return vector - 2 * vector.DotProduct(normal) * normal;
        }

        /// <summary>Reflect this vector off a surface with the given <paramref name="normal"/>.</summary>
        public Vector2d Reflect(Vector2d normal)
        {
            return Reflect(this, normal);
        }

        #endregion

        #region Componentwise Min, Max and Clamp

        /// <summary>The component-wise minimum of two vectors (one corner of their bounding box).</summary>
        public static Vector2d ComponentMin(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y));
        }

        /// <summary>The component-wise minimum of this vector and another.</summary>
        public Vector2d ComponentMin(Vector2d other) => ComponentMin(this, other);

        /// <summary>The component-wise maximum of two vectors (the opposite corner of their bounding box).</summary>
        public static Vector2d ComponentMax(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y));
        }

        /// <summary>The component-wise maximum of this vector and another.</summary>
        public Vector2d ComponentMax(Vector2d other) => ComponentMax(this, other);

        /// <summary>Clamp each component into the box defined by <paramref name="min"/> and <paramref name="max"/>.</summary>
        public static Vector2d Clamp(Vector2d value, Vector2d min, Vector2d max)
        {
            return new Vector2d(
                value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
                value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y);
        }

        /// <summary>Clamp each component of this vector into the box defined by <paramref name="min"/> and <paramref name="max"/>.</summary>
        public Vector2d Clamp(Vector2d min, Vector2d max) => Clamp(this, min, max);

        #endregion

        #region Min, Max, ClampMagnitude and MoveTowards

        /// <summary>Compare the magnitude of two vectors and return the greater.</summary>
        public static Vector2d Max(Vector2d v1, Vector2d v2)
        {
            return v1 >= v2 ? v1 : v2;
        }

        /// <summary>Compare the magnitude of this vector and another, returning the greater.</summary>
        public Vector2d Max(Vector2d other) => Max(this, other);

        /// <summary>Compare the magnitude of two vectors and return the lesser.</summary>
        public static Vector2d Min(Vector2d v1, Vector2d v2)
        {
            return v1 <= v2 ? v1 : v2;
        }

        /// <summary>Compare the magnitude of this vector and another, returning the lesser.</summary>
        public Vector2d Min(Vector2d other) => Min(this, other);

        /// <summary>Cap the vector's magnitude at <paramref name="maxMagnitude"/> while keeping its direction.</summary>
        public static Vector2d ClampMagnitude(Vector2d vector, double maxMagnitude)
        {
            double lengthSq = vector.SumComponentSqrs();
            if (lengthSq <= maxMagnitude * maxMagnitude)
            {
                return vector;
            }

            return vector / Math.Sqrt(lengthSq) * maxMagnitude;
        }

        /// <summary>Cap this vector's magnitude at <paramref name="maxMagnitude"/> while keeping its direction.</summary>
        public Vector2d ClampMagnitude(double maxMagnitude) => ClampMagnitude(this, maxMagnitude);

        /// <summary>
        /// Move <paramref name="current"/> toward <paramref name="target"/> by at most
        /// <paramref name="maxDistanceDelta"/>, never overshooting.
        /// </summary>
        public static Vector2d MoveTowards(Vector2d current, Vector2d target, double maxDistanceDelta)
        {
            Vector2d delta = target - current;
            double dist = delta.Magnitude;
            if (dist <= maxDistanceDelta || dist == 0)
            {
                return target;
            }

            return current + delta / dist * maxDistanceDelta;
        }

        /// <summary>Move this vector toward <paramref name="target"/> by at most <paramref name="maxDistanceDelta"/>.</summary>
        public Vector2d MoveTowards(Vector2d target, double maxDistanceDelta) => MoveTowards(this, target, maxDistanceDelta);

        #endregion

        #region Component Operations

        /// <summary>The sum of the components.</summary>
        public static double SumComponents(Vector2d v1)
        {
            return v1.X + v1.Y;
        }

        /// <summary>The sum of this vector's components.</summary>
        public double SumComponents() => SumComponents(this);

        /// <summary>The sum of the squares of the components.</summary>
        public static double SumComponentSqrs(Vector2d v1)
        {
            return v1.X * v1.X + v1.Y * v1.Y;
        }

        /// <summary>The sum of the squares of this vector's components.</summary>
        public double SumComponentSqrs() => SumComponentSqrs(this);

        /// <summary>Raise each component to <paramref name="power"/>.</summary>
        public static Vector2d PowComponents(Vector2d v1, double power)
        {
            return new Vector2d(Math.Pow(v1.X, power), Math.Pow(v1.Y, power));
        }

        /// <summary>Raise each of this vector's components to <paramref name="power"/>.</summary>
        public Vector2d PowComponents(double power) => PowComponents(this, power);

        /// <summary>The square root of each component.</summary>
        public static Vector2d SqrtComponents(Vector2d v1)
        {
            return new Vector2d(Math.Sqrt(v1.X), Math.Sqrt(v1.Y));
        }

        /// <summary>The square root of each of this vector's components.</summary>
        public Vector2d SqrtComponents() => SqrtComponents(this);

        /// <summary>The square of each component.</summary>
        public static Vector2d SqrComponents(Vector2d v1)
        {
            return new Vector2d(v1.X * v1.X, v1.Y * v1.Y);
        }

        /// <summary>The square of each of this vector's components.</summary>
        public Vector2d SqrComponents() => SqrComponents(this);

        /// <summary>The absolute value of each component.</summary>
        public static Vector2d AbsComponents(Vector2d v1)
        {
            return new Vector2d(Math.Abs(v1.X), Math.Abs(v1.Y));
        }

        /// <summary>The absolute value of each of this vector's components.</summary>
        public Vector2d AbsComponents() => AbsComponents(this);

        /// <summary>The absolute value of each component — alias of <see cref="AbsComponents()"/> matching the single-precision family.</summary>
        public Vector2d Abs() => AbsComponents(this);

        #endregion

        #region Round Components

        /// <summary>Round each component to the nearest integral value.</summary>
        public static Vector2d Round(Vector2d v1)
        {
            return new Vector2d(Math.Round(v1.X), Math.Round(v1.Y));
        }

        /// <summary>Round each component to the given number of fractional <paramref name="digits"/>.</summary>
        public static Vector2d Round(Vector2d v1, int digits)
        {
            return new Vector2d(Math.Round(v1.X, digits), Math.Round(v1.Y, digits));
        }

        /// <summary>Round each component to the nearest integral value, using the given midpoint <paramref name="mode"/>.</summary>
        public static Vector2d Round(Vector2d v1, MidpointRounding mode)
        {
            return new Vector2d(Math.Round(v1.X, mode), Math.Round(v1.Y, mode));
        }

        /// <summary>Round each component to the given <paramref name="digits"/>, using the given midpoint <paramref name="mode"/>.</summary>
        public static Vector2d Round(Vector2d v1, int digits, MidpointRounding mode)
        {
            return new Vector2d(Math.Round(v1.X, digits, mode), Math.Round(v1.Y, digits, mode));
        }

        /// <summary>Round each component to the nearest integral value.</summary>
        public Vector2d Round() => Round(this);

        /// <summary>Round each component to the given number of fractional <paramref name="digits"/>.</summary>
        public Vector2d Round(int digits) => Round(this, digits);

        /// <summary>Round each component to the nearest integral value, using the given midpoint <paramref name="mode"/>.</summary>
        public Vector2d Round(MidpointRounding mode) => Round(this, mode);

        /// <summary>Round each component to the given <paramref name="digits"/>, using the given midpoint <paramref name="mode"/>.</summary>
        public Vector2d Round(int digits, MidpointRounding mode) => Round(this, digits, mode);

        #endregion

        #region Deconstruction and tuple conversions

        /// <summary>Deconstruct the vector into its components, enabling <c>var (x, y) = vector;</c>.</summary>
        public void Deconstruct(out double x, out double y)
        {
            x = this.X;
            y = this.Y;
        }

        /// <summary>Create a vector from an (x, y) tuple.</summary>
        public static implicit operator Vector2d((double x, double y) components)
        {
            return new Vector2d(components.x, components.y);
        }

        /// <summary>Convert a vector to an (x, y) tuple.</summary>
        public static implicit operator (double X, double Y)(Vector2d vector)
        {
            return (vector.X, vector.Y);
        }

        #endregion

        #region Conversions

        /// <summary>Widening from the single-precision <see cref="Vector2"/> is implicit — it never loses precision.</summary>
        public static implicit operator Vector2d(Vector2 v) => new Vector2d(v.X, v.Y);

        /// <summary>Narrowing to the single-precision <see cref="Vector2"/> is explicit — you accept the precision loss by writing the cast.</summary>
        public static explicit operator Vector2(Vector2d v) => new Vector2((float)v.X, (float)v.Y);

        /// <summary>Promote this planar vector to a <see cref="Vector3d"/> with the given <paramref name="z"/> (default 0).</summary>
        public Vector3d ToVector3d(double z = 0) => new Vector3d(this.x, this.y, z);

        #endregion

        #region Standard Operations (ToString, CompareTo etc)

        /// <summary>Textual description of the vector.</summary>
        public override string ToString()
        {
            return this.ToString(null, null);
        }

        /// <summary>Verbose textual description of the vector.</summary>
        public string ToVerbString()
        {
            string output = this.IsUnitVector() ? UNIT_VECTOR : POSITIONAL_VECTOR;
            output += string.Format("( x={0}, y={1} )", this.X, this.Y);
            output += MAGNITUDE + this.Magnitude;
            return output;
        }

        /// <summary>
        /// Textual description of the vector.
        /// </summary>
        /// <param name="format">Formatting string: 'x', 'y' or '' followed by a standard numeric format string.</param>
        /// <param name="formatProvider">The culture specific formatting provider.</param>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Format("({0}, {1})", this.X, this.Y);
            }

            char firstChar = format![0];
            string? remainder = format.Length > 1 ? format.Substring(1) : null;

            switch (firstChar)
            {
                case 'x': return this.X.ToString(remainder, formatProvider);
                case 'y': return this.Y.ToString(remainder, formatProvider);
                default:
                    return string.Format(
                        "({0}, {1})",
                        this.X.ToString(format, formatProvider),
                        this.Y.ToString(format, formatProvider));
            }
        }

        /// <summary>Get the hashcode.</summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.x.GetHashCode();
                hashCode = (hashCode * 397) ^ this.y.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>Equality with another object (a <see cref="Vector2d"/>), treating NaN components as equal.</summary>
        public override bool Equals(object? other)
        {
            return other is Vector2d v && v.Equals(this);
        }

        /// <summary>Equality with another object within a tolerance.</summary>
        public bool Equals(object other, double tolerance)
        {
            return other is Vector2d v && this.Equals(v, tolerance);
        }

        /// <summary>Equality with another vector, treating NaN components as equal (unlike ==).</summary>
        public bool Equals(Vector2d other)
        {
            return this.X.Equals(other.X) && this.Y.Equals(other.Y);
        }

        /// <summary>Equality with another vector within an absolute tolerance on every component.</summary>
        public bool Equals(Vector2d other, double tolerance)
        {
            return this.X.AlmostEqualsWithAbsTolerance(other.X, tolerance)
                && this.Y.AlmostEqualsWithAbsTolerance(other.Y, tolerance);
        }

        /// <summary>Approximate equality — alias of <see cref="Equals(Vector2d, double)"/> matching the single-precision family.</summary>
        public bool ApproximatelyEquals(Vector2d other, double tolerance = 1e-5) => this.Equals(other, tolerance);

        /// <summary>Compare the magnitude of this vector against another's.</summary>
        public int CompareTo(Vector2d other)
        {
            if (this < other) { return -1; }
            if (this > other) { return 1; }
            return 0;
        }

        /// <summary>Compare the magnitude of this vector against another object's (which must be a <see cref="Vector2d"/>).</summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="other"/> is not a <see cref="Vector2d"/>.</exception>
        public int CompareTo(object? other)
        {
            if (other is Vector2d v)
            {
                return this.CompareTo(v);
            }

            throw new ArgumentException(
                NON_VECTOR_COMPARISON + "\n" + ARGUMENT_TYPE + other?.GetType().ToString(),
                nameof(other));
        }

        /// <summary>Compare magnitudes within a tolerance (treating two infinite-magnitude vectors as equal).</summary>
        public int CompareTo(Vector2d other, double tolerance)
        {
            var bothInfinite = double.IsInfinity(this.SumComponentSqrs()) && double.IsInfinity(other.SumComponentSqrs());

            if (this.Equals(other, tolerance) || bothInfinite)
            {
                return 0;
            }

            if (this < other) { return -1; }
            return 1;
        }

        /// <summary>Compare magnitudes within a tolerance against another object (which must be a <see cref="Vector2d"/>).</summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="other"/> is not a <see cref="Vector2d"/>.</exception>
        public int CompareTo(object other, double tolerance)
        {
            if (other is Vector2d v)
            {
                return this.CompareTo(v, tolerance);
            }

            throw new ArgumentException(
                NON_VECTOR_COMPARISON + "\n" + ARGUMENT_TYPE + other?.GetType().ToString(),
                nameof(other));
        }

        #endregion

        #region Decisions

        /// <summary>Whether the vector's magnitude is one within the given <paramref name="tolerance"/>.</summary>
        public static bool IsUnitVector(Vector2d v1, double tolerance)
        {
            return Math.Abs(v1.Magnitude - 1) <= tolerance;
        }

        /// <summary>Whether the vector's magnitude is exactly one.</summary>
        public static bool IsUnitVector(Vector2d v1)
        {
            return v1.Magnitude == 1;
        }

        /// <summary>Whether this vector's magnitude is exactly one.</summary>
        public bool IsUnitVector() => IsUnitVector(this);

        /// <summary>Whether this vector's magnitude is one within the given <paramref name="tolerance"/>.</summary>
        public bool IsUnitVector(double tolerance) => IsUnitVector(this, tolerance);

        /// <summary>Whether this vector's length is one within the given <paramref name="tolerance"/> — alias matching the single-precision family.</summary>
        public bool IsUnit(double tolerance = 1e-5) => IsUnitVector(this, tolerance);

        /// <summary>Whether two vectors are perpendicular within the given <paramref name="tolerance"/>.</summary>
        public static bool IsPerpendicular(Vector2d v1, Vector2d v2, double tolerance)
        {
            return v1.NormalizeOrDefault().DotProduct(v2.NormalizeOrDefault()).AlmostEqualsWithAbsTolerance(0, tolerance);
        }

        /// <summary>Whether two vectors are exactly perpendicular.</summary>
        public static bool IsPerpendicular(Vector2d v1, Vector2d v2)
        {
            return v1.NormalizeOrDefault().DotProduct(v2.NormalizeOrDefault()) == 0;
        }

        /// <summary>Whether this vector is perpendicular to another.</summary>
        public bool IsPerpendicular(Vector2d other) => IsPerpendicular(this, other);

        /// <summary>Whether this vector is perpendicular to another within the given <paramref name="tolerance"/>.</summary>
        public bool IsPerpendicular(Vector2d other, double tolerance) => IsPerpendicular(this, other, tolerance);

        /// <summary>Whether any component is NaN.</summary>
        public static bool IsNaN(Vector2d v1)
        {
            return double.IsNaN(v1.X) || double.IsNaN(v1.Y);
        }

        /// <summary>Whether any of this vector's components is NaN.</summary>
        public bool IsNaN() => IsNaN(this);

        /// <summary>Whether both components are exactly zero.</summary>
        public bool IsZero()
        {
            return this.X == 0 && this.Y == 0;
        }

        /// <summary>Whether the vector's magnitude is within <paramref name="tolerance"/> of zero.</summary>
        public bool IsZero(double tolerance)
        {
            return this.Magnitude <= tolerance;
        }

        #endregion

        #region Cartesian Vectors

        /// <summary>Vector representing the Cartesian origin (0, 0).</summary>
        public static readonly Vector2d Origin = new Vector2d(0, 0);

        /// <summary>Vector representing the Cartesian X axis (1, 0).</summary>
        public static readonly Vector2d XAxis = new Vector2d(1, 0);

        /// <summary>Vector representing the Cartesian Y axis (0, 1).</summary>
        public static readonly Vector2d YAxis = new Vector2d(0, 1);

        #endregion

        #region Constants

        /// <summary>The smallest vector possible (based on the double precision floating point structure).</summary>
        public static readonly Vector2d MinValue = new Vector2d(double.MinValue, double.MinValue);

        /// <summary>The largest vector possible (based on the double precision floating point structure).</summary>
        public static readonly Vector2d MaxValue = new Vector2d(double.MaxValue, double.MaxValue);

        /// <summary>The smallest positive (non-zero) vector possible (based on the double precision floating point structure).</summary>
        public static readonly Vector2d Epsilon = new Vector2d(double.Epsilon, double.Epsilon);

        /// <summary>Vector with components and magnitude of zero — an alias of <see cref="Origin"/>.</summary>
        public static readonly Vector2d Zero = Origin;

        /// <summary>Vector with both components one (1, 1).</summary>
        public static readonly Vector2d One = new Vector2d(1, 1);

        /// <summary>Unit vector along the X axis — an alias of <see cref="XAxis"/>.</summary>
        public static readonly Vector2d UnitX = XAxis;

        /// <summary>Unit vector along the Y axis — an alias of <see cref="YAxis"/>.</summary>
        public static readonly Vector2d UnitY = YAxis;

        /// <summary>Vector with components of NaN.</summary>
        public static readonly Vector2d NaN = new Vector2d(double.NaN, double.NaN);

        #endregion

        #region Messages

        private const string TWO_COMPONENTS = "Array must contain exactly two components, (x,y)";
        private const string NORMALIZE_NaN = "Cannot normalize a vector when it's magnitude is NaN";
        private const string NORMALIZE_0 = "Cannot normalize a vector when it's magnitude is zero";
        private const string NORMALIZE_Inf = "Cannot normalize a vector when it's magnitude is infinite except under special conditions";
        private const string INTERPOLATION_RANGE = "Control parameter must be a value between 0 & 1";
        private const string NON_VECTOR_COMPARISON = "Cannot compare a Vector2d to a non-Vector2d";
        private const string ARGUMENT_TYPE = "The argument provided is a type of ";
        private const string ARGUMENT_VALUE = "The argument provided has a value of ";
        private const string NEGATIVE_MAGNITUDE = "The magnitude of a Vector2d must be a positive value, (i.e. greater than 0)";
        private const string ORIGIN_VECTOR_MAGNITUDE = "Cannot change the magnitude of Vector2d(0,0)";
        private const string UNIT_VECTOR = "Unit vector composing of ";
        private const string POSITIONAL_VECTOR = "Positional vector composing of ";
        private const string MAGNITUDE = " of magnitude ";

        #endregion
    }
}
