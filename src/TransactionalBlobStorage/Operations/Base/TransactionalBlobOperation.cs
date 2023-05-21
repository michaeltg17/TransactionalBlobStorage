namespace TransactionalBlobStorage.Operations.Base
{
    public abstract class TransactionalBlobOperation
    {
        public const string BackupPrefix = "TransactionalBackupBlob_";
        public abstract Task<object?> ExecuteInTransaction();
        public abstract Task Rollback();

        public virtual Task ClearBackups()
        {
            return Task.CompletedTask;
        }
        public static string GetBackupBlobName() => BackupPrefix + Guid.NewGuid().ToString();
    }
}
