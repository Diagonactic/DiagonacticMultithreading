using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Diagonactic.Multithreading.Tests
{
    [TestClass]
    public class ReaderWriterLockExtensionsTest
    {
        public readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        [TestMethod]
        public void ExecuteWrite()
        {
            var s = "test";
            m_lock.ExecuteWrite(() =>
                                {
                                    s = "Acquired Write";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteRead(() => { throw new Exception("Read was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                });

            var writeResult = m_lock.ExecuteWrite(() =>
                                {
                                    s = "Acquired Write";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteRead(() => { throw new Exception("Read was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    return "Done";
                                });

            var lockWriteResult = m_lock.ExecuteWrite(() =>
                                {
                                    s = "Acquired Write";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteRead(() => { throw new Exception("Read was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    return "Done";
                                }, TimeSpan.FromSeconds(1));
            lockWriteResult.Result.ShouldBeEquivalentTo("Done");
            lockWriteResult.State.ShouldBeEquivalentTo(LockResultOutcome.Completed);
            writeResult.ShouldBeEquivalentTo("Done");
            s.ShouldBeEquivalentTo("Acquired Write");
        }

        [TestMethod]
        public void ExecuteRead()
        {
            var s = "test";
            m_lock.ExecuteRead(() =>
                                {
                                    s = "Acquired Read";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { s = "Acquired Upgradable Read"; }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(true);
                                });
            s.ShouldBeEquivalentTo("Acquired Upgradable Read");

            var readResult = m_lock.ExecuteRead(() =>
                                {
                                    s = "Acquired Read";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { s = "Acquired Upgradable Read"; }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(true);
                                    return "Done";
                                });

            LockResult<string> lockReadResult = m_lock.ExecuteRead(() =>
                                {
                                    s = "Acquired Read";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { s = "Acquired Upgradable Read"; }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(true);
                                    return "Done";
                                }, TimeSpan.FromSeconds(1));
            lockReadResult.Result.ShouldBeEquivalentTo("Done");
            lockReadResult.State.ShouldBeEquivalentTo(LockResultOutcome.Completed);

            s.ShouldBeEquivalentTo("Acquired Upgradable Read");
            readResult.ShouldBeEquivalentTo("Done");
        }

        [TestMethod]
        public void ExecuteUpgradableRead()
        {
            var s = "test";
            m_lock.ExecuteUpgradableRead(() =>
                                {
                                    s = "Acquired Read";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { s = "Acquired Upgradable Read"; }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    m_lock.ExecuteWrite(() => { s = "Acquired Write"; }, TimeSpan.FromSeconds(1));
                                    
                                });
            s.ShouldBeEquivalentTo("Acquired Write");

            s = "test";
            var readResult = m_lock.ExecuteUpgradableRead(() =>
                                {
                                    s = "Acquired Read";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { s = "Acquired Upgradable Read"; }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    m_lock.ExecuteWrite(() => { s = "Acquired Write"; }, TimeSpan.FromSeconds(1));
                                    return "Done";
                                });
            s.ShouldBeEquivalentTo("Acquired Write");
            readResult.ShouldBeEquivalentTo("Done");

            var lockReadResult = m_lock.ExecuteUpgradableRead(() =>
                                {
                                    s = "Acquired Read";
                                    bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() => { throw new Exception("Write was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    result = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { s = "Acquired Upgradable Read"; }, TimeSpan.FromMilliseconds(200)));
                                    result.ShouldBeEquivalentTo(false);
                                    m_lock.ExecuteWrite(() => { s = "Acquired Write"; }, TimeSpan.FromSeconds(1));
                                    return "Done";
                                }, TimeSpan.FromSeconds(1));
            lockReadResult.Result.ShouldBeEquivalentTo("Done");
            lockReadResult.State.ShouldBeEquivalentTo(LockResultOutcome.Completed);

            s.ShouldBeEquivalentTo("Acquired Write");
        }

        [TestMethod]
        public void ReadUsing()
        {
            string s = null;
            using (m_lock.ReadUsing())
            {
                s = "test";
                bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteWrite(() =>
                                                  {
                                                      throw new Exception("Write lock was allowed!");
                                                  }, TimeSpan.FromMilliseconds(200)));
                result.ShouldBeEquivalentTo(false);
            }
            s.ShouldBeEquivalentTo("test");
            bool writeResult = m_lock.ExecuteWrite(() => { s = "Acquired Lock"; }, TimeSpan.FromMilliseconds(2000));
            s.ShouldBeEquivalentTo("Acquired Lock");
            writeResult.ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void WriteUsing()
        {
            string s = null;
            using (m_lock.WriteUsing())
            {
                s = "test";
                bool result = SyncExecuteOnOtherThread(() => m_lock.ExecuteRead(() => { throw new Exception("Read lock was allowed!"); }, TimeSpan.FromMilliseconds(200)));
                result.ShouldBeEquivalentTo(false);
            }
            s.ShouldBeEquivalentTo("test");
        }

        [TestMethod]
        public void UpgradableReadUsingWithWrite()
        {
            string s = null;
            using (m_lock.UpgradableReadUsing())
            {
                s = "test";
                bool readResult = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { throw new Exception("Shouldn't have allowed another reader!"); }, TimeSpan.FromMilliseconds(200)));
                m_lock.EnterUpgradeableReadLock();
                bool writeResult = m_lock.ExecuteWrite(() =>
                                                                                 {
                                                                                     s = "I was able to acquire lock";
                                                                                 }, TimeSpan.FromMilliseconds(2000));
                writeResult.ShouldBeEquivalentTo(true);
                readResult.ShouldBeEquivalentTo(false);
            }
            s.ShouldBeEquivalentTo("I was able to acquire lock");
        }

        [TestMethod]
        public void UpgradableReadUsingWithRead()
        {
            string s = null;
            using (m_lock.UpgradableReadUsing())
            {
                s = "test";
                bool readResult = SyncExecuteOnOtherThread(() => m_lock.ExecuteUpgradableRead(() => { throw new Exception("Shouldn't have allowed another reader!"); }, TimeSpan.FromMilliseconds(200)));
                m_lock.EnterUpgradeableReadLock();
                bool writeResult = m_lock.ExecuteRead(() =>
                                                                                 {
                                                                                     s = "I was able to acquire lock";
                                                                                 }, TimeSpan.FromMilliseconds(2000));
                writeResult.ShouldBeEquivalentTo(true);
                readResult.ShouldBeEquivalentTo(false);
            }
            s.ShouldBeEquivalentTo("I was able to acquire lock");
        }

        private T SyncExecuteOnOtherThread<T>(Func<T> toExecute)
        {
            using (var mre = new ManualResetEvent(false))
            {
                T retVal = default(T);
                Thread t = new Thread(() =>
                                      {
                                          try
                                          {
                                              retVal = toExecute();
                                          }
                                          finally
                                          {
                                              mre.Set();
                                          }
                                      });
                t.Start();
                mre.WaitOne();
                t.Join();
                return retVal;
            }
        }
    }
    [TestClass]
    public class InterlockedBooleanTest
    {

        [TestMethod]
        public void TestSet()
        {
            var ib = new InterlockedBoolean(true);
            ib.Value.ShouldBeEquivalentTo(true);

            ib.Value = false;
            ib.Value.ShouldBeEquivalentTo(false);

            ib.Value = true;
            ib.Value.ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void TestInitialize()
        {
            var ib=new InterlockedBoolean();
            ib.Value.Should().Be(false);
            ib = new InterlockedBoolean(false);
            ib.Value.Should().Be(false);
            ib = new InterlockedBoolean(true);
            ib.Value.Should().Be(true);
        }

        [TestMethod]
        public void TestNotEquals()
        {
            var ibTrue = new InterlockedBoolean(true);
            var ibFalse = new InterlockedBoolean(false);
            TestEquality(ibTrue, ibFalse, false);
        }

        [TestMethod]
        public void TestEquals()
        {
            var ibTrue = new InterlockedBoolean(true);
            var ibAlsoTrue = new InterlockedBoolean(true);
            var ibFalse = new InterlockedBoolean(false);
            TestEquality(ibTrue, ibAlsoTrue, true);
            ibTrue.Equals(true).ShouldBeEquivalentTo(true);
            ibTrue.Equals(false).ShouldBeEquivalentTo(false);
            ibFalse.Equals(false).ShouldBeEquivalentTo(true);
            ibFalse.Equals(true).ShouldBeEquivalentTo(false);
        }

        [TestMethod]
        public void TestReferenceEquals()
        {
            var ibTrue = new InterlockedBoolean(true);
            var ibAlsoTrue = ibTrue;
            TestEquality(ibTrue, ibAlsoTrue, true);
        }

        [TestMethod]
        public void TestCast()
        {
            bool @true = (bool)(new InterlockedBoolean(true));
            @true.ShouldBeEquivalentTo(true);
            bool @false = (bool)(new InterlockedBoolean(false));
            @false.ShouldBeEquivalentTo(false);
        }


        public void TestEquality(InterlockedBoolean first, InterlockedBoolean second, bool expected)
        {
            (first == second).ShouldBeEquivalentTo(expected);
            first.Equals(second).ShouldBeEquivalentTo(expected);
            (first.GetHashCode() == second.GetHashCode()).ShouldBeEquivalentTo(expected);
            (first != second).ShouldBeEquivalentTo(!expected);
        }

        

    }
}