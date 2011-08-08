#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System.Runtime.Serialization;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class CreateAccount : ISesCommand
    {
        [DataMember] public readonly string Name;

        public CreateAccount(string name)
        {
            Name = name;
        }
    }
}