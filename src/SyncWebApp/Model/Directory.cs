using System;
using System.Collections.Generic;
using System.Text;

namespace SyncWebApp.Model
{

    public class Directory
    {
        public Field[] Fields { get; set; }
        public Employee[] Employees { get; set; }
    }

    public class Field
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class Employee
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public object PreferredName { get; set; }
        public string Gender { get; set; }
        public string JobTitle { get; set; }
        public object WorkPhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkEmail { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Division { get; set; }
        public string LinkedIn { get; set; }
        public object WorkPhoneExtension { get; set; }
        public bool PhotoUploaded { get; set; }
        public string PhotoUrl { get; set; }
        public int CanUploadPhoto { get; set; }
    }
}
