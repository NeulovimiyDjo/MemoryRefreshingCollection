using System;
using LinqToDB;
using LinqToDB.Data;

namespace TransactionLocks
{
    public static class TransactionLocksExtensions
    {
        public static void TryAcquireLockUntilEndOfTransaction(
            this DataConnectionTransaction tran, string lockID, int timeout = 30)
        {
            if (tran is null)
                throw new Exception($"No transaction is currently active for lockID '{lockID}'");
            int originalTimeout = tran.DataConnection.CommandTimeout;
            tran.DataConnection.CommandTimeout = timeout;
            try
            {
                tran.DataConnection.Execute(@"
                    insert into TransactionLocks(LockID) values (@LockID)
                    delete from TransactionLocks where LockID = @LockID",
                    new DataParameter("@LockID", lockID, DataType.NVarChar)
                );
            }
            finally
            {
                tran.DataConnection.CommandTimeout = originalTimeout;
            }
        }
    }
}
