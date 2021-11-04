
using Npgsql;

private const string TransactionLocksTestTable = nameof(TransactionLocksTestTable);
private const string IntField = nameof(IntField);

[SetUp]
public async Task SetUp()
{
    using Connection con = Connection.Create();
    await con.SetCommand(
        @$"create table ""{TransactionLocksTestTable}""(""{IntField}"" int)"
    ).ExecuteNonQueryAsync();
}

[TearDown]
public async Task TearDown()
{
    using Connection _ = Connection.Create();
    await con.SetCommand(@$"drop table ""{TransactionLocksTestTable}""").ExecuteNonQueryAsync();
}

[Test]
public async Task AcquireLock_AllowsParallelExecution_ForDifferentLockIDs()
{
    TestWorker testWorker1 = new();
    TestWorker testWorker2 = new();
    AutoResetEvent autoResetEvent1 = new(false);
    AutoResetEvent autoResetEvent2 = new(false);
    Task t1 = ExecuteInTransactionWithLock(db => testWorker1.DoWork(db, autoResetEvent1, autoResetEvent2), "TestLockID1");
    Task t2 = ExecuteInTransactionWithLock(db => testWorker2.DoWork(db, autoResetEvent2, autoResetEvent1), "TestLockID2");
    await Task.WhenAll(t1, t2);
    Assert.False(testWorker1.TimedOutOnOtherWait);
}

[Test]
public async Task AcquireLock_ForbidsParallelExecution_ForSameLockID()
{
    TestWorker testWorker1 = new();
    TestWorker testWorker2 = new();
    AutoResetEvent autoResetEvent1 = new(false);
    AutoResetEvent autoResetEvent2 = new(false);
    Task t1 = ExecuteInTransactionWithLock(db => testWorker1.DoWork(db, autoResetEvent1, autoResetEvent2), "TestLockID");
    Task t2 = ExecuteInTransactionWithLock(db => testWorker2.DoWork(db, autoResetEvent2, autoResetEvent1), "TestLockID");
    await Task.WhenAll(t1, t2);
    Assert.True(testWorker1.TimedOutOnOtherWait);
}

[Test]
public void AcquireLock_Throws_OnTimeout()
{
    NpgsqlException ex = Assert.ThrowsAsync<NpgsqlException>(async () =>
    {
        Task t1 = ExecuteInTransactionWithLock(async db => await Task.Delay(2500), "TestLockID");
        await Task.Delay(700);
        Task t2 = ExecuteInTransactionWithLock(db => Task.CompletedTask, "TestLockID", 1);
        await Task.WhenAll(t1, t2);
    });

    Assert.NotNull(ex.InnerException, "Inner exception with timeout message is expected");
    Assert.That(ex.InnerException.Message, Contains.Substring("Timeout during reading attempt"));
}

[Test]
public async Task AcquireLock_CausesNoContetionForDifferentLockIDs_UnderHeavyLoad()
{
    long elapsedMillisecondsNoLock = await RunManyJobsParallel(
        i => ExecuteInTransactionWithNoLock(DoSomeJob),
        90 // less than postgresql max connections/connection string pool size
    );
    long elapsedMillisecondsLock = await RunManyJobsParallel(
        i => ExecuteInTransactionWithLock(DoSomeJob, $"TestLockID{i}"),
        90 // less than postgresql max connections/connection string pool size
    );
    Assert.Less(elapsedMillisecondsLock, elapsedMillisecondsNoLock * 3);
}

private async Task DoSomeJob(Connection con)
{
    await con.SetCommand(
        @$"insert into ""{TransactionLocksTestTable}""(""{IntField}"") values(1)"
    ).ExecuteNonQueryAsync();
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

private class TestWorker
{
    public bool TimedOutOnOtherWait { get; private set; } = false;

    public async Task DoWork(Connection con, AutoResetEvent thisCompletedEvent, AutoResetEvent otherCompletedEvent)
    {
        await con.SetCommand(
            @$"insert into ""{TransactionLocksTestTable}""(""{IntField}"") values(1)"
        ).ExecuteNonQueryAsync();
        thisCompletedEvent.Set();

        if (!otherCompletedEvent.WaitOne(TimeSpan.FromMilliseconds(2500)))
            TimedOutOnOtherWait = true;
    }
}
