using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Diagonactic.Multithreading.Tests
{
    [TestClass]
    public class ReentrancyGuardTests
    {
        [TestMethod]
        public void ValidateReentrancyPreventsReentrancyWithFuncVariant()
        {
            var reentrancyGuard = new ReentrancyGuard();
            bool isInGuard = false;
            bool executedGuardedCode = false;
            using (ManualResetEvent nonReentrantCode = new ManualResetEvent(false))
            using (ManualResetEvent nonReentrantCodeDone = new ManualResetEvent(false))
            {
                reentrancyGuard.IsReentrancyPrevented.Should().BeFalse();
                var allowedThread = new Thread(() =>
                                        {
                                            ReentrancyGuard.CallReentrancySafe(reentrancyGuard, () =>
                                                                               {
                                                                                   nonReentrantCode.Set();
                                                                                   isInGuard = true;
                                                                                   nonReentrantCodeDone.WaitOne();
                                                                                   isInGuard = false;
                                                                                   executedGuardedCode = true;
                                                                                   return executedGuardedCode;
                                                                               }).Should().Be(ReentrancyGuard.ReentrancyCallResult.Success);
                                            executedGuardedCode.ShouldBeEquivalentTo(true);

                                        });
                allowedThread.Start();
                nonReentrantCode.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();

                for (int i = 0; i < 1000; i++)
                {
                    reentrancyGuard.IsReentrancyPrevented.Should().BeTrue();
                    ReentrancyGuard.CallReentrancySafe(reentrancyGuard, () =>
                                                       {
                                                           throw new Exception("Reentered method despite guard being present");
#pragma warning disable 162
                                                           return true;
#pragma warning restore 162
                                                       }).ShouldBeEquivalentTo(ReentrancyGuard.ReentrancyCallResult.GuardBlocked);

                }
                nonReentrantCodeDone.Set();
                allowedThread.Join();
                reentrancyGuard.IsReentrancyPrevented.ShouldBeEquivalentTo(false);
                executedGuardedCode.ShouldBeEquivalentTo(true);
            }
        }

        [TestMethod]
        public void ValidateReentrancyPreventsReentrancyWithActionVariant()
        {
            var reentrancyGuard = new ReentrancyGuard();
            bool isInGuard = false;
            bool executedGuardedCode = false;
            using (ManualResetEvent nonReentrantCode = new ManualResetEvent(false))
            using (ManualResetEvent nonReentrantCodeDone = new ManualResetEvent(false))
            {
                reentrancyGuard.IsReentrancyPrevented.Should().BeFalse();
                var allowedThread = new Thread(() =>
                                        {
                                            ReentrancyGuard.CallReentrancySafe(reentrancyGuard, () =>
                                                                               {
                                                                                   nonReentrantCode.Set();
                                                                                   isInGuard = true;
                                                                                   nonReentrantCodeDone.WaitOne();
                                                                                   isInGuard = false;
                                                                                   executedGuardedCode = true;
                                                                                   
                                                                               }).Should().Be(true);
                                            executedGuardedCode.ShouldBeEquivalentTo(true);

                                        });
                allowedThread.Start();
                nonReentrantCode.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();

                for (int i = 0; i < 1000; i++)
                {
                    reentrancyGuard.IsReentrancyPrevented.Should().BeTrue();
                    ReentrancyGuard.CallReentrancySafe(reentrancyGuard, (Action)(() =>
                                                       {
                                                           throw new Exception("Reentered method despite guard being present");
                                                       })).ShouldBeEquivalentTo(false);

                }
                nonReentrantCodeDone.Set();
                allowedThread.Join();
                reentrancyGuard.IsReentrancyPrevented.ShouldBeEquivalentTo(false);
                executedGuardedCode.ShouldBeEquivalentTo(true);
            }


        }
    }
}
