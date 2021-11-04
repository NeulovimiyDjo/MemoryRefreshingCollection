using System;
using ConfigurationInfo.Scheme;
using LinqToDB;
using LinqToDB.Data;

namespace TranLock
{
    public static class TransactionLocksExtensions
    {
        public static void TryAcquireLockUntilEndOfTransaction(
            this DataConnection dataConnection, string lockID, int timeout = 30)
        {
            if (dataConnection.Transaction is null)
                throw new Exception($"No transaction is currently active for lockID '{lockID}'");
            int originalTimeout = dataConnection.CommandTimeout;
            dataConnection.CommandTimeout = timeout;
            try
            {
                dataConnection.Execute(@$"
                    insert into ""TransactionLocks""(""LockID"") values (@LockID);
                    delete from ""TransactionLocks"" where ""LockID"" = @LockID",
                    new DataParameter("LockID", lockID, DataType.NVarChar)
                );
            }
            finally
            {
                dataConnection.CommandTimeout = originalTimeout;
            }
        }
    }
}
