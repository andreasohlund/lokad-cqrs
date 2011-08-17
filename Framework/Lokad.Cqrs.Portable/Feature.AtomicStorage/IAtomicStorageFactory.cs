using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAtomicStorageFactory 
    {
        IAtomicEntityWriter<TKey,TEntity> GetEntityWriter<TKey,TEntity>();
        IAtomicEntityReader<TKey,TEntity> GetEntityReader<TKey,TEntity>();

        /// <summary>
        /// Call this once on start-up to initialize folders
        /// </summary>
        IEnumerable<string> Initialize();
    }
}