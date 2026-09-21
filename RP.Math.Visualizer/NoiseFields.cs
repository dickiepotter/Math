namespace VectorVisualizer;

using RP.Math.Noise;

/// <summary>Which field the page samples.</summary>
public enum FieldKind
{
    /// <summary>Gradient noise on a square lattice — the classic, and the safest default.</summary>
    Perlin,

    /// <summary>Gradient noise on a simplex lattice: fewer axis-aligned artefacts, cheaper in higher dimensions.</summary>
    Simplex,

    /// <summary>Interpolated lattice values — blockier and cheaper, good for coarse masks.</summary>
    Value,

    /// <summary>Cellular (Worley) noise: distance to scattered feature points.</summary>
    Worley,
}

/// <summary>Which question a Worley sample is asked, since one lookup answers several.</summary>
public enum WorleyOutput
{
    /// <summary>Distance to the nearest feature point — rounded blobs.</summary>
    F1,

    /// <summary><c>F2 − F1</c>: near zero exactly on a cell boundary, which draws cracks and cobbles.</summary>
    Ridges,

    /// <summary>The cell's stable identity, coloured as a region map rather than a magnitude.</summary>
    Cells,
}

/// <summary>A shaping curve applied to the finished field, before it is coloured.</summary>
public enum ShapingCurve
{
    /// <summary>Leave the field as the library produced it.</summary>
    None,

    /// <summary>Quantise into flat bands — sedimentary cliffs and stepped mesas.</summary>
    Terrace,

    /// <summary>Fold about zero so crossings become crests.</summary>
    Ridge,

    /// <summary>Compress the middle of the range into plains while leaving the tails their full height.</summary>
    Continentalness,
}

/// <summary>Everything the noise page lets a visitor change.</summary>
public sealed class NoiseConfig
{
    public FieldKind Field { get; set; } = FieldKind.Perlin;
    public int Seed { get; set; } = 1337;
    public double Frequency { get; set; } = 3.0;

    public int Octaves { get; set; } = 5;
    public double Lacunarity { get; set; } = 2.0;
    public double Gain { get; set; } = 0.5;
    public FractalMode Mode { get; set; } = FractalMode.Brownian;

    public double WarpAmplitude { get; set; }
    public double WarpFrequency { get; set; } = 1.0;
    public int WarpIterations { get; set; } = 2;

    public DistanceMetric Metric { get; set; } = DistanceMetric.Euclidean;
    public double Jitter { get; set; } = 1.0;
    public WorleyOutput Output { get; set; } = WorleyOutput.Ridges;

    public ShapingCurve Curve { get; set; } = ShapingCurve.None;
    public double CurveAmount { get; set; } = 0.5;
    public int TerraceSteps { get; set; } = 6;

    public PaletteKind Palette { get; set; } = PaletteKind.Grey;

    /// <summary>
    /// The side of the square raster, in pixels. Every pixel is a real library call, so this is the one
    /// dial that costs time rather than changing the picture: 160 stays responsive while a slider is being
    /// dragged, 320 is worth waiting for once the look is settled.
    /// </summary>
    public int Resolution { get; set; } = 192;

    /// <summary>True when the fractal controls apply — every field but Worley, which has no octaves here.</summary>
    public bool IsCoherent => this.Field != FieldKind.Worley;
}

/// <summary>
/// Samples a configured field into an index raster, running the real <c>RP.Math.Noise</c> types.
/// </summary>
/// <remarks>
/// Nothing here re-implements any noise: the page's whole point is that what you see is the compiled
/// library's own output. This class only decides <i>where</i> to sample, and flattens the result into the
/// 0–255 byte that the palette then reads.
/// </remarks>
public static class NoiseFields
{
    /// <summary>
    /// How many world units the rendered square covers. Deliberately 1, so that the frequency dial reads
    /// directly as "periods across the image" — at 3 you see three hills, not three hills per eighth of
    /// the picture. Any larger window multiplies into the frequency and turns every setting into static.
    /// </summary>
    private const double Span = 1.0;

    /// <summary>Samples <paramref name="cfg"/> across a <paramref name="size"/>-square raster.</summary>
    public static byte[] Render(NoiseConfig cfg, int size)
    {
        if (cfg is null) throw new ArgumentNullException(nameof(cfg));
        if (size < 1) throw new ArgumentOutOfRangeException(nameof(size), "The raster must have a positive extent.");

        var indices = new byte[size * size];

        // Worley is handled apart from the coherent fields because the interesting outputs (the ridge
        // between cells, the cell's identity) come from SampleCellular, not from INoise2.Sample.
        var worley = cfg.Field == FieldKind.Worley
            ? new WorleyNoise(cfg.Seed, cfg.Metric, cfg.Jitter)
            : null;

        INoise2 field = worley ?? BuildCoherent(cfg);

        // The warp needs some INoise2 to hold as its source; it is only ever asked to Displace here, so
        // the field itself is the honest thing to hand it.
        var warp = cfg.WarpAmplitude > 0.0
            ? DomainWarp2.Simplex(field, cfg.Seed, cfg.WarpAmplitude, cfg.WarpFrequency, cfg.WarpIterations)
            : null;

        for (int py = 0; py < size; py++)
        {
            double y = py / (double)(size - 1) * Span;

            for (int px = 0; px < size; px++)
            {
                double x = px / (double)(size - 1) * Span;

                // Displace into fresh locals: writing back into x and y would carry one pixel's warp
                // into the next, because y is only recomputed once per row.
                double sx = x, sy = y;
                warp?.Displace(x, y, out sx, out sy);

                indices[(py * size) + px] = worley is not null
                    ? SampleWorley(worley, cfg, sx, sy)
                    : Quantise(Shape(cfg, field.Sample(sx, sy)));
            }
        }

        return indices;
    }

