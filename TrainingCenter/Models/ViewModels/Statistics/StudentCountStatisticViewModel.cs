using System;

namespace TrainingCenter.Models.ViewModels
{
    public class StudentCountStatisticViewModel
    {
        public string CourseName { get; set; }
        public string Instructor { get; set; }
        public DateTime StartDate { get; set; }
        public int? MaxStudents { get; set; }
        public int StudentCount { get; set; }

        public int FillRate
        {
            get
            {
                if (!MaxStudents.HasValue || MaxStudents.Value == 0) return 0;
                return (int)((double)StudentCount / MaxStudents.Value * 100);
            }
        }
    }
}
