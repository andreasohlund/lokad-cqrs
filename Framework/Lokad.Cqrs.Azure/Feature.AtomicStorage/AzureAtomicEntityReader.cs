#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Azure implementation of the view reader/writer
    /// </summary>
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    public sealed class AzureAtomicEntityReader<TKey, TEntity> :
        IAtomicEntityReader<TKey, TEntity>
        //where TEntity : IAtomicEntity<TKey>
    {
        readonly CloudBlobContainer _entityContainer;
        readonly IAtomicStorageStrategy _strategy;
        readonly CloudBlobContainer _singletonContainer;

        public AzureAtomicEntityReader(IAzureStorageConfig storage, IAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
            var client = storage.CreateBlobClient();
            _entityContainer = client.GetContainerReference(strategy.GetFolderForEntity(typeof (TEntity)));
            _singletonContainer = client.GetContainerReference(strategy.GetFolderForSingleton());
        }

        CloudBlob GetBlobReference(TKey key)
        {
            var container = typeof(TKey) == typeof(unit) ? _singletonContainer : _entityContainer;
            return container.GetBlobReference(_strategy.GetNameForEntity(typeof(TEntity), key));
        }

        public bool TryGet(TKey key, out TEntity entity)
        {
            var blob = GetBlobReference(key);
            try
            {
                // blob request options are cloned from the config
                using (var data = blob.OpenRead())
                {
                    entity = _strategy.Deserialize<TEntity>(data);
                    return true;
                }
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ResourceNotFound:
                        entity = default(TEntity);
                        return false;
                    default:
                        throw;
                }
            }
        }
    }
}