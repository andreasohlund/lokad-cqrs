#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public class BasicConfigurationTests
    {
        [Test]
        public void CantBeConfiguredWithHandlerSample()
        {
            var builder = new CqrsEngineBuilder();
            builder.MessagesWithHandlersFromStructureMap(
                c => c.HandlerSample<Define.Handle<Define.Command>>(h => h.Handle(null)));

            using (builder.Build()) {}
        }
    }
}