namespace YouSpent.Views
{
    public partial class BaseSettingsPage : ContentPage
    {
        public BaseSettingsPage()
        {
            InitializeComponent();
        }

        // Property to set page title
        public string PageTitleText
        {
            get => PageTitle.Text;
            set => PageTitle.Text = value;
        }

        // Property to set content
        public View PageContent
        {
            get => ContentArea.Content;
            set => ContentArea.Content = value;
        }

        // Virtual method for back button - can be overridden
        protected virtual async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