    private static INoise2 BuildCoherent(NoiseConfig cfg)
    {
        INoise2 source = cfg.Field switch
        {
            FieldKind.Simplex => new SimplexNoise(cfg.Seed),
            FieldKind.Value => new ValueNoise(cfg.Seed),
            _ => new PerlinNoise(cfg.Seed),
        };

        // One octave is still routed through the fractal wrapper: it is where frequency lives, so the
        // zoom control behaves identically whether or not octaves are stacked.
        return new FractalNoise2(source, cfg.Octaves, cfg.Frequency, cfg.Lacunarity, cfg.Gain, cfg.Mode);
    }

    private static byte SampleWorley(WorleyNoise worley, NoiseConfig cfg, double x, double y)
    {
        var sample = worley.SampleCellular(x * cfg.Frequency, y * cfg.Frequency);

        if (cfg.Output == WorleyOutput.Cells)
        {
            // The hash is the cell's identity, so it indexes the palette directly rather than being
            // treated as a magnitude — neighbouring cells should not shade into one another.
            return (byte)(sample.CellHash % 256u);
        }

        double value = cfg.Output == WorleyOutput.F1
            ? sample.F1
            : sample.F2 - sample.F1;

        // Both distances leave the library unbounded above; the useful range is the near one.
        return Quantise(Shape(cfg, System.Math.Clamp((value * 2.0) - 1.0, -1.0, 1.0)));
    }

    /// <summary>Applies the shaping curve. <paramref name="signed"/> and the result are both in [-1, 1].</summary>
    private static double Shape(NoiseConfig cfg, double signed) => cfg.Curve switch
    {
        ShapingCurve.Ridge => (NoiseCurves.Ridge(signed, 1.0 + (cfg.CurveAmount * 5.0)) * 2.0) - 1.0,
        ShapingCurve.Continentalness => NoiseCurves.Continentalness(signed, cfg.CurveAmount, 0.5),
        ShapingCurve.Terrace => (NoiseCurves.Terrace((signed + 1.0) * 0.5, cfg.TerraceSteps, cfg.CurveAmount) * 2.0) - 1.0,
        _ => signed,
    };

    private static byte Quantise(double signed)
    {
        double unit = (System.Math.Clamp(signed, -1.0, 1.0) + 1.0) * 0.5;
        return (byte)System.Math.Clamp(System.Math.Round(unit * 255.0), 0.0, 255.0);
    }

    /// <summary>
    /// The C# that builds the field as currently configured, shown on the page so the controls read as
    /// library calls rather than as magic dials.
    /// </summary>
    public static string DescribeAsCode(NoiseConfig cfg)
    {
        if (cfg is null) throw new ArgumentNullException(nameof(cfg));

        var lines = new List<string>();

        if (cfg.Field == FieldKind.Worley)
        {
            lines.Add($"var field = new WorleyNoise({cfg.Seed}, DistanceMetric.{cfg.Metric}, jitter: {N(cfg.Jitter)});");
            lines.Add(cfg.Output switch
            {
                WorleyOutput.F1 => "var v = field.SampleCellular(x, y).F1;",
                WorleyOutput.Cells => "var v = field.SampleCellular(x, y).CellHash;",
                _ => "var s = field.SampleCellular(x, y);\nvar v = s.F2 - s.F1;",
            });
        }
        else
        {
            string source = cfg.Field switch
            {
                FieldKind.Simplex => $"new SimplexNoise({cfg.Seed})",
                FieldKind.Value => $"new ValueNoise({cfg.Seed})",
                _ => $"new PerlinNoise({cfg.Seed})",
            };

            lines.Add($"var field = new FractalNoise2(");
            lines.Add($"    {source},");
            lines.Add($"    octaves: {cfg.Octaves}, frequency: {N(cfg.Frequency)},");
            lines.Add($"    lacunarity: {N(cfg.Lacunarity)}, gain: {N(cfg.Gain)},");
            lines.Add($"    mode: FractalMode.{cfg.Mode});");
            lines.Add("var v = field.Sample(x, y);");
        }

        if (cfg.WarpAmplitude > 0.0)
        {
            lines.Insert(lines.Count - 1,
                $"var warp = DomainWarp2.Simplex(field, {cfg.Seed}, amplitude: {N(cfg.WarpAmplitude)}, " +
                $"frequency: {N(cfg.WarpFrequency)}, iterations: {cfg.WarpIterations});");
        }

        lines.Add(cfg.Curve switch
        {
            ShapingCurve.Ridge => $"v = NoiseCurves.Ridge(v, sharpness: {N(1.0 + (cfg.CurveAmount * 5.0))});",
            ShapingCurve.Continentalness => $"v = NoiseCurves.Continentalness(v, flatness: {N(cfg.CurveAmount)});",
            ShapingCurve.Terrace => $"v = NoiseCurves.Terrace(v, steps: {cfg.TerraceSteps}, sharpness: {N(cfg.CurveAmount)});",
            _ => string.Empty,
        });

        return string.Join("\n", lines.Where(l => !string.IsNullOrEmpty(l)));
    }

    private static string N(double v) => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}
