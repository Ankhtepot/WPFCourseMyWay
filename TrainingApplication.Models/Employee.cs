using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using TrainingApplication.Core.Attributes;

namespace TrainingApplication.Models
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Employee
    {
        [Hidden] public string UserId { get; set; }

        [Order(3)]
        [ConcatWith(new[] {nameof(UserId), nameof(EmployeeCode)}, separator: " |<>| ")]
        [DisplayName("Job Description")]
        [Disabled]
        public string JobTitle { get; set; }

        [Order(0)] public string FirstName { get; set; }
        [Order(1)] public string LastName { get; set; }

        [Order(2)] [Hidden(false)] public string FullName { get; set; }

        [Hidden] public string EmployeeCode { get; set; }
        [Order(4)] public string Region { get; set; }
        [Order(5)] [Masked("***-****")] public string PhoneNo { get; set; }

        [Order(6)]
        [ShowIfTrue(nameof(ShowDetails))]
        public string EmailId { get; set; }

        [Hidden] public int Id { get; set; }
        [Hidden] public bool ShowDetails { get; set; }
    }
}