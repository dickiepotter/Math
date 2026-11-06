namespace RP.Math.Tests
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the plane geometry: <see cref="Box2d"/>, <see cref="Segment2d"/>, <see cref="Polygon2d"/>, <see cref="Polyline2d"/> and <see cref="Region2d"/>.</summary>
    [TestClass]
    public class PlanarTests
    {
        private const double Tolerance = 1e-9;

        private static Vector2d V(double x, double y) => new Vector2d(x, y);

        // an L shape, anticlockwise: a 2 x 2 square with its top-right 1 x 1 quarter missing, area 3
        private static readonly Polygon2d L = new Polygon2d(V(0, 0), V(2, 0), V(2, 1), V(1, 1), V(1, 2), V(0, 2));

        [TestMethod, TestCategory("Box2d")]
        public void Box_MeasuresGrowsAndOverlaps_Test()
        {
            var box = new Box2d(V(4, 3), V(1, 1));
            Assert.AreEqual(V(1, 1), box.Min);
            Assert.AreEqual(6, box.Area, Tolerance);
            Assert.AreEqual(V(2.5, 2), box.Centre);
            Assert.IsTrue(box.Contains(V(4, 3)));
            Assert.IsFalse(box.Contains(V(5, 3)));

            var other = Box2d.FromCornerAndSize(3, 2, 5, 5);
            Assert.IsTrue(box.Intersects(other));
            Assert.AreEqual(new Box2d(V(3, 2), V(4, 3)), box.Intersection(other));
            Assert.IsFalse(box.Intersects(Box2d.FromCornerAndSize(10, 10, 1, 1)));
            Assert.AreEqual(V(4, 2), box.ClosestPoint(V(9, 2)));
        }

        [TestMethod, TestCategory("Box2d")]
        public void Box_EmptyIsTheStartingPointForMeasuring_Test()
        {
            Assert.IsTrue(Box2d.Empty.IsEmpty);
            Assert.AreEqual(0, Box2d.Empty.Area);
            Assert.AreEqual(new Box2d(V(2, 3), V(2, 3)), Box2d.Empty.Include(V(2, 3)));
            Assert.AreEqual(new Box2d(V(-1, 0), V(3, 5)), Box2d.FromPoints(new[] { V(0, 5), V(3, 0), V(-1, 2) }));
        }

        [TestMethod, TestCategory("Segment2d")]
        public void Segment_FindsTheClosestPointAndTheSide_Test()
        {
            var s = new Segment2d(V(0, 0), V(10, 0));
            Assert.AreEqual(V(4, 0), s.ClosestPoint(V(4, 7)));
            Assert.AreEqual(V(10, 0), s.ClosestPoint(V(15, 1)), "past the end, the end is closest");
            Assert.AreEqual(3, s.DistanceTo(V(-3, 0)), Tolerance);
            Assert.IsTrue(s.SideOf(V(5, 1)) > 0, "above is to the left looking along +x");
            Assert.IsTrue(s.SideOf(V(5, -1)) < 0);
        }

        [TestMethod, TestCategory("Segment2d")]
        public void Segment_Intersect_HandlesCrossingParallelAndOverlap_Test()
        {
            var a = new Segment2d(V(0, 0), V(4, 4));
            Assert.AreEqual(SegmentIntersectionKind.Point, a.Intersect(new Segment2d(V(0, 4), V(4, 0)), out var p));
            Assert.AreEqual(V(2, 2), p);

            Assert.AreEqual(SegmentIntersectionKind.None, a.Intersect(new Segment2d(V(1, 0), V(5, 4)), out _), "parallel");
            Assert.AreEqual(SegmentIntersectionKind.None, a.Intersect(new Segment2d(V(3, 0), V(4, 0)), out _), "would cross only if extended");
            Assert.AreEqual(SegmentIntersectionKind.Overlap, a.Intersect(new Segment2d(V(2, 2), V(6, 6)), out var start));
            Assert.AreEqual(V(2, 2), start);
            Assert.AreEqual(SegmentIntersectionKind.Point, a.Intersect(new Segment2d(V(4, 4), V(9, 9)), out _), "end to end on one line");
        }

        [TestMethod, TestCategory("Polygon2d")]
        public void Polygon_ShoelaceAreaAndWinding_Test()
        {
            Assert.AreEqual(3, L.SignedArea, Tolerance);
            Assert.AreEqual(Winding.Anticlockwise, L.Winding);
            Assert.AreEqual(-3, L.Reversed().SignedArea, Tolerance);
            Assert.AreEqual(Winding.Clockwise, L.Reversed().Winding);
            Assert.AreEqual(8, L.Perimeter, Tolerance);
            Assert.AreEqual(new Box2d(V(0, 0), V(2, 2)), L.Bounds);
        }

        [TestMethod, TestCategory("Polygon2d")]
        public void Polygon_CentroidIsTheBalancePoint_Test()
        {
            Assert.AreEqual(V(1, 1), Polygon2d.Rectangle(0, 0, 2, 2).Centroid);

            // the L is three unit squares centred at (0.5, 0.5), (1.5, 0.5) and (0.5, 1.5)
            var c = L.Centroid;
            Assert.AreEqual(2.5 / 3, c.X, Tolerance);
            Assert.AreEqual(2.5 / 3, c.Y, Tolerance);
        }

        [TestMethod, TestCategory("Polygon2d")]
        public void Polygon_ConvexAndSimple_Test()
        {
            Assert.IsTrue(Polygon2d.Rectangle(0, 0, 3, 1).IsConvex);
            Assert.IsTrue(Polygon2d.Regular(V(0, 0), 1, 7).IsConvex);
            Assert.IsFalse(L.IsConvex);
            Assert.IsTrue(L.IsSimple);

            var bowTie = new Polygon2d(V(0, 0), V(2, 2), V(2, 0), V(0, 2));
            Assert.IsFalse(bowTie.IsSimple);
        }

        [TestMethod, TestCategory("Polygon2d")]
        public void Polygon_WindingNumberDecidesInside_Test()
        {
            Assert.IsTrue(L.Contains(V(0.5, 1.5)));
            Assert.IsFalse(L.Contains(V(1.5, 1.5)), "the missing quarter");
            Assert.IsFalse(L.Contains(V(3, 0.5)));
            Assert.AreEqual(1, L.WindingNumber(V(0.5, 0.5)));
            Assert.AreEqual(-1, L.Reversed().WindingNumber(V(0.5, 0.5)), "the sign follows the winding");
            Assert.IsTrue(L.IsOnBoundary(V(1, 1.5)));
            Assert.AreEqual(V(1, 1.5), L.ClosestPointOnBoundary(V(1.4, 1.5)));
        }

        [TestMethod, TestCategory("Polygon2d")]
        public void Polygon_EarClippingGivesNMinusTwoTrianglesCoveringTheArea_Test()
        {
            foreach (var polygon in new[] { L, L.Reversed(), Polygon2d.Regular(V(3, 3), 2, 9) })
            {
                var triangles = polygon.Triangulate();
                Assert.AreEqual(polygon.Count - 2, triangles.Count);
                double total = triangles.Sum(t => new Polygon2d(polygon[t.A], polygon[t.B], polygon[t.C]).SignedArea);
                Assert.AreEqual(polygon.Area, total, 1e-9, "every triangle anticlockwise, together the whole area");
            }
        }

        [TestMethod, TestCategory("Polygon2d")]
        public void Polygon_ConvexHullWrapsThePoints_Test()
        {
            var points = new[] { V(0, 0), V(4, 0), V(4, 4), V(0, 4), V(2, 2), V(1, 3), V(2, 0), V(3, 1) };
            var hull = Polygon2d.ConvexHull(points);
            Assert.IsNotNull(hull);
            Assert.AreEqual(4, hull!.Count, "inner points and points along an edge are not corners");
            Assert.AreEqual(16, hull.SignedArea, Tolerance);
            Assert.IsNull(Polygon2d.ConvexHull(new[] { V(0, 0), V(1, 1), V(2, 2) }), "all on one line");
        }

        [TestMethod, TestCategory("Polygon2d")]
        public void Polygon_SimplifyDropsNearlyStraightCorners_Test()
        {
            // a square with a slight wobble added halfway along each side
            var wobbly = new Polygon2d(V(0, 0), V(5, 0.01), V(10, 0), V(9.99, 5), V(10, 10), V(5, 10.01), V(0, 10), V(0.01, 5));
            var simple = wobbly.Simplify(0.1);
            Assert.AreEqual(4, simple.Count);
            Assert.AreEqual(100, simple.Area, 1e-9);
            Assert.AreEqual(8, wobbly.Simplify(0.001).Count, "a tolerance finer than the wobble keeps it");
        }

        [TestMethod, TestCategory("Polygon2d")]
        public void Polygon_SutherlandHodgmanClipsToAConvexWindow_Test()
        {
            var window = Polygon2d.Rectangle(1, 1, 10, 10);
            var clipped = Polygon2d.Rectangle(0, 0, 4, 4).ClipTo(window);
            Assert.IsNotNull(clipped);
            Assert.AreEqual(9, clipped!.Area, Tolerance);

            var lClipped = L.ClipTo(Polygon2d.Rectangle(0.5, 0.5, 10, 10));
            Assert.AreEqual(1.25, lClipped!.Area, Tolerance, "the L less a half-unit strip on its left and bottom: 0.25 + 0.5 + 0.5");
            Assert.IsNull(Polygon2d.Rectangle(20, 20, 1, 1).ClipTo(window));
            Assert.ThrowsException<System.ArgumentException>(() => window.ClipTo(L));
        }

        [TestMethod, TestCategory("Polyline2d")]
        public void Polyline_MeasuresWalksAndResamples_Test()
        {
            var line = new Polyline2d(V(0, 0), V(3, 0), V(3, 4));
            Assert.AreEqual(7, line.Length, Tolerance);
            Assert.AreEqual(V(3, 1), line.PointAtDistance(4));
            Assert.AreEqual(V(3, 4), line.PointAtDistance(99));
            var even = line.Resample(1);
            Assert.AreEqual(8, even.Count);
            Assert.AreEqual(V(3, 4), even.End);
            Assert.AreEqual(2, line.DistanceTo(V(1, 2)), Tolerance);
        }

        [TestMethod, TestCategory("Polyline2d")]
        public void Polyline_DouglasPeuckerKeepsTheCorners_Test()
        {
            var line = new Polyline2d(V(0, 0), V(1, 0.05), V(2, -0.05), V(3, 0), V(3, 1), V(3.05, 2), V(3, 3));
            var simple = line.Simplify(0.1);
            CollectionAssert.AreEqual(new[] { V(0, 0), V(3, 0), V(3, 3) }, simple.ToArray());
        }

        [TestMethod, TestCategory("Region2d")]
        public void Region_HolesTakeTheirAreaAway_Test()
        {
            var outer = Polygon2d.Rectangle(0, 0, 10, 10);
            var hole = Polygon2d.Rectangle(2, 2, 3, 3).Reversed();
            var frame = new Region2d(outer, hole);
            Assert.AreEqual(91, frame.Area, Tolerance);
            Assert.IsTrue(frame.Contains(V(1, 1)));
            Assert.IsFalse(frame.Contains(V(3, 3)), "in the hole");
            Assert.AreEqual(1, frame.Holes.Count());

            // with the hole drawn the same way round, only the even-odd rule still sees it as a hole
            var sameWay = new Region2d(new[] { outer, Polygon2d.Rectangle(2, 2, 3, 3) }, FillRule.EvenOdd);
            Assert.IsFalse(sameWay.Contains(V(3, 3)));
        }
    }
}
