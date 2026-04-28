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
    /// Interaction logic for PaginationComponent.xaml
    /// </summary>
    public partial class PaginationComponent : UserControl
    {
        public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register("CurrentPage", typeof(int), typeof(PaginationComponent), new PropertyMetadata(1));

        public static readonly DependencyProperty TotalPagesProperty =
            DependencyProperty.Register("TotalPages", typeof(int), typeof(PaginationComponent), new PropertyMetadata(1));

        public static readonly DependencyProperty NextCommandProperty =
            DependencyProperty.Register("NextCommand", typeof(ICommand), typeof(PaginationComponent));

        public static readonly DependencyProperty PrevCommandProperty =
            DependencyProperty.Register("PrevCommand", typeof(ICommand), typeof(PaginationComponent));

        public int CurrentPage 
        { 
            get => (int)GetValue(CurrentPageProperty); 
            set => SetValue(CurrentPageProperty, value);
        }
        public int TotalPages 
        { 
            get => (int)GetValue(TotalPagesProperty);
            set => SetValue(TotalPagesProperty, value); 
        }
        public ICommand NextCommand 
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value); 
        }
        public ICommand PrevCommand 
        { 
            get => (ICommand)GetValue(PrevCommandProperty); 
            set => SetValue(PrevCommandProperty, value);
        }

        public bool CanGoBack => CurrentPage > 1;
        public bool CanGoForward => CurrentPage < TotalPages;

        public PaginationComponent() => InitializeComponent();
    }
}
