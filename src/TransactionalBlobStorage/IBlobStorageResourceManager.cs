using TransactionalBlobStorage.Operations.Base;

namespace TransactionalBlobStorage
{
    public interface IBlobStorageResourceManager
    {
        public Task ExecuteOperation(BlobOperation operation);
        public Task<T> ExecuteOperation<T>(BlobOperation<T> operation);
    }
}
