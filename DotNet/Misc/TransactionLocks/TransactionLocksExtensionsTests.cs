using LinqToDB.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Helpers.Tests
{
    [TestFixture]
    public class TransactionLocksExtensionsTests : TestBaseCommon
    {
        private const string TransactionLocksTestTable = nameof(TransactionLocksTestTable);
        private const string IntField = nameof(IntField);

        [SetUp]
        public async Task SetUp()
        {
            using Connection con = Connection.Create();
            await con.SetCommand(
                $"create table {TransactionLocksTestTable}({IntField} int)"
            ).ExecuteNonQueryAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            using Connection _ = Connection.Create();
            await con.SetCommand($"drop table {TransactionLocksTestTable}").ExecuteNonQueryAsync();
        }


        [Test]
        public async Task AcquireLock_AllowsParallelExecution_ForDifferentLockIDs()
        {
            TestWorker testWorker1 = new();
            TestWorker testWorker2 = new();
            Task t1 = ExecuteInTransactionWithLock(testWorker1.DoWork, "TestLockID1");
            Task t2 = ExecuteInTransactionWithLock(testWorker2.DoWork, "TestLockID2");
            await Task.WhenAll(t1, t2);
            Assert.AreEqual(2, testWorker1.CountAtEnd - testWorker1.CountAtStart);
            Assert.AreEqual(2, testWorker2.CountAtEnd - testWorker2.CountAtStart);
        }

        [Test]
        public async Task AcquireLock_ForbidsParallelExecution_ForSameLockID()
        {
            TestWorker testWorker1 = new();
            TestWorker testWorker2 = new();
            Task t1 = ExecuteInTransactionWithLock(testWorker1.DoWork, "TestLockID");
            Task t2 = ExecuteInTransactionWithLock(testWorker2.DoWork, "TestLockID");
            await Task.WhenAll(t1, t2);
            Assert.AreEqual(1, testWorker1.CountAtEnd - testWorker1.CountAtStart);
            Assert.AreEqual(1, testWorker2.CountAtEnd - testWorker2.CountAtStart);
        }

        [Test]
        public void AcquireLock_Throws_OnTimeout()
        {
            SqlException ex = Assert.ThrowsAsync<SqlException>(async () =>
            {
                Task t1 = ExecuteInTransactionWithLock(async con => await Task.Delay(2500), "TestLockID");
                await Task.Delay(700);
                Task t2 = ExecuteInTransactionWithLock(con => Task.CompletedTask, "TestLockID", 1);
                await Task.WhenAll(t1, t2);
            });

            Assert.That(ex.Message, Contains.Substring("Timeout expired"));
        }

        [Test]
        public async Task AcquireLock_CausesNoContetionForDifferentLockIDs_UnderHeavyLoad()
        {
            long elapsedMillisecondsNoLock = await RunManyJobsParallel(
                i => ExecuteInTransactionWithNoLock(DoSomeJob),
                1000
            );
            long elapsedMillisecondsLock = await RunManyJobsParallel(
                i => ExecuteInTransactionWithLock(DoSomeJob, $"TestLockID{i}"),
                1000
            );
            Assert.Less(elapsedMillisecondsLock, elapsedMillisecondsNoLock * 2);
        }


        private async Task DoSomeJob(Connection con)
        {
            await con.SetCommand(
                $"insert into {TransactionLocksTestTable}({IntField}) values(1)"
            ).ExecuteNonQueryAsync();
        }

        private class TestWorker
        {
            public int CountAtStart { get; private set; }
            public int CountAtEnd { get; private set; }

            public async Task DoWork(Connection con)
            {
                CountAtStart = await con.SetCommand(
                    $"select count(*) from {TransactionLocksTestTable} with(nolock)"
                ).ExecuteAsync<int>();

                await Task.Delay(100);

                await con.SetCommand(
                    $"insert into {TransactionLocksTestTable}({IntField}) values(1)"
                ).ExecuteNonQueryAsync();

                await Task.Delay(100);

                CountAtEnd = await con.SetCommand(
                    $"select count(*) from {TransactionLocksTestTable} with(nolock)"
                ).ExecuteAsync<int>();
            }
        }

        private async Task<long> RunManyJobsParallel(Func<int, Task> jobFunc, int count)
        {
            Stopwatch sw = new();
            sw.Start();
            List<Task> jobs = new();
            for (int i = 0; i < count; i++)
            {
                Task t = jobFunc(i);
                jobs.Add(t);
            }
            await Task.WhenAll(jobs);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        private async Task ExecuteInTransactionWithNoLock(
            Func<Connection, Task> doWorkFunc)
        {
            using Connection con = Connection.CreateNew();
            await con.SetXactAbortOn();
            DataConnectionTransaction tran = await con.BeginTransactionAsync();
            try
            {
                await doWorkFunc(con);
                await con.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await con.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task ExecuteInTransactionWithLock(
            Func<Connection, Task> doWorkFunc, string lockID, int timeout = 30)
        {
            using Connection con = Connection.CreateNew();
            await con.SetXactAbortOn();
            DataConnectionTransaction tran = await con.BeginTransactionAsync();
            try
            {
                tran.TryAcquireLockUntilEndOfTransaction(lockID, timeout);
                await doWorkFunc(con);
                await con.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await con.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
