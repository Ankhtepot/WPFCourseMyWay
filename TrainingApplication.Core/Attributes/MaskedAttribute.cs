using System;

namespace TrainingApplication.Core.Attributes
{
    public class MaskedAttribute : Attribute
    {
        public readonly string Mask;

        public MaskedAttribute(string mask = "*")
        {
            Mask = mask;
        }
    }
}