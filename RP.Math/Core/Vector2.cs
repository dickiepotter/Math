namespace RP.Math
{
    using System;

    using Math = System.Math;

    /// <summary>
    /// A single-precision (<see cref="float"/>) 2-component vector — texture coordinates, screen-space
    /// positions, 2D directions. The float sibling of the 3D <see cref="Vector3"/>; see that type for the
    /// rationale behind the float/double split.
    /// </summary>
    /// <remarks>
    /// In 2D the "cross product" collapses to a single number, the <b>perpendicular dot</b>
    /// (<see cref="Cross(Vector2, Vector2)"/>): positive when <c>b</c> is to the left of <c>a</c>, negative to the right,
    /// zero when parallel. It is the 2D analogue used for winding and signed-area tests.
    /// </remarks>
    [Serializable]
    public readonly struct Vector2 : IEquatable<Vector2>, IFormattable
    {
        public readonly float X;
        public readonly float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2(float scalar) : this(scalar, scalar) { }

        public static Vector2 Zero => new Vector2(0f, 0f);
        public static Vector2 One => new Vector2(1f, 1f);
        public static Vector2 UnitX => new Vector2(1f, 0f);
        public static Vector2 UnitY => new Vector2(0f, 1f);

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator -(Vector2 v) => new Vector2(-v.X, -v.Y);
        public static Vector2 operator *(Vector2 v, float s) => new Vector2(v.X * s, v.Y * s);
        public static Vector2 operator *(float s, Vector2 v) => v * s;
        public static Vector2 operator *(Vector2 a, Vector2 b) => new Vector2(a.X * b.X, a.Y * b.Y);
        public static Vector2 operator /(Vector2 v, float s) => new Vector2(v.X / s, v.Y / s);
        public static bool operator ==(Vector2 a, Vector2 b) => a.Equals(b);
        public static bool operator !=(Vector2 a, Vector2 b) => !a.Equals(b);

        public float Length => (float)Math.Sqrt((double)X * X + (double)Y * Y);
        public float LengthSquared => X * X + Y * Y;

        /// <summary>Dot product: <c>|a||b|cosθ</c>.</summary>
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
        public float Dot(Vector2 other) => Dot(this, other);

        /// <summary>The 2D perpendicular-dot ("cross"): <c>a.X*b.Y - a.Y*b.X</c>. Signed area of the
        /// parallelogram; sign gives the turn direction from <c>a</c> to <c>b</c>.</summary>
        public static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

        /// <summary>A vector perpendicular to this one, rotated +90° (counter-clockwise).</summary>
        public Vector2 Perpendicular() => new Vector2(-Y, X);

        public Vector2 Normalize()
        {
            float len = Length;
            if (len == 0f) throw new DivideByZeroException("Cannot normalize a zero-length Vector2.");
            return this / len;
        }

        public Vector2 NormalizeOrDefault()
        {
            float len = Length;
            if (len == 0f || float.IsNaN(len) || float.IsInfinity(len)) return Zero;
            return this / len;
        }

        public bool IsUnit(float tolerance = 1e-5f) => Math.Abs(LengthSquared - 1f) <= tolerance;

        public static float Distance(Vector2 a, Vector2 b) => (a - b).Length;
        public static float DistanceSquared(Vector2 a, Vector2 b) => (a - b).LengthSquared;

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) =>
            new Vector2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

        /// <summary>The angle between two vectors, in radians, in [0, π], via the stable atan2 form.</summary>
        public static float Angle(Vector2 a, Vector2 b) => (float)Math.Atan2(Math.Abs(Cross(a, b)), Dot(a, b));

        public Vector2 Reflect(Vector2 normal) => this - 2f * Dot(this, normal) * normal;

        public Vector2 ClampMagnitude(float maxLength)
        {
            float lenSq = LengthSquared;
            if (lenSq <= maxLength * maxLength) return this;
            return this / (float)Math.Sqrt(lenSq) * maxLength;
        }

        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistance)
        {
            Vector2 delta = target - current;
            float dist = delta.Length;
            if (dist <= maxDistance || dist == 0f) return target;
            return current + delta / dist * maxDistance;
        }

        public static Vector2 Min(Vector2 a, Vector2 b) => new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        public static Vector2 Max(Vector2 a, Vector2 b) => new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        public static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max) =>
            new Vector2(FloatMath.Clamp(v.X, min.X, max.X), FloatMath.Clamp(v.Y, min.Y, max.Y));
        public Vector2 Abs() => new Vector2(Math.Abs(X), Math.Abs(Y));
        public bool IsNaN() => float.IsNaN(X) || float.IsNaN(Y);
        public bool IsZero(float tolerance = 0f) =>
            tolerance == 0f ? (X == 0f && Y == 0f) : LengthSquared <= tolerance * tolerance;

