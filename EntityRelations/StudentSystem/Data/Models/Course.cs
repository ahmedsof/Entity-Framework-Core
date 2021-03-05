using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace P01_StudentSystem.Data.Models
{
    public class Course
    {

        public Course()
        {
            this.HomeworkSubmissions = new HashSet<Homework>();
            this.Resources = new HashSet<Resource>();
            this.StudentsEnrolled = new HashSet<StudentCourse>();
        }
        //o CourseId
        public int CourseId { get; set; }


        //o Name(up to 80 characters, unicode)
        [Required] [MaxLength(80)] 
        public string Name { get; set; }
        
        //o Description(unicode, not required)

        public string Description { get; set; }
        //o StartDate
        public DateTime StartDate { get; set; }
        //o EndDate
        public DateTime EndDate { get; set; }

        //o Price
        public decimal Price { get; set; }

        public ICollection<Homework> HomeworkSubmissions { get; set; }

        public ICollection<Resource> Resources { get; set; }

        public ICollection<StudentCourse> StudentsEnrolled { get; set; }
    }
}
