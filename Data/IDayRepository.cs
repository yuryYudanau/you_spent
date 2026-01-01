using YouSpent.Models;

namespace YouSpent.Data
{
    public interface IDayRepository : IRepository<Day>
    {
        Task<Day?> GetDayByDateAsync(DateTime date);
        Task<IEnumerable<Day>> GetDaysByMonthAsync(int monthId);
        Task<IEnumerable<Day>> GetDaysByWeekAsync(int weekId);
        Task<IEnumerable<Day>> GetDaysInRangeAsync(DateTime startDate, DateTime endDate);
    }
}
