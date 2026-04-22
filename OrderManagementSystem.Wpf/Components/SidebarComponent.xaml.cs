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

namespace OrderManagementSystem.Wpf.Components
{
    /// <summary>
    /// Interaction logic for SidebarComponent.xaml
    /// </summary>
    public partial class SidebarComponent : UserControl
    {
        // Add Dependency Properties

        public static readonly DependencyProperty DashboardCommandProperty =
        DependencyProperty.Register(nameof(DashboardCommand), typeof(ICommand), typeof(SidebarComponent));

        public static readonly DependencyProperty CustomersCommandProperty =
            DependencyProperty.Register(nameof(CustomersCommand), typeof(ICommand), typeof(SidebarComponent));

        public static readonly DependencyProperty OrdersCommandProperty =
            DependencyProperty.Register(nameof(OrdersCommand), typeof(ICommand), typeof(SidebarComponent));

        public static readonly DependencyProperty ProductsCommandProperty =
            DependencyProperty.Register(nameof(ProductsCommand), typeof(ICommand), typeof(SidebarComponent));

        public ICommand DashboardCommand
        {
            get => (ICommand)GetValue(DashboardCommandProperty);
            set => SetValue(DashboardCommandProperty, value);
        }
        public ICommand CustomersCommand
        {
            get => (ICommand)GetValue(CustomersCommandProperty);
            set => SetValue(CustomersCommandProperty, value);
        }
        public ICommand OrdersCommand
        {
            get => (ICommand)GetValue(OrdersCommandProperty);
            set => SetValue(OrdersCommandProperty, value);
        }
        public ICommand ProductsCommand
        {
            get => (ICommand)GetValue(ProductsCommandProperty);
            set => SetValue(ProductsCommandProperty, value);
        }

        public SidebarComponent() => InitializeComponent();
        
    }
}
