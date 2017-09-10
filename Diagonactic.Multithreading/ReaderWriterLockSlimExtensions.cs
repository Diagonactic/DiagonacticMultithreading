using System;
using System.Threading;

namespace Diagonactic.Multithreading
{
    
    internal class DelegateDisposable : ThreadSafeDisposableBase
    {
        private Action m_onDispose;

        internal DelegateDisposable(Action onDispose)
        {
            m_onDispose = onDispose;
        }

        protected override void DisposeManagedResources()
        {
            m_onDispose();
        }
    }

    public static class ReaderWriterLockSlimExtensions
    {
        public static IDisposable ReadUsing(this ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterReadLock();
            return new DelegateDisposable(rwLock.ExitReadLock);
        }

        public static IDisposable WriteUsing(this ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterWriteLock();
            return new DelegateDisposable(rwLock.ExitWriteLock);
        }

        public static IDisposable UpgradableReadUsing(this ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterUpgradeableReadLock();
            return new DelegateDisposable(rwLock.ExitUpgradeableReadLock);
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Read Lock from <paramref name="rwLock" />.</summary>
        /// <typeparam name="T">The result of <paramref name="onLockAcquired" /></typeparam>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <returns>The result of <paramref name="onLockAcquired" /></returns>
        public static T ExecuteRead<T>(this ReaderWriterLockSlim rwLock, Func<T> onLockAcquired)
        {
            rwLock.EnterReadLock();
            try
            {
                return onLockAcquired();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Read Lock from <paramref name="rwLock" />.</summary>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        public static void ExecuteRead(this ReaderWriterLockSlim rwLock, Action onLockAcquired)
        {
            rwLock.EnterReadLock();
            try
            {
                onLockAcquired();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Read Lock from <paramref name="rwLock" />, provided <paramref name="timeOut" /> is not reached waiting for lock.</summary>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <param name="timeOut">Length of time to wait to acqurie the lock</param>
        /// <returns>If the lock is acquired and <paramref name="onLockAcquired" /> is executed, returns <see langword="true" />; otherwise <see langword="false" /></returns>
        public static bool ExecuteRead(this ReaderWriterLockSlim rwLock, Action onLockAcquired, TimeSpan timeOut)
        {
            if (!rwLock.TryEnterReadLock(timeOut))
                return false;

            try
            {
                onLockAcquired();
                return true;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Read Lock from <paramref name="rwLock" />, provided <paramref name="timeOut" /> is not reached waiting for lock.</summary>
        /// <typeparam name="T">The result of <paramref name="onLockAcquired" /></typeparam>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <param name="timeOut">Length of time to wait to acqurie the lock</param>
        /// <returns>An instance of <see cref="LockResult{T}" /> with the result of <paramref name="onLockAcquired" /> if it is executed and <see cref="LockResult{T}.WasTimedOut" /> indicating whether the operation timed out waiting for the lock</returns>
        public static LockResult<T> ExecuteRead<T>(this ReaderWriterLockSlim rwLock, Func<T> onLockAcquired, TimeSpan timeOut)
        {
            if (!rwLock.TryEnterReadLock(timeOut))
                return new LockResult<T>(LockResultOutcome.TimedOut);

            try
            {
                return new LockResult<T>(onLockAcquired());
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }


        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Read Lock from <paramref name="rwLock" />, provided <paramref name="timeOut" /> is not reached waiting for lock.</summary>
        /// <typeparam name="T">The result of <paramref name="onLockAcquired" /></typeparam>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <param name="timeOut">Length of time to wait to acqurie the lock</param>
        /// <returns>An instance of <see cref="LockResult{T}" /> with the result of <paramref name="onLockAcquired" /> if it is executed and <see cref="LockResult{T}.WasTimedOut" /> indicating whether the operation timed out waiting for the lock</returns>
        public static LockResult<T> ExecuteUpgradableRead<T>(this ReaderWriterLockSlim rwLock, Func<T> onLockAcquired, TimeSpan timeOut)
        {
            if (!rwLock.TryEnterUpgradeableReadLock(timeOut))
                return new LockResult<T>(LockResultOutcome.TimedOut);

            try
            {
                return new LockResult<T>(onLockAcquired());
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Read Lock from <paramref name="rwLock" />.</summary>
        /// <typeparam name="T">The result of <paramref name="onLockAcquired" /></typeparam>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <returns>The result of <paramref name="onLockAcquired" /></returns>
        public static T ExecuteUpgradableRead<T>(this ReaderWriterLockSlim rwLock, Func<T> onLockAcquired)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                return onLockAcquired();
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Read Lock from <paramref name="rwLock" />.</summary>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        public static void ExecuteUpgradableRead(this ReaderWriterLockSlim rwLock, Action onLockAcquired)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                onLockAcquired();
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Read Lock from <paramref name="rwLock" />, provided <paramref name="timeOut" /> is not reached waiting for lock.</summary>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <param name="timeOut">Length of time to wait to acqurie the lock</param>
        /// <returns>If the lock is acquired and <paramref name="onLockAcquired" /> is executed, returns <see langword="true" />; otherwise <see langword="false" /></returns>
        public static bool ExecuteUpgradableRead(this ReaderWriterLockSlim rwLock, Action onLockAcquired, TimeSpan timeOut)
        {
            if (!rwLock.TryEnterUpgradeableReadLock(timeOut))
                return false;

            try
            {
                onLockAcquired();
                return true;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Write Lock from <paramref name="rwLock" />.</summary>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        public static void ExecuteWrite(this ReaderWriterLockSlim rwLock, Action onLockAcquired)
        {
            rwLock.EnterWriteLock();
            try
            {
                onLockAcquired();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Write Lock from <paramref name="rwLock" />, provided <paramref name="timeOut" /> is not reached waiting for lock.</summary>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <param name="timeOut">Length of time to wait to acqurie the lock</param>
        /// <returns>If the lock is acquired and <paramref name="onLockAcquired" /> is executed, returns <see langword="true" />; otherwise <see langword="false" /></returns>
        public static bool ExecuteWrite(this ReaderWriterLockSlim rwLock, Action onLockAcquired, TimeSpan timeOut)
        {
            if (!rwLock.TryEnterWriteLock(timeOut))
                return false;

            try
            {
                onLockAcquired();
                return true;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Write Lock from <paramref name="rwLock" />.</summary>
        /// <typeparam name="T">The result of <paramref name="onLockAcquired" /></typeparam>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <returns>The result of <paramref name="onLockAcquired" /></returns>
        public static T ExecuteWrite<T>(this ReaderWriterLockSlim rwLock, Func<T> onLockAcquired)
        {
            rwLock.EnterWriteLock();
            try
            {
                return onLockAcquired();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>Executes <paramref name="onLockAcquired" /> after acquiring a Write Lock from <paramref name="rwLock" />, provided <paramref name="timeOut" /> is not reached waiting for lock.</summary>
        /// <typeparam name="T">The result of <paramref name="onLockAcquired" /></typeparam>
        /// <param name="rwLock">The lock to acquire.</param>
        /// <param name="onLockAcquired">Delegate to execute inside of the lock boundaries.</param>
        /// <param name="timeOut">Length of time to wait to acqurie the lock</param>
        /// <returns>An instance of <see cref="LockResult{T}" /> with the result of <paramref name="onLockAcquired" /> if it is executed and <see cref="LockResult{T}.WasTimedOut" /> indicating whether the operation timed out waiting for the lock</returns>
        public static LockResult<T> ExecuteWrite<T>(this ReaderWriterLockSlim rwLock, Func<T> onLockAcquired, TimeSpan timeOut)
        {
            if (!rwLock.TryEnterWriteLock(timeOut))
                return new LockResult<T>(LockResultOutcome.TimedOut);

            try
            {
                return new LockResult<T>(onLockAcquired());
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     Acquires an upgradable read lock and executes <paramref name="onUpgradableReadAcquired" />. If <paramref name="onUpgradableReadAcquired" /> returns <see langword="true" />, acquires write lock and executes
        ///     <paramref name="onWriteAcquired" />
        /// </summary>
        /// <typeparam name="T">The result of <paramref name="onWriteAcquired" /></typeparam>
        /// <param name="rwLock">The lock object to use to acquire the lock.</param>
        /// <param name="onUpgradableReadAcquired">A delegate to determine if a write lock should be acquired and <paramref name="onWriteAcquired" /> should be executed.</param>
        /// <param name="onWriteAcquired">The delegate to execute if <paramref name="onUpgradableReadAcquired" /> returns <see langword="true" />.</param>
        /// <returns>
        /// A <see cref="LockResult{T}"/> indicating the result of the conditional operation, with the <see cref="LockResult{T}.Result"/> set to the value of <paramref name="onWriteAcquired"/>
        /// if the <paramref name="onUpgradableReadAcquired"/> returned <see langword="true"/>
        /// </returns>
        public static LockResult<T> ExecuteReadWithConditionalWrite<T>(this ReaderWriterLockSlim rwLock, Func<bool> onUpgradableReadAcquired, Func<T> onWriteAcquired)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                if (onUpgradableReadAcquired())
                {
                    rwLock.EnterWriteLock();
                    try
                    {
                        return new LockResult<T>(onWriteAcquired());
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
                else
                {
                    return new LockResult<T>(LockResultOutcome.ConditionalFailure);
                }
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }
    }


}