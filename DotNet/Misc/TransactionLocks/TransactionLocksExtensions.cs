using LinqToDB;
using LinqToDB.Data;

namespace Helpers
{
    public static class TransactionLocksExtensions
    {
        public static void TryAcquireLockUntilEndOfTransaction(
            this DataConnectionTransaction tran, string lockID, int timeout = 30)
        {
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
