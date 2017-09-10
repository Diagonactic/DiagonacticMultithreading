using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Diagonactic.Multithreading.Tests
{
    [TestClass]
    public class ThreadSafeDisposableBaseTests
    {
        [TestMethod]
        public void CanDispose()
        {
            var d = new DisposableTest();
            d.FinishedDisposeManagedResources.ShouldBeEquivalentTo(false);
            d.FinishedDisposeUnmanagedResources.ShouldBeEquivalentTo(false);
            d.IsResourceAllocated.ShouldBeEquivalentTo(true);
            d.WaitDisposeManagedResources.Set();
            d.WaitDisposeUnmanagedResources.Set();

            d.Dispose();
            d.FinishedDisposeManagedResources.ShouldBeEquivalentTo(true);
            d.FinishedDisposeUnmanagedResources.ShouldBeEquivalentTo(true);
            d.IsResourceAllocated.ShouldBeEquivalentTo(false);
            d.HasDisposedManagedResources.WaitOne(1).ShouldBeEquivalentTo(true);
            d.HasDisposedUnmanagedResources.WaitOne(1).ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void CanDisposeThreadedSafely()
        {
            var d = new DisposableTest();
            d.EnteredDisposeManagedResources.ShouldBeEquivalentTo(false);
            d.EnteredDisposeUnmanagedResorces.ShouldBeEquivalentTo(false);
            Thread disposingThread = new Thread(() =>
                                                {
                                                    
                                                    d.Dispose();
                                                    d.FinishedDisposeManagedResources.ShouldBeEquivalentTo(true);
                                                    d.FinishedDisposeUnmanagedResources.ShouldBeEquivalentTo(true);
                                                    d.IsResourceAllocated.ShouldBeEquivalentTo(false);
                                                });
            disposingThread.Start();
            for (int i = 0; i < 1000; i++)
            {
                d.FinishedDisposeManagedResources.ShouldBeEquivalentTo(false);
                d.FinishedDisposeUnmanagedResources.ShouldBeEquivalentTo(false);
                Thread tryToDispose = new Thread(() => d.Dispose());
                tryToDispose.Start();
            }
            d.IsResourceAllocated.ShouldBeEquivalentTo(false);
            d.EnteredDisposeManagedResources.ShouldBeEquivalentTo(true);
            d.EnteredDisposeUnmanagedResorces.ShouldBeEquivalentTo(false);
            d.FinishedDisposeManagedResources.ShouldBeEquivalentTo(false);
            d.FinishedDisposeUnmanagedResources.ShouldBeEquivalentTo(false);
            d.WaitDisposeManagedResources.Set();
            d.WaitDisposeUnmanagedResources.Set();
            d.HasDisposedManagedResources.WaitOne(TimeSpan.FromSeconds(5));
            d.HasDisposedUnmanagedResources.WaitOne(TimeSpan.FromSeconds(5));
            d.FinishedDisposeManagedResources.ShouldBeEquivalentTo(true);
            d.FinishedDisposeUnmanagedResources.ShouldBeEquivalentTo(true);
            d.TimesDisposeManagedRan.ShouldBeEquivalentTo(1);
            d.TimesDisposeUnmanagedRan.ShouldBeEquivalentTo(1);
        }

        private class DisposableTest : ThreadSafeDisposableBase
        {
            public bool FinishedDisposeManagedResources { get; private set; }
            public bool FinishedDisposeUnmanagedResources { get; private set; }

            public int TimesDisposeManagedRan { get; private set; }
            public int TimesDisposeUnmanagedRan { get; private set; }
            public bool IsResourceAllocated => base.IsAllocated;
            public ManualResetEvent HasDisposedManagedResources { get; } = new ManualResetEvent(false);
            public ManualResetEvent HasDisposedUnmanagedResources { get; } = new ManualResetEvent(false);
            public ManualResetEvent WaitDisposeManagedResources { get; } = new ManualResetEvent(false);
            public ManualResetEvent WaitDisposeUnmanagedResources { get; } = new ManualResetEvent(false);

            public bool EnteredDisposeManagedResources { get; private set; }
            public bool EnteredDisposeUnmanagedResorces { get; private set; }

            
            protected override void DisposeManagedResources()
            {
                TimesDisposeManagedRan++;
                EnteredDisposeManagedResources = true;
                WaitDisposeManagedResources.WaitOne(TimeSpan.FromSeconds(5));
                FinishedDisposeManagedResources = true;
                HasDisposedManagedResources.Set();
                base.DisposeManagedResources();
            }

            protected override void DisposeUnmanagedResources()
            {
                TimesDisposeUnmanagedRan++;
                EnteredDisposeUnmanagedResorces = true;
                WaitDisposeUnmanagedResources.WaitOne(TimeSpan.FromSeconds(5));
                FinishedDisposeUnmanagedResources = true;
                HasDisposedUnmanagedResources.Set();
                base.DisposeUnmanagedResources();
            }
        }
    }
}