namespace YouSpent.Models
{
    public class Day
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public List<Expense> Expenses { get; set; } = new List<Expense>();
        
        public decimal TotalSpent => Expenses.Sum(e => e.Amount);
        public string DayOfWeek => Date.DayOfWeek.ToString();
        public int DayNumber => Date.Day;
    }
}
