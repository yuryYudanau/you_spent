using YouSpent.Models;

namespace YouSpent.Data
{
    /// <summary>
    /// Example service demonstrating how to use repositories for CRUD operations
    /// </summary>
    public class ExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IDayRepository _dayRepository;

        public ExpenseService(IExpenseRepository expenseRepository, IDayRepository dayRepository)
        {
            _expenseRepository = expenseRepository;
            _dayRepository = dayRepository;
        }

        /// <summary>
        /// Add a new expense and associate it with the corresponding day and expense type
        /// </summary>
        public async Task<Expense> AddExpenseAsync(string description, decimal amount, DateTime date, int? expenseTypeId = null, string? category = null)
        {
            // Create expense
            var expense = new Expense
            {
                Description = description,
                Amount = amount,
                Date = date,
                ExpenseTypeId = expenseTypeId,
                Category = category ?? string.Empty
            };

            // Check if day exists
            var day = await _dayRepository.GetDayByDateAsync(date);
            if (day == null)
            {
                // Create new day if it doesn't exist
                day = new Day
                {
                    Date = date.Date
                };
                day = await _dayRepository.AddAsync(day);
            }

            // Associate expense with day
            expense.DayId = day.Id;

            // Save expense
            return await _expenseRepository.AddAsync(expense);
        }

        /// <summary>
        /// Update an existing expense
        /// </summary>
        public async Task<Expense?> UpdateExpenseAsync(int id, string description, decimal amount, string category)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                return null;
            }

            expense.Description = description;
            expense.Amount = amount;
            expense.Category = category;

            return await _expenseRepository.UpdateAsync(expense);
        }

        /// <summary>
        /// Delete an expense
        /// </summary>
        public async Task<bool> DeleteExpenseAsync(int id)
        {
            return await _expenseRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Get all expenses for a specific date
        /// </summary>
        public async Task<IEnumerable<Expense>> GetExpensesByDateAsync(DateTime date)
        {
            return await _expenseRepository.GetExpensesByDayAsync(date);
        }

        /// <summary>
        /// Get total spent in a date range
        /// </summary>
        public async Task<decimal> GetTotalSpentAsync(DateTime startDate, DateTime endDate)
        {
            return await _expenseRepository.GetTotalSpentAsync(startDate, endDate);
        }

        /// <summary>
        /// Get all expenses by category
        /// </summary>
        public async Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(string category)
        {
            return await _expenseRepository.GetExpensesByCategoryAsync(category);
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            return await _expenseRepository.GetAllCategoriesAsync();
        }

        /// <summary>
        /// Get all expenses
        /// </summary>
        public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
        {
            return await _expenseRepository.GetAllAsync();
        }
    }
}
