namespace TrainingApplication.Core.Attributes
{
    public class OrderAttribute : System.Attribute
    {
        public int Order { get; set; }

        public OrderAttribute(int order)
        {
            Order = order;
        }
    }
}