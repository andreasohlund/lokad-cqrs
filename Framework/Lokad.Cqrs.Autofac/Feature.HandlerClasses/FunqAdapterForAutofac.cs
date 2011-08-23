using System;
using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Container = Lokad.Cqrs.Core.Container;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Resolves Autofac dependencies from the provided Funq <see cref="Container"/>
    /// </summary>
    public sealed class FunqAdapterForAutofac : IRegistrationSource
    {
        readonly Container _container;
        public FunqAdapterForAutofac(Container container)
        {
            _container = container;
        }

        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>
        /// Registrations providing the service.
        /// </returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var type = service as TypedService;

            if (type == null)
            {
                yield break;
            }
            
            var result = typeof(Container)
                .GetMethod("TryResolve", new Type[0])
                .MakeGenericMethod(type.ServiceType)
                .Invoke(_container, null);

            if (null == result)
                yield break;

            yield return new ComponentRegistration(Guid.NewGuid(), new ProvidedInstanceActivator(result),new RootScopeLifetime(), InstanceSharing.Shared, InstanceOwnership.ExternallyOwned, new[] {service}, new Dictionary<string,object>());
        }

        /// <summary>
        /// Gets whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (I.e. like Meta, Func or Owned.)
        /// </summary>
        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }
    }
}