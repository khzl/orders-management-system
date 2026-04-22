using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.ClientServices.EvenService
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();

            _handlers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handlers))
                handlers.Remove(handler);
        }

        public void Publish<T>(T @event)
        {
            if (!_handlers.TryGetValue(typeof(T), out var handlers))
                return;

            // نسخ القائمة لتفادي مشكلة التعديل أثناء الـ iteration
            foreach (var handler in handlers.ToList().Cast<Action<T>>())
                handler(@event);
        }

    }
}
