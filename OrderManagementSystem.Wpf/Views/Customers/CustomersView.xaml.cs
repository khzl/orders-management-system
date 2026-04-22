using OrderManagementSystem.Wpf.ViewModels.Customers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OrderManagementSystem.Wpf.Commands;

namespace OrderManagementSystem.Wpf.Views.Customers
{
    /// <summary>
    /// Interaction logic for CustomersView.xaml
    /// </summary>
    public partial class CustomersView : UserControl
    {
        public CustomersView()
        {
            InitializeComponent();
            // link event loaded here 
            this.Loaded += CustomersView_Loaded;
        }

        // Event Handler 
        private void CustomersView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustomersViewModel customersViewModel)
            {
                // Check the command itself for CanExecute and then Execute
                if (customersViewModel.LoadCommand?.CanExecute(null) == true)
                {
                    customersViewModel.LoadCommand.Execute(null);
                }
            }
        }

    }
}
