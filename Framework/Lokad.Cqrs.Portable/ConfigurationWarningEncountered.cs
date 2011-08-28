#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;

namespace Lokad.Cqrs
{
    [Serializable]
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