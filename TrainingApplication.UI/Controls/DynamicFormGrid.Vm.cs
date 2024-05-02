using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainingApplication.Core;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
        private static Grid _grid;
        private static List<StoredControls> _controls = new List<StoredControls>();
        private static Type _modelType;

        private static void DataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildGrid(e.NewValue);
        }

        private static void TextBoxStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Style style = (Style) e.NewValue;
            if (style == null || style.TargetType != typeof(TextBox))
            {
                Logger.Log("TextBoxStyle is null");
                return;
            }

            _textBoxStyle = style;

            List<TextBox> labels = _controls.Select(controls => controls.Controls?.TextBox).ToList();

            if (!labels.Any() || labels.Any(label => label == null))
            {
                return;
            }

            foreach (TextBox textBox in labels)
            {
                textBox.Style = style;
            }
        }

        private static void LabelStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Style style = (Style) e.NewValue;
            if (style == null || style.TargetType != typeof(Label))
            {
                Logger.Log("LabelStyle is null");
                return;
            }

            _labelStyle = style;

            List<Label> labels = _controls.Select(controls => controls.Controls?.Label).ToList();

            if (!labels.Any() || labels.Any(label => label == null))
            {
                return;
            }

            foreach (Label label in labels)
            {
                label.Style = style;
            }
        }
    }
}