using OrderManagementSystem.Dtos.Customers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for CustomerFormComponent.xaml
    /// </summary>
    public partial class CustomerFormComponent : UserControl
    {
        public CustomerFormComponent()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CustomerNameProperty =
            DependencyProperty.Register(nameof(CustomerName), 
                typeof(string), typeof(CustomerFormComponent), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? CustomerName
        {
            get => (string?)GetValue(CustomerNameProperty);
            set => SetValue(CustomerNameProperty, value);
        }

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register(nameof(Email), 
                typeof(string), typeof(CustomerFormComponent), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? Email
        {
            get => (string?)GetValue(EmailProperty);
            set => SetValue(EmailProperty, value);
        }

        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register(nameof(Address), 
                typeof(string), typeof(CustomerFormComponent), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? Address
        {
            get => (string?)GetValue(AddressProperty);
            set => SetValue(AddressProperty, value);
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register(nameof(ErrorMessage), 
                typeof(string), typeof(CustomerFormComponent),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? ErrorMessage
        {
            get => (string?)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), 
                typeof(bool), typeof(CustomerFormComponent), 
                new PropertyMetadata(false));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty CustomerPhonesProperty =
            DependencyProperty.Register(nameof(CustomerPhones),
                typeof(ObservableCollection<CustomerPhoneDto>), typeof(CustomerFormComponent),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ObservableCollection<CustomerPhoneDto> CustomerPhones
        {
            get => (ObservableCollection<CustomerPhoneDto>)GetValue(CustomerPhonesProperty);
            set => SetValue(CustomerPhonesProperty, value);
        }

        // Commands
        public static readonly DependencyProperty AddPhoneCommandProperty =
            DependencyProperty.Register(nameof(AddPhoneCommand), typeof(ICommand), 
                typeof(CustomerFormComponent), new PropertyMetadata(null));

        public ICommand AddPhoneCommand
        {
            get => (ICommand)GetValue(AddPhoneCommandProperty);
            set => SetValue(AddPhoneCommandProperty, value);
        }

        public static readonly DependencyProperty RemovePhoneCommandProperty =
            DependencyProperty.Register(nameof(RemovePhoneCommand), typeof(ICommand),
                typeof(CustomerFormComponent), new PropertyMetadata(null));

        public ICommand RemovePhoneCommand
        {
            get => (ICommand)GetValue(RemovePhoneCommandProperty);
            set => SetValue(RemovePhoneCommandProperty, value);
        }

        public static readonly DependencyProperty UpdatePhoneCommandProperty =
            DependencyProperty.Register(nameof(UpdatePhoneCommand), typeof(ICommand),
                typeof(CustomerFormComponent), new PropertyMetadata(null));

        public ICommand UpdatePhoneCommand
        {
            get => (ICommand)GetValue(UpdatePhoneCommandProperty);
            set => SetValue(UpdatePhoneCommandProperty, value);
        }

        public static readonly DependencyProperty SetPrimaryCommandProperty =
            DependencyProperty.Register(nameof(SetPrimaryCommand), typeof(ICommand), 
                typeof(CustomerFormComponent), new PropertyMetadata(null));

        public ICommand SetPrimaryCommand
        {
            get => (ICommand)GetValue(SetPrimaryCommandProperty);
            set => SetValue(SetPrimaryCommandProperty, value);
        }

    }
}
