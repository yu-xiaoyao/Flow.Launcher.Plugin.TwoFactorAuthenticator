using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class Settings : BaseModel
    {
        public Settings()
        {
            CopyNotification = true;
            SearchName = false;
            PinyinSearch = false;
            SearchIssuer = false;
        }

        public bool CopyNotification { get; set; }

        public bool PinyinSearch { get; set; }

        public bool SearchName { get; set; }

        public bool SearchIssuer { get; set; }

        public ObservableCollection<OtpParam> OtpParams { get; set; } = new();


        // public ObservableCollection<TotpModel> TotpList { get; set; } = new();
    }
}