using Microsoft.EntityFrameworkCore;
using YouSpent.Models;

namespace YouSpent.Data
{
    public class DayRepository : IDayRepository
    {
        private readonly AppDbContext _context;

        public DayRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Day?> GetByIdAsync(int id)
        {
            return await _context.Days
                .Include(d => d.Expenses)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Day>> GetAllAsync()
        {
            return await _context.Days
                .Include(d => d.Expenses)
                .ToListAsync();
        }

        public async Task<Day> AddAsync(Day entity)
        {
            _context.Days.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Day> UpdateAsync(Day entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var day = await _context.Days.FindAsync(id);
            if (day == null)
            {
                return false;
            }

            _context.Days.Remove(day);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<Day?> GetDayByDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            return await _context.Days
                .Include(d => d.Expenses)
                .FirstOrDefaultAsync(d => d.Date.Date == startOfDay);
        }

        public async Task<IEnumerable<Day>> GetDaysByMonthAsync(int monthId)
        {
            return await _context.Days
                .Include(d => d.Expenses)
                .Where(d => d.MonthId == monthId)
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Day>> GetDaysByWeekAsync(int weekId)
        {
            return await _context.Days
                .Include(d => d.Expenses)
                .Where(d => d.WeekId == weekId)
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Day>> GetDaysInRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Days
                .Include(d => d.Expenses)
                .Where(d => d.Date >= startDate && d.Date <= endDate)
                .OrderBy(d => d.Date)
                .ToListAsync();
        }
    }
}
