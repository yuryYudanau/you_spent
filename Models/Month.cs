namespace YouSpent.Models
{
    public class Month
    {
        public int Id { get; set; }
        public int MonthNumber { get; set; }
        public int Year { get; set; }
        public List<Day> Days { get; set; } = new List<Day>();
        public List<Week> Weeks { get; set; } = new List<Week>();
        
        public decimal TotalSpent => Days.Sum(d => d.TotalSpent);
        public string MonthName => new DateTime(Year, MonthNumber, 1).ToString("MMMM");
    }
}
