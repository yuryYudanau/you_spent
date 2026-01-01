namespace YouSpent.Models
{
    public class Year
    {
        public int Id { get; set; }
        public int YearNumber { get; set; }
        public List<Month> Months { get; set; } = new List<Month>();
        
        public decimal TotalSpent => Months.Sum(m => m.TotalSpent);
        public bool IsLeapYear => DateTime.IsLeapYear(YearNumber);
    }
}
