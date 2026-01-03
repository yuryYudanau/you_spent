using YouSpent.Models;

namespace YouSpent.Data
{
    public interface IExpenseTypeRepository : IRepository<ExpenseType>
    {
        Task<IEnumerable<ExpenseType>> GetActiveTypesAsync();
        Task<ExpenseType?> GetByNameAsync(string name);
        Task<bool> ToggleActiveStatusAsync(int id);
    }
}
