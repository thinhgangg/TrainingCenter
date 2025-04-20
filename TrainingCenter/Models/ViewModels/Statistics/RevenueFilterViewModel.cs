namespace TrainingCenter.Models.ViewModels
{
    public class RevenueFilterViewModel
    {
        public string CourseName { get; set; }
        public decimal Fee { get; set; }
        public int StudentCount { get; set; }
        public decimal Revenue => Fee * StudentCount;
    }
}