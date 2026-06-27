namespace RP.Math
{
    using System;

    using Math = System.Math;

    /// <summary>
    /// A single-precision (<see cref="float"/>) 4-component vector — RGBA colours, homogeneous coordinates
    /// (a 3D point with a <c>W</c>), and shader uniforms that pack four floats. The float sibling of
    /// <see cref="Vector3"/>; see that type for the float/double rationale.
    /// </summary>
    /// <remarks>
    /// There is no cross product in 4D (the cross product is special to three dimensions), so this type
    /// carries arithmetic, dot, length and the interpolation/clamp helpers, but not <c>Cross</c>.
    /// </remarks>
    [Serializable]
    public readonly struct Vector4 : IEquatable<Vector4>, IFormattable
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector4(float scalar) : this(scalar, scalar, scalar, scalar) { }

        /// <summary>Builds a homogeneous 4-vector from a 3-vector and a W (1 for a point, 0 for a direction).</summary>
        public Vector4(Vector3 xyz, float w) : this(xyz.X, xyz.Y, xyz.Z, w) { }

        public static Vector4 Zero => new Vector4(0f, 0f, 0f, 0f);
        public static Vector4 One => new Vector4(1f, 1f, 1f, 1f);
        public static Vector4 UnitX => new Vector4(1f, 0f, 0f, 0f);
        public static Vector4 UnitY => new Vector4(0f, 1f, 0f, 0f);
        public static Vector4 UnitZ => new Vector4(0f, 0f, 1f, 0f);
        public static Vector4 UnitW => new Vector4(0f, 0f, 0f, 1f);

        public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static Vector4 operator -(Vector4 v) => new Vector4(-v.X, -v.Y, -v.Z, -v.W);
        public static Vector4 operator *(Vector4 v, float s) => new Vector4(v.X * s, v.Y * s, v.Z * s, v.W * s);
        public static Vector4 operator *(float s, Vector4 v) => v * s;
        public static Vector4 operator *(Vector4 a, Vector4 b) => new Vector4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        public static Vector4 operator /(Vector4 v, float s) => new Vector4(v.X / s, v.Y / s, v.Z / s, v.W / s);
        public static bool operator ==(Vector4 a, Vector4 b) => a.Equals(b);
        public static bool operator !=(Vector4 a, Vector4 b) => !a.Equals(b);

        public float Length => (float)Math.Sqrt((double)X * X + (double)Y * Y + (double)Z * Z + (double)W * W);
        public float LengthSquared => X * X + Y * Y + Z * Z + W * W;

        public static float Dot(Vector4 a, Vector4 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        public float Dot(Vector4 other) => Dot(this, other);

        public Vector4 Normalize()
        {
            float len = Length;
            if (len == 0f) throw new DivideByZeroException("Cannot normalize a zero-length Vector4.");
            return this / len;
        }

        public Vector4 NormalizeOrDefault()
        {
            float len = Length;
            if (len == 0f || float.IsNaN(len) || float.IsInfinity(len)) return Zero;
            return this / len;
        }

        public static float Distance(Vector4 a, Vector4 b) => (a - b).Length;
        public static float DistanceSquared(Vector4 a, Vector4 b) => (a - b).LengthSquared;

        public static Vector4 Lerp(Vector4 a, Vector4 b, float t) => new Vector4(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t,
            a.W + (b.W - a.W) * t);

        public static Vector4 Min(Vector4 a, Vector4 b) => new Vector4(
            Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z), Math.Min(a.W, b.W));
        public static Vector4 Max(Vector4 a, Vector4 b) => new Vector4(
            Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z), Math.Max(a.W, b.W));
        public static Vector4 Clamp(Vector4 v, Vector4 min, Vector4 max) => new Vector4(
            FloatMath.Clamp(v.X, min.X, max.X), FloatMath.Clamp(v.Y, min.Y, max.Y),
            FloatMath.Clamp(v.Z, min.Z, max.Z), FloatMath.Clamp(v.W, min.W, max.W));
        public Vector4 Abs() => new Vector4(Math.Abs(X), Math.Abs(Y), Math.Abs(Z), Math.Abs(W));
        public bool IsNaN() => float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z) || float.IsNaN(W);
        public bool IsZero(float tolerance = 0f) =>
            tolerance == 0f ? (X == 0f && Y == 0f && Z == 0f && W == 0f) : LengthSquared <= tolerance * tolerance;

        /// <summary>The XYZ part, dropping W — e.g. to read a homogeneous point's coordinates.</summary>
        public Vector3 XYZ => new Vector3(X, Y, Z);

