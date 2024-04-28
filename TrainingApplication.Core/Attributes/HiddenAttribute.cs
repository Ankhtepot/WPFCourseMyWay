namespace TrainingApplication.Core.Attributes
{
    public class HiddenAttribute : System.Attribute
    {
        public bool IsHidden { get; set; }

        public HiddenAttribute(bool isHidden = true)
        {
            IsHidden = isHidden;
        }
    }
}