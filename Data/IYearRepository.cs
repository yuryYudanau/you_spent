using YouSpent.Models;

namespace YouSpent.Data
{
    public interface IYearRepository : IRepository<Year>
    {
        Task<Year?> GetYearAsync(int yearNumber);
        Task<Year?> GetYearWithDetailsAsync(int id);
        Task<IEnumerable<Year>> GetAllYearsWithMonthsAsync();
    }
}
