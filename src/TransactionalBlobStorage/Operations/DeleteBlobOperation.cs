using Azure.Storage.Blobs;
using TransactionalBlobStorage.Net.Extensions;
using TransactionalBlobStorage.Operations.Base;

namespace TransactionalBlobStorage.Operations
{
    public class DeleteBlobOperation(
        BlobContainerClient containerClient,
        string fullFileName) : BlobOperation
    {
        readonly BlobContainerClient containerClient = containerClient.ThrowIfNull();
        readonly string fullFileName = fullFileName.ThrowIfNullEmptyOrWhiteSpace();
        string tempFileFullName;

        public override Task Execute()
        {
            var subjectBlobClient = containerClient.GetBlobClient(fullFileName);
            return subjectBlobClient.DeleteAsync();
        }

        public override async Task<object?> ExecuteInTransaction()
        {
            tempFileFullName = GetBackupBlobName();
            var subjectBlobClient = containerClient.GetBlobClient(fullFileName);
            var tempBlobClient = containerClient.GetBlobClient(tempFileFullName);
            await tempBlobClient.SyncCopyFromUriAsync(subjectBlobClient.Uri);
            await Execute();
            return null;
        }

        public override async Task Rollback()
        {
            var subjectBlobClient = containerClient.GetBlobClient(fullFileName);
            var tempBlobClient = containerClient.GetBlobClient(tempFileFullName);
            await subjectBlobClient.SyncCopyFromUriAsync(tempBlobClient.Uri);
            await ClearBackups();
        }

        public override async Task ClearBackups()
        {
            var tempBlobClient = containerClient.GetBlobClient(tempFileFullName);
            await tempBlobClient.DeleteAsync();
        }
    }
}
