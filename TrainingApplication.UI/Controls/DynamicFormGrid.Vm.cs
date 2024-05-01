using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TrainingApplication.Core;
using TrainingApplication.Core.Attributes;
using Xceed.Wpf.Toolkit;
using StoredControls =
    System.Collections.Generic.KeyValuePair<string, TrainingApplication.UI.Controls.ControlsReference>;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
        private static Grid _grid;
        private static List<StoredControls> _controls = new List<StoredControls>();
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
            foreach (StoredControls controls in _controls)
            {
                BindingOperations.ClearBinding(controls.Value.TextBox, TextBox.TextProperty);
            }

            PropertyInfo[] properties = _modelType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (_controls.All(t => t.Key != property.Name))
                {
                    continue;
                }

                ControlsReference controls = _controls.First(t => t.Key == property.Name).Value;
                TextBox textBox = controls.TextBox;
                Label label = controls.Label;

                bool shouldBeShown = ResolveShowIfTrueAttribute(property, e.NewValue);

                if (shouldBeShown)
                {
                    label.Visibility = Visibility.Visible;
                    textBox.Visibility = Visibility.Visible;
                }
                else
                {
                    label.Visibility = Visibility.Collapsed;
                    textBox.Visibility = Visibility.Collapsed;
                    continue;
                }

                // sets 2 way binding for the TextBox and e.NewValue
                textBox.SetBinding(TextBox.TextProperty, SetTextBoxBinding(property, e.NewValue));
            }
        }

        private static BindingBase SetTextBoxBinding(PropertyInfo property, object oNewValue)
        {
            BindingBase binding = new Binding(property.Name)
            {
                Source = oNewValue,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            if (!HasConcatWithAttribute(property)) return binding;
            ConcatWithAttribute concatWithAttribute = property.GetCustomAttribute<ConcatWithAttribute>();
            binding = new MultiBinding();
            ((MultiBinding) binding).Converter = new StringConcatConverter();
            ((MultiBinding) binding).ConverterParameter = concatWithAttribute.Separator;
            ((MultiBinding) binding).Bindings.Add(new Binding(property.Name)
            {
                Source = oNewValue,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            foreach (string otherProperty in concatWithAttribute.OtherProperty)
            {
                ((MultiBinding) binding).Bindings.Add(new Binding(otherProperty)
                {
                    Source = oNewValue,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            }

            // ((MultiBinding) binding).Bindings.Add(new Binding(concatWithAttribute.OtherProperty)
            // {
            //     Source = oNewValue,
            //     Mode = BindingMode.TwoWay,
            //     UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            // });

            return binding;
        }

        private static bool ResolveShowIfTrueAttribute(PropertyInfo property, object oNewValue)
        {
            if (oNewValue == null)
            {
                Logger.Log("Model item is null");
                return true;
            }

            if (property == null)
            {
                Logger.Log("Property is null");
                return true;
            }

            Attribute[] attributes = Attribute.GetCustomAttributes(property);

            foreach (Attribute attribute in attributes)
            {
                if (!(attribute is ShowIfTrueAttribute showIfTrue)) continue;

                PropertyInfo propertyInfo = oNewValue.GetType().GetProperty(showIfTrue.BooleanFieldName);

                if (propertyInfo != null)
                {
                    bool? value = (bool?) propertyInfo.GetValue(oNewValue);
                    return value ?? true;
                }

                Logger.Log($"Property {showIfTrue.BooleanFieldName} not found in {oNewValue.GetType().Name}");
                return true;
            }

            return true;
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

                Label label = CreateLabel(properties, i);
                TextBox textBox = CreateTextBox(properties, i);

                _grid.Children.Add(label);
                _grid.Children.Add(textBox);

                // Enters TextBox into _textBoxes on the index where key is same as property name
                int index = _controls.FindIndex(t => t.Key == properties[i].Name);
                if (index != -1)
                {
                    _controls[index] = new StoredControls(properties[i].Name,
                        new ControlsReference {Label = label, TextBox = textBox});
                }
                else
                {
                    Logger.Log($"Property {properties[i].Name} not found in _textBoxes");
                }
            }

            _topContainer.Content = _grid;
        }

        private static TextBox CreateTextBox(IEnumerable<PropertyInfo> properties, int index)
        {
            TextBox textBox = ResolveTextBoxType(properties.ElementAt(index));

            Grid.SetRow(textBox, index);
            Grid.SetColumn(textBox, 1);

            return textBox;
        }

        private static TextBox ResolveTextBoxType(PropertyInfo property)
        {
            TextBox textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };


            if (ShouldBeDisabled(property))
            {
                textBox.IsEnabled = false;
            }

            MaskedAttribute maskedAttribute = property.GetCustomAttribute<MaskedAttribute>();

            if (maskedAttribute == null)
            {
                return textBox;
            }

            textBox = new MaskedTextBox();
            ((MaskedTextBox) textBox).Mask = maskedAttribute.Mask;

            return textBox;
        }

        private static bool ShouldBeDisabled(PropertyInfo property)
        {
            return property.GetCustomAttribute<DisabledAttribute>() is DisabledAttribute disabledAttribute &&
                   disabledAttribute.IsDisabled;
        }

        private static Label CreateLabel(IReadOnlyList<PropertyInfo> properties, int i)
        {
            Label label = new Label()
            {
                Content = SetLabelText(properties, i),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };

            Grid.SetRow(label, i);
            Grid.SetColumn(label, 0);

            return label;
        }

        private static string SetLabelText(IReadOnlyList<PropertyInfo> properties, int i)
        {
            DisplayNameAttribute displayNameAttribute = properties[i].GetCustomAttribute<DisplayNameAttribute>();

            return displayNameAttribute != null
                ? displayNameAttribute.DisplayName
                : properties[i].Name.ToHumanReadable();
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
            _controls.Clear();
            int propertiesCount = properties.Count();
            _controls = new List<StoredControls>(propertiesCount);
            _controls.AddRange(new StoredControls[propertiesCount]);

            List<PropertyInfo> unorderedProperties = new List<PropertyInfo>();
            foreach (PropertyInfo propertyInfo in properties)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(propertyInfo);

                if (attributes.OfType<HiddenAttribute>().Any(attr => attr.IsHidden))
                {
                    continue;
                }

                if (!attributes.OfType<OrderAttribute>().Any())
                {
                    unorderedProperties.Add(propertyInfo);
                }
                else
                {
                    OrderAttribute orderAttribute = attributes.OfType<OrderAttribute>().First();

                    int order = orderAttribute.Order;

                    if (order > _controls.Count - 1)
                    {
                        Logger.Log($"Order attribute value {order} is higher than _textBoxes count {_controls.Count}");
                        unorderedProperties.Add(propertyInfo);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(_controls[order].Key))
                    {
                        Logger.Log($"_textBoxes[{order}] is already filled with key {_controls[order].Key}.");
                        unorderedProperties.Add(propertyInfo);
                        continue;
                    }

                    _controls[order] = new StoredControls(propertyInfo.Name, null);
                }
            }

            // Fills _textBoxes with properties from temp list
            foreach (PropertyInfo propertyInfo in unorderedProperties.Distinct())
            {
                int index = _controls.FindIndex(t => string.IsNullOrEmpty(t.Key));
                if (index != -1)
                {
                    _controls[index] = new StoredControls(propertyInfo.Name, null);
                }
                else
                {
                    Logger.Log($"No empty index found in _textBoxes for property {propertyInfo.Name}");
                }
            }
        }

        private static bool HasConcatWithAttribute(PropertyInfo property) =>
            property.GetCustomAttribute<ConcatWithAttribute>() != null;
    }

    internal class ControlsReference
    {
        public TextBox TextBox { get; set; }
        public Label Label { get; set; }
    }
}