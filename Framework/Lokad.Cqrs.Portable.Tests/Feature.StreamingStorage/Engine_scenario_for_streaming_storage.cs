using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public sealed class Engine_scenario_for_streaming_storage : FiniteEngineScenario
    {
        [DataContract]
        public sealed class Do : Define.Command
        {
            [DataMember]
            public string Id;
        }
        [DataContract]
        public sealed class Finish : Define.Command {}

        static void Consume(Do message, IStreamingRoot root, IMessageSender sender)
        {


            if (string.IsNullOrEmpty(message.Id))
            {
                var container = root.GetContainer("test1");
                container.Create();
                container.GetItem("data").Write(s =>
                {
                    using (var writer = new StreamWriter(s, Encoding.UTF8))
                    {
                        writer.Write(new string('*', 50000));
                    }
                });

                sender.SendOne(new Do { Id = "test1" });
                return;
            }
            string result = null;
            root.GetContainer(message.Id).GetItem("data").ReadInto((props, stream) =>
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            });
            Assert.AreEqual(new string('*', 50000), result);
            sender.SendOne(new Finish(), cb => cb.AddString("finish"));
        }



        protected override void Configure(CqrsEngineBuilder b)
        {
            b.Memory(m =>
                {
                    m.AddMemoryProcess("in", Lambda);
                    m.AddMemorySender("in");
                });
            EnlistMessage(new Do());
        }

        static Action<ImmutableEnvelope> Lambda(Container container)
        {
            var handler = new HandlerComposer();
            handler.Add<Do,IStreamingRoot,IMessageSender>(Consume);
            return handler.BuildHandler(container);
        }
    }
}