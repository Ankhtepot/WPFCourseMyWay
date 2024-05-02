using System.Windows;
using System.Windows.Controls;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
        private static ContentControl _topContainer;
        private static DynamicFormGrid _instance;
        private static Style _labelStyle;
        private static Style _textBoxStyle;

        public object DataSource
        {
            get => GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register(nameof(DataSource), typeof(object), typeof(DynamicFormGrid),
                new PropertyMetadata(DataSourceChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(DynamicFormGrid),
                new PropertyMetadata(SelectedItemChanged));

        public Style LabelStyle
        {
            get => (Style) GetValue(LabelStyleProperty);
            set => SetValue(LabelStyleProperty, value);
        }

        public static readonly DependencyProperty LabelStyleProperty =
            DependencyProperty.Register(nameof(LabelStyle), typeof(Style), typeof(DynamicFormGrid),
                new PropertyMetadata(LabelStyleChanged));

        public Style TextBoxStyle
        {
            get => (Style) GetValue(TextBoxStyleProperty);
            set => SetValue(TextBoxStyleProperty, value);
        }

        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register(nameof(TextBoxStyle), typeof(Style), typeof(DynamicFormGrid),
                new PropertyMetadata(TextBoxStyleChanged));

        public DynamicFormGrid()
        {
            InitializeComponent();

            _topContainer = TopContainer;
            _instance = this;
        }
    }
}