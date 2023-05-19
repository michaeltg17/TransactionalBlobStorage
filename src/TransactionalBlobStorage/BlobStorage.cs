using Azure.Storage.Blobs;
using TransactionalBlobStorage.Net.Extensions;
using TransactionalBlobStorage.Operations;
using static TransactionalBlobStorage.Net.Helpers.TransactionHelper;

namespace TransactionalBlobStorage
{
    public class BlobStorage : IObjectStorage
    {
        readonly string connectionString;
        readonly string containerName;
        readonly IBlobStorageResourceManager blobStorageResourceManager;

        public BlobStorage(
            string connectionString,
            string containerName,
            IBlobStorageResourceManager blobStorageResourceManager)
        {
            this.connectionString = connectionString.ThrowIfNull();
            this.containerName = containerName.ThrowIfNull();
            this.blobStorageResourceManager = blobStorageResourceManager.ThrowIfNull();
        }

        public Task Delete(string fullFileName)
        {
            var container = new BlobContainerClient(connectionString, containerName);
            var operation = new DeleteBlobOperation(container, fullFileName);

            if (IsInTransaction())
            {
                return blobStorageResourceManager.ExecuteOperation(operation);
            }

            return operation.Execute();
        }

        public async Task<Stream?> Get(string fullFileName)
        {
            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(fullFileName);

            return await blobClient.ExistsAsync()
                ? await blobClient.OpenReadAsync()
                : null;
        }

        public Task<Stream> GetOrThrow(string fullFileName)
        {
            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(fullFileName);

            return blobClient.OpenReadAsync();
        }

        public async Task<string> Upload(string fullFileName, Stream stream)
        {
            var container = new BlobContainerClient(connectionString, containerName);
            var operation = new UploadBlobOperation(container, fullFileName, stream);

            if (IsInTransaction())
            {
                return await blobStorageResourceManager.ExecuteOperation(operation);
            }

            return await operation.Execute();
        }
    }
}
