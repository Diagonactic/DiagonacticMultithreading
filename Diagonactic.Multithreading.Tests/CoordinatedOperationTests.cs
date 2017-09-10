using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Diagonactic.Multithreading.Tests
{
    [TestClass]
    public class CoordinatedOperationTests
    {
        [TestMethod]
        public void ManualThreadedCoordination()
        {
            var c = new CoordinatedOperation();
            bool waitingToStart = true, waitingToEnd = true;

            Thread t = new Thread(() =>
                                  {
                                      var operation = c.ExecuteOnStartSignal(() =>
                                                                             {
                                                                                 waitingToStart = false;
                                                                                 return waitingToStart;
                                                                             });
                                      operation.ShouldBeEquivalentTo(CoordinatedOperation.OperationResult.Failure);
                                      waitingToEnd = false;
                                  });
            t.Start();
            Thread.Sleep(1000);
            waitingToStart.ShouldBeEquivalentTo(true);
            c.SignalStart();
            c.WaitForFinish(TimeSpan.FromSeconds(2)).ShouldBeEquivalentTo(true);
            t.Join();
            waitingToEnd.ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void AutomaticThreadCoordination()
        {
            var c = new CoordinatedOperation();
            bool waitingToStart = true;
            c.ThreadedExecuteOnStartSignal(() =>
                                           {
                                               waitingToStart = false;
                                               Thread.Sleep(1000);
                                               return waitingToStart;
                                           });

            c.ThreadedOperationResult.ShouldBeEquivalentTo(CoordinatedOperation.OperationResult.Unset);
            waitingToStart.ShouldBeEquivalentTo(true);
            c.SignalStart();
            Thread.Sleep(250);
            waitingToStart.ShouldBeEquivalentTo(false);
            c.ThreadedOperationResult.ShouldBeEquivalentTo(CoordinatedOperation.OperationResult.Unset);
            c.WaitForFinish(TimeSpan.FromSeconds(2)).ShouldBeEquivalentTo(true);
            c.ThreadedOperationResult.ShouldBeEquivalentTo(CoordinatedOperation.OperationResult.Failure);
        }
    }
}