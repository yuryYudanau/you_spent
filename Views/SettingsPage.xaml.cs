namespace YouSpent.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnExpenseTypesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ExpenseTypesPage");
        }
    }
}
