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
    /// Interaction logic for CustomDialogViewComponent.xaml
    /// </summary>
    public partial class CustomDialogViewComponent : UserControl
    {
        public CustomDialogViewComponent()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (CancelBtn.IsVisible)
            {
                CancelBtn.Focus();
            }
        }
    }
}
