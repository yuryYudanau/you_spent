using YouSpent.ViewModels;

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
        
        // ????????????? ?? ????????? ?????????
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        InitializeUI();
    }

    private void InitializeUI()
    {
        YearLabel.Text = _viewModel.SelectedYear.ToString();
        MonthLabel.Text = _viewModel.MonthName;
        UpdateYearColor();
        UpdateMonthColor();
        UpdateSelectedYearInfo();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.YearState))
        {
            UpdateYearColor();
        }
        else if (e.PropertyName == nameof(_viewModel.MonthState))
        {
            UpdateMonthColor();
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
        // ????????? ????? ????? ????? ????
        MonthLabel.Text = _viewModel.MonthName;
        UpdateMonthColor();
    }

    private async void OnNextYearClicked(object sender, EventArgs e)
    {
        if (_isAnimating) return;
        
        await _viewModel.IncrementYearAsync();
        await AnimateYearChange(isNext: true);
        // ????????? ????? ????? ????? ????
        MonthLabel.Text = _viewModel.MonthName;
        UpdateMonthColor();
    }

    private async void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        if (_isAnimating) return;
        
        var oldYear = _viewModel.SelectedYear;
        await _viewModel.DecrementMonthAsync();
        
        // ???? ??? ?????????, ????????? ? ???
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
    }

    private async void OnNextMonthClicked(object sender, EventArgs e)
    {
        if (_isAnimating) return;
        
        var oldYear = _viewModel.SelectedYear;
        await _viewModel.IncrementMonthAsync();
        
        // ???? ??? ?????????, ????????? ? ???
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
    }

    private async Task AnimateYearChange(bool isNext)
    {
        _isAnimating = true;

        try
        {
            double exitX = isNext ? -100 : 100;
            double entryX = isNext ? 100 : -100;

            // ???????? ??????
            var fadeOutTask = YearLabel.FadeTo(0, 200, Easing.CubicIn);
            var moveOutTask = YearLabel.TranslateTo(exitX, 0, 200, Easing.CubicIn);
            
            await Task.WhenAll(fadeOutTask, moveOutTask);

            // ????????? ????? ? ????
            YearLabel.Text = _viewModel.SelectedYear.ToString();
            UpdateYearColor();
            UpdateSelectedYearInfo();
            YearLabel.TranslationX = entryX;
            YearLabel.Opacity = 0;

            // ???????? ?????
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

        // ???????? ??????
        var fadeOutTask = MonthLabel.FadeTo(0, 200, Easing.CubicIn);
        var moveOutTask = MonthLabel.TranslateTo(exitX, 0, 200, Easing.CubicIn);
        
        await Task.WhenAll(fadeOutTask, moveOutTask);

        // ????????? ????? ? ????
        MonthLabel.Text = _viewModel.MonthName;
        UpdateMonthColor();
        UpdateSelectedYearInfo();
        MonthLabel.TranslationX = entryX;
        MonthLabel.Opacity = 0;

        // ???????? ?????
        var fadeInTask = MonthLabel.FadeTo(1, 200, Easing.CubicOut);
        var moveInTask = MonthLabel.TranslateTo(0, 0, 200, Easing.CubicOut);
        
        await Task.WhenAll(fadeInTask, moveInTask);
    }

    private void UpdateSelectedYearInfo()
    {
        SelectedYearInfo.Text = $"???????????? ?????? ?? {_viewModel.MonthName} {_viewModel.SelectedYear}";
    }
}
