namespace RP.Math
{
    using System;

    /// <summary>
    /// A moving reference frame at a point on a curve: the position together with the three mutually
    /// perpendicular unit vectors of the <b>Frenet frame</b> — the <see cref="Tangent"/> (the way the curve
    /// is heading), the <see cref="Normal"/> (the way it is turning), and the <see cref="Binormal"/>
    /// (perpendicular to both). It is exactly what you need to orient an object as it travels along a curve.
    /// </summary>
    /// <remarks>
    /// The three axes form a right-handed basis: <c>Binormal = Tangent × Normal</c>. Where the curve is
    /// momentarily straight (zero curvature) the turning direction is undefined, so an arbitrary — but still
    /// perpendicular and consistent — <see cref="Normal"/> is chosen.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public readonly struct CurveFrame : IEquatable<CurveFrame>
    {
        /// <summary>The point on the curve this frame sits at.</summary>
        public Vector Position { get; }

        /// <summary>The unit tangent — the direction the curve is heading.</summary>
        public Vector Tangent { get; }

        /// <summary>The unit principal normal — the direction the curve is turning toward.</summary>
        public Vector Normal { get; }

        /// <summary>The unit binormal, <c>Tangent × Normal</c>, completing the right-handed frame.</summary>
        public Vector Binormal { get; }

        /// <summary>Construct a frame from its position and three (assumed unit, perpendicular) axes.</summary>
        public CurveFrame(Vector position, Vector tangent, Vector normal, Vector binormal)
        {
            this.Position = position;
            this.Tangent = tangent;
            this.Normal = normal;
            this.Binormal = binormal;
        }

        /// <summary>Equality of position and all three axes.</summary>
        public bool Equals(CurveFrame other)
        {
            return this.Position == other.Position
                && this.Tangent == other.Tangent
                && this.Normal == other.Normal
                && this.Binormal == other.Binormal;
        }

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? obj) => obj is CurveFrame f && this.Equals(f);

        /// <summary>A hash code derived from the position and axes.</summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = this.Position.GetHashCode();
                hash = (hash * 397) ^ this.Tangent.GetHashCode();
                hash = (hash * 397) ^ this.Normal.GetHashCode();
                hash = (hash * 397) ^ this.Binormal.GetHashCode();
                return hash;
            }
        }

        /// <summary>A string of the form <c>CurveFrame[pos; T=…, N=…, B=…]</c>.</summary>
        public override string ToString()
        {
            return string.Format("CurveFrame[{0}; T={1}, N={2}, B={3}]", this.Position, this.Tangent, this.Normal, this.Binormal);
        }
    }
}
