namespace RP.Math
{
    using System;

    /// <summary>
    /// Numerical integrators for the canonical motion problem: given a position, a velocity and an
    /// acceleration, advance them by a time step <c>dt</c>. These are the heart of a physics engine — the
    /// rule that turns "this much force is being applied" into "the ship is now <i>here</i>, moving
    /// <i>this</i> fast".
    /// </summary>
    /// <remarks>
    /// <para><b>Why several methods.</b> They trade accuracy for cost and stability:</para>
    /// <list type="bullet">
    ///   <item><description><b>Explicit (forward) Euler</b> — simplest: step position with the <i>old</i>
    ///   velocity, then update velocity. Cheap but it slowly injects energy, so orbits spiral outward.</description></item>
    ///   <item><description><b>Semi-implicit (symplectic) Euler</b> — update velocity <i>first</i>, then
    ///   step position with the <i>new</i> velocity. Almost free, far more stable, and energy-conserving
    ///   enough for games — this is the right default for spacecraft (build brief S6).</description></item>
    ///   <item><description><b>Runge–Kutta 4 (RK4)</b> — samples the acceleration four times across the
    ///   step and blends them; much more accurate per step, at four times the cost. Use where precision
    ///   matters and the acceleration varies within a step.</description></item>
    /// </list>
    /// <para>All methods are pure: they take the state in and return the new state, never mutating —
    /// matching the rest of RP.Math.</para>
    /// </remarks>
    public static class Integrators
    {
        /// <summary>
        /// Explicit (forward) Euler: <c>x' = x + v·dt</c>, then <c>v' = v + a·dt</c>. Position is stepped
        /// with the <i>old</i> velocity. Exact only for zero acceleration; tends to gain energy otherwise.
        /// </summary>
        public static (Vector Position, Vector Velocity) ExplicitEuler(
            Vector position, Vector velocity, Vector acceleration, double dt)
        {
            Vector newPosition = position + velocity * dt;
            Vector newVelocity = velocity + acceleration * dt;
            return (newPosition, newVelocity);
        }

        /// <summary>
        /// Semi-implicit (symplectic) Euler: <c>v' = v + a·dt</c>, then <c>x' = x + v'·dt</c>. Velocity is
        /// updated first and position uses the <i>new</i> velocity. The stable, energy-friendly default for
        /// Newtonian flight.
        /// </summary>
        public static (Vector Position, Vector Velocity) SemiImplicitEuler(
            Vector position, Vector velocity, Vector acceleration, double dt)
        {
            Vector newVelocity = velocity + acceleration * dt;
            Vector newPosition = position + newVelocity * dt;
            return (newPosition, newVelocity);
        }

        /// <summary>
        /// Classic fourth-order Runge–Kutta for the second-order system <c>x'' = a(x, v, t)</c>. The
        /// acceleration is supplied as a function of state and time, sampled four times across the step.
        /// </summary>
        /// <param name="acceleration">Returns acceleration given (position, velocity, time).</param>
        /// <param name="time">The absolute time at the start of the step (passed to the acceleration fn).</param>
        public static (Vector Position, Vector Velocity) RungeKutta4(
            Vector position, Vector velocity,
            Func<Vector, Vector, double, Vector> acceleration,
            double dt, double time = 0.0)
        {
            if (acceleration is null) throw new ArgumentNullException(nameof(acceleration));

            // Each "k" is a (velocity, acceleration) derivative sample of the (position, velocity) state.
            Vector k1v = velocity;
            Vector k1a = acceleration(position, velocity, time);

            Vector k2v = velocity + k1a * (dt * 0.5);
            Vector k2a = acceleration(position + k1v * (dt * 0.5), k2v, time + dt * 0.5);

            Vector k3v = velocity + k2a * (dt * 0.5);
            Vector k3a = acceleration(position + k2v * (dt * 0.5), k3v, time + dt * 0.5);

            Vector k4v = velocity + k3a * dt;
            Vector k4a = acceleration(position + k3v * dt, k4v, time + dt);

            Vector newPosition = position + (k1v + 2.0 * k2v + 2.0 * k3v + k4v) * (dt / 6.0);
            Vector newVelocity = velocity + (k1a + 2.0 * k2a + 2.0 * k3a + k4a) * (dt / 6.0);
            return (newPosition, newVelocity);
        }
    }
}
