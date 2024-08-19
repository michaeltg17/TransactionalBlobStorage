using Azure.Storage.Blobs;
using TransactionalBlobStorage.Operations;
using TransactionalBlobStorage.Operations.Base;
using static TransactionalBlobStorage.Net.Helpers.TransactionHelper;

namespace TransactionalBlobStorage
{
    public class BlobStorage(BlobContainerClient containerClient, IBlobStorageResourceManager blobStorageResourceManager)
    {
        public Task Delete(string fullFileName)
        {
            var operation = new DeleteBlobOperation(containerClient, fullFileName);

            if (IsInTransaction())
            {
                return blobStorageResourceManager.ExecuteOperation(operation);
            }

            return operation.Execute();
        }

        public Task<bool> HasTransactionalBackupBlobs()
        {
            return containerClient
                .GetBlobsAsync(prefix: TransactionalBlobOperation.BackupPrefix)
                .AnyAsync()
                .AsTask();
        }

        public async Task<Stream?> Get(string fullFileName)
        {
            var blobClient = containerClient.GetBlobClient(fullFileName);

            return await blobClient.ExistsAsync()
                ? await blobClient.OpenReadAsync()
                : null;
        }

        public Task<Stream> GetOrThrow(string fullFileName)
        {
            var blobClient = containerClient.GetBlobClient(fullFileName);

            return blobClient.OpenReadAsync();
        }

        public async Task<string> Upload(string fullFileName, Stream stream)
        {
            var operation = new UploadBlobOperation(containerClient, fullFileName, stream);

            if (IsInTransaction())
            {
                return await blobStorageResourceManager.ExecuteOperation(operation);
            }

            return await operation.Execute();
        }
    }
}
