namespace RP.Math.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for <see cref="TriangleSurface"/>: area, volume, centroid and closedness of triangle surfaces.</summary>
    [TestClass]
    public class TriangleSurfaceTests
    {
        // a 2 x 3 x 4 box's corners and its 12 triangles, wound anticlockwise seen from outside
        private static readonly Vector3d[] BoxCorners =
        {
            new Vector3d(0, 0, 0), new Vector3d(2, 0, 0), new Vector3d(2, 3, 0), new Vector3d(0, 3, 0),
            new Vector3d(0, 0, 4), new Vector3d(2, 0, 4), new Vector3d(2, 3, 4), new Vector3d(0, 3, 4),
        };

        private static readonly (int A, int B, int C)[] BoxFaces =
        {
            (0, 2, 1), (0, 3, 2), (4, 5, 6), (4, 6, 7), (0, 1, 5), (0, 5, 4),
            (1, 2, 6), (1, 6, 5), (2, 3, 7), (2, 7, 6), (3, 0, 4), (3, 4, 7),
        };

        private static IEnumerable<(Vector3d A, Vector3d B, Vector3d C)> Soup(Vector3d[] corners, IEnumerable<(int A, int B, int C)> faces, Vector3d offset) =>
            faces.Select(f => (corners[f.A] + offset, corners[f.B] + offset, corners[f.C] + offset));

        [TestMethod, TestCategory("TriangleSurface")]
        public void ABox_HasItsVolumeAreaAndCentre_WhereverItIs_Test()
        {
            foreach (var offset in new[] { Vector3d.Zero, new Vector3d(100, -50, 7) })
            {
                var box = Soup(BoxCorners, BoxFaces, offset).ToList();
                Assert.AreEqual(24, TriangleSurface.SignedVolume(box), 1e-9, "the origin can be anywhere: the outside cancels");
                Assert.AreEqual(2 * ((2 * 3) + (3 * 4) + (2 * 4)), TriangleSurface.SurfaceArea(box), 1e-9);
                var centre = TriangleSurface.Centroid(box)!.Value;
                Assert.AreEqual(1 + offset.X, centre.X, 1e-9);
                Assert.AreEqual(1.5 + offset.Y, centre.Y, 1e-9);
                Assert.AreEqual(2 + offset.Z, centre.Z, 1e-9);
            }
        }

        [TestMethod, TestCategory("TriangleSurface")]
        public void InsideOut_GivesANegativeVolume_Test()
        {
            var flipped = BoxFaces.Select(f => (f.A, f.C, f.B)).ToList();
            Assert.AreEqual(-24, TriangleSurface.SignedVolume(Soup(BoxCorners, flipped, Vector3d.Zero)), 1e-9);
            Assert.IsTrue(TriangleSurface.IsClosed(flipped), "consistently inside out is still closed");
        }

        [TestMethod, TestCategory("TriangleSurface")]
        public void Normal_FollowsTheRightHandRule_Test()
        {
            var n = TriangleSurface.Normal(new Vector3d(0, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
            Assert.AreEqual(1, n.Z, 1e-12);
            Assert.AreEqual(0.5, TriangleSurface.Area(new Vector3d(0, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0)), 1e-12);
            Assert.AreEqual(0, TriangleSurface.Normal(new Vector3d(0, 0, 0), new Vector3d(1, 0, 0), new Vector3d(2, 0, 0)).Magnitude, "no area, no direction");
        }

        [TestMethod, TestCategory("TriangleSurface")]
        public void HolesAndFlippedTriangles_ShowAsBadEdges_Test()
        {
            Assert.AreEqual(0, TriangleSurface.BadEdges(BoxFaces).Count);

            var open = BoxFaces.Skip(2).ToList(); // the bottom taken off: a square hole
            Assert.IsFalse(TriangleSurface.IsClosed(open));
            Assert.AreEqual(4, TriangleSurface.BadEdges(open).Count, "the four edges round the hole");

            var oneFlipped = BoxFaces.ToList();
            oneFlipped[0] = (0, 1, 2);
            Assert.IsFalse(TriangleSurface.IsClosed(oneFlipped));
        }

        [TestMethod, TestCategory("TriangleSurface")]
        public void EulersFormula_CountsTheHolesThrough_Test()
        {
            Assert.AreEqual(2, TriangleSurface.EulerCharacteristic(BoxFaces), "8 corners - 18 edges + 12 faces");
            Assert.AreEqual(0, TriangleSurface.Genus(BoxFaces));

            var ring = Ring(8, 6);
            Assert.IsTrue(TriangleSurface.IsClosed(ring));
            Assert.AreEqual(0, TriangleSurface.EulerCharacteristic(ring));
            Assert.AreEqual(1, TriangleSurface.Genus(ring), "a ring has one hole through it");
        }

        [TestMethod, TestCategory("TriangleSurface")]
        public void ARing_EnclosesAboutTheTorusVolume_Test()
        {
            // a torus of tube radius 1 round a circle of radius 3: volume 2 pi^2 R r^2, approached as the grid gets finer
            const double R = 3, r = 1;
            int around = 96, tube = 48;
            var corners = new List<Vector3d>();
            for (int i = 0; i < around; i++)
            {
                for (int j = 0; j < tube; j++)
                {
                    double u = 2 * Math.PI * i / around, v = 2 * Math.PI * j / tube;
                    corners.Add(new Vector3d((R + (r * Math.Cos(v))) * Math.Cos(u), (R + (r * Math.Cos(v))) * Math.Sin(u), r * Math.Sin(v)));
                }
            }

            var faces = Ring(around, tube);
            double volume = TriangleSurface.SignedVolume(Soup(corners.ToArray(), faces, Vector3d.Zero));
            Assert.AreEqual(2 * Math.PI * Math.PI * R * r * r, volume, 0.5, "flat facets sit just inside the curved surface");
            Assert.IsTrue(volume > 0, "wound outwards");
        }

        // a torus-shaped grid of quads, each split into two triangles, wound outwards
        private static List<(int A, int B, int C)> Ring(int around, int tube)
        {
            int At(int i, int j) => ((i % around) * tube) + (j % tube);
            var faces = new List<(int, int, int)>();
            for (int i = 0; i < around; i++)
            {
                for (int j = 0; j < tube; j++)
                {
                    faces.Add((At(i, j), At(i + 1, j), At(i + 1, j + 1)));
                    faces.Add((At(i, j), At(i + 1, j + 1), At(i, j + 1)));
                }
            }

            return faces;
        }
    }
}
