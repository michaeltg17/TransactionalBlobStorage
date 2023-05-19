namespace TransactionalBlobStorage.Operations.Base
{
    public abstract class TransactionalBlobOperation
    {
        const string BackupPrefix = "TransactionlBackupBlob_";
        public abstract Task<object?> ExecuteInTransaction();
        public abstract Task Rollback();
        public static string GetBackupBlobName() => BackupPrefix + Guid.NewGuid().ToString();
    }
}
