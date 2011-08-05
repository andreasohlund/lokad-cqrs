using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Funq;
using System.Linq;

namespace Lokad.Cqrs
{
    
    [ComVisible(true)]
  [Serializable]
  //[StructLayout(LayoutKind.Sequential, Size = 1)]
  //public struct unit
  //  {
  //      public static readonly unit it = default(unit);

  //      public static implicit operator unit(int value)
  //      {
  //          if (value != 0) throw new InvalidOperationException();
  //          return default(unit);
  //      }
  //  }

    public sealed class Handling
    {
        readonly IDictionary<Type, Action<Container, object>> _handler = new Dictionary<Type, Action<Container, object>>();

        public void Add<T1,T2,T3>(Action<T1,T2,T3> add)
        {
            _handler.Add(typeof(T1), (container, o) =>
                {
                    var a1 = container.Resolve<T2>();
                    var a2 = container.Resolve<T3>();
                    add((T1) o, a1, a2);
                });
        }

        public Action<Container,ImmutableEnvelope> BuildHandler()
        {
            return (container, envelope) => Execute(container, envelope, (container1, o) => { });
        }

        public static Func<Container, Action<ImmutableEnvelope>> Empty = container => (envelope => { }); 



        void Execute(Container container, ImmutableEnvelope envelope, Action<Container,object> whenNotFound)
        {
            foreach (var item in envelope.Items)
            {
                Action<Container, object> action;
                if (!_handler.TryGetValue(item.MappedType, out action))
                {
                    whenNotFound(container, item);
                    
                }
                else
                {
                    action(container, item.Content);
                }
            }
        }
    }
    
}