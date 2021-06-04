using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public class Semester
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Course> Courses { get; set; } = null!;
    }
}