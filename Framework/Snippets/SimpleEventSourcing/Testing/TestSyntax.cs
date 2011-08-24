using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Text;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Testing
{
    public abstract class AggregateTestSyntax<TAggregate>
        where TAggregate : IAggregate
    
    {
        readonly List<ISesEvent> _given = new List<ISesEvent>();
        readonly List<ISesCommand> _when = new List<ISesCommand>();
        readonly List<ISesEvent> _actual = new List<ISesEvent>();
        readonly List<Exception> _exceptions = new List<Exception>();


        [SetUp]
        public void Setup()
        {
            _given.Clear();
            _when.Clear();
            _actual.Clear();
            _exceptions.Clear();
        }

        protected abstract TAggregate BuildAggregate(IEnumerable<ISesEvent> events, Action<ISesEvent> observer);

        protected void Given(params ISesEvent[] events)
        {
            _given.AddRange(events);
        }

        protected void When(params ISesCommand[] commands)
        {
            var tx = new List<ISesEvent>();
            var ar = BuildAggregate(_given, tx.Add);
            try
            {
                foreach (var command in commands)
                {
                    ar.Execute(command);
                }
                // commit tx
                _actual.AddRange(tx);
            }
            catch (Exception ex)
            {
                _exceptions.Add(ex);
            }
        }

        static IEnumerable<string> Print(IEnumerable<ISesEvent> c)
        {
            return c.Select(e =>
            {
                var type = e.GetType();
                var s = JsonSerializer.SerializeToString(e, type);
                return type.Name + ": " + s;
            }).ToArray();
        }

        protected void Then(params ISesEvent[] events)
        {
            var actual = Print(_actual);
            var expected = Print(events);
            CollectionAssert.AreEqual(expected, actual);
        }
        protected void Then(params Type[] expectedTypes)
        {
            var actualTypes = _exceptions.Any() ? _exceptions.Select(t => t.GetType()).ToArray() :  _actual.Select(t => t.GetType()).ToArray();
            CollectionAssert.AreEqual(expectedTypes, actualTypes);
        }

        protected DateTime Utc(int year, int month = 1, int day = 1, int hour = 0, int minute = 0)
        {
            return new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Utc);
        }
        protected DateTime Time(int hour, int minute = 0, int second = 0)
        {
            return new DateTime(2011, 1, 1, hour, minute, second, DateTimeKind.Utc);
        }
    }
}