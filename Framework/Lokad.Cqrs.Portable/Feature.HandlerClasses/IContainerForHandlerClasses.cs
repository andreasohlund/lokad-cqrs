using System;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Allows plugging in external containers to handle constructor and property injection
    /// of the message handlers. It manages lifetime and sharing of the components as needed
    /// </summary>
    public interface IContainerForHandlerClasses : IDisposable
    {
        /// <summary>
        /// Gets the child container.
        /// </summary>
        /// <param name="level">The hierarchy level (used for tagging in Autofac).</param>
        /// <returns></returns>
        IContainerForHandlerClasses GetChildContainer(ContainerScopeLevel level);
        /// <summary>
        /// Tries to get the handler instance, given it's service type, throwing exception in case of failure
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>handler instance</returns>
        object ResolveHandlerByServiceType(Type serviceType);
    }
}