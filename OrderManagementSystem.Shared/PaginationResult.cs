using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Shared
{
    public class PaginationResult<T>
    {
        // return Data , Total Count , Total Pages , Current Page , Page Size 

        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
