using System;

namespace TrainingApplication.Core.Attributes
{
    public class ShowIfTrueAttribute : Attribute
    {
        public readonly string BooleanFieldName;

        public ShowIfTrueAttribute(string booleanFieldName)
        {
            BooleanFieldName = booleanFieldName;
        }
    }
}