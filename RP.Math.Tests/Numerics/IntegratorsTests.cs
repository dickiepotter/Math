namespace RP.Math.Tests.Numerics
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RP.Math;

    /// <summary>
    /// Integrator correctness against closed-form cases (build brief S20): constant thrust → expected
    /// position, no force → conserved momentum, RK4 exact for constant acceleration, and convergence as
    /// the step shrinks.
    /// </summary>
    [TestClass]
    public sealed class IntegratorsTests
    {
        [TestMethod]
        public void ExplicitEuler_ZeroAcceleration_MovesByVelocityTimesDt()
        {
            var (pos, vel) = Integrators.ExplicitEuler(
                new Vector(0, 0, 0), new Vector(2, 0, 0), Vector.Zero, 0.5);

            Assert.AreEqual(new Vector(1, 0, 0), pos); // x += v*dt
            Assert.AreEqual(new Vector(2, 0, 0), vel); // unchanged
        }

        [TestMethod]
        public void SemiImplicitEuler_OneStep_UsesTheUpdatedVelocity()
        {
            // v' = v + a*dt; x' = x + v'*dt.
            var (pos, vel) = Integrators.SemiImplicitEuler(
                Vector.Zero, new Vector(1, 0, 0), new Vector(2, 0, 0), 1.0);

            Assert.AreEqual(new Vector(3, 0, 0), vel);   // 1 + 2*1
            Assert.AreEqual(new Vector(3, 0, 0), pos);   // 0 + 3*1
        }

        [TestMethod]
        public void AllMethods_ZeroForce_ConserveMomentum()
        {
            var v = new Vector(3, -2, 1);
            Assert.AreEqual(v, Integrators.ExplicitEuler(Vector.Zero, v, Vector.Zero, 0.123).Velocity);
            Assert.AreEqual(v, Integrators.SemiImplicitEuler(Vector.Zero, v, Vector.Zero, 0.123).Velocity);
            Assert.AreEqual(v, Integrators.RungeKutta4(Vector.Zero, v, (p, vv, t) => Vector.Zero, 0.123).Velocity);
        }

        [TestMethod]
        public void RungeKutta4_ConstantAcceleration_MatchesAnalyticExactly()
        {
            // Analytic: x = x0 + v0*t + 0.5*a*t^2 ; v = v0 + a*t. RK4 is exact for this polynomial.
            var x0 = new Vector(1, 2, 3);
            var v0 = new Vector(0, 1, 0);
            var a = new Vector(0, -9.81, 0);
            const double dt = 2.0;

            var (pos, vel) = Integrators.RungeKutta4(x0, v0, (p, v, t) => a, dt);

            var expectedPos = x0 + v0 * dt + a * (0.5 * dt * dt);
            var expectedVel = v0 + a * dt;
            Assert.IsTrue((pos.Distance(expectedPos)) < (1e-9));
            Assert.IsTrue((vel.Distance(expectedVel)) < (1e-9));
        }

        [TestMethod]
        public void RungeKutta4_ConstantAcceleration_ManySmallStepsAlsoMatchAnalytic()
        {
            var x0 = new Vector(0, 0, 0);
            var v0 = new Vector(5, 0, 0);
            var a = new Vector(1, 0, 0);
            const double total = 3.0;
            const int steps = 30;
            double dt = total / steps;

            Vector pos = x0, vel = v0;
            for (int i = 0; i < steps; i++)
            {
                (pos, vel) = Integrators.RungeKutta4(pos, vel, (p, v, t) => a, dt);
            }

            var expectedPos = x0 + v0 * total + a * (0.5 * total * total);
            Assert.IsTrue((pos.Distance(expectedPos)) < (1e-6));
        }

        [TestMethod]
        public void SemiImplicitEuler_ConvergesToAnalytic_AsStepShrinks()
        {
            // Constant acceleration; semi-implicit Euler has O(dt) error per the dt^2 term, so a smaller
            // step must land closer to the analytic answer.
            var a = new Vector(0, -10, 0);
            const double total = 1.0;

            double ErrorFor(int steps)
            {
                double dt = total / steps;
                Vector pos = Vector.Zero, vel = Vector.Zero;
                for (int i = 0; i < steps; i++)
                {
                    (pos, vel) = Integrators.SemiImplicitEuler(pos, vel, a, dt);
                }

                Vector analytic = a * (0.5 * total * total);
                return pos.Distance(analytic);
            }

            double coarse = ErrorFor(10);
            double fine = ErrorFor(1000);
            Assert.IsTrue((fine) < (coarse));
        }

        [TestMethod]
        public void SemiImplicitEuler_HarmonicOscillator_StaysBounded()
        {
            // a = -k x (a spring). The symplectic integrator must not let the amplitude blow up — the
            // classic reason it beats explicit Euler for orbital/spacecraft motion.
            const double k = 4.0;
            Vector pos = new Vector(1, 0, 0), vel = Vector.Zero;
            const double dt = 0.01;

            double maxDisplacement = 0;
            for (int i = 0; i < 10000; i++)
            {
                Vector a = pos * -k;
                (pos, vel) = Integrators.SemiImplicitEuler(pos, vel, a, dt);
                maxDisplacement = Math.Max(maxDisplacement, pos.Magnitude);
            }

            // Started at amplitude 1; should stay close to it, never diverge.
            Assert.IsTrue((maxDisplacement) < (1.1));
        }
    }
}
