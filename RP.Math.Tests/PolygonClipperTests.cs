namespace RP.Math.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for <see cref="PolygonClipper"/>: booleans, offsetting and line clipping.</summary>
    [TestClass]
    public class PolygonClipperTests
    {
        private static readonly PolygonClipper Clipper = new PolygonClipper();

        private static readonly Polygon2d A = Polygon2d.Rectangle(0, 0, 4, 4);
        private static readonly Polygon2d B = Polygon2d.Rectangle(2, 2, 4, 4);

        private static Vector2d V(double x, double y) => new Vector2d(x, y);

        private static IReadOnlyList<Vector2d>[] Of(params Polygon2d[] polygons) => polygons.Cast<IReadOnlyList<Vector2d>>().ToArray();

        [TestMethod, TestCategory("PolygonClipper")]
        public void Booleans_GiveTheExpectedAreas_Test()
        {
            Assert.AreEqual(28, Clipper.Union(Of(A, B)).Area, 1e-9, "16 + 16 - the 4 they share");
            Assert.AreEqual(4, Clipper.Intersection(Of(A), Of(B)).Area, 1e-9);
            Assert.AreEqual(12, Clipper.Difference(Of(A), Of(B)).Area, 1e-9);
            Assert.AreEqual(24, Clipper.Xor(Of(A), Of(B)).Area, 1e-9);
        }

        [TestMethod, TestCategory("PolygonClipper")]
        public void Results_AreCleanOutlines_OuterAnticlockwiseHolesClockwise_Test()
        {
            var frame = Clipper.Difference(Of(Polygon2d.Rectangle(0, 0, 10, 10)), Of(Polygon2d.Rectangle(3, 3, 4, 4)));
            Assert.AreEqual(1, frame.Outers.Count());
            Assert.AreEqual(1, frame.Holes.Count());
            Assert.AreEqual(84, frame.Area, 1e-9);
            Assert.AreEqual(4, frame.Outers.Single().Count, "the straight-on corners left by splitting are gone");
            Assert.IsFalse(frame.Contains(V(5, 5)));
            Assert.IsTrue(frame.Contains(V(1, 1)));
        }

        [TestMethod, TestCategory("PolygonClipper")]
        public void SharedEdgesAndTouchingCorners_AreHandled_Test()
        {
            // two squares side by side share an edge: their union is one rectangle
            var side = Clipper.Union(Of(Polygon2d.Rectangle(0, 0, 2, 2), Polygon2d.Rectangle(2, 0, 2, 2)));
            Assert.AreEqual(1, side.Count);
            Assert.AreEqual(4, side[0].Count);
            Assert.AreEqual(8, side.Area, 1e-9);

            // two squares meeting only at a corner stay two outlines
            var corner = Clipper.Union(Of(Polygon2d.Rectangle(0, 0, 2, 2), Polygon2d.Rectangle(2, 2, 2, 2)));
            Assert.AreEqual(2, corner.Count);
            Assert.AreEqual(8, corner.Area, 1e-9);

            // the same square twice is the square
            Assert.AreEqual(16, Clipper.Union(Of(A, A)).Area, 1e-9);
            Assert.AreEqual(16, Clipper.Intersection(Of(A), Of(A)).Area, 1e-9);
            Assert.IsTrue(Clipper.Difference(Of(A), Of(A)).IsEmpty);
        }

        [TestMethod, TestCategory("PolygonClipper")]
        public void FillRules_ReadOverlapsDifferently_Test()
        {
            // a small square drawn inside a big one, the same way round
            var nested = Of(Polygon2d.Rectangle(0, 0, 10, 10), Polygon2d.Rectangle(2, 2, 2, 2));
            Assert.AreEqual(96, Clipper.Union(nested, FillRule.EvenOdd).Area, 1e-9, "even-odd: the inner square is a hole");
            Assert.AreEqual(100, Clipper.Union(nested).Area, 1e-9, "non-zero: it is covered twice, still inside");

            // a bow tie crosses itself: non-zero keeps both lobes
            var bowTie = new Polygon2d(V(0, 0), V(2, 2), V(2, 0), V(0, 2));
            var lobes = Clipper.Union(Of(bowTie));
            Assert.AreEqual(2, lobes.Count);
            Assert.AreEqual(2, lobes.Area, 1e-9);
            Assert.IsTrue(lobes.All(o => o.Winding == Winding.Anticlockwise));
        }

        [TestMethod, TestCategory("PolygonClipper")]
        public void Offset_GrowsAndShrinks_Test()
        {
            Assert.AreEqual(36, Clipper.Offset(Of(A), 1).Area, 1e-9, "mitred corners: a 6 x 6 square");
            Assert.AreEqual(4, Clipper.Offset(Of(A), -1).Area, 1e-9);
            double rounded = Clipper.Offset(Of(A), 1, JoinStyle.Round).Area;
            Assert.AreEqual(16 + 16 + Math.PI, rounded, 0.05, "the square, four strips and a circle's worth of corners (the straight pieces sit just inside the arc)");
            Assert.IsTrue(Clipper.Offset(Of(A), -3).IsEmpty, "shrunk to nothing");

            // an L shrunk: its inner corner stays square, and it does not turn inside out
            var l = new Polygon2d(V(0, 0), V(4, 0), V(4, 2), V(2, 2), V(2, 4), V(0, 4));
            var shrunk = Clipper.Offset(Of(l), -0.5);
            Assert.AreEqual(1, shrunk.Count);
            Assert.AreEqual(5, shrunk.Area, 1e-9, "inset by 0.5: two 3 x 1 strips sharing a 1 x 1 square");
        }

        [TestMethod, TestCategory("PolygonClipper")]
        public void Offset_OfAFrame_ShrinksTheHoleToo_Test()
        {
            var frame = Clipper.Difference(Of(Polygon2d.Rectangle(0, 0, 10, 10)), Of(Polygon2d.Rectangle(3, 3, 4, 4)));
            var thinner = Clipper.Offset(frame, -1);
            Assert.AreEqual((8 * 8) - (6 * 6), thinner.Area, 1e-9, "outer in by 1, hole out by 1");
            var thicker = Clipper.Offset(frame, 1);
            Assert.AreEqual((12 * 12) - (2 * 2), thicker.Area, 1e-9);
        }

        [TestMethod, TestCategory("PolygonClipper")]
        public void ClipLines_KeepsThePiecesInside_Test()
        {
            var pieces = Clipper.ClipLines(new[] { new Polyline2d(V(-5, 1), V(10, 1)) }, Of(A));
            Assert.AreEqual(1, pieces.Count);
            Assert.AreEqual(4, pieces[0].Length, 1e-9);

            // across a frame: two pieces, the hole left out
            var frame = Clipper.Difference(Of(Polygon2d.Rectangle(0, 0, 10, 10)), Of(Polygon2d.Rectangle(3, 3, 4, 4)));
            var across = Clipper.ClipLines(new[] { new Polyline2d(V(-1, 5), V(11, 5)) }, frame);
            Assert.AreEqual(2, across.Count);
            Assert.AreEqual(6, across.Sum(p => p.Length), 1e-9);
        }

        [TestMethod, TestCategory("PolygonClipper")]
        public void RandomShapes_AgreeWithTheIdentities_Test()
        {
            // for any two regions: |A u B| + |A n B| = |A| + |B|, and |A - B| = |A| - |A n B|; snapping every point to
            // the micron grid moves an area by up to its outline's length times the grid, hence the tolerance
            var random = new Random(20261106);
            for (int round = 0; round < 60; round++)
            {
                var a = RandomStar(random);
                var b = RandomStar(random);
                double union = Clipper.Union(Of(a, b)).Area;
                double both = Clipper.Intersection(Of(a), Of(b)).Area;
                double aOnly = Clipper.Difference(Of(a), Of(b)).Area;
                double areaA = Clipper.Union(Of(a)).Area;
                double areaB = Clipper.Union(Of(b)).Area;
                Assert.AreEqual(areaA + areaB, union + both, 1e-4, $"round {round}");
                Assert.AreEqual(areaA - both, aOnly, 1e-4, $"round {round}");
                Assert.AreEqual(union - both, Clipper.Xor(Of(a), Of(b)).Area, 1e-4, $"round {round}");
            }
        }

        // a star-shaped polygon (simple, but often not convex) round a random centre
        private static Polygon2d RandomStar(Random random)
        {
            int n = random.Next(3, 12);
            double cx = random.NextDouble() * 10, cy = random.NextDouble() * 10;
            var points = new List<Vector2d>();
            for (int i = 0; i < n; i++)
            {
                double angle = 2 * Math.PI * i / n;
                double r = 1 + (random.NextDouble() * 5);
                points.Add(new Vector2d(cx + (r * Math.Cos(angle)), cy + (r * Math.Sin(angle))));
            }

            return new Polygon2d(points);
        }
    }
}
