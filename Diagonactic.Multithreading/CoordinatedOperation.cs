using System;
using System.Threading;

namespace Diagonactic.Multithreading
{
    /// <summary>An operation that waits until a start signal is received and signals when the operation is completed.</summary>
    /// <remarks>Not unit tested, yet, so this is internal only</remarks>
    public class CoordinatedOperation : ThreadSafeDisposableBase
    {
        /// <summary>The result of a coordinated operation</summary>
        public enum OperationResult
        {
            /// <summary>Operation has not executed or waited for execution to start</summary>
            Unset = 0,

            /// <summary>The lambda operation returned true</summary>
            Success = 1,

            /// <summary>The lambda operation returned false</summary>
            Failure = 2,

            /// <summary>The lambda method was not executed because a timeout was reached</summary>
            TimedOut = 3
        }

        private readonly ManualResetEvent m_waitForFinish = new ManualResetEvent(false);
        private readonly ManualResetEvent m_waitForStart = new ManualResetEvent(false);
        private Thread m_operationThread;

        /// <summary>The result of the operation when executed on a separate thread by <see cref="ThreadedExecuteOnStartSignal(System.TimeSpan,System.Func{bool})" />
        /// </summary>
        public OperationResult ThreadedOperationResult { get; private set; }

        /// <summary>Spawns a thread and executes <paramref name="operation" /> once the start signal is rceived and signals its completion.</summary>
        /// <param name="startTimeout">The time to wait for the start signal to be received</param>
        /// <param name="operation">The operation to execute on a separate thread</param>
        /// <returns>The <see cref="CoordinatedOperation" /> that was created to execute <paramref name="operation" /></returns>
        public static CoordinatedOperation ThreadedExecute(TimeSpan startTimeout, Func<bool> operation)
        {
            var coordinatedOperation = new CoordinatedOperation();
            coordinatedOperation.ThreadedExecuteOnStartSignal(startTimeout, operation);
            return coordinatedOperation;
        }

        protected override void DisposeManagedResources()
        {
            m_waitForStart.Dispose();
            m_waitForFinish.Dispose();
            base.DisposeManagedResources();
        }

        /// <summary>Synchronously executes <paramref name="operation" /> once the start signal is received and signals its completion</summary>
        /// <param name="timeout">The time to wait for the start signal to be received</param>
        /// <param name="operation">The code to execute after the start signal is received</param>
        /// <returns>A <see cref="OperationResult" /> indicating the result of the call</returns>
        public OperationResult ExecuteOnStartSignal(TimeSpan timeout, Func<bool> operation)
        {
            if (timeout == default(TimeSpan))
                m_waitForStart.WaitOne();
            else if (!m_waitForStart.WaitOne(timeout))
                return OperationResult.TimedOut;

            try
            {
                return operation() ? OperationResult.Success : OperationResult.Failure;
            }
            finally
            {
                m_waitForFinish.Set();
            }
        }

        /// <summary>Spawns a thread and executes <paramref name="operation" /> once the start signal is rceived and signals its completion.</summary>
        /// <param name="timeout">The time to wait for the start signal to be received</param>
        /// <param name="operation">The operation to execute on a separate thread</param>
        public void ThreadedExecuteOnStartSignal(TimeSpan timeout, Func<bool> operation)
        {
            if (m_operationThread != null)
                throw new InvalidOperationException($"{nameof(CoordinatedOperation)} has already been requested");
            m_operationThread = new Thread(() => { ThreadedOperationResult = ExecuteOnStartSignal(timeout, () => { return operation(); }); })
                                {
                                    Name = "CoordinatedOperation"
                                };
            m_operationThread.Start();
        }

        /// <summary>Spawns a thread and executes <paramref name="operation" /> once the start signal is rceived and signals its completion.</summary>
        /// <param name="operation">The operation to execute on a separate thread</param>
        public void ThreadedExecuteOnStartSignal(Func<bool> operation) => ThreadedExecuteOnStartSignal(default(TimeSpan), operation);

        public OperationResult ExecuteOnStartSignal(Func<bool> operation) => ExecuteOnStartSignal(default(TimeSpan), operation);

        /// <summary>Waits until operation signals completion or timeout is reached.</summary>
        /// <param name="timeout">The time to wait for the end signal to be set</param>
        /// <returns>If signal is received, <see langword="true" />; otherwise <see langword="false" /></returns>
        public bool WaitForFinish(TimeSpan timeout) => m_waitForFinish.WaitOne(timeout);

        /// <summary>Waits until the operation signals completion.</summary>
        public void WaitForFinish() => m_waitForFinish.WaitOne();

        /// <summary>Signals that the operation can begin executing</summary>
        public void SignalStart()
        {
            m_waitForStart.Set();
        }
    }
}