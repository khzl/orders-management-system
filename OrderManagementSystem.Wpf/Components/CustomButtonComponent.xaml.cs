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
    /// Interaction logic for CustomButtonComponent.xaml
    /// </summary>
    public partial class CustomButtonComponent : UserControl
    {
        public CustomButtonComponent()
        {
            InitializeComponent();
        }

        // --- Text Property ---
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), 
                typeof(string), typeof(CustomButtonComponent), new PropertyMetadata("Button"));

        // --- Icon Property ---
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), 
                typeof(string), typeof(CustomButtonComponent), new PropertyMetadata(string.Empty));

        // --- Command Property (أهم شي للزر) ---
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command),
                typeof(ICommand), typeof(CustomButtonComponent), new PropertyMetadata(null));

        // --- Command Parameter ---
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), 
                typeof(object), typeof(CustomButtonComponent), new PropertyMetadata(null));
    }
}
