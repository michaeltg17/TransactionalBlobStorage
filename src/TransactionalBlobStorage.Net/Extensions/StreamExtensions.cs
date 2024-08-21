namespace TransactionalBlobStorage.Net.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream @stream)
        {
            if (@stream is MemoryStream @memoryStream) return @memoryStream.ToArray();

            using var memoryStream = new MemoryStream();
            @stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
