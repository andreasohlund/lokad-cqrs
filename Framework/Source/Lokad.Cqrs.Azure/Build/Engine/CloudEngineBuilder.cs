#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Consume.Build;
using Lokad.Cqrs.Domain;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Scheduled.Build;
using Lokad.Cqrs.Sender;
using Lokad.Cqrs.Transport;
// ReSharper disable UnusedMethodReturnValue.Global
namespace Lokad.Cqrs
{
	

	/// <summary>
	/// Fluent API for creating and configuring <see cref="CloudEngineHost"/>
	/// </summary>
	public class CloudEngineBuilder : Syntax
	{

		public AutofacBuilderForLogging Logging { get { return new AutofacBuilderForLogging(Builder); } }
		public AutofacBuilderForSerialization Serialization { get { return new AutofacBuilderForSerialization(Builder);} }
		public AutofacBuilderForAzure Azure { get { return new AutofacBuilderForAzure(Builder);}}

		public CloudEngineBuilder()
		{
			// System presets
			Logging.LogToTrace();
			Serialization.UseDataContractSerializer();

			// Azure presets
			Azure.UseDevelopmentStorageAccount();
			Builder.RegisterType<AzureQueueFactory>().As<AzureQueueFactory, AzureQueueFactory>().SingleInstance();
			Builder.RegisterType<ConsumingProcess>();

			// some defaults
			Builder.RegisterType<CloudEngineHost>().SingleInstance();
		}
	
		/// <summary>
		/// Adds Message Handling Feature to the instance of <see cref="CloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder AddMessageHandler(Action<HandleMessagesModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Adds Task Scheduling Feature to the instance of <see cref="CloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder AddScheduler(Action<ScheduledModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="CloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder DomainIs(Action<DomainBuildModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Creates default message sender for the instance of <see cref="CloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder AddMessageClient(Action<SenderModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Builds this <see cref="CloudEngineHost"/>.
		/// </summary>
		/// <returns>new instance of cloud engine host</returns>
		public CloudEngineHost Build()
		{
			ILifetimeScope container = Builder.Build();
			return container.Resolve<CloudEngineHost>(TypedParameter.From(container));
		}
	}
}