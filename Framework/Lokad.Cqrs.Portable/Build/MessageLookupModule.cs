using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Feature.DirectoryDispatch.Default;

namespace Lokad.Cqrs.Build
{
    public sealed class MessageLookupModule : HideObjectMembersFromIntelliSense
    {
        readonly List<Assembly> _assemblies;
        readonly IList<Predicate<Type>> _constraints;


        bool _constraintsDirty;
        public MessageLookupModule()
        {
            _constraints = new List<Predicate<Type>>
                {
                    type => false == type.IsAbstract
                };
            _assemblies = new List<Assembly>();
        }


        /// <summary>
        /// Includes assemblies of the specified types into the discovery process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public void InAssemblyOf<T>()
        {
            _assemblies.Add(typeof(T).Assembly);
        }

        public void InAssemblyOf(object instance)
        {
            _assemblies.Add(instance.GetType().Assembly);
        }

        public void WhereMessages(Predicate<Type> constraint)
        {
            _constraints.Add(constraint);
            _constraintsDirty = true;
        }

        public IEnumerable<Type> LookupMessages()
        {
            if (_assemblies.Count == 0)
            {
                _assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(AssemblyScanEvil.IsProbablyUserAssembly)); 
            }
            if (!_constraintsDirty)
            {
                _constraints.Add(t => typeof(IMessage).IsAssignableFrom(t));
            }
            return _assemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => _constraints.All(predicate => predicate(t)));
        }
    }
}