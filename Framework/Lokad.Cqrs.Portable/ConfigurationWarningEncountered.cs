using System;

namespace Lokad.Cqrs
{
    public sealed class ConfigurationWarningEncountered : ISystemEvent
    {
        public readonly string Message;
        public readonly Exception OptionalException;
        public ConfigurationWarningEncountered(string message, Exception optionalException = null)
        {
            Message = message;
            OptionalException = optionalException;
        }

        public override string ToString()
        {
            var s = "Configuration warning: " + Message;
            if (OptionalException != null)
            {
                s += " Ex: " + OptionalException.Message;
            }
            return s;
        }
    }
}