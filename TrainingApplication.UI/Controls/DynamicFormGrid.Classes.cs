using System.Windows.Controls;

namespace TrainingApplication.UI.Controls
{
    public partial class DynamicFormGrid
    {
        internal class ControlsReference
        {
            public TextBox TextBox { get; set; }
            public Label Label { get; set; }
        }

        internal class StoredControls
        {
            public string PropertyName { get; set; } = string.Empty;
            public ControlsReference Controls { get; set; } = null;
        }
    }
}