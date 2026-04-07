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
    /// Interaction logic for CardComponent.xaml
    /// </summary>
    public partial class CardComponent : UserControl
    {
        public CardComponent()
        {
            InitializeComponent();
        }

        // Dependency Property for Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CardComponent), new PropertyMetadata("", (d, e) => {
                ((CardComponent)d).txtTitle.Text = e.NewValue.ToString();
            }));


        // Dependency Property for Value
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(CardComponent), new PropertyMetadata("", (d, e) => {
                ((CardComponent)d).txtValue.Text = e.NewValue.ToString();
            }));


    }
}
