# RP.Math — Interactive Visualizer

A small **Blazor WebAssembly** app for exploring the `RP.Math` library. It runs the
**actual compiled library** in the browser, so the visualisation and every result reflect the
real code — not a re-implementation.

![Operations covered](https://img.shields.io/badge/operations-67-4be0a8)
![Noise fields](https://img.shields.io/badge/noise%20fields-Perlin%2C%20Simplex%2C%20Value%2C%20Worley-4be0a8)

Two pages, reached from the bar at the top:

| Page | What it explores |
|---|---|
| **Vectors & geometry** (`/`) | `Vector`, the rotation types and the line/plane family, drawn in a 3D scene |
| **Noise fields** (`/noise`) | `RP.Math.Noise` — Perlin, Simplex, value and Worley fields, rendered as an image |

## Vectors & geometry

- Pick any operation from a grouped list. Coverage spans several `RP.Math` types:
  - **Vector** — arithmetic, products, geometry, components, predicates, comparison.
  - **Rotation types** — `Quaternion`, `Matrix`, `AxisAngle`, `Rotation` and `Pose`, each rotating
    (or transforming) **A** about axis **B** so you can watch the representations agree.
  - **Lines & planes** — `Line`, `Ray`, `LineSegment` and `Plane` built from **B**/**C**, with
    closest-point, projection, reflection and signed-distance queries drawn in the scene.
- Adjust vectors **A**, **B**, **C** with sliders/number boxes, or **drag a vector's tip** in the
  3D scene. Drag the background to **orbit**, scroll to **zoom**.
- See the result recompute live: vectors are drawn as arrows, positions as point markers, and the
  defining line/plane/segment is drawn alongside; scalars / booleans / integers appear in the result
  card next to the formula and a plain-English explanation.

## Noise fields

`RP.Math.Noise` is not something you can draw with an arrow, so it gets its own page and its own kind
of picture: the field is sampled across a square and shown as an image. Every pixel is one real call
into the compiled library — there is no shader and no pre-baked texture anywhere in this app.

- **Four fields.** Perlin and Simplex (gradient), Value (lattice) and Worley (cellular). A Worley
  sample can be read three ways — `F1` for blobs, `F2 - F1` for the cracks between cells, or the cell
  hash for a map of regions — because one lookup answers all three.
- **Fractal stacking.** Octaves, lacunarity and gain, in all four `FractalMode`s (Brownian, Billow,
  Ridged, Turbulence), so the difference between rolling terrain and a knife-edged mountain range is
  one dropdown.
- **Domain warping.** `DomainWarp2` displaces where the field is sampled — the thing that turns
  recognisably generated noise into something that looks eroded.
- **Shaping curves.** `NoiseCurves.Terrace`, `Ridge` and `Continentalness`, which are what buy flat
  ground to build on without flattening the mountains.
- **Palettes.** The same field read as grey, terrain, heat, ice, or — for cell hashes — as distinct
  regions, because how a scalar field is coloured decides what it appears to *be*.

The panel shows **the calls behind the image**: the actual `RP.Math.Noise` constructor arguments for
whatever the dials currently say, so the controls read as library usage rather than as magic.

Detail is selectable (128–320 square). It is the one control that costs time rather than changing the
picture, since every pixel is a library call: drag the sliders at 128, then raise it once the look
has settled.

## Run it

```bash
dotnet run --project RP.Math.Visualizer
```

Then open the printed `http://localhost:<port>` URL.

## Publish (static site)

```bash
dotnet publish RP.Math.Visualizer -c Release
```

The output under `bin/Release/net8.0/publish/wwwroot` is a static site that can be hosted anywhere
(GitHub Pages, any static host).

`.github/workflows/pages.yml` does this on every push to `master` and deploys the result to GitHub
Pages, after running the library's tests. It sets `<base href>` to the repository's Pages path and
adds a `404.html` copy of `index.html`, so opening `/noise` directly still reaches the app. To switch
it on once, open the repository's **Settings → Pages** and set **Build and deployment → Source** to
**GitHub Actions**. The visualizer then appears at `https://<owner>.github.io/<repository>/`.

## How it's wired

- `Operations.cs` — a data-driven catalogue of every operation, each with a `Func<OpContext, OpResult>`
  that calls the real `Vector` method.
- `NoiseFields.cs` — the noise page's configuration, and the sampling loop that turns it into an index
  raster. It decides *where* to sample and nothing else; every value comes from the library.
- `NoisePalettes.cs` — the 256-entry colour ramps, kept apart from sampling so that changing palette
  does not re-sample the field.
- `NoiseRaster.cs` — a minimal indexed-PNG encoder, so the field can be shown as a plain `<img>` with
  no JavaScript interop and no canvas.
- `Geometry.cs` — a tiny orthographic camera (kept independent of the library) that projects the 3D
  scene onto the SVG canvas.
- `Components/VectorEditor.razor` — the reusable slider/number editor for a vector.
- `Pages/Home.razor` — the scene (inline SVG) and the control panel.