#if NET6_0_OR_GREATER
        // GPU/SIMD interop offered only on modern TFMs (see the note in Vector3) to keep netstandard2.0
        // free of the System.Numerics.Vectors dependency.
        public static implicit operator System.Numerics.Vector4(Vector4 v) => new System.Numerics.Vector4(v.X, v.Y, v.Z, v.W);
        public static implicit operator Vector4(System.Numerics.Vector4 v) => new Vector4(v.X, v.Y, v.Z, v.W);
#endif
        public static implicit operator Vector4((float x, float y, float z, float w) t) => new Vector4(t.x, t.y, t.z, t.w);
        public void Deconstruct(out float x, out float y, out float z, out float w)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        public bool Equals(Vector4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public bool ApproximatelyEquals(Vector4 other, float tolerance = 1e-5f) =>
            Math.Abs(X - other.X) <= tolerance && Math.Abs(Y - other.Y) <= tolerance &&
            Math.Abs(Z - other.Z) <= tolerance && Math.Abs(W - other.W) <= tolerance;
        public override bool Equals(object? obj) => obj is Vector4 other && Equals(other);
        public override int GetHashCode() => FloatMath.CombineHash(X, Y, Z, W);
        public override string ToString() => $"({X}, {Y}, {Z}, {W})";

        #region Vector4d-name parity and added capabilities

        // ---- Magnitude aliases ----

        /// <summary>The vector's magnitude — an alias of <see cref="Length"/> matching <see cref="Vector4d"/>.</summary>
        public float Magnitude => Length;

        /// <summary>The vector's squared magnitude — an alias of <see cref="LengthSquared"/>.</summary>
        public float MagnitudeSquared => LengthSquared;

        // ---- Dot aliases ----

        /// <summary>The dot product — alias of <see cref="Dot(Vector4, Vector4)"/> matching <see cref="Vector4d"/>.</summary>
        public static float DotProduct(Vector4 a, Vector4 b) => Dot(a, b);

        /// <summary>The dot product of this vector with another — alias of <see cref="Dot(Vector4)"/>.</summary>
        public float DotProduct(Vector4 other) => Dot(this, other);

        // ---- Projection, rejection, reflection ----

        /// <summary>The vector resolute of <paramref name="v"/> along <paramref name="direction"/>;
        /// <see cref="Zero"/> when the direction is zero.</summary>
        public static Vector4 Projection(Vector4 v, Vector4 direction)
        {
            float denom = Dot(direction, direction);
            if (denom == 0f) return Zero;
            return direction * (Dot(v, direction) / denom);
        }

        /// <summary>The vector resolute of this vector along <paramref name="direction"/>.</summary>
        public Vector4 Projection(Vector4 direction) => Projection(this, direction);

        /// <summary>Vector projection — terse alias of <see cref="Projection(Vector4, Vector4)"/>.</summary>
        public static Vector4 Project(Vector4 v, Vector4 direction) => Projection(v, direction);

        /// <summary>Vector projection — terse alias of <see cref="Projection(Vector4)"/>.</summary>
        public Vector4 Project(Vector4 direction) => Projection(this, direction);

        /// <summary>The component of <paramref name="v"/> perpendicular to <paramref name="direction"/>
        /// (so projection + rejection = original).</summary>
        public static Vector4 Rejection(Vector4 v, Vector4 direction) => v - Projection(v, direction);

        /// <summary>The component of this vector perpendicular to <paramref name="direction"/>.</summary>
        public Vector4 Rejection(Vector4 direction) => Rejection(this, direction);

        /// <summary>Vector rejection — terse alias of <see cref="Rejection(Vector4, Vector4)"/>.</summary>
        public static Vector4 Reject(Vector4 v, Vector4 direction) => Rejection(v, direction);

        /// <summary>Vector rejection — terse alias of <see cref="Rejection(Vector4)"/>.</summary>
        public Vector4 Reject(Vector4 direction) => Rejection(this, direction);

        /// <summary>Reflect a vector off a hyperplane with the given unit <paramref name="normal"/>
        /// (<c>v − 2(v·n)n</c>).</summary>
        public static Vector4 Reflect(Vector4 v, Vector4 normal) => v - 2f * Dot(v, normal) * normal;

        /// <summary>Reflect this vector off a hyperplane with the given unit <paramref name="normal"/>.</summary>
        public Vector4 Reflect(Vector4 normal) => Reflect(this, normal);

        /// <summary>Reflect <paramref name="v"/> <i>about</i> the line through <paramref name="line"/>
        /// (mirroring it across that direction), preserving magnitude.</summary>
        public static Vector4 Reflection(Vector4 v, Vector4 line) => 2f * Projection(v, line) - v;

        /// <summary>Reflect this vector about the line through <paramref name="line"/>.</summary>
        public Vector4 Reflection(Vector4 line) => Reflection(this, line);

        // ---- Interpolation ----

        /// <summary>Interpolate (or extrapolate) between two vectors.</summary>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="t"/> is outside [0, 1] and
        /// extrapolation is not allowed.</exception>
        public static Vector4 Interpolate(Vector4 a, Vector4 b, float t, bool allowExtrapolation)
        {
            if (!allowExtrapolation && (t > 1f || t < 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(t), t, "Control parameter must be a value between 0 & 1");
            }

            return new Vector4(
                a.X * (1f - t) + b.X * t,
                a.Y * (1f - t) + b.Y * t,
                a.Z * (1f - t) + b.Z * t,
                a.W * (1f - t) + b.W * t);
        }

        /// <summary>Interpolate between two vectors (t in [0, 1]).</summary>
        public static Vector4 Interpolate(Vector4 a, Vector4 b, float t) => Interpolate(a, b, t, false);

        /// <summary>Interpolate between this vector and another (t in [0, 1]).</summary>
        public Vector4 Interpolate(Vector4 other, float t) => Interpolate(this, other, t, false);

        /// <summary>Interpolate, or extrapolate, between this vector and another.</summary>
        public Vector4 Interpolate(Vector4 other, float t, bool allowExtrapolation) => Interpolate(this, other, t, allowExtrapolation);

        /// <summary>Spherically interpolate between two vectors, blending direction along the shortest arc and
        /// magnitude linearly; falls back to <see cref="Lerp"/> when (anti)parallel or either is zero.</summary>
        public static Vector4 Slerp(Vector4 a, Vector4 b, float t)
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
        public Vector4 Slerp(Vector4 other, float t) => Slerp(this, other, t);

        // ---- Component-wise min/max aliases ----

        /// <summary>The component-wise minimum — alias of <see cref="Min(Vector4, Vector4)"/>.</summary>
        public static Vector4 ComponentMin(Vector4 a, Vector4 b) => Min(a, b);

        /// <summary>The component-wise minimum of this vector and another.</summary>
        public Vector4 ComponentMin(Vector4 other) => Min(this, other);

        /// <summary>The component-wise maximum — alias of <see cref="Max(Vector4, Vector4)"/>.</summary>
        public static Vector4 ComponentMax(Vector4 a, Vector4 b) => Max(a, b);

        /// <summary>The component-wise maximum of this vector and another.</summary>
        public Vector4 ComponentMax(Vector4 other) => Max(this, other);

        // ---- Component maths ----

        /// <summary>The absolute value of each component — alias of <see cref="Abs"/>.</summary>
        public static Vector4 AbsComponents(Vector4 v) => v.Abs();

        /// <summary>The absolute value of each of this vector's components.</summary>
        public Vector4 AbsComponents() => Abs();

        /// <summary>The square root of each component.</summary>
        public static Vector4 SqrtComponents(Vector4 v) => new Vector4((float)Math.Sqrt(v.X), (float)Math.Sqrt(v.Y), (float)Math.Sqrt(v.Z), (float)Math.Sqrt(v.W));

        /// <summary>The square root of each of this vector's components.</summary>
        public Vector4 SqrtComponents() => SqrtComponents(this);

        /// <summary>The square of each component.</summary>
        public static Vector4 SqrComponents(Vector4 v) => new Vector4(v.X * v.X, v.Y * v.Y, v.Z * v.Z, v.W * v.W);

        /// <summary>The square of each of this vector's components.</summary>
        public Vector4 SqrComponents() => SqrComponents(this);

        /// <summary>Raise each component to <paramref name="power"/>.</summary>
        public static Vector4 PowComponents(Vector4 v, float power) => new Vector4((float)Math.Pow(v.X, power), (float)Math.Pow(v.Y, power), (float)Math.Pow(v.Z, power), (float)Math.Pow(v.W, power));

        /// <summary>Raise each of this vector's components to <paramref name="power"/>.</summary>
        public Vector4 PowComponents(float power) => PowComponents(this, power);

        /// <summary>The sum of the components.</summary>
        public static float SumComponents(Vector4 v) => v.X + v.Y + v.Z + v.W;

        /// <summary>The sum of this vector's components.</summary>
        public float SumComponents() => SumComponents(this);

        /// <summary>The sum of the squares of the components.</summary>
        public static float SumComponentSqrs(Vector4 v) => v.X * v.X + v.Y * v.Y + v.Z * v.Z + v.W * v.W;

        /// <summary>The sum of the squares of this vector's components.</summary>
        public float SumComponentSqrs() => SumComponentSqrs(this);

        // ---- Rounding ----

        /// <summary>Round each component to the nearest integral value.</summary>
        public static Vector4 Round(Vector4 v) => new Vector4((float)Math.Round((double)v.X), (float)Math.Round((double)v.Y), (float)Math.Round((double)v.Z), (float)Math.Round((double)v.W));

        /// <summary>Round each component to the given number of fractional <paramref name="digits"/>.</summary>
        public static Vector4 Round(Vector4 v, int digits) => new Vector4((float)Math.Round((double)v.X, digits), (float)Math.Round((double)v.Y, digits), (float)Math.Round((double)v.Z, digits), (float)Math.Round((double)v.W, digits));

        /// <summary>Round each component to the nearest integral value using the given midpoint <paramref name="mode"/>.</summary>
        public static Vector4 Round(Vector4 v, MidpointRounding mode) => new Vector4((float)Math.Round((double)v.X, mode), (float)Math.Round((double)v.Y, mode), (float)Math.Round((double)v.Z, mode), (float)Math.Round((double)v.W, mode));

        /// <summary>Round each component to the given <paramref name="digits"/> using the given midpoint <paramref name="mode"/>.</summary>
        public static Vector4 Round(Vector4 v, int digits, MidpointRounding mode) => new Vector4((float)Math.Round((double)v.X, digits, mode), (float)Math.Round((double)v.Y, digits, mode), (float)Math.Round((double)v.Z, digits, mode), (float)Math.Round((double)v.W, digits, mode));

        /// <summary>Round each component to the nearest integral value.</summary>
        public Vector4 Round() => Round(this);

        /// <summary>Round each component to the given number of fractional <paramref name="digits"/>.</summary>
        public Vector4 Round(int digits) => Round(this, digits);

        /// <summary>Round each component to the nearest integral value using the given midpoint <paramref name="mode"/>.</summary>
        public Vector4 Round(MidpointRounding mode) => Round(this, mode);

        /// <summary>Round each component to the given <paramref name="digits"/> using the given midpoint <paramref name="mode"/>.</summary>
        public Vector4 Round(int digits, MidpointRounding mode) => Round(this, digits, mode);

        // ---- Distance instance overloads ----

        /// <summary>The distance between this vector and another.</summary>
        public float Distance(Vector4 other) => Distance(this, other);

        /// <summary>The squared distance between this vector and another.</summary>
        public float DistanceSquared(Vector4 other) => DistanceSquared(this, other);

        // ---- ClampMagnitude and MoveTowards (gaps relative to Vector2/Vector3) ----

        /// <summary>Caps the vector's length at <paramref name="maxLength"/> while keeping its direction.</summary>
        public Vector4 ClampMagnitude(float maxLength)
        {
            float lenSq = LengthSquared;
            if (lenSq <= maxLength * maxLength) return this;
            return this / (float)Math.Sqrt(lenSq) * maxLength;
        }

        /// <summary>Moves <paramref name="current"/> toward <paramref name="target"/> by at most
        /// <paramref name="maxDistance"/>, never overshooting.</summary>
        public static Vector4 MoveTowards(Vector4 current, Vector4 target, float maxDistance)
        {
            Vector4 delta = target - current;
            float dist = delta.Length;
            if (dist <= maxDistance || dist == 0f) return target;
            return current + delta / dist * maxDistance;
        }

        /// <summary>Moves this vector toward <paramref name="target"/> by at most <paramref name="maxDistance"/>.</summary>
        public Vector4 MoveTowards(Vector4 target, float maxDistance) => MoveTowards(this, target, maxDistance);

        // ---- Decisions ----

        /// <summary>True if the vector's length is 1 within <paramref name="tolerance"/>.</summary>
        public bool IsUnit(float tolerance = 1e-5f) => Math.Abs(LengthSquared - 1f) <= tolerance;

        /// <summary>Whether this vector's length is one within the default tolerance — alias of <see cref="IsUnit"/>.</summary>
        public bool IsUnitVector() => IsUnit();

        /// <summary>Whether this vector's length is one within the given <paramref name="tolerance"/>.</summary>
        public bool IsUnitVector(float tolerance) => IsUnit(tolerance);

        /// <summary>Whether two vectors are perpendicular within <paramref name="tolerance"/> (on the normalized dot).</summary>
        public static bool IsPerpendicular(Vector4 a, Vector4 b, float tolerance = 1e-6f) =>
            Math.Abs(Dot(a.NormalizeOrDefault(), b.NormalizeOrDefault())) <= tolerance;

        /// <summary>Whether this vector is perpendicular to another within <paramref name="tolerance"/>.</summary>
        public bool IsPerpendicular(Vector4 other, float tolerance = 1e-6f) => IsPerpendicular(this, other, tolerance);

        /// <summary>Approximate equality within <paramref name="tolerance"/> — alias of <see cref="ApproximatelyEquals"/>.</summary>
        public bool Equals(Vector4 other, float tolerance) => ApproximatelyEquals(other, tolerance);

        // ---- Homogeneous coordinates ----

        /// <summary>The perspective divide: returns the <see cref="Vector3"/> <c>(x/w, y/w, z/w)</c>, mapping a
        /// homogeneous point back to ordinary 3D space. Use <see cref="XYZ"/> to drop <c>w</c> without dividing.</summary>
        public Vector3 Dehomogenize() => new Vector3(X / W, Y / W, Z / W);

        // ---- Constants ----

        /// <summary>The smallest vector possible (each component <see cref="float.MinValue"/>).</summary>
        public static Vector4 MinValue => new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);

        /// <summary>The largest vector possible (each component <see cref="float.MaxValue"/>).</summary>
        public static Vector4 MaxValue => new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);

        /// <summary>The smallest positive (non-zero) vector possible (each component <see cref="float.Epsilon"/>).</summary>
        public static Vector4 Epsilon => new Vector4(float.Epsilon, float.Epsilon, float.Epsilon, float.Epsilon);

        /// <summary>Vector with components of NaN.</summary>
        public static Vector4 NaN => new Vector4(float.NaN, float.NaN, float.NaN, float.NaN);

        // ---- IFormattable ----

        /// <summary>Formatted textual description of the vector.</summary>
        /// <param name="format">'x', 'y', 'z', 'w' or '' followed by a standard numeric format string.</param>
        /// <param name="formatProvider">The culture specific formatting provider.</param>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Format("({0}, {1}, {2}, {3})", X, Y, Z, W);
            }

            char firstChar = format![0];
            string? remainder = format.Length > 1 ? format.Substring(1) : null;

            switch (firstChar)
            {
                case 'x': return X.ToString(remainder, formatProvider);
                case 'y': return Y.ToString(remainder, formatProvider);
                case 'z': return Z.ToString(remainder, formatProvider);
                case 'w': return W.ToString(remainder, formatProvider);
                default:
                    return string.Format(
                        "({0}, {1}, {2}, {3})",
                        X.ToString(format, formatProvider),
                        Y.ToString(format, formatProvider),
                        Z.ToString(format, formatProvider),
                        W.ToString(format, formatProvider));
            }
        }

        #endregion
    }
}
