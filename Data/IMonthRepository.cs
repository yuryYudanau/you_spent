using YouSpent.Models;

namespace YouSpent.Data
{
    public interface IMonthRepository : IRepository<Month>
    {
        Task<Month?> GetMonthAsync(int year, int monthNumber);
        Task<IEnumerable<Month>> GetMonthsByYearAsync(int yearId);
        Task<Month?> GetMonthWithDetailsAsync(int id);
    }
}
