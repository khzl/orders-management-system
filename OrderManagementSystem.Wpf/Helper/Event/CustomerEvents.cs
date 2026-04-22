using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.Helper.Event
{
    public class CustomerEvents
    {
        public class CustomerCreatedEvent
        {
            // Property
            public int CustomerId { get; init; }

            // Constructor 
            public CustomerCreatedEvent(int customerId) => CustomerId = customerId;
        }

        public class CustomerUpdatedEvent
        {
            // Property
            public int CustomerId { get; init; }

            // Constructor 
            public CustomerUpdatedEvent(int customerId) => CustomerId = customerId;
        }

        public class CustomerDeletedEvent
        {
            // Property
            public int CustomerId { get; init; }

            // Constructor
            public CustomerDeletedEvent(int customerId) => CustomerId = customerId;
        }

        public class CustomerDeletedAllEvent
        {

        }
    }
}
