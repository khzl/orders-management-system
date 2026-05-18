using OrderManagementSystem.Wpf.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        // Helper Class To Represent Each Page Number In UI
        public class PageItem
        {
            public string? DisplayText { get; set; }
            public int Number { get; set; }
            public bool IsActive { get; set; }
            public bool IsEllipsis { get; set; }
        }

        // Group Number will be Showed In UI , For Example: 1,2,3,4,5
        public ObservableCollection<PageItem> VisiblePages { get; } = new(); // ReadOnly Collection To Bind With UI

        // 1. خاصية الصفحة الحالية - تدعم الربط ثنائي الاتجاه افتراضياً لتبادل التحديثات
        public static readonly DependencyProperty CurrentPageProperty =
           DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationComponent),
           new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPageDataChanged));

        // 2. خاصية إجمالي الصفحات
        public static readonly DependencyProperty TotalPagesProperty =
           DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationComponent),
           new PropertyMetadata(1, OnPageDataChanged));


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

        // Internal Commands to Binding in XAML
        // ReadOnly Properties 
        public ICommand? PrevCommand { get; }
        public ICommand? NextCommand { get; }
        public ICommand? SelectPageCommand { get; }

        public PaginationComponent()
        {
            // initialize Commands with Conditional Enables them
            // 1. نجهز الأوامر أولاً في الذاكرة قبل أن تقرأها الواجهة
            PrevCommand = new RelayCommand(_ => MovePrev(), _ => CurrentPage > 1);

            NextCommand = new RelayCommand(_ => MoveNext(), _ => CurrentPage < TotalPages);

            SelectPageCommand = new RelayCommand(parameter => ChoosePage(parameter));

            // 2. الآن نقوم ببناء الواجهة والـ Binding ليلتقط الأوامر الجاهزة فوراً
            InitializeComponent();

            this.Loaded += (s,e) => UpdateVisiblePages(); // تحديث الأرقام عند إنشاء المكون
        }


        // استدعاء التحديث فور تغير الصفحة الحالية أو الإجمالي
        private static void OnPageDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PaginationComponent control)
            {
                control.UpdateVisiblePages();
                // إجبار WPF على إعادة فحص شروط التفعيل (CanExecute) للأزرار فوراً
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Logic Navigation Methods For Arrows Buttons
        private void MovePrev() => CurrentPage--;

        private void MoveNext() => CurrentPage++;

        private void ChoosePage(object? parameter)
        {
            if (parameter is PageItem page && !page.IsEllipsis && page.Number != CurrentPage)
            {
                CurrentPage = page.Number;
            }
        }

        // توليد الصفحات ديناميكيا بحسب موضع الصفحة الحالية
        private void UpdateVisiblePages()
        {
            VisiblePages.Clear();

            if (TotalPages <= 0)
                return;

            // الحالة الاولى : اذا كان الاجمالي الصفحات 5 صفحات او اقل تظهر الارقام متتالية طبيعيا 
            if (TotalPages <= 5)
            {
                for (int index = 1; index <= TotalPages; index++)
                {
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = index.ToString(),
                        Number = index,
                        IsActive = (index == CurrentPage),
                        IsEllipsis = false
                    });
                }
            }
            // الحالة الثانية اذا كان اجمالي الصفحات اكبر من 5 (تطبيق لوجك النقاط المتقدم
            else
            {
                if (CurrentPage <= 3)
                {
                    // الموضع في البداية : يعرض (1,2,3,4,5,...,الصفحة الاخيرة
                    for (int index = 1; index <= 4; index++)
                    {
                        VisiblePages.Add(new PageItem
                        {
                            DisplayText = index.ToString(),
                            Number = index,
                            IsActive = (index == CurrentPage)
                        });
                    }
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = "...",
                        Number = 0,
                        IsEllipsis = true
                    });
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = TotalPages.ToString(),
                        Number = TotalPages
                    });
                }
                else if (CurrentPage >= TotalPages - 2)
                {
                    // الموضع في النهاية: يعرض (1، ...، إجمالي-3، إجمالي-2، إجمالي-1، إجمالي)
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = "1",
                        Number = 1
                    });
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = "...",
                        Number = 0,
                        IsEllipsis = true
                    });
                    for (int index = TotalPages - 3; index <= TotalPages; index++)
                    {
                        VisiblePages.Add(new PageItem
                        {
                            DisplayText = index.ToString(),
                            Number = index,
                            IsActive = (index == CurrentPage)
                        });
                    }
                }
                else
                {
                    // الموضع في المنتصف: يعرض (1، ...، الحالي-1، الحالي (نشط)، الحالي+1، ...، الصفحة الأخيرة)
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = "1",
                        Number = 1
                    });
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = "...",
                        Number = 0,
                        IsEllipsis = true
                    });

                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = (CurrentPage - 1).ToString(),
                        Number = CurrentPage - 1
                    });
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = CurrentPage.ToString(),
                        Number = CurrentPage,
                        IsActive = true
                    });
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = (CurrentPage + 1).ToString(),
                        Number = CurrentPage + 1
                    });

                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = "...",
                        Number = 0,
                        IsEllipsis = true
                    });
                    VisiblePages.Add(new PageItem
                    {
                        DisplayText = TotalPages.ToString(),
                        Number = TotalPages
                    });
                }
            }
        }

    }
}
