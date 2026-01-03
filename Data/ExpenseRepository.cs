using Microsoft.EntityFrameworkCore;
using YouSpent.Models;

namespace YouSpent.Data
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly AppDbContext _context;

        public ExpenseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Expense> AddAsync(Expense entity)
        {
            _context.Expenses.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Expense> UpdateAsync(Expense entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return false;
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(string category)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Where(e => e.Category == category)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByDayAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Where(e => e.Date >= startOfDay && e.Date < endOfDay)
                .AsNoTracking()
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByTypeIdAsync(int expenseTypeId, DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Where(e => e.ExpenseTypeId == expenseTypeId && e.Date >= startDate && e.Date <= endDate)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalSpentAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .SumAsync(e => e.Amount);
        }

        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            return await _context.Expenses
                .Select(e => e.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}
