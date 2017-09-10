using System;
using System.Threading;

namespace Diagonactic.Multithreading
{
    /// <summary>Class for handling methods or code blocks that cannot allow re-entrancy</summary>
    public class ReentrancyGuard
    {
        public enum ReentrancyCallResult
        {
            Unset = 0,
            Success = 1,
            Fail = 2,
            GuardBlocked = 3,
        }

        private const int ReentrancyPrevented = 1, ReentrancyAllowed = 0;

        private int m_reentrancyState = 0;

        public bool IsReentrancyPrevented => Interlocked.CompareExchange(ref m_reentrancyState, ReentrancyPrevented, ReentrancyPrevented) == ReentrancyPrevented;

        /// <summary>Used at the opening of a reentrancy prevention block.  Sets guard to prevent reentrancy.</summary>
        /// <returns>When it's safe to enter the reentrancy prevented block, returns <see langword="true" />; otherwise <see langword="false" /></returns>
        public virtual bool SetGuardAndCheckEntry() => Interlocked.CompareExchange(ref m_reentrancyState, ReentrancyPrevented, ReentrancyAllowed) == ReentrancyAllowed;

        /// <summary>Used after the reentrancy prevention block to reset and allow future callers to enter the method.  This should always be used in a <see langword="finally" /> block to ensure it is set.</summary>
        /// <remarks>Use <see cref="CallReentrancySafe" /> for a convenient method to implement this pattern.</remarks>
        public virtual void AllowReentrancy()
        {
            Interlocked.Exchange(ref m_reentrancyState, ReentrancyAllowed);
        }

        /// <summary>Calls the <paramref name="action" />, preventing reentrancy if the reentrancy guard indicates that this method is already running</summary>
        /// <remarks>If the method is called but reentrancy is prevented, the code is not queued, it is simply not executed and the method returns <see cref="ReentrancyCallResult.GuardBlocked" /></remarks>
        /// <param name="guard">The reentrancy guard to use to prevent reentrancy - normally a unique guard should be used for a unique call</param>
        /// <param name="action">Code that needs to be protected from reentrancy</param>
        /// <returns>
        ///     Returns <see cref="ReentrancyCallResult.Success" /> if <paramref name="action" /> is called and returns <see langword="true" />, <see cref="ReentrancyCallResult.Fail" /> if it returns <see langword="false" /> and
        ///     <see cref="ReentrancyCallResult.GuardBlocked" /> if the code is not executed because another thread is currently executing the code.
        /// </returns>
        public static ReentrancyCallResult CallReentrancySafe(ReentrancyGuard guard, Func<bool> action) => guard.CallReentrancySafe(action);

        /// <summary>Calls the <paramref name="action" />, preventing reentrancy if the reentrancy guard indicates that this method is already running</summary>
        /// <remarks>If the method is called but reentrancy is prevented, the code is not queued, it is simply not executed and the method returns <see cref="ReentrancyCallResult.GuardBlocked" /></remarks>
        /// <param name="action">Code that needs to be protected from reentrancy</param>
        /// <returns>
        ///     Returns <see cref="ReentrancyCallResult.Success" /> if <paramref name="action" /> is called and returns <see langword="true" />, <see cref="ReentrancyCallResult.Fail" /> if it returns <see langword="false" /> and
        ///     <see cref="ReentrancyCallResult.GuardBlocked" /> if the code is not executed because another thread is currently executing the code.
        /// </returns>
        public virtual ReentrancyCallResult CallReentrancySafe(Func<bool> action)
        {
            if (!SetGuardAndCheckEntry())
                return ReentrancyCallResult.GuardBlocked;

            try
            {
                return action() ? ReentrancyCallResult.Success : ReentrancyCallResult.Fail;
            }
            finally
            {
                AllowReentrancy();
            }
        }

        /// <summary>Calls the <paramref name="action" />, preventing reentrancy if the reentrancy guard indicates that this method is already running</summary>
        /// <remarks>If the method is called but reentrancy is prevented, the code is not queued, it is simply not executed and the method returns <see langword="false" /></remarks>
        /// <param name="action">Code that needs to be protected from reentrancy</param>
        /// <returns>Returns <see langword="true" /> if the code is executed; otherwise <see langword="false"/></returns>
        public virtual bool CallReentrancySafe(Action action) => CallReentrancySafe(() =>
                                                                                    {
                                                                                        action();
                                                                                        return true;
                                                                                    }) ==
                                                                 ReentrancyCallResult.Success;

        /// <summary>Calls the <paramref name="action" />, preventing reentrancy if the reentrancy guard indicates that this method is already running</summary>
        /// <remarks>If the method is called but reentrancy is prevented, the code is not queued, it is simply not executed and the method returns <see langword="false" /></remarks>
        /// <param name="guard">The reentrancy guard to use to prevent reentrancy - normally a unique guard should be used for a unique call</param>
        /// <param name="action">Code that needs to be protected from reentrancy</param>
        /// <returns>Returns <see langword="true" /> if the code is executed; otherwise <see langword="false"/></returns>
        public static bool CallReentrancySafe(ReentrancyGuard guard, Action action) => guard.CallReentrancySafe(action);
    }
}