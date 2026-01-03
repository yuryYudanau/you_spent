using YouSpent.ViewModels;
using YouSpent.Models;
using YouSpent.Resources.Strings;

namespace YouSpent.Views;

public partial class ExpensesPage : ContentPage
{
    private readonly ExpensesPageViewModel _viewModel;
    private bool _isAnimating = false;

    public ExpensesPage(ExpensesPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Add converters
        Resources.Add("InvertedBoolConverter", new InvertedBoolConverter());
        
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        InitializeUI();
    }

    private void InitializeUI()
    {
        YearLabel.Text = _viewModel.SelectedYear.ToString();
        MonthLabel.Text = _viewModel.MonthName;
        UpdateYearColor();
        UpdateMonthColor();
        UpdateExpenseTypesVisibility();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            System.Diagnostics.Debug.WriteLine("[ExpensesPage] OnAppearing - Loading expenses...");
            
            // Check if future period first
            if (_viewModel.MonthState == PeriodState.Future)
            {
                DebugLabel.Text = ExpensesPageResources.FuturePeriodMessage;
                DebugLabel.FontSize = 14;
                DebugContainer.IsVisible = true;
                GoToSettingsButton.IsVisible = false;
                ExpenseTypesCollection.IsVisible = false;
                return;
            }
            
            DebugLabel.Text = ExpensesPageResources.Loading;
            GoToSettingsButton.IsVisible = false;
            ExpenseTypesCollection.IsVisible = true;
            
            await _viewModel.LoadExpensesAsync();
            
            var count = _viewModel.ExpenseTypeGroups.Count;
            System.Diagnostics.Debug.WriteLine($"[ExpensesPage] Loaded {count} expense type groups");
            
            if (count == 0)
            {
                DebugLabel.Text = $"? {ExpensesPageResources.NoExpenseTypes}\n{ExpensesPageResources.AddExpenseTypesInSettings}";
                DebugLabel.FontSize = 14;
                GoToSettingsButton.IsVisible = true;
            }
            else
            {
                DebugLabel.Text = string.Format(ExpensesPageResources.LoadedTypesCount, count);
                DebugLabel.FontSize = 12;
                
                await Task.Delay(2000);
                DebugContainer.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExpensesPage] Error loading: {ex.Message}");
            DebugLabel.Text = $"? {ExpensesPageResources.Error}: {ex.Message}";
            await DisplayAlert(ExpensesPageResources.Error, $"{ex.Message}", ExpensesPageResources.Ok);
        }
    }

    private async void OnGoToSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SettingsPage/ExpenseTypesPage");
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.YearState))
        {
            UpdateYearColor();
            UpdateExpenseTypesVisibility();
        }
        else if (e.PropertyName == nameof(_viewModel.MonthState))
        {
            UpdateMonthColor();
            UpdateExpenseTypesVisibility();
        }
    }

    private void UpdateExpenseTypesVisibility()
    {
        // ???????? ???? ???????? ??? ??????? ????????
        bool isFuture = _viewModel.MonthState == PeriodState.Future;
        ExpenseTypesCollection.IsVisible = !isFuture;
        
        if (isFuture)
        {
            DebugContainer.IsVisible = true;
            DebugLabel.Text = ExpensesPageResources.FuturePeriodMessage;
            DebugLabel.FontSize = 14;
            GoToSettingsButton.IsVisible = false;
        }
        else
        {
            // Restore normal state - just update visibility, data will be loaded in OnAppearing
            if (_viewModel.ExpenseTypeGroups.Count > 0)
            {
                DebugContainer.IsVisible = false;
            }
        }
    }

    private void UpdateYearColor()
    {
        YearBorder.BackgroundColor = _viewModel.YearState switch
        {
            PeriodState.Current => (Color)Application.Current!.Resources["CurrentPeriod"],
            PeriodState.Past => (Color)Application.Current!.Resources["PastPeriod"],
            PeriodState.Future => (Color)Application.Current!.Resources["FuturePeriod"],
            _ => (Color)Application.Current!.Resources["Primary"]
        };
    }

    private void UpdateMonthColor()
    {
        MonthBorder.BackgroundColor = _viewModel.MonthState switch
        {
            PeriodState.Current => (Color)Application.Current!.Resources["CurrentPeriod"],
            PeriodState.Past => (Color)Application.Current!.Resources["PastPeriod"],
            PeriodState.Future => (Color)Application.Current!.Resources["FuturePeriod"],
            _ => (Color)Application.Current!.Resources["Primary"]
        };
    }

    private async void OnPreviousYearClicked(object sender, EventArgs e)
    {
        if (_isAnimating) return;
        await _viewModel.DecrementYearAsync();
        await AnimateYearChange(isNext: false);
        MonthLabel.Text = _viewModel.MonthName;
        UpdateMonthColor();
        
        // Reload expenses after year change
        await ReloadExpensesIfNeeded();
    }

    private async void OnNextYearClicked(object sender, EventArgs e)
    {
        if (_isAnimating) return;
        await _viewModel.IncrementYearAsync();
        await AnimateYearChange(isNext: true);
        MonthLabel.Text = _viewModel.MonthName;
        UpdateMonthColor();
        
        // Reload expenses after year change
        await ReloadExpensesIfNeeded();
    }

    private async void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        if (_isAnimating) return;
        var oldYear = _viewModel.SelectedYear;
        await _viewModel.DecrementMonthAsync();
        
        if (oldYear != _viewModel.SelectedYear)
        {
            var yearTask = AnimateYearChange(isNext: false);
            var monthTask = AnimateMonthChange(isNext: false);
            await Task.WhenAll(yearTask, monthTask);
        }
        else
        {
            await AnimateMonthChange(isNext: false);
        }
        
        // Reload expenses after month change
        await ReloadExpensesIfNeeded();
    }

    private async void OnNextMonthClicked(object sender, EventArgs e)
    {
        if (_isAnimating) return;
        var oldYear = _viewModel.SelectedYear;
        await _viewModel.IncrementMonthAsync();
        
        if (oldYear != _viewModel.SelectedYear)
        {
            var yearTask = AnimateYearChange(isNext: true);
            var monthTask = AnimateMonthChange(isNext: true);
            await Task.WhenAll(yearTask, monthTask);
        }
        else
        {
            await AnimateMonthChange(isNext: true);
        }
        
        // Reload expenses after month change
        await ReloadExpensesIfNeeded();
    }

    private async Task ReloadExpensesIfNeeded()
    {
        if (_viewModel.MonthState == PeriodState.Future)
        {
            // Show future period message
            DebugContainer.IsVisible = true;
            DebugLabel.Text = ExpensesPageResources.FuturePeriodMessage;
            DebugLabel.FontSize = 14;
            GoToSettingsButton.IsVisible = false;
            ExpenseTypesCollection.IsVisible = false;
        }
        else
        {
            // Load expenses for current or past period
            ExpenseTypesCollection.IsVisible = true;
            DebugContainer.IsVisible = false;
            
            try
            {
                // Small delay to let animation finish
                await Task.Delay(50);
                
                await _viewModel.LoadExpensesAsync();
                
                if (_viewModel.ExpenseTypeGroups.Count == 0)
                {
                    DebugContainer.IsVisible = true;
                    DebugLabel.Text = $"? {ExpensesPageResources.NoExpenseTypes}\n{ExpensesPageResources.AddExpenseTypesInSettings}";
                    DebugLabel.FontSize = 14;
                    GoToSettingsButton.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ExpensesPage] Error reloading: {ex.Message}");
                DebugContainer.IsVisible = true;
                DebugLabel.Text = $"? {ExpensesPageResources.Error}: {ex.Message}";
            }
        }
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is ExpenseTypeWithExpenses group)
        {
            var expenseType = group.ExpenseType;
            
            var amount = await DisplayPromptAsync(
                ExpensesPageResources.AddExpenseTitle, 
                string.Format(ExpensesPageResources.EnterAmountPrompt, expenseType.Name),
                keyboard: Keyboard.Numeric,
                placeholder: "0");
            
            if (!string.IsNullOrWhiteSpace(amount) && decimal.TryParse(amount, out var parsedAmount) && parsedAmount > 0)
            {
                try
                {
                    await _viewModel.AddQuickExpenseAsync(expenseType.Id, expenseType.Name, parsedAmount);
                    // Success - no dialog, just silent success
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ExpensesPage] Error adding expense: {ex.Message}");
                    await DisplayAlert(ExpensesPageResources.Error, ExpensesPageResources.FailedToAddExpense, ExpensesPageResources.Ok);
                }
            }
        }
    }

    private async void OnExpenseTagTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is Expense expense)
        {
            var dateStr = expense.Date.ToString("dd.MM.yyyy HH:mm");
            var action = await DisplayActionSheet(
                string.Format(ExpensesPageResources.ExpenseDetails, $"{expense.Amount:N0} ?", dateStr),
                ExpensesPageResources.Cancel,
                ExpensesPageResources.Delete,
                ExpensesPageResources.Edit);
            
            if (action == ExpensesPageResources.Delete)
            {
                var confirm = await DisplayAlert(ExpensesPageResources.Confirmation, string.Format(ExpensesPageResources.DeleteExpenseConfirm, $"{expense.Amount:N0} ?"), ExpensesPageResources.Yes, ExpensesPageResources.No);
                if (confirm)
                {
                    try
                    {
                        var success = await _viewModel.DeleteExpenseAsync(expense.Id);
                        if (!success)
                        {
                            await DisplayAlert(ExpensesPageResources.Error, ExpensesPageResources.FailedToDeleteExpense, ExpensesPageResources.Ok);
                        }
                        // Success - no dialog, just silent success
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ExpensesPage] Error deleting expense: {ex.Message}");
                        await DisplayAlert(ExpensesPageResources.Error, ExpensesPageResources.FailedToDeleteExpense, ExpensesPageResources.Ok);
                    }
                }
            }
            else if (action == ExpensesPageResources.Edit)
            {
                var newAmount = await DisplayPromptAsync(
                    ExpensesPageResources.Edit, 
                    ExpensesPageResources.NewAmount,
                    initialValue: expense.Amount.ToString("0"),
                    keyboard: Keyboard.Numeric);
                
                if (!string.IsNullOrWhiteSpace(newAmount) && decimal.TryParse(newAmount, out var parsedAmount) && parsedAmount > 0)
                {
                    try
                    {
                        expense.Amount = parsedAmount;
                        // Success - no dialog, just silent success
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ExpensesPage] Error updating expense: {ex.Message}");
                        await DisplayAlert(ExpensesPageResources.Error, ExpensesPageResources.FailedToUpdateExpense, ExpensesPageResources.Ok);
                    }
                }
            }
        }
    }

    private async Task AnimateYearChange(bool isNext)
    {
        _isAnimating = true;
        try
        {
            double exitX = isNext ? -100 : 100;
            double entryX = isNext ? 100 : -100;

            var fadeOutTask = YearLabel.FadeTo(0, 200, Easing.CubicIn);
            var moveOutTask = YearLabel.TranslateTo(exitX, 0, 200, Easing.CubicIn);
            await Task.WhenAll(fadeOutTask, moveOutTask);

            YearLabel.Text = _viewModel.SelectedYear.ToString();
            UpdateYearColor();
            YearLabel.TranslationX = entryX;
            YearLabel.Opacity = 0;

            var fadeInTask = YearLabel.FadeTo(1, 200, Easing.CubicOut);
            var moveInTask = YearLabel.TranslateTo(0, 0, 200, Easing.CubicOut);
            await Task.WhenAll(fadeInTask, moveInTask);
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task AnimateMonthChange(bool isNext)
    {
        double exitX = isNext ? -80 : 80;
        double entryX = isNext ? 80 : -80;

        var fadeOutTask = MonthLabel.FadeTo(0, 200, Easing.CubicIn);
        var moveOutTask = MonthLabel.TranslateTo(exitX, 0, 200, Easing.CubicIn);
        await Task.WhenAll(fadeOutTask, moveOutTask);

        MonthLabel.Text = _viewModel.MonthName;
        UpdateMonthColor();
        MonthLabel.TranslationX = entryX;
        MonthLabel.Opacity = 0;

        var fadeInTask = MonthLabel.FadeTo(1, 200, Easing.CubicOut);
        var moveInTask = MonthLabel.TranslateTo(0, 0, 200, Easing.CubicOut);
        await Task.WhenAll(fadeInTask, moveInTask);
    }
}

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
