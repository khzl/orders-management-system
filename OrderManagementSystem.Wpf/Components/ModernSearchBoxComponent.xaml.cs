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
    /// Interaction logic for ModernSearchBoxComponent.xaml
    /// </summary>
    public partial class ModernSearchBoxComponent : UserControl
    {
        public ModernSearchBoxComponent()
        {
            InitializeComponent();
        }

        // Dependency Property للـ Text المربوط بالـ ViewModel
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), 
                typeof(ModernSearchBoxComponent),
                new FrameworkPropertyMetadata(string.Empty, 
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        // Dependency Property لنص التلميح (Placeholder/Tag)
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), 
                typeof(ModernSearchBoxComponent),
                new PropertyMetadata("Type to search..."));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

    }
}
