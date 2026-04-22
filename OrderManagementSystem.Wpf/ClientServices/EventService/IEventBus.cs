using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.ClientServices.EvenService
{
    public interface IEventBus
    {
        public void Publish<T>(T @event);

        public void Subscribe<T>(Action<T> handler);
        public void Unsubscribe<T>(Action<T> handler);
    }
}
