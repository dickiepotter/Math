namespace VectorVisualizer;

using RP.Math;

public enum ResultKind { Vector, Scalar, Boolean, Integer }

/// <summary>The inputs the user can adjust, beyond vector A.</summary>
public enum Input { B, C, Scalar, Angle, Control, Digits, Tolerance }

/// <summary>Kinds of auxiliary geometry an operation can contribute to the 3D scene.</summary>
public enum SceneKind { Point, Segment, Line, Plane, Triad }

/// <summary>
/// An extra primitive an operation asks the scene to draw alongside the A/B/C/result arrows —
/// the line a point was projected onto, the plane it was reflected across, a rotated frame, etc.
/// </summary>
public sealed class SceneItem
{
    public SceneKind Kind { get; init; }
    /// <summary>Primary anchor: the point, segment tail, line point, or plane point.</summary>
    public Vector P0 { get; init; }
    /// <summary>Secondary: segment head, line/ray direction, or plane normal. Unused for Point.</summary>
    public Vector P1 { get; init; }
    public string Color { get; init; } = "#9aa4b2";
    public string? Label { get; init; }
    /// <summary>For a <see cref="SceneKind.Ray"/>-style line that only extends forwards.</summary>
    public bool Forward { get; init; }

    public static SceneItem Point(Vector p, string color, string? label = null)
        => new() { Kind = SceneKind.Point, P0 = p, Color = color, Label = label };
    public static SceneItem Segment(Vector tail, Vector head, string color, string? label = null)
        => new() { Kind = SceneKind.Segment, P0 = tail, P1 = head, Color = color, Label = label };
    public static SceneItem Line(Vector point, Vector dir, string color, string? label = null, bool forward = false)
        => new() { Kind = SceneKind.Line, P0 = point, P1 = dir, Color = color, Label = label, Forward = forward };
    public static SceneItem Plane(Vector point, Vector normal, string color, string? label = null)
        => new() { Kind = SceneKind.Plane, P0 = point, P1 = normal, Color = color, Label = label };
    public static SceneItem Triad(Vector origin, string color, string? label = null)
        => new() { Kind = SceneKind.Triad, P0 = origin, Color = color, Label = label };
}

/// <summary>The outcome of running an operation, tagged with how to present it.</summary>
public sealed class OpResult
{
    public ResultKind Kind { get; private init; }
    public Vector Vector { get; private init; }
    public double Scalar { get; private init; }
    public bool Boolean { get; private init; }
    public int Integer { get; private init; }

    /// <summary>When true a Vector result is a position, drawn as a point marker rather than an arrow.</summary>
    public bool PointResult { get; private init; }
    /// <summary>Auxiliary geometry to draw in the scene (lines, planes, frames…).</summary>
    public IReadOnlyList<SceneItem> Extras { get; private init; } = System.Array.Empty<SceneItem>();

    public OpResult With(params SceneItem[] extras) => new()
    {
        Kind = Kind, Vector = Vector, Scalar = Scalar, Boolean = Boolean, Integer = Integer,
        PointResult = PointResult, Extras = extras,
    };

    public OpResult AsPoint() => new()
    {
        Kind = Kind, Vector = Vector, Scalar = Scalar, Boolean = Boolean, Integer = Integer,
        PointResult = true, Extras = Extras,
    };

    public static OpResult Vec(Vector v) => new() { Kind = ResultKind.Vector, Vector = v };
    public static OpResult Num(double d) => new() { Kind = ResultKind.Scalar, Scalar = d };
    public static OpResult Bool(bool b) => new() { Kind = ResultKind.Boolean, Boolean = b };
    public static OpResult Int(int i) => new() { Kind = ResultKind.Integer, Integer = i };
}

/// <summary>All the values an operation can draw on.</summary>
public sealed class OpContext
{
    public Vector A { get; set; }
    public Vector B { get; set; }
    public Vector C { get; set; }
    public double Scalar { get; set; } = 2;
    public double AngleDeg { get; set; } = 45;
    public double Control { get; set; } = 0.5;
    public int Digits { get; set; } = 2;
    public double Tolerance { get; set; } = 1e-6;

