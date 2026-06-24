namespace RP.Math
{
    using System;

    using Math = System.Math;

    /// <summary>
    /// A single-precision (<see cref="float"/>) 3-component vector — the workhorse for data that lives on
    /// the GPU (vertex positions, normals) and for rebased, camera-relative simulation positions.
    /// </summary>
    /// <remarks>
    /// <para><b>Why a float sibling of <see cref="Vector3d"/>.</b> The maths library's "truth" type is the
    /// double <see cref="Vector3d"/>: doubles avoid the precision traps that wreck a kilometre-scale world.
    /// But a GPU vertex buffer is 32-bit floats, and rendering/physics near the camera fit comfortably in
    /// float once the world has been rebased around the player (the floating-origin scheme). So the engine
    /// simulates and stores positions in <see cref="Vector3d"/> and converts to <see cref="Vector3"/> only
    /// at the edge — which is exactly the conversions this type provides (narrowing is explicit, widening
    /// implicit, so you never lose precision by accident).</para>
    ///
    /// <para>Like the rest of RP.Math this is an <b>immutable value type</b>: every operation returns a new
    /// vector. Conventions match <see cref="Vector3d"/> — right-handed cross product, radians, and the same
    /// "<c>…OrDefault</c>" safety pattern for the degenerate zero-length case.</para>
    /// </remarks>
    [Serializable]
    public readonly struct Vector3 : IEquatable<Vector3>
    {
        /// <summary>The X component.</summary>
        public readonly float X;

        /// <summary>The Y component.</summary>
        public readonly float Y;

        /// <summary>The Z component.</summary>
        public readonly float Z;

        /// <summary>Constructs a vector from its three components.</summary>
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Constructs a vector with all three components set to the same value.</summary>
        public Vector3(float scalar) : this(scalar, scalar, scalar) { }

        /// <summary>Constructs a vector from a 2-vector and a Z component.</summary>
        public Vector3(Vector2 xy, float z) : this(xy.X, xy.Y, z) { }

        #region Constants

        /// <summary>(0, 0, 0).</summary>
        public static Vector3 Zero => new Vector3(0f, 0f, 0f);

        /// <summary>(1, 1, 1).</summary>
        public static Vector3 One => new Vector3(1f, 1f, 1f);

        /// <summary>(1, 0, 0).</summary>
        public static Vector3 UnitX => new Vector3(1f, 0f, 0f);

        /// <summary>(0, 1, 0).</summary>
        public static Vector3 UnitY => new Vector3(0f, 1f, 0f);

        /// <summary>(0, 0, 1).</summary>
        public static Vector3 UnitZ => new Vector3(0f, 0f, 1f);

        #endregion

        #region Arithmetic operators

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        /// <summary>Negation — reverses direction.</summary>
        public static Vector3 operator -(Vector3 v) => new Vector3(-v.X, -v.Y, -v.Z);

        public static Vector3 operator *(Vector3 v, float s) => new Vector3(v.X * s, v.Y * s, v.Z * s);

        public static Vector3 operator *(float s, Vector3 v) => v * s;

        /// <summary>Component-wise (Hadamard) product — handy for non-uniform scaling.</summary>
        public static Vector3 operator *(Vector3 a, Vector3 b) => new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);

        public static Vector3 operator /(Vector3 v, float s) => new Vector3(v.X / s, v.Y / s, v.Z / s);

        public static bool operator ==(Vector3 a, Vector3 b) => a.Equals(b);

        public static bool operator !=(Vector3 a, Vector3 b) => !a.Equals(b);

        #endregion

        #region Length, products, normalisation

        /// <summary>The vector's length (magnitude). Involves a square root; prefer
        /// <see cref="LengthSquared"/> when only comparing lengths.</summary>
        public float Length => (float)Math.Sqrt((double)X * X + (double)Y * Y + (double)Z * Z);

        /// <summary>The squared length — the cheaper quantity to compare, since √ preserves ordering.</summary>
        public float LengthSquared => X * X + Y * Y + Z * Z;

        /// <summary>Dot product: <c>|a||b|cosθ</c>. Zero when perpendicular; sign tells you which side.</summary>
        public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        /// <summary>The dot product of this vector with another.</summary>
        public float Dot(Vector3 other) => Dot(this, other);

        /// <summary>Cross product: a vector perpendicular to both inputs, right-handed (a × b).</summary>
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

        /// <summary>The cross product of this vector with another (this × other).</summary>
        public Vector3 Cross(Vector3 other) => Cross(this, other);

        /// <summary>
        /// Returns a unit-length vector in the same direction.
        /// </summary>
        /// <exception cref="DivideByZeroException">If the vector has zero length — a zero vector has no
        /// direction to preserve. Use <see cref="NormalizeOrDefault"/> for the safe form.</exception>
        public Vector3 Normalize()
        {
            float len = Length;
            if (len == 0f) throw new DivideByZeroException("Cannot normalize a zero-length Vector3.");
            return this / len;
        }

        /// <summary>
        /// Returns a unit-length vector in the same direction, or <see cref="Zero"/> for a zero (or
        /// non-finite) input. The safe counterpart to <see cref="Normalize"/>, matching RP.Math's
        /// <c>…OrDefault</c> convention.
        /// </summary>
        public Vector3 NormalizeOrDefault()
        {
            float len = Length;
            if (len == 0f || float.IsNaN(len) || float.IsInfinity(len)) return Zero;
            return this / len;
        }

        /// <summary>True if the vector's length is 1 within <paramref name="tolerance"/>.</summary>
        public bool IsUnit(float tolerance = 1e-5f) => Math.Abs(LengthSquared - 1f) <= tolerance;

        #endregion

        #region Distance, interpolation, geometry

        /// <summary>Straight-line distance between two points.</summary>
        public static float Distance(Vector3 a, Vector3 b) => (a - b).Length;

        /// <summary>Squared distance — cheaper when only comparing distances.</summary>
        public static float DistanceSquared(Vector3 a, Vector3 b) => (a - b).LengthSquared;

        /// <summary>
        /// Linear interpolation from <paramref name="a"/> to <paramref name="b"/>. <paramref name="t"/> = 0
        /// returns <paramref name="a"/>, 1 returns <paramref name="b"/>; values outside [0,1] extrapolate.
        /// </summary>
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => new Vector3(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t);

        /// <summary>The angle between two vectors, in radians, in [0, π]. Uses the numerically stable
        /// <c>atan2(|a×b|, a·b)</c> form (the same fix as <see cref="Vector3d"/>), so it never returns NaN
        /// for (anti)parallel inputs.</summary>
        public static float Angle(Vector3 a, Vector3 b)
        {
            float crossLen = Cross(a, b).Length;
            float dot = Dot(a, b);
            return (float)Math.Atan2(crossLen, dot);
        }

        /// <summary>
        /// Reflects this vector about a surface with the given <paramref name="normal"/> (the classic
        /// "bounce": angle of incidence equals angle of reflection). The normal should be unit length.
        /// </summary>
        public Vector3 Reflect(Vector3 normal) => this - 2f * Dot(this, normal) * normal;

        /// <summary>The component of this vector along <paramref name="direction"/> (its "shadow").</summary>
        public Vector3 Project(Vector3 direction)
        {
            float denom = Dot(direction, direction);
            if (denom == 0f) return Zero;
            return direction * (Dot(this, direction) / denom);
        }

        /// <summary>The component of this vector perpendicular to <paramref name="direction"/>
        /// (so <c>Project + Reject == original</c>).</summary>
        public Vector3 Reject(Vector3 direction) => this - Project(direction);

        /// <summary>Moves <paramref name="current"/> toward <paramref name="target"/> by at most
        /// <paramref name="maxDistance"/>, never overshooting — the staple of frame-by-frame movement.</summary>
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistance)
        {
            Vector3 delta = target - current;
            float dist = delta.Length;
            if (dist <= maxDistance || dist == 0f) return target;
            return current + delta / dist * maxDistance;
        }

        /// <summary>Caps the vector's length at <paramref name="maxLength"/> while keeping its direction.</summary>
        public Vector3 ClampMagnitude(float maxLength)
        {
            float lenSq = LengthSquared;
            if (lenSq <= maxLength * maxLength) return this;
            return this / (float)Math.Sqrt(lenSq) * maxLength;
        }

        #endregion

        #region Component-wise helpers

        /// <summary>Per-axis minimum of two vectors (one corner of their bounding box).</summary>
        public static Vector3 Min(Vector3 a, Vector3 b) => new Vector3(
            Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));

        /// <summary>Per-axis maximum of two vectors (the opposite corner of their bounding box).</summary>
        public static Vector3 Max(Vector3 a, Vector3 b) => new Vector3(
            Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));

        /// <summary>Clamps each component into the box defined by <paramref name="min"/>/<paramref name="max"/>.</summary>
        public static Vector3 Clamp(Vector3 v, Vector3 min, Vector3 max) => new Vector3(
            Math.Clamp(v.X, min.X, max.X), Math.Clamp(v.Y, min.Y, max.Y), Math.Clamp(v.Z, min.Z, max.Z));

        /// <summary>Per-component absolute value.</summary>
        public Vector3 Abs() => new Vector3(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));

        /// <summary>True if any component is NaN.</summary>
        public bool IsNaN() => float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z);

        /// <summary>True if the vector is exactly (or within <paramref name="tolerance"/> of) zero.</summary>
        public bool IsZero(float tolerance = 0f) =>
            tolerance == 0f ? (X == 0f && Y == 0f && Z == 0f) : LengthSquared <= tolerance * tolerance;

        #endregion

        #region Conversions

        /// <summary>Widening to the double <see cref="Vector3d"/> is implicit — it never loses precision.</summary>
        public static implicit operator Vector3d(Vector3 v) => new Vector3d(v.X, v.Y, v.Z);

        /// <summary>Narrowing from the double <see cref="Vector3d"/> is explicit — you accept the precision
        /// loss by writing the cast. This is the deliberate "edge of the world" conversion before GPU upload.</summary>
        public static explicit operator Vector3(Vector3d v) => new Vector3((float)v.X, (float)v.Y, (float)v.Z);

        /// <summary>Interop with <see cref="System.Numerics.Vector3"/> (same precision) for GPU upload and
        /// SIMD paths — lossless, so implicit both ways.</summary>
        public static implicit operator System.Numerics.Vector3(Vector3 v) => new System.Numerics.Vector3(v.X, v.Y, v.Z);

        /// <summary>Interop from <see cref="System.Numerics.Vector3"/>.</summary>
        public static implicit operator Vector3(System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);

        /// <summary>Repack from a tuple.</summary>
        public static implicit operator Vector3((float x, float y, float z) t) => new Vector3(t.x, t.y, t.z);

        /// <summary>Deconstruction into components.</summary>
        public void Deconstruct(out float x, out float y, out float z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        #endregion

        #region Equality, formatting

        /// <summary>Exact component equality. For floating point you usually want
        /// <see cref="ApproximatelyEquals"/> instead.</summary>
        public bool Equals(Vector3 other) => X == other.X && Y == other.Y && Z == other.Z;

        /// <summary>True if every component is within <paramref name="tolerance"/> of <paramref name="other"/>.</summary>
        public bool ApproximatelyEquals(Vector3 other, float tolerance = 1e-5f) =>
            Math.Abs(X - other.X) <= tolerance &&
            Math.Abs(Y - other.Y) <= tolerance &&
            Math.Abs(Z - other.Z) <= tolerance;

        public override bool Equals(object? obj) => obj is Vector3 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public override string ToString() => $"({X}, {Y}, {Z})";

        #endregion
    }
}
