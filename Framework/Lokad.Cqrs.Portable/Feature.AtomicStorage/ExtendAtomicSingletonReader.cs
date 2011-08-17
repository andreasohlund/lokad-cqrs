#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendAtomicSingletonReader
    {
        public static TView GetOrNew<TView>(this IAtomicEntityReader<unit,TView> reader)
            where TView : new()
        {
            TView view;
            if (reader.TryGet(unit.it,out view))
            {
                return view;
            }
            return new TView();
        }

        public static Optional<TSingleton> Get<TSingleton>(this IAtomicEntityReader<unit,TSingleton> reader)
        {
            TSingleton singleton;
            if (reader.TryGet(unit.it,out singleton))
            {
                return singleton;
            }
            return Optional<TSingleton>.Empty;
        }
    }
}