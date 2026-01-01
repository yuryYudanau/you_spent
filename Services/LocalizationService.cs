using System.Globalization;

namespace YouSpent.Services
{
    public class LocalizationService
    {
        public void SetCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        public string GetCurrentCulture()
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        }

        public void SetDeviceLanguage()
        {
            var systemLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            
            // Support Russian and English, default to English
            if (systemLang == "ru")
            {
                SetCulture("ru-RU");
            }
            else
            {
                SetCulture("en");
            }
        }
    }
}
