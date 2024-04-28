using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TrainingApplication.Core;
using TrainingApplication.Core.Attributes;
using TextBoxPair = System.Collections.Generic.KeyValuePair<string, System.Windows.Controls.TextBox>;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
        private static Grid _grid;
        private static List<TextBoxPair> _textBoxes = new List<TextBoxPair>();
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
            foreach (TextBoxPair pair in _textBoxes)
            {
                BindingOperations.ClearBinding(pair.Value, TextBox.TextProperty);
            }

            PropertyInfo[] properties = _modelType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (_textBoxes.All(t => t.Key != property.Name))
                {
                    continue;
                }

                TextBox box = _textBoxes.First(t => t.Key == property.Name).Value;

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

            PropertyInfo[] properties = RemoveHiddenProperties(type.GetProperties());

            if (properties.Length == 0)
            {
                Logger.Log("No properties found");
                return;
            }

            _modelType = type;

            ConstructGrid(properties);
        }

        private static PropertyInfo[] RemoveHiddenProperties(IEnumerable<PropertyInfo> getProperties)
        {
            return getProperties.Where(p =>
                !Attribute.GetCustomAttributes(p).Any(a => a is HiddenAttribute hidden && hidden.IsHidden)).ToArray();
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

            FillTextBoxesKeys(properties);

            for (int i = 0; i < properties.Count; i++)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(properties[i]);

                if (attributes.Any(a => a is HiddenAttribute hidden && hidden.IsHidden))
                {
                    continue;
                }

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

                // Enters TextBox into _textBoxes on the index where key is same as property name
                int index = _textBoxes.FindIndex(t => t.Key == properties[i].Name);
                if (index != -1)
                {
                    _textBoxes[index] = new TextBoxPair(properties[i].Name, textBox);
                }
                else
                {
                    Logger.Log($"Property {properties[i].Name} not found in _textBoxes");
                }
            }

            _topContainer.Content = _grid;
        }

        /// <summary>
        /// Fills _textBoxes with key value pairs where key is property placed at the index of Order attribute,
        /// those without Order attribute are ignored and put into temp list, if index is already filled with property
        /// name and null, then shows Logger message about it and places that property into temp list. Consequently,
        /// places previously ignored properties into _textBoxes on free indexes.
        /// </summary>
        /// <param name="properties"></param>
        private static void FillTextBoxesKeys(IEnumerable<PropertyInfo> properties)
        {
            _textBoxes.Clear();
            int propertiesCount = properties.Count();
            _textBoxes = new List<TextBoxPair>(propertiesCount);
            _textBoxes.AddRange(new TextBoxPair[propertiesCount]);

            List<PropertyInfo> temp = new List<PropertyInfo>();
            foreach (PropertyInfo t in properties)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(t);
                foreach (Attribute attribute in attributes)
                {
                    if (attribute is HiddenAttribute hidden)
                    {
                        if (hidden.IsHidden)
                        {
                            break;
                        }

                        continue;
                    }

                    if (!(attribute is OrderAttribute orderAttribute))
                    {
                        temp.Add(t);
                        continue;
                    }

                    int order = orderAttribute.Order;

                    if (order > _textBoxes.Count - 1)
                    {
                        Logger.Log($"Order attribute value {order} is higher than _textBoxes count {_textBoxes.Count}");
                        temp.Add(t);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(_textBoxes[order].Key))
                    {
                        Logger.Log($"_textBoxes[{order}] is already filled with key {_textBoxes[order].Key}.");
                        temp.Add(t);
                        continue;
                    }

                    _textBoxes[order] = new TextBoxPair(t.Name, null);
                }
            }

            // Fills _textBoxes with properties from temp list
            foreach (PropertyInfo propertyInfo in temp)
            {
                int index = _textBoxes.FindIndex(t => string.IsNullOrEmpty(t.Key));
                if (index != -1)
                {
                    _textBoxes[index] = new TextBoxPair(propertyInfo.Name, null);
                }
                else
                {
                    Logger.Log($"No empty index found in _textBoxes for property {propertyInfo.Name}");
                }
            }
        }
    }
}