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
    public readonly struct Vector4 : IEquatable<Vector4>
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
            Math.Clamp(v.X, min.X, max.X), Math.Clamp(v.Y, min.Y, max.Y),
            Math.Clamp(v.Z, min.Z, max.Z), Math.Clamp(v.W, min.W, max.W));
        public Vector4 Abs() => new Vector4(Math.Abs(X), Math.Abs(Y), Math.Abs(Z), Math.Abs(W));
        public bool IsNaN() => float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z) || float.IsNaN(W);
        public bool IsZero(float tolerance = 0f) =>
            tolerance == 0f ? (X == 0f && Y == 0f && Z == 0f && W == 0f) : LengthSquared <= tolerance * tolerance;

        /// <summary>The XYZ part, dropping W — e.g. to read a homogeneous point's coordinates.</summary>
        public Vector3 XYZ => new Vector3(X, Y, Z);

        public static implicit operator System.Numerics.Vector4(Vector4 v) => new System.Numerics.Vector4(v.X, v.Y, v.Z, v.W);
        public static implicit operator Vector4(System.Numerics.Vector4 v) => new Vector4(v.X, v.Y, v.Z, v.W);
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
        public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
        public override string ToString() => $"({X}, {Y}, {Z}, {W})";
    }
}
