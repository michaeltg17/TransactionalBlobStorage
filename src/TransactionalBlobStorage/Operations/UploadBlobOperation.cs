using Azure.Storage.Blobs;
using TransactionalBlobStorage.Net.Extensions;
using TransactionalBlobStorage.Operations.Base;

namespace TransactionalBlobStorage.Operations
{
    public class UploadBlobOperation : BlobOperation<string>
    {
        readonly BlobContainerClient containerClient;
        readonly string fullFileName;
        readonly Stream stream;

        public UploadBlobOperation(
            BlobContainerClient containerClient,
            string fullFileName,
            Stream stream)
        {
            this.containerClient = containerClient.ThrowIfNull();
            this.fullFileName = fullFileName.ThrowIfNullEmptyOrWhiteSpace();
            this.stream = stream.ThrowIfNull();
        }

        public override async Task<string> Execute()
        {
            var blobClient = containerClient.GetBlobClient(fullFileName);

            await blobClient.UploadAsync(stream);

            return blobClient.Uri.AbsoluteUri;
        }

        public override async Task<object?> ExecuteInTransaction()
        {
            return await Execute();
        }

        public override async Task Rollback()
        {
            var blobClient = containerClient.GetBlobClient(fullFileName);

            await blobClient.DeleteAsync();
        }
    }
}
