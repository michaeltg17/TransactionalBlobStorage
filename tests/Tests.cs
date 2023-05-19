﻿using FluentAssertions;
using Xunit;
using System.Transactions;
using Azure;
using TransactionalBlobStorage.Net.Extensions;

namespace TransactionalBlobStorage.Tests
{
    public class Tests
    {
        static IObjectStorage GetBlobStorage()
        {
            return new BlobStorage(
                Settings.AzureStorageAccountConnectionString,
                "tests",
                new BlobStorageResourceManager());
        }

        static (Stream stream, byte[] content, string fullFileName) GetRandomFile()
        {
            var content = new byte[1024];
            Random.Shared.NextBytes(content);
            var stream = new MemoryStream(content);
            var fullFileName = $"{nameof(Tests)}_{Guid.NewGuid()}.txt";

            return (stream, content, fullFileName);
        }

        [Fact]
        public async Task GivenFile_WhenUpload_Uploaded()
        {
            //Given
            var storage = GetBlobStorage();
            var (stream, uploadedFile, uploadedFileFullName) = GetRandomFile();

            //When
            await storage.Upload(uploadedFileFullName, stream);

            //Then
            var downloadedFile = await storage.GetOrThrow(uploadedFileFullName);
            downloadedFile.ReadAllBytes().Should().BeEquivalentTo(uploadedFile);

            //Clear
            await storage.Delete(uploadedFileFullName);
        }

        [Fact]
        public async Task GivenFile_WhenUploadInOkTransaction_Uploaded()
        {
            //Given
            var storage = GetBlobStorage();
            var (stream, uploadedFile, uploadedFileFullName) = GetRandomFile();

            //When
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await storage.Upload(uploadedFileFullName, stream);
                transactionScope.Complete();
            }

            //Then
            var downloadedFile = await storage.GetOrThrow(uploadedFileFullName);
            downloadedFile.ReadAllBytes().Should().BeEquivalentTo(uploadedFile);

            //Clear
            await storage.Delete(uploadedFileFullName);
        }

        [Fact]
        public async Task GivenFile_WhenUploadInFailedTransaction_Deleted()
        {
            //Given
            var storage = GetBlobStorage();
            var (stream, uploadedFile, uploadedFileFullName) = GetRandomFile();

            //When
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await storage.Upload(uploadedFileFullName, stream);
            }

            //Then
            var getUploadedFile = () => storage.GetOrThrow(uploadedFileFullName);
            (await getUploadedFile.Should().ThrowAsync<RequestFailedException>()).Which.ErrorCode.Should().Be("BlobNotFound");
        }

        [Fact]
        public async Task GivenBlob_WhenDelete_Deleted()
        {
            //Given
            var storage = GetBlobStorage();
            var (stream, _, uploadedFileFullName) = GetRandomFile();
            await storage.Upload(uploadedFileFullName, stream);

            //When
            await storage.Delete(uploadedFileFullName);

            //Then
            (await storage.Get(uploadedFileFullName)).Should().BeNull();
        }

        [Fact]
        public async Task GivenBlob_WhenDeleteInOkTransaction_Deleted()
        {
            //Given
            var storage = GetBlobStorage();
            var (stream, _, uploadedFileFullName) = GetRandomFile();
            await storage.Upload(uploadedFileFullName, stream);

            //When
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await storage.Delete(uploadedFileFullName);
                transactionScope.Complete();
            }

            //Then
            (await storage.Get(uploadedFileFullName)).Should().BeNull();
        }

        [Fact]
        public async Task GivenBlob_WhenDeleteInFailedTransaction_NotDeleted()
        {
            //Given
            var storage = GetBlobStorage();
            var (stream, uploadedFile, uploadedFileFullName) = GetRandomFile();
            await storage.Upload(uploadedFileFullName, stream);

            //When
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await storage.Delete(uploadedFileFullName);
            }

            //Then
            (await storage.Get(uploadedFileFullName))!.ReadAllBytes().Should().BeEquivalentTo(uploadedFile);
        }

        [Fact]
        public async Task GivenBlob_WhenDeleteUploadInTransaction_OriginalBlob()
        {
            //Given
            var storage = GetBlobStorage();
            var (stream, initialBlobContent, uploadedBlobFullName) = GetRandomFile();
            await storage.Upload(uploadedBlobFullName, stream);

            //When
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await storage.Delete(uploadedBlobFullName);

                var (stream2, _, uploadedFileFullName2) = GetRandomFile();
                await storage.Upload(uploadedFileFullName2, stream2);
            }

            //Then
            (await storage.Get(uploadedBlobFullName))!.ReadAllBytes().Should().BeEquivalentTo(initialBlobContent);
        }
    }
}
