using System.Windows;
using System.Windows.Controls;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
        private const string DataSourceName = "DataSource";
        private const string SelectedItemName = "SelectedItem";

        private static ContentControl _topContainer;

        public object DataSource
        {
            get => GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register(DataSourceName, typeof(object), typeof(DynamicFormGrid),
                new PropertyMetadata(DataSourceChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(SelectedItemName, typeof(object), typeof(DynamicFormGrid),
                new PropertyMetadata(SelectedItemChanged));

        public DynamicFormGrid()
        {
            InitializeComponent();

            _topContainer = TopContainer;
        }
    }
}