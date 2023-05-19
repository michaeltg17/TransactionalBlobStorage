namespace TransactionalBlobStorage
{
    public interface IObjectStorage
    {
        public Task<Stream?> Get(string fullFileName);
        public Task<Stream> GetOrThrow(string fullFileName);
        Task<string> Upload(string fullFileName, Stream stream);
        Task Delete(string fullFileName);
    }
}
