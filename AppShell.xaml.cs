namespace YouSpent
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Routing.RegisterRoute("SettingsPage", typeof(Views.SettingsPage));
            Routing.RegisterRoute("ExpenseTypesPage", typeof(Views.ExpenseTypesPage));
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }
    }
}
