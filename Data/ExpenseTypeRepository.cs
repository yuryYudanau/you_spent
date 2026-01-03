using Microsoft.EntityFrameworkCore;
using YouSpent.Models;

namespace YouSpent.Data
{
    public class ExpenseTypeRepository : IExpenseTypeRepository
    {
        private readonly AppDbContext _context;

        public ExpenseTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ExpenseType> AddAsync(ExpenseType entity)
        {
            _context.ExpenseTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var expenseType = await _context.ExpenseTypes.FindAsync(id);
            if (expenseType == null) return false;

            _context.ExpenseTypes.Remove(expenseType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ExpenseType>> GetAllAsync()
        {
            return await _context.ExpenseTypes
                .AsNoTracking()
                .OrderBy(et => et.Name)
                .ToListAsync();
        }

        public async Task<ExpenseType?> GetByIdAsync(int id)
        {
            return await _context.ExpenseTypes.FindAsync(id);
        }

        public async Task<ExpenseType?> UpdateAsync(ExpenseType entity)
        {
            var existingType = await _context.ExpenseTypes.FindAsync(entity.Id);
            if (existingType == null) return null;

            existingType.Name = entity.Name;
            existingType.IsActive = entity.IsActive;

            await _context.SaveChangesAsync();
            return existingType;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ExpenseType>> GetActiveTypesAsync()
        {
            return await _context.ExpenseTypes
                .Where(et => et.IsActive)
                .OrderBy(et => et.Name)
                .ToListAsync();
        }

        public async Task<ExpenseType?> GetByNameAsync(string name)
        {
            return await _context.ExpenseTypes
                .FirstOrDefaultAsync(et => et.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            System.Diagnostics.Debug.WriteLine($"[Repo] ToggleActiveStatusAsync called for id: {id}");
            
            var expenseType = await _context.ExpenseTypes.FindAsync(id);
            if (expenseType == null)
            {
                System.Diagnostics.Debug.WriteLine($"[Repo] ExpenseType with id {id} not found");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"[Repo] Found: {expenseType.Name}, Current IsActive: {expenseType.IsActive}");
            
            expenseType.IsActive = !expenseType.IsActive;
            
            System.Diagnostics.Debug.WriteLine($"[Repo] Changed to IsActive: {expenseType.IsActive}");
            
            await _context.SaveChangesAsync();
            
            System.Diagnostics.Debug.WriteLine($"[Repo] Changes saved to database");
            
            return true;
        }
    }
}
