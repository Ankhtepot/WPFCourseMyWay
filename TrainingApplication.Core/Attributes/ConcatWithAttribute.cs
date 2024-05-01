using System;

namespace TrainingApplication.Core.Attributes
{
    public class ConcatWithAttribute : Attribute
    {
        public readonly string[] OtherProperty;
        public readonly string Separator;

        public ConcatWithAttribute(string[] otherProperty, string separator = " ")
        {
            OtherProperty = otherProperty;
            Separator = separator;
        }
    }
}