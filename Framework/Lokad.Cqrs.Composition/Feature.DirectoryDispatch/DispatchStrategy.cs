#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Transactions;
using Autofac;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
{
    public interface INestedResolver : IDisposable
    {
        INestedResolver GetChildContainer(object tag);
        object Resolve(Type type);
    }
    public sealed class NestedContainer : INestedResolver
    {
        readonly ILifetimeScope _scope;

        public NestedContainer(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public INestedResolver GetChildContainer(object tag)
        {
            return new NestedContainer(_scope.BeginLifetimeScope(tag));
        }

        public object Resolve(Type serviceType)
        {
            return _scope.Resolve(serviceType);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }

    public sealed class DispatchStrategy
    {
        readonly INestedResolver _scope;
        readonly Func<TransactionScope> _scopeFactory;
        readonly Func<Type, Type, MethodInfo> _hint;
        readonly IMethodContextManager _context;

        public DispatchStrategy(INestedResolver scope, Func<TransactionScope> scopeFactory,
            Func<Type, Type, MethodInfo> hint, IMethodContextManager context)
        {
            _scope = scope;
            _scopeFactory = scopeFactory;
            _hint = hint;
            _context = context;
        }

        public void Dispatch(ImmutableEnvelope envelope, IEnumerable<Tuple<Type, ImmutableMessage>> pairs)
        {
            using (var tx = _scopeFactory())
            using (var outer = _scope.GetChildContainer(DispatchLifetimeScopeTags.MessageEnvelopeScopeTag))
            {
                foreach (var tuple in pairs)
                {
                    using (var inner = outer.GetChildContainer(DispatchLifetimeScopeTags.MessageItemScopeTag))
                    {
                        var handlerType = tuple.Item1;
                        var instance = inner.Resolve(handlerType);
                        var message = tuple.Item2;
                        var consume = _hint(handlerType, message.MappedType);
                        _context.SetContext(envelope, message);
                        try
                        {
                            consume.Invoke(instance, new[] {message.Content});
                        }
                        catch (TargetInvocationException e)
                        {
                            throw InvocationUtil.Inner(e);
                        }
                        finally
                        {
                            _context.ClearContext();
                        }
                    }
                }
                tx.Complete();
            }
        }
    }
}