    public double AngleRad => AngleDeg * Math.PI / 180.0;
}

public sealed class OperationDef
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Formula { get; init; }
    public required string Description { get; init; }
    public required Func<OpContext, OpResult> Run { get; init; }
    public HashSet<Input> Inputs { get; init; } = new();

    public bool Uses(Input i) => Inputs.Contains(i);
}

public static class Operations
{
    public static readonly IReadOnlyList<OperationDef> All = Build();

    private static List<OperationDef> Build()
    {
        var ops = new List<OperationDef>();

        void Add(string key, string name, string cat, string formula, string desc,
                 Func<OpContext, OpResult> run, params Input[] inputs)
            => ops.Add(new OperationDef
            {
                Key = key, Name = name, Category = cat, Formula = formula,
                Description = desc, Run = run, Inputs = new HashSet<Input>(inputs),
            });

        // ---- Arithmetic ----
        Add("add", "Add (A + B)", "Arithmetic", "A + B",
            "Adds the vectors component-wise. Geometrically, place B's tail at A's tip; the sum reaches the new tip.",
            c => OpResult.Vec(c.A + c.B), Input.B);
        Add("sub", "Subtract (A − B)", "Arithmetic", "A − B",
            "Subtracts component-wise. A − B is the vector pointing from B's tip to A's tip.",
            c => OpResult.Vec(c.A - c.B), Input.B);
        Add("neg", "Negate (−A)", "Arithmetic", "−A",
            "Flips the vector to point the opposite way, same length.",
            c => OpResult.Vec(-c.A));
        Add("mul", "Multiply by scalar (A · s)", "Arithmetic", "A · s",
            "Scales every component by s. |s| > 1 lengthens, < 1 shortens, negative flips direction.",
            c => OpResult.Vec(c.A * c.Scalar), Input.Scalar);
        Add("div", "Divide by scalar (A ÷ s)", "Arithmetic", "A ÷ s",
            "Divides every component by s.",
            c => OpResult.Vec(c.A / c.Scalar), Input.Scalar);
        Add("scaleTo", "Scale to magnitude", "Arithmetic", "Scale(A, m)",
            "Returns a vector in A's direction but with magnitude m (A normalised, then × m).",
            c => OpResult.Vec(c.A.Scale(c.Scalar)), Input.Scalar);

        // ---- Products ----
        Add("dot", "Dot product (A · B)", "Products", "A · B = |A||B|cosθ",
            "A scalar measuring how much the vectors point the same way. Zero ⇒ perpendicular; positive ⇒ acute angle.",
            c => OpResult.Num(c.A.DotProduct(c.B)), Input.B);
        Add("cross", "Cross product (A × B)", "Products", "A × B",
            "A vector perpendicular to both A and B; its length equals the area of the parallelogram they span.",
            c => OpResult.Vec(c.A.CrossProduct(c.B)), Input.B);
        Add("mixed", "Triple product A · (B × C)", "Products", "A · (B × C)",
            "The signed volume of the parallelepiped formed by A, B and C.",
            c => OpResult.Num(c.A.MixedProduct(c.B, c.C)), Input.B, Input.C);

        // ---- Geometry ----
        Add("magnitude", "Magnitude |A|", "Geometry", "|A| = √(x² + y² + z²)",
            "The length of the vector.",
            c => OpResult.Num(c.A.Magnitude));
        Add("normalize", "Normalize (Â)", "Geometry", "Â = A / |A|",
            "Returns the unit vector in A's direction (length 1). Throws for a zero vector.",
            c => OpResult.Vec(c.A.Normalize()));
        Add("distance", "Distance(A, B)", "Geometry", "|A − B|",
            "The straight-line distance between the two tips.",
            c => OpResult.Num(c.A.Distance(c.B)), Input.B);
        Add("angle", "Angle(A, B)", "Geometry", "arccos( (A·B) / (|A||B|) )",
            "The angle between the vectors, in radians (degrees shown alongside).",
            c => OpResult.Num(c.A.Angle(c.B)), Input.B);
        Add("projection", "Projection onto B", "Geometry", "(A·B / |B|²) B",
            "The component of A that lies along B — A's 'shadow' on the line of B.",
            c => OpResult.Vec(c.A.Projection(c.B)), Input.B);
        Add("rejection", "Rejection from B", "Geometry", "A − proj_B(A)",
            "The component of A perpendicular to B (A minus its projection onto B).",
            c => OpResult.Vec(c.A.Rejection(c.B)), Input.B);
        Add("reflection", "Reflection about B", "Geometry", "reflect A across B",
            "Reflects A in the plane/line defined by B (like a bounce).",
            c => OpResult.Vec(c.A.Reflection(c.B)), Input.B);
        Add("interpolate", "Interpolate(A, B, t)", "Geometry", "A(1 − t) + B·t",
            "Linear blend between A (t = 0) and B (t = 1). Values outside 0–1 extrapolate along the line.",
            c => OpResult.Vec(Vector.Interpolate(c.A, c.B, c.Control, true)), Input.B, Input.Control);
        Add("min", "Min(A, B)", "Geometry", "smaller of |A|, |B|",
            "Returns whichever of A or B has the smaller magnitude (length) — not a component-wise minimum.",
            c => OpResult.Vec(c.A.Min(c.B)), Input.B);
        Add("max", "Max(A, B)", "Geometry", "greater of |A|, |B|",
            "Returns whichever of A or B has the greater magnitude (length) — not a component-wise maximum.",
            c => OpResult.Vec(c.A.Max(c.B)), Input.B);
        Add("reflectNormal", "Reflect A about normal B", "Geometry", "A − 2(A·n̂)n̂",
            "Reflects A about the surface whose normal is B (a bounce). B is normalised internally.",
            c => OpResult.Vec(c.A.Reflect(c.B)), Input.B);
        Add("slerp", "Slerp(A, B, t)", "Geometry", "spherical interpolation",
            "Interpolates direction along the great-circle arc between A and B (magnitude blended linearly).",
            c => OpResult.Vec(c.A.Slerp(c.B, c.Control)), Input.B, Input.Control);
        Add("moveTowards", "MoveTowards(A → B, step)", "Geometry", "step from A towards B",
            "Moves A towards target B by at most 'step', stopping exactly on B.",
            c => OpResult.Vec(c.A.MoveTowards(c.B, c.Scalar)), Input.B, Input.Scalar);
        Add("distanceSq", "DistanceSquared(A, B)", "Geometry", "|A − B|²",
            "The squared distance between the tips — cheaper than Distance (no square root).",
            c => OpResult.Num(c.A.DistanceSquared(c.B)), Input.B);
        Add("componentMin", "Component min(A, B)", "Geometry", "(min xs, min ys, min zs)",
            "A true component-wise minimum (each axis independently), unlike magnitude-based Min.",
            c => OpResult.Vec(c.A.ComponentMin(c.B)), Input.B);
        Add("componentMax", "Component max(A, B)", "Geometry", "(max xs, max ys, max zs)",
            "A true component-wise maximum (each axis independently), unlike magnitude-based Max.",
            c => OpResult.Vec(c.A.ComponentMax(c.B)), Input.B);
        Add("clamp", "Clamp(A, min B, max C)", "Geometry", "clamp each component",
            "Clamps each component of A between the matching components of B (min) and C (max).",
            c => OpResult.Vec(c.A.Clamp(c.B, c.C)), Input.B, Input.C);
        Add("clampMag", "ClampMagnitude(A, max)", "Geometry", "cap |A| at max",
            "Caps A's length at 'max', keeping its direction.",
            c => OpResult.Vec(c.A.ClampMagnitude(c.Scalar)), Input.Scalar);

        // ---- Rotation (degrees in, applied as radians) ----
        Add("rotateX", "Rotate about X", "Rotation", "Rx(θ) · A",
            "Rotates A around the X axis by θ.",
            c => OpResult.Vec(c.A.RotateX(c.AngleRad)), Input.Angle);
        Add("rotateY", "Rotate about Y", "Rotation", "Ry(θ) · A",
            "Rotates A around the Y axis by θ.",
            c => OpResult.Vec(c.A.RotateY(c.AngleRad)), Input.Angle);
        Add("rotateZ", "Rotate about Z", "Rotation", "Rz(θ) · A",
            "Rotates A around the Z axis by θ.",
            c => OpResult.Vec(c.A.RotateZ(c.AngleRad)), Input.Angle);
        Add("yaw", "Yaw", "Rotation", "yaw θ (about Up)",
            "Aviation-style yaw: rotation about the convention's Up (DirectX: +Y).",
            c => OpResult.Vec(c.A.Yaw(new Angle(c.AngleRad), OrthogonalAxes.DirectX)), Input.Angle);
        Add("pitch", "Pitch", "Rotation", "pitch θ (about Right)",
            "Aviation-style pitch: rotation about the convention's Right (DirectX: +X).",
            c => OpResult.Vec(c.A.Pitch(new Angle(c.AngleRad), OrthogonalAxes.DirectX)), Input.Angle);
        Add("roll", "Roll", "Rotation", "roll θ (about Forward)",
            "Aviation-style roll: rotation about the convention's Forward (DirectX: +Z).",
            c => OpResult.Vec(c.A.Roll(new Angle(c.AngleRad), OrthogonalAxes.DirectX)), Input.Angle);

        // ---- Components ----
        Add("abs", "Abs()", "Components", "|A|",
            "Absolute value of the vector (its magnitude).",
            c => OpResult.Num(c.A.Abs()));
        Add("sumComponents", "Sum of components", "Components", "x + y + z",
            "Adds the three components together.",
            c => OpResult.Num(c.A.SumComponents()));
        Add("sumComponentSqrs", "Sum of squares", "Components", "x² + y² + z²",
            "Adds the squares of the components (magnitude squared).",
            c => OpResult.Num(c.A.SumComponentSqrs()));
        Add("magnitudeSq", "Magnitude squared", "Components", "|A|²",
            "The squared magnitude — cheaper than Magnitude when only comparing lengths.",
            c => OpResult.Num(c.A.MagnitudeSquared));
        Add("sqrtComponents", "Sqrt of components", "Components", "(√x, √y, √z)",
            "Square root of each component.",
            c => OpResult.Vec(c.A.SqrtComponents()));
        Add("sqrComponents", "Square of components", "Components", "(x², y², z²)",
            "Squares each component.",
            c => OpResult.Vec(c.A.SqrComponents()));
        Add("powComponents", "Power of components", "Components", "(xⁿ, yⁿ, zⁿ)",
            "Raises each component to the power n.",
            c => OpResult.Vec(c.A.PowComponents(c.Scalar)), Input.Scalar);
        Add("round", "Round(digits)", "Components", "round each to d digits",
            "Rounds each component to d decimal places.",
            c => OpResult.Vec(c.A.Round(c.Digits)), Input.Digits);

        // ---- Predicates ----
        Add("isUnit", "Is unit vector?", "Predicates", "|A| ≈ 1",
            "True when A's length is 1 (within tolerance).",
            c => OpResult.Bool(c.A.IsUnitVector(c.Tolerance)), Input.Tolerance);
        Add("isNaN", "Is NaN?", "Predicates", "any component NaN",
            "True when any component is Not-a-Number.",
            c => OpResult.Bool(c.A.IsNaN()));
        Add("isZero", "Is zero?", "Predicates", "|A| ≈ 0",
            "True when the vector's magnitude is within tolerance of zero.",
            c => OpResult.Bool(c.A.IsZero(c.Tolerance)), Input.Tolerance);
        Add("isPerp", "Is perpendicular to B?", "Predicates", "A · B ≈ 0",
            "True when A and B meet at a right angle (within tolerance).",
            c => OpResult.Bool(c.A.IsPerpendicular(c.B, c.Tolerance)), Input.B, Input.Tolerance);
        Add("isBackFace", "Is back face? (A normal, B sight)", "Predicates", "A · B < 0",
            "Treating A as a surface normal and B as the line of sight, true when they point against each other (dot < 0).",
            c => OpResult.Bool(c.A.IsBackFace(c.B)), Input.B);

        // ---- Comparison ----
        Add("equals", "Equals(A, B)?", "Comparison", "A == B",
            "True when the vectors are equal (within tolerance).",
            c => OpResult.Bool(c.A.Equals(c.B, c.Tolerance)), Input.B, Input.Tolerance);
        Add("compareTo", "CompareTo(A, B)", "Comparison", "sign(|A| − |B|)",
            "Compares the vectors by magnitude: −1, 0 or 1.",
            c => OpResult.Int(c.A.CompareTo(c.B)), Input.B);

        // ---- Orientation: rotate A about axis B by θ, via every rotation representation ----
        // All of these answer the same question through a different RP.Math type, so dragging B
        // (the axis) and the θ slider rotates the green result around it identically in each case.
        Add("quatRotate", "Quaternion · A (axis B, θ)", "Orientation", "q ⊗ A ⊗ q⁻¹",
            "Builds a unit quaternion from axis B and angle θ, then rotates A by it. The canonical, gimbal-free rotation.",
            c => OpResult.Vec(Quaternion.FromAxisAngle(c.B, new Angle(c.AngleRad)).Rotate(c.A))
                 .With(SceneItem.Line(new Vector(0, 0, 0), c.B, "#ff7ac6", "axis")),
            Input.B, Input.Angle);
        Add("axisAngleRotate", "AxisAngle · A (axis B, θ)", "Orientation", "rot(axis B, θ) · A",
            "Rotates A about axis B by θ using the AxisAngle type (axis + magnitude) directly.",
            c => OpResult.Vec(new AxisAngle(c.B, new Angle(c.AngleRad)).Rotate(c.A))
                 .With(SceneItem.Line(new Vector(0, 0, 0), c.B, "#ff7ac6", "axis")),
            Input.B, Input.Angle);
        Add("matrixRotX", "Matrix Rx(θ) · A", "Orientation", "Rx(θ) · A",
            "Multiplies A by a 3×3 rotation matrix about the X axis — the matrix form of the rotation.",
            c => OpResult.Vec(Matrix.RotationMatrixAboutXAxis(new Angle(c.AngleRad)) * c.A), Input.Angle);
        Add("matrixRotY", "Matrix Ry(θ) · A", "Orientation", "Ry(θ) · A",
            "Multiplies A by a 3×3 rotation matrix about the Y axis.",
            c => OpResult.Vec(Matrix.RotationMatrixAboutYAxis(new Angle(c.AngleRad)) * c.A), Input.Angle);
        Add("matrixRotZ", "Matrix Rz(θ) · A", "Orientation", "Rz(θ) · A",
            "Multiplies A by a 3×3 rotation matrix about the Z axis.",
            c => OpResult.Vec(Matrix.RotationMatrixAboutZAxis(new Angle(c.AngleRad)) * c.A), Input.Angle);
        Add("matrixScale", "Matrix scale · A (by B)", "Orientation", "diag(Bx,By,Bz) · A",
            "Multiplies A by a non-uniform scaling matrix whose diagonal is B — stretches each axis independently.",
            c => OpResult.Vec(Matrix.ScalingMatrix(c.B.X, c.B.Y, c.B.Z) * c.A), Input.B);
        Add("rotationAboutX", "Rotation AboutX(θ) · A", "Orientation", "Rotation.AboutX(θ) · A",
            "Rotates A about X using the Euler-style Rotation type (intrinsic X-Y-Z angles).",
            c => OpResult.Vec(Rotation.AboutX(new Angle(c.AngleRad)).Rotate(c.A)), Input.Angle);
        Add("rotationAboutY", "Rotation AboutY(θ) · A", "Orientation", "Rotation.AboutY(θ) · A",
            "Rotates A about Y using the Rotation type.",
            c => OpResult.Vec(Rotation.AboutY(new Angle(c.AngleRad)).Rotate(c.A)), Input.Angle);
        Add("rotationAboutZ", "Rotation AboutZ(θ) · A", "Orientation", "Rotation.AboutZ(θ) · A",
            "Rotates A about Z using the Rotation type.",
            c => OpResult.Vec(Rotation.AboutZ(new Angle(c.AngleRad)).Rotate(c.A)), Input.Angle);
        Add("quatSlerpRotate", "Slerp(I, q)·A (axis B, t)", "Orientation", "slerp(I, q)·A",
            "Spherically interpolates from no rotation toward a 180° turn about axis B by t, then applies it to A — a smooth swing.",
            c => OpResult.Vec(
                    Quaternion.Slerp(new Quaternion(0, 0, 0, 1),
                                     Quaternion.FromAxisAngle(c.B, new Angle(Math.PI)), c.Control)
                              .Rotate(c.A))
                 .With(SceneItem.Line(new Vector(0, 0, 0), c.B, "#ff7ac6", "axis")),
            Input.B, Input.Control);
        Add("poseApply", "Pose · A (rot axis B θ, move C)", "Orientation", "T(C)·R(B,θ) · A",
            "Treats A as a local point: rotates it about axis B by θ, then translates by C — a full rigid transform (Pose).",
            c => OpResult.Vec(Pose.FromAxisAngle(c.C, c.B, new Angle(c.AngleRad)).Apply(c.A))
                 .AsPoint()
                 .With(SceneItem.Line(new Vector(0, 0, 0), c.B, "#ff7ac6", "axis"),
                       SceneItem.Point(c.C, "#b39ddb", "move C")),
            Input.B, Input.C, Input.Angle);

        // ---- Geometry primitives: lines, rays, segments and planes built from B/C ----
        Add("lineClosest", "Closest point on line(0,B) to A", "Lines & Planes", "Line.ClosestPointTo(A)",
            "Defines a line through the origin in direction B, then finds the point on it nearest A (the foot of the perpendicular).",
            c => { var line = new Line(new Vector(0, 0, 0), c.B); var p = line.ClosestPointTo(c.A);
                   return OpResult.Vec(p).AsPoint().With(
                       SceneItem.Line(new Vector(0, 0, 0), c.B, "#ff7ac6", "line"),
                       SceneItem.Segment(c.A, p, "#6f7a88", "perp")); },
            Input.B);
        Add("lineDistance", "Distance from A to line(0,B)", "Lines & Planes", "Line.DistanceTo(A)",
            "The perpendicular distance from A to the line through the origin along B.",
            c => { var line = new Line(new Vector(0, 0, 0), c.B);
                   return OpResult.Num(line.DistanceTo(c.A)).With(
                       SceneItem.Line(new Vector(0, 0, 0), c.B, "#ff7ac6", "line"),
                       SceneItem.Segment(c.A, line.ClosestPointTo(c.A), "#6f7a88")); },
            Input.B);
        Add("rayClosest", "Closest point on ray(0,B) to A", "Lines & Planes", "Ray.ClosestPointTo(A)",
            "Like the line version but the ray only extends forwards from the origin along B, so the nearest point is clamped at the origin.",
            c => { var ray = new Ray(new Vector(0, 0, 0), c.B); var p = ray.ClosestPointTo(c.A);
                   return OpResult.Vec(p).AsPoint().With(
                       SceneItem.Line(new Vector(0, 0, 0), c.B, "#ff7ac6", "ray", forward: true),
                       SceneItem.Segment(c.A, p, "#6f7a88", "perp")); },
            Input.B);
        Add("segClosest", "Closest point on segment B→C to A", "Lines & Planes", "LineSegment.ClosestPointTo(A)",
            "Finds the point on the finite segment from B to C that is nearest A (clamped to the endpoints).",
            c => { var seg = new LineSegment(c.B, c.C); var p = seg.ClosestPointTo(c.A);
                   return OpResult.Vec(p).AsPoint().With(
                       SceneItem.Segment(c.B, c.C, "#ff7ac6", "segment"),
                       SceneItem.Segment(c.A, p, "#6f7a88", "perp")); },
            Input.B, Input.C);
        Add("segInterpolate", "Point along segment B→C (t)", "Lines & Planes", "LineSegment.Interpolate(t)",
            "Walks from B (t = 0) to C (t = 1) along the segment.",
            c => { var seg = new LineSegment(c.B, c.C); var p = seg.Interpolate(c.Control);
                   return OpResult.Vec(p).AsPoint().With(SceneItem.Segment(c.B, c.C, "#ff7ac6", "segment")); },
            Input.B, Input.C, Input.Control);
        Add("planeProject", "Project A onto plane(0, n=B)", "Lines & Planes", "Plane.ClosestPoint(A)",
            "Drops A perpendicularly onto the plane through the origin whose normal is B (A's shadow on the plane).",
            c => { var plane = Plane.FromPointNormal(new Vector(0, 0, 0), c.B); var p = plane.ClosestPoint(c.A);
                   return OpResult.Vec(p).AsPoint().With(
                       SceneItem.Plane(new Vector(0, 0, 0), c.B, "#4472c4", "plane"),
                       SceneItem.Segment(c.A, p, "#6f7a88", "drop")); },
            Input.B);
        Add("planeReflect", "Reflect A across plane(0, n=B)", "Lines & Planes", "Plane.Reflect(A)",
            "Mirrors A through the plane through the origin with normal B (a bounce off the surface).",
            c => { var plane = Plane.FromPointNormal(new Vector(0, 0, 0), c.B);
                   return OpResult.Vec(plane.Reflect(c.A)).AsPoint().With(
                       SceneItem.Plane(new Vector(0, 0, 0), c.B, "#4472c4", "plane")); },
            Input.B);
        Add("planeSignedDist", "Signed distance A to plane(0, n=B)", "Lines & Planes", "Plane.SignedDistanceTo(A)",
            "How far A sits from the origin-plane with normal B; positive on the side the normal points to, negative behind it.",
            c => { var plane = Plane.FromPointNormal(new Vector(0, 0, 0), c.B);
                   return OpResult.Num(plane.SignedDistanceTo(c.A)).With(
                       SceneItem.Plane(new Vector(0, 0, 0), c.B, "#4472c4", "plane")); },
            Input.B);

        // ---- Curves: Bézier / Catmull–Rom built from A, B, C, with a moving Frenet frame ----
        Add("bezier", "Bézier curve (A → B, control C)", "Curves", "Bezier(A, C, B).PointAt(t)",
            "A quadratic Bézier from A to B pulled toward control point C. The marker rides the curve at t; "
            + "the small triad is the Frenet frame (tangent / normal / binormal) there.",
            c => {
                var curve = new Bezier(c.A, c.C, c.B);
                var p = curve.PointAt(c.Control);
                var extras = new List<SceneItem>();
                extras.AddRange(Polyline(curve.Sample(48), "#ffd166"));
                extras.Add(SceneItem.Point(c.C, "#b39ddb", "control C"));
                extras.AddRange(FrameAxes(curve.Frame(c.Control)));
                return OpResult.Vec(p).AsPoint().With(extras.ToArray());
            },
            Input.B, Input.C, Input.Control);
        Add("catmullRom", "Catmull–Rom (0 → A → B → C)", "Curves", "CatmullRom(O,A,B,C).PointAt(t)",
            "A spline that passes through the origin, A, B and C in turn. The marker rides it at t.",
            c => {
                var spline = new CatmullRom(new Vector(0, 0, 0), c.A, c.B, c.C);
                var p = spline.PointAt(c.Control);
                var extras = new List<SceneItem>();
                extras.AddRange(Polyline(spline.Sample(64), "#06d6a0"));
                extras.AddRange(FrameAxes(spline.Frame(c.Control)));
                return OpResult.Vec(p).AsPoint().With(extras.ToArray());
            },
            Input.B, Input.C, Input.Control);
        Add("bezierLength", "Bézier arc length (A → B, control C)", "Curves", "Bezier(A, C, B).Length()",
            "The arc length of the quadratic Bézier from A to B with control C (sampled).",
            c => {
                var curve = new Bezier(c.A, c.C, c.B);
                return OpResult.Num(curve.Length())
                    .With(Polyline(curve.Sample(48), "#ffd166").ToArray());
            },
            Input.B, Input.C);
        Add("bezierClosest", "Closest point on Bézier to A", "Curves", "Bezier(O, C, B).ClosestPoint(A)",
            "The point on the Bézier (origin → B, control C) nearest A, with the perpendicular drawn in.",
            c => {
                var curve = new Bezier(new Vector(0, 0, 0), c.C, c.B);
                var p = curve.ClosestPoint(c.A);
                var extras = new List<SceneItem>();
                extras.AddRange(Polyline(curve.Sample(48), "#ffd166"));
                extras.Add(SceneItem.Segment(c.A, p, "#6f7a88", "perp"));
                return OpResult.Vec(p).AsPoint().With(extras.ToArray());
            },
            Input.B, Input.C);

        // ---- Bounding volumes fitted to A, B, C ----
        Add("aabb", "Bounding box of A, B, C", "Bounds", "BoundingBox.FromPoints(A, B, C)",
            "The smallest axis-aligned box containing A, B and C. The result value is its volume; the box is drawn as a wireframe.",
            c => {
                var box = BoundingBox.FromPoints(c.A, c.B, c.C);
                return OpResult.Num(box.Volume).With(BoxEdges(box, "#4472c4"));
            },
            Input.B, Input.C);
        Add("bsphere", "Bounding sphere of A, B, C", "Bounds", "BoundingSphere.FromPoints(A, B, C)",
            "A small sphere (Ritter's algorithm) containing A, B and C. The result value is its radius; three rings show the sphere.",
            c => {
                var s = BoundingSphere.FromPoints(c.A, c.B, c.C);
                return OpResult.Num(s.Radius)
                    .With(SphereRings(s.Center, s.Radius, "#ef476f").ToArray())
                    .AsPoint();
            },
            Input.B, Input.C);

        return ops;
    }

