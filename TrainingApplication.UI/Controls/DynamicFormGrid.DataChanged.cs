using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TrainingApplication.Core;
using TrainingApplication.Core.Attributes;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
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
                BindingOperations.ClearBinding(controls.Controls.TextBox, TextBox.TextProperty);
            }

            PropertyInfo[] properties = _modelType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (_controls.All(t => t.PropertyName != property.Name))
                {
                    continue;
                }

                ControlsReference controls = _controls.First(t => t.PropertyName == property.Name).Controls;
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
    }
}