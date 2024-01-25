using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows.Documents;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class Settings : BaseModel
    {
        public Settings()
        {
        }

        public ObservableCollection<TotpModel> TotpList { get; set; } = new();

        public List<OtpAuthModel> GetOtpList()
        {
            var list = new List<OtpAuthModel>();

            foreach (var totpModel in TotpList)
            {
                list.Add(totpModel);
            }

            return list;
        }
    }
}