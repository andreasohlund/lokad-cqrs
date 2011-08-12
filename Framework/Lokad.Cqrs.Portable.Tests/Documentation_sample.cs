#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class Documentation_sample
    {
        [DataContract]
        public sealed class CreateCustomer : Define.Command
        {
            [DataMember] public string CustomerName;
            [DataMember] public int CustomerId;
        }


        [DataContract]
        public sealed class CustomerCreated : Define.Event
        {
            [DataMember] public int CustomerId;
            [DataMember] public string CustomerName;
        }

        [DataContract]
        public sealed class Customer
        {
            [DataMember] public string Name;
            [DataMember] public int Id;

            public Customer(int id, string name)
            {
                Name = name;
                Id = id;
            }
        }

        [Test, Explicit]
        public void Run_in_test()
        {
            var builder = new CqrsEngineBuilder();

            


            builder.Memory(m =>
                {
                    m.AddMemorySender("work");
                    m.AddMemoryProcess("work", Lambda);
                });
            builder.Storage(m => m.AtomicIsInMemory());

            using (var engine = builder.Build())
            {
                //engine.Resolve<IMessageSender>().SendOne(new CreateCustomer()
                //{
                //    CustomerId = 1,
                //    CustomerName = "Rinat Abdullin"
                //});
                engine.RunForever();
            }
        }

        Action<ImmutableEnvelope> Lambda(Container container)
        {
            var handling = new HandlerComposer();
            handling.Add<CreateCustomer,NuclearStorage,IMessageSender>(Consume);
            return handling.BuildFactory()(container);
        }

        static void Consume(CreateCustomer cmd, NuclearStorage storage, IMessageSender sender)
        {
            var customer = new Customer(cmd.CustomerId, cmd.CustomerName);
            storage.AddEntity(customer.Id, customer);
            sender.SendOne(new CustomerCreated
                {
                    CustomerId = cmd.CustomerId,
                    CustomerName = cmd.CustomerName
                });
        }
    }
}