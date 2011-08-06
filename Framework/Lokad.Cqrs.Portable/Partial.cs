using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Transactions;
using System.Linq;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs
{

    public sealed class HandlerComposer : HideObjectMembersFromIntelliSense
    {
        readonly IDictionary<Type, Action<Container, object>> _handler = new Dictionary<Type, Action<Container, object>>();
        readonly Func<TransactionScope> _optionalTxProvider;
        readonly Action<Container, object> _whenNotFound = (container, o) => { };

        public void Add<TMessage,TArg1,TArg2>(Action<TMessage,TArg1,TArg2> add)
        {
            _handler.Add(typeof(TMessage), (container, o) =>
                {
                    var a1 = container.Resolve<TArg1>();
                    var a2 = container.Resolve<TArg2>();
                    add((TMessage) o, a1, a2);
                });
        }

        public HandlerComposer(Func<TransactionScope> optionalTxProvider = null)
        {
            _optionalTxProvider = optionalTxProvider;
        }

        public void Add<TMessage>(Action<TMessage> add)
        {
            _handler.Add(typeof(TMessage), (container, o) => add((TMessage)o));
        }

        public void Add<TMessage,TArg>(Action<TMessage,TArg> add)
        {
            _handler.Add(typeof(TMessage), (container, o) =>
                {
                    var a1 = container.Resolve<TArg>();
                    add((TMessage) o, a1);
                });
        }

        public Action<ImmutableEnvelope> BuildHandler(Container container)
        {
            return envelope => Execute(container, envelope);
        }
        public HandlerFactory BuildFactory()
        {
            return container => (envelope => Execute(container, envelope));
        }

        public static HandlerFactory Empty = container => (envelope => { }); 

        void Execute(Container container, ImmutableEnvelope envelope)
        {
            if (_optionalTxProvider == null)
            {
                ExecuteInner(container, envelope);
                return;
            }
            using (var scope = _optionalTxProvider())
            {
                ExecuteInner(container, envelope);
                scope.Complete();
            }
        }

        void ExecuteInner(Container container, ImmutableEnvelope envelope)
        {
            foreach (var item in envelope.Items)
            {
                Action<Container, object> action;
                if (!_handler.TryGetValue(item.MappedType, out action))
                {
                    _whenNotFound(container, item);
                }
                else
                {
                    action(container, item.Content);
                }
            }
        }
    }
}