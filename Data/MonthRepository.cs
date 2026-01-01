using Microsoft.EntityFrameworkCore;
using YouSpent.Models;

namespace YouSpent.Data
{
    public class MonthRepository : IMonthRepository
    {
        private readonly AppDbContext _context;

        public MonthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Month?> GetByIdAsync(int id)
        {
            return await _context.Months
                .Include(m => m.Days)
                .Include(m => m.Weeks)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Month>> GetAllAsync()
        {
            return await _context.Months
                .Include(m => m.Days)
                .Include(m => m.Weeks)
                .ToListAsync();
        }

        public async Task<Month> AddAsync(Month entity)
        {
            _context.Months.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Month> UpdateAsync(Month entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var month = await _context.Months.FindAsync(id);
            if (month == null)
            {
                return false;
            }

            _context.Months.Remove(month);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<Month?> GetMonthAsync(int year, int monthNumber)
        {
            return await _context.Months
                .Include(m => m.Days)
                    .ThenInclude(d => d.Expenses)
                .Include(m => m.Weeks)
                .FirstOrDefaultAsync(m => m.Year == year && m.MonthNumber == monthNumber);
        }

        public async Task<IEnumerable<Month>> GetMonthsByYearAsync(int yearId)
        {
            return await _context.Months
                .Include(m => m.Days)
                .Include(m => m.Weeks)
                .Where(m => m.YearId == yearId)
                .OrderBy(m => m.MonthNumber)
                .ToListAsync();
        }

        public async Task<Month?> GetMonthWithDetailsAsync(int id)
        {
            return await _context.Months
                .Include(m => m.Days)
                    .ThenInclude(d => d.Expenses)
                .Include(m => m.Weeks)
                    .ThenInclude(w => w.Days)
                        .ThenInclude(d => d.Expenses)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
