using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncWebApp.Model
{
    public class UpdateO365Message
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string JobTitle { get; set; }
        public string WorkEmail { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Division { get; set; }

        public string SupervisorId { get; set; }
        public string SupervisorEmail { get; set; }
    }

    public class EmployeeMessage
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string JobTitle { get; set; }
        public string WorkEmail { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Division { get; set; }

        public UpdateO365Message To(string supervisorEmail, string supervisorId)
        {
            return new UpdateO365Message
            {
                Department = this.Department,
                Division = this.Division,
                FirstName = this.FirstName,
                Gender = this.Gender,
                Id = this.Id,
                JobTitle = this.JobTitle,
                LastName = this.LastName,
                Location = this.Location,
                WorkEmail = this.WorkEmail,
                SupervisorId = supervisorId,
                SupervisorEmail = supervisorEmail
            };
        }
    }

    public static class EmployeeExtensions
    {
        public static IEnumerable<EmployeeMessage> ToEmployeeMessage(this Employee[] employees)
        {
            return employees.Select(x => ToEmployeeMessage(x));
        }

        public static EmployeeMessage ToEmployeeMessage(this Employee employee)
        {
            return new EmployeeMessage
            {
                Department = employee.Department,
                Division = employee.Division,
                FirstName = employee.FirstName,
                Gender = employee.Gender,
                Id = employee.Id,
                JobTitle = employee.JobTitle,
                LastName = employee.LastName,
                Location = employee.Location,
                WorkEmail = employee.WorkEmail
            };
        }
    }
}
