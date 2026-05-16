using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        // 1. CurrentPage
        public static readonly DependencyProperty CurrentPageProperty =
           DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationComponent),
           new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // 2. TotalPages
        public static readonly DependencyProperty TotalPagesProperty =
           DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationComponent),
           new PropertyMetadata(1));

        // 3. NextCommand
        public static readonly DependencyProperty NextCommandProperty =
            DependencyProperty.Register(nameof(NextCommand), typeof(ICommand), typeof(PaginationComponent));

        // 4. PrevCommand
        public static readonly DependencyProperty PrevCommandProperty =
            DependencyProperty.Register(nameof(PrevCommand), typeof(ICommand), typeof(PaginationComponent));

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

        public PaginationComponent() => InitializeComponent();
    }
}
