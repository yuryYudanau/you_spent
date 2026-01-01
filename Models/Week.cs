namespace YouSpent.Models
{
    public class Week
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public List<Day> Days { get; set; } = new List<Day>();
        
        public decimal TotalSpent => Days.Sum(d => d.TotalSpent);
        
        // Foreign key
        public int? MonthId { get; set; }
    }
}
