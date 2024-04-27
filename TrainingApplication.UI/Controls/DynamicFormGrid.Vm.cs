using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TrainingApplication.Core;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
        private static Grid _grid;
        private static readonly Dictionary<string, TextBox> _textBoxes = new Dictionary<string, TextBox>();
        private static Type _modelType;

        private static void SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DynamicFormGrid control = (DynamicFormGrid) d;

            if (control == null)
            {
                return;
            }

            //Checks if e.NewValue is not null and type fits to _modelType
            if (e.NewValue == null || e.NewValue.GetType() != _modelType) return;
            //Clears all bindings from TextBoxes to prevent overwriting previous item
            foreach (TextBox textBox in _textBoxes.Values)
            {
                BindingOperations.ClearBinding(textBox, TextBox.TextProperty);
            }

            PropertyInfo[] properties = _modelType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (!_textBoxes.TryGetValue(property.Name, out TextBox box)) continue;

                box.Text = property.GetValue(e.NewValue).ToString();
                // sets 2 way binding for the TextBox and e.NewValue
                box.SetBinding(TextBox.TextProperty, new Binding(property.Name)
                {
                    Source = e.NewValue,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            }
        }

        private static void BuildGrid(object modelItem)
        {
            if (modelItem == null)
            {
                Logger.Log("Model item is null");
                return;
            }

            Type type = modelItem.GetType();

            if (type.IsGenericType && type.IsGenericEnumerableType())
            {
                type = type.GetGenericArguments()[0];
            }

            Logger.Log($"Building grid for collection of type: {type.Name}");

            PropertyInfo[] properties = type.GetProperties();

            if (properties.Length == 0)
            {
                Logger.Log("No properties found");
                return;
            }

            _modelType = type;

            ConstructGrid(properties);
        }

        private static void DataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildGrid(e.NewValue);
        }

        private static void ConstructGrid(IReadOnlyList<PropertyInfo> properties)
        {
            _topContainer.Content = null;

            _grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            _grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
            _grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)});

            for (int i = 0; i < properties.Count; i++)
            {
                _grid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});

                Label label = new Label()
                {
                    Content = properties[i].Name.ToHumanReadable(),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };

                Grid.SetRow(label, i);
                Grid.SetColumn(label, 0);

                TextBox textBox = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 5, 0)
                };

                Grid.SetRow(textBox, i);
                Grid.SetColumn(textBox, 1);

                _grid.Children.Add(label);
                _grid.Children.Add(textBox);

                _textBoxes.Add(properties[i].Name, textBox);
            }

            _topContainer.Content = _grid;
        }


        // public event PropertyChangedEventHandler PropertyChanged;
        //
        // protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        // {
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // }
    }
}