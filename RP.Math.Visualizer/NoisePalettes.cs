namespace VectorVisualizer;

/// <summary>Which colour ramp a rendered field is read through.</summary>
public enum PaletteKind
{
    /// <summary>Black to white — the field as it actually is, with nothing editorialised.</summary>
    Grey,

    /// <summary>Sea, shore, grass, rock and snow, banded by height: the field read as terrain.</summary>
    Terrain,

    /// <summary>Black through red and orange to white — heat, lava, and anything where the top end matters.</summary>
    Ember,

    /// <summary>Deep blue to white: cold, cloud and vapour.</summary>
    Ice,

    /// <summary>A scatter of distinct hues, for reading cell <i>identity</i> rather than magnitude.</summary>
    Cells,
}

/// <summary>
/// The 256-entry colour ramps the noise page renders through.
/// </summary>
/// <remarks>
/// A palette is where a scalar field turns into a picture, and the choice is not decoration: the same
/// Worley field read through <see cref="PaletteKind.Grey"/> is a distance, and read through
/// <see cref="PaletteKind.Cells"/> is a map of regions. Keeping the ramps here rather than in the page
/// means the field is sampled once and can be re-read instantly.
/// </remarks>
public static class NoisePalettes
{
    /// <summary>Builds the 768-byte (256 × RGB) palette for <paramref name="kind"/>.</summary>
    public static byte[] Build(PaletteKind kind) => kind switch
    {
        PaletteKind.Terrain => Ramp(TerrainStops),
        PaletteKind.Ember => Ramp(EmberStops),
        PaletteKind.Ice => Ramp(IceStops),
        PaletteKind.Cells => CellHues(),
        _ => Ramp(GreyStops),
    };

    // Stops are (position 0..1, R, G, B) and are interpolated linearly between.
    private static readonly (double At, int R, int G, int B)[] GreyStops =
    {
        (0.00, 0, 0, 0),
        (1.00, 255, 255, 255),
    };

    private static readonly (double At, int R, int G, int B)[] TerrainStops =
    {
        (0.00, 12, 30, 68),      // deep water
        (0.38, 32, 88, 150),     // shelf
        (0.46, 60, 150, 200),    // shallows
        (0.50, 214, 200, 142),   // sand
        (0.56, 96, 142, 66),     // grass
        (0.70, 54, 96, 48),      // forest
        (0.82, 118, 110, 98),    // rock
        (0.92, 168, 164, 158),   // scree
        (1.00, 255, 255, 255),   // snow
    };

    private static readonly (double At, int R, int G, int B)[] EmberStops =
    {
        (0.00, 8, 6, 12),
        (0.35, 120, 18, 24),
        (0.62, 224, 88, 20),
        (0.85, 250, 196, 64),
        (1.00, 255, 252, 226),
    };

    private static readonly (double At, int R, int G, int B)[] IceStops =
    {
        (0.00, 6, 14, 38),
        (0.45, 30, 78, 140),
        (0.75, 118, 186, 224),
        (1.00, 246, 252, 255),
    };

    private static byte[] Ramp((double At, int R, int G, int B)[] stops)
    {
        var palette = new byte[768];

        for (int i = 0; i < 256; i++)
        {
            double t = i / 255.0;

            // Find the span this index falls in. The table is short enough that a scan beats anything cleverer.
            int upper = 1;
            while (upper < stops.Length - 1 && stops[upper].At < t)
            {
                upper++;
            }

            var a = stops[upper - 1];
            var b = stops[upper];
            double width = b.At - a.At;
            double f = width <= 0 ? 0 : System.Math.Clamp((t - a.At) / width, 0.0, 1.0);

            palette[i * 3] = Lerp(a.R, b.R, f);
            palette[(i * 3) + 1] = Lerp(a.G, b.G, f);
            palette[(i * 3) + 2] = Lerp(a.B, b.B, f);
        }

        return palette;
    }

    /// <summary>
    /// A palette of visually separated hues, so adjacent cells almost never share a colour.
    /// </summary>
    /// <remarks>
    /// The golden-ratio step around the hue circle is the standard trick for this: consecutive indices
    /// land far apart, so a cell map reads as distinct regions rather than a gradient.
    /// </remarks>
    private static byte[] CellHues()
    {
        var palette = new byte[768];

        for (int i = 0; i < 256; i++)
        {
            double hue = (i * 0.618033988749895) % 1.0;
            double value = 0.62 + (((i * 7) % 5) * 0.07);
            var (r, g, b) = HsvToRgb(hue, 0.58, value);
            palette[i * 3] = r;
            palette[(i * 3) + 1] = g;
            palette[(i * 3) + 2] = b;
        }

        return palette;
    }

    private static (byte R, byte G, byte B) HsvToRgb(double h, double s, double v)
    {
        double i = System.Math.Floor(h * 6.0);
        double f = (h * 6.0) - i;
        double p = v * (1.0 - s);
        double q = v * (1.0 - (f * s));
        double t = v * (1.0 - ((1.0 - f) * s));

        double r, g, b;
        switch (((int)i) % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }

        return (Byte255(r), Byte255(g), Byte255(b));
    }

    private static byte Lerp(int a, int b, double f) => Byte255((a + ((b - a) * f)) / 255.0);

    private static byte Byte255(double unit) => (byte)System.Math.Clamp(System.Math.Round(unit * 255.0), 0.0, 255.0);
}
