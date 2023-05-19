using Microsoft.Extensions.Configuration;
using TransactionalBlobStorage.Net.Extensions;

namespace TransactionalBlobStorage.Tests
{
    public static class Settings
    {
        static Settings()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Settings).Assembly)
                .Build();

            AzureStorageAccountConnectionString = configuration
                .GetValue<string>(nameof(AzureStorageAccountConnectionString))
                .ThrowIfNullEmptyOrWhiteSpace();
        }

        public static string AzureStorageAccountConnectionString { get; set; }
    }
}
