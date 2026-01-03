using YouSpent.Models;

namespace YouSpent.Data
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(string category);
        Task<IEnumerable<Expense>> GetExpensesByDayAsync(DateTime date);
        Task<IEnumerable<Expense>> GetExpensesByTypeIdAsync(int expenseTypeId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalSpentAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
    }
}
