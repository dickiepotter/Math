namespace RP.Math.Sample.SpaceFlight
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using RP.Math;                                   // Vector, Matrix, Quaternion, Pose, Angle, shapes, curves ...
    using Xna = Microsoft.Xna.Framework;             // Game, GameTime, GraphicsDeviceManager
    using XColor = Microsoft.Xna.Framework.Color;
    using XMatrix = Microsoft.Xna.Framework.Matrix;
    using XVec2 = Microsoft.Xna.Framework.Vector2;

    /// <summary>
    /// A 3rd-person "fly through the alien spacecraft" scene whose every spatial calculation — camera,
    /// orientation, motion, collision, targeting and even the projection matrix — is done with RP.Math.
    /// MonoGame (Direct3D 11) only supplies the window, the input and the triangle rasteriser.
    /// </summary>
    internal sealed class SpaceGame : Xna.Game
    {
        // The coordinate convention the flight model lives in. MathsYUp == OpenGL: right-handed, Y up,
        // forward = -Z. It matches RP.Math's right-handed LookAt / perspective builders. (The library also
        // ships OrthogonalAxes.DirectX — left-handed, +Z into the screen — but pairing that with the
        // right-handed view/projection here would mirror the world, so we stay in one consistent frame.)
        private static readonly OrthogonalAxes Convention = OrthogonalAxes.MathsYUp;
        private const string ConventionName = "MathsYUp / OpenGL";

        private readonly Xna.GraphicsDeviceManager _gdm;
        private SpriteBatch _sprite = null!;
        private BasicEffect _effect = null!;
        private BitmapFont _font = null!;

        // --- Player (a rigid Pose) + a velocity vector -------------------------------------------------
        private Pose _player = new Pose(new Vector(0, 3, 34), Quaternion.Identity);
        private Vector _velocity = Vector.Zero;
        private const double MaxSpeed = 30.0;
        private const double Accel = 26.0;
        private const double PlayerRadius = 1.1;

        // --- Smoothed chase camera ---------------------------------------------------------------------
        private Vector _camEye = new Vector(0, 8, 46);
        private Vector _camTarget = Vector.Zero;

        // --- Scene -------------------------------------------------------------------------------------
        private readonly List<AlienCraft> _craft = new();
        private VertexPositionColor[] _grid = null!;
        private VertexPositionColor[] _arenaWire = null!;
        private VertexPositionColor[] _playerMesh = null!;
        private BoundingBox _arena;
        private CatmullRom _saucerPath = null!;
        private Bezier _dartPath = null!;
        private static readonly Plane Scanner = Plane.XZ; // the y = 0 reference plane drawn as the grid

        // --- Targeting / collision read-outs for the HUD ----------------------------------------------
        private Vector _laserStart, _laserEnd;
        private bool _laserHit;
        private string _targetName = "(none)";
        private double _targetDist;
        private bool _colliding;
        private string _collideName = "";

        public SpaceGame()
        {
            _gdm = new Xna.GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720,
                PreferMultiSampling = true,
            };
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            Window.Title = "RP.Math — Alien Spacecraft Fly-Through (MonoGame / Direct3D 11)";
            Content.RootDirectory = "Content"; // unused; kept so the framework is happy
        }

        protected override void Initialize()
        {
            BuildScene();
            base.Initialize(); // triggers LoadContent
        }

        protected override void LoadContent()
        {
            _sprite = new SpriteBatch(GraphicsDevice);
            _font = new BitmapFont(GraphicsDevice, 16f);
            _effect = new BasicEffect(GraphicsDevice) { VertexColorEnabled = true, LightingEnabled = false };
            _playerMesh = MeshFactory.PlayerDart(new XColor(190, 210, 255), new XColor(120, 150, 235));
        }

        // ----------------------------------------------------------------------------------------------
        //  Scene construction: each craft is a different RP.Math placed shape, wired to its own
        //  distance / closest-point queries and a motion behaviour.
        // ----------------------------------------------------------------------------------------------
        private void BuildScene()
        {
            // 1. Monolith — a slowly yawing PlacedCuboid.
            {
                var box = new Cuboid(4, 7, 1.5);
                double br = 0.5 * Math.Sqrt(4 * 4 + 7 * 7 + 1.5 * 1.5);
                _craft.Add(new AlienCraft(
                    "Monolith", MeshFactory.Cuboid(4, 7, 1.5, new XColor(150, 130, 170)),
                    new Pose(new Vector(-16, 3, -8), Quaternion.Identity), br,
                    (pose, p) => new PlacedCuboid(box, pose).DistanceTo(p),
                    (pose, p) => new PlacedCuboid(box, pose).ClosestPoint(p),
                    (c, t, dt) => c.Pose = c.Pose.RotateBy(Quaternion.FromAxisAngle(Convention.Up, new Angle(0.25 * dt)))));
            }

            // 2. Scout — a hovering, bobbing PlacedSphere.
            {
                var sphere = new Sphere(2.4);
                var home = new Vector(12, 5, -16);
                _craft.Add(new AlienCraft(
                    "Scout", MeshFactory.Sphere(2.4, new XColor(120, 200, 160)),
                    new Pose(home, Quaternion.Identity), 2.4,
                    (pose, p) => new PlacedSphere(sphere, pose).DistanceTo(p),
                    (pose, p) => new PlacedSphere(sphere, pose).ClosestSurfacePoint(p),
                    (c, t, dt) => c.Pose = c.Pose.WithPosition(home + new Vector(0, 1.6 * Math.Sin(t * 0.9), 0))));
            }

            // 3. Drum — a tumbling PlacedCylinder driven by Euler angles (Rotation).
            {
                var cyl = new Cylinder(2.3, 6.0);
                var home = new Vector(18, 2, 8);
                _craft.Add(new AlienCraft(
                    "Drum", MeshFactory.Cylinder(2.3, 6.0, new XColor(210, 170, 110)),
                    new Pose(home, Quaternion.Identity), 0.5 * Math.Sqrt(6 * 6 + (4.6 * 4.6)),
                    (pose, p) => new PlacedCylinder(cyl, pose).DistanceTo(p),
                    (pose, p) => new PlacedCylinder(cyl, pose).ClosestPoint(p),
                    (c, t, dt) => c.Pose = c.Pose.WithRotation(
                        new Rotation(new Angle(t * 0.6), Angle.Zero_Angle, new Angle(t * 0.35)).ToQuaternion())));
            }

            // 4. Pod — a gently bobbing, tilted PlacedCapsule.
            {
                var cap = new Capsule(1.7, 4.0);
                var home = new Vector(-10, 6, 16);
                var tilt = Quaternion.FromAxisAngle(Convention.Right, new Angle(55, AngleUnits.DEG));
                _craft.Add(new AlienCraft(
                    "Pod", MeshFactory.Capsule(1.7, 4.0, new XColor(160, 190, 210)),
                    new Pose(home, tilt), 0.5 * 4.0 + 1.7,
                    (pose, p) => new PlacedCapsule(cap, pose).DistanceTo(p),
                    (pose, p) => new PlacedCapsule(cap, pose).ClosestPoint(p),
                    (c, t, dt) => c.Pose = c.Pose.WithPosition(home + Convention.Up * (1.2 * Math.Sin(t * 1.1 + 1.0)))));
            }

            // 5. Ring Station — a spinning, tilted PlacedTorus (spins about its OWN +Z axis).
            {
                var torus = new Torus(5.0, 1.2);
                var home = new Vector(0, 9, -24);
                var tilt = Quaternion.FromAxisAngle(Convention.Right, new Angle(70, AngleUnits.DEG));
                _craft.Add(new AlienCraft(
                    "Ring Station", MeshFactory.Torus(5.0, 1.2, new XColor(170, 200, 235)),
                    new Pose(home, tilt), 5.0 + 1.2,
                    (pose, p) => new PlacedTorus(torus, pose).DistanceTo(p),
                    (pose, p) => new PlacedTorus(torus, pose).ClosestPoint(p),
                    (c, t, dt) => c.Pose = new Pose(home, tilt * Quaternion.FromAxisAngle(new Vector(0, 0, 1), new Angle(t * 0.8)))));
            }

            // 6. Saucer — a PlacedEllipsoid gliding a closed Catmull-Rom loop, banking along the tangent.
            {
                var ell = new Ellipsoid(3.0, 0.9, 3.0);
                _saucerPath = new CatmullRom(
                    new Vector(-22, 7, -10), new Vector(-12, 10, 12), new Vector(8, 6, 20),
                    new Vector(22, 11, 4), new Vector(16, 8, -18), new Vector(-4, 12, -22),
                    new Vector(-22, 7, -10), new Vector(-12, 10, 12)); // wrap points to close the loop
                _craft.Add(new AlienCraft(
                    "Saucer", MeshFactory.Ellipsoid(3.0, 0.9, 3.0, new XColor(120, 220, 230)),
                    new Pose(_saucerPath.PointAt(0), Quaternion.Identity), 3.0,
                    (pose, p) => new PlacedEllipsoid(ell, pose).DistanceTo(p),
                    (pose, p) => new PlacedEllipsoid(ell, pose).ClosestPoint(p),
                    (c, t, dt) =>
                    {
                        double s = (t * 0.045) % 1.0;
                        Vector pos = _saucerPath.PointAt(s);
                        Vector dir = _saucerPath.Tangent(s).NormalizeOrDefault();
                        c.Pose = new Pose(pos, Quaternion.LookRotation(dir, Convention.Up, Convention));
                        c.CurveParameter = s;
                    }));
            }

            // 7. Dart — a PlacedCone patrolling a cubic Bezier back and forth (apex leads).
            {
                var cone = new Cone(1.4, 4.5);
                _dartPath = Bezier.Cubic(
                    new Vector(24, 4, 18), new Vector(6, 14, 10),
                    new Vector(-18, 2, 6), new Vector(-24, 9, -16));
                _craft.Add(new AlienCraft(
                    "Dart", MeshFactory.Cone(1.4, 4.5, new XColor(230, 130, 120)),
                    new Pose(_dartPath.Start, Quaternion.Identity), 4.5,
                    (pose, p) => new PlacedCone(cone, pose).DistanceTo(p),
                    (pose, p) => new PlacedCone(cone, pose).ClosestPoint(p),
                    (c, t, dt) =>
                    {
                        double phase = (t * 0.12) % 2.0;
                        bool forward = phase < 1.0;
                        double s = forward ? phase : 2.0 - phase;     // ping-pong 0..1..0
                        Vector pos = _dartPath.PointAt(s);
                        Vector travel = _dartPath.Tangent(s).NormalizeOrDefault() * (forward ? 1.0 : -1.0);
                        // The cone's apex is its local +Z, so look along -travel to make the apex lead.
                        c.Pose = new Pose(pos, Quaternion.LookRotation(-travel, Convention.Up, Convention));
                        c.CurveParameter = s;
                    }));
            }

            // Arena bounds: fit a box around every craft (BoundingBox.FromPoints) then pad it (FromCenterHalfExtents).
            var centres = new List<Vector>();
            foreach (var c in _craft) centres.Add(c.Pose.Position);
            BoundingBox hull = BoundingBox.FromPoints(centres.ToArray());
            _arena = hull.Expand(14); // pad the fitted box outward by a uniform margin

            _grid = MeshFactory.GridAndAxes(40, 4);
            _arenaWire = MeshFactory.BoxWire(_arena.Min, _arena.Max, new XColor(60, 90, 120));
        }

        // ----------------------------------------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------------------------------------
        protected override void Update(Xna.GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalSeconds;
            double t = gameTime.TotalGameTime.TotalSeconds;
            var k = Keyboard.GetState();

            if (k.IsKeyDown(Keys.Escape)) Exit();

            // --- Flight controls: build a body-frame yaw/pitch/roll delta and compose it on the right ----
            double yawIn = (k.IsKeyDown(Keys.A) ? 1 : 0) - (k.IsKeyDown(Keys.D) ? 1 : 0);
            double pitchIn = (k.IsKeyDown(Keys.S) || k.IsKeyDown(Keys.Down) ? 1 : 0) - (k.IsKeyDown(Keys.W) || k.IsKeyDown(Keys.Up) ? 1 : 0);
            double rollIn = (k.IsKeyDown(Keys.Q) ? 1 : 0) - (k.IsKeyDown(Keys.E) ? 1 : 0);

            Quaternion bodyTurn = Quaternion.FromYawPitchRoll(
                new Angle(yawIn * 1.4 * dt),
                new Angle(pitchIn * 1.2 * dt),
                new Angle(rollIn * 1.9 * dt),
                Convention);
            _player = _player.WithRotation((_player.Rotation * bodyTurn).NormalizeOrDefault());

            // --- Thrust along the craft's nose (its local forward, in world space) -----------------------
            Vector forward = _player.ApplyDirection(Convention.Forward);
            double throttle = (k.IsKeyDown(Keys.Space) ? 1 : 0) - (k.IsKeyDown(Keys.LeftControl) || k.IsKeyDown(Keys.RightControl) ? 1 : 0);
            _velocity += forward * (throttle * Accel * dt);
            if (k.IsKeyDown(Keys.X)) _velocity = _velocity.MoveTowards(Vector.Zero, Accel * 1.5 * dt); // brake
            _velocity = _velocity.ClampMagnitude(MaxSpeed);
            _player = _player.Translate(_velocity * dt);

            // --- Keep the player inside the arena (BoundingBox.ClosestPoint), killing the outward velocity --------
            if (!_arena.Contains(_player.Position))
            {
                Vector clamped = _arena.ClosestPoint(_player.Position);
                _player = _player.WithPosition(clamped);
                _velocity = _velocity * 0.2;
            }

            // --- Move the alien craft along their behaviours ---------------------------------------------
            foreach (var c in _craft) c.Update(t, dt);

            ResolveCollisions();
            UpdateTargeting();
            UpdateCamera(dt);
            BuildHud(gameTime);

            base.Update(gameTime);
        }

        /// <summary>Push the player out of any hull it has entered and bounce its velocity off the surface.</summary>
        private void ResolveCollisions()
        {
            _colliding = false;
            var playerSphere = new BoundingSphere(_player.Position, PlayerRadius);

            foreach (var c in _craft)
            {
                // Broad phase: bounding-sphere vs bounding-sphere.
                if (!c.Bounds.Intersects(playerSphere)) continue;

                // Narrow phase: exact distance to the placed shape (0 when the centre is inside).
                Vector centre = _player.Position;
                double d = c.DistanceTo(centre);
                if (d >= PlayerRadius) continue;

                Vector surface = c.ClosestPoint(centre);
                Vector normal = (centre - surface);
                if (normal.IsZero()) normal = centre - c.Pose.Position; // centre was inside the hull
                normal = normal.NormalizeOrDefault();
                if (normal.IsZero()) continue;

                double penetration = PlayerRadius - d;
                _player = _player.Translate(normal * penetration);     // separate
                _velocity = Vector.Reflect(_velocity, normal) * 0.45;  // bounce + damp

                _colliding = true;
                _collideName = c.Name;
                playerSphere = new BoundingSphere(_player.Position, PlayerRadius);
            }
        }

        /// <summary>Fire a forward ray from the nose and find the nearest craft it hits (ray vs bounding sphere).</summary>
        private void UpdateTargeting()
        {
            Vector origin = _player.Position;
            Vector dir = _player.ApplyDirection(Convention.Forward);
            var ray = new Ray(origin, dir);

            double best = double.PositiveInfinity;
            string name = "(none)";
            const double beam = 90.0;

            foreach (var c in _craft)
            {
                // Solve |origin + t*dir - centre|^2 = r^2 for t with RP.Math's quadratic solver.
                Vector oc = origin - c.Pose.Position;
                double b = 2.0 * dir.DotProduct(oc);
                double cc = oc.DotProduct(oc) - c.BoundRadius * c.BoundRadius;
                foreach (double root in PolynomialRoots.SolveQuadratic(1.0, b, cc))
                {
                    if (root > 0.05 && root < best) { best = root; name = c.Name; }
                }
            }

            _laserHit = !double.IsPositiveInfinity(best);
            _laserStart = ray.PointAt(0.6);
            _laserEnd = ray.PointAt(_laserHit ? best : beam);
            _targetName = name;
            _targetDist = _laserHit ? best : 0;
        }

        /// <summary>3rd-person chase camera: sit behind/above the craft (Pose.Apply) and ease towards it.</summary>
        private void UpdateCamera(double dt)
        {
            Vector desiredEye = _player.Apply(new Vector(0, 1.8, 8.5));      // behind (+Z) and above (+Y) in body space
            Vector forward = _player.ApplyDirection(Convention.Forward);
            Vector desiredTarget = _player.Position + forward * 10.0;

            double a = 1.0 - Math.Exp(-7.0 * dt); // frame-rate independent smoothing factor in [0,1]
            _camEye = _camEye.Interpolate(desiredEye, a);
            _camTarget = _camTarget.Interpolate(desiredTarget, a);
        }

        // ----------------------------------------------------------------------------------------------
        //  Telemetry HUD text
        // ----------------------------------------------------------------------------------------------
        private string _hud = "";

        private void BuildHud(Xna.GameTime gameTime)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string F(double v) => v.ToString("0.0", ci);

            // Nearest craft by centre distance, plus the off-bore angle to it.
            AlienCraft? nearest = null;
            double nearestDist = double.PositiveInfinity;
            foreach (var c in _craft)
            {
                double dd = _player.Position.Distance(c.Pose.Position);
                if (dd < nearestDist) { nearestDist = dd; nearest = c; }
            }

            Vector forward = _player.ApplyDirection(Convention.Forward);
            Angle offBore = Angle.Zero_Angle;
            string offClass = "";
            if (nearest != null)
            {
                Vector toShip = nearest.Pose.Position - _player.Position;
                offBore = new Angle(forward.Angle(toShip));
                offClass = offBore.IsAcute() ? "ahead" : (offBore.IsObtuse() ? "behind" : "abeam");
            }

            // Orientation read-outs: quaternion, yaw/pitch/roll (Attitude), and total tilt vs identity.
            Quaternion q = _player.Rotation;
            Attitude att = Attitude.FromQuaternion(q, Convention);
            Angle tilt = q.AngleTo(Quaternion.Identity);

            double speed = _velocity.Magnitude;
            bool cruising = DoubleExtension.AlmostEqualsWithAbsTolerance(speed, MaxSpeed, 0.4);

            // Scanner plane (y = 0): signed distance and which side.
            double planeDist = Scanner.SignedDistanceTo(_player.Position);
            int side = Scanner.SideOf(_player.Position, 0.05);
            string sideText = side > 0 ? "above" : (side < 0 ? "below" : "on");

            var sb = new StringBuilder();
            sb.Append("RP.Math  •  Alien Spacecraft Fly-Through\n");
            sb.Append($"convention : {ConventionName} ({Convention.Handedness}-handed)\n");
            sb.Append($"fwd / up   : {Fmt(forward)}  /  {Fmt(Convention.Up)}\n");
            sb.Append("\n");
            sb.Append($"cam eye    : {Fmt(_camEye)}\n");
            sb.Append($"position   : {Fmt(_player.Position)}\n");
            sb.Append($"speed      : {F(speed)} / {F(MaxSpeed)} u/s {(cruising ? "[CRUISE]" : "")}\n");
            sb.Append($"quaternion : ({F(q.X)}, {F(q.Y)}, {F(q.Z)}, {F(q.W)})\n");
            sb.Append($"yaw/pit/rol: {F(att.Yaw.Deg)}° {F(att.Pitch.Deg)}° {F(att.Roll.Deg)}°\n");
            sb.Append($"tilt vs id : {F(tilt.Deg)}°\n");
            sb.Append("\n");
            if (nearest != null)
                sb.Append($"nearest    : {nearest.Name} @ {F(nearestDist)}u, {F(offBore.Deg)}° {offClass}\n");
            sb.Append($"target lock: {_targetName}{(_laserHit ? $" @ {F(_targetDist)}u" : "")}\n");
            sb.Append($"collision  : {(_colliding ? "CONTACT — " + _collideName : "CLEAR")}\n");
            sb.Append($"scanner y=0: {F(planeDist)}u ({sideText})\n");

            // Show a curve parameter for the two craft that follow splines.
            foreach (var c in _craft)
                if (!double.IsNaN(c.CurveParameter))
                    sb.Append($"  {c.Name,-12}: curve t = {F(c.CurveParameter)}\n");

            sb.Append("\n");
            sb.Append("W/S pitch  A/D yaw  Q/E roll  SPACE thrust  CTRL reverse  X brake  ESC quit");
            _hud = sb.ToString();

            string Fmt(Vector v) => $"({F(v.X)}, {F(v.Y)}, {F(v.Z)})";
        }

        // ----------------------------------------------------------------------------------------------
        //  Draw
        // ----------------------------------------------------------------------------------------------
        protected override void Draw(Xna.GameTime gameTime)
        {
            GraphicsDevice.Clear(new XColor(6, 8, 16));
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None, MultiSampleAntiAlias = true };

            // View comes from RP.Math.LookAt; projection from RP.Math.PerspectiveFieldOfView (remapped to D3D z).
            double aspect = GraphicsDevice.Viewport.AspectRatio;
            Vector up = _player.ApplyDirection(Convention.Up);
            XMatrix view;
            try { view = Interop.ToXna(Matrix.LookAt(_camEye, _camTarget, up)); }
            catch (ArgumentException) { view = Interop.ToXna(Matrix.LookAt(_camEye, _camTarget, Convention.Up)); }

            _effect.View = view;
            _effect.Projection = Interop.ToXna(Interop.PerspectiveD3D(new Angle(60, AngleUnits.DEG), aspect, 0.5, 400.0));

            DrawPrimitives(_grid, PrimitiveType.LineList, XMatrix.Identity);
            DrawPrimitives(_arenaWire, PrimitiveType.LineList, XMatrix.Identity);

            foreach (var c in _craft)
                DrawPrimitives(c.Mesh, PrimitiveType.TriangleList, Interop.ToXna(c.Pose.ToMatrix()));

            DrawPrimitives(_playerMesh, PrimitiveType.TriangleList, Interop.ToXna(_player.ToMatrix()));

            // Targeting laser.
            var beam = new[]
            {
                new VertexPositionColor(Interop.ToXna(_laserStart), _laserHit ? XColor.OrangeRed : new XColor(60, 120, 90)),
                new VertexPositionColor(Interop.ToXna(_laserEnd), _laserHit ? XColor.Yellow : new XColor(30, 70, 50)),
            };
            DrawPrimitives(beam, PrimitiveType.LineList, XMatrix.Identity);

            // HUD + crosshair.
            _sprite.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            _font.Draw(_sprite, _hud, new XVec2(16, 14), XColor.LightSkyBlue);
            int cx = GraphicsDevice.Viewport.Width / 2, cy = GraphicsDevice.Viewport.Height / 2;
            _font.Draw(_sprite, "+", new XVec2(cx - _font.CharWidth * 0.5f, cy - _font.LineHeight * 0.5f),
                _laserHit ? XColor.Yellow : new XColor(120, 160, 200));
            _sprite.End();

            base.Draw(gameTime);
        }

        private void DrawPrimitives(VertexPositionColor[] verts, PrimitiveType type, XMatrix world)
        {
            if (verts.Length == 0) return;
            int count = type == PrimitiveType.TriangleList ? verts.Length / 3 : verts.Length / 2;
            if (count == 0) return;

            _effect.World = world;
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(type, verts, 0, count);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _font?.Dispose();
                _effect?.Dispose();
                _sprite?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
