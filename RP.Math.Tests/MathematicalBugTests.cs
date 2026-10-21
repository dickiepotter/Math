namespace RP.Math.Tests
{
    using System;
    using System.Linq;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>
    /// Tests that pin down genuine MATHEMATICAL bugs in the production code.
    ///
    /// Every assertion here states a fact that is provably true by elementary mathematics,
    /// independent of the implementation. Each one localises a real defect, not a quirk of the test;
    /// they are deliberately NOT written to rubber-stamp the existing behaviour.
    ///
    /// Status: Bugs 1, 2, 3, 5 and 6 are now FIXED — those tests pass and act as regression guards.
    /// Bug 4 remains OPEN by choice and is tracked in the README's "Future considerations"; its test is
    /// still EXPECTED TO FAIL. Bug 7 pins a newly-documented defect (unary minus).
    /// (The former "Bug 8" was a convention feature, not a defect — the convention-aware Roll honouring
    /// each frame's own Forward — and now lives as a positive test in ConventionRotationTests.)
    /// </summary>
    [TestClass]
    public class MathematicalBugTests
    {
        // ------------------------------------------------------------------
        // BUG 1 (FIXED) — Vector.Angle used to yield NaN for anti-parallel vectors.
        //
        // The old code did  Math.Acos(Math.Min(1.0f, dot)).  The dot of two independently-normalised
        // vectors is mathematically in [-1, 1] but rounding routinely pushes it just outside; e.g.
        // normalize(v) . normalize(v) can be 1.0000000000000002, so for v and -v the dot is
        // -1.0000000000000002 and Acos(<-1) = NaN.  Only the upper bound was clamped, not the lower.
        //
        // Fixed by computing the angle as atan2(|v1 x v2|, v1 . v2), which is well-conditioned across
        // the whole 0..pi range and returns exactly pi for a vector and its negation — no NaN, and none
        // of the ~1e-8 precision loss acos suffers near +/-1 even once clamped.  This test now passes
        // and acts as a regression guard.  The angle between any non-zero vector and its negation is
        // exactly pi.
        // ------------------------------------------------------------------
        [TestMethod, TestCategory("Bug")]
        public void Angle_BetweenAVectorAndItsNegation_IsAlwaysPi_NeverNaN_Test()
        {
            // A sweep of plain integer vectors; the angle to the exact negation is pi
            // for every one of them.  The buggy clamp turns some of these into NaN.
            for (int x = 1; x <= 5; x++)
            for (int y = 1; y <= 5; y++)
            for (int z = 1; z <= 5; z++)
            {
                var v = new Vector(x, y, z);
                var opposite = new Vector(-x, -y, -z);

                double angle = Vector.Angle(v, opposite);

                Assert.IsFalse(double.IsNaN(angle), $"the angle between {v} and its negation must be pi, not NaN");
                Assert.AreEqual(Math.PI, angle, 1e-9);
            }
        }

        // ------------------------------------------------------------------
        // BUG 2 (fixed) — Matrix * Matrix multiplied in the WRONG ORDER.
        //
        // Matrix.cs:336-371 builds result[r,c] = sum_k m1[k,c]*m2[r,k], which equals
        // (m2 . m1)[r,c] — the reverse of the intended (m1 . m2).  Meanwhile
        // Matrix * Vector (316-331) is the standard M.v.  So composing transforms via
        // the * operator and applying them to a vector silently applies them in the
        // wrong order.
        //
        // Compose translate-by-10-in-x (T) with uniform scale-by-2 (S).  As column-vector
        // transforms, (T*S) means "scale then translate":
        //     (T*S) * (1,0,0) = T * (S * (1,0,0)) = T * (2,0,0) = (12,0,0).
        // The reversed product gives T applied first then S = (22,0,0).
        // ------------------------------------------------------------------
        [TestMethod, TestCategory("Bug")]
        public void MatrixMultiply_ComposesTransformsInCorrectOrder_Test()
        {
            var t = Matrix.TranslationMatrix(10, 0, 0);
            var s = Matrix.ScalingMatrix(2, 2, 2);

            var ts = t * s;

            // The translation column must survive composition unchanged (scale has no
            // translation part): (T*S)[0,3] == 10.  Reversed product gives 20.
            Assert.AreEqual(10, ts[0, 3]);

            var mapped = ts * new Vector(1, 0, 0);
            Assert.AreEqual(12, mapped.X);
            Assert.AreEqual(0, mapped.Y);
            Assert.AreEqual(0, mapped.Z);
        }

        // ------------------------------------------------------------------
        // BUG 3 (fixed) — Matrix.ScalingMatrix(double[]) returned a TRANSLATION matrix.
        //
        // Matrix.cs:808-812 delegates to TranslationMatrix(xyz[0], xyz[1], xyz[2]) — a
        // copy-paste error.  The scalar overload ScalingMatrix(x,y,z) is correct; only
        // the array overload is wrong.
        //
        // A scale matrix for (2,3,4) maps (1,1,1) to (2,3,4); a translation maps it to
        // (3,4,5).
        // ------------------------------------------------------------------
        [TestMethod, TestCategory("Bug")]
        public void ScalingMatrixFromArray_Scales_NotTranslates_Test()
        {
            var scale = Matrix.ScalingMatrix(new[] { 2.0, 3.0, 4.0 });

            // Diagonal carries the scale factors; translation column is zero.
            Assert.AreEqual(2, scale[0, 0]);
            Assert.AreEqual(3, scale[1, 1]);
            Assert.AreEqual(4, scale[2, 2]);
            Assert.AreEqual(0, scale[0, 3]);

            var scaled = scale * new Vector(1, 1, 1);
            Assert.AreEqual(new Vector(2, 3, 4), scaled);
        }

        // ------------------------------------------------------------------
        // BUG 4 (fixed) — Angle.ToAngleValue reduced to the wrong range and sign.
        //
        // It used  rad > 2*pi ? IEEERemainder(rad, 2*pi) : rad.  IEEERemainder returns a
        // value in [-pi, pi], NOT [0, 2*pi), so for 540 degrees (= 3*pi) it returned
        // IEEERemainder(3*pi, 2*pi) = -pi = -180 degrees, contradicting the method's own
        // docstring ("3*PI (540 degree) ==> PI (180 degree)").
        //
        // Now uses the ordinary remainder  rad % 2*pi, which keeps the input's sign and
        // maps 540 -> 180.  This test guards against regression.
        // ------------------------------------------------------------------
        [TestMethod, TestCategory("Bug")]
        public void Angle_ReduceFiveFortyDegrees_GivesOneEighty_PerOwnDocstring_Test()
        {
            var a = new Angle(540, AngleUnits.DEG);

            // 540 deg and 180 deg are the same direction; the canonical value is +180.
            Assert.AreEqual(180, a.Deg, 1e-9);
            Assert.AreEqual(Math.PI, a.Rad, 1e-9);
        }

        // ------------------------------------------------------------------
        // BUG 5 (fixed) — PolynomialRoots.SolveQuadratic lost precision to catastrophic
        // cancellation for the root near zero.
        //
        // PolynomialRoots.cs:42-43 uses the naive (-b +/- sqrt(disc)) / (2a).  When b is
        // large relative to sqrt(disc), (-b + sqrt(disc)) subtracts two nearly-equal
        // numbers and the small root loses most of its significant digits.
        //
        // For x^2 - 1e8 x + 1 = 0 the product of the roots is c/a = 1, so the roots are
        // exactly 1e8-ish and its reciprocal ~ 1e-8.  The naive formula returns about
        // 7.45e-9 for the small root — ~25% relative error.  The stable form
        // (q = -(b + sign(b)sqrt(disc))/2; roots q/a and c/q) is accurate.
        // ------------------------------------------------------------------
        [TestMethod, TestCategory("Bug")]
        public void SolveQuadratic_SmallRoot_IsAccurate_NoCatastrophicCancellation_Test()
        {
            double[] roots = PolynomialRoots.SolveQuadratic(1, -1e8, 1);

            Assert.AreEqual(2, roots.Length);

            double small = roots.Min();
            double large = roots.Max();

            // Product of the roots equals c/a = 1 (Vieta).  This is the cleanest invariant.
            Assert.AreEqual(1.0, (small * large), 1e-9);

            // The small root is 1e-8 to far better than the ~25% error the naive form gives.
            Assert.AreEqual(1e-8, small, 1e-11);
        }

        // ------------------------------------------------------------------
        // BUG 6 (fixed) — OrthogonalAxes.ToString() used to throw NullReferenceException.
        //
        // ToString() calls ToString(null, null), which guarded with
        //   if (format != null || format != "")   — a tautology (no string is simultaneously
        // non-null AND equal to ""), so the branch was always entered and format[0] dereferenced
        // the null format. The guard is now string.IsNullOrEmpty(format); this test guards against
        // regression.
        // ------------------------------------------------------------------
        [TestMethod, TestCategory("Bug")]
        public void OrthogonalAxesToString_DefaultFormat_DoesNotThrow_Test()
        {
            var axes = new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.UP, AxisAlignment.FAR);

            Action act = () => axes.ToString();

            act();
        }

        // ------------------------------------------------------------------
        // BUG 7 (fixed) — Angle's unary minus did not numerically negate.
        //
        // It used to define  -a  as  a.IsClockwise() ? new Angle(-(2*pi - a.Rad)) : a, i.e. it
        // returned the "counter-clockwise companion" rather than the arithmetic negative. For +90 deg
        // (pi/2) it yielded -270 deg (-3*pi/2) instead of -90 deg, so a + (-a) was not zero.
        //
        // Unary minus now negates the raw radians (new Angle(-a.Rad)); the companion behaviour moved to
        // the named CounterClockwise()/Clockwise() methods. This test guards against regression.
        // ------------------------------------------------------------------
        [TestMethod, TestCategory("Bug")]
        public void Angle_UnaryMinus_NumericallyNegates_Test()
        {
            var a = new Angle(Math.PI / 2); // +90 degrees

            var negated = -a;

            // The negative of +90 deg is -90 deg, and the two must cancel.
            Assert.AreEqual(-Math.PI / 2, negated.Rad, 1e-9);
            Assert.AreEqual(0, (a + negated).Rad, 1e-9);
        }
    }
}
