namespace TransactionalBlobStorage.Operations.Base
{
    public abstract class BlobOperation : TransactionalBlobOperation
    {
        public abstract Task Execute();
    }
}
