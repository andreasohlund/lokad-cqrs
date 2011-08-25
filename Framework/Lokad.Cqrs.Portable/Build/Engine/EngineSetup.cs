using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class EngineSetup : IDisposable
    {
        readonly List<IEngineProcess> _processes;

        public void AddProcess(IEngineProcess process)
        {
            _processes.Add(process);
        }

        public IEnumerable<IEngineProcess> GetProcesses()
        {
            return _processes.AsReadOnly();
        }

        public EngineSetup()
        {
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