using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EDA.Events;

namespace EDA
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }

            _subscribers[eventType].Add(handler);
        }

        public void Publish<T>(T @event) where T : IEvent
        {
            var eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    if (handler is Action<T> action)
                    {
                        action(@event);
                    }
                }
            }
        }
    }
}

