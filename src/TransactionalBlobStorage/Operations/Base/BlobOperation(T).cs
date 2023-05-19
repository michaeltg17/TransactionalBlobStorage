namespace TransactionalBlobStorage.Operations.Base
{
    public abstract class BlobOperation<T> : TransactionalBlobOperation
    {
        public abstract Task<T> Execute();
    }
}
