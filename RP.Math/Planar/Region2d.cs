namespace RP.Math
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>How overlapping outlines decide what is inside.</summary>
    public enum FillRule
    {
        /// <summary>
        /// Inside where the outlines wind round a point any number of times other than zero. With outer outlines
        /// anticlockwise and holes clockwise, holes cancel the outline round them and come out empty.
        /// </summary>
        NonZero,

        /// <summary>Inside where a ray from the point crosses the outlines an odd number of times, whichever way they run.</summary>
        EvenOdd,

        /// <summary>
        /// Inside only where the winding number is above zero: an outline running the "wrong" way (clockwise) adds
        /// nothing and takes away from what it overlaps. Offsetting uses it to throw away the inside-out loops a
        /// shrunk outline leaves behind.
        /// </summary>
        Positive,
    }

    /// <summary>
    /// A <b>region of the plane</b> made of several outlines: shapes, the holes in them, islands in the holes.
    /// It is what a cross-section, a letter with a counter (like "o"), or the result of joining two shapes looks
    /// like. By convention outer outlines run anticlockwise and holes clockwise, so the <see cref="FillRule.NonZero"/>
    /// rule and a plain sum of signed areas both come out right.
    /// </summary>
    public sealed class Region2d : IReadOnlyList<Polygon2d>
    {
        private readonly Polygon2d[] outlines;

        public Region2d(IEnumerable<Polygon2d> outlines, FillRule fillRule = FillRule.NonZero)
        {
            this.outlines = (outlines ?? throw new ArgumentNullException(nameof(outlines))).ToArray();
            this.FillRule = fillRule;
        }

        public Region2d(params Polygon2d[] outlines)
            : this((IEnumerable<Polygon2d>)outlines)
        {
        }

        /// <summary>A region with nothing in it.</summary>
        public static Region2d Empty { get; } = new Region2d();

        public FillRule FillRule { get; }

        public int Count => this.outlines.Length;

        public Polygon2d this[int index] => this.outlines[index];

        public bool IsEmpty => this.outlines.Length == 0;

        /// <summary>The outer outlines (anticlockwise).</summary>
        public IEnumerable<Polygon2d> Outers => this.outlines.Where(o => o.Winding == Winding.Anticlockwise);

        /// <summary>The holes (clockwise).</summary>
        public IEnumerable<Polygon2d> Holes => this.outlines.Where(o => o.Winding == Winding.Clockwise);

        /// <summary>
        /// The area covered: outer outlines add their area and holes take theirs away - the signed areas simply
        /// sum. (Exact for clean regions, such as an <see cref="IPolygonClipper"/> returns; overlapping outlines
        /// should be joined with <see cref="IPolygonClipper.Union"/> first.)
        /// </summary>
        public double Area => this.outlines.Sum(o => o.SignedArea);

        public Box2d Bounds => this.outlines.Aggregate(Box2d.Empty, (box, o) => box.Union(o.Bounds));

        public double Perimeter => this.outlines.Sum(o => o.Perimeter);

        /// <summary>
        /// True when the point is inside under the region's fill rule: the outlines' winding numbers are added,
        /// and the total (or, for even-odd, its parity) decides.
        /// </summary>
        public bool Contains(Vector2d point)
        {
            int winding = this.outlines.Sum(o => o.WindingNumber(point));
            return IsInside(winding, this.FillRule);
        }

        /// <summary>Whether a winding number counts as inside under a fill rule.</summary>
        public static bool IsInside(int winding, FillRule fillRule)
        {
            switch (fillRule)
            {
                case FillRule.EvenOdd:
                    return winding % 2 != 0;
                case FillRule.Positive:
                    return winding > 0;
                default:
                    return winding != 0;
            }
        }

        public IEnumerator<Polygon2d> GetEnumerator() => ((IEnumerable<Polygon2d>)this.outlines).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public override string ToString() => $"Region2d({this.outlines.Length} outlines, area {this.Area:G4})";
    }

    /// <summary>How an offset outline turns a corner.</summary>
    public enum JoinStyle
    {
        /// <summary>Extends the edges until they meet in a sharp point (cut off if it would reach too far).</summary>
        Miter,

        /// <summary>Goes round the corner on an arc: the shape a round tool would cut.</summary>
        Round,

        /// <summary>Cuts the corner off square.</summary>
        Square,
    }

    /// <summary>
    /// <b>Boolean operations and offsetting</b> for arbitrary regions: joining, subtracting and overlapping shapes,
    /// and growing or shrinking them by a distance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These sound simple and are famously hard to get right: outlines that touch at a corner, share an edge,
    /// cross many times or nearly coincide all need care, and floating-point rounding turns "nearly" into
    /// "sometimes". <see cref="PolygonClipper"/> does them here with an <i>overlay</i>: cut every edge where it
    /// crosses another, decide for each piece whether the result is inside on one side and outside on the
    /// other, and join the pieces that are into outlines. It is written to be read. Production libraries such
    /// as Angus Johnson's Clipper (Vatti's sweep-line algorithm) are faster on very large inputs; the
    /// interface lets either be used.
    /// </para>
    /// <para>
    /// For the special case of clipping to a <i>convex</i> window, <see cref="Polygon2d.ClipTo"/> does it here,
    /// with the Sutherland-Hodgman algorithm.
    /// </para>
    /// <para>
    /// Results come back clean: no overlaps, outer outlines anticlockwise, holes clockwise.
    /// </para>
    /// </remarks>
    public interface IPolygonClipper
    {
        /// <summary>Everything covered by any of the outlines, read with the fill rule.</summary>
        Region2d Union(IEnumerable<IReadOnlyList<Vector2d>> outlines, FillRule fillRule = FillRule.NonZero);

        /// <summary>What is in <paramref name="subject"/> but not in <paramref name="clip"/>.</summary>
        Region2d Difference(IEnumerable<IReadOnlyList<Vector2d>> subject, IEnumerable<IReadOnlyList<Vector2d>> clip);

        /// <summary>What is in both.</summary>
        Region2d Intersection(IEnumerable<IReadOnlyList<Vector2d>> subject, IEnumerable<IReadOnlyList<Vector2d>> clip);

        /// <summary>What is in one or the other but not both.</summary>
        Region2d Xor(IEnumerable<IReadOnlyList<Vector2d>> subject, IEnumerable<IReadOnlyList<Vector2d>> clip);

        /// <summary>The region grown outwards by <paramref name="delta"/> (shrunk inwards when negative).</summary>
        Region2d Offset(IEnumerable<IReadOnlyList<Vector2d>> outlines, double delta, JoinStyle join = JoinStyle.Miter);

        /// <summary>The pieces of open lines that lie inside a region: hatching, scan lines, a path cut to a boundary.</summary>
        IReadOnlyList<Polyline2d> ClipLines(IEnumerable<IReadOnlyList<Vector2d>> lines, IEnumerable<IReadOnlyList<Vector2d>> region);
    }
}
