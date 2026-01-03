using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YouSpent.Models
{
    public class Expense : INotifyPropertyChanged
    {
        private int _id;
        private string _description = string.Empty;
        private decimal _amount;
        private DateTime _date;
        private string _category = string.Empty;
        private int? _dayId;
        private int? _expenseTypeId;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        // Foreign key for Day relationship
        public int? DayId
        {
            get => _dayId;
            set
            {
                _dayId = value;
                OnPropertyChanged();
            }
        }

        // Foreign key for ExpenseType relationship
        public int? ExpenseTypeId
        {
            get => _expenseTypeId;
            set
            {
                _expenseTypeId = value;
                OnPropertyChanged();
            }
        }

        // Navigation property
        public ExpenseType? ExpenseType { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