    // ---- Scene helpers for the curve / bounds operations ----

    private static IEnumerable<SceneItem> Polyline(Vector[] points, string color)
    {
        for (int i = 1; i < points.Length; i++)
        {
            yield return SceneItem.Segment(points[i - 1], points[i], color);
        }
    }

    private static IEnumerable<SceneItem> FrameAxes(CurveFrame f, double scale = 1.0)
    {
        yield return SceneItem.Segment(f.Position, f.Position + f.Tangent * scale, "#ff7ac6", "T");
        yield return SceneItem.Segment(f.Position, f.Position + f.Normal * scale, "#7ac6ff", "N");
        yield return SceneItem.Segment(f.Position, f.Position + f.Binormal * scale, "#c6ff7a", "B");
    }

    private static SceneItem[] BoxEdges(BoundingBox box, string color)
    {
        Vector[] k = box.Corners;
        // Corner index layout: bit0 = X, bit1 = Y, bit2 = Z (min/max).
        int[,] edges =
        {
            { 0, 1 }, { 1, 3 }, { 3, 2 }, { 2, 0 }, // bottom (z = min)
            { 4, 5 }, { 5, 7 }, { 7, 6 }, { 6, 4 }, // top (z = max)
            { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }, // verticals
        };

        var items = new SceneItem[edges.GetLength(0)];
        for (int e = 0; e < edges.GetLength(0); e++)
        {
            items[e] = SceneItem.Segment(k[edges[e, 0]], k[edges[e, 1]], color);
        }

        return items;
    }

    private static IEnumerable<SceneItem> SphereRings(Vector center, double radius, string color)
    {
        const int steps = 32;
        for (int i = 0; i < steps; i++)
        {
            double a0 = 2 * Math.PI * i / steps;
            double a1 = 2 * Math.PI * (i + 1) / steps;
            double c0 = Math.Cos(a0), s0 = Math.Sin(a0), c1 = Math.Cos(a1), s1 = Math.Sin(a1);

            // Three great circles in the XY, XZ and YZ planes.
            yield return SceneItem.Segment(
                center + new Vector(c0, s0, 0) * radius, center + new Vector(c1, s1, 0) * radius, color);
            yield return SceneItem.Segment(
                center + new Vector(c0, 0, s0) * radius, center + new Vector(c1, 0, s1) * radius, color);
            yield return SceneItem.Segment(
                center + new Vector(0, c0, s0) * radius, center + new Vector(0, c1, s1) * radius, color);
        }
    }
}
