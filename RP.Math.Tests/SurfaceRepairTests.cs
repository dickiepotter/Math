namespace RP.Math.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for <see cref="SurfaceRepair"/>: welding, cleaning, orienting and closing triangle surfaces.</summary>
    [TestClass]
    public class SurfaceRepairTests
    {
        // a 2 x 3 x 4 box, wound anticlockwise seen from outside
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

        private static double Volume(IReadOnlyList<Vector3d> corners, IEnumerable<(int A, int B, int C)> faces) =>
            TriangleSurface.SignedVolume(faces.Select(f => (corners[f.A], corners[f.B], corners[f.C])));

        [TestMethod, TestCategory("SurfaceRepair")]
        public void Weld_JoinsCornersThatNearlyMeet_Test()
        {
            // as an STL file stores it: every face with three corners of its own, a hair out
            var corners = new List<Vector3d>();
            var faces = new List<(int A, int B, int C)>();
            int n = 0;
            foreach (var f in BoxFaces)
            {
                foreach (int i in new[] { f.A, f.B, f.C })
                {
                    corners.Add(BoxCorners[i] + new Vector3d(1e-6 * (n % 3), 0, 0));
                    n++;
                }

                faces.Add((corners.Count - 3, corners.Count - 2, corners.Count - 1));
            }

            Assert.IsFalse(TriangleSurface.IsClosed(faces), "no two faces share a corner yet");
            int[] kept = SurfaceRepair.Weld(corners, faces, 1e-4);
            Assert.AreEqual(8, kept.Length);
            Assert.IsTrue(TriangleSurface.IsClosed(faces));
            Assert.AreEqual(24, Volume(kept.Select(i => corners[i]).ToList(), faces), 1e-4);
        }

        [TestMethod, TestCategory("SurfaceRepair")]
        public void BrokenAndDoubledFaces_AreDropped_Test()
        {
            var faces = BoxFaces.ToList();
            faces.Add((0, 0, 1));             // uses a corner twice
            faces.Add((0, 1, 1));
            faces.Add((2, 1, 0));             // the first face again, the other way round
            faces.Add((0, 2, 1));             // and the same way
            var corners = BoxCorners.Concat(new[] { new Vector3d(1, 0, 0) }).ToList();
            faces.Add((0, 8, 1));             // three corners on one line: no area
            Assert.AreEqual(3, SurfaceRepair.RemoveDegenerate(corners, faces));
            Assert.AreEqual(2, SurfaceRepair.RemoveDuplicates(faces));
            CollectionAssert.AreEqual(BoxFaces, faces);
        }

        [TestMethod, TestCategory("SurfaceRepair")]
        public void Orient_TurnsStrayFacesAndInsideOutPieces_Test()
        {
            var faces = BoxFaces.ToList();
            faces[3] = (faces[3].A, faces[3].C, faces[3].B);
            faces[8] = (faces[8].A, faces[8].C, faces[8].B);
            Assert.IsTrue(SurfaceRepair.MisorientedEdgeCount(faces) > 0);
            Assert.AreEqual(2, SurfaceRepair.OrientConsistently(BoxCorners, faces));
            Assert.AreEqual(0, SurfaceRepair.MisorientedEdgeCount(faces));
            Assert.AreEqual(24, Volume(BoxCorners, faces), 1e-9);

            var insideOut = BoxFaces.Select(f => (f.A, f.C, f.B)).ToList();
            Assert.AreEqual(12, SurfaceRepair.OrientConsistently(BoxCorners, insideOut), "the whole piece turned round");
            Assert.AreEqual(24, Volume(BoxCorners, insideOut), 1e-9);
        }

        [TestMethod, TestCategory("SurfaceRepair")]
        public void FillHoles_ClosesSmallHolesAndLeavesBigOnes_Test()
        {
            var open = BoxFaces.Skip(2).ToList(); // no bottom: one four-edged hole
            Assert.AreEqual(4, SurfaceRepair.OpenEdgeCount(open));
            var loops = SurfaceRepair.HoleLoops(open);
            Assert.AreEqual(1, loops.Count);
            Assert.AreEqual(4, loops[0].Count);

            var tooSmall = new List<(int A, int B, int C)>(open);
            Assert.AreEqual((0, 1), SurfaceRepair.FillHoles(BoxCorners.ToList(), tooSmall, largestHole: 3));

            var corners = BoxCorners.ToList();
            Assert.AreEqual((1, 0), SurfaceRepair.FillHoles(corners, open));
            Assert.AreEqual(9, corners.Count, "a new corner in the middle of the hole");
            Assert.IsTrue(TriangleSurface.IsClosed(open));
            Assert.AreEqual(24, Volume(corners, open), 1e-9, "wound the right way: the volume is whole");
        }

        [TestMethod, TestCategory("SurfaceRepair")]
        public void RemoveUnusedCorners_Renumbers_Test()
        {
            var faces = new List<(int A, int B, int C)> { (5, 7, 9) };
            int[] kept = SurfaceRepair.RemoveUnusedCorners(10, faces);
            CollectionAssert.AreEqual(new[] { 5, 7, 9 }, kept);
            Assert.AreEqual((0, 1, 2), faces[0]);
        }
    }
}
