using YouSpent.Models;
using YouSpent.ViewModels;

namespace YouSpent.Views
{
    public partial class ExpenseTypesPage : BaseSettingsPage
    {
        private readonly ExpenseTypesPageViewModel _viewModel;

        public ExpenseTypesPage(ExpenseTypesPageViewModel viewModel)
        {
            InitializeComponent();
            
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            // Set the page title
            PageTitleText = "Expense Types";
            
            // Add converters to resources
            Resources.Add("BoolToTextColorConverter", new BoolToTextColorConverter());
            Resources.Add("BoolToActiveButtonColorConverter", new BoolToActiveButtonColorConverter());
            Resources.Add("BoolToActiveIconConverter", new BoolToActiveIconConverter());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadExpenseTypesAsync();
        }

        private async void OnAddExpenseTypeClicked(object sender, EventArgs e)
        {
            var success = await _viewModel.AddExpenseTypeAsync();
            if (!success)
            {
                if (string.IsNullOrWhiteSpace(_viewModel.NewTypeName))
                {
                    await DisplayAlert("Error", "Please enter a type name", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "This expense type already exists", "OK");
                }
            }
        }

        private async void OnToggleActiveClicked(object sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is ExpenseType expenseType)
            {
                System.Diagnostics.Debug.WriteLine($"[UI] Toggle clicked for: {expenseType.Name}, Current IsActive: {expenseType.IsActive}");
                var success = await _viewModel.ToggleExpenseTypeAsync(expenseType.Id);
                System.Diagnostics.Debug.WriteLine($"[UI] Toggle result: {success}, New IsActive: {expenseType.IsActive}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[UI] Toggle clicked but BindingContext is null or invalid");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is ExpenseType expenseType)
            {
                var newName = await DisplayPromptAsync("Edit", "Enter new name:", initialValue: expenseType.Name);
                if (!string.IsNullOrWhiteSpace(newName) && newName != expenseType.Name)
                {
                    expenseType.Name = newName;
                    await _viewModel.UpdateExpenseTypeAsync(expenseType);
                }
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is ExpenseType expenseType)
            {
                var confirm = await DisplayAlert("Delete", $"Are you sure you want to delete '{expenseType.Name}'?", "Yes", "No");
                if (confirm)
                {
                    await _viewModel.DeleteExpenseTypeAsync(expenseType.Id);
                }
            }
        }
    }

    // Converters
    public class BoolToTextColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isActive && !isActive)
            {
                return Application.Current?.Resources.TryGetValue("Gray500", out var color) == true 
                    ? color : Colors.Gray;
            }
            return Application.Current?.Resources.TryGetValue("Black", out var blackColor) == true 
                ? blackColor : Colors.Black;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToActiveButtonColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isActive && isActive)
            {
                // Green for active
                return Colors.Green;
            }
            // Gray for inactive
            return Application.Current?.Resources.TryGetValue("Gray400", out var grayColor) == true 
                ? grayColor : Colors.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToActiveIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            // Using simple text that works on all platforms
            return value is bool isActive && isActive ? "ON" : "OFF";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
