# RP.Math — Alien Spacecraft Fly-Through (DirectX / MonoGame sample)

A small 3rd-person space scene that drives **as much of the RP.Math library as practical** from real
gameplay: you fly a little craft through a cluster of drifting alien spacecraft, with a live telemetry
HUD showing the maths as it happens.

The renderer is **MonoGame on Windows**, which sits on **Direct3D 11** (the `WindowsDX` platform).
MonoGame only owns the window, the keyboard and the triangle rasteriser — **every spatial calculation
is done with RP.Math**: the camera, the craft orientation, the flight model, collision, targeting, and
even the projection matrix.

## Run it

```sh
dotnet run --project RP.Math.Sample.SpaceFlight
```

No content pipeline is needed: all geometry is generated procedurally and the HUD font is rasterised at
runtime, so a bare `dotnet run` works. Requires the .NET 8 SDK on Windows.

## Controls

| Key            | Action                                  |
|----------------|-----------------------------------------|
| `W` / `S`      | Pitch nose down / up                    |
| `A` / `D`      | Yaw left / right                        |
| `Q` / `E`      | Roll left / right                       |
| `Space`        | Thrust forward (along the nose)         |
| `Ctrl`         | Reverse thrust                          |
| `X`            | Brake (ease velocity to zero)           |
| `Esc`          | Quit                                    |

Fly into a hull and you bounce off it; aim the crosshair at a craft and the targeting laser locks on.

## The scene

Seven alien craft, **each a different RP.Math placed shape** with its own motion:

| Craft         | RP.Math shape        | Behaviour                                             |
|---------------|----------------------|-------------------------------------------------------|
| Monolith      | `PlacedCuboid`       | Slowly yaws about the convention's up axis            |
| Scout         | `PlacedSphere`       | Hovers and bobs                                       |
| Drum          | `PlacedCylinder`     | Tumbles, driven by Euler angles (`Rotation`)          |
| Pod           | `PlacedCapsule`      | Tilted, bobs along the world up vector                |
| Ring Station  | `PlacedTorus`        | Spins about its **own** axis (quaternion composition) |
| Saucer        | `PlacedEllipsoid`    | Glides a closed **Catmull-Rom** loop, banking         |
| Dart          | `PlacedCone`         | Patrols a cubic **Bézier** back and forth, apex-first |

## How the maths maps to the renderer

RP.Math matrices are **column-vector / row-major** with an **OpenGL** clip space (near/far → z = −1/+1).
MonoGame matrices are **row-vector** with a **Direct3D** clip space (near/far → z = 0/+1). So `Interop`:

* **transposes** every RP.Math matrix on the way to MonoGame (`(P·V·M·v)ᵀ = vᵀ·Mᵀ·Vᵀ·Pᵀ`), and
* composes the OpenGL projection with a tiny z-remap matrix to get the Direct3D depth range — which is
  itself an RP.Math matrix multiply.

The flight model uses the `OrthogonalAxes.MathsYUp` convention (right-handed, Y-up, forward = −Z), which
matches RP.Math's right-handed `LookAt` / `PerspectiveFieldOfView`. The library also ships
`OrthogonalAxes.DirectX` (left-handed, +Z into the screen); it is intentionally **not** used here, because
pairing a left-handed convention with the right-handed view/projection would mirror the world.

## RP.Math features exercised

* **Vector** — arithmetic, `DotProduct`, `CrossProduct` (mesh face normals + shading), `Normalize`,
  `Distance`, `Angle`, `Interpolate` (camera easing), `Reflect` (collision bounce), `MoveTowards` (brake),
  `ClampMagnitude` (speed limit), `IsZero`.
* **Angle** — degrees/radians, FOV, yaw/pitch/roll rates, `IsAcute` / `IsObtuse` (off-bore classification).
* **Quaternion** — `FromYawPitchRoll`, `FromAxisAngle`, `LookRotation`, Hamilton product (`*`), `Rotate`,
  `AngleTo`, `NormalizeOrDefault`, `ToMatrix`.
* **Pose** — `Apply` / `ApplyDirection` (camera + thrust), `WithPosition` / `WithRotation`, `Translate`,
  `RotateBy`, `ToMatrix`.
* **Matrix** — `LookAt` (view), `PerspectiveFieldOfView` (projection), matrix multiply (z-remap),
  `Pose.ToMatrix` (model).
* **OrthogonalAxes** — `MathsYUp` convention, `Up` / `Right` / `Forward`, `Handedness`.
* **Geometry** — `Ray` + `PointAt` (targeting laser), `Plane` `SignedDistanceTo` / `SideOf` (scanner plane).
* **Bounds** — `BoundingSphere` broad-phase (`Intersects`), `BoundingBox.FromPoints` / `Expand` / `Contains` /
  `ClosestPoint` (the arena).
* **Curves** — `Bezier.Cubic` and `CatmullRom` with `PointAt` / `Tangent` (patrol paths).
* **Shapes** — `PlacedSphere`, `PlacedCuboid`, `PlacedCylinder`, `PlacedCone`, `PlacedCapsule`,
  `PlacedTorus`, `PlacedEllipsoid` with `DistanceTo` / `ClosestPoint` (narrow-phase collision).
* **Orientation extras** — `Rotation` (Euler tumble), `Attitude.FromQuaternion` (HUD yaw/pitch/roll).
* **Numerics** — `PolynomialRoots.SolveQuadratic` (ray-vs-sphere targeting),
  `DoubleExtension.AlmostEqualsWithAbsTolerance` (cruise-speed read-out).

## Files

| File              | Responsibility                                                              |
|-------------------|-----------------------------------------------------------------------------|
| `Program.cs`      | Entry point.                                                                |
| `SpaceGame.cs`    | Game loop: input, flight model, camera, collision, targeting, HUD, drawing. |
| `AlienCraft.cs`   | One craft: mesh + pose + behaviour + placed-shape collision queries.        |
| `MeshFactory.cs`  | Procedural meshes (matching each shape's local frame) with baked shading.   |
| `Interop.cs`      | RP.Math ↔ MonoGame conversions (the matrix transpose + the D3D projection). |
| `BitmapFont.cs`   | Runtime GDI font atlas for the HUD (no content pipeline).                   |
