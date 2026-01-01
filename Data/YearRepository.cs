using Microsoft.EntityFrameworkCore;
using YouSpent.Models;

namespace YouSpent.Data
{
    public class YearRepository : IYearRepository
    {
        private readonly AppDbContext _context;

        public YearRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Year?> GetByIdAsync(int id)
        {
            return await _context.Years
                .Include(y => y.Months)
                .FirstOrDefaultAsync(y => y.Id == id);
        }

        public async Task<IEnumerable<Year>> GetAllAsync()
        {
            return await _context.Years
                .Include(y => y.Months)
                .ToListAsync();
        }

        public async Task<Year> AddAsync(Year entity)
        {
            _context.Years.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Year> UpdateAsync(Year entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var year = await _context.Years.FindAsync(id);
            if (year == null)
            {
                return false;
            }

            _context.Years.Remove(year);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<Year?> GetYearAsync(int yearNumber)
        {
            return await _context.Years
                .Include(y => y.Months)
                    .ThenInclude(m => m.Days)
                        .ThenInclude(d => d.Expenses)
                .FirstOrDefaultAsync(y => y.YearNumber == yearNumber);
        }

        public async Task<Year?> GetYearWithDetailsAsync(int id)
        {
            return await _context.Years
                .Include(y => y.Months)
                    .ThenInclude(m => m.Days)
                        .ThenInclude(d => d.Expenses)
                .Include(y => y.Months)
                    .ThenInclude(m => m.Weeks)
                        .ThenInclude(w => w.Days)
                .FirstOrDefaultAsync(y => y.Id == id);
        }

        public async Task<IEnumerable<Year>> GetAllYearsWithMonthsAsync()
        {
            return await _context.Years
                .Include(y => y.Months)
                .OrderByDescending(y => y.YearNumber)
                .ToListAsync();
        }
    }
}
