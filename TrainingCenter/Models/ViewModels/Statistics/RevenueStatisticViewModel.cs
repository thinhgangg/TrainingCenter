namespace TrainingCenter.Models.ViewModels
{
    public class RevenueStatisticViewModel
    {
        public string CourseName { get; set; }
        public decimal Fee { get; set; }
        public int StudentCount { get; set; }
        public decimal Revenue => Fee * StudentCount;
    }
}
