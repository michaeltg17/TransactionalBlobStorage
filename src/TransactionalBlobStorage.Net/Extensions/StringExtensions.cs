using System.Runtime.CompilerServices;

namespace TransactionalBlobStorage.Net.Extensions
{
    public static class StringExtensions
    {
        public static string ThrowIfNullEmptyOrWhiteSpace(this string? @string, [CallerArgumentExpression(nameof(@string))] string? name = null)
        {
            if (string.IsNullOrWhiteSpace(@string))
                throw new ArgumentException($"String cannot be null, empty or whitespace.", name);
            return @string;
        }
    }
}
