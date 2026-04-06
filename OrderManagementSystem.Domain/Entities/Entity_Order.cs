using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Domain.Entities
{
    public class Entity_Order
    {
        #region Properties
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        #endregion
    }
}
