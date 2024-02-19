using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class Settings : BaseModel
    {
        public Settings()
        {
        }

        public ObservableCollection<OtpParam> OtpParams { get; set; } = new();

        // public ObservableCollection<TotpModel> TotpList { get; set; } = new();
    }
}