#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.DirectoryDispatch.Default;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class SmokeTests
    {
        // ReSharper disable InconsistentNaming


        static CqrsEngineHost BuildHost()
        {
            var builder = new CqrsEngineBuilder();
            var config = AzureStorage.CreateConfigurationForDev();
            builder.Azure(x => x.AddAzureProcess(config, "process-vip", DoSomething));
            builder.Memory(x =>
                {
                    x.AddMemoryProcess("process-all", DoSomething);
                    x.AddMemoryRouter("inbox", e =>
                        {
                            var isVip = e.Items.Any(i => i.MappedType == typeof (VipMessage));
                            return isVip ? "azure-dev:process-vip" : "memory:process-all";
                        });
                    x.AddMemorySender("inbox", cm => cm.IdGeneratorForTests());
                });


            return builder.Build();
        }


        [DataContract]
        public sealed class VipMessage : IMessage
        {
            [DataMember(Order = 1)]
            public string Word { get; set; }
        }

        [DataContract]
        public sealed class UsualMessage : IMessage
        {
            [DataMember(Order = 1)]
            public string Word { get; set; }
        }

        static Action<ImmutableEnvelope> DoSomething(Container container)
        {
            var sender = container.Resolve<IMessageSender>();
            return envelope =>
                {
                    Dump(envelope);
                    sender.SendOne(envelope.Items[0].Content);
                };
        }

        static void Dump(ImmutableEnvelope envelope)
        {
            foreach (var item in envelope.Items)
            {
                var vip = item.Content as VipMessage;

                if (vip != null)
                {
                    Print(vip.Word, envelope.EnvelopeId);
                    return;
                }
                var usual = item.Content as UsualMessage;

                if (usual != null)
                {
                    Print(usual.Word, envelope.EnvelopeId);
                }
            }
        }

        static void Print(string value, string id)
        {
            if (value.Length > 20)
            {
                Trace.WriteLine(string.Format("[{0}]: {1}... ({2})", id, value.Substring(0, 16),
                    value.Length));
            }
            else
            {
                Trace.WriteLine(string.Format("[{0}]: {1}", value, id));
            }
        }

        

        [Test, Explicit]
        public void Test()
        {
            using (var host = BuildHost())
            {
                var client = host.Resolve<IMessageSender>();

                client.SendOne(new VipMessage {Word = "VIP1 Message"});
                client.SendOne(new UsualMessage {Word = "Usual Large:" + new string(')', 9000)});


                var vipMessage = new VipMessage
                    {
                        Word = "VIP Delayed Large :" + new string(')', 9000)
                    };
                client.SendOne(vipMessage, cb => cb.DelayBy(TimeSpan.FromSeconds(3)));

                client.SendOne(new UsualMessage {Word = "Usual Delayed"}, cb => cb.DelayBy(TimeSpan.FromSeconds(2)));

                //client.SendBatch(new VipMessage { Word = " VIP with usual "}, new UsualMessage() { Word = "Vip with usual"});

                using (var cts = new CancellationTokenSource())
                {
                    var task = host.Start(cts.Token);
                    Thread.Sleep(TimeSpan.FromSeconds(10));


                    cts.Cancel(true);
                    task.Wait(TimeSpan.FromSeconds(5));
                }
                // second run
                using (var cts = new CancellationTokenSource())
                {
                    var task = host.Start(cts.Token);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    cts.Cancel(true);
                    task.Wait(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}