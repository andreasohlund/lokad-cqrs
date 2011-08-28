#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Reactive;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class EngineSetup : HideObjectMembersFromIntelliSense, IDisposable
    {
        readonly List<IEngineProcess> _processes;

        public readonly SystemObserver Observer;
        public readonly QueueWriterRegistry Registry;

        public void AddProcess(IEngineProcess process)
        {
            _processes.Add(process);
        }

        public IEnumerable<IEngineProcess> GetProcesses()
        {
            return _processes.AsReadOnly();
        }

        public EngineSetup(SystemObserver observer)
        {
            Observer = observer;
            Registry = new QueueWriterRegistry();
            _processes = new List<IEngineProcess>();
        }

        public void Dispose()
        {
            foreach (var process in _processes)
            {
                process.Dispose();
            }
        }
    }
}