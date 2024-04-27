using System.Collections.Generic;

namespace TrainingApplication.Models
{
    public class Organisation
    {
        public string OrgName { get; set; }

        public List<Employee> Employees { get; set; }
    }
}