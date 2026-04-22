using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OrderManagementSystem.Wpf.Helper
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        // this method delete and add and send one notifications only in end 
        public void ReplaceRange(IEnumerable<T> items)
        {
            if (items == null)
                return;

            // stop wait for notifications 
            Items.Clear();

            foreach(var item in items)
            {
                Items.Add(item);
            }

            // send one notification only for UI and All Menu Reset 
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }
    }
}
