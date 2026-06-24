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
    /// (<see cref="Cross"/>): positive when <c>b</c> is to the left of <c>a</c>, negative to the right,
    /// zero when parallel. It is the 2D analogue used for winding and signed-area tests.
    /// </remarks>
    [Serializable]
    public readonly struct Vector2 : IEquatable<Vector2>
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
    }
}
