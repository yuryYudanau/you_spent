using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YouSpent.Data;
using YouSpent.Models;

namespace YouSpent.ViewModels
{
    public class ExpenseTypesPageViewModel : INotifyPropertyChanged
    {
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        private string _newTypeName = string.Empty;

        public ObservableCollection<ExpenseType> ExpenseTypes { get; set; } = new();

        public string NewTypeName
        {
            get => _newTypeName;
            set
            {
                _newTypeName = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ExpenseTypesPageViewModel(IExpenseTypeRepository expenseTypeRepository)
        {
            _expenseTypeRepository = expenseTypeRepository;
        }

        public async Task LoadExpenseTypesAsync()
        {
            var types = await _expenseTypeRepository.GetAllAsync();
            ExpenseTypes.Clear();
            foreach (var type in types)
            {
                ExpenseTypes.Add(type);
            }
        }

        public async Task<bool> AddExpenseTypeAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTypeName))
                return false;

            // Check if already exists
            var existing = await _expenseTypeRepository.GetByNameAsync(NewTypeName);
            if (existing != null)
                return false;

            var newType = new ExpenseType
            {
                Name = NewTypeName.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var added = await _expenseTypeRepository.AddAsync(newType);
            ExpenseTypes.Add(added);
            NewTypeName = string.Empty;
            return true;
        }

        public async Task<bool> ToggleExpenseTypeAsync(int id)
        {
            System.Diagnostics.Debug.WriteLine($"[VM] ToggleExpenseTypeAsync called for id: {id}");
            
            // Find the item in the collection first
            var item = ExpenseTypes.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                System.Diagnostics.Debug.WriteLine($"[VM] Item with id {id} not found in collection");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"[VM] Found item: {item.Name}, Current IsActive: {item.IsActive}");

            // Toggle in database
            var success = await _expenseTypeRepository.ToggleActiveStatusAsync(id);
            System.Diagnostics.Debug.WriteLine($"[VM] Database toggle result: {success}");
            
            if (success)
            {
                // Update UI object - this will trigger INotifyPropertyChanged
                item.IsActive = !item.IsActive;
                System.Diagnostics.Debug.WriteLine($"[VM] Updated UI item, New IsActive: {item.IsActive}");
            }
            return success;
        }

        public async Task<bool> UpdateExpenseTypeAsync(ExpenseType expenseType)
        {
            var updated = await _expenseTypeRepository.UpdateAsync(expenseType);
            if (updated != null)
            {
                await LoadExpenseTypesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteExpenseTypeAsync(int id)
        {
            var success = await _expenseTypeRepository.DeleteAsync(id);
            if (success)
            {
                var typeToRemove = ExpenseTypes.FirstOrDefault(t => t.Id == id);
                if (typeToRemove != null)
                {
                    ExpenseTypes.Remove(typeToRemove);
                }
            }
            return success;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
