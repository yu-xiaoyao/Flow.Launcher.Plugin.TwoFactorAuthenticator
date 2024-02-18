using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class Settings : BaseModel
    {
        public Settings()
        {
        }

        public ObservableCollection<TotpModel> TotpList { get; set; } = new();


        /// <summary>
        /// 获取所有类型的
        /// </summary>
        /// <returns></returns>
        public List<OtpAuthModel> GetOtpList()
        {
            var list = new List<OtpAuthModel>();

            foreach (var totpModel in TotpList)
            {
                list.Add(totpModel);
            }

            // more type add here

            return list;
        }
    }
}