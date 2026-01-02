using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace YouSpent.ViewModels
{
    public enum PeriodState
    {
        Past,
        Current,
        Future
    }

    public class ExpensesPageViewModel : INotifyPropertyChanged
    {
        private int _selectedYear;
        private int _selectedMonth;
        private const int MinYear = 2000;
        private const int MaxYear = 2100;
        private readonly CultureInfo _culture = new CultureInfo("ru-RU");

        public event PropertyChangedEventHandler? PropertyChanged;

        public ExpensesPageViewModel()
        {
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
                await Task.CompletedTask;
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
                await Task.CompletedTask;
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
            await Task.CompletedTask;
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
            await Task.CompletedTask;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