#if NET6_0_OR_GREATER
        // GPU/SIMD interop offered only on modern TFMs (see the note in Vector3) to keep netstandard2.0
        // free of the System.Numerics.Vectors dependency.
        public static implicit operator System.Numerics.Vector2(Vector2 v) => new System.Numerics.Vector2(v.X, v.Y);
        public static implicit operator Vector2(System.Numerics.Vector2 v) => new Vector2(v.X, v.Y);
#endif
        public static implicit operator Vector2((float x, float y) t) => new Vector2(t.x, t.y);
        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
        public bool ApproximatelyEquals(Vector2 other, float tolerance = 1e-5f) =>
            Math.Abs(X - other.X) <= tolerance && Math.Abs(Y - other.Y) <= tolerance;
        public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);
        public override int GetHashCode() => FloatMath.CombineHash(X, Y);
        public override string ToString() => $"({X}, {Y})";

        #region Vector2d-name parity and added capabilities

        // ---- Magnitude aliases ----

        /// <summary>The vector's magnitude — an alias of <see cref="Length"/> matching <see cref="Vector2d"/>.</summary>
        public float Magnitude => Length;

        /// <summary>The vector's squared magnitude — an alias of <see cref="LengthSquared"/>.</summary>
        public float MagnitudeSquared => LengthSquared;

        // ---- Dot / Cross aliases ----

        /// <summary>The dot product — alias of <see cref="Dot(Vector2, Vector2)"/> matching <see cref="Vector2d"/>.</summary>
        public static float DotProduct(Vector2 a, Vector2 b) => Dot(a, b);

        /// <summary>The dot product of this vector with another — alias of <see cref="Dot(Vector2)"/>.</summary>
        public float DotProduct(Vector2 other) => Dot(this, other);

        /// <summary>The 2D cross product (perp-dot) — alias of <see cref="Cross(Vector2, Vector2)"/>.</summary>
        public static float CrossProduct(Vector2 a, Vector2 b) => Cross(a, b);

        /// <summary>The 2D cross product (perp-dot) of this vector with another.</summary>
        public float CrossProduct(Vector2 other) => Cross(this, other);

        /// <summary>The 2D cross product (perp-dot) of this vector with another.</summary>
        public float Cross(Vector2 other) => Cross(this, other);

        // ---- Perpendicular / rotation ----

        /// <summary>A vector perpendicular to this one, rotated -90° (clockwise): <c>(y, -x)</c>.</summary>
        public Vector2 PerpendicularCW() => new Vector2(Y, -X);

        /// <summary>Rotate a vector in the plane by <paramref name="radians"/> counter-clockwise (+X turns toward +Y).</summary>
        public static Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        /// <summary>Rotate this vector in the plane by <paramref name="radians"/> counter-clockwise.</summary>
        public Vector2 Rotate(float radians) => Rotate(this, radians);

        /// <summary>The signed angle, in radians, in (−π, π], from <paramref name="a"/> to <paramref name="b"/>
        /// (positive counter-clockwise), via <c>atan2(cross, dot)</c>.</summary>
        public static float SignedAngle(Vector2 a, Vector2 b)
        {
            var na = a.NormalizeOrDefault();
            var nb = b.NormalizeOrDefault();
            return (float)Math.Atan2(Cross(na, nb), Dot(na, nb));
        }

        /// <summary>The signed angle from this vector to another, in radians, in (−π, π].</summary>
        public float SignedAngle(Vector2 other) => SignedAngle(this, other);

        // ---- Projection, rejection, reflection ----

        /// <summary>The vector resolute of <paramref name="v"/> along <paramref name="direction"/> (its "shadow");
        /// <see cref="Zero"/> when the direction is zero.</summary>
        public static Vector2 Projection(Vector2 v, Vector2 direction)
        {
            float denom = Dot(direction, direction);
            if (denom == 0f) return Zero;
            return direction * (Dot(v, direction) / denom);
        }

        /// <summary>The vector resolute of this vector along <paramref name="direction"/>.</summary>
        public Vector2 Projection(Vector2 direction) => Projection(this, direction);

        /// <summary>Vector projection — terse alias of <see cref="Projection(Vector2, Vector2)"/>.</summary>
        public static Vector2 Project(Vector2 v, Vector2 direction) => Projection(v, direction);

        /// <summary>Vector projection — terse alias of <see cref="Projection(Vector2)"/>.</summary>
        public Vector2 Project(Vector2 direction) => Projection(this, direction);

        /// <summary>The component of <paramref name="v"/> perpendicular to <paramref name="direction"/>
        /// (so projection + rejection = original).</summary>
        public static Vector2 Rejection(Vector2 v, Vector2 direction) => v - Projection(v, direction);

        /// <summary>The component of this vector perpendicular to <paramref name="direction"/>.</summary>
        public Vector2 Rejection(Vector2 direction) => Rejection(this, direction);

        /// <summary>Vector rejection — terse alias of <see cref="Rejection(Vector2, Vector2)"/>.</summary>
        public static Vector2 Reject(Vector2 v, Vector2 direction) => Rejection(v, direction);

        /// <summary>Vector rejection — terse alias of <see cref="Rejection(Vector2)"/>.</summary>
        public Vector2 Reject(Vector2 direction) => Rejection(this, direction);

        /// <summary>Reflect <paramref name="v"/> <i>about</i> the line through <paramref name="line"/>
        /// (mirroring it across that direction), preserving magnitude.</summary>
        public static Vector2 Reflection(Vector2 v, Vector2 line) => 2f * Projection(v, line) - v;

        /// <summary>Reflect this vector about the line through <paramref name="line"/>.</summary>
        public Vector2 Reflection(Vector2 line) => Reflection(this, line);

        // ---- Interpolation ----

        /// <summary>Interpolate (or extrapolate) between two vectors.</summary>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="t"/> is outside [0, 1] and
        /// extrapolation is not allowed.</exception>
        public static Vector2 Interpolate(Vector2 a, Vector2 b, float t, bool allowExtrapolation)
        {
            if (!allowExtrapolation && (t > 1f || t < 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(t), t, "Control parameter must be a value between 0 & 1");
            }

            return new Vector2(a.X * (1f - t) + b.X * t, a.Y * (1f - t) + b.Y * t);
        }

        /// <summary>Interpolate between two vectors (t in [0, 1]).</summary>
        public static Vector2 Interpolate(Vector2 a, Vector2 b, float t) => Interpolate(a, b, t, false);

        /// <summary>Interpolate between this vector and another (t in [0, 1]).</summary>
        public Vector2 Interpolate(Vector2 other, float t) => Interpolate(this, other, t, false);

        /// <summary>Interpolate, or extrapolate, between this vector and another.</summary>
        public Vector2 Interpolate(Vector2 other, float t, bool allowExtrapolation) => Interpolate(this, other, t, allowExtrapolation);

        /// <summary>Spherically interpolate between two vectors, blending direction along the shortest arc and
        /// magnitude linearly; falls back to <see cref="Lerp"/> when (anti)parallel or either is zero.</summary>
        public static Vector2 Slerp(Vector2 a, Vector2 b, float t)
        {
            var na = a.NormalizeOrDefault();
            var nb = b.NormalizeOrDefault();

            float dot = Dot(na, nb);
            if (dot > 1f) { dot = 1f; }
            else if (dot < -1f) { dot = -1f; }

            float theta = (float)Math.Acos(dot);
            float sinTheta = (float)Math.Sin(theta);

            if (sinTheta < 1e-6f)
            {
                return Interpolate(a, b, t, true);
            }

            float wa = (float)(Math.Sin((1f - t) * theta) / sinTheta);
            float wb = (float)(Math.Sin(t * theta) / sinTheta);
            return wa * a + wb * b;
        }

        /// <summary>Spherically interpolate between this vector and another.</summary>
        public Vector2 Slerp(Vector2 other, float t) => Slerp(this, other, t);

        // ---- Component-wise min/max aliases ----

        /// <summary>The component-wise minimum — alias of <see cref="Min(Vector2, Vector2)"/>.</summary>
        public static Vector2 ComponentMin(Vector2 a, Vector2 b) => Min(a, b);

        /// <summary>The component-wise minimum of this vector and another.</summary>
        public Vector2 ComponentMin(Vector2 other) => Min(this, other);

        /// <summary>The component-wise maximum — alias of <see cref="Max(Vector2, Vector2)"/>.</summary>
        public static Vector2 ComponentMax(Vector2 a, Vector2 b) => Max(a, b);

        /// <summary>The component-wise maximum of this vector and another.</summary>
        public Vector2 ComponentMax(Vector2 other) => Max(this, other);

        // ---- Component maths ----

        /// <summary>The absolute value of each component — alias of <see cref="Abs"/>.</summary>
        public static Vector2 AbsComponents(Vector2 v) => v.Abs();

        /// <summary>The absolute value of each of this vector's components.</summary>
        public Vector2 AbsComponents() => Abs();

        /// <summary>The square root of each component.</summary>
        public static Vector2 SqrtComponents(Vector2 v) => new Vector2((float)Math.Sqrt(v.X), (float)Math.Sqrt(v.Y));

        /// <summary>The square root of each of this vector's components.</summary>
        public Vector2 SqrtComponents() => SqrtComponents(this);

        /// <summary>The square of each component.</summary>
        public static Vector2 SqrComponents(Vector2 v) => new Vector2(v.X * v.X, v.Y * v.Y);

        /// <summary>The square of each of this vector's components.</summary>
        public Vector2 SqrComponents() => SqrComponents(this);

        /// <summary>Raise each component to <paramref name="power"/>.</summary>
        public static Vector2 PowComponents(Vector2 v, float power) => new Vector2((float)Math.Pow(v.X, power), (float)Math.Pow(v.Y, power));

        /// <summary>Raise each of this vector's components to <paramref name="power"/>.</summary>
        public Vector2 PowComponents(float power) => PowComponents(this, power);

        /// <summary>The sum of the components.</summary>
        public static float SumComponents(Vector2 v) => v.X + v.Y;

        /// <summary>The sum of this vector's components.</summary>
        public float SumComponents() => SumComponents(this);

        /// <summary>The sum of the squares of the components.</summary>
        public static float SumComponentSqrs(Vector2 v) => v.X * v.X + v.Y * v.Y;

        /// <summary>The sum of the squares of this vector's components.</summary>
        public float SumComponentSqrs() => SumComponentSqrs(this);

        // ---- Rounding ----

        /// <summary>Round each component to the nearest integral value.</summary>
        public static Vector2 Round(Vector2 v) => new Vector2((float)Math.Round((double)v.X), (float)Math.Round((double)v.Y));

        /// <summary>Round each component to the given number of fractional <paramref name="digits"/>.</summary>
        public static Vector2 Round(Vector2 v, int digits) => new Vector2((float)Math.Round((double)v.X, digits), (float)Math.Round((double)v.Y, digits));

        /// <summary>Round each component to the nearest integral value using the given midpoint <paramref name="mode"/>.</summary>
        public static Vector2 Round(Vector2 v, MidpointRounding mode) => new Vector2((float)Math.Round((double)v.X, mode), (float)Math.Round((double)v.Y, mode));

        /// <summary>Round each component to the given <paramref name="digits"/> using the given midpoint <paramref name="mode"/>.</summary>
        public static Vector2 Round(Vector2 v, int digits, MidpointRounding mode) => new Vector2((float)Math.Round((double)v.X, digits, mode), (float)Math.Round((double)v.Y, digits, mode));

        /// <summary>Round each component to the nearest integral value.</summary>
        public Vector2 Round() => Round(this);

        /// <summary>Round each component to the given number of fractional <paramref name="digits"/>.</summary>
        public Vector2 Round(int digits) => Round(this, digits);

        /// <summary>Round each component to the nearest integral value using the given midpoint <paramref name="mode"/>.</summary>
        public Vector2 Round(MidpointRounding mode) => Round(this, mode);

        /// <summary>Round each component to the given <paramref name="digits"/> using the given midpoint <paramref name="mode"/>.</summary>
        public Vector2 Round(int digits, MidpointRounding mode) => Round(this, digits, mode);

        // ---- Distance instance overloads ----

        /// <summary>The distance between this vector and another.</summary>
        public float Distance(Vector2 other) => Distance(this, other);

        /// <summary>The squared distance between this vector and another.</summary>
        public float DistanceSquared(Vector2 other) => DistanceSquared(this, other);

        // ---- Decisions ----

        /// <summary>Whether this vector's length is one within the default tolerance — alias of <see cref="IsUnit"/>.</summary>
        public bool IsUnitVector() => IsUnit();

        /// <summary>Whether this vector's length is one within the given <paramref name="tolerance"/>.</summary>
        public bool IsUnitVector(float tolerance) => IsUnit(tolerance);

        /// <summary>Whether two vectors are perpendicular within <paramref name="tolerance"/> (on the normalized dot).</summary>
        public static bool IsPerpendicular(Vector2 a, Vector2 b, float tolerance = 1e-6f) =>
            Math.Abs(Dot(a.NormalizeOrDefault(), b.NormalizeOrDefault())) <= tolerance;

        /// <summary>Whether this vector is perpendicular to another within <paramref name="tolerance"/>.</summary>
        public bool IsPerpendicular(Vector2 other, float tolerance = 1e-6f) => IsPerpendicular(this, other, tolerance);

        /// <summary>Approximate equality within <paramref name="tolerance"/> — alias of <see cref="ApproximatelyEquals"/>.</summary>
        public bool Equals(Vector2 other, float tolerance) => ApproximatelyEquals(other, tolerance);

        // ---- Constants ----

        /// <summary>The smallest vector possible (each component <see cref="float.MinValue"/>).</summary>
        public static Vector2 MinValue => new Vector2(float.MinValue, float.MinValue);

        /// <summary>The largest vector possible (each component <see cref="float.MaxValue"/>).</summary>
        public static Vector2 MaxValue => new Vector2(float.MaxValue, float.MaxValue);

        /// <summary>The smallest positive (non-zero) vector possible (each component <see cref="float.Epsilon"/>).</summary>
        public static Vector2 Epsilon => new Vector2(float.Epsilon, float.Epsilon);

        /// <summary>Vector with components of NaN.</summary>
        public static Vector2 NaN => new Vector2(float.NaN, float.NaN);

        // ---- IFormattable ----

        /// <summary>Formatted textual description of the vector.</summary>
        /// <param name="format">'x', 'y' or '' followed by a standard numeric format string.</param>
        /// <param name="formatProvider">The culture specific formatting provider.</param>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Format("({0}, {1})", X, Y);
            }

            char firstChar = format![0];
            string? remainder = format.Length > 1 ? format.Substring(1) : null;

            switch (firstChar)
            {
                case 'x': return X.ToString(remainder, formatProvider);
                case 'y': return Y.ToString(remainder, formatProvider);
                default:
                    return string.Format(
                        "({0}, {1})",
                        X.ToString(format, formatProvider),
                        Y.ToString(format, formatProvider));
            }
        }

        #endregion
    }
}
