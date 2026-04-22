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

        // --- Title & Value ---
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), 
                typeof(CardComponent), new PropertyMetadata("Title"));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value),
                typeof(string), typeof(CardComponent), new PropertyMetadata("0"));

        // --- Icon Property ---
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), 
                typeof(string), typeof(CardComponent), new PropertyMetadata("📊"));

        // --- Trend (Percentage) ---
        public string Trend
        {
            get => (string)GetValue(TrendProperty);
            set => SetValue(TrendProperty, value);
        }
        public static readonly DependencyProperty TrendProperty =
            DependencyProperty.Register(nameof(Trend),
                typeof(string), typeof(CardComponent), new PropertyMetadata("0%"));

        // --- Trend Brush (Color) ---
        public Brush TrendBrush
        {
            get => (Brush)GetValue(TrendBrushProperty);
            set => SetValue(TrendBrushProperty, value);
        }
        public static readonly DependencyProperty TrendBrushProperty =
            DependencyProperty.Register(nameof(TrendBrush), 
                typeof(Brush), typeof(CardComponent), new PropertyMetadata(Brushes.Green));

        // --- Description (Since last month) ---
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description),
                typeof(string), typeof(CardComponent), new PropertyMetadata("vs last month"));

    }
}
