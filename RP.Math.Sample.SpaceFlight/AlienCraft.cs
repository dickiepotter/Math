namespace RP.Math.Sample.SpaceFlight
{
    using System;
    using Microsoft.Xna.Framework.Graphics;

    using RVector = RP.Math.Vector3d;

    /// <summary>
    /// One alien spacecraft: a procedurally-built mesh, a current <see cref="Pose"/>, a motion behaviour,
    /// and collision queries that defer to the matching RP.Math placed shape. The shape kind (sphere,
    /// cuboid, cylinder, cone, capsule, torus, ellipsoid) is captured by the injected delegates, so this
    /// class stays agnostic of which one it is.
    /// </summary>
    internal sealed class AlienCraft
    {
        public string Name { get; }
        public VertexPositionColor[] Mesh { get; }
        public Pose Pose { get; set; }

        /// <summary>Broad-phase bounding radius in local space (for ray targeting / proximity).</summary>
        public double BoundRadius { get; }

        private readonly Func<Pose, RVector, double> _distanceTo;
        private readonly Func<Pose, RVector, RVector> _closestPoint;
        private readonly Action<AlienCraft, double, double> _update;

        /// <summary>The patrol parameter t (0..1) for craft that follow a curve; otherwise NaN.</summary>
        public double CurveParameter { get; set; } = double.NaN;

        public AlienCraft(
            string name,
            VertexPositionColor[] mesh,
            Pose initialPose,
            double boundRadius,
            Func<Pose, RVector, double> distanceTo,
            Func<Pose, RVector, RVector> closestPoint,
            Action<AlienCraft, double, double> update)
        {
            Name = name;
            Mesh = mesh;
            Pose = initialPose;
            BoundRadius = boundRadius;
            _distanceTo = distanceTo;
            _closestPoint = closestPoint;
            _update = update;
        }

        /// <summary>Distance from <paramref name="worldPoint"/> to this craft's hull (0 if inside), via its placed shape.</summary>
        public double DistanceTo(RVector worldPoint) => _distanceTo(Pose, worldPoint);

        /// <summary>The closest point on/in this craft's hull to <paramref name="worldPoint"/>, via its placed shape.</summary>
        public RVector ClosestPoint(RVector worldPoint) => _closestPoint(Pose, worldPoint);

        /// <summary>The broad-phase bounding sphere in world space.</summary>
        public BoundingSphere Bounds => new BoundingSphere(Pose.Position, BoundRadius);

        public void Update(double totalSeconds, double dt) => _update(this, totalSeconds, dt);
    }
}
