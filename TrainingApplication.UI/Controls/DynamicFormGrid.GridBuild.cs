using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using TrainingApplication.Core;
using TrainingApplication.Core.Attributes;
using Xceed.Wpf.Toolkit;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
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

            List<PropertyInfo> properties = RemoveHiddenProperties(type.GetProperties()).ToList();

            if (!properties.Any())
            {
                Logger.Log("No properties found");
                return;
            }

            _modelType = type;

            ConstructGrid(properties);
        }

        private static void ConstructGrid(IEnumerable<PropertyInfo> properties)
        {
            _topContainer.Content = null;

            _grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            _grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
            _grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)});

            FillTextBoxesKeys(properties);

            for (int i = 0; i < _controls.Count; i++)
            {
                PropertyInfo property = null;

                for (int j = 0; j < properties.Count(); j++)
                {
                    if (properties.ElementAt(j).Name != _controls[i].PropertyName) continue;
                    property = properties.ElementAt(j);
                    break;
                }

                if (property == null)
                {
                    Logger.Log($"Property {_controls[i].PropertyName} not found in properties");
                    continue;
                }

                Attribute[] attributes =
                    Attribute.GetCustomAttributes(property);

                if (attributes.Any(a => a is HiddenAttribute hidden && hidden.IsHidden))
                {
                    continue;
                }

                _grid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});

                Label label = CreateLabel(property, i);
                TextBox textBox = CreateTextBox(property, i);

                _grid.Children.Add(label);
                _grid.Children.Add(textBox);

                // Enters TextBox into _textBoxes on the index where key is same as property name
                _controls[i] = new StoredControls
                {
                    PropertyName = property.Name,
                    Controls = new ControlsReference {Label = label, TextBox = textBox}
                };
            }

            _topContainer.Content = _grid;
        }

        private static TextBox CreateTextBox(PropertyInfo property, int index)
        {
            TextBox textBox = ResolveTextBoxType(property);

            Grid.SetRow(textBox, index);
            Grid.SetColumn(textBox, 1);

            return textBox;
        }

        private static TextBox ResolveTextBoxType(PropertyInfo property)
        {
            TextBox textBox = new TextBox
            {
                Style = _textBoxStyle,
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

            textBox = new MaskedTextBox()
            {
                Mask = maskedAttribute.Mask,
                Style = _textBoxStyle != null && _textBoxStyle.TargetType == typeof(TextBox)
                    ? _textBoxStyle
                    : null
            };

            return textBox;
        }

        private static bool ShouldBeDisabled(PropertyInfo property)
        {
            return property.GetCustomAttribute<DisabledAttribute>() is DisabledAttribute disabledAttribute &&
                   disabledAttribute.IsDisabled;
        }

        private static IEnumerable<PropertyInfo> RemoveHiddenProperties(IEnumerable<PropertyInfo> getProperties)
        {
            return getProperties.Where(p =>
                !Attribute.GetCustomAttributes(p).Any(a => a is HiddenAttribute hidden && hidden.IsHidden));
        }

        private static Label CreateLabel(PropertyInfo property, int i)
        {
            Label label = new Label
            {
                Content = SetLabelText(property),
                Style = _labelStyle != null && _labelStyle.TargetType == typeof(Label)
                    ? _labelStyle
                    : null
            };

            Grid.SetRow(label, i);
            Grid.SetColumn(label, 0);

            return label;
        }

        private static string SetLabelText(PropertyInfo property)
        {
            DisplayNameAttribute displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();

            return displayNameAttribute != null
                ? displayNameAttribute.DisplayName
                : property.Name.ToHumanReadable();
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
            for (int i = 0; i < propertiesCount; i++)
            {
                _controls.Add(new StoredControls());
            }

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

                    if (!string.IsNullOrEmpty(_controls[order]?.PropertyName))
                    {
                        Logger.Log($"_textBoxes[{order}] is already filled with key {_controls[order].PropertyName}.");
                        unorderedProperties.Add(propertyInfo);
                        continue;
                    }

                    _controls[order] = new StoredControls
                    {
                        PropertyName = propertyInfo.Name,
                        Controls = null
                    };
                }
            }

            // Fills _textBoxes with properties from temp list
            foreach (PropertyInfo propertyInfo in unorderedProperties.Distinct())
            {
                int index = _controls.FindIndex(t => string.IsNullOrEmpty(t.PropertyName));
                if (index != -1)
                {
                    _controls[index] = new StoredControls
                    {
                        PropertyName = propertyInfo.Name,
                        Controls = null
                    };
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
}