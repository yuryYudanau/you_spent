using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Collections.ObjectModel;
using YouSpent.Models;
using YouSpent.Data;

namespace YouSpent.ViewModels
{
    public enum PeriodState
    {
        Past,
        Current,
        Future
    }

    public class ExpenseTypeWithExpenses : INotifyPropertyChanged
    {
        private ExpenseType _expenseType;
        private ObservableCollection<Expense> _expenses;

        public ExpenseType ExpenseType
        {
            get => _expenseType;
            set
            {
                _expenseType = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(HasExpenses));
            }
        }

        public decimal TotalAmount => Expenses?.Sum(e => e.Amount) ?? 0;
        public bool HasExpenses => Expenses?.Any() ?? false;

        public ExpenseTypeWithExpenses(ExpenseType expenseType)
        {
            _expenseType = expenseType;
            _expenses = new ObservableCollection<Expense>();
        }

        public void RefreshTotals()
        {
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(HasExpenses));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExpensesPageViewModel : INotifyPropertyChanged
    {
        private int _selectedYear;
        private int _selectedMonth;
        private const int MinYear = 2000;
        private const int MaxYear = 2100;
        private readonly CultureInfo _culture = new CultureInfo("ru-RU");
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        private readonly IExpenseRepository _expenseRepository;

        public ObservableCollection<ExpenseTypeWithExpenses> ExpenseTypeGroups { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ExpensesPageViewModel(IExpenseTypeRepository expenseTypeRepository, IExpenseRepository expenseRepository)
        {
            _expenseTypeRepository = expenseTypeRepository;
            _expenseRepository = expenseRepository;
            
            var now = DateTime.Now;
            _selectedYear = now.Year;
            _selectedMonth = now.Month;
        }

        public int SelectedYear
        {
            get => _selectedYear;
            private set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(YearState));
                }
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            private set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MonthName));
                    OnPropertyChanged(nameof(MonthState));
                }
            }
        }

        public string MonthName
        {
            get
            {
                var date = new DateTime(SelectedYear, SelectedMonth, 1);
                return _culture.DateTimeFormat.GetMonthName(SelectedMonth);
            }
        }

        public PeriodState YearState
        {
            get
            {
                var currentYear = DateTime.Now.Year;
                if (SelectedYear < currentYear) return PeriodState.Past;
                if (SelectedYear > currentYear) return PeriodState.Future;
                return PeriodState.Current;
            }
        }

        public PeriodState MonthState
        {
            get
            {
                var now = DateTime.Now;
                var selected = new DateTime(SelectedYear, SelectedMonth, 1);
                var current = new DateTime(now.Year, now.Month, 1);
                
                if (selected < current) return PeriodState.Past;
                if (selected > current) return PeriodState.Future;
                return PeriodState.Current;
            }
        }

        public async Task LoadExpensesAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[ExpensesVM] Loading expenses for {MonthName} {SelectedYear}");
            
            // Load all active expense types
            var expenseTypes = await _expenseTypeRepository.GetActiveTypesAsync();
            
            // Get date range for selected month
            var startDate = new DateTime(SelectedYear, SelectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            // Load all expenses for the month
            var allExpenses = await _expenseRepository.GetExpensesByDateRangeAsync(startDate, endDate);
            
            // Update existing groups or create new ones instead of clearing
            var expenseTypesList = expenseTypes.ToList();
            
            // Remove groups that are no longer active
            for (int i = ExpenseTypeGroups.Count - 1; i >= 0; i--)
            {
                if (!expenseTypesList.Any(et => et.Id == ExpenseTypeGroups[i].ExpenseType.Id))
                {
                    ExpenseTypeGroups.RemoveAt(i);
                }
            }
            
            // Update or add groups
            foreach (var expenseType in expenseTypesList)
            {
                var existingGroup = ExpenseTypeGroups.FirstOrDefault(g => g.ExpenseType.Id == expenseType.Id);
                
                if (existingGroup != null)
                {
                    // Update existing group
                    var typeExpenses = allExpenses.Where(e => e.Category == expenseType.Name).ToList();
                    
                    // Clear and re-add expenses to avoid recreating the whole group
                    existingGroup.Expenses.Clear();
                    foreach (var expense in typeExpenses)
                    {
                        existingGroup.Expenses.Add(expense);
                    }
                    existingGroup.RefreshTotals();
                }
                else
                {
                    // Add new group
                    var group = new ExpenseTypeWithExpenses(expenseType);
                    var typeExpenses = allExpenses.Where(e => e.Category == expenseType.Name).ToList();
                    
                    foreach (var expense in typeExpenses)
                    {
                        group.Expenses.Add(expense);
                    }
                    
                    ExpenseTypeGroups.Add(group);
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[ExpensesVM] Loaded {ExpenseTypeGroups.Count} expense types");
        }

        public async Task<Expense> AddQuickExpenseAsync(int expenseTypeId, string expenseTypeName, decimal amount)
        {
            System.Diagnostics.Debug.WriteLine($"[ExpensesVM] Adding quick expense: {expenseTypeName}, Amount: {amount}");
            
            var expense = new Expense
            {
                Description = expenseTypeName,
                Amount = amount,
                Date = DateTime.Now,
                Category = expenseTypeName // Will use ExpenseTypeId later
            };
            
            var added = await _expenseRepository.AddAsync(expense);
            
            // Add to appropriate group
            var group = ExpenseTypeGroups.FirstOrDefault(g => g.ExpenseType.Id == expenseTypeId);
            if (group != null)
            {
                group.Expenses.Add(added);
                group.RefreshTotals();
            }
            
            return added;
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId)
        {
            var success = await _expenseRepository.DeleteAsync(expenseId);
            if (success)
            {
                // Remove from UI
                foreach (var group in ExpenseTypeGroups)
                {
                    var expense = group.Expenses.FirstOrDefault(e => e.Id == expenseId);
                    if (expense != null)
                    {
                        group.Expenses.Remove(expense);
                        group.RefreshTotals();
                        break;
                    }
                }
            }
            return success;
        }

        public async Task IncrementYearAsync()
        {
            if (SelectedYear < MaxYear)
            {
                SelectedYear++;
                if (SelectedYear == DateTime.Now.Year)
                {
                    SelectedMonth = DateTime.Now.Month;
                }
                else
                {
                    SelectedMonth = 1;
                }
                // LoadExpensesAsync is now called from View
            }
        }

        public async Task DecrementYearAsync()
        {
            if (SelectedYear > MinYear)
            {
                SelectedYear--;
                if (SelectedYear == DateTime.Now.Year)
                {
                    SelectedMonth = DateTime.Now.Month;
                }
                else
                {
                    SelectedMonth = 1;
                }
                // LoadExpensesAsync is now called from View
            }
        }

        public async Task IncrementMonthAsync()
        {
            if (SelectedMonth < 12)
            {
                SelectedMonth++;
            }
            else
            {
                if (SelectedYear < MaxYear)
                {
                    SelectedYear++;
                    SelectedMonth = 1;
                }
            }
            // LoadExpensesAsync is now called from View
        }

        public async Task DecrementMonthAsync()
        {
            if (SelectedMonth > 1)
            {
                SelectedMonth--;
            }
            else
            {
                if (SelectedYear > MinYear)
                {
                    SelectedYear--;
                    SelectedMonth = 12;
                }
            }
            // LoadExpensesAsync is now called from View
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
