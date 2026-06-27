namespace RP.Math
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    using Exceptions;

    using Math = System.Math;

    /// <summary>
    /// A double-precision vector with four components (x, y, z, w) — the 4-D sibling of
    /// <see cref="Vector3d"/>, completing the double-precision vector family.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Four-component vectors are most often <b>homogeneous coordinates</b>: an (x, y, z) point or direction
    /// carried with a fourth coordinate <c>w</c> so that translation and perspective become ordinary matrix
    /// multiplications. <see cref="Dehomogenize"/> performs the perspective divide back to a
    /// <see cref="Vector3d"/>, and <see cref="XYZ"/> drops <c>w</c> directly.
    /// </para>
    /// <para>
    /// The surface mirrors <see cref="Vector3d"/> — operators, dot product, normalisation, interpolation,
    /// projection/rejection/reflection, component-wise maths, rounding, tolerance-aware equality/comparison
    /// and the same numeric edge-case handling — minus the operations that only exist in three dimensions
    /// (there is no cross product or axis rotation in 4-D). The angle between two vectors uses Kahan's stable
    /// <c>2·atan2(‖a−b‖, ‖a+b‖)</c> form, which is accurate across the whole 0…π range in any dimension.
    /// </para>
    /// <para>
    /// As with the rest of RP.Math this is an immutable value type. Method names follow <see cref="Vector3d"/>
    /// (<see cref="DotProduct(Vector4d)"/>, <see cref="Interpolate(Vector4d, double)"/>, …); the terser
    /// single-precision names (<see cref="Dot(Vector4d)"/>, <see cref="Length"/>, …) are provided as aliases.
    /// </para>
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [ImmutableObject(true), Serializable]
    public struct Vector4d
        : IComparable, IComparable<Vector4d>, IEquatable<Vector4d>, IFormattable
    {
        #region Class Variables

        private readonly double x;
        private readonly double y;
        private readonly double z;
        private readonly double w;

        #endregion

        #region Constructors

        /// <summary>Construct a vector from its four components.</summary>
        public Vector4d(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>Construct a vector from a four-element array.</summary>
        /// <exception cref="ArgumentException">Thrown if the array does not contain exactly four components.</exception>
        public Vector4d(double[] xyzw)
        {
            if (xyzw.Length == 4)
            {
                this.x = xyzw[0];
                this.y = xyzw[1];
                this.z = xyzw[2];
                this.w = xyzw[3];
            }
            else
            {
                throw new ArgumentException(FOUR_COMPONENTS);
            }
        }

        /// <summary>Construct a copy of another vector.</summary>
        public Vector4d(Vector4d v1)
        {
            this.x = v1.X;
            this.y = v1.Y;
            this.z = v1.Z;
            this.w = v1.W;
        }

        /// <summary>Construct a double vector from the single-precision <see cref="Vector4"/>.</summary>
        public Vector4d(Vector4 v1)
        {
            this.x = v1.X;
            this.y = v1.Y;
            this.z = v1.Z;
            this.w = v1.W;
        }

        /// <summary>Construct a 4-vector from a <see cref="Vector3d"/> and a <paramref name="w"/> coordinate.</summary>
        public Vector4d(Vector3d xyz, double w)
        {
            this.x = xyz.X;
            this.y = xyz.Y;
            this.z = xyz.Z;
            this.w = w;
        }

        #endregion

        #region Accessors & Mutators

        /// <summary>Get the x component of the vector.</summary>
        public double X { get { return this.x; } }

        /// <summary>Get the y component of the vector.</summary>
        public double Y { get { return this.y; } }

        /// <summary>Get the z component of the vector.</summary>
        public double Z { get { return this.z; } }

        /// <summary>Get the w component of the vector.</summary>
        public double W { get { return this.w; } }

        /// <summary>Gets the magnitude (aka. length or absolute value) of the vector.</summary>
        public double Magnitude { get { return Math.Sqrt(this.SumComponentSqrs()); } }

        /// <summary>The square of the vector's magnitude, avoiding a square root where only relative magnitudes are needed.</summary>
        public double MagnitudeSquared { get { return this.SumComponentSqrs(); } }

        /// <summary>The vector's length — an alias of <see cref="Magnitude"/> matching the single-precision family.</summary>
        public double Length { get { return this.Magnitude; } }

        /// <summary>The vector's squared length — an alias of <see cref="MagnitudeSquared"/>.</summary>
        public double LengthSquared { get { return this.MagnitudeSquared; } }

        /// <summary>The (x, y, z) part of the vector, dropping <c>w</c>.</summary>
        public Vector3d XYZ { get { return new Vector3d(this.x, this.y, this.z); } }

        /// <summary>Gets the vector as an array.</summary>
        [XmlIgnore]
        public double[] Array { get { return new[] { this.x, this.y, this.z, this.w }; } }

        /// <summary>An index accessor mapping [0] -&gt; X, [1] -&gt; Y, [2] -&gt; Z and [3] -&gt; W.</summary>
        /// <exception cref="ArgumentException">Thrown if the index is not in 0..3.</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return this.X;
                    case 1: return this.Y;
                    case 2: return this.Z;
                    case 3: return this.W;
                    default: throw new ArgumentException(FOUR_COMPONENTS, "index");
                }
            }
        }

        #endregion

        #region Operators

        /// <summary>Addition of two vectors.</summary>
        public static Vector4d operator +(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
        }

        /// <summary>Subtraction of two vectors.</summary>
        public static Vector4d operator -(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);
        }

        /// <summary>Scalar multiplication (vector on the left).</summary>
        public static Vector4d operator *(Vector4d v1, double s2)
        {
            return new Vector4d(v1.X * s2, v1.Y * s2, v1.Z * s2, v1.W * s2);
        }

        /// <summary>Scalar multiplication (scalar on the left).</summary>
        public static Vector4d operator *(double s1, Vector4d v2)
        {
            return v2 * s1;
        }

        /// <summary>Component-wise (Hadamard) product.</summary>
        public static Vector4d operator *(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W);
        }

        /// <summary>Scalar division.</summary>
        public static Vector4d operator /(Vector4d v1, double s2)
        {
            return new Vector4d(v1.X / s2, v1.Y / s2, v1.Z / s2, v1.W / s2);
        }

        /// <summary>Negation — reverses direction.</summary>
        public static Vector4d operator -(Vector4d v1)
        {
            return new Vector4d(-v1.X, -v1.Y, -v1.Z, -v1.W);
        }

        /// <summary>Unary plus — returns the vector unchanged.</summary>
        public static Vector4d operator +(Vector4d v1)
        {
            return new Vector4d(+v1.X, +v1.Y, +v1.Z, +v1.W);
        }

        /// <summary>Less-than by magnitude (see <see cref="CompareTo(Vector4d)"/>).</summary>
        public static bool operator <(Vector4d v1, Vector4d v2)
        {
            return v1.SumComponentSqrs() < v2.SumComponentSqrs();
        }

        /// <summary>Greater-than by magnitude.</summary>
        public static bool operator >(Vector4d v1, Vector4d v2)
        {
            return v1.SumComponentSqrs() > v2.SumComponentSqrs();
        }

        /// <summary>Less-than-or-equal by magnitude.</summary>
        public static bool operator <=(Vector4d v1, Vector4d v2)
        {
            return v1.SumComponentSqrs() <= v2.SumComponentSqrs();
        }

        /// <summary>Greater-than-or-equal by magnitude.</summary>
        public static bool operator >=(Vector4d v1, Vector4d v2)
        {
            return v1.SumComponentSqrs() >= v2.SumComponentSqrs();
        }

        /// <summary>Exact component equality.</summary>
        public static bool operator ==(Vector4d v1, Vector4d v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z && v1.W == v2.W;
        }

        /// <summary>Exact component inequality.</summary>
        public static bool operator !=(Vector4d v1, Vector4d v2)
        {
            return !(v1 == v2);
        }

        #endregion

        #region Magnitude operation

        /// <summary>Scale a vector to the given magnitude, preserving direction.</summary>
        public static Vector4d Scale(Vector4d vector, double magnitude)
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
        public Vector4d Scale(double magnitude)
        {
            return Scale(this, magnitude);
        }

        #endregion

        #region Product Operations

        /// <summary>The dot product: <c>|a||b|cosθ</c>. Zero when perpendicular; its sign tells you which side.</summary>
        public static double DotProduct(Vector4d v1, Vector4d v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W;
        }

        /// <summary>The dot product of this vector with another.</summary>
        public double DotProduct(Vector4d other)
        {
            return DotProduct(this, other);
        }

        /// <summary>The dot product — terse alias of <see cref="DotProduct(Vector4d, Vector4d)"/>.</summary>
        public static double Dot(Vector4d v1, Vector4d v2) => DotProduct(v1, v2);

        /// <summary>The dot product — terse alias of <see cref="DotProduct(Vector4d)"/>.</summary>
        public double Dot(Vector4d other) => DotProduct(this, other);

        #endregion

        #region Normalize Operations

        /// <summary>Get the normalized unit vector with a magnitude of one.</summary>
        /// <exception cref="NormalizeVectorException">
        /// Thrown when the vector has a magnitude of zero, NaN, or an un-normalizable infinite magnitude.
        /// </exception>
        public static Vector4d Normalize(Vector4d v1)
        {
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
        /// Get the normalized unit vector, returning the origin for a zero magnitude and (NaN, …) for a NaN
        /// magnitude rather than throwing.
        /// </summary>
        public static Vector4d NormalizeOrDefault(Vector4d v1)
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
        public Vector4d Normalize()
        {
            return Normalize(this);
        }

        /// <summary>Get the normalized unit vector, falling back to the origin / NaN as <see cref="NormalizeOrDefault(Vector4d)"/>.</summary>
        public Vector4d NormalizeOrDefault()
        {
            return NormalizeOrDefault(this);
        }

        private static Vector4d NormalizeOrNaN(Vector4d v1)
        {
            double inverse = 1 / v1.Magnitude;
            return new Vector4d(v1.X * inverse, v1.Y * inverse, v1.Z * inverse, v1.W * inverse);
        }

        private static Vector4d NormalizeSpecialCasesOrOriginal(Vector4d v1)
        {
            if (double.IsInfinity(v1.Magnitude))
            {
                var x = v1.X == 0 ? 0 : double.IsPositiveInfinity(v1.X) ? 1 : double.IsNegativeInfinity(v1.X) ? -1 : double.NaN;
                var y = v1.Y == 0 ? 0 : double.IsPositiveInfinity(v1.Y) ? 1 : double.IsNegativeInfinity(v1.Y) ? -1 : double.NaN;
                var z = v1.Z == 0 ? 0 : double.IsPositiveInfinity(v1.Z) ? 1 : double.IsNegativeInfinity(v1.Z) ? -1 : double.NaN;
                var w = v1.W == 0 ? 0 : double.IsPositiveInfinity(v1.W) ? 1 : double.IsNegativeInfinity(v1.W) ? -1 : double.NaN;

                return new Vector4d(x, y, z, w);
            }

            return v1;
        }

        #endregion

        #region Interpolation Operations

        /// <summary>Take an interpolated value from between two vectors, or an extrapolated value if allowed.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the control is not between 0 and 1 and extrapolation is not allowed.
        /// </exception>
        public static Vector4d Interpolate(Vector4d v1, Vector4d v2, double control, bool allowExtrapolation)
        {
            if (!allowExtrapolation && (control > 1 || control < 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(control),
                    control,
                    INTERPOLATION_RANGE + "\n" + ARGUMENT_VALUE + control);
            }

            return new Vector4d(
                v1.X * (1 - control) + v2.X * control,
                v1.Y * (1 - control) + v2.Y * control,
                v1.Z * (1 - control) + v2.Z * control,
                v1.W * (1 - control) + v2.W * control);
        }

        /// <summary>Take an interpolated value from between two vectors (control in [0, 1]).</summary>
        public static Vector4d Interpolate(Vector4d v1, Vector4d v2, double control)
        {
            return Interpolate(v1, v2, control, false);
        }

        /// <summary>Take an interpolated value between this vector and another (control in [0, 1]).</summary>
        public Vector4d Interpolate(Vector4d other, double control)
        {
            return Interpolate(this, other, control);
        }

        /// <summary>Take an interpolated, or extrapolated, value between this vector and another.</summary>
        public Vector4d Interpolate(Vector4d other, double control, bool allowExtrapolation)
        {
            return Interpolate(this, other, control, allowExtrapolation);
        }

        /// <summary>Linear interpolation — terse alias of <see cref="Interpolate(Vector4d, Vector4d, double, bool)"/> allowing extrapolation.</summary>
        public static Vector4d Lerp(Vector4d v1, Vector4d v2, double control) => Interpolate(v1, v2, control, true);

        /// <summary>Linear interpolation — terse alias allowing extrapolation.</summary>
        public Vector4d Lerp(Vector4d other, double control) => Interpolate(this, other, control, true);

        /// <summary>
        /// Spherically interpolate between two vectors: the direction follows the shortest arc while the
        /// magnitude is blended linearly. Falls back to linear interpolation when the vectors are
        /// (anti)parallel or either is zero.
        /// </summary>
        public static Vector4d Slerp(Vector4d v1, Vector4d v2, double control)
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
        public Vector4d Slerp(Vector4d other, double control) => Slerp(this, other, control);

        #endregion

        #region Distance Operations

        /// <summary>Find the distance between two vectors (Pythagoras).</summary>
        public static double Distance(Vector4d v1, Vector4d v2)
        {
            return Math.Sqrt(
                (v1.X - v2.X) * (v1.X - v2.X) +
                (v1.Y - v2.Y) * (v1.Y - v2.Y) +
                (v1.Z - v2.Z) * (v1.Z - v2.Z) +
                (v1.W - v2.W) * (v1.W - v2.W));
        }

        /// <summary>Find the distance between this vector and another.</summary>
        public double Distance(Vector4d other)
        {
            return Distance(this, other);
        }

        /// <summary>The squared distance between two vectors — cheaper when only comparing distances.</summary>
        public static double DistanceSquared(Vector4d v1, Vector4d v2)
        {
            return (v1.X - v2.X) * (v1.X - v2.X)
                + (v1.Y - v2.Y) * (v1.Y - v2.Y)
                + (v1.Z - v2.Z) * (v1.Z - v2.Z)
                + (v1.W - v2.W) * (v1.W - v2.W);
        }

        /// <summary>The squared distance between this vector and another.</summary>
        public double DistanceSquared(Vector4d other) => DistanceSquared(this, other);

        #endregion

        #region Angle Operations

        /// <summary>
        /// The angle between two vectors, in radians, in [0, π]. Uses Kahan's numerically stable
        /// <c>2·atan2(‖u1 − u2‖, ‖u1 + u2‖)</c> form on the normalized vectors, so it is accurate across the
        /// whole range and never returns NaN for (anti)parallel inputs.
        /// </summary>
        public static double Angle(Vector4d v1, Vector4d v2)
        {
            if (v1 == v2)
            {
                return 0;
            }

            var u1 = NormalizeOrDefault(v1);
            var u2 = NormalizeOrDefault(v2);

            return 2.0 * Math.Atan2((u1 - u2).Magnitude, (u1 + u2).Magnitude);
        }

        /// <summary>The angle between this vector and another, in radians, in [0, π].</summary>
        public double Angle(Vector4d other)
        {
            return Angle(this, other);
        }

        /// <summary>The angle between this vector and another as an <see cref="RP.Math.Angle"/>.</summary>
        public Angle AngleTo(Vector4d other) => new Angle(Angle(this, other));

        /// <summary>The angle between two vectors as an <see cref="RP.Math.Angle"/>.</summary>
        public static Angle AngleBetween(Vector4d v1, Vector4d v2) => new Angle(Angle(v1, v2));

        #endregion

        #region Projection, Rejection and Reflection Operations

        /// <summary>The vector resolute of <paramref name="v1"/> in the direction of <paramref name="v2"/> (its "shadow").</summary>
        public static Vector4d Projection(Vector4d v1, Vector4d v2)
        {
            return v2 * (v1.DotProduct(v2) / Math.Pow(v2.Magnitude, 2));
        }

        /// <summary>The vector resolute of this vector in the given <paramref name="direction"/>.</summary>
        public Vector4d Projection(Vector4d direction)
        {
            return Projection(this, direction);
        }

        /// <summary>Vector projection — terse alias of <see cref="Projection(Vector4d, Vector4d)"/>.</summary>
        public static Vector4d Project(Vector4d v1, Vector4d v2) => Projection(v1, v2);

        /// <summary>Vector projection — terse alias of <see cref="Projection(Vector4d)"/>.</summary>
        public Vector4d Project(Vector4d direction) => Projection(this, direction);

        /// <summary>The component of <paramref name="v1"/> perpendicular to <paramref name="v2"/> (so projection + rejection = original).</summary>
        public static Vector4d Rejection(Vector4d v1, Vector4d v2)
        {
            return v1 - v1.Projection(v2);
        }

        /// <summary>The component of this vector perpendicular to the given <paramref name="direction"/>.</summary>
        public Vector4d Rejection(Vector4d direction)
        {
            return Rejection(this, direction);
        }

        /// <summary>Vector rejection — terse alias of <see cref="Rejection(Vector4d, Vector4d)"/>.</summary>
        public static Vector4d Reject(Vector4d v1, Vector4d v2) => Rejection(v1, v2);

        /// <summary>Vector rejection — terse alias of <see cref="Rejection(Vector4d)"/>.</summary>
        public Vector4d Reject(Vector4d direction) => Rejection(this, direction);

        /// <summary>
        /// Reflect a vector off a hyperplane with the given <paramref name="normal"/> (angle of incidence
        /// equals angle of reflection). The normal should be unit length.
        /// </summary>
        public static Vector4d Reflect(Vector4d vector, Vector4d normal)
        {
            return vector - 2 * vector.DotProduct(normal) * normal;
        }

        /// <summary>Reflect this vector off a hyperplane with the given <paramref name="normal"/>.</summary>
        public Vector4d Reflect(Vector4d normal)
        {
            return Reflect(this, normal);
        }

        #endregion

        #region Componentwise Min, Max and Clamp

        /// <summary>The component-wise minimum of two vectors.</summary>
        public static Vector4d ComponentMin(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y), Math.Min(v1.Z, v2.Z), Math.Min(v1.W, v2.W));
        }

        /// <summary>The component-wise minimum of this vector and another.</summary>
        public Vector4d ComponentMin(Vector4d other) => ComponentMin(this, other);

        /// <summary>The component-wise maximum of two vectors.</summary>
        public static Vector4d ComponentMax(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y), Math.Max(v1.Z, v2.Z), Math.Max(v1.W, v2.W));
        }

        /// <summary>The component-wise maximum of this vector and another.</summary>
        public Vector4d ComponentMax(Vector4d other) => ComponentMax(this, other);

        /// <summary>Clamp each component into the box defined by <paramref name="min"/> and <paramref name="max"/>.</summary>
        public static Vector4d Clamp(Vector4d value, Vector4d min, Vector4d max)
        {
            return new Vector4d(
                value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
                value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
                value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z,
                value.W < min.W ? min.W : value.W > max.W ? max.W : value.W);
        }

        /// <summary>Clamp each component of this vector into the box defined by <paramref name="min"/> and <paramref name="max"/>.</summary>
        public Vector4d Clamp(Vector4d min, Vector4d max) => Clamp(this, min, max);

        #endregion

        #region Min, Max, ClampMagnitude and MoveTowards

        /// <summary>Compare the magnitude of two vectors and return the greater.</summary>
        public static Vector4d Max(Vector4d v1, Vector4d v2)
        {
            return v1 >= v2 ? v1 : v2;
        }

        /// <summary>Compare the magnitude of this vector and another, returning the greater.</summary>
        public Vector4d Max(Vector4d other) => Max(this, other);

        /// <summary>Compare the magnitude of two vectors and return the lesser.</summary>
        public static Vector4d Min(Vector4d v1, Vector4d v2)
        {
            return v1 <= v2 ? v1 : v2;
        }

        /// <summary>Compare the magnitude of this vector and another, returning the lesser.</summary>
        public Vector4d Min(Vector4d other) => Min(this, other);

        /// <summary>Cap the vector's magnitude at <paramref name="maxMagnitude"/> while keeping its direction.</summary>
        public static Vector4d ClampMagnitude(Vector4d vector, double maxMagnitude)
        {
            double lengthSq = vector.SumComponentSqrs();
            if (lengthSq <= maxMagnitude * maxMagnitude)
            {
                return vector;
            }

            return vector / Math.Sqrt(lengthSq) * maxMagnitude;
        }

        /// <summary>Cap this vector's magnitude at <paramref name="maxMagnitude"/> while keeping its direction.</summary>
        public Vector4d ClampMagnitude(double maxMagnitude) => ClampMagnitude(this, maxMagnitude);

        /// <summary>
        /// Move <paramref name="current"/> toward <paramref name="target"/> by at most
        /// <paramref name="maxDistanceDelta"/>, never overshooting.
        /// </summary>
        public static Vector4d MoveTowards(Vector4d current, Vector4d target, double maxDistanceDelta)
        {
            Vector4d delta = target - current;
            double dist = delta.Magnitude;
            if (dist <= maxDistanceDelta || dist == 0)
            {
                return target;
            }

            return current + delta / dist * maxDistanceDelta;
        }

        /// <summary>Move this vector toward <paramref name="target"/> by at most <paramref name="maxDistanceDelta"/>.</summary>
        public Vector4d MoveTowards(Vector4d target, double maxDistanceDelta) => MoveTowards(this, target, maxDistanceDelta);

        #endregion

        #region Component Operations

        /// <summary>The sum of the components.</summary>
        public static double SumComponents(Vector4d v1)
        {
            return v1.X + v1.Y + v1.Z + v1.W;
        }

        /// <summary>The sum of this vector's components.</summary>
        public double SumComponents() => SumComponents(this);

        /// <summary>The sum of the squares of the components.</summary>
        public static double SumComponentSqrs(Vector4d v1)
        {
            return v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z + v1.W * v1.W;
        }

        /// <summary>The sum of the squares of this vector's components.</summary>
        public double SumComponentSqrs() => SumComponentSqrs(this);

        /// <summary>Raise each component to <paramref name="power"/>.</summary>
        public static Vector4d PowComponents(Vector4d v1, double power)
        {
            return new Vector4d(Math.Pow(v1.X, power), Math.Pow(v1.Y, power), Math.Pow(v1.Z, power), Math.Pow(v1.W, power));
        }

        /// <summary>Raise each of this vector's components to <paramref name="power"/>.</summary>
        public Vector4d PowComponents(double power) => PowComponents(this, power);

        /// <summary>The square root of each component.</summary>
        public static Vector4d SqrtComponents(Vector4d v1)
        {
            return new Vector4d(Math.Sqrt(v1.X), Math.Sqrt(v1.Y), Math.Sqrt(v1.Z), Math.Sqrt(v1.W));
        }

        /// <summary>The square root of each of this vector's components.</summary>
        public Vector4d SqrtComponents() => SqrtComponents(this);

        /// <summary>The square of each component.</summary>
        public static Vector4d SqrComponents(Vector4d v1)
        {
            return new Vector4d(v1.X * v1.X, v1.Y * v1.Y, v1.Z * v1.Z, v1.W * v1.W);
        }

        /// <summary>The square of each of this vector's components.</summary>
        public Vector4d SqrComponents() => SqrComponents(this);

        /// <summary>The absolute value of each component.</summary>
        public static Vector4d AbsComponents(Vector4d v1)
        {
            return new Vector4d(Math.Abs(v1.X), Math.Abs(v1.Y), Math.Abs(v1.Z), Math.Abs(v1.W));
        }

        /// <summary>The absolute value of each of this vector's components.</summary>
        public Vector4d AbsComponents() => AbsComponents(this);

        /// <summary>The absolute value of each component — alias of <see cref="AbsComponents()"/> matching the single-precision family.</summary>
        public Vector4d Abs() => AbsComponents(this);

        #endregion

        #region Round Components

        /// <summary>Round each component to the nearest integral value.</summary>
        public static Vector4d Round(Vector4d v1)
        {
            return new Vector4d(Math.Round(v1.X), Math.Round(v1.Y), Math.Round(v1.Z), Math.Round(v1.W));
        }

        /// <summary>Round each component to the given number of fractional <paramref name="digits"/>.</summary>
        public static Vector4d Round(Vector4d v1, int digits)
        {
            return new Vector4d(Math.Round(v1.X, digits), Math.Round(v1.Y, digits), Math.Round(v1.Z, digits), Math.Round(v1.W, digits));
        }

        /// <summary>Round each component to the nearest integral value, using the given midpoint <paramref name="mode"/>.</summary>
        public static Vector4d Round(Vector4d v1, MidpointRounding mode)
        {
            return new Vector4d(Math.Round(v1.X, mode), Math.Round(v1.Y, mode), Math.Round(v1.Z, mode), Math.Round(v1.W, mode));
        }

        /// <summary>Round each component to the given <paramref name="digits"/>, using the given midpoint <paramref name="mode"/>.</summary>
        public static Vector4d Round(Vector4d v1, int digits, MidpointRounding mode)
        {
            return new Vector4d(Math.Round(v1.X, digits, mode), Math.Round(v1.Y, digits, mode), Math.Round(v1.Z, digits, mode), Math.Round(v1.W, digits, mode));
        }

        /// <summary>Round each component to the nearest integral value.</summary>
        public Vector4d Round() => Round(this);

        /// <summary>Round each component to the given number of fractional <paramref name="digits"/>.</summary>
        public Vector4d Round(int digits) => Round(this, digits);

        /// <summary>Round each component to the nearest integral value, using the given midpoint <paramref name="mode"/>.</summary>
        public Vector4d Round(MidpointRounding mode) => Round(this, mode);

        /// <summary>Round each component to the given <paramref name="digits"/>, using the given midpoint <paramref name="mode"/>.</summary>
        public Vector4d Round(int digits, MidpointRounding mode) => Round(this, digits, mode);

        #endregion

        #region Homogeneous coordinates

        /// <summary>
        /// The perspective divide: returns the <see cref="Vector3d"/> <c>(x/w, y/w, z/w)</c>, mapping a
        /// homogeneous point back to ordinary 3-D space. For a direction (w == 0) this yields infinite
        /// components; use <see cref="XYZ"/> to drop <c>w</c> without dividing.
        /// </summary>
        public Vector3d Dehomogenize()
        {
            return new Vector3d(this.x / this.w, this.y / this.w, this.z / this.w);
        }

        #endregion

        #region Deconstruction and tuple conversions

        /// <summary>Deconstruct the vector into its components, enabling <c>var (x, y, z, w) = vector;</c>.</summary>
        public void Deconstruct(out double x, out double y, out double z, out double w)
        {
            x = this.X;
            y = this.Y;
            z = this.Z;
            w = this.W;
        }

        /// <summary>Create a vector from an (x, y, z, w) tuple.</summary>
        public static implicit operator Vector4d((double x, double y, double z, double w) components)
        {
            return new Vector4d(components.x, components.y, components.z, components.w);
        }

        /// <summary>Convert a vector to an (x, y, z, w) tuple.</summary>
        public static implicit operator (double X, double Y, double Z, double W)(Vector4d vector)
        {
            return (vector.X, vector.Y, vector.Z, vector.W);
        }

        #endregion

        #region Conversions

        /// <summary>Widening from the single-precision <see cref="Vector4"/> is implicit — it never loses precision.</summary>
        public static implicit operator Vector4d(Vector4 v) => new Vector4d(v.X, v.Y, v.Z, v.W);

        /// <summary>Narrowing to the single-precision <see cref="Vector4"/> is explicit — you accept the precision loss by writing the cast.</summary>
        public static explicit operator Vector4(Vector4d v) => new Vector4((float)v.X, (float)v.Y, (float)v.Z, (float)v.W);

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
            output += string.Format("( x={0}, y={1}, z={2}, w={3} )", this.X, this.Y, this.Z, this.W);
            output += MAGNITUDE + this.Magnitude;
            return output;
        }

        /// <summary>
        /// Textual description of the vector.
        /// </summary>
        /// <param name="format">Formatting string: 'x', 'y', 'z', 'w' or '' followed by a standard numeric format string.</param>
        /// <param name="formatProvider">The culture specific formatting provider.</param>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Format("({0}, {1}, {2}, {3})", this.X, this.Y, this.Z, this.W);
            }

            char firstChar = format![0];
            string? remainder = format.Length > 1 ? format.Substring(1) : null;

            switch (firstChar)
            {
                case 'x': return this.X.ToString(remainder, formatProvider);
                case 'y': return this.Y.ToString(remainder, formatProvider);
                case 'z': return this.Z.ToString(remainder, formatProvider);
                case 'w': return this.W.ToString(remainder, formatProvider);
                default:
                    return string.Format(
                        "({0}, {1}, {2}, {3})",
                        this.X.ToString(format, formatProvider),
                        this.Y.ToString(format, formatProvider),
                        this.Z.ToString(format, formatProvider),
                        this.W.ToString(format, formatProvider));
            }
        }

        /// <summary>Get the hashcode.</summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.x.GetHashCode();
                hashCode = (hashCode * 397) ^ this.y.GetHashCode();
                hashCode = (hashCode * 397) ^ this.z.GetHashCode();
                hashCode = (hashCode * 397) ^ this.w.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>Equality with another object (a <see cref="Vector4d"/>), treating NaN components as equal.</summary>
        public override bool Equals(object? other)
        {
            return other is Vector4d v && v.Equals(this);
        }

        /// <summary>Equality with another object within a tolerance.</summary>
        public bool Equals(object other, double tolerance)
        {
            return other is Vector4d v && this.Equals(v, tolerance);
        }

        /// <summary>Equality with another vector, treating NaN components as equal (unlike ==).</summary>
        public bool Equals(Vector4d other)
        {
            return this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z) && this.W.Equals(other.W);
        }

        /// <summary>Equality with another vector within an absolute tolerance on every component.</summary>
        public bool Equals(Vector4d other, double tolerance)
        {
            return this.X.AlmostEqualsWithAbsTolerance(other.X, tolerance)
                && this.Y.AlmostEqualsWithAbsTolerance(other.Y, tolerance)
                && this.Z.AlmostEqualsWithAbsTolerance(other.Z, tolerance)
                && this.W.AlmostEqualsWithAbsTolerance(other.W, tolerance);
        }

        /// <summary>Approximate equality — alias of <see cref="Equals(Vector4d, double)"/> matching the single-precision family.</summary>
        public bool ApproximatelyEquals(Vector4d other, double tolerance = 1e-5) => this.Equals(other, tolerance);

        /// <summary>Compare the magnitude of this vector against another's.</summary>
        public int CompareTo(Vector4d other)
        {
            if (this < other) { return -1; }
            if (this > other) { return 1; }
            return 0;
        }

        /// <summary>Compare the magnitude of this vector against another object's (which must be a <see cref="Vector4d"/>).</summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="other"/> is not a <see cref="Vector4d"/>.</exception>
        public int CompareTo(object? other)
        {
            if (other is Vector4d v)
            {
                return this.CompareTo(v);
            }

            throw new ArgumentException(
                NON_VECTOR_COMPARISON + "\n" + ARGUMENT_TYPE + other?.GetType().ToString(),
                nameof(other));
        }

        /// <summary>Compare magnitudes within a tolerance (treating two infinite-magnitude vectors as equal).</summary>
        public int CompareTo(Vector4d other, double tolerance)
        {
            var bothInfinite = double.IsInfinity(this.SumComponentSqrs()) && double.IsInfinity(other.SumComponentSqrs());

            if (this.Equals(other, tolerance) || bothInfinite)
            {
                return 0;
            }

            if (this < other) { return -1; }
            return 1;
        }

        /// <summary>Compare magnitudes within a tolerance against another object (which must be a <see cref="Vector4d"/>).</summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="other"/> is not a <see cref="Vector4d"/>.</exception>
        public int CompareTo(object other, double tolerance)
        {
            if (other is Vector4d v)
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
        public static bool IsUnitVector(Vector4d v1, double tolerance)
        {
            return Math.Abs(v1.Magnitude - 1) <= tolerance;
        }

        /// <summary>Whether the vector's magnitude is exactly one.</summary>
        public static bool IsUnitVector(Vector4d v1)
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
        public static bool IsPerpendicular(Vector4d v1, Vector4d v2, double tolerance)
        {
            return v1.NormalizeOrDefault().DotProduct(v2.NormalizeOrDefault()).AlmostEqualsWithAbsTolerance(0, tolerance);
        }

        /// <summary>Whether two vectors are exactly perpendicular.</summary>
        public static bool IsPerpendicular(Vector4d v1, Vector4d v2)
        {
            return v1.NormalizeOrDefault().DotProduct(v2.NormalizeOrDefault()) == 0;
        }

        /// <summary>Whether this vector is perpendicular to another.</summary>
        public bool IsPerpendicular(Vector4d other) => IsPerpendicular(this, other);

        /// <summary>Whether this vector is perpendicular to another within the given <paramref name="tolerance"/>.</summary>
        public bool IsPerpendicular(Vector4d other, double tolerance) => IsPerpendicular(this, other, tolerance);

        /// <summary>Whether any component is NaN.</summary>
        public static bool IsNaN(Vector4d v1)
        {
            return double.IsNaN(v1.X) || double.IsNaN(v1.Y) || double.IsNaN(v1.Z) || double.IsNaN(v1.W);
        }

        /// <summary>Whether any of this vector's components is NaN.</summary>
        public bool IsNaN() => IsNaN(this);

        /// <summary>Whether all components are exactly zero.</summary>
        public bool IsZero()
        {
            return this.X == 0 && this.Y == 0 && this.Z == 0 && this.W == 0;
        }

        /// <summary>Whether the vector's magnitude is within <paramref name="tolerance"/> of zero.</summary>
        public bool IsZero(double tolerance)
        {
            return this.Magnitude <= tolerance;
        }

        #endregion

        #region Cartesian Vectors

        /// <summary>Vector representing the Cartesian origin (0, 0, 0, 0).</summary>
        public static readonly Vector4d Origin = new Vector4d(0, 0, 0, 0);

        /// <summary>Vector representing the X axis (1, 0, 0, 0).</summary>
        public static readonly Vector4d XAxis = new Vector4d(1, 0, 0, 0);

        /// <summary>Vector representing the Y axis (0, 1, 0, 0).</summary>
        public static readonly Vector4d YAxis = new Vector4d(0, 1, 0, 0);

        /// <summary>Vector representing the Z axis (0, 0, 1, 0).</summary>
        public static readonly Vector4d ZAxis = new Vector4d(0, 0, 1, 0);

        /// <summary>Vector representing the W axis (0, 0, 0, 1).</summary>
        public static readonly Vector4d WAxis = new Vector4d(0, 0, 0, 1);

        #endregion

        #region Constants

        /// <summary>The smallest vector possible (based on the double precision floating point structure).</summary>
        public static readonly Vector4d MinValue = new Vector4d(double.MinValue, double.MinValue, double.MinValue, double.MinValue);

        /// <summary>The largest vector possible (based on the double precision floating point structure).</summary>
        public static readonly Vector4d MaxValue = new Vector4d(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue);

        /// <summary>The smallest positive (non-zero) vector possible (based on the double precision floating point structure).</summary>
        public static readonly Vector4d Epsilon = new Vector4d(double.Epsilon, double.Epsilon, double.Epsilon, double.Epsilon);

        /// <summary>Vector with components and magnitude of zero — an alias of <see cref="Origin"/>.</summary>
        public static readonly Vector4d Zero = Origin;

        /// <summary>Vector with all components one (1, 1, 1, 1).</summary>
        public static readonly Vector4d One = new Vector4d(1, 1, 1, 1);

        /// <summary>Unit vector along the X axis — an alias of <see cref="XAxis"/>.</summary>
        public static readonly Vector4d UnitX = XAxis;

        /// <summary>Unit vector along the Y axis — an alias of <see cref="YAxis"/>.</summary>
        public static readonly Vector4d UnitY = YAxis;

        /// <summary>Unit vector along the Z axis — an alias of <see cref="ZAxis"/>.</summary>
        public static readonly Vector4d UnitZ = ZAxis;

        /// <summary>Unit vector along the W axis — an alias of <see cref="WAxis"/>.</summary>
        public static readonly Vector4d UnitW = WAxis;

        /// <summary>Vector with components of NaN.</summary>
        public static readonly Vector4d NaN = new Vector4d(double.NaN, double.NaN, double.NaN, double.NaN);

        #endregion

        #region Messages

        private const string FOUR_COMPONENTS = "Array must contain exactly four components, (x,y,z,w)";
        private const string NORMALIZE_NaN = "Cannot normalize a vector when it's magnitude is NaN";
        private const string NORMALIZE_0 = "Cannot normalize a vector when it's magnitude is zero";
        private const string NORMALIZE_Inf = "Cannot normalize a vector when it's magnitude is infinite except under special conditions";
        private const string INTERPOLATION_RANGE = "Control parameter must be a value between 0 & 1";
        private const string NON_VECTOR_COMPARISON = "Cannot compare a Vector4d to a non-Vector4d";
        private const string ARGUMENT_TYPE = "The argument provided is a type of ";
        private const string ARGUMENT_VALUE = "The argument provided has a value of ";
        private const string NEGATIVE_MAGNITUDE = "The magnitude of a Vector4d must be a positive value, (i.e. greater than 0)";
        private const string ORIGIN_VECTOR_MAGNITUDE = "Cannot change the magnitude of Vector4d(0,0,0,0)";
        private const string UNIT_VECTOR = "Unit vector composing of ";
        private const string POSITIONAL_VECTOR = "Positional vector composing of ";
        private const string MAGNITUDE = " of magnitude ";

        #endregion
    }
}
