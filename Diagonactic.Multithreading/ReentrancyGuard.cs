using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Diagonactic.Multithreading
{
    /// <summary>
    /// Class for handling methods or code blocks that cannot allow re-entrancy
    /// </summary>
    public class ReentrancyGuard
    {
        private const int ReentrancyPrevented = 1, ReentrancyAllowed = 0;

        private int m_reentrancy = 0;

        public enum ReentrancyCallResult
        {
            Unset = 0,
            Success = 1,
            Fail = 2,
            GuardBlocked = 3,
        }

        public bool IsReentrancyPrevented => Interlocked.CompareExchange(ref m_reentrancy, ReentrancyPrevented, ReentrancyPrevented) == ReentrancyPrevented;

        /// <summary>
        /// Used at the opening of a reentrancy prevention block.  Sets guard to prevent reentrancy.
        /// </summary>
        /// <returns>When it's safe to enter the reentrancy prevented block, returns <see langword="true"/>; otherwise <see langword="false"/></returns>
        public bool SetGuardAndCheckEntry()
        {
            return Interlocked.CompareExchange(ref m_reentrancy, ReentrancyPrevented, ReentrancyAllowed) == ReentrancyAllowed;
        }

        /// <summary>
        /// Used after the reentrancy prevention block to reset and allow future callers to enter the method.  This should always be used in a <see langword="finally"/> block to ensure it is set.
        /// </summary>
        /// <remarks>Use <see cref="CallReentrancySafe"/> for a convenient method to implement this pattern.</remarks>
        public void AllowReentrancy()
        {
            Interlocked.Exchange(ref m_reentrancy, ReentrancyAllowed);
        }

        /// <summary>
        /// Executes <paramrf name="action"/> with reentrancy protection from <paramref name="guard"/>
        /// </summary>
        /// <param name="guard">The reentrancy guard to use to prevent reentrancy - normally a unique guard should be used for a unique call</param>
        /// <param name="action">The action to execute (will throw exceptions if the action throws exceptions</param>
        /// <returns>If the guard blocks the call, returns <see cref="ReentrancyCallResult.GuardBlocked"/>; otherwise returns the result of <paramref name="action"/> as <see cref="ReentrancyCallResult.Success"/> or <see cref="ReentrancyCallResult.Fail"/></returns>
        public static ReentrancyCallResult CallReentrancySafe(ReentrancyGuard guard, Func<bool> action)
        {
            if (!guard.SetGuardAndCheckEntry())
                return ReentrancyCallResult.GuardBlocked;

            try
            {
                return action() ? ReentrancyCallResult.Success : ReentrancyCallResult.Fail;
            }
            finally
            {
                guard.AllowReentrancy();
            }
        }

        public static bool CallReentrancySafe(ReentrancyGuard guard, Action action)
        {
            if (!guard.SetGuardAndCheckEntry())
                return false;

            try
            {
                action();
                return true;
            }
            finally
            {
                guard.AllowReentrancy();
            }
        }
    }
}
