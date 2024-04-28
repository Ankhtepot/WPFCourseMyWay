using TrainingApplication.Core.Attributes;

namespace TrainingApplication.Models
{
    public class Employee
    {
        [Order(3)] public string UserId { get; set; }
        [Order(2)] public string JobTitle { get; set; }
        [Order(0)] public string FirstName { get; set; }
        [Order(1)] public string LastName { get; set; }
        [Order(4)] [Hidden(false)] public string FullName { get; set; }
        [Order(5)] public string EmployeeCode { get; set; }
        [Order(6)] public string Region { get; set; }
        [Order(7)] public string PhoneNo { get; set; }
        [Order(8)] public string EmailId { get; set; }
        [Hidden] public int Id { get; set; }
    }
}