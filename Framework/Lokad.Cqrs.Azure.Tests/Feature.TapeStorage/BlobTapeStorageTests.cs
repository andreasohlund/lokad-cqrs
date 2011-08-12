using System;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class BlobTapeStorageTests : TapeStorageTests
    {
        const string ContainerName = "blob-tape-test";

        readonly CloudStorageAccount _cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
        ITapeStorageFactory _storageFactory;

        protected override void PrepareEnvironment()
        {
            var cloudBlobClient = _cloudStorageAccount.CreateCloudBlobClient();

            try
            {
                cloudBlobClient.GetContainerReference(ContainerName).FetchAttributes();
                throw new InvalidOperationException("Container '" + ContainerName + "' already exists!");
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode != StorageErrorCode.ResourceNotFound)
                    throw new InvalidOperationException("Container '" + ContainerName + "' already exists!");
            }
        }

        protected override ITapeStream InitializeAndGetTapeStorage()
        {
            var config = AzureStorage.CreateConfig(_cloudStorageAccount);
            _storageFactory = new BlobTapeStorageFactory(config, ContainerName);
            _storageFactory.InitializeForWriting();

            const string name = "test";

            return _storageFactory.GetOrCreateStream(name);
        }

        protected override void FreeResources()
        {
            _storageFactory = null;
            _storageFactory = null;
        }

        protected override void TearDownEnvironment()
        {
            var cloudBlobClient = _cloudStorageAccount.CreateCloudBlobClient();
            var container = cloudBlobClient.GetContainerReference(ContainerName);

            if (container.Exists())
                container.Delete();
        }
    }
}
