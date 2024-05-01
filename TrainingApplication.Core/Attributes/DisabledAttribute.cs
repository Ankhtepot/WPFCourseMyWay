using System;

namespace TrainingApplication.Core.Attributes
{
    public class DisabledAttribute : Attribute
    {
        public readonly bool IsDisabled;

        public DisabledAttribute(bool isDisabled = true)
        {
            IsDisabled = isDisabled;
        }
    }
}