using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RP.Math.Tests
{
    /// <summary>
    /// Covers the merged <see cref="OrthogonalAxes"/> convention type: derived basis vectors, derived
    /// handedness, validation, the predefined system conventions, and formatting.
    /// </summary>
    [TestClass]
    public class OrthogonalAxesTests
    {
        // ---- Constructor validation -------------------------------------------------

        [TestMethod]
        public void Constructor_RejectsTwoAxesFromTheSamePair()
        {
            System.Action act = () => new OrthogonalAxes(AxisAlignment.UP, AxisAlignment.DOWN, AxisAlignment.RIGHT);
            Assert.ThrowsException<System.ArgumentException>(act);
        }

        [TestMethod]
        public void Constructor_RejectsRepeatedRole()
        {
            System.Action act = () => new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.RIGHT, AxisAlignment.UP);
            Assert.ThrowsException<System.ArgumentException>(act);
        }

        [TestMethod]
        public void Constructor_AcceptsAGenuineConvention()
        {
            System.Action act = () => new OrthogonalAxes(AxisAlignment.FAR, AxisAlignment.RIGHT, AxisAlignment.UP);
            act();
        }

        // ---- Derived basis vectors --------------------------------------------------

        [TestMethod]
        public void Right_Up_Forward_FollowTheLabelsAndSigns()
        {
            // Unreal: +x forward (far), +y right, +z up.
            var a = new OrthogonalAxes(AxisAlignment.FAR, AxisAlignment.RIGHT, AxisAlignment.UP);

            Assert.AreEqual(new Vector(1, 0, 0), a.Forward);
            Assert.AreEqual(new Vector(0, 1, 0), a.Right);
            Assert.AreEqual(new Vector(0, 0, 1), a.Up);
        }

        [TestMethod]
        public void NegativeLabels_FlipTheBasisVector()
        {
            // z carries NEAR, so the forward/far direction is -z.
            var a = new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.UP, AxisAlignment.NEAR);

            Assert.AreEqual(new Vector(0, 0, -1), a.Forward);
        }

        // ---- Derived handedness -----------------------------------------------------

        [TestMethod]
        public void Handedness_IsRightHandedForOpenGlStyleFrames()
        {
            Assert.AreEqual(Handedness.Right, OrthogonalAxes.OpenGL.Handedness);
            Assert.AreEqual(Handedness.Right, OrthogonalAxes.Blender.Handedness);
        }

        [TestMethod]
        public void Handedness_IsLeftHandedForDirectXStyleFrames()
        {
            Assert.AreEqual(Handedness.Left, OrthogonalAxes.DirectX.Handedness);
            Assert.AreEqual(Handedness.Left, OrthogonalAxes.Unreal.Handedness);
        }

        // ---- Predefined conventions -------------------------------------------------

        [TestMethod]
        public void OpenGlAndDirectX_DifferOnlyInTheDepthAxis()
        {
            // The whole right- vs left-handed distinction is exactly NEAR vs FAR on z.
            Assert.AreEqual(AxisAlignment.NEAR, OrthogonalAxes.OpenGL.Z);
            Assert.AreEqual(AxisAlignment.FAR, OrthogonalAxes.DirectX.Z);
            Assert.AreEqual(OrthogonalAxes.DirectX.X, OrthogonalAxes.OpenGL.X);
            Assert.AreEqual(OrthogonalAxes.DirectX.Y, OrthogonalAxes.OpenGL.Y);
        }

        [TestMethod]
        public void AliasedConventions_AreEqual()
        {
            Assert.AreEqual(OrthogonalAxes.OpenGL, OrthogonalAxes.Maya);
            Assert.AreEqual(OrthogonalAxes.OpenGL, OrthogonalAxes.Godot);
            Assert.AreEqual(OrthogonalAxes.OpenGL, OrthogonalAxes.MathsYUp);
            Assert.AreEqual(OrthogonalAxes.DirectX, OrthogonalAxes.Unity);
            Assert.AreEqual(OrthogonalAxes.Blender, OrthogonalAxes.Max3ds);
            Assert.AreEqual(OrthogonalAxes.Blender, OrthogonalAxes.MathsZUp);
        }

        // ---- Formatting (was BUG 6) -------------------------------------------------

        [TestMethod]
        public void ToString_DefaultAndVerbose_DoNotThrow()
        {
            var a = OrthogonalAxes.OpenGL;

            Assert.AreEqual("(RIGHT, UP, NEAR)", a.ToString());
            StringAssert.Contains(a.ToString("v", null), "Right-handed");
        }
    }
}
