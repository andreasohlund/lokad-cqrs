#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Transactions;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
{
    public enum ContainerScopeLevel
    {
        Envelope,Item
    }

    public sealed class DispatchStrategy
    {
        readonly IContainerForHandlerClasses _scope;
        readonly Func<TransactionScope> _scopeFactory;
        readonly Func<Type, Type, MethodInfo> _hint;
        readonly IMethodContextManager _context;

        public DispatchStrategy(IContainerForHandlerClasses scope, Func<TransactionScope> scopeFactory,
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
            using (var outer = _scope.GetChildContainer(ContainerScopeLevel.Envelope))
            {
                foreach (var tuple in pairs)
                {
                    using (var inner = outer.GetChildContainer(ContainerScopeLevel.Item))
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