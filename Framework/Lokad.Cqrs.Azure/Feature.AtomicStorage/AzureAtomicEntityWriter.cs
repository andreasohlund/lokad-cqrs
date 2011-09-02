#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Azure implementation of the view reader/writer
    /// </summary>
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    /// <typeparam name="TKey">the type of the key</typeparam>
    public sealed class AzureAtomicEntityWriter<TKey, TEntity> :
        IAtomicEntityWriter<TKey, TEntity>
        //where TEntity : IAtomicEntity<TKey>
    {
        readonly CloudBlobContainer _entityContainer;
        readonly CloudBlobContainer _singletonContainer;
        readonly IAtomicStorageStrategy _strategy;

        public AzureAtomicEntityWriter(IAzureStorageConfig storage, IAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
            var client = storage.CreateBlobClient();
            _entityContainer = client.GetContainerReference(strategy.GetFolderForEntity(typeof(TEntity)));
            _singletonContainer = client.GetContainerReference(strategy.GetFolderForSingleton());
        }

        public TEntity AddOrUpdate(TKey key, Func<TEntity> addViewFactory, Func<TEntity, TEntity> updateViewFactory,
            AddOrUpdateHint hint)
        {
            // TODO: implement proper locking and order
            var blob = GetBlobReference(key);
            TEntity view;
            try
            {
                // atomic entities should be small, so we can use the simple method
                var bytes = blob.DownloadByteArray();
                using (var stream = new MemoryStream(bytes))
                {
                    view = _strategy.Deserialize<TEntity>(stream);
                }

                view = updateViewFactory(view);
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        var s = string.Format(
                            "Container '{0}' does not exist. You need to initialize this atomic storage and ensure that '{1}' is known to '{2}'.",
                            blob.Container.Name, typeof(TEntity).Name, _strategy.GetType().Name);
                        throw new InvalidOperationException(s, ex);
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ResourceNotFound:
                        view = addViewFactory();
                        break;
                    default:
                        throw;
                }
            }
            // atomic entities should be small, so we can use the simple method
            // http://toolheaven.net/post/Azure-and-blob-write-performance.aspx
            using (var memory = new MemoryStream())
            {
                _strategy.Serialize(view, memory);
                // note that upload from stream does weird things
                blob.UploadByteArray(memory.ToArray());
            }
            return view;
        }


        public bool TryDelete(TKey key)
        {
            var blob = GetBlobReference(key);
            return blob.DeleteIfExists();
        }

        CloudBlob GetBlobReference(TKey key)
        {
            if (typeof(TKey) == typeof(unit))
            {
                return _singletonContainer.GetBlobReference(_strategy.GetNameForSingleton(typeof(TEntity)));
            }
            return _entityContainer.GetBlobReference(_strategy.GetNameForEntity(typeof(TEntity), key));
        }
    }
}