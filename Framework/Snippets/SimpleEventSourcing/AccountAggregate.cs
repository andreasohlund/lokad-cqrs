using System;
using Lokad.Cqrs.Scenarios.SimpleES;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;
using Snippets.SimpleEventSourcing.Contracts;

namespace Snippets.SimpleEventSourcing
{
    public sealed class AccountAggregate
    {
        Action<ISesEvent> _observer;
        AccountAggregateState _state;

        public AccountAggregate(Action<ISesEvent> observer, AccountAggregateState state)
        {
            _observer = observer;
            _state = state;
        }

        public void When(AddLogin c)
        {
            Apply(new LoginAdded(c.Username, c.Password));
        }

        public void When(CreateAccount c)
        {
            Apply(new AccountCreated(c.Name));
        }

        

        public void Execute(ISesCommand c)
        {
            RedirectToWhen.InvokeCommand(this, c);
        }

        void Apply(ISesEvent e)
        {
            _state.Apply(e);
            _observer(e);
        }
    }
}