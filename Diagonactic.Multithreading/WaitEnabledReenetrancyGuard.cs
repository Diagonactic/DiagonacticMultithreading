using System;
using System.Threading;

namespace Diagonactic.Multithreading
{
    /// <summary>Similar to <see cref="ReentrancyGuard" /> but signals a <see cref="ManualResetEvent" /> when the guard is lifted and resets it when it's started.</summary>
    internal class WaitEnabledReenetrancyGuard : ReentrancyGuard, IDisposable
    {
        private const int Allocated = 0, Disposing = 1, Disposed = 2;

        private readonly ManualResetEvent m_waitForCompletion = new ManualResetEvent(false);
        private int m_state;
        protected bool IsAllocated => Interlocked.CompareExchange(ref m_state, Allocated, Allocated) == Allocated;

        /// <summary>Disposes of managed/non-managed resources</summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref m_state, Disposing, Allocated) != Allocated)
                return;

            m_waitForCompletion.Dispose();

            GC.SuppressFinalize(this);
            Interlocked.Exchange(ref m_state, Disposed);
        }

        /// <summary>Waits for the signal that a thread has completed running the non-reentrant code.</summary>
        /// <remarks>
        ///     <para>
        ///         This method does not guarantee that the non-reentrant method will be ready to execute since another thread could enter the protected code immediately after the wait is completed but before the code on the thread waiting is able
        ///         to enter.
        ///     </para>
        ///     <para>It must be combined with other methods to ensure that the waiting thread will be able to call the non-reentrant code if the non-reentrant call is critical to force a run.</para>
        /// </remarks>
        public void WaitForCompletion()
        {
            m_waitForCompletion.WaitOne();
        }

        /// <summary>Waits for the signal that a thread has completed running the non-reentrant code.</summary>
        /// <remarks>
        ///     <para>
        ///         This method does not guarantee that the non-reentrant method will be ready to execute since another thread could enter the protected code immediately after the wait is completed but before the code on the thread waiting is able
        ///         to enter.
        ///     </para>
        ///     <para>It must be combined with other methods to ensure that the waiting thread will be able to call the non-reentrant code if the non-reentrant call is critical to force a run.</para>
        /// </remarks>
        /// <param name="timeout">The length of time to wait before giving up.</param>
        /// <returns>If the wait was signalled before the timeout, returns <see langword="true" />; otherwise <see langword="false" /></returns>
        public bool WaitForCompletion(TimeSpan timeout) => m_waitForCompletion.WaitOne(timeout);

        /// <summary>Waits for the signal that a thread has completed running the non-reentrant code.</summary>
        /// <remarks>
        ///     <para>
        ///         This method does not guarantee that the non-reentrant method will be ready to execute since another thread could enter the protected code immediately after the wait is completed but before the code on the thread waiting is able
        ///         to enter.
        ///     </para>
        ///     <para>It must be combined with other methods to ensure that the waiting thread will be able to call the non-reentrant code if the non-reentrant call is critical to force a run.</para>
        /// </remarks>
        /// <param name="timeout">The length of time to wait before giving up.</param>
        /// <returns>If the wait was signalled before the timeout, returns <see langword="true" />; otherwise <see langword="false" /></returns>
        public bool WaitForCompletion(int timeout) => m_waitForCompletion.WaitOne(timeout);

        public static ReentrancyCallResult CallReentrancySafe(WaitEnabledReenetrancyGuard guard, Func<bool> action) => guard.CallReentrancySafe(action);

        /// <summary>Calls the <paramref name="action" />, preventing reentrancy if the reentrancy guard indicates that this method is already running and signaling its completion</summary>
        /// <remarks>If the method is called but reentrancy is prevented, the code is not queued, it is simply not executed and the method returns <see langword="false" /></remarks>
        /// <param name="guard">The reentrancy guard to use to prevent reentrancy - normally a unique guard should be used for a unique call</param>
        /// <param name="action">Code that needs to be protected from reentrancy</param>
        /// <returns>Returns <see langword="true" /> if the code is executed; otherwise <see langword="false" /></returns>
        public static bool CallReentrancySafe(WaitEnabledReenetrancyGuard guard, Action action) => guard.CallReentrancySafe(action);

        public override void AllowReentrancy()
        {
            try
            {
                base.AllowReentrancy();
            }
            finally
            {
                m_waitForCompletion.Set();
            }
        }

        public override bool SetGuardAndCheckEntry()
        {
            m_waitForCompletion.Reset();
            return base.SetGuardAndCheckEntry();
        }
    }
}