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
    /// Interaction logic for AlertNotificationComponent.xaml
    /// </summary>
    public partial class AlertNotificationComponent : UserControl
    {
        public AlertNotificationComponent()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), 
                typeof(AlertNotificationComponent),
                new PropertyMetadata(string.Empty));


        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

    }
}
