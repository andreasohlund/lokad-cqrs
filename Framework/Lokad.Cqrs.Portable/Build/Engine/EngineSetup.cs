using System;
using System.Collections.Generic;
using Lokad.Cqrs.Core.Reactive;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class EngineSetup : IDisposable
    {
        readonly List<IEngineProcess> _processes;

        public readonly SystemObserver Observer;

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